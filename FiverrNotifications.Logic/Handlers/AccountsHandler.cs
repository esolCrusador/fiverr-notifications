using FiverrNotifications.Logic.Helpers;
using FiverrNotifications.Logic.Models;
using FiverrNotifications.Logic.Repositories;
using FiverrNotifications.Logic.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Reactive.Linq;

namespace FiverrNotifications.Logic.Handlers
{
    public class AccountsHandler
    {
        private readonly ILogger<AccountsHandler> _accountsHandlerLogger;
        private readonly ILogger<AccountSessionHandler> _accountsLogger;
        private readonly SubscriptionFactory _subscriptionFactory;
        private readonly IAccountsService _accountsService;
        private readonly IChatsRepository _chatsRepository;
        private readonly ConcurrentDictionary<int, AccountSessionHandler> _accounts = new ConcurrentDictionary<int, AccountSessionHandler>();
        private IObservable<SessionData> _sessionsObservable;

        public AccountsHandler(ILogger<AccountsHandler> accountsHandlerLogger, ILogger<AccountSessionHandler> accountsLogger, SubscriptionFactory subscriptionFactory, IAccountsService accountsService, IChatsRepository chatsRepository)
        {
            this._accountsHandlerLogger = accountsHandlerLogger;
            _accountsLogger = accountsLogger;
            _subscriptionFactory = subscriptionFactory;
            _accountsService = accountsService;
            _chatsRepository = chatsRepository;
        }

        public void Initialize()
        {
            _sessionsObservable = Observable.Create<SessionData>(observer =>
            {
                var subscription = _subscriptionFactory.Create();

                var sessionsSubscription = _accountsService.GetSessions().Subscribe(session =>
                {
                    if (_accounts.TryGetValue(session.SessionId, out var sessionHandler))
                    {
                        if (session.IsDeleted)
                        {
                            _accounts.TryRemove(session.SessionId, out sessionHandler);
                            sessionHandler.Dispose();
                            subscription.Remove(sessionHandler);

                            observer.OnNext(session);
                        }
                    }
                    else
                    {
                        sessionHandler = new AccountSessionHandler(session, _accountsLogger, _chatsRepository, _subscriptionFactory);
                        sessionHandler.Initialize();
                        subscription.Add(sessionHandler);
                        _accounts.TryAdd(session.SessionId, sessionHandler);

                        subscription.Add(
                            sessionHandler.SessionChanges.Subscribe(sessionChanges => observer.OnNext(sessionChanges))
                        );
                    }
                },
                error => observer.OnError(error),
                () => observer.OnCompleted()
                );
                subscription.Add(sessionsSubscription);

                return () => subscription.Dispose();
            }).LogException(_accountsHandlerLogger);
        }

        public IObservable<SessionData> GetSessions() => _sessionsObservable;
    }
}
