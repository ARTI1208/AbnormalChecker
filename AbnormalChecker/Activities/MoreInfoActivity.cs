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
        Theme = "@style/MainTheme",
        Icon = "@mipmap/icon"
    )]
    public class MoreInfoActivity : AppCompatActivity
    {
        private DataHolder.CategoryData _mCategoryData;

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
            SetContentView(Resource.Layout.more_info_activity);
            TextView status = FindViewById<TextView>(Resource.Id.status);
            TextView permissions = FindViewById<TextView>(Resource.Id.permissions);
            TextView data = FindViewById<TextView>(Resource.Id.data);
            _mCategoryData = DataHolder.CategoriesDataDic[Intent.GetStringExtra("category")];
            if (_mCategoryData == null)
            {
                DataHolder.Initialize(this);
                _mCategoryData = DataHolder.CategoriesDataDic[Intent.GetStringExtra("category")];
            }
            SupportActionBar.Title = _mCategoryData.Title;
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            if (Intent.HasExtra("notification_id"))
            {
                ((NotificationManager) GetSystemService(NotificationService)).Cancel(
                    Intent.GetIntExtra("notification_id", 0));
            }

            if (_mCategoryData.RequiredPermissions == null)
            {
                permissions.Text = "Required permissions:\n\tNo";
            }
            else
            {
                string perms = "";
                foreach (var p in _mCategoryData.RequiredPermissions)
                {
                    perms += "\n\t" + p;
                }

                permissions.Text = $"Required permissions: {perms}";
            }

            status.Text = $"Status:\n\t{_mCategoryData.Status}";
            if (_mCategoryData.DataFilePath == null)
            {
                data.Text = $"Data message:\n\t{_mCategoryData.Data}";
            }
            else if (new File(FilesDir, _mCategoryData.DataFilePath).Exists())
            {
                using (StreamReader reader = new StreamReader(OpenFileInput(_mCategoryData.DataFilePath)))
                {
                    data.Text = $"Data message:\n\t{reader.ReadToEnd()}";
                }
            }
            else
            {
                data.Text = $"No data available";
            }
        }
    }
}