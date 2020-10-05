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
        public bool IsAccountUpdated { get; set; }
        public ISessionCommunicator SessionCommunicator { get; set; }
    }
}
