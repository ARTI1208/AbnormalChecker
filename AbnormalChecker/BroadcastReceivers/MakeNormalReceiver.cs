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
            Toast.MakeText(context, intent.GetStringExtra("category"), ToastLength.Short).Show();
            NotificationManager notificationManager = 
                (NotificationManager) context.GetSystemService(Context.NotificationService);
            notificationManager.Cancel(intent.GetIntExtra("notification_id", 0));
        }
    }
}