using System;

namespace FiverrNotifications.Telegram.Models
{
    public class TelegramMessageException: Exception
    {
        public TelegramMessageException(string message, Exception innerException)
            :base(message, innerException)
        {
        }
    }
}
