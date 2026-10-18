using System;
namespace MaritimeLegend.Domain {
    [Serializable] public class GameCalendarState {
        public long elapsedGameDays,lastSettledDay;
        public double secondsIntoDay,dayDurationSeconds;
    }
    // Fictional calendar constants are format rules, not Earth dates or UTC production time.
    public static class GameCalendar {
        public const int StartYear=1700,DaysPerMonth=30,MonthsPerYear=12;
        public static string Date(long elapsedDays) {
            if(elapsedDays<0)throw new ArgumentOutOfRangeException(nameof(elapsedDays));
            return string.Format("{0:D4}-{1:D2}-{2:D2}",StartYear+elapsedDays/360,elapsedDays%360/30+1,elapsedDays%30+1);
        }
        public static void ValidateSeconds(double seconds) {
            if(double.IsNaN(seconds)||double.IsInfinity(seconds)||seconds<0)throw new ArgumentOutOfRangeException(nameof(seconds));
        }
    }
}
