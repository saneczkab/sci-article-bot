## SciArticleBot
Бот, который раз в сутки отправляет информацию о новых научных статьях по введённому пользователем запросу.  
Сервис функционирует в Telegram: @sci_updates_bot  
Примечание: токен бота, встречающийся в старых коммитах, неактуален.  

## Запуск
Для запуска вам понадобится:
* Python >= 3.10
* C# & dotnet 8.0
* Redis Stack

### Redis Stack
Запустите Redis Stack на порту 6379. Проще всего сделать это с помощью Docker:
```
docker run -p 6379:6379 redis/redis-stack
```


>Важно: необходимо запустить Redis Stack перед всем остальным, так как другие элементы зависят от него.

### Python

1. Установите все зависимости из файла `Scraper/requirements.txt`:
```
pip install -r Scaper/requirements.txt
```

2. Запустите файл `Scraper/main.py`:
```
python Scraper/main.py
```

### C#
```
cd TelegramBot
dotnet run
```
