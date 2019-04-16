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
using Java.Lang;
using File = Java.IO.File;
using String = System.String;

namespace AbnormalChecker
{
	
	public delegate DataHolder.CategoryData CategoryUpdater(DataHolder.CategoryData data);

	[SuppressMessage("ReSharper", "UnusedMember.Global")]
	public class DataHolder
	{
		public static Dictionary<string, CategoryUpdater> Dictionary = new Dictionary<string, CategoryUpdater>();
		public static Dictionary<string, CategoryUpdater> Normalizer = new Dictionary<string, CategoryUpdater>();
		public static readonly Dictionary<string, CategoryData> CategoriesDataDic = new Dictionary<string, CategoryData>();
		public static Dictionary<string, string[]> Permissions = new Dictionary<string, string[]>();
		public static string RootCategory = "root";
		public const string ScreenLocksCategory = "screen";
		public static string LocationCategory = "location";
		public const string SystemCategory = "system";
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
			public string Status = "OK";
			public string Data;
			public CheckStatus Level = CheckStatus.Normal;
			public string[] RequiredPermissions;
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
			if (Checker.IsRooted(_mContext))
			{
				data.Status = "Device has root access";
				data.Data = $"Found su binary at {Checker.GetSuBinaryPath()}";
				data.Level = CheckStatus.Dangerous;
			}
			else
			{
				data.Status = "Device is not rooted";
			}

			return data;
		}

		public static CategoryData GetLocationData(CategoryData data)
		{
			string status = "Permissions denied";
			data.RequiredPermissions = new[]
			{
				Manifest.Permission.AccessFineLocation,
				Manifest.Permission.AccessCoarseLocation
			};
			data.Level =
				data.RequiredPermissions.Any(s => ContextCompat.CheckSelfPermission(_mContext, s) == Permission.Denied)
					? CheckStatus.PermissionsRequired
					: CheckStatus.Normal;
			if (ContextCompat.CheckSelfPermission(_mContext, Manifest.Permission.AccessFineLocation) ==
			    Permission.Granted)
			{
				
				
				LocationManager locationManager = (LocationManager) 
					_mContext.GetSystemService(Context.LocationService);

				bool locationEnabled = false;

				if (Build.VERSION.SdkInt >= BuildVersionCodes.P)
				{
					locationEnabled = locationManager.IsLocationEnabled;
					Log.Debug("Locatinch", locationEnabled.ToString());
				}
				else
				{
					foreach (var provider in locationManager.AllProviders)
					{
						if (provider != LocationManager.PassiveProvider && locationManager.IsProviderEnabled(provider))
						{
							locationEnabled = true;
							Log.Debug("Locatinch", provider);
							break;
						}	
					}
					
					
					
					Log.Debug("Locatinch", locationEnabled.ToString());
				}
				
				status = locationEnabled ? "Location tracking enabled" : "Tracking disabled";
				
				LocationRequest request = new LocationRequest();
				request.SetInterval(5000);
				request.SetFastestInterval(5000);
				request.SetPriority(LocationRequest.PriorityHighAccuracy);
				//Probably not needed
				_fusedLocationProviderClient.RequestLocationUpdates(request, Callback , Looper.MyLooper());

				if (_locationPendingIntent == null)
				{
					Intent intent = new Intent(_mContext, typeof(LocationUpdateReceiver));
					_locationPendingIntent = PendingIntent.GetBroadcast(_mContext, 123, intent, PendingIntentFlags.UpdateCurrent);
					_fusedLocationProviderClient.RequestLocationUpdates(request, _locationPendingIntent);
				}
			}

			data.Status = status;
			return data;
		}

		public static CategoryData GetSystemData(CategoryData data)
		{
			data.Title = "System Modification";
			if (SystemModListenerService.Logger.Length > 0)
			{
				data.Data = SystemModListenerService.Logger;
				data.Status = "Modifications detected!";
				data.Level = CheckStatus.Dangerous;
			}
			else
			{
				data.Status = "No modifications detected";
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
						data.Status = "Modifications detected!";
						data.Level = CheckStatus.Dangerous;
					}
					else
					{
						data.Data = null;
						data.Status = "No modifications detected";
						data.Level = CheckStatus.Normal;
					}
				}
			}
			else
			{
				data.Data = null;
				data.Status = "No modifications detected";
				data.Level = CheckStatus.Normal;
			}

			return data;
		}

		public static CategoryData GetScreenData(CategoryData data)
		{
			int tmpUnlocks;
			if (ScreenUnlockReceiver.UnlockedTimes < (tmpUnlocks = _mPreferences.GetInt(
				    ScreenUnlockReceiver.UnlocksToday, 0)))
			{
				ScreenUnlockReceiver.UnlockedTimes = tmpUnlocks;
			}
			if (ScreenUnlockReceiver.IsNormal)
			{
				data.Status = "Normal";
				data.Level = CheckStatus.Normal;
				data.Data = $"{ScreenUnlockReceiver.UnlockedTimes} unlocks this day";
			}
			else
			{
				data.Status = "Questionably";
				data.Level = CheckStatus.Warning;
				data.Data = $"{ScreenUnlockReceiver.UnlockedTimes} unlocks this day, " +
				            $"normal value {ScreenUnlockReceiver.NormalCount}";
			}
			return data;
		}

		public static CategoryData GetPhoneData(CategoryData data)
		{
			data.Title = "Phone calls";
			data.RequiredPermissions = new[]
			{
				Manifest.Permission.ReadPhoneState,
				Manifest.Permission.ProcessOutgoingCalls
			};
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
				Log.Error("opiom", "unknown loc");
			}
			else
			{
				
				if (PreviousLocation != null)
				{
					float[] res = new float[2];
					Location.DistanceBetween(PreviousLocation.Latitude,
						PreviousLocation.Longitude, location.Latitude, location.Longitude, res);
					Log.Debug("AbnormalLocationResolver", $"Distance = {res[0]}");
					if (res[0] > 10 * 1000)
					{
						NotificationSender sender = new NotificationSender(_mContext, LocationCategory);
						sender.Send(NotificationSender.NotificationType.WarningNotification, 
							$"Too big distance from previous point = {res[0] / 1000}km !");
					}
				}

				PreviousLocation = location;
				
				CategoriesDataDic["location"].Data = FormatLocation(location);
				MainActivity.adapter?.NotifyDataSetChanged();
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
				                        new[] {typeof(CategoryData)}, null) ??
			                        throw new NullPointerException(
				                        $"Cannot find method {methodName} in class {typeof(DataHolder)}");
			CategoryUpdater categoryDelegate = Delegate.CreateDelegate(typeof(CategoryUpdater),
				methodInfo) as CategoryUpdater;
			return categoryDelegate;
		}

		public static String FormatLocation(Location location)
		{
			if (location == null)
				return "";
			return
				$"Coordinates: lat = {location.Latitude:F3}, lon = {location.Longitude:F3}, time = {new Date(location.Time)}";
		}
	}
}