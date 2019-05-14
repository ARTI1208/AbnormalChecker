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
using Android.Gms.Common.Stats;
using Android.OS;
using Android.Preferences;
using Android.Util;
using Android.Widget;
using Java.IO;
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
		private static ScreenUnlockReceiver _unlockReceiver;

		private static ISharedPreferences _mPreferences;

		private readonly string AbnormalUnlocksTimeInterval = "unlocks_time_interval";

		#region UnlockSpeed

		private static int _abnormalUnlockMinCount = 3;

		private static int _abnormalUnlockPercentage = 20;

		private static int _abnormalUnlockInterval = 10;

		private static List<long> _unlockMillis;

		private static int _todayNormalSpeedValue;

		#endregion

		private static readonly int DefaultUnlocksCount = 10;

		private static int _lastDayUnlocked = -1;

		public static bool IsNormal = true;

		public static int NormalCount { get; private set; } = -1;

		public static int UnlockedTimes { get; set; }

		public static string DebugFile = "screen_unlocks_debug_file.txt";

		private static int GetMonitoringDayCount()
		{
			return _mPreferences.GetBoolean(SettingsActivity.ScreenLockAutoAdjustmentType, true) ? 7 : 1;
		}

		public static void SetToZero()
		{
			NormalCount = -1;
			UnlockedTimes = 0;
			IsNormal = true;
			_lastDayUnlocked = -1;
		}

		private int GetNormalUnlocksCount(ISharedPreferences preferences)
		{
			if (GetMonitoringDayCount() == 7 &&
			    preferences.Contains($"{ScreenUtils.UnlocksDayNumber}{_lastDayUnlocked}"))
			{
				NormalCount = preferences.GetInt($"{ScreenUtils.UnlocksDayNumber}{_lastDayUnlocked}",
					DefaultUnlocksCount);
			}
			else
			{
				int s = 0;
				int k = 0;
				for (int i = 1; i <= 7; i++)
				{
					int un = preferences.GetInt($"{ScreenUtils.UnlocksDayNumber}{i}", -1);
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
			Log.Debug("AAction", intent.Action);
			if (intent.Action != Intent.ActionScreenOn)
			{
				return;
			}

			NotificationSender notificationSender = new NotificationSender(context, DataHolder.ScreenLocksCategory);
			if (_mPreferences == null)
			{
				_mPreferences = PreferenceManager.GetDefaultSharedPreferences(context);
			}

			Date now = new Date();

			long monitoringStartTime;
			if ((monitoringStartTime = _mPreferences.GetLong(ScreenUtils.MonitoringLastStartTime, -1)) == -1)
			{
				monitoringStartTime = ScreenUtils.GetMonitoringStartTime(now);
				_mPreferences.Edit().PutLong(ScreenUtils.MonitoringLastStartTime, monitoringStartTime).Apply();
				using (StreamWriter writer = new StreamWriter(
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
				int thisDay = _mPreferences.GetInt($"{ScreenUtils.UnlocksDayNumber}{_lastDayUnlocked}", 0);
				_todayNormalSpeedValue = Math.Max(
					_abnormalUnlockMinCount,
					(int) (thisDay * _abnormalUnlockPercentage / 100d));
				_unlockMillis = new List<long>();
			}

//            if (lastDayUnlocked != -1 && lastDayUnlocked != Calendar.Instance.Get(CalendarField.DayOfWeek))
			if (TimeUnit.Milliseconds.ToDays(now.Time - monitoringStartTime) >= 1)
			{
				int unlocksToPut = UnlockedTimes;
				int u;

				if ((u = _mPreferences.GetInt($"{ScreenUtils.UnlocksDayNumber}{_lastDayUnlocked}", -1)) != -1)
				{
					unlocksToPut = (int) Math.Ceiling((u + unlocksToPut) / 2d);
				}

				int tmpDay = _lastDayUnlocked;
				_lastDayUnlocked = Calendar.Instance.Get(CalendarField.DayOfWeek);
				UnlockedTimes = 1;

				int thisDay = _mPreferences.GetInt($"{ScreenUtils.UnlocksDayNumber}{_lastDayUnlocked}", 0);
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

			int mode = 0;

			if (UnlockedTimes > GetNormalUnlocksCount(_mPreferences) * 1.1)
			{
				mode = 1;
				IsNormal = false;
			}
			else
			{
				IsNormal = true;
			}

			_abnormalUnlockInterval = _mPreferences.GetInt(AbnormalUnlocksTimeInterval, _abnormalUnlockInterval);

			if (_unlockMillis.Count > _todayNormalSpeedValue)
			{
				if (TimeUnit.Milliseconds.ToSeconds(now.Time - _unlockMillis[0]) <= _abnormalUnlockInterval)
				{
					mode = 2;
					using (StreamWriter writer = new StreamWriter(
						context.OpenFileOutput(DebugFile, FileCreationMode.Append)))
					{
						writer.WriteLine($"----Time: {new Date().GetFormattedDateTime()}----");
						foreach (var unlock in _unlockMillis)
						{
							writer.WriteLine($"Unlock at {new Date(unlock).GetFormattedDateTime()}, val = {unlock}");
						}

						writer.WriteLine("-----End1-----");
					}
				}
				else
				{
					List<long> tmpList = new List<long>();
					foreach (var time in _unlockMillis)
					{
						if (TimeUnit.Milliseconds.ToSeconds(now.Time - time) <= _abnormalUnlockInterval)
						{
							tmpList.Add(time);
						}
					}

					_unlockMillis = tmpList;
					if (_unlockMillis.Count > _todayNormalSpeedValue &&
					    TimeUnit.Milliseconds.ToSeconds(now.Time - _unlockMillis[0]) <= _abnormalUnlockInterval)
					{
						mode = 2;
						using (StreamWriter writer = new StreamWriter(
							context.OpenFileOutput(DebugFile, FileCreationMode.Append)))
						{
							writer.WriteLine($"----Time: {new Date().GetFormattedDateTime()}----");
							foreach (var unlock in _unlockMillis)
							{
								writer.WriteLine($"Unlock at {new Date(unlock).GetFormattedDateTime()}, val = {unlock}");
							}

							writer.WriteLine("-----End2-----");
						}
					}
				}
			}

			_unlockMillis.Add(now.Time);

			Log.Debug(nameof(ScreenUnlockReceiver), "ok");
			string notificationText;

			switch (mode)
			{
				case 1:
					notificationText =
						string.Format(context.GetString(Resource.String.category_screen_notif_daily_overflow),
							UnlockedTimes, NormalCount);
					notificationSender.PutNormalizeExtra(ScreenUtils.UnlocksNewNormalCount,
						(int) (UnlockedTimes * 1.2));
//					MainActivity.Adapter?.Refresh();
					CategoriesAdapter.Refresh(DataHolder.ScreenCategory);
					break;
				case 2:
					int tmpInterval = (int) (.9 * _abnormalUnlockInterval);
					if (tmpInterval >= _abnormalUnlockInterval)
					{
						if (tmpInterval > 1)
						{
							tmpInterval--;
						}
					}

					notificationText =
						string.Format(context.GetString(Resource.String.category_screen_notif_west_fast_hand),
							_unlockMillis.Count,
							TimeUnit.Milliseconds.ToSeconds(_unlockMillis.Last() - _unlockMillis.First()));

					notificationSender.PutNormalizeExtra(AbnormalUnlocksTimeInterval, tmpInterval);
//					MainActivity.Adapter?.Refresh();
					CategoriesAdapter.Refresh(DataHolder.ScreenCategory);
					break;
				default:
//					MainActivity.Adapter?.Refresh();
					CategoriesAdapter.Refresh(DataHolder.ScreenCategory);
					return;
			}

			notificationSender.Send(NotificationType.WarningNotification, notificationText);
		}

		public static void SetUnlockReceiverStatus(Context context, bool enable)
		{
			if (enable && _unlockReceiver == null)
			{
				IntentFilter screenStateFilter = new IntentFilter();
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
	}
}