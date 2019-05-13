using AbnormalChecker.Activities;
using AbnormalChecker.Utils;
using Android.App;
using Android.Content;
using Android.Locations;
using Android.Util;
using ILocationListener = Android.Gms.Location.ILocationListener;

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