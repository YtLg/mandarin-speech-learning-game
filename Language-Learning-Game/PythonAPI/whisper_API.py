import contextlib
import wave
from fastapi import FastAPI, File, UploadFile
from fastapi.responses import JSONResponse
import uvicorn
import whisperx

device = "cuda"
batch_size = 16
compute_type = "float16"

model = whisperx.load_model("large-v2", device, compute_type=compute_type)
# model = whisperx.load_model("large-v2").to("cuda") # Run on GPU -> needs numpy 2.0
app = FastAPI()


# print(torch.cuda.device_count())
# print(torch.cuda.get_device_name(0))

# --------------------- TEST URIs ----------------------------

@app.get("/testHelloWorld")
async def root():
    return {"message": "Hello World"}

# ------------------- IMPLEMENTATION URIs --------------------

@app.post("/transcribeAudio")
async def transcribeAudio(file: UploadFile = File(...)):
    uploadedAudio = await file.read()
    print("Transcription: Request Recieved!")

    # Write the byte data to a file
    with open("1.wav", "wb") as f:    
        f.write(uploadedAudio)
        f.close
    print("Transcription: Request saved!")

    transcription = whisperTranscribe("1.wav")

    print("Transcription: Transcription complete and returning!")
    return JSONResponse(content=transcription)

# ------------- AUDIO PROCESSING FUNCTIONS --------------------

# Transcribes the audio data and aligns it to get an accurate timestamp.
def whisperTranscribe(audioFile):
    audio = whisperx.load_audio(audioFile)
    result = model.transcribe(audio, batch_size=batch_size, language="zh")

    model_a, metadata = whisperx.load_align_model(language_code=result["language"], device=device)
    resultAligned = whisperx.align(result["segments"], model_a, metadata, audio, device, return_char_alignments=True)

    wordsList = []
    for item in resultAligned["segments"][0]["words"]:
        if "start" not in item or "end" not in item or "score" not in item: #Skip punctuations which have no start/end/score.
            continue
        wordsList.append({
            "word":item["word"],
            "start":item["start"],
            "end":item["end"],
            "score":item["score"]
        })

    # remove all punctuation, special charactes & spaces.
    temp = resultAligned["segments"][0]["text"]
    tempStripped = ''.join(e for e in temp if e.isalnum())

    wordsList[-1]["end"] = getDuration("1.wav")
    # Prepare final transcription result for return
    transcriptionResult = {
        "transcription": tempStripped,
        "words": wordsList
    }

    return transcriptionResult

# --------------- HELPER FUNCTIONS ----------------

def getDuration(file): # Gets the duration of the audio file path provided.
    with contextlib.closing(wave.open(file,'r')) as f: 
        frames = f.getnframes()
        rate = f.getframerate()
        length = frames/float(rate)
    return length


if __name__ == "__main__":
    uvicorn.run("whisper_API:app", host="0.0.0.0", port=8000, reload=True)
