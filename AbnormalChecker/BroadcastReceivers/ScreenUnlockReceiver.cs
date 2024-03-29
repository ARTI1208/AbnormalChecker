using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AbnormalChecker.Activities;
using AbnormalChecker.Extensions;
using AbnormalChecker.OtherUI;
using AbnormalChecker.Utils;
using Android.App;
using Android.Content;
using Android.Preferences;
using Android.Util;
using Java.Util;
using Java.Util.Concurrent;

namespace AbnormalChecker.BroadcastReceivers
{
	[BroadcastReceiver]
	[IntentFilter(new[]
	{
		Intent.ActionScreenOn
	})]
	public class ScreenUnlockReceiver : BroadcastReceiver
	{
		public enum ScreenStatus
		{
			OK,
			Many,
			Speed
		}

		private const string AbnormalUnlocksTimeInterval = "unlocks_time_interval";
		private static ScreenUnlockReceiver _unlockReceiver;

		private static ISharedPreferences _mPreferences;

		private static readonly int DefaultUnlocksCount = 10;

		private static int _lastDayUnlocked = -1;

		public static ScreenStatus Status = ScreenStatus.OK;

		public static string DebugFile = "screen_unlocks_debug_file.txt";

		public static int NormalCount { get; private set; } = -1;

		public static int UnlockedTimes { get; set; }

		private static int GetMonitoringDayCount()
		{
			return _mPreferences.GetBoolean(SettingsActivity.ScreenLockAutoAdjustmentType, true) ? 7 : 1;
		}

		public static void SetToZero()
		{
			NormalCount = -1;
			UnlockedTimes = 0;
			Status = ScreenStatus.OK;
			_lastDayUnlocked = -1;
		}

		private static int GetNormalUnlocksCount(ISharedPreferences preferences)
		{
			if (GetMonitoringDayCount() == 7 &&
			    preferences.Contains($"{ScreenUtils.UnlocksDayNumber}{_lastDayUnlocked}"))
			{
				NormalCount = preferences.GetInt($"{ScreenUtils.UnlocksDayNumber}{_lastDayUnlocked}",
					DefaultUnlocksCount);
			}
			else
			{
				var s = 0;
				var k = 0;
				for (var i = 1; i <= 7; i++)
				{
					var un = preferences.GetInt($"{ScreenUtils.UnlocksDayNumber}{i}", -1);
					if (un > 0)
					{
						s += un;
						k++;
					}
				}

				NormalCount = k > 0 ? (int) Math.Ceiling((double) s / k) : DefaultUnlocksCount;
			}

			NormalCount = Math.Max(NormalCount, preferences.GetInt(ScreenUtils.UnlocksNewNormalCount, NormalCount));
			return NormalCount;
		}

