from fastapi import FastAPI, File, UploadFile
from fastapi.responses import JSONResponse
import uvicorn

import whisper

model = whisper.load_model("small")
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
    file_path = "1.wav"
    # Write the byte data to a file
    with open(file_path, "wb") as f:    
        f.write(uploadedAudio)
    transcription = whisperTranscribe("1.wav")
    return JSONResponse(content=transcription)

# ------------- AUDIO PROCESSING FUNCTIONS --------------------

def whisperTranscribe(audioFile):
    result = model.transcribe(audioFile, language="zh", word_timestamps=True)
    words_list = [segment["words"] for segment in result.get("segments")]
    transcriptionResult = {
        "transcription": result.get("text", ""),
        "segments": words_list  # Word segment + timestamp + probability
    }
    print(transcriptionResult)
    return transcriptionResult


if __name__ == "__main__":
    uvicorn.run("whisper_API:app", host="0.0.0.0", port=8000, reload=True)
