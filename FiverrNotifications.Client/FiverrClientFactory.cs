using FiverrNotifications.Logic.Clients;

namespace FiverrNotifications.Client
{
    public class FiverrClientFactory : IFiverrClientFactory
    {
        public IFiverrClient Create() => new FiverrClient();
    }
}
