import requests
import sys
import json

from filter import ArticleFilter


def get_last_articles(query, max_articles):
    url = "https://api.crossref.org/works"
    params = {
        "query": query,
        "rows": max_articles,
        "sort": "deposited",
        "order": "desc"
    }

    response = requests.get(url, params=params)
    response.raise_for_status()
    data = response.json()
    articles = []
    for item in data.get("message", {}).get("items", []):
        title = item.get("title", ["-"])[0]
        deposited_date = item.get("deposited", {}).get("timestamp", 0) // 1000
        issn = item.get("ISSN", ["-"])[0]
        article_url = item.get("URL", "-")
        articles.append({
            "title": title,
            "deposited_date": deposited_date,
            "issn": issn,
            "url": article_url
        })
    return articles

def main():
    query = sys.argv[1]
    max_articles = int(sys.argv[2])
    filter = ArticleFilter()

    articles = get_last_articles(query, 100)
    filtered_articles = filter.get_filter_articles(articles)
    if len(filtered_articles) > max_articles:
        print(json.dumps(filtered_articles[:max_articles]))
    else:
        print(json.dumps(filtered_articles))

if __name__ == "__main__":
    main()