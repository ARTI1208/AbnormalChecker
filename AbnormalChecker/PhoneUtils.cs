using PhoneNumbers;

namespace AbnormalChecker
{
	public static class PhoneUtils
	{
		public const string CountryCodeKey = "country_code";
		public const string IsOutgoingKey = "outgoing_call";
		public const string ExcludedOutCountryCodesFile = "excluded_out_codes.txt";
		public const string ExcludedInCountryCodesFile = "excluded_in_codes.txt";
		public const string SuspiciousCallsFile = "suspicious_calls.txt";
		
//		public const string ExcludedOutCountryCodesFile = "excluded_out_codes.txt";
//		public const string ExcludedInCountryCodesFile = "excluded_in_codes.txt";
		public const string SuspiciousSmsFile = "suspicious_sms.txt";
		
	}

}