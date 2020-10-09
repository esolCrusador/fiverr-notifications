using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using FiverrNotifications.Logic.Helpers;
using FiverrNotifications.Logic.Models;
using FiverrNotifications.Logic.Repositories;
using FiverrNotifications.Logic.Services;
using Telegram.Bot;
using Telegram.Bot.Args;

namespace FiverrNotifications.Telegram
{
    public class TelegramAccountsHandler : IAccountsService
    {
        private readonly IChatsRepository _chatsRepository;
        private readonly BotClientFactory _botClientFactory;
        private readonly MessageFactory _messageFactory;
        private readonly IResourceResolver _resourceResolver;
        private readonly Subscription _subscriptions;

        private readonly Subject<SessionData> _sessionChanges;
        private IObservable<KeyValuePair<int, MessageEventArgs>> _messages;
        private Dictionary<int, TelegramBotClient> _clients;
        private IObservable<KeyValuePair<int, MessageEventArgs>> Messages => _messages ??= GetMessages();

        public TelegramAccountsHandler(SubscriptionFactory subscriptionFactory, IChatsRepository chatsRepository, BotClientFactory botClientFactory, MessageFactory messageFactory, IResourceResolver resourceResolver)
        {
            _subscriptions = subscriptionFactory.Create();
            _sessionChanges = new Subject<SessionData>();
            _subscriptions.Add(_sessionChanges);

            _chatsRepository = chatsRepository;
            _botClientFactory = botClientFactory;
            _messageFactory = messageFactory;
            this._resourceResolver = resourceResolver;
        }

        public async Task InitializeAsync()
        {
            var bots = await _chatsRepository.GetBots();
            _clients = bots.ToDictionary(b => b.BotId, b => _botClientFactory.GetClient(b));

            SubscribeOnAccounts();
        }

        public void Start()
        {
            foreach (var client in _clients.Values)
                client.StartReceiving();
        }

        public IObservable<SessionData> GetSessions()
        {
            return _chatsRepository.GetSessions(_clients.Keys).ToObservable()
                .SelectMany(sessions => sessions.Select(s => CreateSessionData(s)))
                .Concat(_sessionChanges);
        }

        private void SubscribeOnAccounts()
        {
            _subscriptions.Add(
                Messages.Where(m =>
                    m.Value.Message.NewChatMembers?.Any(cm => cm.Id == m.Key) == true
                    || m.Value.Message.Text == "/start"
                    )
                    .SelectAsync(m => AddChat(m.Key, m.Value.Message.Chat.Id))
                    .Where(sessionData => sessionData != null)
                    .SelectAsync(sessionData => sessionData.SessionCommunicator.SendMessage(MessageType.Started, true))
                    .Subscribe()
            );

            _subscriptions.Add(
                Messages.Where(m => m.Value.Message.Text == "/stop")
                    .SelectAsync(m => RemoveChat(m.Key, m.Value.Message.Chat.Id))
                    .SelectAsync(sessionData => sessionData.SessionCommunicator.SendMessage(MessageType.Stopped, true))
                    .Subscribe()
            );

            _subscriptions.Add(
                Messages.Where(m => m.Value.Message.LeftChatMember?.Id == m.Key)
                    .Select(m => RemoveChat(m.Key, m.Value.Message.Chat.Id))
                    .Subscribe()
            );
        }

        private async Task<SessionData> AddChat(int botId, long chatId)
        {
            var storedSession = await _chatsRepository.AddChat(botId, chatId);
            if (storedSession == null)
                return null;

            var sessionData = CreateSessionData(storedSession);

            _sessionChanges.OnNext(sessionData);
            return sessionData;
        }

        private async Task<SessionData> RemoveChat(int botId, long chatId)
        {
            var storedSession = await _chatsRepository.RemoveChat(botId, chatId);
            var sessionData = CreateSessionData(storedSession, true);

            _sessionChanges.OnNext(sessionData);
            return sessionData;
        }

        private SessionData CreateSessionData(StoredSession storedSession, bool isDeleted = false)
        {
            return new SessionData
            {
                SessionId = storedSession.SessionId,
                ChatId = storedSession.ChatId,
                Username = storedSession.Username,
                Session = storedSession.Session,
                Token = storedSession.Token,
                IsDeleted = isDeleted,
                SessionCommunicator = CreateSessionCommunicator(storedSession),
                IsPaused = storedSession.IsPaused,
                IsMuted = storedSession.IsMuted
            };
        }

        private IObservable<KeyValuePair<int, MessageEventArgs>> GetMessages()
        {
            return _clients.Select(c => GetMessages(c.Value)
                .Select(m =>
                {
                    Console.WriteLine($"Telegram Message: {Newtonsoft.Json.JsonConvert.SerializeObject(m.Message)}");
                    return m;
                })
                .Select(m => new KeyValuePair<int, MessageEventArgs>(c.Key, m)))
                .Merge();
        }

        private IObservable<MessageEventArgs> GetMessages(TelegramBotClient botClient)
        {
            return Observable.FromEventPattern<MessageEventArgs>(h => botClient.OnMessage += h, h => botClient.OnMessage -= h)
                .Select(eventPatter => eventPatter.EventArgs)
                .Publish()
                .RefCount();
        }

        private ISessionCommunicator CreateSessionCommunicator(StoredSession storedSession) =>
            CreateSessionCommunicator(storedSession.ChatId, storedSession.BotId);

        private ISessionCommunicator CreateSessionCommunicator(long chatId, int botId) =>
            new SessionCommunicator(
                chatId,
                _clients[botId],
                Messages.Where(m => m.Key == botId && m.Value.Message.Chat.Id == chatId)
                .Select(m => m.Value.Message.Text),
                _messageFactory,
                _resourceResolver
            );

        public void Dispose()
        {
            _subscriptions.Dispose();

            if (_clients != null)
                foreach (var client in _clients.Values)
                    client.StopReceiving();
        }
    }
}
