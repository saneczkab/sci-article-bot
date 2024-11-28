from datetime import date, datetime, timezone
from functools import partial
from typing import Optional

from redis_om import Field, HashModel


ISSN_REGEX = r"^[0-9]{4}-[0-9]{3}[0-9X]$"


class Article(HashModel):
    title: str
    entity_created: datetime = Field(default_factory=partial(datetime.now, timezone.utc))
    doi: Optional[str] = Field(index=True)
    author: Optional[str]
    publisher: Optional[str]
    issued: Optional[date] = Field(index=True)
    journal: Optional[str]
    url: Optional[str]
    print_issn: Optional[str] = Field(regex=ISSN_REGEX)
    electronic_issn: Optional[str] = Field(regex=ISSN_REGEX)
