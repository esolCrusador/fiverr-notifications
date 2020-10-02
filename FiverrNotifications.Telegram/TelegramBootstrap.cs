using FiverrNotifications.Logic.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FiverrNotifications.Telegram
{
    public static class TelegramBootstrap
    {
        public static void BootstrapTelegram(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<BotClientFactory>();
            serviceCollection.AddSingleton<IAccountsService, TelegramAccountsHandler>();
            serviceCollection.AddSingleton<MessageSanitizer>();
            serviceCollection.AddSingleton<MessageFactory>();
        }
    }
}
