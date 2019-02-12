using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android;
using Android.Content;
using Android.Locations;
using Android.OS;
using Android.Preferences;
using Android.Runtime;
using Android.Support.V4.Content;
using Android.Widget;
using Java.Security;
using Java.Text;
using Java.Util;
using Permission = Android.Content.PM.Permission;
using Android.Gms.Common;
using Android.Gms;
using Android.Gms.Location;
using Android.Util;
using ILocationListener = Android.Locations.ILocationListener;

namespace AbnormalChecker
{
    public class CategoriesData : Java.Lang.Object, ILocationListener
    {
        public class CategoryStruct
        {
            public string Title;
            public string Status;
            public string Data;
            public CheckStatus Level = CheckStatus.Normal;
            public string[] RequiredPermissions;
        }

        public enum CheckStatus
        {
            Normal,
            Warning,
            Dangerous,
            PermissionsRequired
        }

        public static string RootCategory = "Root";
        public static string ScreenLocksCategory = "ScreenLocks";
        public static string LocationCategory = "Location";

        enum CheckerCategory
        {
            Root,
            ScreenLocks
        }

        public static List<CategoryStruct> categoriesList = new List<CategoryStruct>();

        private Context mContext;
        private ISharedPreferences mPreferences;
        private ILocationListener mLocationListenerImplementation;


        public void Refresh()
        {
            categoriesList.Clear();
            CategoryStruct root = new CategoryStruct {Title = "Root"};
            if (Checker.IsRooted(mContext))
            {
                root.Status = "Device has root access";
                root.Data = $"Found su binary at {Checker.GetSuBinaryPath()}";
                root.Level = CheckStatus.Dangerous;
            }
            else
            {
                root.Status = "Device is not rooted";
                
            }

            CategoryStruct screenLocks = new CategoryStruct {Title = "ScreenLocks"};
            if (mPreferences.GetBoolean(Settings.ScreenLockAutoAdjustment, false))
            {
                screenLocks.Status = "Monitoring your regular activity";
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

                screenLocks.Data = $"{monitoringTime}-day " +
                                   $"monitoring ends at {dateFormat.Format(monitoringStop)}";
            }
            else
            {
                screenLocks.Status = "Normal";
                screenLocks.Data = $"{AbnormalBroadcastReceiver.unlockedTimes} unlocks total";
            }

            CategoryStruct location = new CategoryStruct {Title = "Location"};
            string status = "";
            location.RequiredPermissions = new []{
                Manifest.Permission.AccessFineLocation,
                Manifest.Permission.AccessCoarseLocation
            };
            
            


            location.Level = 
                location.RequiredPermissions.Any(s => ContextCompat.CheckSelfPermission(mContext, s) == Permission.Denied) 
                    ? CheckStatus.PermissionsRequired : CheckStatus.Normal;
            
            
            if (ContextCompat.CheckSelfPermission(mContext, Manifest.Permission.AccessFineLocation) ==
                Permission.Granted)
            {
                status += "Gps access granted";
            }

            if (ContextCompat.CheckSelfPermission(mContext, Manifest.Permission.AccessCoarseLocation) ==
                Permission.Granted)
            {
                status += " network location access granted";
            }

            location.Status = status.Length > 0 ? status : "Access denied";
            

//            locationManager.RequestLocationUpdates(LocationManager.GpsProvider,
//                0, 0, this);
//            locationManager.RequestLocationUpdates(
//                LocationManager.NetworkProvider, 0, 0,
//                this);
//            checkEnabled();
//            Log.Debug("srvices", IsGooglePlayServicesInstalled().ToString());
            GetLastLocationFromDevice();
            categoriesList.Add(root);
            categoriesList.Add(screenLocks);
            categoriesList.Add(location);
        }

        private LocationManager locationManager;

        private void checkEnabled()
        {
//            Toast.MakeText(mContext, $"gps {locationManager.IsProviderEnabled(LocationManager.GpsProvider)}",
//                ToastLength.Short).Show();
//
//            Toast.MakeText(mContext, $"net {locationManager.IsProviderEnabled(LocationManager.NetworkProvider)}",
//                ToastLength.Short).Show();
//            tvEnabledGPS.setText("Enabled: "
//                                 + locationManager
//                                     .isProviderEnabled(LocationManager.GPS_PROVIDER));
//            tvEnabledNet.setText("Enabled: "
//                                 + locationManager
//                                     .isProviderEnabled(LocationManager.NETWORK_PROVIDER));
        }

        
        
        
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

        FusedLocationProviderClient fusedLocationProviderClient;

        async Task GetLastLocationFromDevice()
        {
            // This method assumes that the necessary run-time permission checks have succeeded.
//            getLastLocationButton.SetText(Resource.String.getting_last_location);
            Android.Locations.Location location = await fusedLocationProviderClient.GetLastLocationAsync();
//            fusedLocationProviderClient.
            if (location == null)
            {
                Log.Error("opiom", "unknown loc");
                // Seldom happens, but should code that handles this scenario
            }
            else
            {

                if (prevLock == null)
                {
                    prevLock = location;
                }
                else
                {

                    
                    
//                    Toast.MakeText(mContext, location.DistanceTo(prevLock).ToString(), ToastLength.Short).Show();
                    prevLock = location;
                }
                
                // Do something with the location
                categoriesList[2].Data = formatLocation(location);
//                Location.DistanceBetween();
                MainActivity.adapter?.NotifyDataSetChanged();
                Log.Debug("Sample", "The latitude is " + location.Latitude);
            }
        }

        public static string[] GetAllRequiredPermissions(Context context)
        {
            if (categoriesList.Count == 0)
            {
                CategoriesData data = new CategoriesData(context);
                
            }
            
            List<string> permissions = new List<string>();
            foreach (var category in categoriesList)
            {
                if (category.RequiredPermissions != null)
                {
                    permissions.AddRange(category.RequiredPermissions);
                }
            }
            return permissions.ToArray();
        }

        

        private Location prevLock;

        public CategoriesData(Context context)
        {
            mContext = context;
            mLocationListenerImplementation = this;
            mPreferences = PreferenceManager.GetDefaultSharedPreferences(mContext);
            fusedLocationProviderClient = LocationServices.GetFusedLocationProviderClient(mContext);
            LocationListener.LocationChangedDelegate changedDelegate = location =>
            {
                Toast.MakeText(mContext, "loc changed", ToastLength.Short);
            }; 
            locationManager = (LocationManager) mContext.GetSystemService(Context.LocationService);
            
            
            
            Refresh();
        }

        private String formatLocation(Location location)
        {
            if (location == null)
                return "";
            return
                $"Coordinates: lat = {location.Latitude:F3}, lon = {location.Longitude:F3}, time = {new Date(location.Time)}";
        }
        
        

        public void OnLocationChanged(Location location)
        {
            categoriesList[2].Data = formatLocation(location);
            Toast.MakeText(mContext, "loc changed", ToastLength.Short);
        }

        public void OnProviderDisabled(string provider)
        {
            Toast.MakeText(mContext, "disabled", ToastLength.Short);
        }

        public void OnProviderEnabled(string provider)
        {
            Toast.MakeText(mContext, "enabled", ToastLength.Short);
        }

        public void OnStatusChanged(string provider, Availability status, Bundle extras)
        {
            Toast.MakeText(mContext, "stat changed", ToastLength.Short);
        }

        public void Dispose()
        {
            mLocationListenerImplementation?.Dispose();
        }
    }
}