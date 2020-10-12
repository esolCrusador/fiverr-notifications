using FiverrNotifications.Telegram.Models;
using System;

namespace FiverrNotifications.Logic.Models.Messages
{
    public class RequestLocationTelegramMessage : TextTelegramMessage
    {
        public string RequestLocation { get; }

        public RequestLocationTelegramMessage(string text, string requestLocation) : base(text, TelegramMessageType.RequestLocation)
        {
            RequestLocation = requestLocation;
        }

        public override TelegramMessage Clone() => new RequestLocationTelegramMessage(Text, RequestLocation);
        public override TelegramMessage Format(string[] arguments) => throw new NotImplementedException();
    }
}
