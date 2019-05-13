using System;
using System.Collections.Generic;
using System.IO;
using AbnormalChecker.BroadcastReceivers;
using AbnormalChecker.Extensions;
using AbnormalChecker.OtherUI;
using AbnormalChecker.Utils;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Preferences;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Android.Widget;
using Java.Util;
using Java.Util.Concurrent;

namespace AbnormalChecker.Activities
{
	[Activity
	(
		Label = "AbnormalChecker",
		Theme = "@style/StartTheme",
		Icon = "@mipmap/icon",
		MainLauncher = true
	)]
	// ReSharper disable once ClassNeverInstantiated.Global
	public class MainActivity : AppCompatActivity
	{
		private const int PermissionRequestCode = 666;

		private const int SettingsRequestCode = 777;

		private const int OnBoardingRequestCode = 555;

		private static MainActivity _activity;
		private ISharedPreferences mPreferences;
		public static CategoriesAdapter Adapter { get; private set; }

		public static void GrantPermissions(string[] permissions)
		{
			if (permissions != null && permissions.Length > 0)
				_activity.RequestPermissions(permissions, PermissionRequestCode);
		}

		private void CheckAndGrantPermissions()
		{
			if (Build.VERSION.SdkInt < BuildVersionCodes.M) return;

			StartActivityForResult(new Intent(this, typeof(WelcomeActivity)), OnBoardingRequestCode);
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

		private void RefreshList()
		{
			Adapter?.Refresh();
			if (Build.VERSION.SdkInt >= BuildVersionCodes.O
			    && GetSystemService(ShortcutService) is ShortcutManager shortcutManager)
			{
				var shortcuts = new List<ShortcutInfo>();
				foreach (var cat in DataHolder.GetSelectedCategories())
				{
					if (!DataHolder.CategoriesDataDic.ContainsKey(cat))
					{
						DataHolder.Initialize(this);
					}

					var data = DataHolder.CategoriesDataDic[cat];

					var info = new Intent(this, typeof(CategoryInfoActivity));
					info.PutExtra("category", cat);
					info.SetAction(Intent.ActionDefault);

					var shortcutOne = new ShortcutInfo.Builder(this, cat)
						.SetShortLabel(data.Title)
						.SetIcon(Icon.CreateWithResource(this, Resource.Mipmap.Icon))
						.SetIntent(info).Build();
					shortcuts.Add(shortcutOne);

					if (shortcuts.Count == shortcutManager.MaxShortcutCountPerActivity)
					{
						break;
					}
					
				}
				shortcutManager.SetDynamicShortcuts(shortcuts);
			}
		}

		private void SetAdapter()
		{
			var recyclerView = FindViewById<RecyclerView>(Resource.Id.categoriesRecyclerView);
			var llm = new LinearLayoutManager(this);
			Adapter = new CategoriesAdapter(this);
			recyclerView.SetLayoutManager(llm);
			recyclerView.SetAdapter(Adapter);
			Adapter?.Refresh();
		}

		protected override void OnCreate(Bundle bundle)
		{
			base.OnCreate(bundle);
			SetTheme(Resource.Style.MainTheme);
			SetContentView(Resource.Layout.Main);
			_activity = this;
			mPreferences = PreferenceManager.GetDefaultSharedPreferences(this);
			Log.Debug("AbDateWTF", TimeUnit.Milliseconds.ToSeconds(2941000).ToString());
			DataHolder.Initialize(this);
			UpdateScreenData();
			if (IsFirstRun())
				CheckAndGrantPermissions();
			else
				SetAdapter();
			var starter = new Intent();
			starter.SetAction(MonitoringStarter.ActionAbnormalMonitoring);
			starter.SetClass(this, typeof(MonitoringStarter));
			SendBroadcast(starter);
			var b = FindViewById<Button>(Resource.Id.notif);

			SupportActionBar.Elevation = 0;

			try
			{
				RefreshList();
			}
			catch (Exception e)
			{
				Log.Error("AbnormalShortcuts", e.Message);
			}
			
			b.Visibility = ViewStates.Gone;
			b.Click += delegate
			{
				Adapter?.Refresh();
			};
		}

		private void UpdateScreenData()
		{
			var now = new Date();
			long monitoringStartTime;
			if ((monitoringStartTime = mPreferences.GetLong(ScreenUtils.MonitoringLastStartTime, -1)) == -1)
			{
				monitoringStartTime = ScreenUtils.GetMonitoringStartTime(now);
				mPreferences.Edit().PutLong(ScreenUtils.MonitoringLastStartTime, monitoringStartTime).Apply();
				Log.Debug("UpdateScreenDatad", "monitoring");
				using (StreamWriter writer = new StreamWriter(
					OpenFileOutput(ScreenUnlockReceiver.DebugFile, FileCreationMode.Append)))
				{
					
					
					writer.WriteLine($"----Monitoring m start Time: {now.GetFormattedDateTime()}----");
				}
			}
			else
			{
//				monitoringStartTime = ScreenUtils.GetMonitoringStartTime(now);
				var today = mPreferences.GetInt(ScreenUtils.UnlocksToday, -1);
				Log.Debug("UpdateScreenDatae", $"{today}");
				monitoringStartTime = ScreenUtils.GetMonitoringStartTime(new Date(monitoringStartTime));
				if (TimeUnit.Milliseconds.ToDays(now.Time - monitoringStartTime) >= 1 && today != -1)
				{
					int u;
					var lastDayUnlocked = mPreferences.GetInt(ScreenUtils.LastUnlockDay, -1);
					if ((u = mPreferences.GetInt($"{ScreenUtils.UnlocksDayNumber}{lastDayUnlocked}", -1)) != -1)
						today = (int) Math.Ceiling((u + today) / 2d);
					Log.Debug("UpdateScreenDatad", $"{lastDayUnlocked}");
					Log.Debug("UpdateScreenDatac", $"{today}");
					if (lastDayUnlocked != -1)
						mPreferences.Edit()
							.PutInt($"{ScreenUtils.UnlocksDayNumber}{lastDayUnlocked}", today)
							.PutInt(ScreenUtils.LastUnlockDay, lastDayUnlocked)
							.PutLong(ScreenUtils.MonitoringLastStartTime, monitoringStartTime)
							.PutInt(ScreenUtils.UnlocksToday, 0)
							.PutInt(ScreenUtils.UnlocksNewNormalCount, -1)
							.Apply();
					else
						mPreferences.Edit()
							.PutLong(ScreenUtils.MonitoringLastStartTime, monitoringStartTime)
							.PutInt(ScreenUtils.UnlocksToday, 0)
							.PutInt(ScreenUtils.UnlocksNewNormalCount, -1)
							.Apply();
					ScreenUnlockReceiver.SetToZero();
				}
			}

			Adapter?.Refresh();
		}

		protected override void OnResume()
		{
			base.OnResume();
			UpdateScreenData();
			Adapter?.Refresh();
		}

		protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
		{
			if (requestCode == SettingsRequestCode) Adapter.Refresh();

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
				StartActivityForResult(new Intent(this, typeof(SettingsActivity)), SettingsRequestCode);

			return base.OnOptionsItemSelected(item);
		}
	}
}