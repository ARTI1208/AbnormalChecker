using System;
using Android.Annotation;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Telephony;
using Android.Util;
using Android.Widget;

namespace AbnormalChecker.BroadcastReceivers
{
    [BroadcastReceiver(Enabled = true, Exported = true)]
    [IntentFilter(new []
        {
            Intent.ActionNewOutgoingCall,
            TelephonyManager.ActionPhoneStateChanged      
        }
    )]
    [TargetApi(Value = 21)]
    public class PhoneCallReceiver : BroadcastReceiver
    {

        public const string TAG = nameof(PhoneCallReceiver);
        
        [TargetApi(Value = 21)]
        public override void OnReceive(Context context, Intent intent)
        {
            TelephonyManager telephonyManager = TelephonyManager.FromContext(context);
            if (intent.Action == Intent.ActionNewOutgoingCall)
            {                
                OnOutgoingCall(context, intent.GetStringExtra(Intent.ExtraPhoneNumber));
            }
            else
            {     
                switch (telephonyManager.CallState)
                {
                    case CallState.Offhook:
                        OnOutgoingCall(context, intent.GetStringExtra(Intent.ExtraPhoneNumber));
                        break;
                    case CallState.Ringing:
                        OnIncomingCall(context, intent.GetStringExtra(TelephonyManager.ExtraIncomingNumber));
                        break;
                }
            }
        }

        public void OnOutgoingCall(Context context, string phoneNumber)
        {
            TelephonyManager telephonyManager = TelephonyManager.FromContext(context);
            string myPhoneNumber = telephonyManager.Line1Number;
            if (phoneNumber == null)
            {
                Log.Error(TAG, "Can't proceed outgoing call, phone number is null!");
                return;
            }
            Log.Debug(TAG, $"Outgoing call to {phoneNumber}, my number : {myPhoneNumber}");
            
        }
        
        public void OnIncomingCall(Context context, string phoneNumber)
        {
            TelephonyManager telephonyManager = TelephonyManager.FromContext(context);
            string myPhoneNumber = telephonyManager.Line1Number;
            Log.Debug(TAG, $"Incoming call from {phoneNumber}, my number : {myPhoneNumber}");
            if (phoneNumber == null)
            {
                Log.Error(TAG, "Can't proceed incoming call, phone number is null!");
                return;
            }
            
            
            
        }
        
    }
}