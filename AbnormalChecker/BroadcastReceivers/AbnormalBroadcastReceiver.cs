using System;
using AbnormalChecker.Activities;
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
    public class AbnormalBroadcastReceiver : BroadcastReceiver
    {
        

        private static ISharedPreferences mPreferences;
        public static string ScreenLocksCountKey = "monitor_unlock_count";

        private static long firstTime;

        private static readonly int speedUnlock = 10;
        private static readonly int dayUnlocksAmpl = 10;
        

        private static readonly int abnormalCount = 3;
        private static Date start;
        private static Date[] unlocks = new Date[abnormalCount];
        private static int last;
        public static int[] d;
        private static int current;
        private static Date started;

        public static int AutoAdjustmentMonitorUnlockCount;

        public static int unlockedTimes { get; private set; }

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
            if (mPreferences.GetBoolean(Settings.ScreenLockAutoAdjustment, false))
            {
                if (TimeUnit.Milliseconds.ToDays(now.Time -
                                                 mPreferences.GetLong("auto_start_time", now.Time))
                    >= mPreferences.GetInt(Settings.ScreenLockAutoAdjustmentDayCount, 1))
                {
                    mPreferences.Edit().PutBoolean("auto_unlock_limit", false).Apply();
//                    sendNotification(context);
                    notificationSender.Send(NotificationSender.InfoNotification,
                        $"{mPreferences.GetInt("auto_unlock_monitor_time", 1)}-day monitoring ended. " +
                        $"Detected {mPreferences.GetInt("monitor_unlock_count", AutoAdjustmentMonitorUnlockCount)} " +
                        "unlocks");
                    MainActivity.adapter?.Refresh();
                    return;
                }


                Log.Debug("Couunt", AutoAdjustmentMonitorUnlockCount.ToString());
                AutoAdjustmentMonitorUnlockCount++;
                mPreferences.Edit().PutInt(ScreenLocksCountKey, AutoAdjustmentMonitorUnlockCount).Apply();
                MainActivity.adapter?.Refresh();
                return;
            }

            if (intent.Action == Intent.ActionScreenOn)
                unlockedTimes++;
            unlocks[last = ++last % abnormalCount] = new Date();
            if (d == null)
            {
                d = new int[mPreferences.GetInt("unlock_check_time", 24)];
                started = now;
            }


            if (unlockedTimes == 1)
            {
                firstTime = now.Time;
            }

            if (now.Time - started.Time < TimeUnit.Hours.ToMillis(1))
            {
                d[current]++;
            }
            else
            {
                d[++current]++;
                started = now;
            }

            double normalDay = (double) mPreferences.GetInt(ScreenLocksCountKey, AutoAdjustmentMonitorUnlockCount)
                               / (mPreferences.GetInt(Settings.ScreenLockAutoAdjustmentDayCount, 1) * 24);

//            if (unlockedTimes < abnormalCount) return;
//            if (TimeSpan.FromMilliseconds(unlocks[last].Time - unlocks[(last + 1) % abnormalCount].Time) >=
//                TimeSpan.FromSeconds(mPreferences.GetInt("screen_limit", 5))) return;

            int mode = 0;

            Log.Debug("AuutoAdj",
                mPreferences.GetInt(ScreenLocksCountKey, AutoAdjustmentMonitorUnlockCount).ToString());

            if (unlockedTimes >= mPreferences.GetInt(ScreenLocksCountKey, AutoAdjustmentMonitorUnlockCount) +
                dayUnlocksAmpl)
            {
                mode = 1;
            }
            else if (unlockedTimes / TimeSpan.FromMilliseconds(now.Time - firstTime).TotalHours >
                     normalDay * speedUnlock)
            {
                if (unlockedTimes > abnormalCount)
                {
                    mode = 2;
                }
            }

            string notificationText;

            
            switch (mode)
            {
                case 1:
                    notificationText = $"Detected too many unlocks during last day : {unlockedTimes}";
                    break;
                case 2:
//                notificationText = $"Detected {unlockedTimes} unlocks in last {TimeUnit.Milliseconds.ToSeconds(unlocks[last].Time - unlocks[(last + 1) % abnormalCount].Time)} seconds";
                    notificationText =
                        $"High speed : {unlockedTimes / TimeSpan.FromMilliseconds(now.Time - firstTime).TotalHours} vs {normalDay * speedUnlock}";
                    break;
                default:
                    MainActivity.adapter?.Refresh();
                    return;
            }

            notificationSender.Send(NotificationSender.WarningNotification, notificationText);
            
        }

    }
}