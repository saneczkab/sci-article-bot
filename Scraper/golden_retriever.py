from datetime import datetime, timedelta, date

from habanero import Crossref

from infrastructure.models import Article, Journal


def search_today_articles(query: str) -> list[Article]:
    cr = Crossref()
    now = datetime.now().strftime("%Y-%m-%d")
    yesterday = (datetime.now() - timedelta(days=1)).strftime("%Y-%m-%d")

    results = cr.works(
        query=query,
        filter={
            "from-online-pub-date": yesterday,
            "until-online-pub-date": now}
    )

    articles = []
    for item in results['message']['items']:
        journal_title = item.get('container-title', [None])[0]
        if journal_title:
            print_issn = None
            electronic_issn = None

            for issn_info in item.get('issn-type', []):
                match issn_info.get('type'):
                    case "electronic":
                        electronic_issn = issn_info.get("value")
                    case "print":
                        print_issn = issn_info.get("value")
            journal = Journal(
                title=journal_title, print_issn=print_issn, electronic_issn=electronic_issn)
        else:
            journal = None

        article = Article(
            title=item.get('title', [None])[0],
            doi=item.get('DOI', None),
            author=_format_author(item) or None,
            publisher=item.get('publisher', None),
            issued=_dateparts_to_date(item.get('issued', {}).get('date-parts', [None])[0]),
            journal=journal,
            url=item.get('URL', None),
        )

        articles.append(article)

    return articles


def _dateparts_to_date(dateparts):
    if dateparts is None:
        return None
    return date(*dateparts)


def _format_author(item):
    return ', '.join(f"{author['family']} {author['given']}" for author in item.get('author', []))
