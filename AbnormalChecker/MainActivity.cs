using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Widget;
using Android.OS;
using Android.Preferences;
using Android.Support.V17.Leanback.App;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;

namespace AbnormalChecker
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
        
        private OnboardingSupportFragment MyOnboardingSupportFragment;
        private ISharedPreferences mPreferences;
        
        private static MainActivity _activity;
        public static CategoriesAdapter adapter
        {
            get;
            private set;
        }
        
        private const int PermissionRequestCode = 666;
        
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
            if (mPreferences.GetBoolean("first_run", true))
            {    
                //TODO : OnBoarding Fragment
                RequestPermissions(CategoriesData.GetAllRequiredPermissions(this), PermissionRequestCode);    
                mPreferences.Edit().PutBoolean("first_run", false).Apply();   
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            if (requestCode == PermissionRequestCode)
            {
                adapter?.Refresh();                      
            }
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Main);
            _activity = this;
            mPreferences = PreferenceManager.GetDefaultSharedPreferences(this);
            CheckAndGrantPermissions();
            Intent starter = new Intent();
            starter.SetAction(ServiceStarter.ActionStartAbnormalMonitoring);
            starter.SetClass(this, typeof(ServiceStarter));
            SendBroadcast(starter);
            Button b = FindViewById<Button>(Resource.Id.notif);
            b.Click += delegate
            {
                adapter?.Refresh();
            };
            RecyclerView recyclerView = FindViewById<RecyclerView>(Resource.Id.categoriesRecyclerView);
            LinearLayoutManager llm = new LinearLayoutManager(this);
            adapter = new CategoriesAdapter(this);
            recyclerView.SetLayoutManager(llm);
            recyclerView.SetAdapter(adapter);
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
                StartActivity(new Intent(this, typeof(Settings)));
            }
            return base.OnOptionsItemSelected(item);
        }
    }
}