using System.Collections.Generic;
using AbnormalChecker.OtherUI;
using Android.App;
using Android.OS;
using Android.Widget;

namespace AbnormalChecker.Activities
{
    [Activity
    (
        Label = "AbnormalCheckerOn",
        Theme = "@style/MainTheme",
        Icon = "@drawable/icon",
        MainLauncher = false
    )]
    public class StartActivity : OnBoardingActivity
    {
        public override void onFinishButtonPressed()
        {
            Toast.MakeText(this, "Done", ToastLength.Short).Show();
            RequestPermissions(DataHolder.GetAllRequiredPermissions(this), MainActivity.PermissionRequestCode);
            MainActivity.mPreferences.Edit().PutBoolean("first_run", false).Apply();
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            List<OnBoardingCard> list = new List<OnBoardingCard>();
            for (int i = 0; i < 3; i++)
            {
                list.Add(new OnBoardingCard($"Title {i+1}", $"Desc {i+1}"));
            }
            setOnboardPages(list);
        }
    }
}