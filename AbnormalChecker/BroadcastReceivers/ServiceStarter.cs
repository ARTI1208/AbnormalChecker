using AbnormalChecker.Activities;
using AbnormalChecker.Services;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Preferences;
using Android.Util;
using Android.Widget;

namespace AbnormalChecker.BroadcastReceivers
{
    [BroadcastReceiver(Enabled = true, Exported = true)]
    [IntentFilter(new []
    {
        Intent.ActionBootCompleted,
        Intent.ActionMyPackageReplaced
    })]
    public class ServiceStarter : BroadcastReceiver
    {
        
        private static ISharedPreferences _preferences;
        private static bool _isStarted;
        private static Intent mSystemIntent;
        
        public const string ActionAbnormalMonitoring = "ru.art2000.action.ABNORMAL_MONITORING";
        
        public override void OnReceive(Context context, Intent intent)
        {
//            if (!_isStarted)
//            {
                Log.Debug("AbnormalMonitorService", $"Starting by received {intent.Action}");
                if (DataHolder.IsSelectedCategory(DataHolder.SystemCategory))
                {
                    SetSystemMonitoringOn(context, true);
                }
                else if (mSystemIntent != null)
                {
                    context.StopService(mSystemIntent);
                }
                IntentFilter screenStateFilter = new IntentFilter();
                screenStateFilter.AddAction(Intent.ActionScreenOn);
                if (_preferences == null)
                {
                    _preferences = PreferenceManager.GetDefaultSharedPreferences(context);
                }
                if (_preferences.GetBoolean(Settings.ScreenLockAutoAdjustment, false))
                {
                    AbnormalBroadcastReceiver.AutoAdjustmentMonitorUnlockCount = 
                        _preferences.GetInt("monitor_unlock_count", 0);
                }
                context.ApplicationContext.RegisterReceiver(new AbnormalBroadcastReceiver(), screenStateFilter);
//                _isStarted = true;
//                return;
//            }
//            Log.Debug("AbnormalMonitorService", $"Received {intent.Action}, but monitoring already started!");
        }

        private static void SetSystemMonitoringOn(Context context, bool value)
        {
            if (value)
            {
                mSystemIntent = new Intent(context, typeof(SystemModListenerService));
                context.StartService(mSystemIntent);    
            }
            else if (mSystemIntent != null)
            {
                context.StopService(mSystemIntent);
            }
        }

        public static void StartStopMonitoring(Context context)
        {
            SetSystemMonitoringOn(context, false);
        }
        
        
    }
}