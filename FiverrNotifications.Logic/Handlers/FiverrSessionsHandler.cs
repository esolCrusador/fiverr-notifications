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
        private readonly TaskHelper _taskHelper;
        private readonly ConcurrentDictionary<int, (BehaviorSubject<SessionData> SessionData, IFiverrClient FiverrClient, IDisposable Subscription)> _sessions = new ConcurrentDictionary<int, (BehaviorSubject<SessionData> SessionData, IFiverrClient FiverrClient, IDisposable Subscription)>();

        public FiverrSessionsHandler(ILogger<FiverrSessionsHandler> logger, SubscriptionFactory subscriptionFactory, IFiverrClientFactory fiverrClientFactory, AccountsHandler accountsHandler, IMessagesRepository messageRepository)
        {
            _logger = logger;
            _fiverrClientFactory = fiverrClientFactory;
            _accountsService = accountsHandler;
            _messageRepository = messageRepository;
            _subscriptions = subscriptionFactory.Create();
            _taskHelper = new TaskHelper(logger);
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

                        var subscription = _interval
                        .Select(interval => sessionsSubj
                        .Where(session => !session.IsPaused)
                        .SelectAsync(async session  =>
                        {
                            _logger.LogDebug($"Handling Interval: {TimeSpan.FromTicks(interval)}. Session Data: {System.Text.Json.JsonSerializer.Serialize(session)}");

                            if (session.Username == null || !session.Session.HasValue || session.Token == null)
                                return await Task.FromResult(new List<FiverrRequest>());

                            try
                            {
                                var requests = await fiverrClient.GetRequsts(session.Username, session.Session.Value, session.Token);

                                if (session.IsAccountUpdated)
                                {
                                    await session.SessionCommunicator.SendMessage(MessageType.SuccessfullyConnected, !session.IsMuted);
                                    session.IsAccountUpdated = false;
                                }

                                return requests;
                            }
                            catch (WrongCredentialsException)
                            {
                                await session.SessionCommunicator.SendMessage(MessageType.WrongCredentials, !session.IsMuted);
                            }
                            catch(Exception ex)
                            {
                                _logger.LogError(ex, ex.Message);
                            }

                            return await Task.FromResult(new List<FiverrRequest>());
                        }))
                        .Switch()
                        .SelectAsync(async requests => await _taskHelper.Safe(_messageRepository.FindNewRequests(session.SessionId, requests)))
                        .Where(newRequests => newRequests != null)
                        .SelectMany(newRequests => newRequests)
                        .SelectAsync(async newRequest => await _taskHelper.Safe(session.SessionCommunicator.SendMessage(newRequest, !session.IsMuted)))
                        .LogException(_logger)
                        .Subscribe();

                        _subscriptions.Add(subscription);
                        _sessions.TryAdd(session.SessionId, (sessionsSubj, fiverrClient, subscription));
                    }
                })
            );
        }
    }
}
