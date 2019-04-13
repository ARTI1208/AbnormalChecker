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
	[IntentFilter(new[]
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
			if (_isStarted)
			{
				//DO NOT REMOVE
				//Helps to avoid multiple receivers registration on same event
				Log.Info("AbnormalMonitorService", $"Received {intent.Action}, but monitoring already started!");
				return;
			}
			
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
			context.ApplicationContext.RegisterReceiver(new ScreenUnlockReceiver(), screenStateFilter);
			_isStarted = true;
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