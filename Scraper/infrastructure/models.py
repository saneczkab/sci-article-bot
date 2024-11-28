from datetime import date, datetime, timezone
from typing import Optional

from redis_om import EmbeddedJsonModel, Field, JsonModel

ISSN_REGEX = r"^[0-9]{4}-[0-9]{3}[0-9X]$"


class Journal(EmbeddedJsonModel):
    title: str
    print_issn: Optional[str] = Field(regex=ISSN_REGEX)
    electronic_issn: Optional[str] = Field(regex=ISSN_REGEX)


class Article(EmbeddedJsonModel):
    title: str
    entity_created: datetime = Field(default_factory=lambda: datetime.now(timezone.utc))
    doi: Optional[str] = Field(index=True)
    author: Optional[str]
    publisher: Optional[str]
    issued: Optional[date] = Field(index=True)
    journal: Optional[Journal]
    url: Optional[str]


class Query(EmbeddedJsonModel):
    text: str
    last_search: date
    new_articles: list[Article]


class User(JsonModel):
    id: int = Field(primary_key=True)
    queries: list[Query]
    shown_articles_dois: list[str]

    class Meta:
        model_key_prefix = "User"
