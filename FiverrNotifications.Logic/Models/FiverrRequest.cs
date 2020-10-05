using System;
using System.Collections.Generic;

namespace FiverrNotifications.Logic.Models
{
    public class FiverrRequest
    {
        public string RequestId { get; set; }
        public string Buyer { get; set; }
        public DateTime? Date { get; set; }
        public string Request { get; set; }
        public string Duration { get; set; }
        public string Budget { get; set; }
        public IReadOnlyCollection<string> Tags { get; set; }
    }
}
