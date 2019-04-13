using System.Collections.Generic;
using System.IO;
using AbnormalChecker.BroadcastReceivers;
using AbnormalChecker.OtherUI;
using AbnormalChecker.Services;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Preferences;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Java.Util;

namespace AbnormalChecker.Activities
{
    [Activity
    (
        Label = "AbnormalChecker",
        Theme = "@style/MainTheme",
        Icon = "@mipmap/icon",
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
        
        private const int OnBoardingRequestCode = 555;

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
            StartActivityForResult(new Intent(this, typeof(StartActivity)), OnBoardingRequestCode);
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
                Toast.MakeText(this, "Result", ToastLength.Short).Show();
            }
        }

        public void RefreshList()
        {
            adapter?.Refresh();
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O 
                && GetSystemService(ShortcutService) is ShortcutManager shortcutManager)
            {
                List<ShortcutInfo> shortcuts = new List<ShortcutInfo>();
                foreach (var cat in DataHolder.GetSelectedCategories())
                {
                    DataHolder.CategoryData data = DataHolder.CategoriesDataDic[cat];

                    Intent info = new Intent(this, typeof(MoreInfoActivity));
                    info.PutExtra("category", cat);
                    info.SetAction(Android.Content.Intent.ActionDefault);
                    
                    ShortcutInfo shortcutOne = new ShortcutInfo.Builder(this, cat)
                        .SetShortLabel(data.Title)
                        .SetIcon(Icon.CreateWithResource(this, Resource.Mipmap.Icon))
                        .SetIntent(info).Build();
                    shortcuts.Add(shortcutOne);
                    shortcutManager.SetDynamicShortcuts(shortcuts);
                }
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
//            Intent serv = new Intent(ApplicationContext, typeof(AbnormalChecker.Services.TestService));
//            StartService(serv); 
//            StopService(serv); 
            Intent starter = new Intent();
            starter.SetAction(ServiceStarter.ActionAbnormalMonitoring);
            starter.SetClass(this, typeof(ServiceStarter));
            SendBroadcast(starter);
            Button b = FindViewById<Button>(Resource.Id.notif);
//            RefreshList();
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

            if (requestCode == OnBoardingRequestCode)
            {
                mPreferences.Edit().PutBoolean("first_run", false).Apply();
                RequestPermissions(DataHolder.GetAllRequiredPermissions(this), PermissionRequestCode);
                SetAdapter();
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