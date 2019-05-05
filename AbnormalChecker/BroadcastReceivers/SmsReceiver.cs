using System;
using Android.App;
using Android.Content;
using Android.Provider;
using Android.Telecom;
using Android.Telephony;
using Android.Util;
using Android.Widget;

namespace AbnormalChecker.BroadcastReceivers
{
	public class SmsReceiver : BroadcastReceiver
	{
		private static SmsReceiver _smsReceiver;
		
		public static void SetSmsReceiverStatus(Context context, bool enable)
		{
			if (enable && _smsReceiver == null)
			{
				IntentFilter screenStateFilter = new IntentFilter();
				screenStateFilter.AddAction(Telephony.Sms.Intents.SmsReceivedAction);
				_smsReceiver = new SmsReceiver();
				context.ApplicationContext.RegisterReceiver(_smsReceiver, screenStateFilter);
			}
			else if (!enable && _smsReceiver != null)
			{
				context.ApplicationContext.UnregisterReceiver(_smsReceiver);
				_smsReceiver.UnregisterFromRuntime();
				_smsReceiver.Dispose();
				_smsReceiver = null;
			}
		}
		
		public override void OnReceive(Context context, Intent intent)
		{
			Toast.MakeText(context, "Received sms", ToastLength.Short).Show();

			foreach (var key in intent.Extras.KeySet())
			{
				Log.Debug(nameof(SmsReceiver), key);
				try
				{
					Log.Debug(nameof(SmsReceiver), intent.GetIntExtra(key, -111).ToString());	
				}
				catch (Exception e)
				{
//					Console.WriteLine(e);
				}
			}
			
		}
	}
}