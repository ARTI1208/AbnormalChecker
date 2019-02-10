

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Support.V4.App;

namespace AbnormalChecker
{
    public class NotificationSender
    {
        private ISharedPreferences mPreferences;

        public const int WarningNotification = 0;
        public const int SummaryNotification = 1;
        public const int InfoNotification = 2;

        private NotificationManager notificationManager;
        private Context mContext;

        public NotificationSender(Context context)
        {
            mContext = context;
            notificationManager = (NotificationManager) mContext.GetSystemService(Context.NotificationService);
        }

        public void Send(string category, string text, int notificationType)
        {
            string title = category;
            string description = "BaseDescription";
            NotificationCompat.Builder builder = new NotificationCompat.Builder(mContext, category);
            builder.SetPriority(NotificationCompat.PriorityHigh);
            NotificationImportance importance = NotificationImportance.Max;
            switch (notificationType)
            {
                case WarningNotification:
                    title = category + " Warning";
                    description = $"This channel is used to send warnings associated with {category}";
                    break;
                case SummaryNotification:
                    title = "Daily Summary";
                    description = $"This channel is used to send daily summaries";
                    break;
                case InfoNotification:
                    title = category + " Info";
                    description = $"This channel is used to send information associated with {category}";
                    builder.SetPriority(NotificationCompat.PriorityDefault);
                    importance = NotificationImportance.Default;
                    break;
            }
            
            builder.SetContentTitle(title)
                .SetContentText(text)
                .SetChannelId(category)
                .SetDefaults(NotificationCompat.DefaultSound)
                .SetSmallIcon(Resource.Drawable.Icon);
            
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                NotificationChannel channel = new NotificationChannel(category, title, importance)
                {
                    Description = description,
                    LightColor = Color.Red
                };
                channel.EnableLights(true);
                channel.EnableVibration(true);
                notificationManager.CreateNotificationChannel(channel);    
            }
            notificationManager.Notify(category.GetHashCode(), builder.Build());
        }
    }
}