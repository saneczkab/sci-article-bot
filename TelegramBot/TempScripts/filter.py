import os


class ArticleFilter:
    def __init__(self):
        self.approved_issn_path = os.path.join(os.path.dirname(__file__), "issns.json")
        self.approved_issns = self.load_approved_issns()

    def load_approved_issns(self):
        """Считывает ISSN из txt-файла и возвращает их в виде множества."""
        with open(self.approved_issn_path, "r", encoding="utf-8") as file:
            issns = {line.strip() for line in file if line.strip()}
        return issns

    def get_filter_articles(self, articles):
        """
        Фильтрует список статей по ISSN, проверяя их наличие в self.approved_issns.
        Возвращает первую статью с рецензией, если она найдена.
        """
        approved_articles = [
            article for article in articles if article.get("issn") in self.approved_issns
        ]

        return approved_articles
