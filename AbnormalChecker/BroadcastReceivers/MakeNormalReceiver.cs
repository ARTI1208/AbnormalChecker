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
            NotificationManager notificationManager = NotificationManager.FromContext(context);
            notificationManager.Cancel(intent.GetIntExtra(NotificationSender.ExtraNotificationId, 0));

            switch (intent.GetStringExtra(NotificationSender.ExtraNotificationCategory))
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
            }
            MainActivity.Adapter?.Refresh();
        }
    }
}