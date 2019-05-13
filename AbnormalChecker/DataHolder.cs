using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using AbnormalChecker.Activities;
using AbnormalChecker.BroadcastReceivers;
using AbnormalChecker.Extensions;
using AbnormalChecker.Services;
using AbnormalChecker.Utils;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Location;
using Android.Locations;
using Android.OS;
using Android.Preferences;
using Android.Support.V4.Content;
using Android.Util;
using Java.Text;
using Java.Util;
using File = Java.IO.File;

namespace AbnormalChecker
{
	public delegate DataHolder.CategoryData CategoryUpdater(DataHolder.CategoryData data);

	[SuppressMessage("ReSharper", "UnusedMember.Global")]
	public class DataHolder
	{
		public enum CheckStatus
		{
			Normal,
			Warning,
			Dangerous,
			PermissionsRequired
		}

		public const string RootCategory = "root";
		public const string ScreenLocksCategory = "screen";
		public const string LocationCategory = "location";
		public const string SystemCategory = "system";
		public const string ScreenCategory = "screen";
		public const string PhoneCategory = "phone";
		public const string SmsCategory = "sms";

		public static readonly Dictionary<string, CategoryData> CategoriesDataDic =
			new Dictionary<string, CategoryData>();

		private static Context _mContext;
		private static ISharedPreferences _mPreferences;
		private static FusedLocationProviderClient _fusedLocationProviderClient;
		private static string[] _allCategories;
		private static PendingIntent _locationPendingIntent;
		private static readonly AbnormalLocationCallback Callback = new AbnormalLocationCallback();
		public static Location PreviousLocation;

		public DataHolder(Context context)
		{
			_mContext = context;
			_mPreferences = PreferenceManager.GetDefaultSharedPreferences(_mContext);
			_fusedLocationProviderClient = LocationServices.GetFusedLocationProviderClient(_mContext);
			_allCategories = context.Resources.GetStringArray(Resource.Array.categories_values);
			foreach (var category in _allCategories)
				CategoriesDataDic[category] =
					new CategoryData(category.MakeFirstUpper(), CreateCategoryDelegate(category));
		}

		public static ICollection<string> GetSelectedCategories()
		{
			return _mPreferences.GetStringSet("selected_categories", _allCategories);
		}

		public static bool IsSelectedCategory(string category)
		{
			ICollection<string> collection = GetSelectedCategories();
			return collection != null && collection.Contains(category);
		}

		public static void Refresh()
		{
			var sel = GetSelectedCategories();
			if (sel == null) return;

			foreach (var category in sel)
			{
				var categoryData = CategoriesDataDic[category];
				categoryData.Update();
			}
		}

		public static async void GetLastLocationFromDevice()
		{
			Log.Debug("LastAbLocation", "start");
			if (_fusedLocationProviderClient == null)
			{
				Log.Debug("LastAbLocation", "fu is null!");
				return;
			}
			var location = await _fusedLocationProviderClient.GetLastLocationAsync();
			if (location == null) return;
			Log.Debug("LastAbLocation", "loc not null");
			var res = new float[2];
			var bigDestination = true;
			var entered = false;
			if (new File(_mContext.FilesDir, LocationUtils.LocationCoordinatesFile).Exists())
			{
				using (var reader =
					new StreamReader(_mContext.OpenFileInput(LocationUtils.LocationCoordinatesFile)))
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
						Log.Debug("NewAbLoction", res[0].ToString());
						entered = true;
						if (res[0] < 10 * 1000)
						{
							bigDestination = false;
							break;
						}
					}
				}
				Log.Debug("LastAbLocation", "read known loc");
			}

