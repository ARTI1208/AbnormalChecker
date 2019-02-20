using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AbnormalChecker.Activities;
using AbnormalChecker.BroadcastReceivers;
using AbnormalChecker.Extensions;
using AbnormalChecker.Services;
using Android;
using Android.Content;
using Android.Content.PM;
using Android.Locations;
using Android.Preferences;
using Android.Support.V4.Content;
using Java.Text;
using Java.Util;
using Permission = Android.Content.PM.Permission;
using Android.Gms.Common;
using Android.Gms.Location;
using Android.OS;
using Android.Util;
using Java.Lang;
using Object = Java.Lang.Object;
using String = System.String;

namespace AbnormalChecker
{
    public delegate DataHolder.CategoryData CategoryDataDel(DataHolder.CategoryData data);

    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class DataHolder
    {
        public static Dictionary<string, CategoryDataDel> dictionary = new Dictionary<string, CategoryDataDel>();
        public static Dictionary<string, CategoryData> CategoriesDataDic = new Dictionary<string, CategoryData>();
        public static Dictionary<string, string[]> Permissions = new Dictionary<string, string[]>();
        public static string RootCategory = "Root";
        public static string ScreenLocksCategory = "ScreenLocks";
        public static string LocationCategory = "Location";
        private static Context mContext;
        private static ISharedPreferences mPreferences;
        
//        public class CategoryParcel : Java.Lang.Object, IParcelable
//        {
//
//            public CategoryData Data
//            {
//                get;
//                set;    
//            }
//            
//            public int DescribeContents()
//            {
//                throw new NotImplementedException();
//            }
//
//            public void WriteToParcel(Parcel dest, ParcelableWriteFlags flags)
//            {
//                
//                dest.WriteString(Data.Title);
//                dest.WriteString(Data.Status);
//                dest.WriteString(Data.Data);
//                dest.WriteStringArray(Data.RequiredPermissions);
//                
//                dest.WriteLong(User.Id);
//                dest.WriteString(User.FirstName);
//                dest.WriteString(User.LastName);
//                dest.WriteInt(User.Age);
//                dest.WriteString(User.Address);
//                dest.WriteString(User.City);
//                dest.WriteString(User.Country);
//            }
//        }
        
        public class CategoryData
        {
            public CategoryData(string title, CategoryDataDel updater)
            {
                Title = title;
                DataUpdater = updater;
            }
            
            

            public void Update()
            {
                DataUpdater?.Invoke(this);
            }

            public string Title;
            public string Status = "OK";
            public string Data;
            public CheckStatus Level = CheckStatus.Normal;
            public string[] RequiredPermissions;
            public CategoryDataDel DataUpdater;
        }

        public enum CheckStatus
        {
            Normal,
            Warning,
            Dangerous,
            PermissionsRequired
        }

        public static CategoryData GetRootData(CategoryData data)
        {
            if (Checker.IsRooted(mContext))
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
                data.RequiredPermissions.Any(s => ContextCompat.CheckSelfPermission(mContext, s) == Permission.Denied)
                    ? CheckStatus.PermissionsRequired
                    : CheckStatus.Normal;
            if (ContextCompat.CheckSelfPermission(mContext, Manifest.Permission.AccessFineLocation) ==
                Permission.Granted)
            {
                status = "Permissions granted";
                GetLastLocationFromDevice();
            }

            data.Status = status;
            return data;
        }

        public static CategoryData GetSystemData(CategoryData data)
        {
            data.Title = "System Modification";
            if (SystemModListenerService.Logger.Length > 0)
            {
                if (SystemModListenerService.Logger.Contains("\n"))
                {
                    data.Data = SystemModListenerService.Logger.Substring(SystemModListenerService.Logger.LastIndexOf(
                        "\n",
                        StringComparison.CurrentCultureIgnoreCase)).Trim();
                }
                else
                {
                    data.Data = SystemModListenerService.Logger;
                }

                data.Status = "Modifications detected!";
                data.Level = CheckStatus.Dangerous;
            }
            else
            {
                data.Status = "No modifications detected";
                data.Level = CheckStatus.Normal;
            }

            return data;
        }

        public ICollection<string> GetSelectedCategories()
        {
            return mPreferences.GetStringSet("selected_categories", _allCategories);
        }

        public static CategoryData GetScreenData(CategoryData data)
        {
            if (mPreferences.GetBoolean(Settings.ScreenLockAutoAdjustment, false))
            {
                data.Status = "Monitoring your regular activity";
                SimpleDateFormat dateFormat;
                Date now = new Date();
                Date monitoringStart = new Date(mPreferences.GetLong("auto_start_time", now.Time));
                int monitoringTime = mPreferences.GetInt(Settings.ScreenLockAutoAdjustmentDayCount, 1);
                Date monitoringStop = new Date(monitoringStart.Time + TimeSpan.FromDays(monitoringTime).Milliseconds);
                if (now.Time - monitoringStart.Time < TimeSpan.FromDays(1).Milliseconds)
                {
                    dateFormat = new SimpleDateFormat("kk:mm");
                }
                else
                {
                    dateFormat = new SimpleDateFormat("kk:mm, dd.MM.yyyy");
                }

                data.Data = $"{monitoringTime}-day " +
                            $"monitoring ends at {dateFormat.Format(monitoringStop)}";
            }
            else
            {
                data.Status = "Normal";
                data.Data = $"{AbnormalBroadcastReceiver.unlockedTimes} unlocks total";
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

//        public void Refresh(string category)
//        {
//            CategoryData categoryData = dictionary[category]?.Invoke();
//            if (categoryData != null)
//            {
//                    if (structs.ContainsKey(category))
//                    {
//                        structs[category] = categoryData;
//                    }
//                    else
//                    {
//                         structs.Add(category, categoryData);    
//                    }
//            }
//        }

        bool IsGooglePlayServicesInstalled()
        {
            var queryResult = GoogleApiAvailability.Instance.IsGooglePlayServicesAvailable(mContext);
            if (queryResult == ConnectionResult.Success)
            {
                Log.Info("MainActivity", "Google Play Services is installed on this device.");
                return true;
            }

            if (GoogleApiAvailability.Instance.IsUserResolvableError(queryResult))
            {
                // Check if there is a way the user can resolve the issue
                var errorString = GoogleApiAvailability.Instance.GetErrorString(queryResult);
                Log.Error("MainActivity", "There is a problem with Google Play Services on this device: {0} - {1}",
                    queryResult, errorString);

                // Alternately, display the error to the user.
            }

            return false;
        }

        static FusedLocationProviderClient fusedLocationProviderClient;

        static async void GetLastLocationFromDevice()
        {
            Location location = await fusedLocationProviderClient.GetLastLocationAsync();
            if (location == null)
            {
                Log.Error("opiom", "unknown loc");
            }
            else
            {
                CategoriesDataDic["location"].Data = formatLocation(location);
                MainActivity.adapter?.NotifyDataSetChanged();
            }
        }

        private static Dictionary<string, CategoryDataDel> UpdateDelegates(Context context)
        {
            if (_allCategories == null)
            {
                _allCategories = context.Resources.GetStringArray(Resource.Array.categories_values);
            }

            foreach (var category in _allCategories)
            {
                if (!dictionary.ContainsKey(category))
                {
                    dictionary.Add(category, CreateCategoryDelegate(category));
                }
            }

            return dictionary;
        }

        public static String[] GetAllRequiredPermissions(Context context)
        {
            mContext = context;
            return context
                .PackageManager
                .GetPackageInfo(context.PackageName, PackageInfoFlags.Permissions)
                .RequestedPermissions.ToArray();
        }


//        public static string[] GetAllRequiredPermissions(Context context)
//        {
//
//            return retrievePermissions(context);
//            
//            mContext = context;
//            mPreferences = PreferenceManager.GetDefaultSharedPreferences(mContext);
//            fusedLocationProviderClient = LocationServices.GetFusedLocationProviderClient(mContext);
//            _allCategories = context.Resources.GetStringArray(Resource.Array.categories_values);
//            UpdateDelegates(context);
//            foreach (var pair in dictionary)
//            {
//                if (!CategoriesDataDic.ContainsKey(pair.Key))
//                {
//                    CategoriesDataDic.Add(pair.Key, pair.Value?.Invoke());
//                }
//            }
//            List<string> permissions = new List<string>();
//            foreach (var category in CategoriesDataDic)
//            {
//                if (category.Value.RequiredPermissions != null)
//                {
//                    permissions.AddRange(category.Value.RequiredPermissions);
//                }
//            }
//
//            return permissions.ToArray();
//        }

        private static string[] _allCategories;

        static DataHolder()
        {
        }

        public DataHolder(Context context)
        {
            mContext = context;
            mPreferences = PreferenceManager.GetDefaultSharedPreferences(mContext);
            fusedLocationProviderClient = LocationServices.GetFusedLocationProviderClient(mContext);
            _allCategories = context.Resources.GetStringArray(Resource.Array.categories_values);

//            Refresh();

//            UpdateDelegates(context);

            foreach (var category in _allCategories)
            {
                CategoriesDataDic[category] =
                    new CategoryData(category.MakeFirstUpper(), CreateCategoryDelegate(category));
            }
        }


        private static CategoryDataDel CreateCategoryDelegate(string category)
        {
            string methodName = $"Get{category.First().ToString().ToUpper()}{category.Substring(1)}Data";
            MethodInfo methodInfo = typeof(DataHolder).GetMethod(methodName, BindingFlags.Public | BindingFlags.Static,
                                        null,
                                        new[] {typeof(CategoryData)}, null) ??
                                    throw new NullPointerException(
                                        $"Cannot find method {methodName} in class {typeof(DataHolder)}");
            CategoryDataDel categoryDelegate = Delegate.CreateDelegate(typeof(CategoryDataDel),
                methodInfo) as CategoryDataDel;
            return categoryDelegate;
        }

        private static String formatLocation(Location location)
        {
            if (location == null)
                return "";
            return
                $"Coordinates: lat = {location.Latitude:F3}, lon = {location.Longitude:F3}, time = {new Date(location.Time)}";
        }
    }
}