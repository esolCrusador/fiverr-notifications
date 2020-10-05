using System;
using System.Collections.Generic;
using System.Reactive;
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
        private readonly Subject<Unit> _cancelSubject;

        private bool _commandInProgress = false;

        private readonly Dictionary<string, Func<ISessionCommunicator, IObservable<Unit>>> _supportedMessages;

        public AccountSessionHandler(SessionData sessionData, IChatsRepository chatsRepository, SubscriptionFactory subscriptionFactory)
        {
            _sessionData = sessionData;
            _chatsRepository = chatsRepository;
            _subscription = subscriptionFactory.Create();
            _sessionSubject = new BehaviorSubject<SessionData>(sessionData);
            _subscription.Add(_sessionSubject);
            SessionChanges = _sessionSubject;
            _cancelSubject = new Subject<Unit>();
            _subscription.Add(_cancelSubject);

            _supportedMessages = new Dictionary<string, Func<ISessionCommunicator, IObservable<Unit>>>(StringComparer.OrdinalIgnoreCase)
            {
                { "/start", communicator => Observable.Empty<Unit>() },
                { "/stop", communicator => Observable.Empty<Unit>() },
                { "/help", communicator => ShowHelp() },
                {"/login", communicator =>  Login()},
                {"/username", communicator => RequestUsername()},
                {"/session", communicator => RequestSession()},
                {"/token", communicator => RequestAuthToken()},
                { "/cancel", communicator => Cancel() },
                { "/pause", communicator => Pause() },
                { "/resume", communicator => Resume() },
            };
        }

        private IObservable<Unit> Pause()
        {
            return Observable.FromAsync(async () =>
            {
                _sessionData.IsPaused = true;
                await UpdateSession();
            }).SelectAsync(_ => SendMessage(MessageType.Paused));
        }

        public IObservable<Unit> Resume()
        {
            return Observable.FromAsync(async () =>
            {
                _sessionData.IsPaused = false;
                await UpdateSession();
            }).SelectAsync(_ => SendMessage(MessageType.Resumed));
        }

        public void Initialize()
        {
            _subscription.Add(
                StartHandleMessages().Subscribe()
            );

        }

        public void Dispose() => _subscription.Dispose();

        public IObservable<SessionData> SessionChanges;

        private IObservable<Unit> ShowHelp()
        {
            return Observable.FromAsync(() => SendMessage(MessageType.Help));
        }

        private IObservable<Unit> Cancel()
        {
            return ObservableHelper.FromAction(() => _cancelSubject.OnNext(Unit.Default))
                .SelectAsync(_ => SendMessage(MessageType.Cancelled));
        }

        private IObservable<Unit> StartHandleMessages()
        {
            return _sessionData.SessionCommunicator.Messages
                .Where(m => !string.IsNullOrWhiteSpace(m) && m.StartsWith('/')) // Handling commands only
                .Select(m => m.Trim())
                .Select(m =>
                    {
                        IObservable<Unit> result;
                        if (_supportedMessages.TryGetValue(m.Trim(), out var handler))
                            result = handler(_sessionData.SessionCommunicator);
                        else
                            result = Observable.FromAsync(() => SendMessage(MessageType.UnknownCommand));

                        if (_commandInProgress && m != "/cancel")
                            _cancelSubject.OnNext(Unit.Default);

                        return result;
                    })
                .Merge();
        }

        private IObservable<Unit> Login()
        {
            return RequestUsername()
                        .Select(r => RequestSession())
                        .Concat()
                        .Select(r => RequestAuthToken())
                        .Concat();
        }

        private IObservable<Unit> RequestUsername()
        {
            var getUsername = _sessionData.SessionCommunicator.Messages
                .FirstAsync()
                .Select(async m =>
                {
                    _sessionData.Username = m;
                    await UpdateSession();
                });

            _commandInProgress = true;
            return Observable.FromAsync(cancellation => SendMessage(MessageType.RequestUsername))
                .Select(r => getUsername)
                .Concat()
                .SelectAsync()
                .Finally(() => _commandInProgress = false)
                .SelectAsync(_ => SendMessage(MessageType.UsernameSpecified))
                .TakeUntil(_cancelSubject);
        }

        private IObservable<Unit> RequestSession()
        {
            var getSessionId = _sessionData.SessionCommunicator.Messages
                .FirstAsync()
                .SelectAsync(async m =>
                {
                    _sessionData.Session = Guid.Parse(m);
                    await UpdateSession();
                });

            _commandInProgress = true;
            var requestSession = Observable.FromAsync(cancellation => SendMessage(MessageType.RequestSessionKey));

            return requestSession.Select(r => getSessionId).Concat().Retry(5)
                .Finally(() => _commandInProgress = false)
                .SelectAsync(_ => SendMessage(MessageType.SessionKeySpecified))
                .TakeUntil(_cancelSubject);
        }

        private IObservable<Unit> RequestAuthToken()
        {
            var getAuthToken = _sessionData.SessionCommunicator.Messages
                .FirstAsync()
                .SelectAsync(async m =>
                {
                    _sessionData.Token = m;
                    await UpdateSession();
                });

            _commandInProgress = true;
            var requestToken = Observable.FromAsync(cancellation => SendMessage(MessageType.RequestToken));

            return requestToken.Select(r => getAuthToken).Concat()
                .Finally(() => _commandInProgress = false)
                .SelectAsync(_ => SendMessage(MessageType.TokenSpecified))
                .TakeUntil(_cancelSubject);
        }

        private StoredSession GetStoredSession() =>
            new StoredSession
            {
                SessionId = _sessionData.SessionId,
                Username = _sessionData.Username,
                Session = _sessionData.Session,
                Token = _sessionData.Token
            };

        private async Task SendMessage(MessageType messageType) =>
            await _sessionData.SessionCommunicator.SendMessage(messageType);

        private async Task UpdateSession()
        {
            await _chatsRepository.UpdateSession(GetStoredSession());
            _sessionData.IsAccountUpdated = true;
            _sessionSubject.OnNext(_sessionData);
        }
    }
}
