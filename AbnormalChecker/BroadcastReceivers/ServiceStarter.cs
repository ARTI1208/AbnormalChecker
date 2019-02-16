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
        private static FileObserver _fileObserver;
        
        public const string ActionStartAbnormalMonitoring = "ru.art2000.action.ABNORMAL_MONITORING";
        
        public override void OnReceive(Context context, Intent intent)
        {
            if (!_isStarted)
            {
//                _fileObserver = new FileObserver("/sdcard/f");
//                _fileObserver.StartWatching();
                Intent systemIntent = new Intent(context, typeof(SystemModListenerService));
                context.StartService(systemIntent);
                Log.Debug("AbnormalMonitorService", $"Starting by received {intent.Action}");
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
                _isStarted = true;
                return;
            }
            Log.Debug("AbnormalMonitorService", $"Received {intent.Action}, but monitoring already started!");
        }
    }
}