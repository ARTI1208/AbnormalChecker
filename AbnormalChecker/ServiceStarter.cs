using Android.App;
using Android.Content;
using Android.Preferences;
using Android.Util;
using Android.Widget;

namespace AbnormalChecker
{
    [BroadcastReceiver(Enabled = true, Exported = true)]
    [IntentFilter(new []
    {
        Intent.ActionBootCompleted,
        Intent.ActionMyPackageReplaced
    })]
    public class ServiceStarter : BroadcastReceiver
    {
        
        public const string ActionStartAbnormalMonitoring = "ru.art2000.action.ABNORMAL_MONITORING";
        private static bool _isStarted;
        private static ISharedPreferences mPreferences;
        
        public override void OnReceive(Context context, Intent intent)
        {
            Toast.MakeText(context, ActionStartAbnormalMonitoring, ToastLength.Short).Show();
            if (!_isStarted)
            {
                Log.Debug("AbnormalMonitorService", $"Starting by received {intent.Action}");
                IntentFilter screenStateFilter = new IntentFilter();
                screenStateFilter.AddAction(Intent.ActionScreenOn);
                if (mPreferences == null)
                {
                    mPreferences = PreferenceManager.GetDefaultSharedPreferences(context);
                }

                if (mPreferences.GetBoolean(Settings.ScreenLockAutoAdjustment, false))
                {
                    AbnormalBroadcastReceiver.AutoAdjustmentMonitorUnlockCount = 
                        mPreferences.GetInt("monitor_unlock_count", 0);
                    Log.Debug("Couunt",AbnormalBroadcastReceiver.AutoAdjustmentMonitorUnlockCount.ToString());
                }
                
                
                context.ApplicationContext.RegisterReceiver(new AbnormalBroadcastReceiver(), screenStateFilter);
                _isStarted = true;
                return;
            }
            Log.Debug("AbnormalMonitorService", $"Received {intent.Action}, but monitoring already started!");
        }
    }
}