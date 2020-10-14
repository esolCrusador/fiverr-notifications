using FiverrNotifications.Logic.Models.Messages;
using System;
using System.Collections.Generic;

namespace FiverrNotifications.Telegram.Models
{
    public class TextTelegramMessage : TelegramMessage
    {
        public string Text { get; }
        public TextTelegramMessage(string text) : this(text, TelegramMessageType.Text)
        {
            Text = text;
        }
        protected TextTelegramMessage(string text, TelegramMessageType messageType) : base(messageType)
        {
            Text = text;
        }

        public override TelegramMessage Clone() => new TextTelegramMessage(Text);
        public override TelegramMessage Format(string[] arguments) => new TextTelegramMessage(string.Format(Text, arguments));
        public override TelegramMessage Sanitize(Func<string, string> escape) => new TextTelegramMessage(escape(Text));
    }
}
