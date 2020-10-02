using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using FiverrNotifications.Logic.Clients;
using FiverrNotifications.Logic.Exceptions;
using FiverrNotifications.Logic.Helpers;
using FiverrNotifications.Logic.Models;
using FiverrNotifications.Logic.Repositories;
using Microsoft.Extensions.Logging;

namespace FiverrNotifications.Logic.Handlers
{
    public class FiverrSessionsHandler : IDisposable
    {
        private readonly ILogger<FiverrSessionsHandler> _logger;
        private readonly IFiverrClientFactory _fiverrClientFactory;
        private readonly AccountsHandler _accountsService;
        private readonly IMessagesRepository _messageRepository;
        private readonly Subscription _subscriptions;
        private readonly IObservable<long> _interval;
        private readonly ConcurrentDictionary<int, (BehaviorSubject<SessionData> SessionData, IFiverrClient FiverrClient, IDisposable Subscription)> _sessions = new ConcurrentDictionary<int, (BehaviorSubject<SessionData> SessionData, IFiverrClient FiverrClient, IDisposable Subscription)>();

        public FiverrSessionsHandler(ILogger<FiverrSessionsHandler> logger, SubscriptionFactory subscriptionFactory, IFiverrClientFactory fiverrClientFactory, AccountsHandler accountsHandler, IMessagesRepository messageRepository)
        {
            _logger = logger;
            _fiverrClientFactory = fiverrClientFactory;
            _accountsService = accountsHandler;
            _messageRepository = messageRepository;
            _subscriptions = subscriptionFactory.Create();
            _interval = Observable.Interval(TimeSpan.FromMinutes(1)).StartWith(0).Replay(1).RefCount();
        }

        public void Initialize()
        {
            HandleFiverrAccounts();
        }

        public void Dispose() => _subscriptions.Dispose();

        private void HandleFiverrAccounts()
        {
            _subscriptions.Add(
                _accountsService.GetSessions().Subscribe(session =>
                {
                    if (_sessions.TryGetValue(session.SessionId, out var existingSession))
                    {
                        if (session.IsDeleted)
                        {
                            if (_sessions.Remove(session.SessionId, out existingSession))
                            {
                                existingSession.Subscription.Dispose();
                                existingSession.FiverrClient.Dispose();
                                _subscriptions.Remove(existingSession.Subscription);
                                _subscriptions.Remove(existingSession.FiverrClient);
                            }
                        }
                        else
                        {
                            existingSession.SessionData.OnNext(session);
                        }
                    }
                    else
                    {
                        var sessionsSubj = new BehaviorSubject<SessionData>(session);
                        var fiverrClient = _fiverrClientFactory.Create();
                        _subscriptions.Add(fiverrClient);

                        var subscription = sessionsSubj.Select(sessionData => _interval
                        .SelectAsync(async interval =>
                        {
                            _logger.LogDebug($"Handling Interval: {TimeSpan.FromTicks(interval)}. Session Data: {System.Text.Json.JsonSerializer.Serialize(session)}");

                            if (session.Username == null || !session.Session.HasValue || session.Token == null)
                                return await Task.FromResult(new List<FiverrRequest>());

                            try
                            {
                                return await fiverrClient.GetRequsts(session.Username, session.Session.Value, session.Token);
                            }
                            catch (WrongCredentialsException)
                            {
                                await session.SessionCommunicator.SendMessage("Wrong credentials");
                                return await Task.FromResult(new List<FiverrRequest>());
                            }
                        }))
                        .Switch()
                        .SelectAsync(async requests => await _messageRepository.FindNewRequests(session.SessionId, requests))
                        .SelectAsync(async newRequests => await Task.WhenAll(newRequests.Select(async r => await session.SessionCommunicator.SendMessage(r))))
                        .Subscribe();

                        _subscriptions.Add(subscription);
                    }
                })
            );
        }
    }
}
