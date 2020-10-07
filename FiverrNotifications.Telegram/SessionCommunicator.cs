using FiverrNotifications.Logic.Models;
using System;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;

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

        public async Task SendMessage(FiverrRequest r, bool notify)
        {
            await SendMessage(_messageFactory.GetRequestMessage(r), notify);
        }

        public async Task SendMessage(MessageType messageType, bool notify)
        {
            await SendMessage(_messageFactory.GetStandardMessage(messageType), notify);
        }

        private async Task SendMessage(string message, bool notify)
        {
            await _botClient.SendTextMessageAsync(_chatId, message, ParseMode.MarkdownV2, disableNotification: !notify);
        }

        private async Task SendMessage(TelegramMessage message, bool notify)
        {
            switch (message.Type)
            {
                case TelegramMessageType.Text:
                    await _botClient.SendTextMessageAsync(_chatId, message.Text, ParseMode.MarkdownV2, disableWebPagePreview: true, disableNotification: !notify);
                    break;
                case TelegramMessageType.Photo:
                    await _botClient.SendPhotoAsync(_chatId, new InputOnlineFile(message.ImageUrl), message.Text, ParseMode.MarkdownV2, disableNotification: !notify);
                    break;
                default:
                    throw new NotSupportedException($"The {nameof(TelegramMessageType)}.{message.Type} is not supported.");
            }
            
        }
    }
}
