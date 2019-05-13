using AbnormalChecker.Activities;
using AbnormalChecker.Utils;
using Android.Content;
using Android.Util;

namespace AbnormalChecker.BroadcastReceivers
{
	public class LocationModeChangeReceiver : BroadcastReceiver
	{
		public override void OnReceive(Context context, Intent intent)
		{
			LocationUtils.SetLocationTrackingEnabled(DataHolder.IsSelectedCategory(DataHolder.LocationCategory));
			MainActivity.Adapter?.Refresh();
		}
	}
}