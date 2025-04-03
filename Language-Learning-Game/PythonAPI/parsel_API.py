from fastapi import FastAPI, File, UploadFile
from fastapi.responses import JSONResponse
from fastapi import FastAPI
from pydantic import BaseModel
import uvicorn
import noisereduce as nr
from scipy.io import wavfile
from pydub import AudioSegment
import parselmouth
import numpy as np
from scipy.ndimage.interpolation import zoom
import copy

tone = 1

originalReferenceData = []
originalRecordingData = []

template1 = "Your tone seems to be {} throughout your speech just like the reference! Well done!"
template2 = "Your tone seems to be {} throughout your speech, however it should be {} instead!"
template3 = "Your contour seems to exhibit {}. It should exhibit {} instead!"
deviationTemplate = "Going from left to right, your deviations show that your tone is {}!"

app1 = FastAPI()

sizeThreshold = 0.045 # 4.5% / # threshold of difference for collapsing difference array

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

    # load data
    rate, data = wavfile.read("recording.wav")
    # perform noise reduction
    reduced_noise = nr.reduce_noise(y=data, sr=rate)
    wavfile.write("recordingReduced.wav", rate, reduced_noise)

    # 2: Check for Excessive noise + Silence
        # Cut out silence <- DONE IN PARSELMOUTH LATER //
        # if noise exceeds a certain amount, return with a null value for failure. <-!!! TODO !!!

    # 3: Processing Audio + Pitch Data

    # Converts the file specified by filepath to pitch values + timestamps.
    recordingInfo, recordingPitch, recordingTimes = fileToPitch("recordingReduced.wav")
    referenceInfo, referencePitch, referenceTimes = fileToPitch("croppedReference.wav")
   
    # Removes leading + trailing '0' pitch values + interpolates remaining gaps.
    non0Recording, recordingTimes = maskAndNull(recordingPitch, recordingTimes)
    interpolatedRecording, recordingTimes = interpolateValues(non0Recording, recordingTimes)
    non0Reference, referenceTimes = maskAndNull(referencePitch, referenceTimes)
    interpolatedReference, referenceTimes = interpolateValues(non0Reference, referenceTimes)
    
    # Resamples the shortest length pitch contour to match the sample size of the longer one.
    interpolatedReferenceResampled, referenceTimesResampled, interpolatedRecordingResampled, recordingTimesResampled = resampleShortest(interpolatedReference, referenceTimes, 
                                                                                                    interpolatedRecording, recordingTimes)
    
    originalRecordingData, originalReferenceData = interpolatedRecordingResampled, interpolatedReferenceResampled

    # Performs Min-Max normalisation to put them onto the same scale.
    minMaxRecording = minMaxNormalise(interpolatedRecordingResampled)
    minMaxReference = minMaxNormalise(interpolatedReferenceResampled)

    
    # Applies a flattening function onto pitch values given some threshold. Fixes amplified differences for only tone 3.
    minMaxRecordingFlatten = flattenValues(minMaxRecording, interpolatedRecordingResampled, 30)
    minMaxReferenceFlatten = flattenValues(minMaxReference, interpolatedReferenceResampled, 30)    
    # Gets the datapoints that differ too much past a given threshold.
    allMajorDifferences, relevantDifferences = getDifferences(minMaxReferenceFlatten, minMaxRecordingFlatten, 0.25)
    
    # Uses that to compute the percentage of incorrect datapoints to overall datapoints to get % accuracy.
    score = getCorrectness(allMajorDifferences, minMaxRecordingFlatten)

    minMaxRecordingFlattenCopy = copy.deepcopy(minMaxRecordingFlatten)
    minMaxReferenceFlattenCopy = copy.deepcopy(minMaxReferenceFlatten)

    # Generates feedback from the processed data.
    feedback = []
    if tone == 0:
        feedback = compareNeutral(minMaxReferenceFlatten, referenceTimes, minMaxRecordingFlatten, recordingTimes)
        print(feedback)
    else:
        feedback = compareToneTrends(minMaxReferenceFlattenCopy, referenceTimes, minMaxRecordingFlattenCopy, recordingTimes, score)
        print(feedback)

    # Only do this if score is under 65?
    relevantDeviations = []
    if score < 70:
        diffFeedback, relevantDeviations = generateDifferenceFeedback(minMaxReferenceFlattenCopy, minMaxRecordingFlattenCopy, relevantDifferences)
        if len(diffFeedback) > 0:
            feedback.append("Highlighted in red you will see points of major deviations in your tone!")
            for i in diffFeedback:
                feedback.append(i)

    # Collapse relevant deviations back into 1d array.
    relevantDeviationsCollapsed = []
    for i in relevantDeviations:
        relevantDeviationsCollapsed += i

    print(relevantDeviationsCollapsed)
    # Need to return: pitches / times / major differences
    analysisResult = {
        "pitchRecording": minMaxRecordingFlatten.tolist(),
        "timestampsRecording": recordingTimesResampled.tolist(),
        "pitchReference": minMaxReferenceFlatten.tolist(),
        "timestampsReference": referenceTimesResampled.tolist(),
        "accuracyScore": score,
        "relevantDeviations": relevantDeviationsCollapsed,
        "feedbackList": feedback
    }

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

