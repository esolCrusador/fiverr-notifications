using FiverrNotifications.Logic.Models;
using System;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace FiverrNotifications.Telegram
{
    public class SessionCommunicator: ISessionCommunicator
    {
        private readonly long _chatId;
        private readonly TelegramBotClient _botClient;
        private readonly MessageFactory _messageFactory;

        public IObservable<string> Messages { get; }

        public SessionCommunicator(long chatId, TelegramBotClient botClient, IObservable<string> messages, MessageFactory messageFactory)
        {
            _chatId = chatId;
            _botClient = botClient;
            Messages = messages;
            _messageFactory = messageFactory;
        }

        public async Task SendMessage(string message)
        {
            await _botClient.SendTextMessageAsync(_chatId, message, ParseMode.MarkdownV2);
        }

        public async Task SendMessage(FiverrRequest r)
        {
            await SendMessage(_messageFactory.GetRequestMessage(r));
        }
    }
}
