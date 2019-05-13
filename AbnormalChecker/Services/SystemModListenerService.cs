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
using Java.Util.Concurrent;
using File = Java.IO.File;

namespace AbnormalChecker.Services
{
	[Service]
	public class SystemModListenerService : Service
	{
		private static Intent _instanceIntent;

		public static string Logger = "";

		private RecursiveFileObserver _mFileObserver;

		private static Date _lastTime;

		public static readonly string LogFile = "system_modification_log.txt";

		public static readonly string ExcludedFiles = "system_modification_exclude.txt";

		public static readonly string ExtraFilePath = "file_path";

		public static readonly string ExtraFileEvent = "file_event";

		public static void SetSystemMonitoringStatus(Context context, bool enable)
		{
			if (enable && _instanceIntent == null)
			{
				_instanceIntent = new Intent(context, typeof(SystemModListenerService));
//				context.StartForegroundService(_instanceIntent);
				context.StartService(_instanceIntent);
			}
			else if (_instanceIntent != null)
			{
				context.StopService(_instanceIntent);
				_instanceIntent = null;
			}
		}

		public override void OnCreate()
		{
			File system = new File("/system");
			NotificationSender sender = new NotificationSender(this, DataHolder.SystemCategory);
			
			void OnModificationDetected(FileObserverEvents events, string path)
			{
				if (events >= FileObserverEvents.AllEvents)
				{
					return;
				}
				if (_lastTime == null)
				{
					_lastTime = new Date();
				}
				else if (TimeUnit.Milliseconds.ToSeconds(new Date().Time - _lastTime.Time) < 1)
				{
					_lastTime = new Date();
					return;
				}
				_lastTime = new Date();
				Logger = $"{new Date().GetFormattedDateTime()} : Detected {events} event for {path}";
				if (new File(FilesDir, ExcludedFiles).Exists())
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
				MainActivity.Adapter?.Refresh();
			}

			_mFileObserver = new RecursiveFileObserver(system.AbsolutePath, OnModificationDetected,
				RecursiveFileObserver.ChangesOnly);
		}

		private void WriteAndSend(NotificationSender sender, string path, FileObserverEvents events)
		{
			string message = string.Format(GetString(Resource.String.category_system_notif_modification_detected), events, path);
			
			using (StreamWriter writer = new StreamWriter(OpenFileOutput(LogFile, FileCreationMode.Append)))
			{
				writer.WriteLine($"{new Date().GetFormattedDateTime()} : {message}");
			}

			sender.PutNormalizeExtra(ExtraFilePath, path);
			sender.PutNormalizeExtra(ExtraFileEvent, events.ToString());
			sender.Send(NotificationType.WarningNotification, message);
		}

		public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
		{
			_mFileObserver.StartWatching();
			return base.OnStartCommand(intent, flags, startId);
		}

		public override void OnDestroy()
		{
			base.OnDestroy();
			_mFileObserver?.StopWatching();
		}

		public override IBinder OnBind(Intent intent)
		{
			return null;
		}
	}
}