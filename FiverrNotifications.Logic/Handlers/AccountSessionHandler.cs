using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using FiverrNotifications.Logic.Helpers;
using FiverrNotifications.Logic.Models;
using FiverrNotifications.Logic.Models.Common;
using FiverrNotifications.Logic.Models.Messages;
using FiverrNotifications.Logic.Repositories;
using FiverrNotifications.Telegram.Models;
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
                { "/pause", () => StartPauseDialog() },
                { "/resume", () => Resume() },
                { "/mute", () => StartMuteDialog() },
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
                if (_sessionData.IsPaused)
                {
                    _sessionData.IsPaused = false;
                    await UpdateSession();
                    await SendMessage(StandardMessage.Resumed);
                }
                else
                {
                    await SendMessage(StandardMessage.NotPaused);
                }
            });
        }

        public IObservable<Unit> StartMuteDialog()
        {
            return Observable.FromAsync(async () =>
                await _sessionData.SessionCommunicator.SendMessage(new TextTelegramMessage(
                        $"Is currently {(_sessionData.IsMuted ? "Muted" : "Not Muted")}. " +
                        (_sessionData.MutePeriod.HasValue
                        ? $"Mute period is: {_sessionData.MutePeriod.ToTimeString(_sessionData.TimeZoneId)}."
                        : $"Mute period is not set."
                        )
                    ),
                    !_sessionData.IsCurrentlyMuted
                )
            )
            .Select(_ =>
            {
                string operationId = Guid.NewGuid().ToString();
                return Observable.FromAsync(async () =>
                             {
                                 var commands = new List<KeyValuePair<string, string>>
                                 {
                                    new KeyValuePair<string, string>($"{operationId}:MuteNow", "Mute Now"),
                                    new KeyValuePair<string, string>($"{operationId}:SetPeriod", "Specify Period"),
                                 };
                                 if (_sessionData.MutePeriod.HasValue)
                                     commands.Add(new KeyValuePair<string, string>($"{operationId}:RemovePeriod", "Remove Period"));
                                 commands.Add(new KeyValuePair<string, string>($"{operationId}:Cancel", "Cancel"));

                                 return await _sessionData.SessionCommunicator.SendMessage(
                                     new SelectOptionTelegramMessage("Choose what to do", commands),
                                    !_sessionData.IsCurrentlyMuted
                                 );
                             })
                .Select(messageId =>
                    _sessionData.SessionCommunicator.Replies
                    .Where(r => r.StartsWith(operationId))
                    .Select(r => r.Substring(r.IndexOf(':') + 1))
                    .FirstAsync()
                    .FinallyAsync(() => _sessionData.SessionCommunicator.DeleteMessage(messageId))
                    .Select(command =>
                    {
                        switch (command)
                        {
                            case "MuteNow":
                                return Mute();
                            case "SetPeriod":
                                return ReqeustMutePeriod();
                            case "RemovePeriod":
                                return RemoveMutePeriod();
                            case "Cancel":
                                return Cancel();
                            default:
                                throw new NotSupportedException($"Command \"{command}\" is not supported.");
                        }
                    })
                    .Concat()
                    )
                .Concat();
            })
            .Concat()
            .TakeUntil(_cancelSubject);
        }

        public IObservable<Unit> StartPauseDialog()
        {
            return Observable.FromAsync(async () =>
                await _sessionData.SessionCommunicator.SendMessage(new TextTelegramMessage(
                        $"Is currently {(_sessionData.IsPaused ? "Paused" : "Not Paused")}. " +
                        (_sessionData.PausePeriod.HasValue
                        ? $"Pause period is: {_sessionData.PausePeriod.ToTimeString(_sessionData.TimeZoneId)}."
                        : $"Pause period is not set."
                        )
                    ),
                    !_sessionData.IsCurrentlyMuted
                )
            )
            .Select(_ =>
            {
                string operationId = Guid.NewGuid().ToString();
                return Observable.FromAsync(async () =>
                {
                    var commands = new List<KeyValuePair<string, string>>
                                    {
                                                new KeyValuePair<string, string>($"{operationId}:PauseNow", "Pause Now"),
                                                new KeyValuePair<string, string>($"{operationId}:SetPeriod", "Specify Period"),
                                    };
                    if (_sessionData.PausePeriod.HasValue)
                        commands.Add(new KeyValuePair<string, string>($"{operationId}:RemovePeriod", "Remove Period"));
                    commands.Add(new KeyValuePair<string, string>($"{operationId}:Cancel", "Cancel"));

                    return await _sessionData.SessionCommunicator.SendMessage(
                                    new SelectOptionTelegramMessage("Choose what to do", commands),
                                    !_sessionData.IsCurrentlyMuted
                                );
                })
                .Select(messageId =>
                    _sessionData.SessionCommunicator.Replies
                    .Where(r => r.StartsWith(operationId))
                    .Select(r => r.Substring(r.IndexOf(':') + 1))
                    .FirstAsync()
                    .FinallyAsync(() => _sessionData.SessionCommunicator.DeleteMessage(messageId))
                    .Select(command =>
                    {
                        switch (command)
                        {
                            case "PauseNow":
                                return Pause();
                            case "SetPeriod":
                                return ReqeustPausePeriod();
                            case "RemovePeriod":
                                return RemovePausePeriod();
                            case "Cancel":
                                return Cancel();
                            default:
                                throw new NotSupportedException($"Command \"{command}\" is not supported.");
                        }
                    })
                    .Concat()
                    )
                .Concat();
            })
            .Concat()
            .TakeUntil(_cancelSubject);
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
                if (_sessionData.IsMuted)
                {
                    _sessionData.IsMuted = false;
                    await UpdateSession();
                    await SendMessage(StandardMessage.Unmuted);
                }
                else
                {
                    await SendMessage(StandardMessage.NotMuted);
                }
            });
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
            return Observable.FromAsync(cancellation => SendMessage(StandardMessage.LocationForTimezone))
              .Do(_ => _commandInProgress = true)
              .Select(_ => RequestTime())
              .Concat()
              .Select(time =>
              {
                  var utc = DateTime.UtcNow;
                  var offset = Math.Round((time - utc).TotalMinutes / 15) * 15;

                  var timezones = TimeZoneInfo.GetSystemTimeZones()
                      .Where(tz => tz.BaseUtcOffset.TotalMinutes == offset)
                      .Select(tz => tz.Id).ToList();

                  return (IReadOnlyCollection<string>)timezones;
              })
              .Select(timezones =>
              {
                  IObservable<string> selectTimezone = null;
                  var operationId = Guid.NewGuid().ToString();

                  selectTimezone = Observable.FromAsync(
                    () => _sessionData.SessionCommunicator.SendMessage(
                        new SelectOptionTelegramMessage("Selezt timezone please", timezones.Select(tz => new KeyValuePair<string, string>($"{operationId}:{tz}", tz))),
                        !_sessionData.IsCurrentlyMuted
                    )
                  ).Select(messageId =>
                      _sessionData.SessionCommunicator.Replies
                      .Where(r => r.StartsWith(operationId))
                      .FirstAsync()
                      .Select(r => r.Substring(r.IndexOf(':') + 1))
                      .Select(message => timezones.First(tz => tz == message))
                      .Catch<string, Exception>(ex => selectTimezone)
                      .SelectAsync(async tz =>
                      {
                          _sessionData.TimeZoneId = tz;
                          await UpdateSession(false);
                          return tz;
                      })
                      .FinallyAsync(() => _sessionData.SessionCommunicator.DeleteMessage(messageId))
                  )
                  .Concat();

                  return selectTimezone;
              })
              .Concat()
              .Finally(() => _commandInProgress = false)
              .SelectAsync(tz => SendMessage(StandardMessage.TimezoneSpecified, tz))
              .TakeUntil(_cancelSubject);
        }

        public IObservable<Unit> RequestTimezoneIfNeeded()
        {
            if (_sessionData.TimeZoneId != null)
                return ObservableHelper.One();

            return RequestTimezone();
        }

        public IObservable<Unit> ReqeustPausePeriod()
        {
            return RequestTimezoneIfNeeded()
                .SelectAsync(async _ => await SendMessage(StandardMessage.RequestPauseFrom))
                .Do(_ => _commandInProgress = true)
                .Select(_ =>
                    RequestTime()
                    .SelectAsync(async startTime =>
                    {
                        await SendMessage(StandardMessage.RequestPauseTo);
                        return RequestTime().SelectAsync(async endTime =>
                        {
                            _sessionData.PausePeriod = new DateTimeRange(startTime, endTime, _sessionData.TimeZoneId);
                            await UpdateSession();
                            await SendMessage(StandardMessage.PausePeriodSpecified);
                        });
                    }).Concat()
                ).Concat()
                .Finally(() => _commandInProgress = false)
                .TakeUntil(_cancelSubject);
        }

        public IObservable<Unit> RemovePausePeriod()
        {
            return Observable.FromAsync(async () =>
            {
                _sessionData.PausePeriod = DateTimeRange.Null;
                await UpdateSession();

                await SendMessage(StandardMessage.PausePeriodRemoved);
            });
        }

        public IObservable<Unit> ReqeustMutePeriod()
        {
            return RequestTimezoneIfNeeded()
                .SelectAsync(async _ => await SendMessage(StandardMessage.RequestMuteFrom))
                .Do(_ => _commandInProgress = true)
                .Select(_ =>
                    RequestTime()
                    .SelectAsync(async startTime =>
                    {
                        await SendMessage(StandardMessage.RequestMuteTo);
                        return RequestTime().SelectAsync(async endTime =>
                        {
                            _sessionData.MutePeriod = new DateTimeRange(startTime, endTime, _sessionData.TimeZoneId);
                            await UpdateSession();
                            await SendMessage(StandardMessage.MutePeriodSpecified);
                        });
                    }).Concat()
                ).Concat()
                .Finally(() => _commandInProgress = false)
                .TakeUntil(_cancelSubject);
        }

        public IObservable<Unit> RemoveMutePeriod()
        {
            return Observable.FromAsync(async () =>
            {
                _sessionData.MutePeriod = DateTimeRange.Null;
                await UpdateSession();

                await SendMessage(StandardMessage.MutePeriodRemoved);
            });
        }

        private IObservable<DateTime> RequestTime()
        {
            IObservable<DateTime> awaitingTime = null;

            awaitingTime = _sessionData.SessionCommunicator.Messages
                .FirstAsync()
                .Select(timeMessage => DateTime.Parse(timeMessage))
                .Catch<DateTime, Exception>(ex =>
                {
                    return Observable.FromAsync(cancellation => SendMessage(StandardMessage.CouldNotParseTime))
                        .Select(_ => awaitingTime)
                        .Concat();
                });

            return awaitingTime;
        }

        private StoredSession GetStoredSession() =>
            new StoredSession
            {
                SessionId = _sessionData.SessionId,
                Username = _sessionData.Username,
                Session = _sessionData.Session,
                Token = _sessionData.Token,
                IsPaused = _sessionData.IsPaused,
                IsMuted = _sessionData.IsCurrentlyMuted,
                MutePeriod = _sessionData.MutePeriod,
                PausePeriod = _sessionData.PausePeriod,
                TimeZoneId = _sessionData.TimeZoneId
            };

        private async Task SendMessage(StandardMessage messageType) =>
            await _sessionData.SessionCommunicator.SendMessage(messageType, !_sessionData.IsCurrentlyMuted);

        private async Task SendMessage(StandardMessage messageType, params string[] arguments) =>
            await _sessionData.SessionCommunicator.SendMessage(messageType, !_sessionData.IsCurrentlyMuted, arguments);

        private async Task UpdateSession(bool notify = true)
        {
            await _chatsRepository.UpdateSession(GetStoredSession());
            if (notify)
                _sessionData.IsAccountUpdated = true;

            _sessionSubject.OnNext(_sessionData);
        }
    }
}
