using FiverrNotifications.Logic.Models;
using FiverrNotifications.Logic.Models.Messages;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FiverrNotifications.Telegram
{
    public class MessageFactory
    {
        private readonly MessageSanitizer _messageSanitizer;

        private readonly Dictionary<StandardMessage, TelegramMessage> _standatdMessages;

        public MessageFactory(MessageSanitizer messageSanitizer)
        {
            _messageSanitizer = messageSanitizer;

            _standatdMessages = new Dictionary<StandardMessage, TelegramMessage>
            {
                [StandardMessage.Help] = TelegramMessage.TextMessage(
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
                [StandardMessage.Started] = TelegramMessage.TextMessage("Started\\. Use /login to enter [fiverr](https://fiverr\\.com) credentials\\."),
                [StandardMessage.Stopped] = TelegramMessage.TextMessage("Stopped\\. Session data has been removed\\."),

                [StandardMessage.Paused] = TelegramMessage.TextMessage("Paused\\. Use /resume to resume\\."),
                [StandardMessage.Resumed] = TelegramMessage.TextMessage("Resumed\\."),
                [StandardMessage.NotPaused] = TelegramMessage.TextMessage("Not Paused\\."),

                [StandardMessage.Muted] = TelegramMessage.TextMessage("Muted\\. Use /unmute to unmute\\."),
                [StandardMessage.Unmuted] = TelegramMessage.TextMessage("Unmuted\\."),
                [StandardMessage.NotMuted] = TelegramMessage.TextMessage("Not Muted\\."),

                [StandardMessage.RequestUsername] = TelegramMessage.TextMessage("Please enter fiverr username\\."),
                [StandardMessage.UsernameSpecified] = TelegramMessage.TextMessage("Usernames was succesfuly specified\\."),

                [StandardMessage.RequestSessionKey] = TelegramMessage.PhotoMessage("Please enter \\_fiverr\\_session\\_key\\." +
                "\r\n Open Chrome, login to [fiverr](https://fiverr.com), press F12 or Fn\\+F12 to open console\\." +
                "\r\n1\\. Select *Application* tab\\.\r\n2\\. Select *Cookie* section\\.\r\n3\\. Select *fiverr* tab\\.\r\n4\\. Filter with *\\_fiverr\\_session\\_key*\\.\r\n5\\. Copy *Value* and send to Telegram\\.",
                "Images\\SessionKeyGuide.jpg"),
                [StandardMessage.SessionKeySpecified] = TelegramMessage.TextMessage("Session key was successfuly specified\\."),

                [StandardMessage.RequestToken] = TelegramMessage.PhotoMessage("Please enter \\_fiverr\\_session\\_key\\." +
                "\r\n Open Chrome, login to [fiverr](https://fiverr.com), press F12 or Fn\\+F12 to open console\\." +
                "\r\n1\\. Select *Application* tab\\.\r\n2\\. Select *Cookie* section\\.\r\n3\\. Select *fiverr* tab\\.\r\n4\\. Filter with *hodor\\_creds*\\.\r\n5\\. Copy *Value* and send to Telegram\\.",
                "Images\\SessionKeyGuide.jpg"),
                [StandardMessage.TokenSpecified] = TelegramMessage.TextMessage("Auth Token was successfuly specified\\."),

                [StandardMessage.SuccessfullyConnected] = TelegramMessage.TextMessage("Successfully connected to feverr\\."),

                [StandardMessage.UnknownCommand] = TelegramMessage.TextMessage("Unknown command\\."),
                [StandardMessage.Cancelled] = TelegramMessage.TextMessage("Cancelled\\."),
                [StandardMessage.WrongCredentials] = TelegramMessage.TextMessage("Your credentials are wrong our expired\\. Please updare /username, /session, /token\\."),

                [StandardMessage.LocationForTimezone] = TelegramMessage.TextMessage("Please provide your local time to calculate your location"),
                [StandardMessage.TimezoneSpecified] = TelegramMessage.TextMessage("Timezone \"{0}\" successfully specified\\."),
                [StandardMessage.CouldNotParseTime] = TelegramMessage.TextMessage("Could not parse time\\."),

                [StandardMessage.PausePeriodRemoved] = TelegramMessage.TextMessage("Pause period has been removed\\."),
                [StandardMessage.RequestPauseFrom] = TelegramMessage.TextMessage("Please enter pause start time\\."),
                [StandardMessage.RequestPauseTo] = TelegramMessage.TextMessage("Please enter pause end time\\."),
                [StandardMessage.PausePeriodSpecified] = TelegramMessage.TextMessage("Pause period has been specified\\."),

                [StandardMessage.MutePeriodRemoved] = TelegramMessage.TextMessage("Mute period has been removed\\."),
                [StandardMessage.RequestMuteFrom] = TelegramMessage.TextMessage("Please enter mute start time\\."),
                [StandardMessage.RequestMuteTo] = TelegramMessage.TextMessage("Please enter mute end time\\."),
                [StandardMessage.MutePeriodSpecified] = TelegramMessage.TextMessage("Mute period has been specified\\."),
            };
        }
        public string GetRequestMessage(FiverrRequest request) =>
            $"Request *{_messageSanitizer.EscapeString(request.Budget)}* for *{_messageSanitizer.EscapeString(request.Duration)}*\\." +
            (request.Tags.Count > 0 ? $"\r\n{string.Join(" ", request.Tags.Select(tag => $"{_messageSanitizer.EscapeString($"#{tag.Trim('#')}")}"))}" : string.Empty) +
            $"\r\nDescription:\r\n{_messageSanitizer.EscapeString(request.Request)}";

        public TelegramMessage GetStandardMessage(StandardMessage messageType)
        {
            if (!_standatdMessages.TryGetValue(messageType, out var message))
                throw new NotSupportedException($"Message type {nameof(StandardMessage)}.{messageType} is not supported");

            return message;
        }

        public TelegramMessage GetStandardMessage(StandardMessage messageType, string[] arguments)
        {
            return GetStandardMessage(messageType).Format(arguments.Select(arg => _messageSanitizer.EscapeString(arg)).ToArray());
        }

        public string Sanitize(string message)
        {
            return _messageSanitizer.EscapeString(message);
        }
    }
}
