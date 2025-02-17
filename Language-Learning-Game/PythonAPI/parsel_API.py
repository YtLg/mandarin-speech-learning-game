from fastapi import FastAPI
import uvicorn

app = FastAPI()

@app.get("/")
async def hello():
    return {"message": "helloooo"}

if __name__ == "__main__":
    uvicorn.run("parsel_API.py", host="0.0.0.0", port=8001, reload=True)