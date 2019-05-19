using Java.Util;

namespace AbnormalChecker.Utils
{
	public static class ScreenUtils
	{
		#region Keys

		public const string MonitoringLastStartTime = "last_monitor_start";

		public static readonly string UnlocksDayNumber = "unlocks_dow_";

		public static readonly string UnlocksToday = "unlocks_today";

		public static readonly string LastUnlockDay = "last_unlock_day";

		public static readonly string UnlocksNewNormalCount = "unlocks_nn_count";

		#endregion
		
		public static long GetMonitoringStartTime(Date dateTime)
		{
			Calendar c = Calendar.Instance;
			c.Time = dateTime;
			c.Set(CalendarField.HourOfDay, 0);
			c.Set(CalendarField.Minute, 0);
			c.Set(CalendarField.Second, 1);
			c.Set(CalendarField.Millisecond, 0);
			return c.TimeInMillis;
		}
	}
}