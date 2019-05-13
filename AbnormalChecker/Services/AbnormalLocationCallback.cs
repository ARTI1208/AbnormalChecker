using AbnormalChecker.Utils;
using Android.Gms.Location;

namespace AbnormalChecker.Services
{
	public class AbnormalLocationCallback : LocationCallback
	{

		public override void OnLocationResult(LocationResult result)
		{
			LocationUtils.GetLastLocationFromDevice();
		}
	}
}