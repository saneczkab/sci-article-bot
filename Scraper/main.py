from datetime import datetime, timezone
from time import sleep
from typing import Iterable

import schedule
from redis_om import get_redis_connection

from Filter import filter_articles
from golden_retriever import search_today_articles
from infrastructure.db import set_up_models

from infrastructure.models import User

UPDATED_USERS_SET = "updated_users"

redis_connection = get_redis_connection()


class ArticleProcessor:
    def handle_users(self):
        """
        Основной процесс обработки: получение, фильтрация и сохранение статей.
        """

        users: Iterable[User] = User.find()
        for user in users:
            self.handle_user(user)

    def handle_user(self, user: User):
        new_articles_found = False
        for query in user.queries:
            query.last_search = datetime.now(tz=timezone.utc)
            articles = filter_articles(search_today_articles(query.text))
            for article in articles:
                if article.doi in user.shown_articles_dois:
                    continue
                new_articles_found = True
                query.new_articles.append(article)
                user.shown_articles_dois.add(article.doi)
        user.save()

        if new_articles_found:
            redis_connection.sadd(UPDATED_USERS_SET, user.id)

    def schedule_daily_task(self, time):
        """Запускает ежедневное  выполнение функции do_task в time"""
        schedule.every().day.at(time).do(self.handle_users)

    def blocking_run(self):
        while True:
            schedule.run_pending()
            sleep(60)


if __name__ == "__main__":
    set_up_models()
    processor = ArticleProcessor()
    processor.handle_users()
    processor.schedule_daily_task("09:00")

    processor.blocking_run()
