namespace nvxapp.server.data.Extensions
{

    public enum TimeRoundInterval { Min1 = 1, Min5 = 5, Min15 = 15, Min30 = 30, Min60 = 60 }

    public enum RoundDirection { Down, Up }

    public struct TimeRoundOptions
    {
        public TimeRoundInterval Interval { get; init; }
        public RoundDirection Direction { get; init; }
        public TimeRoundOptions(TimeRoundInterval interval, RoundDirection direction) { Interval = interval; Direction = direction; }
    }

    public static class Roundings
    {
        public static TimeSpan RoundTimeSpan(TimeSpan time, TimeRoundOptions options)
        {
            int interval = (int)options.Interval; 
            int totalMinutes = (int)time.TotalMinutes; 
            int lower = (totalMinutes / interval) * interval; 
            int upper = lower + interval; 
            return options.Direction switch { RoundDirection.Down => TimeSpan.FromMinutes(lower), RoundDirection.Up => TimeSpan.FromMinutes(upper), _ => time };
        }

        public static DateTime RoundDateTime(DateTime dt, TimeRoundOptions options)
        {
            TimeSpan rounded = RoundTimeSpan(dt.TimeOfDay, options);
            return dt.Date + rounded;
        }

    }


    //var dt = new DateTime(2024, 10, 12, 14, 46, 0); 
    //var opt5Down = new TimeRoundOptions(5, RoundDirection.Down); 
    //var opt5Up = new TimeRoundOptions(5, RoundDirection.Up); 
    //var r1 = RoundDateTime(dt, opt5Down); // 2024-10-12 14:45
    //var r2 = RoundDateTime(dt, opt5Up); // 2024-10-12 14:50

}
