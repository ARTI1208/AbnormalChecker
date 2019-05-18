using System;
using AbnormalChecker.Activities;
using AbnormalChecker.Services;
using Android.App;
using Android.Content;
using Android.Util;
using Android.Widget;

namespace AbnormalChecker.BroadcastReceivers
{
	[BroadcastReceiver]
	[IntentFilter(new[]
	{
		NormalAction
	})]
	public class MakeNormalReceiver : BroadcastReceiver
	{
		private const string NormalAction = "ru.art2000.action.MakeNormalAction";

		public override void OnReceive(Context context, Intent intent)
		{
			var notificationManager = NotificationManager.FromContext(context);
			notificationManager.Cancel(intent.GetIntExtra(NotificationSender.ExtraNotificationId, 0));
			var category = intent.GetStringExtra(NotificationSender.ExtraNotificationCategory);
			switch (category)
			{
				case DataHolder.SystemCategory:
					DataHolder.NormalizeSystemData(
						intent.GetStringExtra(SystemModListenerService.ExtraFilePath),
						intent.GetStringExtra(SystemModListenerService.ExtraFileEvent));
					break;
				case DataHolder.ScreenLocksCategory:
					DataHolder.NormalizeScreenData(intent);
					break;
				case DataHolder.LocationCategory:
					DataHolder.NormalizeLocationData(intent);
					break;
				case DataHolder.PhoneCategory:
					DataHolder.NormalizePhoneData(intent);
					break;
				case DataHolder.SmsCategory:
					DataHolder.NormalizeSmsData(intent);
					break;
				case AlarmReceiver.SummaryCategory:
					try
					{
						AlarmReceiver.ExportSummary(intent.GetStringExtra(AlarmReceiver.ExtraSummaryText));
						Toast.MakeText(context, context.GetString(Resource.String.toast_export_successful),
							ToastLength.Short).Show();
					}
					catch (Exception e)
					{
						Log.Error(nameof(AlarmReceiver.ExportSummary), e.StackTrace);
						Log.Error(nameof(AlarmReceiver.ExportSummary), e.Message);
						Toast.MakeText(context, context.GetString(Resource.String.toast_export_failed),
							ToastLength.Short).Show();
					}

					break;
			}

			MainActivity.Adapter?.Refresh();
		}
	}
}