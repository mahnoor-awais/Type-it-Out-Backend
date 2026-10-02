from fastapi import FastAPI

from .database import Base, engine
from .models import *


Base.metadata.create_all(bind=engine)


app = FastAPI(title="Type-It-Out API")


@app.get("/")
def root():
    return {"message": "Type-It-Out API is running!"}