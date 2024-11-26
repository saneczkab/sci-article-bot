using Bot.TelegramBot;
using Ninject;

namespace Bot.Bot;

public static class KernelHandler
{
    public static IKernel Kernel => new StandardKernel(new BotModule());
}