def flattenValues(normalisedValues, originalvalues, flattenThreshold):
    print("hello!")
    print(originalvalues)
    print("max", max(originalvalues))
    print("min", min(originalvalues))
    if max(originalvalues) - min(originalvalues) <= flattenThreshold:
        print("Flattening!")
        for i in range(len(normalisedValues)):
            normalisedValues[i] = ((normalisedValues[i] - 0.5) * 0.1) + 0.5
    return normalisedValues

def getDifferences(pitchA, pitchB, threshold=0.25):
    pitchDifferences = pitchA - pitchB
    
    bigDifferencesIndexes = np.where(np.abs(pitchDifferences)>threshold)[0] # Creates an array where those differences are larger than a threshold.
    if len(bigDifferencesIndexes) == 0:
        return [], []
    # Groups consequtive indexes, then groups arrays within a certain range of each other
    diffIndexesConsequtive = groupByConsequtive(bigDifferencesIndexes)
    arraySizeThreshold = round(len(pitchDifferences) *sizeThreshold)
    groupedDiffIndexes = groupNeighbours(diffIndexesConsequtive, arraySizeThreshold)

    # Removes any difference arrays that are below 10% of pitch data size to avoid noise.
    for i in groupedDiffIndexes:
        if len(i) <= round(len(pitchDifferences)*0.10):
            groupedDiffIndexes.remove(i)

    # Returns all the indexes, the relevant indexes
    return bigDifferencesIndexes, groupedDiffIndexes

def getCorrectness(majorDeviations,recordingValues):
    if len(majorDeviations) == 0:
        return 100
    errorRate = len(majorDeviations) / len(recordingValues)*100
    return 100-errorRate

def groupByConsequtive(diffIndexes):
    diffIndexes = [int(x) for x in diffIndexes] # Fix formatting issue
    print("Diff indexes are:", diffIndexes)
    groupedArr = [[diffIndexes[0]]]
    for x in diffIndexes[1:]:
        if x == groupedArr[-1][-1] + 1:
            groupedArr[-1].append(x)
        else:
            groupedArr.append([x])
    return groupedArr

def groupNeighbours(indexArray, n=2):
    finalArray = []

    for subArray in indexArray:       
        if indexArray[0] == subArray:
            finalArray.append(subArray)
            continue

        if abs(finalArray[-1][-1] - subArray[0]) <= n:
            finalArray[-1].extend(subArray)
        else:
            finalArray.append(subArray)
    
    return finalArray


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
    referenceTrends, recordingTrends = getTrend(referencePitchValues, recordingPitchValues, 3) # Computes the trend of data for each "n" amount of segments.
    print("reference trends are", referenceTrends)
    print("recording trends are", recordingTrends)
    referenceTrends = removeDuplicateConsequtive(referenceTrends)
    recordingTrends = removeDuplicateConsequtive(recordingTrends)   
    print("reference trends are2", referenceTrends)
    print("recording trends are2", recordingTrends)

    #compare length -------
    feedback.append(compareLength(referenceTimes, recordingTimes))

    # Compare Values -------
    print(percent)
    # Fallback to avoid unhelpful feedback if accuracy is high:
    if percent > 90:
        print("TOO CORRECT, SKIP EVAL", percent)
        feedback.append(template1.format(generateSentence(referenceTrends, " then ")))
        return feedback

    else:
        # CASE: Tone isn't flat
        generatedFeedback = generateFeedback(referenceTrends, recordingTrends) #Generates feedback from the trends.
        feedback = feedback + generatedFeedback
        return feedback

# Gets the overall trend of the data, if it's going up vs down
def getTrend(referencePitchValues, recordingPitchValues, segments):
    referenceTrends = []
    recordingTrends = []
    # Segments minmax data
    segmentedRecordingPitches = segmentData(recordingPitchValues, segments)
    segmentedReferencePitches = segmentData(referencePitchValues, segments)
    
    # Segments original data
    segmentedRecordingOriginal = segmentData(originalRecordingData, segments)
    segmentedReferenceOriginal = segmentData(originalReferenceData, segments)

    for i in range(len(segmentedRecordingPitches)):
        indexes = [i for i in range(len(segmentedRecordingPitches[i]))]
        trend = detectTrend(indexes, segmentedRecordingPitches[i], segmentedRecordingOriginal[i])
        trendRef = detectTrend(indexes, segmentedReferencePitches[i], segmentedReferenceOriginal[i])
        recordingTrends.append(numTrendToString(trend))
        referenceTrends.append(numTrendToString(trendRef))
    
    return referenceTrends, recordingTrends

# FUNCTIONS TO FORMAT DATA FOR FEEDBACK
def removeDuplicateConsequtive(trends): # Removes consequtive equivalent items
    processedTrends = []
    for trend in trends:
        if len(processedTrends)<1 or trend != processedTrends[-1]: # if current item is same as latest processed item leave it out.
            processedTrends.append(trend)
    return processedTrends

