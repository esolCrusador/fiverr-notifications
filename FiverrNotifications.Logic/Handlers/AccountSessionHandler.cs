using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using FiverrNotifications.Logic.Helpers;
using FiverrNotifications.Logic.Models;
using FiverrNotifications.Logic.Repositories;

namespace FiverrNotifications.Logic.Handlers
{
    public class AccountSessionHandler : IDisposable
    {
        private readonly SessionData _sessionData;
        private readonly IChatsRepository _chatsRepository;
        private readonly Subscription _subscription;
        private readonly BehaviorSubject<SessionData> _sessionSubject;

        private bool _commandInProgress = false;

        private readonly Dictionary<string, Func<ISessionCommunicator, IObservable<int>>> _supportedMessages;

        public AccountSessionHandler(SessionData sessionData, IChatsRepository chatsRepository, SubscriptionFactory subscriptionFactory)
        {
            _sessionData = sessionData;
            _chatsRepository = chatsRepository;
            _subscription = subscriptionFactory.Create();
            _sessionSubject = new BehaviorSubject<SessionData>(sessionData);
            _subscription.Add(_sessionSubject);
            SessionChanges = _sessionSubject;

            _supportedMessages = new Dictionary<string, Func<ISessionCommunicator, IObservable<int>>>(StringComparer.OrdinalIgnoreCase)
            {
                {"Username", communicator => RequestUsername()},
                {"Session", communicator => RequestSession()},
                {"Token", communicator => RequestAuthToken()}
            };
        }

        public void StartHandle()
        {
            if (_sessionData.Username == null)
            {
                _subscription.Add(
                    RequestUsername()
                        .Select(r => RequestSession())
                        .Concat()
                        .Select(r => RequestAuthToken())
                        .Concat()
                        .Select(r => StartHandleMessages())
                        .Concat()
                        .Subscribe()
                );
            }
            else
            {
                _subscription.Add(
                    StartHandleMessages().Subscribe()
                );
            }


        }

        public void Dispose() => _subscription.Dispose();

        public IObservable<SessionData> SessionChanges;

        private IObservable<int> StartHandleMessages()
        {
            return _sessionData.SessionCommunicator.Messages
                .Where(m => !_commandInProgress)
                .Select(m =>
                    {
                        IObservable<int> result;
                        if (!string.IsNullOrWhiteSpace(m) && _supportedMessages.TryGetValue(m.Trim(), out var handler))
                            result = handler(_sessionData.SessionCommunicator);
                        else
                            result = Observable.Create<Task>(observer =>
                            {
                                var task = _sessionData.SessionCommunicator.SendMessage("Unknown command");
                                observer.OnNext(task);
                                observer.OnCompleted();

                                return task;
                            }).SelectAsync();

                        return result;
                    })
                .Merge();
        }

        private IObservable<int> RequestUsername()
        {
            var getUsername = _sessionData.SessionCommunicator.Messages
                .FirstAsync()
                .Select(async m =>
                {
                    _sessionData.Username = m;
                    await UpdateSession();
                });

            _commandInProgress = true;
            return Observable.FromAsync(cancellation => _sessionData.SessionCommunicator.SendMessage("Please enter username"))
                .Select(r => getUsername)
                .Concat()
                .SelectAsync()
                .Do(t => _commandInProgress = false);
        }

        private IObservable<int> RequestSession()
        {
            var getSessionId = _sessionData.SessionCommunicator.Messages
                .FirstAsync()
                .SelectAsync(async m =>
                {
                    _sessionData.Session = Guid.Parse(m);
                    await UpdateSession();
                });

            _commandInProgress = true;
            var requestSession = Observable.FromAsync(cancellation => _sessionData.SessionCommunicator.SendMessage("Please enter session id"));

            return requestSession.Select(r => getSessionId).Concat().Retry(5).Do(t => _commandInProgress = false);
        }

        private IObservable<int> RequestAuthToken()
        {
            var getAuthToken = _sessionData.SessionCommunicator.Messages
                .FirstAsync()
                .SelectAsync(async m =>
                {
                    _sessionData.Token = m;
                    await UpdateSession();
                });

            _commandInProgress = true;
            var requestToken = Observable.FromAsync(cancellation => _sessionData.SessionCommunicator.SendMessage("Please enter hodor_creds cookie"));

            return requestToken.Select(r => getAuthToken).Concat().Do(t => _commandInProgress = false);
        }

        private StoredSession GetStoredSession() =>
            new StoredSession
            {
                SessionId = _sessionData.SessionId,
                Username = _sessionData.Username,
                Session = _sessionData.Session,
                Token = _sessionData.Token
            };

        private async Task UpdateSession()
        {
            await _chatsRepository.UpdateSession(GetStoredSession());
            _sessionSubject.OnNext(_sessionData);
        }
    }
}
