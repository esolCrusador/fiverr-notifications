using FiverrNotifications.Logic.Handlers;
using FiverrNotifications.Logic.Helpers;
using Microsoft.Extensions.DependencyInjection;

namespace FiverrNotifications.Logic
{
    public static class LogicBootstrap
    {
        public static void BootstrapLogic(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<SubscriptionFactory>();
            serviceCollection.AddSingleton<AccountsHandler>();
            serviceCollection.AddSingleton<FiverrSessionsHandler>();
            serviceCollection.AddSingleton<MainHandler>();
        }
    }
}
