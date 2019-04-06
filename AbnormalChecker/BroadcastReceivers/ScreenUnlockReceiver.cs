using System;
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

        private readonly string LastMonitorStarted = "last_monitor_start";

        private readonly string UnlocksDayNumber = "unlocks_dow_";
        
        public static readonly string UnlocksToday = "unlocks_today";
        
        private readonly string LastUnlockDay = "last_unlock_day";

        private static int lastDayUnlocked = -1;

        public static bool IsNormal = true;

        public static int NormalCount { get; set; } = -1;

        public static int unlockedTimes { get; set; }

        private static int GetMonitoringDayCount()
        {
            return mPreferences.GetBoolean(Settings.ScreenLockAutoAdjustmentType, true) ? 7 : 1;
        }

        public int GetNormalUnlocksCount(ISharedPreferences preferences)
        {
            if (GetMonitoringDayCount() == 7 && preferences.Contains($"{UnlocksDayNumber}{lastDayUnlocked}"))
            {
                NormalCount = preferences.GetInt($"{UnlocksDayNumber}{lastDayUnlocked}", DefaultUnlocksCount);
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

            long monitorStarted;
            if ((monitorStarted = mPreferences.GetLong(LastMonitorStarted, -1)) == -1)
            {
                monitorStarted = GetMonitoringStartTime(now);
                mPreferences.Edit().PutLong(LastMonitorStarted, monitorStarted).Apply();
            }
            
            if (lastDayUnlocked == -1)
            {
                int tmpDay;
                if ((tmpDay = mPreferences.GetInt(LastUnlockDay, -1)) != -1)
                {
                    lastDayUnlocked = tmpDay;
                    unlockedTimes = mPreferences.GetInt(UnlocksToday, 0);
                }
                else
                {
                    lastDayUnlocked = Calendar.Instance.Get(CalendarField.DayOfWeek);
                    mPreferences.Edit().PutInt(LastUnlockDay, lastDayUnlocked).Apply();                    
                }
            }


//            if (lastDayUnlocked != -1 && lastDayUnlocked != Calendar.Instance.Get(CalendarField.DayOfWeek))
            if (TimeUnit.Milliseconds.ToDays(now.Time - monitorStarted) >= 1)
            {
                int unlocksToPut = unlockedTimes;
                int u;
                
                if ((u = mPreferences.GetInt($"{UnlocksDayNumber}{lastDayUnlocked}", -1)) != -1)
                {
                    unlocksToPut = (int) Math.Ceiling((u + unlocksToPut) / 2d);
                }

                int tmpDay = lastDayUnlocked;
                lastDayUnlocked = Calendar.Instance.Get(CalendarField.DayOfWeek);
                unlockedTimes = 1;
                
                mPreferences.Edit()
                    .PutInt($"{UnlocksDayNumber}{tmpDay}", unlocksToPut)
                    .PutInt(LastUnlockDay, lastDayUnlocked)
                    .PutLong(LastMonitorStarted, GetMonitoringStartTime(now))
                    .PutInt(UnlocksToday, unlockedTimes)
                    .Apply();
                
                MainActivity.adapter?.Refresh();
                return;
            }

            
            mPreferences.Edit().PutInt(UnlocksToday, ++unlockedTimes).Apply();
            
            int mode = 0;

            if (unlockedTimes > GetNormalUnlocksCount(mPreferences) * 1.1)
            {
                mode = 1;
                IsNormal = false;
            }
            else
            {
                IsNormal = true;
            }

            string notificationText;

            switch (mode)
            {
                case 1:
                    notificationText =
                        $"Detected too many unlocks during last day : {unlockedTimes}, normal value : {NormalCount}";
                    MainActivity.adapter?.Refresh();
                    break;
//                case 2:
////                notificationText = $"Detected {unlockedTimes} unlocks in last {TimeUnit.Milliseconds.ToSeconds(unlocks[last].Time - unlocks[(last + 1) % abnormalCount].Time)} seconds";
//                    notificationText =
//                        $"High speed : {unlockedTimes / TimeSpan.FromMilliseconds(now.Time - firstTime).TotalHours} vs {normalDay * speedUnlock}";
//                    break;
                default:
                    MainActivity.adapter?.Refresh();
                    return;
            }

            notificationSender.Send(NotificationSender.WarningNotification, notificationText);
        }
    }
}