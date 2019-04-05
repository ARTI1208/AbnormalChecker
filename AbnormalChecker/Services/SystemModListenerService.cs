using System;
using System.IO;
using AbnormalChecker.Activities;
using AbnormalChecker.Extensions;
using Android.App;
using Android.Content;
using Android.Icu.Text;
using Android.OS;
using Android.Util;
using Android.Widget;
using Java.Util;
using File = Java.IO.File;

namespace AbnormalChecker.Services
{
    [Service]
    public class SystemModListenerService : Service
    {
        public static string Logger = "";

        private RecursiveFileObserver mFileObserver;

        private static Date _lastTime;

        public static readonly string LogFile = "system_modification_log.txt";

        public static readonly string ExcludedFiles = "system_modification_exclude.txt";

        public static readonly string ExtraFilePath = "file_path";

        public static readonly string ExtraFileEvent = "file_event";

        public override void OnCreate()
        {
            File system = new File("/system");
            NotificationSender sender = new NotificationSender(this, DataHolder.SystemCategory);
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
                Logger = $"{GetFormattedDateTime()} : Detected {events} event for {path}";
                if (new File(FilesDir, LogFile).Exists())
                {
                    using (StreamReader reader = new StreamReader(OpenFileInput(ExcludedFiles)))
                    {
                        if (!reader.ReadToEnd().Contains(path))
                        {
                            WriteAndSend(sender, path, events);
                        }
                    }
                }
                else
                {
                    WriteAndSend(sender, path, events);
                }
                MainActivity.adapter?.Refresh();
            }

            mFileObserver = new RecursiveFileObserver(system.AbsolutePath, Ev, RecursiveFileObserver.ChangesOnly);
        }

        private void WriteAndSend(NotificationSender sender, string path, FileObserverEvents events)
        {
            using (StreamWriter writer = new StreamWriter(OpenFileOutput(LogFile, FileCreationMode.Append)))
            {
                writer.WriteLine($"{GetFormattedDateTime()} : Detected {events} event for {path}");
            }
            sender.PutNormalizeExtra(ExtraFilePath, path);
            sender.PutNormalizeExtra(ExtraFileEvent, events.ToString());
            sender.Send(NotificationSender.WarningNotification, $"Detected {events} event for {path}");
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