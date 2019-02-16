using Android.App;
using Android.Content;
using Android.Widget;

namespace AbnormalChecker.BroadcastReceivers
{
    [BroadcastReceiver]
    [IntentFilter(new []
        {
            Intent.ActionNewOutgoingCall
        }
    )]
    public class PhoneCallReceiver : BroadcastReceiver
    {
        
        public override void OnReceive(Context context, Intent intent)
        {
            Toast.MakeText(context, "Outgoing call",ToastLength.Short).Show();
        }
    }
}