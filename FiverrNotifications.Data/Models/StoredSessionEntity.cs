﻿using System;

namespace FiverrTelegramNotifications.Data.Models
{
    public class StoredSessionEntity
    {
        public int SessionId { get; set; }
        public long ChatId { get; set; }
        public int BotId { get; set; }
        public string ChatName { get; set; }
        public string FiverrUsername { get; set; }
        public Guid? FiverrSession { get; set; }
        public string FiverrToken { get; set; }
        public bool IsAuthRequested { get; set; }
        public bool IsPaused { get; set; }
        public bool IsMuted { get; set; }
        public DateTime? PauseFrom { get; set; }
        public DateTime? PauseTo { get; set; }
        public DateTime? MuteFrom { get; set; }
        public DateTime? MuteTo { get; set; }
        public string TimeZoneId { get; set; }
    }
}
