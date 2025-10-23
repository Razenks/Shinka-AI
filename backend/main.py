from fastapi import FastAPI, Request
from fastapi.middleware.cors import CORSMiddleware
from services import gemini_service

app = FastAPI()

app.add_middleware(
    CORSMiddleware, 
    allow_origins= ["*"],
    allow_credentials= True,
    allow_methods= ["*"],
    allow_headers=["*"],
)

@app.post("/chat")
async def chat(request: Request):
    data = await request.json()
    user_question = data.get("question")
    
    if not user_question:
        return {"error": "Pergunta não enviada."}
    
    response = gemini_service.ai_response(user_question)
    return {"answer": response}