from fastapi import FastAPI, File, UploadFile
from fastapi.responses import JSONResponse

import whisper
import io
import torchaudio

import numpy as np
from pydub import AudioSegment
from scipy.io.wavfile import write

model = whisper.load_model("turbo")
app = FastAPI()

# --------------------- TEST URIs ----------------------------

@app.get("/testHelloWorld")
async def root():
    return {"message": "Hello World"}

# ------------------- IMPLEMENTATION URIs --------------------

@app.post("/transcribeAudio")
async def transcribeAudio(file: UploadFile = File(...)):
    uploadedAudio = await file.read()

    # whisper doesn't take in bytes, going to save as file path and use that instead:
    file_path = "1.mp3"
    # Write the byte data to a file
    with open(file_path, "wb") as f:    
        f.write(uploadedAudio)
    transcription = whisperTranscribe("1.mp3")
    return JSONResponse(content=transcription)

# ------------- AUDIO PROCESSING FUNCTIONS --------------------

def whisperTranscribe(audioFile):
    result = model.transcribe(audioFile, language="zh", word_timestamps=True)
    transcriptionResult = {
        "transcription": result.get("text", ""),
        "segments": result.get("segments", [])  # Word timestamps
    }
    print(transcriptionResult)
    return transcriptionResult
