from fastapi import FastAPI

app = FastAPI(title="Type-It-Out API")


@app.get("/")
def root():
    return {"message": "Type-It-Out API is running!"}