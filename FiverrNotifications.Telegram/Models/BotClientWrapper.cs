using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;

namespace FiverrNotifications.Telegram.Models
{
    public class BotClientWrapper : IDisposable
    {
        private bool _isReceiving;
        private readonly HashSet<UpdateType> _messageTypes;
        private readonly TelegramBotClient _botClient;

        public event EventHandler<ChosenInlineResultEventArgs> OnInlineResultChosen
        {
            add
            {
                StartReceiving(UpdateType.ChosenInlineResult);
                _botClient.OnInlineResultChosen += value;
            }
            remove
            {
                StopReceiving(UpdateType.ChosenInlineResult);
                _botClient.OnInlineResultChosen -= value;
            }
        }

        public event EventHandler<CallbackQueryEventArgs> OnCallbackQuery
        {
            add
            {
                StartReceiving(UpdateType.CallbackQuery);
                _botClient.OnCallbackQuery += value;
            }
            remove
            {
                StopReceiving(UpdateType.CallbackQuery);
                _botClient.OnCallbackQuery -= value;
            }
        }

        internal async Task<int> SendTextMessageAsync(long chatId, string message, bool disableWebPagePreview = false, bool disableNotification = false, IReplyMarkup replyMarkup = null)
        {
            var telegramMessage = await _botClient.SendTextMessageAsync(chatId, message, ParseMode.MarkdownV2, disableWebPagePreview: disableWebPagePreview, disableNotification: disableNotification, replyMarkup: replyMarkup);
            return telegramMessage.MessageId;
        }

        internal async Task DeleteMessageAsync(long chatId, int messageId)
        {
            await _botClient.DeleteMessageAsync(chatId, messageId);
        }

        public event EventHandler<MessageEventArgs> OnMessage
        {
            add
            {
                StartReceiving(UpdateType.Message);
                _botClient.OnMessage += value;
            }
            remove
            {
                StopReceiving(UpdateType.Message);
                _botClient.OnMessage -= value;
            }
        }

        public BotClientWrapper(TelegramBotClient botClient)
        {
            _messageTypes = new HashSet<UpdateType>();
            _botClient = botClient;
        }

        private void StartReceiving(UpdateType messageType)
        {
            if (_messageTypes.Contains(messageType)) 
                return;

            lock (_messageTypes)
            {
                if (!_messageTypes.Add(messageType))
                    return;

                if (_isReceiving)
                    _botClient.StopReceiving();

                _botClient.StartReceiving(_messageTypes.ToArray());
                _isReceiving = true;
            }
        }

        internal async Task<int> SendPhotoAsync(long chatId, InputOnlineFile inputOnlineFile, string text, bool disableNotification = false)
        {
            var telegramMessage = await _botClient.SendPhotoAsync(chatId, inputOnlineFile, text, ParseMode.MarkdownV2, disableNotification: disableNotification);
            return telegramMessage.MessageId;
        }

        private void StopReceiving(UpdateType messageType)
        {
            if (!_messageTypes.Contains(messageType))
                return;

            lock (_messageTypes)
            {
                if (!_messageTypes.Remove(messageType))
                    return;

                if (!_isReceiving)
                    return;

                _botClient.StopReceiving();
                if (_messageTypes.Count == 0)
                {
                    _isReceiving = false;
                    return;
                }

                _botClient.StartReceiving(_messageTypes.ToArray());
            }
        }

        public void Dispose()
        {
            if (_isReceiving)
                _botClient.StopReceiving();
        }
    }
}
