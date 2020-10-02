using Microsoft.Extensions.Logging;

namespace FiverrNotifications.Logic.Helpers
{
    public class SubscriptionFactory
    {
        private readonly ILogger<Subscription> _logger;

        public SubscriptionFactory(ILogger<Subscription> logger) => _logger = logger;

        public Subscription Create() => new Subscription(_logger);
    }
}
