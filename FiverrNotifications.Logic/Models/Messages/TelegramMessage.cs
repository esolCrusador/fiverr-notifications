using FiverrNotifications.Telegram.Models;
using System.Collections.Generic;

namespace FiverrNotifications.Logic.Models.Messages
{
    public abstract class TelegramMessage
    {
        public TelegramMessageType Type { get; }
        public bool DisableWebPagePreview { get; set; } = true;

        protected TelegramMessage(TelegramMessageType type) => Type = type;

        public static TelegramMessage TextMessage(string message) => new TextTelegramMessage(message);
        public static TelegramMessage PhotoMessage(string message, string imageUrl) => new PhotoTelegramMessage(message, imageUrl);
        public static TelegramMessage RequestLocation(string message, string requestLocation) => new RequestLocationTelegramMessage(message, requestLocation);
        public static TelegramMessage SelectOption(string message, IReadOnlyCollection<KeyValuePair<string, string>> options) => new SelectOptionTelegramMessage(message, options);

        public abstract TelegramMessage Clone();
        public abstract TelegramMessage Format(string[] arguments);
    }
}
