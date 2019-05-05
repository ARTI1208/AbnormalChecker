using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Reflection;
using AbnormalChecker.Activities;
using AbnormalChecker.BroadcastReceivers;
using AbnormalChecker.Extensions;
using AbnormalChecker.Services;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Locations;
using Android.Preferences;
using Android.Support.V4.Content;
using Java.Util;
using Permission = Android.Content.PM.Permission;
using Android.Gms.Location;
using Android.OS;
using Android.Util;
using Java.IO;
using Java.Lang;
using Java.Text;
using Console = System.Console;
using File = Java.IO.File;
using String = System.String;
using TimeZone = Java.Util.TimeZone;

namespace AbnormalChecker
{
	public delegate DataHolder.CategoryData CategoryUpdater(DataHolder.CategoryData data);

	[SuppressMessage("ReSharper", "UnusedMember.Global")]
	public class DataHolder
	{
		public static Dictionary<string, CategoryUpdater> Dictionary = new Dictionary<string, CategoryUpdater>();
		public static Dictionary<string, CategoryUpdater> Normalizer = new Dictionary<string, CategoryUpdater>();

		public static readonly Dictionary<string, CategoryData> CategoriesDataDic =
			new Dictionary<string, CategoryData>();


		private const string TAG = nameof(DataHolder);

		public static Dictionary<string, string[]> Permissions = new Dictionary<string, string[]>();
		public static string RootCategory = "root";
		public const string ScreenLocksCategory = "screen";
		public const string LocationCategory = "location";
		public const string SystemCategory = "system";
		public const string ScreenCategory = "screen";
		public const string PhoneCategory = "phone";
		public const string SmsCategory = "sms";

		private static Context _mContext;
		private static ISharedPreferences _mPreferences;
		static FusedLocationProviderClient _fusedLocationProviderClient;
		private static string[] _allCategories;
		private static PendingIntent _locationPendingIntent;
		private static readonly AbnormalLocationCallback Callback = new AbnormalLocationCallback();
		public static Location PreviousLocation;

		public class CategoryData
		{
			public CategoryData(string title, CategoryUpdater updater)
			{
				Title = title;
				_dataUpdater = updater;
			}

			public void Update()
			{
				_dataUpdater?.Invoke(this);
			}

			public string DataFilePath;
			public string Title;

			public string Status
			{
				get => _status;
				set
				{
					if (_requiredPermissions != null && _requiredPermissions.Any(s => ContextCompat.CheckSelfPermission(_mContext, s) == Permission.Denied))
					{
						_level = CheckStatus.PermissionsRequired;
						_status = _mContext.GetString(Resource.String.category_info_permissions_denied);
					}
					else
					{
						_status = value;
					}
				}
			}
			private string _status = "OK";
			public string Data;

			public CheckStatus Level
			{
				get => _level;
				set
				{
					if (_requiredPermissions != null && _requiredPermissions.Any(s => ContextCompat.CheckSelfPermission(_mContext, s) == Permission.Denied))
					{
						_level = CheckStatus.PermissionsRequired;
						Status = _mContext.GetString(Resource.String.category_info_permissions_denied);
					}
					else
					{
						_level = value;
					}
				}
			}
			private CheckStatus _level = CheckStatus.Normal;

			public string[] RequiredPermissions
			{
				get => _requiredPermissions;
				set {
					if (value.Any(s => ContextCompat.CheckSelfPermission(_mContext, s) == Permission.Denied))
					{
						Level = CheckStatus.PermissionsRequired;
						Status = _mContext.GetString(Resource.String.category_info_permissions_denied);
					}
					_requiredPermissions = value;
				}
			}

			private string[] _requiredPermissions;
			
			private readonly CategoryUpdater _dataUpdater;
		}

		public enum CheckStatus
		{
			Normal,
			Warning,
			Dangerous,
			PermissionsRequired
		}

		enum DelegateType
		{
			Updater,
			Normalizer
		}

		#region Updaters

