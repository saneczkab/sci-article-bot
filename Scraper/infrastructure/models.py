from datetime import date, datetime, timezone
from typing import Optional

from redis_om import EmbeddedJsonModel, Field, JsonModel

ISSN_REGEX = r"^[0-9]{4}-[0-9]{3}[0-9X]$"


class Journal(EmbeddedJsonModel):
    title: str
    print_issn: Optional[str] = Field(schema_extra=dict(pattern=ISSN_REGEX))
    electronic_issn: Optional[str] = Field(schema_extra=dict(pattern=ISSN_REGEX))


class Article(EmbeddedJsonModel):
    title: str
    doi: Optional[str]
    author: Optional[str]
    publisher: Optional[str]
    issued: Optional[date]
    journal: Optional[Journal]
    url: Optional[str]


class Query(EmbeddedJsonModel):
    text: str
    last_search: Optional[datetime] = None
    new_articles: list[Article] = Field(default_factory=list)


class User(JsonModel):
    id: int = Field(primary_key=True)
    status: int = 0
    queries: list[Query]
    shown_articles_dois: set[str] = Field(default_factory=set)

    class Meta:
        model_key_prefix = "User"
