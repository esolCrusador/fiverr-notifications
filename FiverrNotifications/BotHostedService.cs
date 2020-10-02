using FiverrNotifications.Logic.Handlers;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FiverrNotifications
{
    public class BotHostedService : IHostedService
    {
        private readonly MainHandler _mainHandler;
        public BotHostedService(MainHandler mainHandler) => _mainHandler = mainHandler;
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _mainHandler.Initialize();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _mainHandler.Dispose();
            return Task.CompletedTask;
        }
    }
}
