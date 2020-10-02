using FiverrNotifications.Logic.Services;
using System;
using System.Threading.Tasks;

namespace FiverrNotifications.Logic.Handlers
{
    public class MainHandler : IDisposable
    {
        private readonly IAccountsService _accountsService;
        private readonly AccountsHandler _accountsHandler;
        private readonly FiverrSessionsHandler _fiverrSessionsHandler;

        public MainHandler(IAccountsService accountsService, AccountsHandler accountsHandler, FiverrSessionsHandler fiverrSessionsHandler)
        {
            _accountsService = accountsService;
            _accountsHandler = accountsHandler;
            _fiverrSessionsHandler = fiverrSessionsHandler;
        }

        public async Task Initialize()
        {
            await _accountsService.InitializeAsync();
            _accountsHandler.Initialize();
            _fiverrSessionsHandler.Initialize();

            _accountsService.Start();
        }

        public void Dispose()
        {
            _accountsService.Dispose();
            _fiverrSessionsHandler.Dispose();
        }
    }
}
