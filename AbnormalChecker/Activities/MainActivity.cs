﻿using AbnormalChecker.BroadcastReceivers;
using AbnormalChecker.OtherUI;
using AbnormalChecker.Services;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Preferences;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;

namespace AbnormalChecker.Activities
{
    [Activity
    (
        Label = "AbnormalChecker",
        Theme = "@style/MainTheme",
        Icon = "@drawable/icon",
        MainLauncher = true
    )]
    // ReSharper disable once ClassNeverInstantiated.Global
    public class MainActivity : AppCompatActivity
    {
        public static ISharedPreferences mPreferences { get; private set; }

        private static MainActivity _activity;
        public static CategoriesAdapter adapter { get; private set; }

        public const int PermissionRequestCode = 666;
        private const int SettingsRequestCode = 777;

        public static void GrantPermissions(string[] permissions)
        {
            if (permissions != null && permissions.Length > 0)
            {
                _activity.RequestPermissions(permissions, PermissionRequestCode);
            }
        }

        private void CheckAndGrantPermissions()
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.M)
            {
                return;
            }

            //TODO : OnBoarding Fragment
            StartActivity(new Intent(this, typeof(StartActivity)));
            
            
            
        }
        
        

        private bool IsFirstRun()
        {
            return mPreferences.GetBoolean("first_run", true);
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions,
            Permission[] grantResults)
        {
            if (requestCode == PermissionRequestCode)
            {
                SetAdapter();
            }
        }

        private void SetAdapter()
        {
            RecyclerView recyclerView = FindViewById<RecyclerView>(Resource.Id.categoriesRecyclerView);
            LinearLayoutManager llm = new LinearLayoutManager(this);
            adapter = new CategoriesAdapter(this);
            recyclerView.SetLayoutManager(llm);
            recyclerView.SetAdapter(adapter);
            adapter?.Refresh();
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Main);
            _activity = this;
            mPreferences = PreferenceManager.GetDefaultSharedPreferences(this);
            DataHolder.Initialize(this);
            if (IsFirstRun())
            {
                CheckAndGrantPermissions();
            }
            else
            {
                SetAdapter();
            }
            Intent serv = new Intent(ApplicationContext, typeof(AbnormalChecker.Services.TestService));
            StartService(serv); 
            StopService(serv); 
            Intent starter = new Intent();
            starter.SetAction(ServiceStarter.ActionAbnormalMonitoring);
            starter.SetClass(this, typeof(ServiceStarter));
            SendBroadcast(starter);
            Button b = FindViewById<Button>(Resource.Id.notif);
            b.Click += delegate
            {
                adapter?.Refresh();
            };
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            if (requestCode == SettingsRequestCode)
            {
                adapter.Refresh();
            }
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
                StartActivityForResult(new Intent(this, typeof(Settings)), SettingsRequestCode);
            }
            return base.OnOptionsItemSelected(item);
        }
    }
}