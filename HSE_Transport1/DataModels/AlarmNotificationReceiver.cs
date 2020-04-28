using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;

namespace HSE_Transport1.DataModels
{
    [BroadcastReceiver]
    public class AlarmReceiver : BroadcastReceiver
    {
        public const string SECONDARY_CHANNEL = "second";
        public const int NOTI_SECONDARY1 = 1200;

        NotificationChannel SetNotificationChannel()
        {
            var chan2 = new NotificationChannel(SECONDARY_CHANNEL,
                   Resource.String.noti_channel_second.ToString(), NotificationImportance.High);
            chan2.LightColor = Color.Blue;
            chan2.LockscreenVisibility = NotificationVisibility.Public;
            return chan2;
        }

        public override void OnReceive(Context context, Intent intent)
        {
            var message = intent.GetStringExtra("message");
            var title = intent.GetStringExtra("title");

            var resultIntent = new Intent(context, typeof(MainActivity));
            resultIntent.SetFlags(ActivityFlags.NewTask | ActivityFlags.ClearTask);

            var pending = PendingIntent.GetActivity(context, 0,
                resultIntent,
                PendingIntentFlags.CancelCurrent);

            var builder =
                new Notification.Builder(context, SECONDARY_CHANNEL)
                    .SetContentTitle(title)
                    .SetContentText(message)
                    .SetSmallIcon(Resource.Drawable.icon_notification);

            builder.SetContentIntent(pending);

            var notification = builder.Build();
            NotificationChannel chan2 = SetNotificationChannel();

            var manager = NotificationManager.FromContext(context);
            manager.CreateNotificationChannel(chan2);
            manager.Notify(NOTI_SECONDARY1, notification);       
        }
    }
}