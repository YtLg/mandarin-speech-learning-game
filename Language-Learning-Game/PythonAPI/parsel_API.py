from fastapi import FastAPI, File, UploadFile
from fastapi.responses import JSONResponse
from fastapi import FastAPI
from pydantic import BaseModel
import uvicorn

from pydub import AudioSegment
import parselmouth
import numpy as np
from scipy.ndimage.interpolation import zoom

tone = 1

originalReferenceData = []
originalRecordingData = []

template1 = "Your tone seems to be {} throughout your speech just like the reference! Well done!"
template2 = "Your tone seems to be {} throughout your speech, however it should be {} instead! Try to make your pitch contour display a similar contour to the reference line as shown on the graph!"
template3 = "Your contour seems to exhibit {}. It should exhibit {} instead!"

app1 = FastAPI()

flattenThreshold = 30 # Hz Definition of threshold for flattenValues

class TimestampArray(BaseModel):
    timestamps: list = []



# ----------------------- TEST URI ------------------------------------------
@app1.post("/")
async def hello(timestamp: TimestampArray):
    print(timestamp.timestamps[0], timestamp.timestamps[1])
    return {"message": "helloooo"}


#----------------------- IMPLEMENTATION URI ---------------------------------
@app1.post("/analyseAudio")
async def analyseAudio(timestampStart: float, timestampEnd: float, referenceFile: UploadFile = File(...), recordingFile: UploadFile = File(...)):
    global originalRecordingData, originalReferenceData

    referenceAudio = await referenceFile.read()
    recordingAudio = await recordingFile.read()

    with open("reference.wav", "wb") as f:    
        f.write(referenceAudio)
        f.close
    with open("recording.wav","wb") as f:
        f.write(recordingAudio)
        f.close
    # 1: Crop 2.wav to start + end timestamp (reference audio)
    audio = AudioSegment.from_file("reference.wav")
    segment = audio[timestampStart*1000:timestampEnd*1000]
    segment.export("croppedReference.wav", format="wav")

    # 2: Check for Excessive noise + Silence
        # Cut out silence <- DONE IN PARSELMOUTH LATER //
        # if noise exceeds a certain amount, return with a null value for failure. <-!!! TODO !!!

    # 3: Processing Audio + Pitch Data

    # Converts the file specified by filepath to pitch values + timestamps.
    recordingInfo, recordingPitch, recordingTimes = fileToPitch("recording.wav")
    referenceInfo, referencePitch, referenceTimes = fileToPitch("croppedReference.wav")
    print("RECORDING PRE-PROCESSING:", len(recordingPitch), len(recordingTimes) )
    # Removes leading + trailing '0' pitch values + interpolates remaining gaps.
    non0Recording, recordingTimes = maskAndNull(recordingPitch, recordingTimes)
    interpolatedRecording, recordingTimes = interpolateValues(non0Recording, recordingTimes)
    non0Reference, referenceTimes = maskAndNull(referencePitch, referenceTimes)
    interpolatedReference, referenceTimes = interpolateValues(non0Reference, referenceTimes)
    
    originalRecordingData, originalReferenceData = interpolatedRecording, interpolatedReference
    # Resamples the shortest length pitch contour to match the sample size of the longer one.
    interpolatedReferenceResampled, referenceTimesResampled, interpolatedRecordingResampled, recordingTimesResampled = resampleShortest(interpolatedReference, referenceTimes, 
                                                                                                    interpolatedRecording, recordingTimes)
    # Performs Min-Max normalisation to put them onto the same scale.
    minMaxRecording = minMaxNormalise(interpolatedRecordingResampled)
    minMaxReference = minMaxNormalise(interpolatedReferenceResampled)

    # Applies a flattening function onto pitch values given some threshold. Fixes amplified differences for only tone 3.
    minMaxRecording = flattenValues(minMaxRecording, interpolatedRecordingResampled)
    minMaxReference = flattenValues(minMaxReference, interpolatedReferenceResampled)

    # Gets the datapoints that differ too much past a given threshold.
    majorDifferences = getDifferences(minMaxReference, minMaxRecording, 0.3)
    # Uses that to compute the percentage of incorrect datapoints to overall datapoints to get % accuracy.
    score = getCorrectness(majorDifferences, minMaxRecording)

    # Generates feedback from the processed data.
    feedback = []
    if tone == 0:
        feedback = compareNeutral(minMaxReference, referenceTimes, minMaxRecording, recordingTimes)
        print(feedback)
    else:
        feedback = compareToneTrends(minMaxReference, referenceTimes, minMaxRecording, recordingTimes, score)
        print(feedback)

    # Need to return: pitches / times / major differences
    analysisResult = {
        "pitchRecording": minMaxRecording.tolist(),
        "timestampsRecording": recordingTimesResampled.tolist(),
        "pitchReference": minMaxReference.tolist(),
        "timestampsReference": referenceTimesResampled.tolist(),
        "accuracyScore": score,
        "feedbackList": feedback
    }

    # print(analysisResult)
    return JSONResponse(content=analysisResult)


