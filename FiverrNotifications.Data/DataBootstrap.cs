using FiverrNotifications.Logic.Repositories;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace FiverrTelegramNotifications.Data
{
    public static class DataBootstrap
    {
        public static void BootstrapData(this IServiceCollection serviceCollection, Func<IServiceProvider, string> getConnnectionString)
        {
            serviceCollection.AddSingleton<IChatsRepository, ChatsRepository>();
            serviceCollection.AddSingleton<IMessagesRepository, MessagesRepository>();
            serviceCollection.AddSingleton(s => new SqlConnectionFactory(getConnnectionString(s)));
        }
    }
}