			if (entered && bigDestination)
			{
				Log.Debug("LastAbLocation", "big d1");
				var notPrevious = true;
				if (PreviousLocation != null)
				{
					float[] prevRes = new float[2];
					Location.DistanceBetween(PreviousLocation.Latitude,
						PreviousLocation.Longitude, location.Latitude, location.Longitude, prevRes);
					if (prevRes[0] < 500) notPrevious = false;
					Log.Debug("LastAbLocation", "big d2");
				}
				Log.Debug("LastAbLocation", "big d3");
				if (notPrevious)
				{
					var sender = new NotificationSender(_mContext, LocationCategory);
					sender.PutNormalizeExtra(LocationUtils.LocationLatitude, location.Latitude);
					sender.PutNormalizeExtra(LocationUtils.LocationLongitude, location.Longitude);

					string distance =
						string.Format(_mContext.GetString(Resource.String.category_location_notif_big_distance),
							res[0] / 1000);

					sender.Send(NotificationType.WarningNotification, distance);
					PreviousLocation = location;
					CategoriesDataDic[LocationCategory].Level = CheckStatus.Dangerous;
					Log.Debug("LastAbLocation", "big d4");
				}
			}
			else
			{
				Log.Debug("LastAbLocation", "big d5");
				if (CategoriesDataDic[LocationCategory].Level == CheckStatus.Dangerous)
				{
					CategoriesDataDic[LocationCategory].Level = CheckStatus.Warning;	
				}
			}

			if (!new File(_mContext.FilesDir, LocationUtils.LocationCoordinatesFile).Exists())
				using (var writer =
					new StreamWriter(_mContext.OpenFileOutput(LocationUtils.LocationCoordinatesFile,
						FileCreationMode.Private)))
				{
					writer.WriteLine("Known locations:");
					writer.WriteLine($"Latitude = {location.Latitude}, Longitude = {location.Longitude}");
				}

			CategoriesDataDic[LocationCategory].Data = FormatLocation(location);
			Log.Debug("LastAbLocation", "big d6");
			if (MainActivity.Adapter != null)
			{
				
				int pos = MainActivity.Adapter.categories.FindIndex(s => s == LocationCategory);
				Log.Debug("LastAbLocation", $"big d8 = {pos} ");
				MainActivity.Adapter.NotifyItemChanged(pos);
			}
			
