using System;
using AbnormalChecker.Activities;
using AbnormalChecker.Extensions;
using Android.App;
using Android.Content;
using Android.Icu.Text;
using Android.OS;
using Android.Util;
using Java.IO;
using Java.Util;

namespace AbnormalChecker.Services
{
    [Service]
    public class SystemModListenerService : Service
    {
        public static string Logger = "";

        private RecursiveFileObserver mFileObserver;

        private static Date _lastTime;

        public override void OnCreate()
        {
            File system = new File("/system");
            NotificationSender sender = new NotificationSender(this, "System Modification");
            void Ev(FileObserverEvents events, string path)
            {
                if (_lastTime == null)
                {
                    _lastTime = new Date();
                }
                else if (TimeSpan.FromMilliseconds(new Date().Time - _lastTime.Time).Seconds < 1)
                {
                    _lastTime = new Date();
                    return;
                }
                _lastTime = new Date();
                string st = "";
                if (Logger.Length > 0)
                {
                    st = "\n";
                }
                Logger += $"{st}{GetFormattedDateTime()} : Detected {events} event for {path}";
                sender.Send(NotificationSender.WarningNotification, $"Detected {events} event for {path}");
                MainActivity.adapter?.Refresh();
            }
            mFileObserver = new RecursiveFileObserver(system.AbsolutePath, Ev, RecursiveFileObserver.ChangesOnly);
        }

        private string GetFormattedDateTime()
        {
            Date now = new Date();
            SimpleDateFormat dateFormat = new SimpleDateFormat("dd.MM.yyyy, kk:mm:ss");
            return dateFormat.Format(now);
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            mFileObserver.StartWatching();
            return base.OnStartCommand(intent, flags, startId);
        }

        public override IBinder OnBind(Intent intent)
        {
            return null;
        }
    }
}