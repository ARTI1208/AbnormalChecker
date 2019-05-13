using AbnormalChecker.Activities;
using AbnormalChecker.BroadcastReceivers;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Support.V4.App;

namespace AbnormalChecker
{
	public enum NotificationType
	{
		WarningNotification,
		SummaryNotification,
		InfoNotification
	}

	public class NotificationSender
	{
		public static readonly string ExtraNotificationId = "notification_id";
		private static readonly string ExtraNotificationCategory = "category";

		private readonly string mCategory;
		private readonly Context mContext;
		private Intent normalizeIntent;
		private readonly NotificationManager notificationManager;

		public NotificationSender(Context context, string category)
		{
			mCategory = category;
			mContext = context;
			notificationManager = NotificationManager.FromContext(context);
		}

		public void PutNormalizeExtra(string key, string value)
		{
			CreateNormalizeIntent().PutExtra(key, value);
		}

		public void PutNormalizeExtra(string key, int value)
		{
			CreateNormalizeIntent().PutExtra(key, value);
		}

		public void PutNormalizeExtra(string key, double value)
		{
			CreateNormalizeIntent().PutExtra(key, value);
		}

		public void PutNormalizeExtra(string key, bool value)
		{
			CreateNormalizeIntent().PutExtra(key, value);
		}

		private Intent CreateNormalizeIntent()
		{
			if (normalizeIntent == null)
			{
				var normalIntent = new Intent(mContext, typeof(MakeNormalReceiver));
				normalIntent.PutExtra(ExtraNotificationCategory, mCategory);
				normalizeIntent = normalIntent;
			}

			return normalizeIntent;
		}

		private PendingIntent IntentToNormalize(Intent intent)
		{
			return PendingIntent.GetBroadcast(mContext, mCategory.GetHashCode(), intent,
				PendingIntentFlags.CancelCurrent);
		}

		public void Send(NotificationType notificationType, string text)
		{
			var categoryTitle = DataHolder.CategoriesDataDic[mCategory].Title;
			var title = "DummyTitle";
			var description = "BaseDescription";
			var builder = new NotificationCompat.Builder(mContext, title);
			builder.SetPriority(NotificationCompat.PriorityHigh);
			var importance = NotificationImportance.Max;
			switch (notificationType)
			{
				case NotificationType.WarningNotification:
					title = string.Format(mContext.GetString(Resource.String.notification_type_warning), categoryTitle);
					description =
						string.Format(mContext.GetString(Resource.String.notification_type_warning_description),
							categoryTitle);
					break;
				case NotificationType.SummaryNotification:
					title = "Daily Summary";
					description = "This channel is used to send daily summaries";
					break;
				case NotificationType.InfoNotification:
					title = categoryTitle + " Info";
					description = $"This channel is used to send information associated with {categoryTitle}";
					builder.SetPriority(NotificationCompat.PriorityDefault);
					importance = NotificationImportance.Default;
					break;
			}

			var intent = new Intent(mContext, typeof(CategoryInfoActivity));
			intent.PutExtra(ExtraNotificationId, mCategory.GetHashCode());
			intent.PutExtra(ExtraNotificationCategory, mCategory);
			var pendingIntent = PendingIntent.GetActivity(mContext,
				(int) notificationType, intent, PendingIntentFlags.CancelCurrent);

			builder.SetContentTitle(title)
				.SetContentText(text)
				.SetChannelId(mCategory)
				.SetDefaults(NotificationCompat.DefaultSound)
				.SetContentIntent(pendingIntent)
				.SetAutoCancel(true);

			switch (notificationType)
			{
				case NotificationType.WarningNotification:
					builder.SetSmallIcon(Android.Resource.Drawable.StatSysWarning);
					var makeNormal = new NotificationCompat.Action(Resource.Drawable.ic_notif_normalize,
						mContext.GetString(Resource.String.notification_button_normalize),
						IntentToNormalize(CreateNormalizeIntent()));

					var viewDetails = new NotificationCompat.Action(Resource.Drawable.ic_notif_details,
						mContext.GetString(Resource.String.notification_button_details), pendingIntent);
					builder.AddAction(makeNormal).AddAction(viewDetails);
					break;
				case NotificationType.InfoNotification:
					builder.SetSmallIcon(Resource.Drawable.ic_stat_info);
					break;
				case NotificationType.SummaryNotification:
					builder.SetSmallIcon(Resource.Drawable.ic_stat_summary);
					break;
				default:
					builder.SetSmallIcon(Resource.Mipmap.Icon);
					break;
			}

			if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
			{
				var channel = new NotificationChannel(mCategory, title, importance)
				{
					Description = description,
					LightColor = Color.Red
				};
				channel.EnableLights(true);
				channel.EnableVibration(true);
				notificationManager.CreateNotificationChannel(channel);
			}
			Notification bigNotification = new NotificationCompat.BigTextStyle(builder)
				.BigText(text).Build();
			notificationManager.Notify(mCategory.GetHashCode(), bigNotification);
		}
	}
}