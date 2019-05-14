using System.Collections.Generic;
using System.IO;
using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Util;
using Android.Widget;
using File = Java.IO.File;

namespace AbnormalChecker.Activities
{
	[Activity(
		Label = "AbnormalChecker",
		Theme = "@style/StartTheme",
		Icon = "@mipmap/icon"
	)]
	public class CategoryInfoActivity : AppCompatActivity
	{
		private DataHolder.CategoryData mCategoryData;

		public override bool OnSupportNavigateUp()
		{
			Finish();
			return true;
		}

		public override bool OnNavigateUp()
		{
			Finish();
			return true;
		}

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			SetTheme(Resource.Style.MainTheme);
			SetContentView(Resource.Layout.more_info_activity);
			var status = FindViewById<TextView>(Resource.Id.status);
			var permissions = FindViewById<TextView>(Resource.Id.permissions);
			var data = FindViewById<TextView>(Resource.Id.data);
			mCategoryData = DataHolder.CategoriesDictionary.GetValueOrDefault(Intent.GetStringExtra("category"), null);
			if (mCategoryData == null)
			{
				DataHolder.Initialize(this);
				DataHolder.Refresh();
				mCategoryData = DataHolder.CategoriesDictionary[Intent.GetStringExtra("category")];
			}
			SupportActionBar.Title = mCategoryData.Title;
			SupportActionBar.SetDisplayHomeAsUpEnabled(true);
			if (Intent.HasExtra("notification_id"))
				((NotificationManager) GetSystemService(NotificationService)).Cancel(
					Intent.GetIntExtra("notification_id", 0));

			if (mCategoryData.RequiredPermissions == null)
			{
				permissions.SetText(Resource.String.category_info_no_permissions_required);
			}
			else
			{
				var perms = "";
				for (var i = 0; i < mCategoryData.RequiredPermissions.Length; i++)
				{
					perms += $"{mCategoryData.RequiredPermissions[i]}";
					if (i != mCategoryData.RequiredPermissions.Length - 1) perms += "\n";
				}

				permissions.Text = perms;
			}

			status.Text = mCategoryData.Status;
			if (mCategoryData.DataFilePath == null)
			{
				if (mCategoryData.Data == null)
					data.SetText(Resource.String.category_info_no_data);
				else
					data.Text = mCategoryData.Data;
			}
			else if (new File(FilesDir, mCategoryData.DataFilePath).Exists())
			{
				Log.Debug(nameof(CategoryInfoActivity), "File exists");
				using (var reader = new StreamReader(OpenFileInput(mCategoryData.DataFilePath)))
				{
					data.Text = reader.ReadToEnd();
				}
			}
			else
			{
				data.SetText(Resource.String.category_info_no_data);
			}
		}
	}
}