using System.Collections.Generic;
using AbnormalChecker.BroadcastReceivers;
using AbnormalChecker.Services;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V14.Preferences;
using Android.Support.V7.App;
using Android.Support.V7.Preferences;
using Android.Util;
using Java.Util;

namespace AbnormalChecker.Activities
{
	[Activity(Label = "@string/menu_item_settings",
		Theme = "@style/MainTheme",
		Icon = "@mipmap/icon"
	)]
	public class SettingsActivity : AppCompatActivity
	{
		public enum SettingsCategory
		{
			Main,
			Developer
		}

		public static readonly string ScreenLockAutoAdjustmentType = "unlock_monitor_type";

		private SettingsCategory currentCategory = SettingsCategory.Main;

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			SupportActionBar.SetDisplayHomeAsUpEnabled(true);
			SetContentView(Resource.Layout.settings_activity);
//            FrameLayout container = FindViewById<FrameLayout>(Resource.Id.fragment_container);
			LoadScreen(SettingsCategory.Main);
		}

		private void LoadScreen(SettingsCategory category)
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
					Log.Error(nameof(SettingsFragment), "Not implemented Settings category");
					return;
			}

			currentCategory = category;
			SupportFragmentManager.BeginTransaction().Replace(Resource.Id.fragment_container,
				newFragment).Commit();
		}

		public override bool OnSupportNavigateUp()
		{
			if (currentCategory == SettingsCategory.Main)
				Finish();
			else
				LoadScreen(SettingsCategory.Main);

			return base.OnSupportNavigateUp();
		}

		private class DeveloperSettingsFragment : PreferenceFragmentCompat
		{
			public override void OnCreatePreferences(Bundle savedInstanceState, string rootKey)
			{
				AddPreferencesFromResource(Resource.Xml.dev_options);
				var clearData = FindPreference("clear_files");
				clearData.PreferenceClick += (sender, args) =>
				{
					foreach (var file in Activity.FilesDir.ListFiles())
						if (file.IsFile)
							file.Delete();
				};
			}
		}

		private class SettingsFragment : PreferenceFragmentCompat
		{
			private int mDevClickedTimes;
			private ISharedPreferences mPreferences;

			public override void OnCreatePreferences(Bundle savedInstanceState, string rootKey)
			{
				AddPreferencesFromResource(Resource.Xml.settings);
				mPreferences = PreferenceManager.GetDefaultSharedPreferences(Activity);

				var categoriesPreference =
					(MultiSelectListPreference) FindPreference("selected_categories");

				categoriesPreference.PreferenceChange += (sender, args) =>
				{
					var set = args.NewValue.JavaCast<HashSet>();
					SystemModListenerService.SetSystemMonitoringStatus(Activity,
						set.Contains(DataHolder.SystemCategory));
					DataHolder.SetLocationTrackingEnabled(set.Contains(DataHolder.LocationCategory));
					ScreenUnlockReceiver.SetUnlockReceiverStatus(Activity, set.Contains(DataHolder.ScreenCategory));
					PhoneCallReceiver.SetCallReceiverStatus(Activity, set.Contains(DataHolder.PhoneCategory));
					SmsReceiver.SetSmsReceiverStatus(Activity, set.Contains(DataHolder.SmsCategory));
					DataHolder.SetLocationTrackingEnabled(set.Contains(DataHolder.LocationCategory));

					var categories = new List<string>();
					foreach (var val in set.ToArray())
					{
						Log.Debug("SettingsFragAb", val.ToString());
						categories.Add(val.ToString());
					}

					mPreferences.Edit().PutStringSet(categoriesPreference.Key, categories).Apply();
				};

				var about = FindPreference("app_info");
				about.Summary =
					Activity.ApplicationContext.PackageManager.GetPackageInfo(Activity.PackageName, 0).VersionName;
				about.PreferenceClick += (sender, args) =>
				{
					if ((mDevClickedTimes = (mDevClickedTimes + 1) % 7) == 0)
						if (Activity is SettingsActivity parent)
							parent.LoadScreen(SettingsCategory.Developer);
				};
			}
		}
	}
}