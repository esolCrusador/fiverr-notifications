using System;

namespace FiverrNotifications.Logic.Models.Common
{
    public struct DateTimeRange
    {
        public DateTimeOffset? From { get; }
        public TimeSpan? FromTimeOfDay { get; }
        public DateTimeOffset? To { get; }
        public TimeSpan? ToTimeOfDay { get; }

        public DateTimeRange(DateTime? from, DateTime? to, string timeZoneId)
        {
            var timeZone = timeZoneId == null ? null : TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            if (from.HasValue)
                From = new DateTimeOffset(from.Value, timeZone.BaseUtcOffset);
            else
                From = null;
            FromTimeOfDay = From?.TimeOfDay;

            if (to.HasValue)
                To = new DateTimeOffset(to.Value, timeZone.BaseUtcOffset);
            else
                To = null;
            ToTimeOfDay = To?.TimeOfDay;
        }

        public bool IsInPeriod(DateTime? time = null)
        {
            if (!HasValue)
                return false;

            time = DateTime.SpecifyKind((time ?? DateTime.Now).ToUniversalTime(), DateTimeKind.Local);
            TimeSpan timeOfDay = time.Value.Add(From.Value.Offset).TimeOfDay;

            if (FromTimeOfDay < ToTimeOfDay)
                return FromTimeOfDay <= timeOfDay && timeOfDay <= ToTimeOfDay;
            else // Over night
                return ToTimeOfDay <= timeOfDay && timeOfDay <= FromTimeOfDay;
        }
        public bool HasValue => From.HasValue && To.HasValue;
        public DateTimeRangeValue Value => HasValue ? new DateTimeRangeValue(From.Value, To.Value) : DateTimeRangeValue.Default;
        public static readonly DateTimeRange Null = new DateTimeRange(null, null, null);

        public string ToTimeString(string timeZoneId)
        {
            if (!HasValue)
                throw new InvalidOperationException();

            var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            return $"{From.Value.DateTime.ToShortTimeString()} - {To.Value.DateTime.ToShortTimeString()}";
        }
    }

    public struct DateTimeRangeValue
    {
        public DateTimeOffset From { get; set; }
        public DateTimeOffset To { get; set; }

        public DateTimeRangeValue(DateTimeOffset from, DateTimeOffset to)
        {
            From = from;
            To = to;
        }

        public static readonly DateTimeRangeValue Default = new DateTimeRangeValue(default, default);
    }
}
