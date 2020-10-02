using FiverrNotifications.Logic.Models;

namespace FiverrNotifications.Telegram
{
    public class MessageFactory
    {
        private readonly MessageSanitizer _messageSanitizer;

        public MessageFactory(MessageSanitizer messageSanitizer) => _messageSanitizer = messageSanitizer;
        public string GetRequestMessage(FiverrRequest request) =>
            $"Request \\({request.Budget}\\) for \\({request.Duration}\\)\\.\r\nDescription:\r\n{_messageSanitizer.EscapeString(request.Request)}";
    }
}
