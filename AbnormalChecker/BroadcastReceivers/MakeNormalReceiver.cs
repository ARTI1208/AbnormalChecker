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
            Toast.MakeText(context, NormalAction, ToastLength.Short).Show();
        }
    }
}