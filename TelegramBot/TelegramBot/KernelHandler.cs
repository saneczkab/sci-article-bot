using Ninject;

namespace Bot.TelegramBot;

public static class KernelHandler
{
    public static IKernel Kernel => new StandardKernel(new BotModule());
}