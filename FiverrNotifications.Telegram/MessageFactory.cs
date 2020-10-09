using FiverrNotifications.Logic.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FiverrNotifications.Telegram
{
    public class MessageFactory
    {
        private readonly MessageSanitizer _messageSanitizer;

        private readonly Dictionary<MessageType, TelegramMessage> _standatdMessages;

        public MessageFactory(MessageSanitizer messageSanitizer)
        {
            _messageSanitizer = messageSanitizer;

            _standatdMessages = new Dictionary<MessageType, TelegramMessage>
            {
                [MessageType.Help] = TelegramMessage.TextMessage(
@"Fiverr notifications bot allows you to receive notifications about new fiverr requests\.
To start it you must specify your username and copy paste your cookies\.
Bot checks your requests page every 1 minute and sends notification if new request has arrived\.
/help \- Help
/login \- Requests your [fiverr](https://fiverr.com) credentials
/username \- Sets your user account username
/session \- Fiverr session key
/token \- Fiverr session token
/start \- Starts bot
/stop \- Removes your account
/pause \- Pauses notifications
/resume \- Resumes receiving notifications
/cancel \- Cancels current action
Contact http://t\.me/esolCrusador for more information\."
),
                [MessageType.Started] = TelegramMessage.TextMessage("Started\\. Use /login to enter [fiverr](https://fiverr\\.com) credentials\\."),
                [MessageType.Stopped] = TelegramMessage.TextMessage("Stopped\\. Session data has been removed\\."),

                [MessageType.Paused] = TelegramMessage.TextMessage("Paused\\."),
                [MessageType.Resumed] = TelegramMessage.TextMessage("Resumed\\."),

                [MessageType.Muted] = TelegramMessage.TextMessage("Muted\\."),
                [MessageType.Unmuted] = TelegramMessage.TextMessage("Unmuted\\."),

                [MessageType.RequestUsername] = TelegramMessage.TextMessage("Please enter fiverr username\\."),
                [MessageType.UsernameSpecified] = TelegramMessage.TextMessage("Usernames was succesfuly specified\\."),

                [MessageType.RequestSessionKey] = TelegramMessage.PhotoMessage("Please enter \\_fiverr\\_session\\_key\\." +
                "\r\n Open Chrome, login to [fiverr](https://fiverr.com), press F12 or Fn\\+F12 to open console\\." +
                "\r\n1\\. Select *Application* tab\\.\r\n2\\. Select *Cookie* section\\.\r\n3\\. Select *fiverr* tab\\.\r\n4\\. Filter with *\\_fiverr\\_session\\_key*\\.\r\n5\\. Copy *Value* and send to Telegram\\.",
                "https://fiverr-notifications.azurewebsites.net/SessionKeyGuide.jpg"),
                [MessageType.SessionKeySpecified] = TelegramMessage.TextMessage("Session key was successfuly specified\\."),

                [MessageType.RequestToken] = TelegramMessage.TextMessage("Please enter hodor\\_creds\\." +
                "\r\n Open Chrome, login to [fiverr](https://fiverr.com), press F12, select *Application* tab, select *Cookie* section, filter by *hodor\\_creds*, copy *Value*, send it here\\."),
                [MessageType.TokenSpecified] = TelegramMessage.TextMessage("Auth Token was successfuly specified\\."),

                [MessageType.SuccessfullyConnected] = TelegramMessage.TextMessage("Successfully connected to feverr\\."),

                [MessageType.UnknownCommand] = TelegramMessage.TextMessage("Unknown command\\."),
                [MessageType.Cancelled] = TelegramMessage.TextMessage("Cancelled\\."),
                [MessageType.WrongCredentials] = TelegramMessage.TextMessage("Your credentials are wrong our expired\\. Please updare /username, /session, /token\\.")
            };
        }
        public string GetRequestMessage(FiverrRequest request) =>
            $"Request *{_messageSanitizer.EscapeString(request.Budget)}* for *{_messageSanitizer.EscapeString(request.Duration)}*\\." +
            (request.Tags.Count > 0 ? $"\r\n{string.Join(" ", request.Tags.Select(tag => _messageSanitizer.EscapeString($"{_messageSanitizer.EscapeString($"#{tag.Trim('#')}")}")))}" : string.Empty) +
            $"\r\nDescription:\r\n{_messageSanitizer.EscapeString(request.Request)}";

        public TelegramMessage GetStandardMessage(MessageType messageType)
        {
            if (!_standatdMessages.TryGetValue(messageType, out var message))
                throw new NotSupportedException($"Message type {nameof(MessageType)}.{messageType} is not supported");

            return message;
        }
    }
}
