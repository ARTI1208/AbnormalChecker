using System;
using System.IO;
using AbnormalChecker.Activities;
using Android.App;
using Android.Content;
using Android.Preferences;
using Android.Util;
using Java.Util;
using Environment = Android.OS.Environment;
using File = Java.IO.File;

namespace AbnormalChecker.BroadcastReceivers
{
	[BroadcastReceiver]
	public class AlarmReceiver : BroadcastReceiver
	{
		private static PendingIntent AlarmPendingIntent;

		public const string CurrentSummaryFile = "current_summary.txt";

		private const string SentSummaryFile = "sent_summary.txt";

		private const string SavedSummaryFile = "summary_{0}.txt";

		public const string SummaryCategory = "summary";
		
		public const string ExtraSummaryText = "summary_text";

		public static void RegisterReceiver(Context context, int type = -1)
		{
			ISharedPreferences preferences = PreferenceManager.GetDefaultSharedPreferences(context);
			
			long repeatInterval;

			switch (type)
			{
				case -1:
					repeatInterval = preferences.GetBoolean(SettingsActivity.MonitoringSummaryType, false)
						? AlarmManager.IntervalDay * 7
						: AlarmManager.IntervalDay;
					break;
				case 1:
					repeatInterval = AlarmManager.IntervalDay * 7;
					break;
				default:
					repeatInterval = AlarmManager.IntervalDay;
					break;
			}

			Intent alarmIntent = new Intent(context, typeof(AlarmReceiver));
			AlarmPendingIntent =
				PendingIntent.GetBroadcast(context, 909, alarmIntent, PendingIntentFlags.UpdateCurrent);

			Calendar calendar = Calendar.Instance;
			calendar.TimeInMillis = Java.Lang.JavaSystem.CurrentTimeMillis();
			calendar.Set(CalendarField.HourOfDay, 21);
			calendar.Set(CalendarField.Minute, 00);

			AlarmManager.FromContext(context).SetInexactRepeating(AlarmType.RtcWakeup, calendar.TimeInMillis,
				repeatInterval, AlarmPendingIntent);
		}

		public static void ExportSummary(string text)
		{
			Date now = new Date();
			File outputFile = new File(Environment.ExternalStorageDirectory, string.Format(SavedSummaryFile, now.GetHashCode()));
			using (StreamWriter writer = new StreamWriter(new FileStream(outputFile.AbsolutePath, FileMode.Create)))
			{
				writer.Write(text);
			}
		}

		public override void OnReceive(Context context, Intent intent)
		{

			File currentSummary = new File(context.FilesDir, CurrentSummaryFile);
			File sentSummary = new File(context.FilesDir, SentSummaryFile);
			NotificationSender sender = new NotificationSender(context, SummaryCategory);
			
			if (currentSummary.Exists())
			{
				try
				{
					string text;
					using (StreamReader reader = new StreamReader(context.OpenFileInput(CurrentSummaryFile)))
					{
						text = reader.ReadToEnd();
					}
					Log.Debug(nameof(AlarmReceiver), text);
					sender.PutNormalizeExtra(ExtraSummaryText, text);
					sender.Send(NotificationType.SummaryNotification, text);
					if (sentSummary.Exists())
					{
						sentSummary.Delete();
					}
					currentSummary.RenameTo(sentSummary);
				}
				catch (Exception e)
				{
					Log.Error(nameof(AlarmReceiver), e.StackTrace);
				}
			}
			else
			{
				Log.Debug(nameof(AlarmReceiver), $"File {currentSummary.AbsolutePath} not exist");
			}
		}
	}
} 