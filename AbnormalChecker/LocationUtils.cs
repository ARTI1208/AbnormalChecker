namespace AbnormalChecker
{
	public static class LocationUtils
	{
		public const string LocationCoordinatesFile = "saved_coordinates.txt";
		public const string LocationLatitude = "location_latitude";
		public const string LocationLongitude = "location_longitude";
		public const string LocationCoordinatesPattern = @"Latitude = [0-9\.]\,+ Longitude = [0-9\.]+";
	}
	
	public static class StringExtension
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
	} 
}