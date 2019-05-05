using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using AbnormalChecker.BroadcastReceivers;
using AbnormalChecker.OtherUI;
using AbnormalChecker.Services;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics.Drawables;
using Android.Locations;
using Android.OS;
using Android.Preferences;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Telecom;
using Android.Telephony;
using Android.Util;
using Android.Views;
using Android.Widget;
using Java.Util;
using Java.Util.Concurrent;
using PhoneNumbers;
using File = Java.IO.File;
using PhoneNumberFormat = PhoneNumbers.PhoneNumberFormat;

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
		private static ISharedPreferences mPreferences { get; set; }

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
			UpdateScreenData();
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
			starter.SetAction(MonitoringStarter.ActionAbnormalMonitoring);
			starter.SetClass(this, typeof(MonitoringStarter));
			SendBroadcast(starter);
			Button b = FindViewById<Button>(Resource.Id.notif);

			SupportActionBar.Elevation = 0;
			
//            RefreshList();
			b.Click += delegate
			{

				TelephonyManager telephonyManager = TelephonyManager.FromContext(this);
				
				PhoneNumberUtil phoneNumberUtils = PhoneNumberUtil.GetInstance();
				try
				{
					PhoneNumber phoneNumberProto = phoneNumberUtils.Parse("+79169268915", "");
					PhoneNumber phoneNumberProto2 = phoneNumberUtils.Parse("+79169268915", "");
					PhoneNumber phoneNumber = new PhoneNumber.Builder()
						.SetRawInput("+79169268915")
//						.SetNationalNumber(ulong.Parse("89169268915"))
						.Build();
					
					
					string myPhoneNumber = new String(telephonyManager.Line1Number.Where(Char.IsDigit).ToArray());
					
					if (myPhoneNumber.Length == 0)
					{
						Log.Error("Art2000Tag", "Can't proceed outgoing call, your phone number is null!");
						return;
					}
					
					PhoneNumber thisPhoneNumber = phoneNumberUtils.Parse(myPhoneNumber, Resources.Configuration.Locale.Country);
            
					Log.Debug("Art2000Tag", $"my number : {myPhoneNumber}, int num {phoneNumberUtils.Format(thisPhoneNumber, PhoneNumberFormat.INTERNATIONAL)}");

					
					
					
					Log.Debug("MainActiivity", phoneNumberProto.CountryCode.ToString());
				}
				catch (Exception e)
				{
					Console.WriteLine($"MainActiivity exception: {e.Message}");
				}
				

				
				
				adapter?.Refresh();
				
			};
		}

		private void UpdateScreenData()
		{
			Date now = new Date();
			long monitoringStartTime;
			if ((monitoringStartTime = mPreferences.GetLong(ScreenUtils.MonitoringLastStartTime, -1)) == -1)
			{
				monitoringStartTime = ScreenUtils.GetMonitoringStartTime(now);
				mPreferences.Edit().PutLong(ScreenUtils.MonitoringLastStartTime, monitoringStartTime).Apply();
				Log.Debug("UpdateScreenDatad", $"monitoring");
			}
			else
			{
				int today = mPreferences.GetInt(ScreenUtils.UnlocksToday, -1);
				Log.Debug("UpdateScreenDatae", $"{today}");
				if (TimeUnit.Milliseconds.ToDays(now.Time - monitoringStartTime) >= 1 && today != -1)
				{
					int u;
					int lastDayUnlocked = mPreferences.GetInt(ScreenUtils.LastUnlockDay, -1);
					if ((u = mPreferences.GetInt($"{ScreenUtils.UnlocksDayNumber}{lastDayUnlocked}", -1)) != -1)
					{
						today = (int) Math.Ceiling((u + today) / 2d);
					}
					Log.Debug("UpdateScreenDatad", $"{lastDayUnlocked}");
					Log.Debug("UpdateScreenDatac", $"{today}");
					if (lastDayUnlocked != -1)
					{
						mPreferences.Edit()
							.PutInt($"{ScreenUtils.UnlocksDayNumber}{lastDayUnlocked}", today)
							.PutInt(ScreenUtils.LastUnlockDay, lastDayUnlocked)
							.PutLong(ScreenUtils.MonitoringLastStartTime, monitoringStartTime)
							.PutInt(ScreenUtils.UnlocksToday, 0)
							.PutInt(ScreenUtils.UnlocksNewNormalCount, -1)
							.Apply();	
					}
					else
					{
						mPreferences.Edit()
							.PutLong(ScreenUtils.MonitoringLastStartTime, monitoringStartTime)
							.PutInt(ScreenUtils.UnlocksToday, 0)
							.PutInt(ScreenUtils.UnlocksNewNormalCount, -1)
							.Apply();
					}
					ScreenUnlockReceiver.SetToZero();
				}
			}
			adapter?.Refresh();
		}

		protected override void OnResume()
		{	
			base.OnResume();
			UpdateScreenData();
			adapter?.Refresh();
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