from pathlib import Path
from typing import Iterable

from infrastructure.models import Article

APPROVED_ISSNS = set()

with open(Path(__file__).parent/"assets/issns.txt") as f:
    APPROVED_ISSNS.update(line.strip() for line in f)


def filter_articles(articles: Iterable[Article]) -> Iterable[Article]:
    """
    Фильтрует список статей по ISSN, проверяя их наличие в списке журналов,
    утверждённых РЦНИ.
    """

    return filter(
        lambda article: article.journal and (
            article.journal.print_issn in APPROVED_ISSNS
            or article.journal.electronic_issn in APPROVED_ISSNS),
        articles)
