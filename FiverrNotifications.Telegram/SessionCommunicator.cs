using FiverrNotifications.Logic.Models;
using FiverrNotifications.Logic.Models.Messages;
using FiverrNotifications.Telegram.Models;
using System;
using System.Threading.Tasks;

namespace FiverrNotifications.Telegram
{
    public class SessionCommunicator : ISessionCommunicator
    {
        private readonly long _chatId;
        private readonly BotClientWrapper _botClient;
        private readonly MessageFactory _messageFactory;
        private readonly MessageSender _messageSender;

        public IObservable<string> Messages { get; }
        public IObservable<Location> LocationMessages { get; }

        public IObservable<string> Replies { get; }

        public SessionCommunicator(long chatId, BotClientWrapper botClient, IObservable<string> messages, IObservable<string> replies, IObservable<Location> locationMessages, MessageFactory messageFactory, MessageSender messageSender)
        {
            _chatId = chatId;
            _botClient = botClient;
            Messages = messages;
            Replies = replies;
            LocationMessages = locationMessages;
            _messageFactory = messageFactory;
            _messageSender = messageSender;
        }

        public async Task<int> SendMessage(FiverrRequest r, bool notify)
        {
            return await SendMessage(_messageFactory.GetRequestMessage(r), notify);
        }

        public async Task<int> SendMessage(StandardMessage messageType, bool notify)
        {
            return await SendMessageInternal(_messageFactory.GetStandardMessage(messageType), notify);
        }

        private async Task<int> SendMessage(string message, bool notify)
        {
            return await _messageSender.SendMessage(_botClient, _chatId, message, notify);
        }

        public async Task<int> SendMessage(TelegramMessage message, bool notify)
        {
            return await SendMessageInternal(message.Sanitize(_messageFactory.Sanitize), notify);
        }

        public async Task<int> SendMessage(StandardMessage messageType, bool notify, string[] arguments)
        {
            return await SendMessageInternal(_messageFactory.GetStandardMessage(messageType, arguments), notify);
        }

        public async Task DeleteMessage(int messageId)
        {
            await _botClient.DeleteMessageAsync(_chatId, messageId);
        }

        private async Task<int> SendMessageInternal(TelegramMessage message, bool notify)
        {
            return await _messageSender.SendMessage(_botClient, _chatId, message, notify);
        }
    }
}
