using System;
using System.IO;
using System.Linq;
using AbnormalChecker.Activities;
using AbnormalChecker.Extensions;
using AbnormalChecker.Utils;
using Android.Content;
using Android.Provider;
using Android.Telephony;
using Android.Util;
using PhoneNumbers;
using File = Java.IO.File;
using PhoneNumberFormat = PhoneNumbers.PhoneNumberFormat;

namespace AbnormalChecker.BroadcastReceivers
{
	public class SmsReceiver : BroadcastReceiver
	{
		private const string Tag = nameof(SmsReceiver);
		private static SmsReceiver _smsReceiver;

		public static void SetSmsReceiverStatus(Context context, bool enable)
		{
			if (enable && _smsReceiver == null)
			{
				var screenStateFilter = new IntentFilter();
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
			foreach (var message in Telephony.Sms.Intents.GetMessagesFromIntent(intent))
			{
				if (message == null) continue;

				var phoneNumber = message.DisplayOriginatingAddress;

				var telephonyManager = TelephonyManager.FromContext(context);
				if (phoneNumber == null)
				{
					Log.Error(Tag, "Can't proceed incoming sms, caller phone number is null!");
					return;
				}

				var myPhoneNumber = new string(telephonyManager.Line1Number.Where(char.IsDigit).ToArray());
				if (myPhoneNumber.Length == 0)
				{
					Log.Error(Tag, "Can't proceed incoming sms, your phone number is null!");
					return;
				}

				var phoneNumberUtils = PhoneNumberUtil.GetInstance();
				PhoneNumber callerPhoneNumber;
				try
				{
					callerPhoneNumber =
						phoneNumberUtils.Parse(phoneNumber, context.Resources.Configuration.Locale.Country);
				}
				catch (Exception e)
				{
					Log.Error(nameof(SmsReceiver), e.Message);
					return;
				}
				
				var thisPhoneNumber =
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
					using (var reader =
						new StreamReader(context.OpenFileInput(PhoneUtils.ExcludedInCountryCodesFile)))
					{
						text = reader.ReadToEnd();
					}

					foreach (var line in text.Split())
						if (int.TryParse(line, out var lineCode) && callerPhoneNumber.CountryCode == lineCode)
						{
							Log.Debug(Tag, $"Found {lineCode} in excluded incoming country codes");
							return;
						}
				}

				var warningMessage = string.Format(
					context.GetString(Resource.String.category_sms_notification_suspicious_message_from),
					callerPhoneNumber.GetInternationalNumber());

				using (var writer =
					new StreamWriter(context.OpenFileOutput(PhoneUtils.SuspiciousSmsFile, FileCreationMode.Append)))
				{
					writer.WriteLine(warningMessage);
				}

				using (var writer =
					new StreamWriter(context.OpenFileOutput(AlarmReceiver.CurrentSummaryFile, FileCreationMode.Append)))
				{
					writer.WriteLine(warningMessage);
				}

				var sender = new NotificationSender(context, DataHolder.SmsCategory);
				sender.PutNormalizeExtra(PhoneUtils.CountryCodeKey, callerPhoneNumber.CountryCode);
				sender.PutNormalizeExtra(PhoneUtils.IsOutgoingKey, false);
				sender.Send(NotificationType.WarningNotification, warningMessage);
				MainActivity.Adapter?.Refresh();
			}
		}
	}
}