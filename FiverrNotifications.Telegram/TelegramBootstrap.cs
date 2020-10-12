using FiverrNotifications.Logic.Services;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace FiverrNotifications.Telegram
{
    public static class TelegramBootstrap
    {
        public static void BootstrapTelegram(this IServiceCollection serviceCollection, Func<IServiceProvider, IResourceResolver> getResourceResolver)
        {
            serviceCollection.AddSingleton<BotClientFactory>();
            serviceCollection.AddSingleton<IAccountsService, TelegramAccountsHandler>();
            serviceCollection.AddSingleton<MessageSanitizer>();
            serviceCollection.AddSingleton<MessageFactory>();
            serviceCollection.AddSingleton(getResourceResolver);
            serviceCollection.AddSingleton<MessageSender>();
        }
    }
}
