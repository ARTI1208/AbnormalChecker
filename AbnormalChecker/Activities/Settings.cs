using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using AbnormalChecker.BroadcastReceivers;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V14.Preferences;
using Android.Support.V7.Preferences;
using Java.Util;
using Object = Java.Lang.Object;

namespace AbnormalChecker.Activities
{
    [Activity(Label = "Settings", 
        Theme = "@style/MainTheme", 
        Icon = "@mipmap/icon"
    )]
    public class Settings : Android.Support.V7.App.AppCompatActivity
    {
        public static readonly string ScreenLockAutoAdjustment = "auto_unlock_limit";
        public static readonly string ScreenLockAutoAdjustmentDayCount = "auto_unlock_monitor_time";
        public static readonly string ScreenLockAutoAdjustmentType = "unlock_monitor_type";
        
        public enum SettingsCategory
        {
            Main,
            Developer
        }

        public bool IsSelectedCategory(string category)
        {
            ICollection<string> selectedCategories = mPreferences.GetStringSet("selected_categories", null);
            return selectedCategories.Contains(category);
        }

        

        private static ISharedPreferences mPreferences;
        
        
        
        
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            mPreferences = PreferenceManager.GetDefaultSharedPreferences(this);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SetContentView(Resource.Layout.settings_activity);
//            FrameLayout container = FindViewById<FrameLayout>(Resource.Id.fragment_container);
            LoadScreen(SettingsCategory.Main);

        }

        private SettingsCategory currentCategory = SettingsCategory.Main;
        
        public void LoadScreen(SettingsCategory category)
        {
            PreferenceFragmentCompat newFragment;
            switch (category)
            {
                case SettingsCategory.Main:
                    newFragment = new SettingsFragment();
                    break;
                case SettingsCategory.Developer:
                    newFragment = new DeveloperSettingsFragment();
                    break;
                default:
                    throw new NotImplementedException("Not implemented Settings Category Fragment");
            }
            currentCategory = category;
            SupportFragmentManager.BeginTransaction().Replace(Resource.Id.fragment_container, 
                    newFragment).Commit();
        }

        public override bool OnSupportNavigateUp()
        {
            if (currentCategory == SettingsCategory.Main)
            {
                Finish();    
            }
            else
            {
                LoadScreen(SettingsCategory.Main);
            }
            return base.OnSupportNavigateUp();
        }

        public class DeveloperSettingsFragment : PreferenceFragmentCompat
        {
            public override void OnCreatePreferences(Bundle savedInstanceState, string rootKey)
            {
                AddPreferencesFromResource(Resource.Xml.dev_options);
            }
        }
        
        public class SettingsFragment : PreferenceFragmentCompat
        {
            private ISharedPreferences mPreferences; 
            private int mDevClickedTimes;    
            public override void OnCreatePreferences(Bundle savedInstanceState, string rootKey)
            {
                AddPreferencesFromResource(Resource.Xml.settings);
                mPreferences = PreferenceManager.GetDefaultSharedPreferences(Activity);
                SeekBarPreference screenLimit = (SeekBarPreference) FindPreference("screen_limit");
                screenLimit.PreferenceChange += (sender, args) =>
                {
                    mPreferences.Edit().PutInt(screenLimit.Key, (int)args.NewValue).Apply();
                };

                MultiSelectListPreference categoriesPreference = 
                    (MultiSelectListPreference) FindPreference("selected_categories");

                categoriesPreference.PreferenceChange += (sender, args) =>
                {
                    mPreferences.Edit().PutStringSet(categoriesPreference.Key, categoriesPreference.Values).Apply();
                    ServiceStarter.StartStopMonitoring(Activity);
                };
                
                Preference about =  FindPreference("app_info");
                about.Summary = 
                    Activity.ApplicationContext.PackageManager.GetPackageInfo(Activity.PackageName, 0).VersionName;
                about.PreferenceClick += (sender, args) =>
                {
                    if ((mDevClickedTimes = (mDevClickedTimes + 1) % 1) == 0)
                    {
                        if (Activity is Settings parent)
                        {
                            parent.LoadScreen(SettingsCategory.Developer);    
                        }
                    }   
                };
                SwitchPreferenceCompat auto = (SwitchPreferenceCompat) FindPreference(ScreenLockAutoAdjustment);
                SwitchPreferenceCompat autoRestart = (SwitchPreferenceCompat) FindPreference("auto_unlock_limit_restart");
                
                auto.PreferenceChange += (sender, args) =>
                {
                    bool val = (bool) args.NewValue;
                    mPreferences.Edit().PutBoolean(((Preference)sender).Key, val).Apply();
                    if (val)
                    {
                        mPreferences.Edit().PutLong("auto_start_time", new Date().Time).Apply();    
                    }
                };
                
            }

            
        }
        
    }
}