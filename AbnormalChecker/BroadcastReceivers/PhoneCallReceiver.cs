using System.IO;
using System.Linq;
using AbnormalChecker.Extensions;
using Android.Content;
using Android.Telephony;
using Android.Util;
using PhoneNumbers;
using File = Java.IO.File;
using PhoneNumberFormat = PhoneNumbers.PhoneNumberFormat;

namespace AbnormalChecker.BroadcastReceivers
{
	public class PhoneCallReceiver : BroadcastReceiver
	{
		private const string Tag = nameof(PhoneCallReceiver);

		private static PhoneCallReceiver _callReceiver;

		public static void SetCallReceiverStatus(Context context, bool enable)
		{
			if (enable && _callReceiver == null)
			{
				IntentFilter screenStateFilter = new IntentFilter();
				screenStateFilter.AddAction(Intent.ActionNewOutgoingCall);
				screenStateFilter.AddAction(TelephonyManager.ActionPhoneStateChanged);
				_callReceiver = new PhoneCallReceiver();
				context.ApplicationContext.RegisterReceiver(_callReceiver, screenStateFilter);
			}
			else if (!enable && _callReceiver != null)
			{
				context.ApplicationContext.UnregisterReceiver(_callReceiver);
				_callReceiver.UnregisterFromRuntime();
				_callReceiver.Dispose();
				_callReceiver = null;
			}
		}

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
					case CallState.Ringing:
						OnIncomingCall(context, intent.GetStringExtra(TelephonyManager.ExtraIncomingNumber));
						break;
				}
			}
		}

		private void OnOutgoingCall(Context context, string phoneNumber)
		{
			TelephonyManager telephonyManager = TelephonyManager.FromContext(context);
			if (phoneNumber == null)
			{
				Log.Error(Tag, "Can't proceed outgoing call, caller phone number is null!");
				return;
			}

			string myPhoneNumber = new string(telephonyManager.Line1Number.Where(char.IsDigit).ToArray());
			if (myPhoneNumber.Length == 0)
			{
				Log.Error(Tag, "Can't proceed outgoing call, your phone number is null!");
				return;
			}

			PhoneNumberUtil phoneNumberUtils = PhoneNumberUtil.GetInstance();
			PhoneNumber callerPhoneNumber =
				phoneNumberUtils.Parse(phoneNumber, context.Resources.Configuration.Locale.Country);
			PhoneNumber thisPhoneNumber =
				phoneNumberUtils.Parse(myPhoneNumber, context.Resources.Configuration.Locale.Country);
			if (callerPhoneNumber.CountryCode == thisPhoneNumber.CountryCode)
			{
				Log.Debug(Tag,
					$"Same for {phoneNumberUtils.Format(callerPhoneNumber, PhoneNumberFormat.INTERNATIONAL)}," +
					$" {phoneNumberUtils.Format(thisPhoneNumber, PhoneNumberFormat.INTERNATIONAL)}");
				return;
			}

			if (new File(context.FilesDir, PhoneUtils.ExcludedOutCountryCodesFile).Exists())
			{
				string text;
				using (StreamReader reader =
					new StreamReader(context.OpenFileInput(PhoneUtils.ExcludedOutCountryCodesFile)))
				{
					text = reader.ReadToEnd();
				}

				foreach (var line in text.Split())
				{
					if (int.TryParse(line, out var lineCode) && callerPhoneNumber.CountryCode == lineCode)
					{
						Log.Debug(Tag, $"Found {lineCode} in excluded outgoing country codes");
						return;
					}
				}
			}

			using (StreamWriter writer =
				new StreamWriter(context.OpenFileOutput(PhoneUtils.SuspiciousCallsFile, FileCreationMode.Append)))
			{
				writer.WriteLine($"Suspicious call to {callerPhoneNumber.GetInternationalNumber()}");
			}

			NotificationSender sender = new NotificationSender(context, DataHolder.PhoneCategory);
			sender.PutNormalizeExtra(PhoneUtils.CountryCodeKey, callerPhoneNumber.CountryCode);
			sender.PutNormalizeExtra(PhoneUtils.IsOutgoingKey, true);
			sender.Send(NotificationSender.NotificationType.WarningNotification,
				$"Suspicious call to {callerPhoneNumber.GetInternationalNumber()}");
		}

		private void OnIncomingCall(Context context, string phoneNumber)
		{
			TelephonyManager telephonyManager = TelephonyManager.FromContext(context);
			if (phoneNumber == null)
			{
				Log.Error(Tag, "Can't proceed incoming call, caller phone number is null!");
				return;
			}

			string myPhoneNumber = new string(telephonyManager.Line1Number.Where(char.IsDigit).ToArray());
			if (myPhoneNumber.Length == 0)
			{
				Log.Error(Tag, "Can't proceed incoming call, your phone number is null!");
				return;
			}

			PhoneNumberUtil phoneNumberUtils = PhoneNumberUtil.GetInstance();
			PhoneNumber callerPhoneNumber = phoneNumberUtils.Parse(phoneNumber, context.Resources.Configuration.Locale.Country);
			PhoneNumber thisPhoneNumber =
				phoneNumberUtils.Parse(myPhoneNumber, context.Resources.Configuration.Locale.Country);
			if (callerPhoneNumber.CountryCode == thisPhoneNumber.CountryCode)
			{
				Log.Debug(Tag,
					$"Same for {phoneNumberUtils.Format(callerPhoneNumber, PhoneNumberFormat.INTERNATIONAL)}," +
					$" {phoneNumberUtils.Format(thisPhoneNumber, PhoneNumberFormat.INTERNATIONAL)}");
				return;
			}

			if (new File(context.FilesDir, PhoneUtils.ExcludedInCountryCodesFile).Exists())
			{
				string text;
				using (StreamReader reader =
					new StreamReader(context.OpenFileInput(PhoneUtils.ExcludedInCountryCodesFile)))
				{
					text = reader.ReadToEnd();
				}

				foreach (var line in text.Split())
				{
					if (int.TryParse(line, out var lineCode) && callerPhoneNumber.CountryCode == lineCode)
					{
						Log.Debug(Tag, $"Found {lineCode} in excluded incoming country codes");
						return;
					}
				}
			}

			using (StreamWriter writer =
				new StreamWriter(context.OpenFileOutput(PhoneUtils.SuspiciousCallsFile, FileCreationMode.Append)))
			{
				writer.WriteLine($"Suspicious call from {callerPhoneNumber.GetInternationalNumber()}");
			}

			NotificationSender sender = new NotificationSender(context, DataHolder.PhoneCategory);
			sender.PutNormalizeExtra(PhoneUtils.CountryCodeKey, callerPhoneNumber.CountryCode);
			sender.PutNormalizeExtra(PhoneUtils.IsOutgoingKey, false);
			sender.Send(NotificationSender.NotificationType.WarningNotification,
				$"Suspicious call from {callerPhoneNumber.GetInternationalNumber()}");
		}
	}
}