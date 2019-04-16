using AbnormalChecker.Activities;
using Android.Gms.Location;
using Android.Util;

namespace AbnormalChecker.Services
{
	public class AbnormalLocationCallback : LocationCallback
	{

		private const string LocationTag = "AbnormalLocationResolver";
		
		public override void OnLocationResult(LocationResult result)
		{
			Log.Debug(LocationTag, DataHolder.FormatLocation(result.LastLocation));
//			DataHolder.CategoriesDataDic["location"].Data = DataHolder.FormatLocation(result.LastLocation);
//			MainActivity.adapter?.NotifyDataSetChanged();

			DataHolder.GetLastLocationFromDevice();

		}
		
		
	}
}