		public override void OnReceive(Context context, Intent intent)
		{
			Log.Debug(nameof(ScreenUnlockReceiver), intent.Action);

			var notificationSender = new NotificationSender(context, DataHolder.ScreenLocksCategory);
			if (_mPreferences == null) _mPreferences = PreferenceManager.GetDefaultSharedPreferences(context);

			var now = new Date();

			long monitoringStartTime;
			if ((monitoringStartTime = _mPreferences.GetLong(ScreenUtils.MonitoringLastStartTime, -1)) == -1)
			{
				monitoringStartTime = ScreenUtils.GetMonitoringStartTime(now);
				_mPreferences.Edit().PutLong(ScreenUtils.MonitoringLastStartTime, monitoringStartTime).Apply();
				using (var writer = new StreamWriter(
					context.OpenFileOutput(DebugFile, FileCreationMode.Append)))
				{
					writer.WriteLine($"----Monitoring u start Time: {now.GetFormattedDateTime()}----");
				}
			}
			else
			{
				monitoringStartTime = ScreenUtils.GetMonitoringStartTime(new Date(monitoringStartTime));
			}

			if (_lastDayUnlocked == -1)
			{
				int tmpDay;
				if ((tmpDay = _mPreferences.GetInt(ScreenUtils.LastUnlockDay, -1)) != -1)
				{
					_lastDayUnlocked = tmpDay;
					UnlockedTimes = _mPreferences.GetInt(ScreenUtils.UnlocksToday, 0);
				}
				else
				{
					_lastDayUnlocked = Calendar.Instance.Get(CalendarField.DayOfWeek);
					_mPreferences.Edit().PutInt(ScreenUtils.LastUnlockDay, _lastDayUnlocked).Apply();
				}
			}

			if (_unlockMillis == null)
			{
				var thisDay = _mPreferences.GetInt($"{ScreenUtils.UnlocksDayNumber}{_lastDayUnlocked}", 0);
				_todayNormalSpeedValue = Math.Max(
					_abnormalUnlockMinCount,
					(int) (thisDay * _abnormalUnlockPercentage / 100d));
				_unlockMillis = new List<long>();
			}
			
			if (TimeUnit.Milliseconds.ToDays(now.Time - monitoringStartTime) >= 1)
			{
				var unlocksToPut = UnlockedTimes;
				int u;

				if ((u = _mPreferences.GetInt($"{ScreenUtils.UnlocksDayNumber}{_lastDayUnlocked}", -1)) != -1)
					unlocksToPut = (int) Math.Ceiling((u + unlocksToPut) / 2d);

				var tmpDay = _lastDayUnlocked;
				_lastDayUnlocked = Calendar.Instance.Get(CalendarField.DayOfWeek);
				UnlockedTimes = 1;

				var thisDay = _mPreferences.GetInt($"{ScreenUtils.UnlocksDayNumber}{_lastDayUnlocked}", 0);
				_todayNormalSpeedValue = Math.Max(
					_abnormalUnlockMinCount,
					(int) (thisDay * _abnormalUnlockPercentage / 100d));
				_unlockMillis = new List<long>();

				_mPreferences.Edit()
					.PutInt($"{ScreenUtils.UnlocksDayNumber}{tmpDay}", unlocksToPut)
					.PutInt(ScreenUtils.LastUnlockDay, _lastDayUnlocked)
					.PutLong(ScreenUtils.MonitoringLastStartTime, ScreenUtils.GetMonitoringStartTime(now))
					.PutInt(ScreenUtils.UnlocksToday, UnlockedTimes)
					.PutInt(ScreenUtils.UnlocksNewNormalCount, -1)
					.Apply();

				MainActivity.Adapter?.Refresh();
				return;
			}

			Log.Debug("AbScreenReceiver",
				$"Not new day. Was {new Date(monitoringStartTime).GetFormattedDateTime()}, now {now.GetFormattedDateTime()}");

			_mPreferences.Edit().PutInt(ScreenUtils.UnlocksToday, ++UnlockedTimes).Apply();

			var mode = 0;

			if (UnlockedTimes > GetNormalUnlocksCount(_mPreferences) * 1.1)
				mode = 1;

			_abnormalUnlockInterval = _mPreferences.GetInt(AbnormalUnlocksTimeInterval, _abnormalUnlockInterval);

			if (_unlockMillis.Count > _todayNormalSpeedValue)
			{
				var s = TimeUnit.Milliseconds.ToSeconds(now.Time - _unlockMillis[0]);

				if (s <= _abnormalUnlockInterval && s > 0)
				{
					mode = 2;
					using (var writer = new StreamWriter(
						context.OpenFileOutput(DebugFile, FileCreationMode.Append)))
					{
						writer.WriteLine($"----Time: {new Date().GetFormattedDateTime()}----");
						foreach (var unlock in _unlockMillis)
							writer.WriteLine($"Unlock at {new Date(unlock).GetFormattedDateTime()}, val = {unlock}");

						writer.WriteLine("-----End1-----");
					}
				}
				else
				{
					var tmpList = new List<long>();
					long sec;
					foreach (var time in _unlockMillis)
					{
						sec = TimeUnit.Milliseconds.ToSeconds(now.Time - time);
						if (sec <= _abnormalUnlockInterval && sec > 0) tmpList.Add(time);
					}

					_unlockMillis = tmpList;
					sec = TimeUnit.Milliseconds.ToSeconds(now.Time - _unlockMillis[0]);
					if (_unlockMillis.Count > _todayNormalSpeedValue && sec <= _abnormalUnlockInterval && sec > 0)
					{
						mode = 2;
						using (var writer = new StreamWriter(
							context.OpenFileOutput(DebugFile, FileCreationMode.Append)))
						{
							writer.WriteLine($"----Time: {new Date().GetFormattedDateTime()}----");
							foreach (var unlock in _unlockMillis)
								writer.WriteLine(
									$"Unlock at {new Date(unlock).GetFormattedDateTime()}, val = {unlock}");

							writer.WriteLine("-----End2-----");
						}
					}
				}
			}

			_unlockMillis.Add(now.Time);

			string notificationText;

			switch (mode)
			{
				case 1:
					Status = ScreenStatus.Many;
					notificationText =
						string.Format(context.GetString(Resource.String.category_screen_notif_daily_overflow),
							UnlockedTimes, NormalCount);
					notificationSender.PutNormalizeExtra(ScreenUtils.UnlocksNewNormalCount,
						(int) (UnlockedTimes * 1.2));

					CategoriesAdapter.Refresh(DataHolder.ScreenCategory);
					break;
				case 2:
					var tmpInterval = (int) (.9 * _abnormalUnlockInterval);
					if (tmpInterval >= _abnormalUnlockInterval)
						if (tmpInterval > 1)
							tmpInterval--;

					Status = ScreenStatus.Speed;
					notificationText =
						string.Format(context.GetString(Resource.String.category_screen_notif_west_fast_hand),
							_unlockMillis.Count,
							TimeUnit.Milliseconds.ToSeconds(_unlockMillis.Last() - _unlockMillis.First()));

					notificationSender.PutNormalizeExtra(AbnormalUnlocksTimeInterval, tmpInterval);
					CategoriesAdapter.Refresh(DataHolder.ScreenCategory);
					break;
				default:
					Status = ScreenStatus.OK;
					CategoriesAdapter.Refresh(DataHolder.ScreenCategory);
					return;
			}

			using (var writer =
				new StreamWriter(context.OpenFileOutput(AlarmReceiver.CurrentSummaryFile, FileCreationMode.Append)))
			{
				writer.WriteLine(notificationText);
			}

			notificationSender.Send(NotificationType.WarningNotification, notificationText);
		}

