// Project: MapleLib
// File: DateTimeWithZone.cs
// Updated By: Jared
// 

using System;

namespace MapleLib.Structs
{
    public class DateTimeWithZone
    {
        private DateTime _utcDateTime;
        private TimeZoneInfo _timeZoneInfo;

        public DateTimeWithZone(TimeZoneInfo timeZoneInfo = null)
        {
            if (timeZoneInfo == null) {
                timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");
            }

            var dateTimeUnspec = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
            _utcDateTime = TimeZoneInfo.ConvertTimeToUtc(dateTimeUnspec, timeZoneInfo);
            _timeZoneInfo = timeZoneInfo;
        }

        public string TimeStamp()
        {
            return _utcDateTime.ToString("T");
        }
    }
}