		public static CategoryData GetRootData(CategoryData data)
		{
			data.Title = _mContext.GetString(Resource.String.category_root);
			if (RootChecker.IsRooted(_mContext))
			{
				data.Status = _mContext.GetString(Resource.String.category_root_status_root_access);
				data.Data = string.Format(_mContext.GetString(Resource.String.category_root_data_binary_path),
					RootChecker.GetSuBinaryPath());
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
			string status = _mContext.GetString(Resource.String.category_info_permissions_denied);
			data.Title = _mContext.GetString(Resource.String.category_location);
			data.RequiredPermissions = new[]
			{
				Manifest.Permission.AccessFineLocation,
				Manifest.Permission.AccessCoarseLocation
			};
//			data.Level =
//				data.RequiredPermissions.Any(s => ContextCompat.CheckSelfPermission(_mContext, s) == Permission.Denied)
//					? CheckStatus.PermissionsRequired
//					: CheckStatus.Normal;
			if (data.Level != CheckStatus.PermissionsRequired)
			{
				LocationManager locationManager = LocationManager.FromContext(_mContext);

				bool locationEnabled = false;

				if (Build.VERSION.SdkInt >= BuildVersionCodes.P)
				{
					locationEnabled = locationManager.IsLocationEnabled;
				}
				else
				{
					foreach (var provider in locationManager.AllProviders)
					{
						if (provider != LocationManager.PassiveProvider && locationManager.IsProviderEnabled(provider))
						{
							locationEnabled = true;
							break;
						}
					}

				}

				status = locationEnabled
					? _mContext.GetString(Resource.String.category_location_tracking_enabled)
					: _mContext.GetString(Resource.String.category_location_tracking_disabled);
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
				using (StreamReader reader = new StreamReader(
					_mContext.OpenFileInput(data.DataFilePath)))
				{
					string text = reader.ReadToEnd();

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
			{
				ScreenUnlockReceiver.UnlockedTimes = tmpUnlocks;
			}

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
				using (StreamReader reader = new StreamReader(
					_mContext.OpenFileInput(data.DataFilePath)))
				{
					string text = reader.ReadToEnd();

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
				Manifest.Permission.ReceiveSms,
				Manifest.Permission.BroadcastSms,
				Manifest.Permission.ReadSms
			};
			data.DataFilePath = PhoneUtils.SuspiciousSmsFile;
			
			if (new File(_mContext.FilesDir, data.DataFilePath).Exists())
			{
				using (StreamReader reader = new StreamReader(
					_mContext.OpenFileInput(data.DataFilePath)))
				{
					string text = reader.ReadToEnd();

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
			using (StreamWriter writer = new StreamWriter(_mContext.OpenFileOutput(
				SystemModListenerService.ExcludedFiles, FileCreationMode.Append)))
			{
				writer.WriteLine($"{path}____{ev}");
			}
		}

		public static void NormalizeScreenData(Intent intent)
		{
			foreach (var key in intent.Extras.KeySet())
			{
				int val = intent.GetIntExtra(key, -1);
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
			{
				return;
			}

//			using (FileWriter writer =
//				new FileWriter(new File(_mContext.FilesDir, LocationUtils.LocationCoordinatesFile)))
//			{
//				writer.Write(intent.GetStringExtra(LocationUtils.LocationLatitude));
//				writer.Write("____");
//				writer.Write(intent.GetStringExtra(LocationUtils.LocationLongitude));
//				writer.Write("\n");
//			}

			using (StreamWriter writer =
				new StreamWriter(_mContext.OpenFileOutput(LocationUtils.LocationCoordinatesFile,
					FileCreationMode.Append)))
			{
				double latDouble = intent.GetDoubleExtra(LocationUtils.LocationLatitude, -500);
				double longDouble = intent.GetDoubleExtra(LocationUtils.LocationLongitude, -500);

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
			int code = intent.GetIntExtra(PhoneUtils.CountryCodeKey, -1);
			if (code == -1)
			{
				return;
			}

			string path = intent.GetBooleanExtra(PhoneUtils.IsOutgoingKey, true)
				? PhoneUtils.ExcludedOutCountryCodesFile
				: PhoneUtils.ExcludedInCountryCodesFile;
			using (StreamWriter writer = new StreamWriter(_mContext.OpenFileOutput(path, FileCreationMode.Append)))
			{
				writer.WriteLine($"{code}");
			}
		}

		#endregion

		public static ICollection<string> GetSelectedCategories()
		{
			return _mPreferences.GetStringSet("selected_categories", _allCategories);
		}

		public static bool IsSelectedCategory(string category)
		{
			return GetSelectedCategories().Contains(category);
		}

		public void Refresh()
		{
			ICollection<string> sel = GetSelectedCategories();
			if (sel == null)
			{
				return;
			}

			foreach (var category in sel)
			{
				CategoryData categoryData = CategoriesDataDic[category];
				categoryData.Update();
			}
		}

		public static async void GetLastLocationFromDevice()
		{
			Location location = await _fusedLocationProviderClient.GetLastLocationAsync();
			if (location == null)
			{
				return;
			}

			float[] res = new float[2];
			bool bigDestination = true;
			bool entered = false;
			if (new File(_mContext.FilesDir, LocationUtils.LocationCoordinatesFile).Exists())
			{
				using (StreamReader reader =
					new StreamReader(_mContext.OpenFileInput(LocationUtils.LocationCoordinatesFile)))
				{
					foreach (var line in reader.ReadToEnd().Split("\n"))
					{
						if (!line.Contains("Latitude"))
						{
							continue;
						}

						string latString = line.Substring(line.AfterIndex("Latitude = "),
							line.IndexOf(", Long") - line.AfterIndex("Latitude = "));
						string longString = line.Substring(line.AfterIndex("Longitude = "));
						double latDouble;
						double longDouble;
						try
						{
							latDouble = double.Parse(latString);
							longDouble = double.Parse(longString);
						}
						catch (System.Exception)
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
				bool notPrevious = true;
				if (PreviousLocation != null)
				{
					Location.DistanceBetween(PreviousLocation.Latitude,
						PreviousLocation.Longitude, location.Latitude, location.Longitude, res);
					if (res[0] < 100)
					{
						notPrevious = false;
					}
				}

				if (notPrevious)
				{
					NotificationSender sender = new NotificationSender(_mContext, LocationCategory);
					sender.PutNormalizeExtra(LocationUtils.LocationLatitude, location.Latitude);
					sender.PutNormalizeExtra(LocationUtils.LocationLongitude, location.Longitude);
					sender.Send(NotificationSender.NotificationType.WarningNotification,
						$"Too big distance from known location = {res[0] / 1000}km !");
					PreviousLocation = location;
					CategoriesDataDic[LocationCategory].Level = CheckStatus.Warning;
				}
			}

			if (!new File(_mContext.FilesDir, LocationUtils.LocationCoordinatesFile).Exists())
			{
				using (StreamWriter writer =
					new StreamWriter(_mContext.OpenFileOutput(LocationUtils.LocationCoordinatesFile,
						FileCreationMode.Private)))
				{
					writer.WriteLine("Known locations:");
					writer.WriteLine($"Latitude = {location.Latitude}, Longitude = {location.Longitude}");
				}
			}

			CategoriesDataDic[LocationCategory].Data = FormatLocation(location);
			MainActivity.adapter?.NotifyDataSetChanged();
		}

		public static void SetLocationTrackingEnabled(bool enabled)
		{
			if (enabled)
			{
				LocationRequest request = new LocationRequest();
				request.SetInterval(5000);
				request.SetFastestInterval(5000);
				request.SetPriority(LocationRequest.PriorityHighAccuracy);

				//Probably not needed
				_fusedLocationProviderClient.RequestLocationUpdates(request, Callback, Looper.MainLooper);

				if (_locationPendingIntent == null)
				{
					Intent intent = new Intent(_mContext, typeof(LocationUpdateReceiver));
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

		public static String[] GetAllRequiredPermissions(Context context)
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
		}

		public DataHolder(Context context)
		{
			_mContext = context;
			_mPreferences = PreferenceManager.GetDefaultSharedPreferences(_mContext);
			_fusedLocationProviderClient = LocationServices.GetFusedLocationProviderClient(_mContext);
			_allCategories = context.Resources.GetStringArray(Resource.Array.categories_values);
			foreach (var category in _allCategories)
			{
				CategoriesDataDic[category] =
					new CategoryData(category.MakeFirstUpper(), CreateCategoryDelegate(category));
			}
		}

		private static CategoryUpdater CreateCategoryDelegate(string category, DelegateType type = DelegateType.Updater)
		{
			string methodName;
			switch (type)
			{
				case DelegateType.Normalizer:
					methodName = $"Normalize{category.First().ToString().ToUpper()}{category.Substring(1)}Data";
					break;
				default:
					methodName = $"Get{category.First().ToString().ToUpper()}{category.Substring(1)}Data";
					break;
			}

			MethodInfo methodInfo = typeof(DataHolder).GetMethod(methodName, BindingFlags.Public | BindingFlags.Static,
				null,
				new[] {typeof(CategoryData)}, null);

			if (methodInfo == null)
			{
				Log.Error(nameof(DataHolder), "Cannot find category updater method");
				return null;
			}
			CategoryUpdater categoryDelegate = Delegate.CreateDelegate(typeof(CategoryUpdater),
				methodInfo) as CategoryUpdater;
			return categoryDelegate;
		}

		public static string FormatLocation(Location location)
		{
			if (location == null)
				return "";
			DateFormat dateFormatter = DateFormat.DateTimeInstance;
			return
				string.Format(_mContext.GetString(Resource.String.category_location_data), location.Latitude,
					location.Longitude, dateFormatter.Format(new Date(location.Time)));
		}
	}
}