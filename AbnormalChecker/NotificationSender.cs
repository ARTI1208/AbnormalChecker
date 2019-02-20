

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

        public NotificationSender(Context context, string category)
        {
            mCategory = category;
            mContext = context;
            notificationManager = (NotificationManager) mContext.GetSystemService(Context.NotificationService);
        }

        public void Send(int notificationType, string text)
        {
            string title = mCategory;
            string description = "BaseDescription";
            NotificationCompat.Builder builder = new NotificationCompat.Builder(mContext, mCategory);
            builder.SetPriority(NotificationCompat.PriorityHigh);
            NotificationImportance importance = NotificationImportance.Max;
            switch (notificationType)
            {
                case WarningNotification:
                    title = mCategory + " Warning";
                    description = $"This channel is used to send warnings associated with {mCategory}";
                    break;
                case SummaryNotification:
                    title = "Daily Summary";
                    description = $"This channel is used to send daily summaries";
                    break;
                case InfoNotification:
                    title = mCategory + " Info";
                    description = $"This channel is used to send information associated with {mCategory}";
                    builder.SetPriority(NotificationCompat.PriorityDefault);
                    importance = NotificationImportance.Default;
                    break;
            }
            
            Intent intent = new Intent(mContext, typeof(MoreInfoActivity));
            intent.PutExtra("title", mCategory);
            PendingIntent pendingIntent = PendingIntent.GetActivity(mContext, 
                notificationType, intent, PendingIntentFlags.OneShot);
                        
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
            
                    Intent normalIntent = new Intent(mContext, typeof(MakeNormalReceiver));
                    PendingIntent normalPendingIntent = PendingIntent.GetBroadcast(mContext, 666, normalIntent, 
                        PendingIntentFlags.CancelCurrent);
                    NotificationCompat.Action.Builder makeNormalBuilder = new NotificationCompat.Action.Builder(Resource.Drawable.Icon, 
                        "Make normal", normalPendingIntent);
                    
                    NotificationCompat.Action makeNormal = new NotificationCompat.Action(Resource.Drawable.Icon, 
                        "Make normal", normalPendingIntent);
                    
                    NotificationCompat.Action viewDetails = new NotificationCompat.Action(Resource.Drawable.Icon, 
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
                    builder.SetSmallIcon(Resource.Drawable.Icon);
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