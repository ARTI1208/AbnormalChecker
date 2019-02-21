using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.App;
using Android.Widget;

namespace AbnormalChecker.Activities
{
    [Activity(
        Label = "AbnormalChecker",
        Theme = "@style/MainTheme",
        Icon = "@drawable/icon"    
    )]
    public class MoreInfoActivity : AppCompatActivity
    {
        private DataHolder.CategoryData mCategoryData;
        
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.more_info_activity);
            TextView status = FindViewById<TextView>(Resource.Id.status);
            TextView permissions = FindViewById<TextView>(Resource.Id.permissions);
            TextView data = FindViewById<TextView>(Resource.Id.data);
            mCategoryData = DataHolder.CategoriesDataDic[Intent.GetStringExtra("category")];
            SupportActionBar.Title = mCategoryData.Title;
            Toast.MakeText(this, Intent.GetStringExtra("category"), ToastLength.Short).Show();
            if (Intent.HasExtra("notification_id"))
            {
                ((NotificationManager) GetSystemService(NotificationService)).
                    Cancel(Intent.GetIntExtra("notification_id", 0));
            }

            status.Text = mCategoryData.Status;

            if (mCategoryData.RequiredPermissions == null)
            {
                permissions.Text = "No permissions required";
            }
            else
            {
                string perms = "";
                foreach (var p in mCategoryData.RequiredPermissions)
                {
                    perms += p + "\n";
                }
                perms = perms.Trim();
                permissions.Text = perms;
            }
            
            status.Text = mCategoryData.Status;
            
            data.Text = mCategoryData.Data;
        }
   
    }
}