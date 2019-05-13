using System.Collections.Generic;
using AbnormalChecker.Services;
using AbnormalChecker.Utils;
using Android.App;
using Android.Content;
using Android.Locations;
using Android.Preferences;
using Android.Util;

namespace AbnormalChecker.BroadcastReceivers
{
	[BroadcastReceiver(Enabled = true, Exported = true)]
	[IntentFilter(new[]
	{
		Intent.ActionBootCompleted,
		Intent.ActionMyPackageReplaced,
		ActionAbnormalMonitoring
	})]
	public class MonitoringStarter : BroadcastReceiver
	{
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

			DataHolder.Initialize(context);
			
			ScreenUnlockReceiver.SetUnlockReceiverStatus(context,
				enable: DataHolder.IsSelectedCategory(DataHolder.ScreenCategory));
			PhoneCallReceiver.SetCallReceiverStatus(context,
				enable: DataHolder.IsSelectedCategory(DataHolder.PhoneCategory));
			SmsReceiver.SetSmsReceiverStatus(context,
				enable: DataHolder.IsSelectedCategory(DataHolder.SmsCategory));
			
			IntentFilter filter = new IntentFilter();
			filter.AddAction(LocationManager.ModeChangedAction);
			context.ApplicationContext.RegisterReceiver(new LocationModeChangeReceiver(), filter);
			
			if (intent.Action == ActionAbnormalMonitoring)
			{
				SystemModListenerService.SetSystemMonitoringStatus(context,
					enable: DataHolder.IsSelectedCategory(DataHolder.SystemCategory));	
			}
			
			LocationUtils.SetLocationTrackingEnabled(DataHolder.IsSelectedCategory(DataHolder.LocationCategory));

			_isStarted = true;
		}
		
	}
}