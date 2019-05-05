using System.Collections.Generic;
using AbnormalChecker.Services;
using Android.App;
using Android.Content;
using Android.Preferences;
using Android.Util;

namespace AbnormalChecker.BroadcastReceivers
{
	[BroadcastReceiver(Enabled = true, Exported = true)]
	[IntentFilter(new[]
	{
		Intent.ActionBootCompleted,
		Intent.ActionMyPackageReplaced
	})]
	public class MonitoringStarter : BroadcastReceiver
	{
		private static ISharedPreferences _preferences;
		private static bool _isStarted;

		public const string ActionAbnormalMonitoring = "ru.art2000.action.ABNORMAL_MONITORING";

		public override void OnReceive(Context context, Intent intent)
		{
			if (_isStarted)
			{
				//DO NOT REMOVE
				//Helps to avoid multiple receivers registration on same event
				Log.Debug("AbnormalMonitorService", $"Received {intent.Action}, but monitoring already started!");
				return;
			}
			Log.Debug("AbnormalMonitorService", $"Received {intent.Action}");
			if (_preferences == null)
			{
				_preferences = PreferenceManager.GetDefaultSharedPreferences(context);
			}
			SystemModListenerService.SetSystemMonitoringStatus(context,
				enable: DataHolder.IsSelectedCategory(DataHolder.SystemCategory));
			ScreenUnlockReceiver.SetUnlockReceiverStatus(context,
				enable: DataHolder.IsSelectedCategory(DataHolder.ScreenCategory));
			PhoneCallReceiver.SetCallReceiverStatus(context,
				enable: DataHolder.IsSelectedCategory(DataHolder.PhoneCategory));
			SmsReceiver.SetSmsReceiverStatus(context,
				enable: DataHolder.IsSelectedCategory(DataHolder.SmsCategory));
			_isStarted = true;
		}

		public void SetNewStatus(IList<string> selected)
		{
			
		}
		
	}
}