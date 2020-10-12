using FiverrNotifications.Logic.Models.Messages;
using FiverrNotifications.Telegram.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;

namespace FiverrNotifications.Telegram
{
    public class MessageSender
    {
        private readonly IResourceResolver _resourceResolver;

        public MessageSender(IResourceResolver resourceResolver)
        {
            _resourceResolver = resourceResolver;
        }

        public async Task<int> SendMessage(BotClientWrapper botClient, long chatId, string message, bool notify)
        {
            return await botClient.SendTextMessageAsync(chatId, message, !notify);
        }

        public Task<int> SendMessage(BotClientWrapper botClient, long chatId, TelegramMessage message, bool notify)
        {
            switch (message.Type)
            {
                case TelegramMessageType.Text:
                    return SendMessage(botClient, chatId, (TextTelegramMessage)message, notify);
                case TelegramMessageType.Photo:
                    return SendMessage(botClient, chatId, (PhotoTelegramMessage)message, notify);
                case TelegramMessageType.RequestLocation:
                    return SendMessage(botClient, chatId, (RequestLocationTelegramMessage)message, notify);
                case TelegramMessageType.SelectOption:
                    return SendMessage(botClient, chatId, (SelectOptionTelegramMessage)message, notify);
                default:
                    throw new NotSupportedException($"{nameof(TelegramMessageType)}.{message.Type} is not supported");
            }
        }

        private async Task<int> SendMessage(BotClientWrapper botClient, long chatId, TextTelegramMessage message, bool notify)
        {
            return await botClient.SendTextMessageAsync(chatId, message.Text, disableWebPagePreview: message.DisableWebPagePreview, disableNotification: !notify);
        }

        private async Task<int> SendMessage(BotClientWrapper botClient, long chatId, PhotoTelegramMessage message, bool notify)
        {
            return await botClient.SendPhotoAsync(chatId, new InputOnlineFile(_resourceResolver.GetResourceStream(message.ImageUrl)), message.Text, disableNotification: !notify);
        }


        private async Task<int> SendMessage(BotClientWrapper botClient, long chatId, RequestLocationTelegramMessage message, bool notify)
        {
            return await botClient.SendTextMessageAsync(chatId, message.Text, disableWebPagePreview: message.DisableWebPagePreview, disableNotification: !notify, replyMarkup: new ReplyKeyboardMarkup(KeyboardButton.WithRequestLocation(message.RequestLocation)));
        }

        private async Task<int> SendMessage(BotClientWrapper botClient, long chatId, SelectOptionTelegramMessage message, bool notify)
        {
            return await botClient.SendTextMessageAsync(
                chatId,
                message.Text,
                disableWebPagePreview: message.DisableWebPagePreview,
                disableNotification: !notify,
                replyMarkup: new InlineKeyboardMarkup(
                    message.Options.Select(o => new InlineKeyboardButton { Text = o.Value, CallbackData = o.Key })
                    .Select((b, idx) => new KeyValuePair<int, InlineKeyboardButton>(idx, b))
                    .GroupBy(kvp => kvp.Key, kvp => kvp.Value)
                    .Select(g => g.AsEnumerable())
                    )
                );
        }

    }
}
