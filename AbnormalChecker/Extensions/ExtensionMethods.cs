using System.Linq;
using Java.Text;
using Java.Util;
using PhoneNumbers;

namespace AbnormalChecker.Extensions
{
	public static class StringExtensions
	{
		public static string AfterString(this string text, string after)
		{
			if (!text.Contains(after))
			{
				return string.Empty;
			}
			return text.Substring(text.IndexOf(after) + after.Length);
		}

		public static int AfterIndex(this string text, string after)
		{
			if (!text.Contains(after))
			{
				return 0;
			}
			return text.IndexOf(after) + after.Length;
		}
		
		public static string MakeFirstUpper(this string s)
		{
			return $"{s.First().ToString().ToUpper()}{s.Substring(1)}";
		}
	}
	
	public static class PhoneNumberExtensions {
		public static string GetInternationalNumber(this PhoneNumber number)
		{
			return PhoneNumberUtil.GetInstance().Format(number, PhoneNumberFormat.INTERNATIONAL);
		}
	}

	public static class DateTimeExtensions
	{
		public static string GetFormattedDateTime(this Date date)
		{
			SimpleDateFormat dateFormat = new SimpleDateFormat("dd.MM.yyyy, kk:mm:ss");
			return dateFormat.Format(date);
		}
	}
}