		public static void SetUnlockReceiverStatus(Context context, bool enable)
		{
			if (enable && _unlockReceiver == null)
			{
				var screenStateFilter = new IntentFilter();
				screenStateFilter.AddAction(Intent.ActionScreenOn);
				_unlockReceiver = new ScreenUnlockReceiver();
				context.ApplicationContext.RegisterReceiver(_unlockReceiver, screenStateFilter);
			}
			else if (!enable && _unlockReceiver != null)
			{
				context.ApplicationContext.UnregisterReceiver(_unlockReceiver);
				_unlockReceiver.UnregisterFromRuntime();
				_unlockReceiver.Dispose();
				_unlockReceiver = null;
			}
		}

		public static void UpdateStatus()
		{
			Status = ScreenStatus.OK;
			_abnormalUnlockInterval = _mPreferences.GetInt(AbnormalUnlocksTimeInterval, _abnormalUnlockInterval);

			if (_unlockMillis.Count > _todayNormalSpeedValue)
			{
				var tmpList = new List<long>();
				var now = new Date();
				foreach (var time in _unlockMillis)
				{
					var s = TimeUnit.Milliseconds.ToSeconds(now.Time - time);

					if (s <= _abnormalUnlockInterval && s > 0) tmpList.Add(time);
				}

				_unlockMillis = tmpList;
				if (_unlockMillis.Count > _todayNormalSpeedValue &&
				    TimeUnit.Milliseconds.ToSeconds(_unlockMillis.Last() - _unlockMillis[0]) <= _abnormalUnlockInterval)
					Status = ScreenStatus.Speed;
			}

			if (UnlockedTimes > GetNormalUnlocksCount(_mPreferences) * 1.1) Status = ScreenStatus.Many;
		}

		#region UnlockSpeed

		private static readonly int _abnormalUnlockMinCount = 3;

		private static readonly int _abnormalUnlockPercentage = 20;

		private static int _abnormalUnlockInterval = 10;

		public static List<long> _unlockMillis;

		private static int _todayNormalSpeedValue;

		#endregion
	}
}