using FiverrNotifications.Logic.Helpers;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FiverrNotifications
{
    public class MaintananceHostedService : IHostedService
    {
        private readonly Subscription _subscription;
        private readonly ILogger<MaintananceHostedService> _logger;
        private HttpClient _httpClient;

        public MaintananceHostedService(ILogger<MaintananceHostedService> logger, SubscriptionFactory subscriptionFactory)
        {
            _subscription = subscriptionFactory.Create();
            _logger = logger;
            _httpClient = new HttpClient();

            _subscription.Add(_httpClient);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            var timer = Observable.Interval(TimeSpan.FromMinutes(1)).StartWith(0);

            var subscription = timer.SelectAsync(async interval => await Ping()).Subscribe();
            _subscription.Add(subscription);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _subscription.Dispose();
            return Task.CompletedTask;
        }

        private async Task Ping()
        {
            try
            {
                var response = await _httpClient.GetAsync("https://fiverr-notifications.azurewebsites.net/api/ping");

                _logger.LogWarning($"Ping: {await response.Content.ReadAsStringAsync()}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }
    }
}
