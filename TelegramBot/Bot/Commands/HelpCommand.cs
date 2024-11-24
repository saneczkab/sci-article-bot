using Telegram.Bot;

namespace Bot.TelegramBot.Commands;

public class HelpCommand : ICommand
{
    private readonly ITelegramBotClient _botClient;
    private readonly User _user;
    private readonly CancellationToken _cancellationToken;

    private const string HelpMessage = "Добро пожаловать в бота, который отправляет уведомления о новых научных статьях! " +
                                       "В боте доступны следующие команды:\n" +
                                       "/help - список доступных команд\n" +
                                       "/new - добавить новый запрос в рассылку\n" +
                                       "/last - показать последние 5 опубликованных статей для запроса\n" +
                                       "/remove - удалить из рассылки один из запросов\n";

    public HelpCommand(ITelegramBotClient botClient, User user, CancellationToken cancellationToken)
    {
        _botClient = botClient;
        _user = user;
        _cancellationToken = cancellationToken;
    }

    public async Task Execute()
    {
        await _botClient.SendMessage(chatId: _user.Id, text: HelpMessage, 
            replyMarkup: MessageHandler.CommandsKeyboard, cancellationToken: _cancellationToken);
    }
}