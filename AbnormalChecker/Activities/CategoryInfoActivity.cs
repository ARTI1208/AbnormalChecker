using System.Collections.Generic;
using System.IO;
using Android.App;
using Android.OS;
using Android.Support.V7.App;
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
		private TextView StatusTV;
		private TextView PermissionsTV;
		private TextView DataTV;

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
			StatusTV = FindViewById<TextView>(Resource.Id.status);
			PermissionsTV = FindViewById<TextView>(Resource.Id.permissions);
			DataTV = FindViewById<TextView>(Resource.Id.data);
			SupportActionBar.SetDisplayHomeAsUpEnabled(true);
		}

		private void LoadData()
		{
			mCategoryData = DataHolder.CategoriesDictionary.GetValueOrDefault(Intent.GetStringExtra("category"), null);
			if (mCategoryData == null)
			{
				DataHolder.Initialize(this);
				DataHolder.Refresh();
				mCategoryData = DataHolder.CategoriesDictionary[Intent.GetStringExtra("category")];
			}
			DataHolder.Refresh();
			if (Intent.HasExtra("notification_id"))
				((NotificationManager) GetSystemService(NotificationService)).Cancel(
					Intent.GetIntExtra("notification_id", 0));
			SupportActionBar.Title = mCategoryData.Title;
			if (mCategoryData.RequiredPermissions == null)
			{
				PermissionsTV.SetText(Resource.String.category_info_no_permissions_required);
			}
			else
			{
				var perms = "";
				for (var i = 0; i < mCategoryData.RequiredPermissions.Length; i++)
				{
					perms += $"{mCategoryData.RequiredPermissions[i]}";
					if (i != mCategoryData.RequiredPermissions.Length - 1) perms += "\n";
				}

				PermissionsTV.Text = perms;
			}

			StatusTV.Text = mCategoryData.Status;
			if (mCategoryData.DataFilePath == null)
			{
				if (mCategoryData.Data == null)
					DataTV.SetText(Resource.String.category_info_no_data);
				else
					DataTV.Text = mCategoryData.Data;
			}
			else if (new File(FilesDir, mCategoryData.DataFilePath).Exists())
			{
				using (var reader = new StreamReader(OpenFileInput(mCategoryData.DataFilePath)))
				{
					DataTV.Text = reader.ReadToEnd();
				}
			}
			else
			{
				DataTV.SetText(Resource.String.category_info_no_data);
			}
		}
		
		protected override void OnResume()
		{
			base.OnResume();
			LoadData();
		}
	}
}