using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using AbnormalChecker.BroadcastReceivers;
using AbnormalChecker.Extensions;
using AbnormalChecker.Services;
using AbnormalChecker.Utils;
using Android;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Common;
using Android.Locations;
using Android.OS;
using Android.Preferences;
using Android.Support.V4.Content;
using Android.Util;
using Java.Util.Concurrent;
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

		public static readonly Dictionary<string, CategoryData> CategoriesDictionary =
			new Dictionary<string, CategoryData>();

		private static Context _mContext;
		private static ISharedPreferences _mPreferences;

		private static string[] _allCategories;

		public static ICollection<string> GetSelectedCategories()
		{
			return _mPreferences.GetStringSet("selected_categories", _allCategories);
		}

		public static bool IsSelectedCategory(string category)
		{
			var collection = GetSelectedCategories();
			return collection != null && collection.Contains(category);
		}

		public static void Refresh()
		{
			var sel = GetSelectedCategories();
			if (sel == null) return;

			foreach (var category in sel)
			{
				var categoryData = CategoriesDictionary[category];
				categoryData.Update();
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
			LocationUtils.Init(context);
			_allCategories = context.Resources.GetStringArray(Resource.Array.categories_values);
			foreach (var category in _allCategories)
				CategoriesDictionary[category] =
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

		public class CategoryData
		{
			private readonly CategoryUpdater dataUpdater;
			public string Data;
			public string DataFilePath;
			private CheckStatus level = CheckStatus.Normal;

			private string[] requiredPermissions;
			private string status = "OK";
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
						status = _mContext.GetString(Resource.String.category_info_permissions_denied);
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
						level = CheckStatus.PermissionsRequired;
						status = _mContext.GetString(Resource.String.category_info_permissions_denied);
					}
					else if (level == CheckStatus.PermissionsRequired)
					{
						level = CheckStatus.Normal;
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
			data.Title = _mContext.GetString(Resource.String.category_location);
			data.RequiredPermissions = new[]
			{
				Manifest.Permission.AccessFineLocation,
				Manifest.Permission.AccessCoarseLocation
			};
			data.DataFilePath = LocationUtils.LocationCoordinatesFile;
			if (GoogleApiAvailability.Instance.IsGooglePlayServicesAvailable(_mContext) != ConnectionResult.Success)
			{
				data.Status =
					_mContext.GetString(Resource.String.category_location_status_no_play_services);
				return data;
			}

			if (data.Level != CheckStatus.PermissionsRequired)
			{
				var locationManager = LocationManager.FromContext(_mContext);

				var locationEnabled = false;

				if (IsSelectedCategory(LocationCategory))
				{
					if (Build.VERSION.SdkInt >= BuildVersionCodes.P)
						locationEnabled = locationManager.IsLocationEnabled;
					else
						foreach (var provider in locationManager.AllProviders)
							if (provider != LocationManager.PassiveProvider &&
							    locationManager.IsProviderEnabled(provider))
							{
								locationEnabled = true;
								break;
							}
				}

				status = locationEnabled
					? _mContext.GetString(Resource.String.category_location_tracking_enabled)
					: _mContext.GetString(Resource.String.category_location_tracking_disabled);

				if (!locationEnabled) data.Data = null;
				LocationUtils.SetLocationTrackingEnabled(locationEnabled);
			}

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

					try
					{
						data.Data = text.Split("\n").Last(s => s.Length > 0);
						data.Status = _mContext.GetString(Resource.String.category_system_modifications_detected);

						data.Level = CheckStatus.Dangerous;
					}
					catch (Exception)
					{
						data.Data = null;
						data.DataFilePath = null;
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


			switch (ScreenUnlockReceiver.Status)
			{
				case ScreenUnlockReceiver.ScreenStatus.OK:
					data.Status = _mContext.GetString(Resource.String.category_general_status_normal);
					data.Level = CheckStatus.Normal;
					data.Data = string.Format(_mContext.GetString(Resource.String.category_screen_data_normal),
						ScreenUnlockReceiver.UnlockedTimes);
					break;
				case ScreenUnlockReceiver.ScreenStatus.Many:
					data.Status = _mContext.GetString(Resource.String.category_general_status_questionably);
					data.Level = CheckStatus.Warning;
					data.Data = string.Format(_mContext.GetString(Resource.String.category_screen_data_questionably),
						ScreenUnlockReceiver.UnlockedTimes, ScreenUnlockReceiver.NormalCount);
					break;
				case ScreenUnlockReceiver.ScreenStatus.Speed:
					data.Status = _mContext.GetString(Resource.String.category_general_status_questionably);
					data.Level = CheckStatus.Warning;
					data.Data = string.Format(_mContext.GetString(Resource.String.category_screen_notif_west_fast_hand),
						ScreenUnlockReceiver._unlockMillis.Count,
						TimeUnit.Milliseconds.ToSeconds(ScreenUnlockReceiver._unlockMillis.Last() -
						                                ScreenUnlockReceiver._unlockMillis.First()));
					break;
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

					try
					{
						data.Data = text.Split("\n").Last(s => s.Length > 0);
						data.Status = _mContext.GetString(Resource.String.category_general_status_suspicious_activity);
						data.Level = CheckStatus.Dangerous;
					}
					catch (Exception)
					{
						data.Data = null;
						data.DataFilePath = null;
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

					try
					{
						data.Data = text.Split("\n").Last(s => s.Length > 0);
						data.Status = _mContext.GetString(Resource.String.category_general_status_suspicious_activity);
						data.Level = CheckStatus.Dangerous;
					}
					catch (Exception)
					{
						data.Data = null;
						data.DataFilePath = null;
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

			string[] text;
			using (var reader = new StreamReader(_mContext.OpenFileInput(SystemModListenerService.LogFile)))
			{
				text = reader.ReadToEnd().Split("\n");
			}

			using (var writer =
				new StreamWriter(_mContext.OpenFileOutput(SystemModListenerService.LogFile, FileCreationMode.Private)))
			{
				foreach (var line in text)
					if (line.Length > 0 && line != "\r" && !line.Contains($"{ev}") && !line.Contains($"{path}"))
						writer.WriteLine(line);
			}
		}

		public static void NormalizeScreenData(Intent intent)
		{
			foreach (var key in intent.Extras.KeySet())
			{
				var val = intent.GetIntExtra(key, -1);
				if (val != -1 && key != NotificationSender.ExtraNotificationId)
					_mPreferences.Edit().PutInt(key, val).Apply();
			}
			ScreenUnlockReceiver.UpdateStatus();
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
					CategoriesDictionary[LocationCategory].Level = CheckStatus.Normal;
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

			string[] text;
			using (var reader = new StreamReader(_mContext.OpenFileInput(PhoneUtils.SuspiciousCallsFile)))
			{
				text = reader.ReadToEnd().Split("\n");
			}

			using (var writer =
				new StreamWriter(_mContext.OpenFileOutput(PhoneUtils.SuspiciousCallsFile, FileCreationMode.Private)))
			{
				foreach (var line in text)
					if (line.Length > 0 && line != "\r" && !line.Contains($"+{code}"))
						writer.WriteLine(line);
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

			string[] text;
			using (var reader = new StreamReader(_mContext.OpenFileInput(PhoneUtils.SuspiciousSmsFile)))
			{
				text = reader.ReadToEnd().Split("\n");
			}

			using (var writer =
				new StreamWriter(_mContext.OpenFileOutput(PhoneUtils.SuspiciousSmsFile, FileCreationMode.Private)))
			{
				foreach (var line in text)
					if (line.Length > 0 && line != "\r" && !line.Contains($"+{code}"))
						writer.WriteLine(line);
			}
		}

		#endregion
	}
}