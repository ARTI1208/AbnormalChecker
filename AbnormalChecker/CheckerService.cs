using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;

namespace AbnormalChecker
{
    [Service]
    public class CheckerService : Service
    {
        public override void OnCreate()
        {
            base.OnCreate();
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            Toast.MakeText(BaseContext, "Service: intent fetched", ToastLength.Long).Show();
            StopSelf();
            return base.OnStartCommand(intent, flags, startId);
        }

        public override IBinder OnBind(Intent intent)
        {
            return null;
        }
    }
}