			Log.Debug("LastAbLocation", "big d7");

//			MainActivity.Adapter?.NotifyDataSetChanged();
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
					var intent = new Intent(_mContext, typeof(LocationUpdateReceiver));
					_locationPendingIntent =
						PendingIntent.GetBroadcast(_mContext, 123, intent, PendingIntentFlags.UpdateCurrent);
					_fusedLocationProviderClient.RequestLocationUpdates(request, _locationPendingIntent);
				}
			}
			else
			{
				_fusedLocationProviderClient.RemoveLocationUpdates(Callback);
				_fusedLocationProviderClient.RemoveLocationUpdates(_locationPendingIntent);
				_locationPendingIntent = null;
			}
		}

		public static string[] GetAllRequiredPermissions(Context context)
		{
			_mContext = context;
			return context
				.PackageManager
				.GetPackageInfo(context.PackageName, PackageInfoFlags.Permissions)
				.RequestedPermissions.ToArray();
		}

		public static void Initialize(Context context)
		{
			_mContext = context;
			_mPreferences = PreferenceManager.GetDefaultSharedPreferences(_mContext);
			_fusedLocationProviderClient = LocationServices.GetFusedLocationProviderClient(_mContext);
			_allCategories = context.Resources.GetStringArray(Resource.Array.categories_values);
			foreach (var category in _allCategories)
				CategoriesDataDic[category] =
					new CategoryData(category.MakeFirstUpper(), CreateCategoryDelegate(category));
		}

		private static CategoryUpdater CreateCategoryDelegate(string category)
		{
			var methodName = $"Get{category.First().ToString().ToUpper()}{category.Substring(1)}Data";

			var methodInfo = typeof(DataHolder).GetMethod(methodName, BindingFlags.Public | BindingFlags.Static,
				null,
				new[] {typeof(CategoryData)}, null);

			if (methodInfo == null)
			{
				Log.Error(nameof(DataHolder), "Cannot find category updater method");
				return null;
			}

			var categoryDelegate = Delegate.CreateDelegate(typeof(CategoryUpdater),
				methodInfo) as CategoryUpdater;
			return categoryDelegate;
		}

		public static string FormatLocation(Location location)
		{
			if (location == null)
				return "";
			var dateFormatter = DateFormat.DateTimeInstance;
			return
				string.Format(_mContext.GetString(Resource.String.category_location_data), location.Latitude,
					location.Longitude, dateFormatter.Format(new Date(location.Time)));
		}

		public class CategoryData
		{
			private readonly CategoryUpdater dataUpdater;
			private CheckStatus level = CheckStatus.Normal;

			private string[] requiredPermissions;
			private string status = "OK";
			public string Data;

			public string DataFilePath;
			public string Title;

			public CategoryData(string title, CategoryUpdater updater)
			{
				Title = title;
				dataUpdater = updater;
			}

			public string Status
			{
				get => status;
				set
				{
					if (requiredPermissions != null && requiredPermissions.Any(s =>
						    ContextCompat.CheckSelfPermission(_mContext, s) == Permission.Denied))
					{
						level = CheckStatus.PermissionsRequired;
						status = _mContext.GetString(Resource.String.category_info_permissions_denied);
					}
					else
					{
						status = value;
					}
				}
			}

			public CheckStatus Level
			{
				get => level;
				set
				{
					if (requiredPermissions != null && requiredPermissions.Any(s =>
						    ContextCompat.CheckSelfPermission(_mContext, s) == Permission.Denied))
					{
						level = CheckStatus.PermissionsRequired;
						Status = _mContext.GetString(Resource.String.category_info_permissions_denied);
					}
					else
					{
						level = value;
					}
				}
			}

			public string[] RequiredPermissions
			{
				get => requiredPermissions;
				set
				{
					if (value.Any(s => ContextCompat.CheckSelfPermission(_mContext, s) == Permission.Denied))
					{
						Level = CheckStatus.PermissionsRequired;
						Status = _mContext.GetString(Resource.String.category_info_permissions_denied);
					}
					else
					{
						Level = CheckStatus.Normal;
					}

					requiredPermissions = value;
				}
			}

			public void Update()
			{
				dataUpdater?.Invoke(this);
			}
		}

		#region Updaters

		public static CategoryData GetRootData(CategoryData data)
		{
			data.Title = _mContext.GetString(Resource.String.category_root);
			if (RootUtils.IsRooted(_mContext))
			{
				data.Status = _mContext.GetString(Resource.String.category_root_status_root_access);
				data.Data = string.Format(_mContext.GetString(Resource.String.category_root_data_binary_path),
					RootUtils.GetSuBinaryPath());
				data.Level = CheckStatus.Dangerous;
			}
			else
			{
				data.Status = _mContext.GetString(Resource.String.category_root_status_no_root);
				data.Data = null;
				data.Level = CheckStatus.Normal;
			}

			return data;
		}

		public static CategoryData GetLocationData(CategoryData data)
		{
			var status = _mContext.GetString(Resource.String.category_info_permissions_denied);
			Log.Debug("AbLocationDat", "den");
			data.Title = _mContext.GetString(Resource.String.category_location);
			data.RequiredPermissions = new[]
			{
				Manifest.Permission.AccessFineLocation,
				Manifest.Permission.AccessCoarseLocation
			};
			if (data.Level != CheckStatus.PermissionsRequired)
			{
				var locationManager = LocationManager.FromContext(_mContext);

				var locationEnabled = false;

				if (Build.VERSION.SdkInt >= BuildVersionCodes.P)
					locationEnabled = locationManager.IsLocationEnabled;
				else
					foreach (var provider in locationManager.AllProviders)
						if (provider != LocationManager.PassiveProvider && locationManager.IsProviderEnabled(provider))
						{
							locationEnabled = true;
							break;
						}

				status = locationEnabled
					? _mContext.GetString(Resource.String.category_location_tracking_enabled)
					: _mContext.GetString(Resource.String.category_location_tracking_disabled);
				Log.Debug("AbLocationDat", status);
				SetLocationTrackingEnabled(locationEnabled);
			}

			data.DataFilePath = LocationUtils.LocationCoordinatesFile;
			data.Status = status;
			return data;
		}

		public static CategoryData GetSystemData(CategoryData data)
		{
			data.Title = _mContext.GetString(Resource.String.category_system);
			if (SystemModListenerService.Logger.Length > 0)
			{
				data.Data = SystemModListenerService.Logger;
				data.Status = _mContext.GetString(Resource.String.category_system_modifications_detected);
				data.Level = CheckStatus.Dangerous;
			}
			else
			{
				data.Status = _mContext.GetString(Resource.String.category_system_no_modifications);
				data.Level = CheckStatus.Normal;
			}

			data.DataFilePath = SystemModListenerService.LogFile;

			if (new File(_mContext.FilesDir, data.DataFilePath).Exists())
			{
				using (var reader = new StreamReader(
					_mContext.OpenFileInput(data.DataFilePath)))
				{
					var text = reader.ReadToEnd();

					if (text.Length > 0)
					{
						data.Data = text.Split("\n").Last(s => s.Length > 0);
						data.Status = _mContext.GetString(Resource.String.category_system_modifications_detected);
						data.Level = CheckStatus.Dangerous;
					}
					else
					{
						data.Data = null;
						data.Status = _mContext.GetString(Resource.String.category_system_no_modifications);
						data.Level = CheckStatus.Normal;
					}
				}
			}
			else
			{
				data.Data = null;
				data.Status = _mContext.GetString(Resource.String.category_system_no_modifications);
				data.Level = CheckStatus.Normal;
			}

			return data;
		}

		public static CategoryData GetScreenData(CategoryData data)
		{
			data.Title = _mContext.GetString(Resource.String.category_screen);
			int tmpUnlocks;
			if (ScreenUnlockReceiver.UnlockedTimes < (tmpUnlocks = _mPreferences.GetInt(
				    ScreenUtils.UnlocksToday, 0)))
				ScreenUnlockReceiver.UnlockedTimes = tmpUnlocks;

			if (ScreenUnlockReceiver.IsNormal)
			{
				data.Status = _mContext.GetString(Resource.String.category_general_status_normal);
				data.Level = CheckStatus.Normal;
				data.Data = string.Format(_mContext.GetString(Resource.String.category_screen_data_normal),
					ScreenUnlockReceiver.UnlockedTimes);
			}
			else
			{
				data.Status = _mContext.GetString(Resource.String.category_general_status_questionably);
				data.Level = CheckStatus.Warning;
				data.Data = string.Format(_mContext.GetString(Resource.String.category_screen_data_questionably),
					ScreenUnlockReceiver.UnlockedTimes, ScreenUnlockReceiver.NormalCount);
//				data.Data = $"{ScreenUnlockReceiver.UnlockedTimes} unlocks this day, " +
//				            $"normal value {ScreenUnlockReceiver.NormalCount}";
			}

			return data;
		}

		public static CategoryData GetPhoneData(CategoryData data)
		{
			data.Title = _mContext.GetString(Resource.String.category_phone);
			data.RequiredPermissions = new[]
			{
				Manifest.Permission.ReadPhoneState,
				Manifest.Permission.ProcessOutgoingCalls
			};
			data.DataFilePath = PhoneUtils.SuspiciousCallsFile;

			if (new File(_mContext.FilesDir, data.DataFilePath).Exists())
			{
				using (var reader = new StreamReader(
					_mContext.OpenFileInput(data.DataFilePath)))
				{
					var text = reader.ReadToEnd();

					if (text.Length > 0)
					{
						data.Data = text.Split("\n").Last(s => s.Length > 0);
						data.Status = _mContext.GetString(Resource.String.category_general_status_suspicious_activity);
						data.Level = CheckStatus.Dangerous;
					}
					else
					{
						data.Data = null;
						data.Status = _mContext.GetString(Resource.String.category_general_status_normal);
						data.Level = CheckStatus.Normal;
					}
				}
			}
			else
			{
				data.Data = null;
				data.Status = _mContext.GetString(Resource.String.category_general_status_normal);
				data.Level = CheckStatus.Normal;
			}


			return data;
		}

		public static CategoryData GetSmsData(CategoryData data)
		{
			data.Title = _mContext.GetString(Resource.String.category_sms);
			data.RequiredPermissions = new[]
			{
				Manifest.Permission.ReceiveSms
			};
			data.DataFilePath = PhoneUtils.SuspiciousSmsFile;

			if (new File(_mContext.FilesDir, data.DataFilePath).Exists())
			{
				using (var reader = new StreamReader(
					_mContext.OpenFileInput(data.DataFilePath)))
				{
					var text = reader.ReadToEnd();

					if (text.Length > 0)
					{
						data.Data = text.Split("\n").Last(s => s.Length > 0);
						data.Status = _mContext.GetString(Resource.String.category_general_status_suspicious_activity);
						data.Level = CheckStatus.Dangerous;
					}
					else
					{
						data.Data = null;
						data.Status = _mContext.GetString(Resource.String.category_general_status_normal);
						data.Level = CheckStatus.Normal;
					}
				}
			}
			else
			{
				data.Data = null;
				data.Status = _mContext.GetString(Resource.String.category_general_status_normal);
				data.Level = CheckStatus.Normal;
			}

			return data;
		}

		#endregion

		#region Normalizers

		public static void NormalizeSystemData(string path, string ev)
		{
			using (var writer = new StreamWriter(_mContext.OpenFileOutput(
				SystemModListenerService.ExcludedFiles, FileCreationMode.Append)))
			{
				writer.WriteLine($"{path}____{ev}");
			}
		}

		public static void NormalizeScreenData(Intent intent)
		{
			foreach (var key in intent.Extras.KeySet())
			{
				var val = intent.GetIntExtra(key, -1);
				if (val != -1 && key != NotificationSender.ExtraNotificationId)
				{
					_mPreferences.Edit().PutInt(key, val).Apply();
					Log.Debug("ScreenNormalizer", $"{key} : {val}");
				}
			}
		}

		[SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
		public static void NormalizeLocationData(Intent intent)
		{
			if (!intent.HasExtra(LocationUtils.LocationLatitude) ||
			    !intent.HasExtra(LocationUtils.LocationLongitude))
				return;

			using (var writer =
				new StreamWriter(_mContext.OpenFileOutput(LocationUtils.LocationCoordinatesFile,
					FileCreationMode.Append)))
			{
				var latDouble = intent.GetDoubleExtra(LocationUtils.LocationLatitude, -500);
				var longDouble = intent.GetDoubleExtra(LocationUtils.LocationLongitude, -500);

				if (latDouble != -500 && longDouble != -500)
				{
					writer.WriteLine(
						$"Latitude = {latDouble}, Longitude = {longDouble}");
					CategoriesDataDic[LocationCategory].Level = CheckStatus.Normal;
				}
			}
		}

		public static void NormalizePhoneData(Intent intent)
		{
			var code = intent.GetIntExtra(PhoneUtils.CountryCodeKey, -1);
			if (code == -1) return;

			var path = intent.GetBooleanExtra(PhoneUtils.IsOutgoingKey, true)
				? PhoneUtils.ExcludedOutCountryCodesFile
				: PhoneUtils.ExcludedInCountryCodesFile;
			using (var writer = new StreamWriter(_mContext.OpenFileOutput(path, FileCreationMode.Append)))
			{
				writer.WriteLine($"{code}");
			}
		}

		public static void NormalizeSmsData(Intent intent)
		{
			var code = intent.GetIntExtra(PhoneUtils.CountryCodeKey, -1);
			if (code == -1) return;

			var path = PhoneUtils.ExcludedInCountryCodesFile;
			using (var writer = new StreamWriter(_mContext.OpenFileOutput(path, FileCreationMode.Append)))
			{
				writer.WriteLine($"{code}");
			}
		}

		#endregion
	}
}