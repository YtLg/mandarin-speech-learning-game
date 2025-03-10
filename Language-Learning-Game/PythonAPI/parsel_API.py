from fastapi import FastAPI, File, UploadFile
from fastapi.responses import JSONResponse
from fastapi import FastAPI
from pydantic import BaseModel
import uvicorn

from pydub import AudioSegment
import simplestretch
import wave
import contextlib

import parselmouth
import numpy as np
from fastdtw import fastdtw
from scipy.spatial.distance import euclidean

app1 = FastAPI()

class TimestampArray(BaseModel):
    timestamps: list = []

# ----------------------- TEST URI ------------------------------------------
@app1.post("/")
async def hello(timestamp: TimestampArray):
    print(timestamp.timestamps[0], timestamp.timestamps[1])
    return {"message": "helloooo"}


#----------------------- IMPLEMENTATION URI ---------------------------------
@app1.post("/analyseAudio")
async def analyseAudio(timestampStart: float = 0.0, timestampEnd: float = 0.0, referenceFile: UploadFile = File(...), recordingFile: UploadFile = File(...)):
    referenceAudio = await referenceFile.read()
    recordingAudio = await recordingFile.read()

    with open("2.wav", "wb") as f:    
        f.write(referenceAudio)
        f.close
    with open("3.wav","wb") as f:
        f.write(recordingAudio)
        f.close
    
    # 1: Crop 2.wav to start + end timestamp (reference audio)
    audio = AudioSegment.from_file("2.wav")
    segment = audio[timestampStart*1000:timestampEnd*1000]
    segment.export("cropped.wav", format="wav")

    # 1.5: Check for Excessive noise + Silence
        # Cut out silence <- DONE IN PARSELMOUTH LATER //
        # if noise exceeds a certain amount, return with a null value for failure. <-!!! TODO !!!

    # # 2: Compress 3.wav to fix start + end timestamp boundaries
    # # Gets length of an audio.
    # with contextlib.closing(wave.open("3.wav",'r')) as f: 
    #     frames = f.getnframes()
    #     rate = f.getframerate()
    #     length = frames/float(rate)
    
    # # To work out percentage of x to y, do x/y, then to scale y to x's size, do y*(x/y)
    #      # where x = 2.wav and y = 3.wav, so length of refence/length of recording
    # rec_scaled = simplestretch.stretch_audio("3.wav", ((timestampEnd-timestampStart)/length), "3out.wav")

    # 3: Do initial analysis.
    referenceSound = parselmouth.Sound("cropped.wav")
    recordingSound = parselmouth.Sound("3.wav")

    # # Get the pitch values from both audios
    # recordingPitch = recordingSound.to_pitch(time_step=0.01, pitch_floor=75, pitch_ceiling=500)
    # referencePitch = referenceSound.to_pitch(time_step=0.01, pitch_floor=75, pitch_ceiling=500)

    recordingInfo, recordingPitch, recordingTimes = fileToPitch(recordingSound)
    referenceInfo, referencePitch, referenceTimes = fileToPitch(referenceSound)

    non0Recording, recordingTimes = maskAndNull(recordingPitch, recordingTimes)
    interpolatedRecording, recordingTimes = interpolateValues(non0Recording, recordingTimes)
    
    print(interpolatedRecording)

    non0Reference, referenceTimes = maskAndNull(referencePitch, referenceTimes)
    interpolatedReference, referenceTimes = interpolateValues(non0Reference, referenceTimes)

    minMaxRecording = minMaxNormalise(interpolatedRecording)
    minMaxReference = minMaxNormalise(interpolatedReference)

    distance, path = getDTWMapping(minMaxRecording, minMaxReference)

    majorDifferences = getDifferences(minMaxReference, minMaxRecording, path, 0.7)

    # Need to return: pitches / times / major differences
    analysisResult = {
        "pitchRecording": interpolatedRecording.tolist(),
        "timestampsRecording": recordingTimes.tolist(),
        "pitchReference": interpolatedReference.tolist(),
        "timestampsReference": referenceTimes.tolist(),
        "majorDifferences": majorDifferences
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

def getDTWMapping(pitchContourA, pitchContourB):
    # DTW expects 1D elements, (x,y), so need to reshape each element to 1d from (y).
    distance, path = fastdtw(pitchContourA.reshape(-1,1), pitchContourB.reshape(-1,1), dist=euclidean, radius = 1)
    return distance, path

def minMaxNormalise(pitchA):
    maxVal = max(pitchA)
    minVal = min(pitchA)
    normalisedA = (pitchA- minVal) / (maxVal - minVal)
    return normalisedA

def getDifferences(pitchA, pitchB, path, threshold=1):
    # i = recording index, j = reference index.
    majorDeviation = []
    for (i, j) in path:
        deviation = pitchB[i] - pitchA[j]
        print(deviation)
        if abs(deviation) > threshold:
            majorDeviation.append({"index_Rec": i, "index_Ref": j, "deviation": deviation})
    return majorDeviation


# -----------------------

if __name__ == "__main__":
    uvicorn.run("parsel_API:app1", host="0.0.0.0", port=8001, reload=True)