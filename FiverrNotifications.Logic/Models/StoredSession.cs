using FiverrNotifications.Logic.Models.Common;
using System;

namespace FiverrNotifications.Logic.Models
{
    public class StoredSession
    {
        public int SessionId { get; set; }
        public long ChatId { get; set; }
        public string Username { get; set; }
        public Guid? Session { get; set; }
        public string Token { get; set; }
        public int BotId { get; set; }
        public bool IsPaused { get; set; }
        public bool IsMuted { get; set; }
        public DateTimeRange PausePeriod { get; set; }
        public DateTimeRange MutePeriod { get; set; }
        public string TimeZoneId { get; set; }
    }
}
