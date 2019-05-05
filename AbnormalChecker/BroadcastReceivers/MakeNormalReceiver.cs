using AbnormalChecker.Services;
using Android.App;
using Android.Content;
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

        public const string NormalAction = "ru.art2000.action.MakeNormalAction";
        
        public override void OnReceive(Context context, Intent intent)
        {
            NotificationManager notificationManager = 
                (NotificationManager) context.GetSystemService(Context.NotificationService);
            notificationManager.Cancel(intent.GetIntExtra("notification_id", 0));

            switch (intent.GetStringExtra("category"))
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
            }
            
        }
    }
}