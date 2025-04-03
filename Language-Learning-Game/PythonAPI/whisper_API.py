import contextlib
import torch
import wave
from fastapi import FastAPI, File, UploadFile
from fastapi.responses import JSONResponse
import uvicorn
import whisperx
from dragonmapper import hanzi
from deep_translator import GoogleTranslator

device = "cuda"
batch_size = 16
compute_type = "float16"
missing = 0
model = whisperx.load_model("turbo", device, compute_type=compute_type)
# model = whisperx.load_model("large-v2").to("cuda") # Run on GPU -> needs numpy 2.0
app = FastAPI()


print(torch.version.cuda)
print(torch.cuda.is_available())
print(torch.cuda.device_count())
print(torch.cuda.get_device_name(0))

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


@app.post("/getTranslations")
async def getTranslations(character: str):
    print("Translating...")
    pinyinWord = hanzi.to_pinyin(character)
    englishWord =  GoogleTranslator(source='auto', target='en').translate(character)  # output -> Weiter so, du bist großartig
    print(englishWord)
    wordTranslations  = {"pinyin":pinyinWord, "english":englishWord}
    return wordTranslations

# ------------- AUDIO PROCESSING FUNCTIONS --------------------

# Transcribes the audio data and aligns it to get an accurate timestamp.
def whisperTranscribe(audioFile):
    missing = 0
    audio = whisperx.load_audio(audioFile)
    result = model.transcribe(audio, batch_size=batch_size, language="zh", task="transcribe")
    model_a, metadata = whisperx.load_align_model(language_code=result["language"], device=device)
    resultAligned = whisperx.align(result["segments"], model_a, metadata, audio, device, return_char_alignments=True)
    
    wordsList = []
    print(len(resultAligned["segments"][0]["words"]))
    for i in range(len(resultAligned["segments"][0]["words"])):
        item = resultAligned["segments"][0]["words"][i] # get current item
        
        # Skip item if it's punctuation transcription
        item["word"] = ''.join(e for e in item["word"] if e.isalnum()) # Remove all punctuation
        if item["word"] == '':
            continue
        
        # If transcription is missing any data.
        if "start" not in item or "end" not in item or "score" not in item: #Skip punctuations which have no start/end/score.
            missing += 1
            item = missingDataCorrection(item, i, resultAligned["segments"][0]["words"])
        
        # Append the current item's data to the return list of words.
        wordsList.append({
            "word":item["word"],
            "start":item["start"],
            "end":item["end"],
            "probability":item["score"]
        })

    # remove all punctuation, special charactes & spaces from transcription.
    temp = resultAligned["segments"][0]["text"]
    tempStripped = ''.join(e for e in temp if e.isalnum())

    # Add whole word in there if word segment is missing
    if missing > 0:
        wordsList.append({
            "word":tempStripped,
            "start":0.0,
            "end":getDuration("1.wav"),
            "probability":wordsList[-1]["probability"]
        })

    wordsList[-1]["end"] = getDuration("1.wav")
    wordsList[0]["start"] = wordsList[0]["start"]*0.5
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

# Fills in the missing transcription using neighboring data.
def missingDataCorrection(item, index, listOfWords):
    if index == 0: # First item
        if len(listOfWords) == 1: # If first item is last item.
            return item 
        item["start"] = 0.0
        item["end"] = listOfWords[index+1]["start"] * 0.9 # -10% from start of next word
        item["score"] = listOfWords[index+1]["score"]
        return item
    if index == len(listOfWords)-1: # Last item
        item["start"] = listOfWords[index-1]["end"] * 1.1 # end of previous item
        item["end"] = getDuration("1.wav") # final timestamp
        item["score"] = listOfWords[index-1]["score"]
        return item
    else:
        item["start"] = listOfWords[index-1]["end"] * 1.1 # end of prev word
        item["end"] = listOfWords[index+1]["start"] * 0.9 # start of next word
        item["score"] = listOfWords[index+1]["score"] # any will work.
        return item
    
if __name__ == "__main__":
    uvicorn.run("whisper_API:app", host="0.0.0.0", port=8000, reload=True)
