using System;
using System.Collections.Generic;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.InputMethodServices;
using Android.Locations;
using Android.Widget;
using Android.OS;
using Android.Support.V4.Content;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Java.Util;

namespace AbnormalChecker
{
    [Activity(
        Label = "AbnormalChecker",
        Theme = "@style/MainTheme",
        Icon = "@drawable/icon",
        MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        public Context mContext;
        private static AbnormalBroadcastReceiver _abnormalBroadcastReceiver = new AbnormalBroadcastReceiver();

        private static string[] permissions =
        {
            Manifest.Permission.AccessFineLocation,
            Manifest.Permission.AccessCoarseLocation
        };

        private const int permissionRequestCode = 666;

        private void CheckAndGrantPermissions()
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.M)
            {
                return;
            }
            RequestPermissions(permissions, permissionRequestCode);
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            if (requestCode == permissionRequestCode)
            {
                Log.Debug("Aadpater", (adapter == null).ToString());
                adapter?.Refresh();
                
            }
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
//            SetTheme(Resource.Style.MainTheme);
            SetContentView(Resource.Layout.Main);
            mContext = ApplicationContext;
            RecyclerView recyclerView = FindViewById<RecyclerView>(Resource.Id.categoriesRecyclerView);
//            IntentFilter screenStateFilter = new IntentFilter();
//            screenStateFilter.AddAction(Intent.ActionScreenOn);
//            RegisterReceiver(_abnormalBroadcastReceiver, screenStateFilter);
            Intent starter = new Intent();
            starter.SetAction(ServiceStarter.ActionStartAbnormalMonitoring);
            starter.SetClass(this, typeof(ServiceStarter));
            SendBroadcast(starter);
            Button b = FindViewById<Button>(Resource.Id.notif);
            CheckAndGrantPermissions();
            b.Click += delegate
            {
                
                if (AbnormalBroadcastReceiver.d != null)
                {
                    String s = "";
                    foreach (var i in AbnormalBroadcastReceiver.d)
                    {
                        s += i.ToString();
                    }
                
                    Log.Debug("cheeck", s);
                    
                }
                adapter?.Refresh();
                AlarmManager manager = AlarmManager.FromContext(this);
//                PendingIntent intent = new PendingIntent();
//                manager.Set(AlarmType.Rtc, new Date().Time + TimeSpan.FromSeconds(5).Milliseconds, );
                
                Toast.MakeText(mContext, "Unlocked times = " + AbnormalBroadcastReceiver.AutoAdjustmentMonitorUnlockCount, 
                    ToastLength.Short).Show();
            };
            LinearLayoutManager llm = new LinearLayoutManager(mContext);
            recyclerView.SetLayoutManager(llm);
            adapter = new CategoriesAdapter(mContext);
//            recyclerView.AddItemDecoration(new DividerItemDecoration(recyclerView.Context,
//                DividerItemDecoration.Vertical));
            recyclerView.SetAdapter(adapter);
        }

        public static CategoriesAdapter adapter
        {
            get;
            private set;
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.main_menu, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.ItemId == Resource.Id.settings_item)
            {
                StartActivity(new Intent(mContext, typeof(Settings)));
            }
            return base.OnOptionsItemSelected(item);
        }
    }
}