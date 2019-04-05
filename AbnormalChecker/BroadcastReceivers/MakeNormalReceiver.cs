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
                    DataHolder.normalizeSystemData(
                        intent.GetStringExtra(SystemModListenerService.ExtraFilePath),
                        intent.GetStringExtra(SystemModListenerService.ExtraFileEvent));
                    break;
            }
            
        }
    }
}