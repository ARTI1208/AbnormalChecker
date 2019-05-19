using System;
using System.IO;
using AbnormalChecker.Activities;
using AbnormalChecker.BroadcastReceivers;
using AbnormalChecker.Extensions;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Util;
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

		private const string ExtraIsForeground = "is_foreground";

		public static void SetSystemMonitoringStatus(Context context, bool enable)
		{
			if (enable && _instanceIntent == null)
			{
				_instanceIntent = new Intent(context, typeof(SystemModListenerService));
				try
				{
					context.StartService(_instanceIntent);
				}
				catch (Exception)
				{
					try
					{
						_instanceIntent.PutExtra(ExtraIsForeground, true);
						context.StartForegroundService(_instanceIntent);
					}
					catch (Exception)
					{
						Log.Error(nameof(SystemModListenerService), "Error starting service");
					}
				}
			}
			else if (_instanceIntent != null)
			{
				context.StopService(_instanceIntent);
				_instanceIntent = null;
			}
		}

		public override void OnCreate()
		{
			if (_instanceIntent == null)
			{
				return;
			}
			
			if (_instanceIntent.GetBooleanExtra(ExtraIsForeground, false))
			{
				int id = new Date().GetHashCode();
				Notification.Builder builder = new Notification.Builder(this, DataHolder.SystemCategory);
				builder.SetContentTitle("System monitoring Service")
					.SetContentText("Monitoring...")
					.SetChannelId(DataHolder.SystemCategory)
					.SetOngoing(true)
					.SetSmallIcon(Resource.Drawable.ic_notif_details);
				
				if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
				{
					var description =
						string.Format(GetString(Resource.String.notification_type_warning_description),
							DataHolder.CategoriesDictionary[DataHolder.SystemCategory].Title);
					var channel = new NotificationChannel(DataHolder.SystemCategory, "System modification Monitor", NotificationImportance.Min)
					{
						Description = description,
						LightColor = Color.Red
					};
					NotificationManager.FromContext(this).CreateNotificationChannel(channel);
				}
				
				StartForeground(id, builder.Build());
			}
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
						if (!reader.ReadToEnd().Contains(path + "____"))
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
			string fullMessage = $"{new Date().GetFormattedDateTime()} : {message}";
			using (StreamWriter writer = new StreamWriter(OpenFileOutput(LogFile, FileCreationMode.Append)))
			{
				writer.WriteLine(fullMessage);
			}
			
			using (StreamWriter writer = new StreamWriter(OpenFileOutput(AlarmReceiver.CurrentSummaryFile, FileCreationMode.Append)))
			{
				writer.WriteLine(fullMessage);
			}

			sender.PutNormalizeExtra(ExtraFilePath, path);
			sender.PutNormalizeExtra(ExtraFileEvent, events.ToString());
			sender.Send(NotificationType.WarningNotification, message);
		}

		public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
		{
			_mFileObserver?.StartWatching();
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