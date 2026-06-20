from pydantic import BaseModel


class NoteCreate(BaseModel):
    pass


class NoteUpdate(BaseModel):
    pass


class NoteRead(BaseModel):
    pass


class ErrorResponse(BaseModel):
    pass
