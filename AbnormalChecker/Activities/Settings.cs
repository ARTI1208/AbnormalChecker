using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using AbnormalChecker.BroadcastReceivers;
using AbnormalChecker.Extensions;
using AbnormalChecker.Services;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V14.Preferences;
using Android.Support.V7.Preferences;
using Android.Util;
using Java.Lang;
using Java.Util;
using ICollection = Java.Util.ICollection;
using IList = Java.Util.IList;
using Object = Java.Lang.Object;
using Timer = System.Timers.Timer;

namespace AbnormalChecker.Activities
{
	[Activity(Label = "@string/menu_item_settings",
		Theme = "@style/MainTheme",
		Icon = "@mipmap/icon"
	)]
	public class Settings : Android.Support.V7.App.AppCompatActivity
	{
		public static readonly string ScreenLockAutoAdjustmentType = "unlock_monitor_type";

		private SettingsCategory _currentCategory = SettingsCategory.Main;

		public enum SettingsCategory
		{
			Main,
			Developer
		}

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

			_currentCategory = category;
			SupportFragmentManager.BeginTransaction().Replace(Resource.Id.fragment_container,
				newFragment).Commit();
		}

		public override bool OnSupportNavigateUp()
		{
			if (_currentCategory == SettingsCategory.Main)
			{
				Finish();
			}
			else
			{
				LoadScreen(SettingsCategory.Main);
			}

			return base.OnSupportNavigateUp();
		}

		private class DeveloperSettingsFragment : PreferenceFragmentCompat
		{
			public override void OnCreatePreferences(Bundle savedInstanceState, string rootKey)
			{
				AddPreferencesFromResource(Resource.Xml.dev_options);
				Preference clearData = FindPreference("clear_files");
				clearData.PreferenceClick += (sender, args) =>
				{
					foreach (var file in Activity.FilesDir.ListFiles())
					{
						if (file.IsFile)
						{
							file.Delete();
						}
					}
				};
			}
		}

		private class SettingsFragment : PreferenceFragmentCompat
		{
			private ISharedPreferences _mPreferences;
			private int _mDevClickedTimes;

			public override void OnCreatePreferences(Bundle savedInstanceState, string rootKey)
			{
				AddPreferencesFromResource(Resource.Xml.settings);
				_mPreferences = PreferenceManager.GetDefaultSharedPreferences(Activity);

				MultiSelectListPreference categoriesPreference =
					(MultiSelectListPreference) FindPreference("selected_categories");

				categoriesPreference.PreferenceChange += (sender, args) =>
				{
					HashSet set = args.NewValue.JavaCast<HashSet>();
					SystemModListenerService.SetSystemMonitoringStatus(Activity,
						set.Contains(DataHolder.SystemCategory));
					DataHolder.SetLocationTrackingEnabled(set.Contains(DataHolder.LocationCategory));
					ScreenUnlockReceiver.SetUnlockReceiverStatus(Activity, set.Contains(DataHolder.ScreenCategory));
					PhoneCallReceiver.SetCallReceiverStatus(Activity, set.Contains(DataHolder.PhoneCategory));
					SmsReceiver.SetSmsReceiverStatus(Activity, set.Contains(DataHolder.SmsCategory));
					List<string> categories = new List<string>();
					foreach (var val in set.ToArray())
					{
						Log.Debug("SettingsFragAb", val.ToString());
						categories.Add(val.ToString());
					}

					_mPreferences.Edit().PutStringSet(categoriesPreference.Key, categories).Apply();
				};

				Preference about = FindPreference("app_info");
				about.Summary =
					Activity.ApplicationContext.PackageManager.GetPackageInfo(Activity.PackageName, 0).VersionName;
				about.PreferenceClick += (sender, args) =>
				{
					if ((_mDevClickedTimes = (_mDevClickedTimes + 1) % 7) == 0)
					{
						if (Activity is Settings parent)
						{
							parent.LoadScreen(SettingsCategory.Developer);
						}
					}
				};
			}
		}
	}
}