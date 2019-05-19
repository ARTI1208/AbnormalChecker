using System;
using System.Collections.Generic;
using System.IO;
using AbnormalChecker.BroadcastReceivers;
using AbnormalChecker.Services;
using AbnormalChecker.Utils;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V14.Preferences;
using Android.Support.V7.App;
using Android.Support.V7.Preferences;
using Android.Util;
using Android.Widget;
using Java.Util;
using Environment = Android.OS.Environment;
using File = Java.IO.File;

namespace AbnormalChecker.Activities
{
	[Activity(Label = "@string/menu_item_settings",
		Theme = "@style/MainTheme",
		Icon = "@mipmap/icon"
	)]
	public class SettingsActivity : AppCompatActivity
	{
		private enum SettingsCategory
		{
			Main,
			Developer
		}

		public static readonly string ScreenLockAutoAdjustmentType = "unlock_monitor_type";

		public static readonly string MonitoringSummaryType = "summary_type";

		private SettingsCategory currentCategory = SettingsCategory.Main;

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			SupportActionBar.SetDisplayHomeAsUpEnabled(true);
			SetContentView(Resource.Layout.settings_activity);
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
					PreferenceManager.GetDefaultSharedPreferences(Activity).Edit().Remove(MainActivity.KeyFirstRun)
						.Apply();
				};
			}
		}

		private class SettingsFragment : PreferenceFragmentCompat
		{
			private static readonly string SettingsFileName = "ru.art2000.abnormal_preferences.xml";

			private static readonly File exportFile =
				new File(Environment.ExternalStorageDirectory, "abnormal_settings.zip");

			private int mDevClickedTimes;
			private ISharedPreferences mPreferences;

			private File SettingsDir;

			public override void OnCreatePreferences(Bundle savedInstanceState, string rootKey)
			{
				AddPreferencesFromResource(Resource.Xml.settings);
				mPreferences = PreferenceManager.GetDefaultSharedPreferences(Activity);
				SettingsDir = new File(Activity.DataDir, "shared_prefs");
				var exportPreference = FindPreference("export_settings");
				exportPreference.PreferenceClick += (sender, args) =>
				{
					var paths = new List<File> {new File(SettingsDir, SettingsFileName)};
					foreach (var file in Activity.FilesDir.ListFiles())
						if (file.IsFile)
							paths.Add(file);

					if (paths.Count > 0)
					{
						if (OtherUtils.CreateZipArchive(paths, exportFile.AbsolutePath))
							Toast.MakeText(Activity, Activity.GetString(Resource.String.toast_export_successful),
								ToastLength.Short).Show();
						else
							Toast.MakeText(Activity, Activity.GetString(Resource.String.toast_export_failed),
								ToastLength.Short).Show();
					}
					else
					{
						Toast.MakeText(Activity, Activity.GetString(Resource.String.toast_export_no_files),
							ToastLength.Short).Show();
					}
				};

				var importPreference = FindPreference("import_settings");
				importPreference.PreferenceClick += (sender, args) =>
				{
					if (!exportFile.Exists())
					{
						Toast.MakeText(Activity, Activity.GetString(Resource.String.toast_import_no_file),
							ToastLength.Short).Show();
						return;
					}

					if (exportFile.Length() > 1024 * 1024)
					{
						Toast.MakeText(Activity, Activity.GetString(Resource.String.toast_import_big_file),
							ToastLength.Short).Show();
						return;
					}

					if (OtherUtils.UnpackZipArchive(exportFile.AbsolutePath, Activity.CacheDir))
					{
						Toast.MakeText(Activity, Activity.GetString(Resource.String.toast_import_successful),
							ToastLength.Short).Show();
					}
					else
					{
						Toast.MakeText(Activity, Activity.GetString(Resource.String.toast_import_failed),
							ToastLength.Short).Show();
						return;
					}

					foreach (var file in Activity.CacheDir.ListFiles())
					{
						Stream outputStream;

						if (file.Name == SettingsFileName)
						{
							var path = new File(SettingsDir, SettingsFileName).AbsolutePath;
							Log.Debug(nameof(OtherUtils.UnpackZipArchive), path);
							outputStream = new FileStream(path, FileMode.Create);
						}
						else
						{
							outputStream = Activity.OpenFileOutput(file.Name, FileCreationMode.Private);
						}

						string text;
						if (!file.Exists()) continue;
						using (var reader = new StreamReader(new FileStream(file.AbsolutePath, FileMode.Open)))
						{
							text = reader.ReadToEnd();
						}

						using (var writer = new StreamWriter(outputStream))
						{
							writer.Write(text);
						}

						file.Delete();
					}
				};

				var categoriesPreference =
					(MultiSelectListPreference) FindPreference("selected_categories");

				categoriesPreference.PreferenceChange += (sender, args) =>
				{
					var set = args.NewValue.JavaCast<HashSet>();
					SystemModListenerService.SetSystemMonitoringStatus(Activity,
						set.Contains(DataHolder.SystemCategory));
					LocationUtils.SetLocationTrackingEnabled(set.Contains(DataHolder.LocationCategory));
					ScreenUnlockReceiver.SetUnlockReceiverStatus(Activity, set.Contains(DataHolder.ScreenCategory));
					PhoneCallReceiver.SetCallReceiverStatus(Activity, set.Contains(DataHolder.PhoneCategory));
					SmsReceiver.SetSmsReceiverStatus(Activity, set.Contains(DataHolder.SmsCategory));

					var categories = new List<string>();
					foreach (var val in set.ToArray()) categories.Add(val.ToString());

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

				try
				{
					var summaryNotifyPreference = FindPreference(MonitoringSummaryType);
					summaryNotifyPreference.PreferenceChange += (sender, args) =>
					{
						AlarmReceiver.RegisterReceiver(Activity, (bool) args.NewValue ? 1 : 0);
					};
				}
				catch (Exception e)
				{
					Console.WriteLine(e.Message);
					Log.Error(nameof(MonitoringSummaryType), e.Message);
				}
			}
		}
	}
}