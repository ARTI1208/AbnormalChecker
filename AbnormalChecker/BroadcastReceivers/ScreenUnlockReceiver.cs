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
        
        private readonly string UnlocksToday = "unlocks_today";
        
        private readonly string LastUnlockDay = "last_unlock_day";

        private static int lastDayUnlocked = -1;

        public static int unlockedTimes { get; private set; }

        private static int GetMonitoringDayCount()
        {
            return mPreferences.GetBoolean(Settings.ScreenLockAutoAdjustmentType, true) ? 7 : 1;
        }


        public long StartNewDayMonitor(Date dateTime)
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
                monitorStarted = StartNewDayMonitor(now);
                mPreferences.Edit().PutLong(LastMonitorStarted, monitorStarted).Apply();
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
                    .PutLong(LastMonitorStarted, StartNewDayMonitor(now))
                    .PutInt(UnlocksToday, unlockedTimes)
                    .Apply();
                
                MainActivity.adapter?.Refresh();
                return;
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
            
            mPreferences.Edit().PutInt(UnlocksToday, ++unlockedTimes).Apply();
            
            int mode = 0;

            int normalValue;

            if (GetMonitoringDayCount() == 7 && mPreferences.Contains($"{UnlocksDayNumber}{lastDayUnlocked}"))
            {
                normalValue = mPreferences.GetInt($"{UnlocksDayNumber}{lastDayUnlocked}", DefaultUnlocksCount);
            }
            else
            {
                int s = 0;
                int k = 0;
                for (int i = 1; i <= 7; i++)
                {
                    int un = mPreferences.GetInt($"{UnlocksDayNumber}{i}", -1);
                    if (un > 0)
                    {
                        s += un;
                        k++;
                    }
                }

                normalValue = k > 0 ? (int) Math.Ceiling((double) s / k) : DefaultUnlocksCount;
            }

            if (unlockedTimes > normalValue * 1.1)
            {
                mode = 1;
            }

            string notificationText;

            switch (mode)
            {
                case 1:
                    notificationText =
                        $"Detected too many unlocks during last day : {unlockedTimes}, normal value : {normalValue}";
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