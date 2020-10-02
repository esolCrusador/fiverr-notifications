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
    }
}
