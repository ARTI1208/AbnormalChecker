using AbnormalChecker.Activities;
using Android.App;
using Android.Content;
using Android.Gms.Location;
using Android.Locations;
using Android.Util;

namespace AbnormalChecker.BroadcastReceivers
{
	[BroadcastReceiver]
	[IntentFilter(new []
	{
		LocationManager.ModeChangedAction
	})]
	public class LocationToggleReceiver : BroadcastReceiver
	{
		public override void OnReceive(Context context, Intent intent)
		{
			if (DataHolder.CategoriesDataDic[DataHolder.LocationCategory] == null)
			{
				DataHolder.Initialize(context);
			}
			DataHolder.CategoriesDataDic[DataHolder.LocationCategory]?.Update();
			MainActivity.adapter?.Refresh();
		}
	}
}