using FiverrNotifications.Telegram.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FiverrNotifications.Logic.Models.Messages
{
    public class SelectOptionTelegramMessage : TextTelegramMessage
    {
        public IReadOnlyCollection<KeyValuePair<string, string>> Options { get; }
        public SelectOptionTelegramMessage(string text, IEnumerable<KeyValuePair<string, string>> options) : base(text, TelegramMessageType.SelectOption)
        {
            Options = options as IReadOnlyCollection<KeyValuePair<string, string>> ?? options.ToArray();
        }

        public override TelegramMessage Clone() => new SelectOptionTelegramMessage(Text, Options);
        public override TelegramMessage Format(string[] arguments) => throw new NotImplementedException();
        public override TelegramMessage Sanitize(Func<string, string> escape) => new SelectOptionTelegramMessage(escape(Text), Options.Select(kvp => new KeyValuePair<string, string>(kvp.Key, escape(kvp.Value))));
    }
}
