from fastapi import FastAPI, File, UploadFile
import whisper

model = whisper.load_model("base")
app = FastAPI()


@app.get("/testHelloWorld")
async def root():
    return {"message": "Hello World"}


@app.post("/transcribeAudio")
async def transcribeAudio(file: UploadFile = File(...)):
    uploadedAudio = await file.read()

    processed_audio = whisperTranscribe(uploadedAudio)



def whisperTranscribe():

    return