# ----------- HELPER FUNCTIONS -----------

def fileToPitch(filepath):
    sound = parselmouth.Sound(filepath) 
    pitch = sound.to_pitch()
    times = pitch.xs()
    pitchValues = pitch.selected_array['frequency']

    return pitch, pitchValues, times

# This removes silence at the beginning and after the end of the recording.
def maskAndNull(pitchValues, times):
    firstNoiseIndex = np.nonzero(pitchValues)[0][0]
    lastNoiseIndex = np.nonzero(pitchValues)[0][-1]
    
    filteredPitchValues = pitchValues[firstNoiseIndex:lastNoiseIndex+1]
    filteredTimeValues = times[firstNoiseIndex:lastNoiseIndex+1]
    
    zeroToNanPitchValues = np.where(filteredPitchValues == 0, np.nan, filteredPitchValues)

    return zeroToNanPitchValues, filteredTimeValues

def interpolateValues(pitchValues, times):
    # Assume leading + trailing NaN values are removed via maskNull & start at beginning + end of pitchValues

    nanIndices = np.isnan(pitchValues) # returns an array that tells if "n" is NaN or not, so can interpolate only on NaN values.

    # puts times[nan](x) onto times[~nan](x) axis and uses the corresponding neighboring pitchValues of those times to figure out its missing pitchValue
    interpolatedPitch = np.interp(times[nanIndices],times[~nanIndices], pitchValues[~nanIndices]) 
    pitchValues[nanIndices] = interpolatedPitch

    return pitchValues, times

def resampleShortest(pitchValuesA, timesa, pitchValuesB, timesb):
    if(len(pitchValuesA > len(pitchValuesB))):
        temp = pitchValuesB
        resample_ratio = len(pitchValuesA) / len(pitchValuesB)
        pitchValuesB = zoom(pitchValuesB, resample_ratio)
        pitchValuesB[-1] = temp[-1] # handle odd case.
        timesb = timesa
    elif(len(pitchValuesB > len(pitchValuesA))):
        temp = pitchValuesA
        resample_ratio = len(pitchValuesB) / len(pitchValuesA)
        pitchValuesA = zoom(len(pitchValuesA) / resample_ratio)
        pitchValuesA[-1] = temp[-1]
        timesa = timesb

    return pitchValuesA, timesa, pitchValuesB, timesb

def minMaxNormalise(pitchA):
    maxVal = max(pitchA)
    minVal = min(pitchA)
    normalisedA = (pitchA- minVal) / (maxVal - minVal)
    return normalisedA

def flattenValues(normalisedValues, originalvalues):
    if max(originalvalues) - min(originalvalues) <= flattenThreshold:
        for i in range(len(normalisedValues)):
            normalisedValues[i] = ((normalisedValues[i] - 0.5) * 0.1) + 0.5
    return normalisedValues

def getDifferences(pitchA, pitchB, threshold=0.25):
    # i = recording index, j = reference index.
    pitchDifferences = pitchA - pitchB
    bigDifferences = np.where(np.abs(pitchDifferences)>threshold)[0]
    return bigDifferences

def getCorrectness(majorDeviations,recordingValues):
    if len(majorDeviations) == 0:
        return 100
    errorRate = len(majorDeviations) / len(recordingValues)*100
    return 100-errorRate

# ---------- FEEDBACK GENERATOR -------------

# -------------- TONE COMPARISON FUNCTIONS ------------------ 
def compareLength(referenceTimes, recordingTimes):
    durationReference = max(referenceTimes) - min(referenceTimes)
    durationRecording = max(recordingTimes) - min(recordingTimes)


    if (durationReference*0.8 > durationRecording): # Allow for 20% error.
        text = "Sorry, but your speech is too short"
        return text
    elif (durationReference*1.2 < durationRecording):
        text = "Sorry, but your speech is too long"

    text = "The length of your speech is correct!"
    return text

# Tone 0: Neutral Tone, light + short, no well-defined contour:
    # - COMPARE ONLY LENGTH TO SEE IF IT'S APPROPRIATE
def compareNeutral(referencePitchValues, referenceTimes, recordingPitchValues, recordingTimes):
    feedback = []
    feedback.append(compareLength(referenceTimes, recordingTimes))
    return feedback

