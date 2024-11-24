from dataclasses import dataclass
from datetime import date


@dataclass
class Article:
    title: str
    doi: str
    author: str
    publisher: str
    issued: date
    journal: str
    url: str
    print_issn: str
    electronic_issn: str
