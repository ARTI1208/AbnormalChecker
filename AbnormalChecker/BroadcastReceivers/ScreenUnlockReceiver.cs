using System;
using System.Collections.Generic;
using System.Linq;
using AbnormalChecker.Activities;
using Android.App;
using Android.Content;
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

        private static int GetMonitoringDayCount()
        {
            return _mPreferences.GetBoolean(Settings.ScreenLockAutoAdjustmentType, true) ? 7 : 1;
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
            if (GetMonitoringDayCount() == 7 && preferences.Contains($"{ScreenUtils.UnlocksDayNumber}{_lastDayUnlocked}"))
            {
                NormalCount = preferences.GetInt($"{ScreenUtils.UnlocksDayNumber}{_lastDayUnlocked}", DefaultUnlocksCount);
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

                MainActivity.adapter?.Refresh();
                return;
            }

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
                    if (_unlockMillis.Count > _todayNormalSpeedValue && 
                        TimeSpan.FromMilliseconds(now.Time - _unlockMillis[0]).Seconds <= _abnormalUnlockInterval)
                    {
                        mode = 2;
                    }
                }
            }
            
            _unlockMillis.Add(now.Time);

            string notificationText;
            
            switch (mode)
            {
                case 1:
                    notificationText =
                        $"Detected too many unlocks during last day : {UnlockedTimes}, normal value : {NormalCount}";
                    notificationSender.PutNormalizeExtra(ScreenUtils.UnlocksNewNormalCount, (int) (UnlockedTimes * 1.2));
                    MainActivity.adapter?.Refresh();
                    break;
                case 2:
                    int tmpInterval = (int) (.9 * _abnormalUnlockInterval);
                    if (tmpInterval < _abnormalUnlockInterval)
                    {
                        if (tmpInterval >= 1)
                        {
                            _abnormalUnlockInterval = tmpInterval;
                        }
                    }
                    else
                    {
                        if (_abnormalUnlockInterval > 1)
                        {
                            _abnormalUnlockInterval--;
                        }
                    }
                    notificationText = $"Detected {_unlockMillis.Count} unlocks in last " +
                                       $@"{TimeUnit.Milliseconds.ToSeconds(
                                           _unlockMillis.Last() - _unlockMillis.First())} seconds";
                    notificationSender.PutNormalizeExtra(AbnormalUnlocksTimeInterval, _abnormalUnlockInterval);
                    MainActivity.adapter?.Refresh();
                    break;
                default:
                    Log.Debug("DefaultOut", "Upd");
                    MainActivity.adapter?.Refresh();
                    return;
            }

            notificationSender.Send(NotificationSender.WarningNotification, notificationText);
        }
    }
}