using FiverrNotifications.Telegram.Models;
using System;

namespace FiverrNotifications.Logic.Models.Messages
{
    public class PhotoTelegramMessage : TextTelegramMessage
    {
        public string ImageUrl { get; }
        public PhotoTelegramMessage(string text, string imageUrl) : base(text, TelegramMessageType.Photo)
        {
            ImageUrl = imageUrl;
        }

        public override TelegramMessage Clone() => new PhotoTelegramMessage(Text, ImageUrl);
        public override TelegramMessage Format(string[] arguments) => throw new NotImplementedException();
    }
}
