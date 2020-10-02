using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace FiverrNotifications.Logic.Helpers
{
    public class Subscription : IDisposable
    {
        private readonly ILogger<Subscription> _logger;
        private readonly HashSet<IDisposable> _subscriptions = new HashSet<IDisposable>();

        public Subscription(ILogger<Subscription> logger)
        {
            _logger = logger;
        }

        public void Add(IDisposable subscription)
        {
            try
            {
                _subscriptions.Add(subscription);
            }
            catch
            {
                lock (_subscriptions)
                {
                    _subscriptions.Add(subscription);
                }
            }
        }

        public void Remove(IDisposable subscription)
        {
            try
            {
                _subscriptions.Remove(subscription);
            }
            catch
            {
                lock (_subscriptions)
                {
                    _subscriptions.Remove(subscription);
                }
            }
        }

        public void Dispose()
        {
            lock (_subscriptions)
            {
                foreach (IDisposable subscription in _subscriptions)
                {
                    try
                    {
                        subscription.Dispose();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, ex.Message);
                    }
                }

                _subscriptions.Clear();
            }
        }
    }
}
