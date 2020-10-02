using FiverrNotifications.Logic.Helpers;
using FiverrNotifications.Logic.Models;
using FiverrNotifications.Logic.Repositories;
using FiverrNotifications.Logic.Services;
using System;
using System.Collections.Concurrent;
using System.Reactive.Linq;

namespace FiverrNotifications.Logic.Handlers
{
    public class AccountsHandler
    {
        private readonly SubscriptionFactory _subscriptionFactory;
        private readonly IAccountsService _accountsService;
        private readonly IChatsRepository _chatsRepository;
        private readonly ConcurrentDictionary<int, AccountSessionHandler> _accounts = new ConcurrentDictionary<int, AccountSessionHandler>();
        private IObservable<SessionData> SessionsObservable;

        public AccountsHandler(SubscriptionFactory subscriptionFactory, IAccountsService accountsService, IChatsRepository chatsRepository)
        {
            _subscriptionFactory = subscriptionFactory;
            _accountsService = accountsService;
            _chatsRepository = chatsRepository;
        }

        public void Initialize()
        {
            SessionsObservable = Observable.Create<SessionData>(observer =>
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
                        sessionHandler = new AccountSessionHandler(session, _chatsRepository, _subscriptionFactory);
                        sessionHandler.StartHandle();
                        subscription.Add(sessionHandler);
                        _accounts.TryAdd(session.SessionId, sessionHandler);

                        subscription.Add(
                            sessionHandler.SessionChanges.Subscribe(sessionChanges => observer.OnNext(sessionChanges))
                        );
                    }
                });
                subscription.Add(sessionsSubscription);

                return () => subscription.Dispose();
            });
        }

        public IObservable<SessionData> GetSessions() => SessionsObservable;
    }
}
