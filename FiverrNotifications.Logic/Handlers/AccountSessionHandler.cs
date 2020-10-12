using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using FiverrNotifications.Logic.Helpers;
using FiverrNotifications.Logic.Models;
using FiverrNotifications.Logic.Models.Messages;
using FiverrNotifications.Logic.Repositories;
using Microsoft.Extensions.Logging;

namespace FiverrNotifications.Logic.Handlers
{
    public class AccountSessionHandler : IDisposable
    {
        private readonly SessionData _sessionData;
        private readonly ILogger<AccountSessionHandler> _logger;
        private readonly IChatsRepository _chatsRepository;
        private readonly Subscription _subscription;
        private readonly BehaviorSubject<SessionData> _sessionSubject;
        private readonly Subject<Unit> _cancelSubject;

        private bool _commandInProgress = false;

        private readonly Dictionary<string, Func<IObservable<Unit>>> _supportedMessages;

        public AccountSessionHandler(SessionData sessionData, ILogger<AccountSessionHandler> logger, IChatsRepository chatsRepository, SubscriptionFactory subscriptionFactory)
        {
            _sessionData = sessionData;
            _logger = logger;
            _chatsRepository = chatsRepository;
            _subscription = subscriptionFactory.Create();
            _sessionSubject = new BehaviorSubject<SessionData>(sessionData);
            _subscription.Add(_sessionSubject);
            SessionChanges = _sessionSubject;
            _cancelSubject = new Subject<Unit>();
            _subscription.Add(_cancelSubject);

            _supportedMessages = new Dictionary<string, Func<IObservable<Unit>>>(StringComparer.OrdinalIgnoreCase)
            {
                { "/start", () => Observable.Empty<Unit>() },
                { "/stop", () => Observable.Empty<Unit>() },
                { "/help", () => ShowHelp() },
                {"/login", () =>  Login()},
                {"/username", () => RequestUsername()},
                {"/session", () => RequestSession()},
                {"/token", () => RequestAuthToken()},
                { "/cancel", () => Cancel() },
                { "/pause", () => Pause() },
                { "/resume", () => Resume() },
                { "/mute", () => Mute() },
                { "/unmute", () => Unmute() },
                { "/timezone", () => RequestTimezone() },
            };
        }

        private IObservable<Unit> Pause()
        {
            return Observable.FromAsync(async () =>
            {
                _sessionData.IsPaused = true;
                await UpdateSession();
            }).SelectAsync(_ => SendMessage(StandardMessage.Paused));
        }

        public IObservable<Unit> Resume()
        {
            return Observable.FromAsync(async () =>
            {
                _sessionData.IsPaused = false;
                await UpdateSession();
            }).SelectAsync(_ => SendMessage(StandardMessage.Resumed));
        }

        public IObservable<Unit> Mute()
        {
            return Observable.FromAsync(async () =>
            {
                _sessionData.IsMuted = true;
                await UpdateSession();
            }).SelectAsync(_ => SendMessage(StandardMessage.Muted));
        }

        public IObservable<Unit> Unmute()
        {
            return Observable.FromAsync(async () =>
            {
                _sessionData.IsMuted = false;
                await UpdateSession();
            }).SelectAsync(_ => SendMessage(StandardMessage.Unmuted));
        }

        public void Initialize()
        {
            _subscription.Add(
                StartHandleMessages().LogException(_logger).Subscribe()
            );
        }

        public void Dispose() => _subscription.Dispose();

        public IObservable<SessionData> SessionChanges;

        private IObservable<Unit> ShowHelp()
        {
            return Observable.FromAsync(() => SendMessage(StandardMessage.Help));
        }

