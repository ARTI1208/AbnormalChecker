using AbnormalChecker.Utils;
using Android.Content;

namespace AbnormalChecker.BroadcastReceivers
{
	[BroadcastReceiver]
	public class LocationUpdateReceiver : BroadcastReceiver
	{
		public override void OnReceive(Context context, Intent intent)
		{
			LocationUtils.GetLastLocationFromDevice();
		}
	}
}