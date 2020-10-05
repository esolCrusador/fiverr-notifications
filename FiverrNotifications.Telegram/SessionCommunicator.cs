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

        public async Task SendMessage(FiverrRequest r)
        {
            await SendMessage(_messageFactory.GetRequestMessage(r));
        }

        public async Task SendMessage(MessageType messageType)
        {
            await SendMessage(_messageFactory.GetStandardMessage(messageType));
        }

        private async Task SendMessage(string message)
        {
            await _botClient.SendTextMessageAsync(_chatId, message, ParseMode.MarkdownV2);
        }

        private async Task SendMessage(TelegramMessage message)
        {
            switch (message.Type)
            {
                case TelegramMessageType.Text:
                    await _botClient.SendTextMessageAsync(_chatId, message.Text, ParseMode.MarkdownV2, disableWebPagePreview: true);
                    break;
                case TelegramMessageType.Photo:
                    await _botClient.SendPhotoAsync(_chatId, new InputOnlineFile(message.ImageUrl), message.Text, ParseMode.MarkdownV2);
                    break;
                default:
                    throw new NotSupportedException($"The {nameof(TelegramMessageType)}.{message.Type} is not supported.");
            }
            
        }
    }
}