        private IObservable<Unit> Cancel()
        {
            return ObservableHelper.FromAction(() => _cancelSubject.OnNext(Unit.Default))
                .SelectAsync(_ => SendMessage(StandardMessage.Cancelled));
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
                            result = handler();
                        else
                            result = Observable.FromAsync(() => SendMessage(StandardMessage.UnknownCommand));

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

            return Observable.FromAsync(cancellation => SendMessage(StandardMessage.RequestUsername))
                .Do(_ => _commandInProgress = true)
                .Select(r => getUsername)
                .Concat()
                .SelectAsync()
                .Finally(() => _commandInProgress = false)
                .SelectAsync(_ => SendMessage(StandardMessage.UsernameSpecified))
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

            var requestSession = Observable.FromAsync(cancellation => SendMessage(StandardMessage.RequestSessionKey))
                .Do(_ => _commandInProgress = true);

            return requestSession.Select(r => getSessionId).Concat().Retry(5)
                .Finally(() => _commandInProgress = false)
                .SelectAsync(_ => SendMessage(StandardMessage.SessionKeySpecified))
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

            var requestToken = Observable.FromAsync(cancellation => SendMessage(StandardMessage.RequestToken))
                .Do(_ => _commandInProgress = true);

            return requestToken.Select(r => getAuthToken).Concat()
                .Finally(() => _commandInProgress = false)
                .SelectAsync(_ => SendMessage(StandardMessage.TokenSpecified))
                .TakeUntil(_cancelSubject);
        }

        public IObservable<Unit> RequestTimezone()
        {
            IObservable<IReadOnlyCollection<string>> awaitingTimezone = null;
            awaitingTimezone = _sessionData.SessionCommunicator.Messages
                .FirstAsync()
                .Select(timeMessage =>
                {
                    var time = DateTime.Parse(timeMessage);
                    var utc = DateTime.UtcNow;
                    var offset = Math.Round((time - utc).TotalMinutes / 15) * 15;

                    var timezones = TimeZoneInfo.GetSystemTimeZones()
                        .Where(tz => tz.BaseUtcOffset.TotalMinutes == offset)
                        .Select(tz => tz.Id).ToList();

                    return (IReadOnlyCollection<string>)timezones;
                })
                .Catch<IReadOnlyCollection<string>, Exception>(ex =>
                {
                    return Observable.FromAsync(cancellation => SendMessage(StandardMessage.CouldNotParseTime))
                        .Select(_ => awaitingTimezone)
                        .Concat();
                });

            return Observable.FromAsync(cancellation => SendMessage(StandardMessage.LocationForTimezone))
              .Do(_ => _commandInProgress = true)
              .Select(_ => awaitingTimezone)
              .Concat()
              .Select(timezones =>
              {
                  IObservable<string> selectTimezone = null;
                  var tzRequestId = Guid.NewGuid().ToString();

                  selectTimezone = Observable.FromAsync(
                    () => _sessionData.SessionCommunicator.SendMessage(
                        new SelectOptionTelegramMessage("Selezt timezone please", timezones.Select(tz => new KeyValuePair<string, string>($"{tzRequestId}:{tz}", tz))), 
                        !_sessionData.IsMuted
                    )
                  ).Select(messageId =>
                      _sessionData.SessionCommunicator.Replies
                      .Where(r => r.StartsWith(tzRequestId))
                      .FirstAsync()
                      .Select(r => r.Substring(r.IndexOf(':') + 1))
                      .Select(message => timezones.First(tz => tz == message))
                      .Catch<string, Exception>(ex => selectTimezone)
                      .SelectAsync(async tz =>
                      {
                          await _sessionData.SessionCommunicator.DeleteMessage(messageId);
                          return tz;
                      })
                  )
                  .Concat();

                  return selectTimezone;
              })
              .Concat()
              .Finally(() => _commandInProgress = false)
              .SelectAsync(tz => SendMessage(StandardMessage.TimezoneSpecified, tz))
              .TakeUntil(_cancelSubject);
        }


        private StoredSession GetStoredSession() =>
            new StoredSession
            {
                SessionId = _sessionData.SessionId,
                Username = _sessionData.Username,
                Session = _sessionData.Session,
                Token = _sessionData.Token,
                IsPaused = _sessionData.IsPaused,
                IsMuted = _sessionData.IsMuted
            };

        private async Task SendMessage(StandardMessage messageType) =>
            await _sessionData.SessionCommunicator.SendMessage(messageType, !_sessionData.IsMuted);

        private async Task SendMessage(StandardMessage messageType, params string[] arguments) =>
            await _sessionData.SessionCommunicator.SendMessage(messageType, !_sessionData.IsMuted, arguments);

        private async Task UpdateSession(bool notify = true)
        {
            await _chatsRepository.UpdateSession(GetStoredSession());
            if (notify)
                _sessionData.IsAccountUpdated = true;

            _sessionSubject.OnNext(_sessionData);
        }
    }
}
