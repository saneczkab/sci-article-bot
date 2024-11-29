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
    entity_created: Optional[datetime] = Field(default_factory=lambda: datetime.now(timezone.utc), index=False)
    doi: Optional[str] = Field(index=True)
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
    queries: list[Query]
    shown_articles_dois: set[str] = Field(default_factory=set)

    class Meta:
        model_key_prefix = "User"
