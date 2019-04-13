using System;
using System.Collections.Generic;
using System.Linq;
using AbnormalChecker.Activities;
using Android.App;
using Android.Content;
using Android.Preferences;
using Android.Util;
using Android.Widget;
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
        private static ISharedPreferences mPreferences;

        private static readonly int DefaultUnlocksCount = 10;

        #region Keys

        private readonly string MonitoringLastStartTime = "last_monitor_start";

        private readonly string UnlocksDayNumber = "unlocks_dow_";

        public static readonly string UnlocksToday = "unlocks_today";

        private readonly string LastUnlockDay = "last_unlock_day";

        #endregion

        #region UnlockSpeed

        private static int _abnormaUnlockMinCount = 3;

        private static int _abnormaUnlockPercentage = 20;

        private static int _abnormalUnlockInterval = 10;

//        private static long[] _unlockMillis;

        private static int _lastPosition = 0;
        
        private static List<long> _unlockMillis;

        private static int _speedUnlocks;
        
        private static int _todayNormalSpeedValue;

        #endregion

        private static int _lastDayUnlocked = -1;

        public static bool IsNormal = true;

        public static int NormalCount { get; set; } = -1;

        public static int UnlockedTimes { get; set; }

        private static int GetMonitoringDayCount()
        {
            return mPreferences.GetBoolean(Settings.ScreenLockAutoAdjustmentType, true) ? 7 : 1;
        }

        private int GetNormalUnlocksCount(ISharedPreferences preferences)
        {
            if (GetMonitoringDayCount() == 7 && preferences.Contains($"{UnlocksDayNumber}{_lastDayUnlocked}"))
            {
                NormalCount = preferences.GetInt($"{UnlocksDayNumber}{_lastDayUnlocked}", DefaultUnlocksCount);
            }
            else
            {
                int s = 0;
                int k = 0;
                for (int i = 1; i <= 7; i++)
                {
                    int un = preferences.GetInt($"{UnlocksDayNumber}{i}", -1);
                    if (un > 0)
                    {
                        s += un;
                        k++;
                    }
                }

                NormalCount = k > 0 ? (int) Math.Ceiling((double) s / k) : DefaultUnlocksCount;
            }
            return NormalCount;
        }

        private long GetMonitoringStartTime(Date dateTime)
        {
            Calendar c = Calendar.Instance;
            c.Time = dateTime;
            c.Set(CalendarField.Hour, 0);
            c.Set(CalendarField.Minute, 0);
            c.Set(CalendarField.Second, 1);
            c.Set(CalendarField.Millisecond, 0);
            return c.TimeInMillis;
        }

        public override void OnReceive(Context context, Intent intent)
        {
            Log.Debug("AAction", intent.Action);
            if (intent.Action != Intent.ActionScreenOn)
            {
                return;
            }

            NotificationSender notificationSender = new NotificationSender(context, DataHolder.ScreenLocksCategory);
            if (mPreferences == null)
            {
                mPreferences = PreferenceManager.GetDefaultSharedPreferences(context);
            }

            Date now = new Date();

            long monitoringStartTime;
            if ((monitoringStartTime = mPreferences.GetLong(MonitoringLastStartTime, -1)) == -1)
            {
                monitoringStartTime = GetMonitoringStartTime(now);
                mPreferences.Edit().PutLong(MonitoringLastStartTime, monitoringStartTime).Apply();
            }

            if (_lastDayUnlocked == -1)
            {
                int tmpDay;
                if ((tmpDay = mPreferences.GetInt(LastUnlockDay, -1)) != -1)
                {
                    _lastDayUnlocked = tmpDay;
                    UnlockedTimes = mPreferences.GetInt(UnlocksToday, 0);
                }
                else
                {
                    _lastDayUnlocked = Calendar.Instance.Get(CalendarField.DayOfWeek);
                    mPreferences.Edit().PutInt(LastUnlockDay, _lastDayUnlocked).Apply();
                }
            }

            if (_unlockMillis == null)
            {
                int thisDay = mPreferences.GetInt($"{UnlocksDayNumber}{_lastDayUnlocked}", 0);
                _todayNormalSpeedValue = Math.Max(
                    _abnormaUnlockMinCount, 
                    (int) (thisDay * _abnormaUnlockPercentage / 100d));
                _unlockMillis = new List<long>();
            }

//            if (lastDayUnlocked != -1 && lastDayUnlocked != Calendar.Instance.Get(CalendarField.DayOfWeek))
            if (TimeUnit.Milliseconds.ToDays(now.Time - monitoringStartTime) >= 1)
            {
                int unlocksToPut = UnlockedTimes;
                int u;

                if ((u = mPreferences.GetInt($"{UnlocksDayNumber}{_lastDayUnlocked}", -1)) != -1)
                {
                    unlocksToPut = (int) Math.Ceiling((u + unlocksToPut) / 2d);
                }

                int tmpDay = _lastDayUnlocked;
                _lastDayUnlocked = Calendar.Instance.Get(CalendarField.DayOfWeek);
                UnlockedTimes = 1;
                
                int thisDay = mPreferences.GetInt($"{UnlocksDayNumber}{_lastDayUnlocked}", 0);
                _todayNormalSpeedValue = Math.Max(
                    _abnormaUnlockMinCount, 
                    (int) (thisDay * _abnormaUnlockPercentage / 100d));
                _unlockMillis = new List<long>();

                mPreferences.Edit()
                    .PutInt($"{UnlocksDayNumber}{tmpDay}", unlocksToPut)
                    .PutInt(LastUnlockDay, _lastDayUnlocked)
                    .PutLong(MonitoringLastStartTime, GetMonitoringStartTime(now))
                    .PutInt(UnlocksToday, UnlockedTimes)
                    .Apply();

                MainActivity.adapter?.Refresh();
                return;
            }


            mPreferences.Edit().PutInt(UnlocksToday, ++UnlockedTimes).Apply();

            
            
            
            int mode = 0;

            if (UnlockedTimes > GetNormalUnlocksCount(mPreferences) * 1.1)
            {
                mode = 1;
                IsNormal = false;
            }
            else
            {
                IsNormal = true;
            }

//            long tmpTime;
//            if (_unlockMillis != null && (tmpTime = _unlockMillis[(++_lastPosition + 1) % _unl]) != 0 &&
//                TimeSpan.FromMilliseconds(now.Time - tmpTime).Seconds < _abnormalUnlockInterval)
//            {
//                mode = 2;
//            }

            if (_unlockMillis.Count > _todayNormalSpeedValue)
            {
                if (TimeSpan.FromMilliseconds(now.Time - _unlockMillis[0]).Seconds <= _abnormalUnlockInterval)
                {
                    mode = 2;
                    Toast.MakeText(context, $"{TimeSpan.FromMilliseconds(now.Time - _unlockMillis[0]).Seconds} vs {_abnormalUnlockInterval}", ToastLength.Short).Show();
                }
                else
                {
                    List<long> tmpList = new List<long>();
                    foreach (var time in _unlockMillis)
                    {
                        if (TimeSpan.FromMilliseconds(now.Time - time).Seconds <= _abnormalUnlockInterval)
                        {
                            tmpList.Add(time);
                        }        
                    }
                    _unlockMillis = tmpList;
                }
            }
            

//            _unlockMillis[_lastPosition] = now.Time;
//            _unlockMillis?.SetValue(now.Time, _lastPosition = _lastPosition % _unlockMillis.Length);
            _unlockMillis.Add(now.Time);

            string notificationText;
            
            switch (mode)
            {
                case 1:
                    notificationText =
                        $"Detected too many unlocks during last day : {UnlockedTimes}, normal value : {NormalCount}";
                    MainActivity.adapter?.Refresh();
                    break;
                case 2:
                    notificationText = $"Detected {_unlockMillis.Count} unlocks in last " +
                                       $@"{TimeUnit.Milliseconds.ToSeconds(
                                           _unlockMillis.Last() - _unlockMillis.First())} seconds";
//                    notificationText =
//                        $"High speed : {unlockedTimes / TimeSpan.FromMilliseconds(now.Time - firstTime).TotalHours} vs {normalDay * speedUnlock}";
                    MainActivity.adapter?.Refresh();
                    break;
                default:
                    MainActivity.adapter?.Refresh();
                    return;
            }

            notificationSender.Send(NotificationSender.WarningNotification, notificationText);
        }
    }
}