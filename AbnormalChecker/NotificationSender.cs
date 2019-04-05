

using AbnormalChecker.Activities;
using AbnormalChecker.BroadcastReceivers;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Support.V4.App;

namespace AbnormalChecker
{
    public class NotificationSender
    {

        public const int WarningNotification = 0;
        public const int SummaryNotification = 1;
        public const int InfoNotification = 2;

        private string mCategory;
        private NotificationManager notificationManager;
        private Context mContext;
        private Intent NormalizeIntent;

        public NotificationSender(Context context, string category)
        {
            mCategory = category;
            mContext = context;
            notificationManager = (NotificationManager) mContext.GetSystemService(Context.NotificationService);
        }

        public void PutNormalizeExtra(string key, string value)
        {
            CreateNormalizeIntent().PutExtra(key, value);
        }

        private Intent CreateNormalizeIntent()
        {
            if (NormalizeIntent == null)
            {
                Intent normalIntent = new Intent(mContext, typeof(MakeNormalReceiver));
                normalIntent.PutExtra("notification_id", mCategory.GetHashCode());
                normalIntent.PutExtra("category", mCategory);
                NormalizeIntent = normalIntent;    
            }

            return NormalizeIntent;
        }

        private PendingIntent IntentToNormalize(Intent intent)
        {
            return PendingIntent.GetBroadcast(mContext, 666, intent, PendingIntentFlags.CancelCurrent);
        }

        public void Send(int notificationType, string text)
        {
            string categoryTitle = DataHolder.CategoriesDataDic[mCategory].Title;
            string title = "DummyTitle";
            string description = "BaseDescription";
            NotificationCompat.Builder builder = new NotificationCompat.Builder(mContext, title);
            builder.SetPriority(NotificationCompat.PriorityHigh);
            NotificationImportance importance = NotificationImportance.Max;
            switch (notificationType)
            {
                case WarningNotification:
                    title = categoryTitle + " Warning";
                    description = $"This channel is used to send warnings associated with {categoryTitle}";
                    break;
                case SummaryNotification:
                    title = "Daily Summary";
                    description = $"This channel is used to send daily summaries";
                    break;
                case InfoNotification:
                    title = categoryTitle + " Info";
                    description = $"This channel is used to send information associated with {categoryTitle}";
                    builder.SetPriority(NotificationCompat.PriorityDefault);
                    importance = NotificationImportance.Default;
                    break;
            }
            
            Intent intent = new Intent(mContext, typeof(MoreInfoActivity));
            intent.PutExtra("notification_id", mCategory.GetHashCode());
            intent.PutExtra("category", mCategory);
            PendingIntent pendingIntent = PendingIntent.GetActivity(mContext, 
                notificationType, intent, PendingIntentFlags.CancelCurrent);
                        
            builder.SetContentTitle(title)
                .SetContentText(text)
                .SetChannelId(mCategory)
                .SetDefaults(NotificationCompat.DefaultSound)
                .SetContentIntent(pendingIntent)
                .SetAutoCancel(true);
               
            switch (notificationType)
            {
                case WarningNotification:
                    builder.SetSmallIcon(Android.Resource.Drawable.StatSysWarning);

                    NotificationCompat.Action makeNormal = new NotificationCompat.Action(Resource.Mipmap.Icon, 
                        "Make normal", IntentToNormalize(CreateNormalizeIntent()));        
                    
                    NotificationCompat.Action viewDetails = new NotificationCompat.Action(Resource.Mipmap.Icon, 
                        "View details", pendingIntent);
                    builder.AddAction(makeNormal).AddAction(viewDetails);
                    break;
                case InfoNotification:
                    builder.SetSmallIcon(Resource.Drawable.ic_stat_info);
                    break;
                case SummaryNotification:
                    builder.SetSmallIcon(Resource.Drawable.ic_stat_summary);
                    break;
                default:
                    builder.SetSmallIcon(Resource.Mipmap.Icon);
                    break;
            }
            
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                NotificationChannel channel = new NotificationChannel(mCategory, title, importance)
                {
                    Description = description,
                    LightColor = Color.Red
                };
                channel.EnableLights(true);
                channel.EnableVibration(true);
                notificationManager.CreateNotificationChannel(channel);    
            }
            notificationManager.Notify(mCategory.GetHashCode(), builder.Build());
        }
    }
}