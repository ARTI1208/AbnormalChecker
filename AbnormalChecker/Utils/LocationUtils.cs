using System;
using System.IO;
using AbnormalChecker.Activities;
using AbnormalChecker.BroadcastReceivers;
using AbnormalChecker.Extensions;
using AbnormalChecker.Services;
using Android.App;
using Android.Content;
using Android.Gms.Location;
using Android.Locations;
using Android.OS;
using Android.Util;
using Java.Text;
using Java.Util;
using File = Java.IO.File;

namespace AbnormalChecker.Utils
{
	public static class LocationUtils
	{
		public const string LocationCoordinatesFile = "saved_coordinates.txt";
		public const string LocationLatitude = "location_latitude";
		public const string LocationLongitude = "location_longitude";
		public const string LocationCoordinatesPattern = @"Latitude = [0-9\.]\,+ Longitude = [0-9\.]+";

		private static FusedLocationProviderClient _fusedLocationProviderClient;
		private static PendingIntent _locationPendingIntent;
		private static readonly AbnormalLocationCallback Callback = new AbnormalLocationCallback();

		private static Context mContext;

		public static Location PreviousLocation;

		public static void Init(Context context)
		{
			mContext = context;
			_fusedLocationProviderClient = LocationServices.GetFusedLocationProviderClient(context);
		}

		public static string FormatLocation(Context context, Location location)
		{
			if (location == null)
				return "";
			var dateFormatter = DateFormat.DateTimeInstance;
			return
				string.Format(context.GetString(Resource.String.category_location_data), location.Latitude,
					location.Longitude, dateFormatter.Format(new Date(location.Time)));
		}

		public static void SetLocationTrackingEnabled(bool enabled)
		{
			if (enabled)
			{
				var request = new LocationRequest();
				request.SetInterval(5000);
				request.SetFastestInterval(5000);
				request.SetPriority(LocationRequest.PriorityHighAccuracy);

				//Probably not needed
				_fusedLocationProviderClient.RequestLocationUpdates(request, Callback, Looper.MainLooper);

				if (_locationPendingIntent == null)
				{
					var intent = new Intent(mContext, typeof(LocationUpdateReceiver));
					_locationPendingIntent =
						PendingIntent.GetBroadcast(mContext, 123, intent, PendingIntentFlags.UpdateCurrent);
					_fusedLocationProviderClient.RequestLocationUpdates(request, _locationPendingIntent);
				}
			}
			else
			{
				_fusedLocationProviderClient.RemoveLocationUpdates(Callback);
				_fusedLocationProviderClient.RemoveLocationUpdates(_locationPendingIntent);
				_locationPendingIntent = null;
			}
			MainActivity.Adapter?.NotifyDataSetChanged();
		}

		public static async void GetLastLocationFromDevice()
		{
			if (_fusedLocationProviderClient == null)
			{
				Log.Debug("LastAbLocation", "fu is null!");
				return;
			}

			var location = await _fusedLocationProviderClient.GetLastLocationAsync();
			if (location == null) return;
			var res = new float[2];
			var bigDestination = true;
			var entered = false;
			if (new File(mContext.FilesDir, LocationCoordinatesFile).Exists())
			{
				using (var reader =
					new StreamReader(mContext.OpenFileInput(LocationCoordinatesFile)))
				{
					foreach (var line in reader.ReadToEnd().Split("\n"))
					{
						if (!line.Contains("Latitude")) continue;

						var latString = line.Substring(line.AfterIndex("Latitude = "),
							line.IndexOf(", Long") - line.AfterIndex("Latitude = "));
						var longString = line.Substring(line.AfterIndex("Longitude = "));
						double latDouble;
						double longDouble;
						try
						{
							latDouble = double.Parse(latString);
							longDouble = double.Parse(longString);
						}
						catch (Exception)
						{
							continue;
						}

						Location.DistanceBetween(latDouble,
							longDouble, location.Latitude, location.Longitude, res);
						entered = true;
						if (res[0] < 10 * 1000)
						{
							bigDestination = false;
							break;
						}
					}
				}
			}

			if (entered && bigDestination)
			{
				var notPrevious = true;
				if (PreviousLocation != null)
				{
					var prevRes = new float[2];
					Location.DistanceBetween(PreviousLocation.Latitude,
						PreviousLocation.Longitude, location.Latitude, location.Longitude, prevRes);
					if (prevRes[0] < 500) notPrevious = false;
				}
				if (notPrevious)
				{
					var sender = new NotificationSender(mContext, DataHolder.LocationCategory);
					sender.PutNormalizeExtra(LocationLatitude, location.Latitude);
					sender.PutNormalizeExtra(LocationLongitude, location.Longitude);

					var distance =
						string.Format(mContext.GetString(Resource.String.category_location_notif_big_distance),
							res[0] / 1000);

					sender.Send(NotificationType.WarningNotification, distance);
					PreviousLocation = location;
					DataHolder.CategoriesDictionary[DataHolder.LocationCategory].Level = DataHolder.CheckStatus.Dangerous;
				}
			}
			else
			{
				if (DataHolder.CategoriesDictionary[DataHolder.LocationCategory].Level == DataHolder.CheckStatus.Dangerous)
					DataHolder.CategoriesDictionary[DataHolder.LocationCategory].Level = DataHolder.CheckStatus.Warning;
			}

			if (!new File(mContext.FilesDir, LocationCoordinatesFile).Exists())
				using (var writer =
					new StreamWriter(mContext.OpenFileOutput(LocationCoordinatesFile,
						FileCreationMode.Private)))
				{
					writer.WriteLine("Known locations:");
					writer.WriteLine($"Latitude = {location.Latitude}, Longitude = {location.Longitude}");
				}

			DataHolder.CategoriesDictionary[DataHolder.LocationCategory].Data = FormatLocation(mContext, location);
			MainActivity.Adapter?.Refresh();
		}
	}
}