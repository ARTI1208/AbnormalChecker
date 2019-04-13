using Android.App;
using Android.Content;
using Android.OS;
using Android.Util;

namespace AbnormalChecker.Services
{
    public class TestService : Service
    {
        public override IBinder OnBind(Intent intent)
        {
            return null;
        }

        public override void OnCreate()
        {
            
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            Log.Debug("testservice", "texxt");
            return base.OnStartCommand(intent, flags, startId);
        }

        public override void OnDestroy()
        {
            Log.Debug("testservice", "STOOOOP!");
            base.OnDestroy();
        }
    }
}