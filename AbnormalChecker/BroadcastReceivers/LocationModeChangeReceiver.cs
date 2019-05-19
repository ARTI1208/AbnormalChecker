using AbnormalChecker.Activities;
using AbnormalChecker.Utils;
using Android.Content;

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