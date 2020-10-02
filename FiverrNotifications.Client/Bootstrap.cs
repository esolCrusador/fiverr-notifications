using FiverrNotifications.Logic.Clients;
using Microsoft.Extensions.DependencyInjection;

namespace FiverrNotifications.Client
{
    public static  class FiverrClientBootstrap
    {
        public static void BootstrapFiverrClient(this IServiceCollection services)
        {
            services.AddSingleton<IFiverrClientFactory, FiverrClientFactory>();
        }
    }
}
