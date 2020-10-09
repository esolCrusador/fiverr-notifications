namespace FiverrNotifications.Logic.Models
{
    public class SessionStatistics
    {
        public int IsLoggedIn { get; set; }
        public int IsPaused { get; set; }
        public int IsMuted { get; set; }
        public int NotificationsCount { get; set; }
    }
}