# Other tones: flat, rising, dipping, falling. short -> long.
def compareToneTrends(referencePitchValues, referenceTimes, recordingPitchValues, recordingTimes, percent):
    feedback = []

    #compare length -------
    feedback.append(compareLength(referenceTimes, recordingTimes))

    # Compare Values -------
    print(percent)
    # Fallback to avoid unhelpful feedback if accuracy is high:
    if percent > 70:
        referenceTrends, recordingTrends = getTrend(referencePitchValues, recordingPitchValues, 3)
        feedback.append(template1.format(generateSentence(referenceTrends, " then ")))
        return feedback

    recordingDifference = max(recordingPitchValues) - min(recordingPitchValues)
    if recordingDifference <= 0.1:
        feedback.append("Your tone is flat, just like the reference, well done!")

    else:
        # CASE: Tone isn't flat
        referenceTrends, recordingTrends = getTrend(referencePitchValues, recordingPitchValues, 3) # Computes the trend of data for each "n" amount of segments.
        generatedFeedback = generateFeedback(referenceTrends, recordingTrends, percent) #Generates feedback from the trends.
        if percent < 61:
            feedback.append("Your Accuracy score seems low, check if your speech exhibits the same trends at similar time intervals to the green line!")
            
        feedback = feedback + generatedFeedback
        return feedback

# Gets the overall trend of the data, if it's going up vs down
def getTrend(referencePitchValues, recordingPitchValues, segments):
    referenceTrends = []
    recordingTrends = []
    segmentedRecordingPitches = segmentData(recordingPitchValues, segments)
    segmentedReferencePitches = segmentData(referencePitchValues, segments)

    for i in range(len(segmentedRecordingPitches)):
        indexes = [i for i in range(len(segmentedRecordingPitches[i]))]
        trend = detectTrend(indexes, segmentedRecordingPitches[i], originalRecordingData)
        trendRef = detectTrend(indexes, segmentedReferencePitches[i], originalReferenceData)
        recordingTrends.append(numTrendToString(trend))
        referenceTrends.append(numTrendToString(trendRef))
        # Do some processing on data to formulate them into actual string sentences.

    return referenceTrends, recordingTrends

# FUNCTIONS TO FORMAT DATA FOR FEEDBACK

def detectTrend(indexArray, dataArray, originalData, order=1):
    dataArray = flattenValues(dataArray, originalData)
    result = np.polyfit(indexArray, list(dataArray), order)
    slope = result[-2]
    return float(slope)

def numTrendToString(trendVal):
    if trendVal >= 0.001: # > 0.001 ranges is rising slope.
        trend = "rising"
    elif abs(trendVal) < 0.001: #0.000 ranges is a relatively maintaining/flat slope.
        trend = "maintaining"
    else:
        trend = "dropping" # lower than -0.001 is decreasing slope.
    return trend

def segmentData(data, segements):
    k, m = divmod(len(data), segements)
    return list((data[i*k+min(i, m):(i+1)*k+min(i+1, m)] for i in range(segements)))

# FEEDBACK GENERATORS

def generateFeedback(referenceTrends,recordingTrends, percent):
    feedback = []

    print(referenceTrends)
    print(recordingTrends)
    referenceTrends = list(set(referenceTrends))
    recordingTrends = list(set(recordingTrends))
    print(referenceTrends)
    print(recordingTrends)

    count=0
    if len(referenceTrends) == len(recordingTrends): # Only if trends are equal in length are they equivalent
        for i in range(referenceTrends): 
            if referenceTrends[i] == recordingTrends[i]: # Checks each individual element, so order matters vs comparison on whole array
                count+=1 # increment counter if they're the same

    # Only provide a confirmation message of positive feedback if everything is correct (indicated by count being equal to size of reference trends & referenceTrends isn't null)
    if count == len(referenceTrends) and len(referenceTrends) != 0:
        feedback.append(template1.format(generateSentence(recordingTrends, " then ")))
        return feedback
    
    thing = feedback.append(template2.format(generateSentence(recordingTrends, " then "), generateSentence(referenceTrends, " then ")))

    # Translates voice feedback into feedback on the graph.
    slopeTrendsRef = generateSlopeFeedback(referenceTrends)
    slopeTrendsRec = generateSlopeFeedback(recordingTrends)

    # If they're the same, but accuracy overall is low due to different points of trend appearance:
    if len(slopeTrendsRef) == len(slopeTrendsRec):
        for i in range(referenceTrends):
        if slopeTrendsRef == slopeTrendsRec:
        feedback.append("Well done! The overall trend of your speech is" + generateSentence(slopeTrendsRec, " followed by ") + "If there are criticisms of your voice earlier, it could be due to noise!")    
    feedback.append(template3.format(generateSentence(slopeTrendsRec, " followed by "), generateSentence(slopeTrendsRef, " followed by ")))
    return feedback

def generateSentence(trend, connector):
    sentence = ""
    i = -1
    for i in range(len(trend)-1):
        sentence = sentence + trend[i+1] + connector
    sentence = sentence + trend[i+1]
    return sentence

def generateSlopeFeedback(trend):
    lines = []
    for i in trend:
        if i == "maintaining":
            lines.append("a steady flat line")
        elif i == "rising":
            lines.append("a consistent and gradual increase in the slope")
        else:
            lines.append("a sharp drop in the slope")
    return lines

# -----------------------

if __name__ == "__main__":
    uvicorn.run("parsel_API:app1", host="0.0.0.0", port=8001, reload=True)