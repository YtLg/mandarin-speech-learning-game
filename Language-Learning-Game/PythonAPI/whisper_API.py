from fastapi import FastAPI, File, UploadFile
from fastapi.responses import JSONResponse
import uvicorn
import torch
import whisper
from pprint import pprint

model = whisper.load_model("small").to("cuda") # Run on GPU -> needs numpy 2.0
app = FastAPI()

print(torch.cuda.device_count())  # Should be 1 (for 3070)
print(torch.cuda.get_device_name(0))  # Should print "NVIDIA GeForce RTX 3070"

# --------------------- TEST URIs ----------------------------

@app.get("/testHelloWorld")
async def root():
    return {"message": "Hello World"}

# ------------------- IMPLEMENTATION URIs --------------------

@app.post("/transcribeAudio")
async def transcribeAudio(file: UploadFile = File(...)):
    uploadedAudio = await file.read()
    print("Transcription: Request Recieved!")
    # whisper doesn't take in bytes, going to save as file path and use that instead:
    file_path = "1.wav"
    # Write the byte data to a file
    with open(file_path, "wb") as f:    
        f.write(uploadedAudio)
    print("Transcription: Request saved!")
    transcription = whisperTranscribe("1.wav")
    print("Transcription: Transcription complete and returning!")
    return JSONResponse(content=transcription)

# ------------- AUDIO PROCESSING FUNCTIONS --------------------

def whisperTranscribe(audioFile):
    result = model.transcribe(audioFile, language="zh", word_timestamps=True)
    words_list = []
    
    for segment in result.get("segments", []):
        words_list.extend(segment["words"])

    transcriptionResult = {
        "transcription": result.get("text", ""),
        "words": words_list 
    }

    print(transcriptionResult)
    return transcriptionResult



if __name__ == "__main__":
    uvicorn.run("whisper_API:app", host="0.0.0.0", port=8000, reload=True)
