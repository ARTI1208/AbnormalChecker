using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.App;

namespace AbnormalChecker.Activities
{
    [Activity(
        Label = "AbnormalChecker",
        Theme = "@style/MainTheme",
        Icon = "@drawable/icon"    
    )]
    public class MoreInfoActivity : AppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Intent intent = Intent;
            SupportActionBar.Title = intent.GetStringExtra("title");
        }

        
    }
}