using FiverrNotifications.Logic.Models.Common;
using System;

namespace FiverrNotifications.Logic.Models
{
    public class SessionData
    {
        public int SessionId { get; set; }
        public long ChatId { get; set; }
        public string Username { get; set; }
        public Guid? Session { get; set; }
        public string Token { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsPaused { get; set; }
        public bool IsCurrentlyPaused => IsPaused || PausePeriod.IsInPeriod();
        public bool IsMuted { get; set; }
        public bool IsCurrentlyMuted => IsMuted || MutePeriod.IsInPeriod();
        public bool IsAccountUpdated { get; set; }
        public DateTimeRange PausePeriod { get; set; }
        public DateTimeRange MutePeriod { get; set; }
        public string TimeZoneId { get; set; }
        public ISessionCommunicator SessionCommunicator { get; set; }
    }
}