def detectTrend(indexArray, dataArray, originalData, order=1):
    # dataArray = flattenValues(dataArray, originalData, 30)
    result = np.polyfit(indexArray, list(dataArray), order)
    slope = result[-2]
    return float(slope)

# Converts the data gotten from detectTrend + getTrend into word feedbck.
def numTrendToString(trendVal):
    if trendVal >= 0.001: # > 0.1 ranges is rising slope.
        trend = "rising"
    elif abs(trendVal) < 0.001: #0.1 ranges is a relatively maintaining/flat slope.
        trend = "maintaining"
    else:
        trend = "dropping" # lower than -0.1 is decreasing slope.
    return trend

def segmentData(data, segements):
    k, m = divmod(len(data), segements)
    return list((data[i*k+min(i, m):(i+1)*k+min(i+1, m)] for i in range(segements)))

# FEEDBACK GENERATORS

def generateFeedback(referenceTrends,recordingTrends):
    feedback = []
    
    sameTrends = True
    if len(referenceTrends) == len(recordingTrends): # Only if trends are equal in length are they equivalent
        for i in range(len(referenceTrends)): 
            if referenceTrends[i] != recordingTrends[i]: # Checks each individual element, so order matters vs comparison on whole array
                print("non same detected")
                sameTrends = False
    else:
        sameTrends = False

    # Only provide a confirmation message of positive feedback if everything is correct (indicated by count being equal to size of reference trends & referenceTrends isn't null)
    if sameTrends == True:
        feedback.append(template1.format(generateSentence(recordingTrends, " then ")))
        return feedback
    
    feedback.append(template2.format(generateSentence(recordingTrends, " then "), generateSentence(referenceTrends, " then ")))

    # Translates voice feedback into feedback on the graph.
    slopeTrendsRef = generateSlopeFeedback(referenceTrends)
    slopeTrendsRec = generateSlopeFeedback(recordingTrends)

    # If they're the same, but accuracy overall is low due to different points of trend appearance:  
    feedback.append(template3.format(generateSentence(slopeTrendsRec, " followed by "), generateSentence(slopeTrendsRef, " followed by ")))
    return feedback

def generateSentence(trend, connector):
    sentence = ""
    if len(trend) <= 1:
        sentence = sentence + trend[0]
        return sentence

    for i in range(len(trend)):
        if i == len(trend)-1:
            sentence = sentence + trend[i]
            continue
        sentence = sentence + trend[i] + connector
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

def generateDifferenceFeedback(referencePitchValues, recordingPitchValues, relevantDeviations):

    # Get the values each index refers to and populates an array of the same format.
    referenceValues = [[referencePitchValues[i] for i in subarray] for subarray in relevantDeviations]
    originalRefValues = [[originalReferenceData[i] for i in subarray] for subarray in relevantDeviations]

    recordingValues = [[recordingPitchValues[i] for i in subarray] for subarray in relevantDeviations]
    originalRecValues = [[originalRecordingData[i] for i in subarray] for subarray in relevantDeviations]

    recordingTrends, referenceTrends, textRefTrends, textRecTrends = [], [], [], []

    for i in range(len(referenceValues)):
        referenceTrend = detectTrend(relevantDeviations[i], referenceValues[i], originalRefValues[i])
        referenceTrends.append(referenceTrend)

        recordingTrend = detectTrend(relevantDeviations[i], recordingValues[i], originalRecValues[i])
        recordingTrends.append(recordingTrend)

        textRecTrends.append(numTrendToString(recordingTrend))
        textRefTrends.append(numTrendToString(referenceTrend))
    
    print(textRefTrends)
    print(textRecTrends)

    feedbackItems = []
    differenceThreshold = 0.2 # 20% difference
    for i in range(len(textRefTrends)):
        if textRefTrends[i] == textRecTrends[i]:# Same trend
            if textRefTrends[i] == "maintaining": 
                feedbackItems.append(-1) # Mark it
                continue # next item
            else:
                # If reference + n% is still smaller than recording, then rec is going too much.
                if (abs(referenceTrends[i]) * (1+differenceThreshold)) < abs(recordingTrends[i]):
                    feedbackItems.append((textRecTrends[i] + " too sharply"))
                elif (abs(referenceTrends[i]) * (1-differenceThreshold)) > abs(recordingTrends[i]):
                    feedbackItems.append((textRecTrends[i] + " too slowly"))
                else:
                    feedbackItems.append(-1)
        else: # Not the same trend
            feedbackItems.append((textRecTrends[i] + " instead of " + textRefTrends[i]))

    # Goes through array backwards to avoid issue of missing index after popping.
    for i in range(len(feedbackItems) - 1, -1, -1):
        if feedbackItems[i] == -1:
            relevantDeviations.pop(i)
            feedbackItems.pop(i)

    feedback = []
    if len(feedbackItems) > 0:
        feedback.append(deviationTemplate.format(generateSentence(feedbackItems, " then ")))
    
    return feedback, relevantDeviations

# -----------------------

if __name__ == "__main__":
    uvicorn.run("parsel_API:app1", host="0.0.0.0", port=8001, reload=True)