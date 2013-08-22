using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Nancy.Helper
{
	//This mad bit of boilerplate is the MSDN recommended way of checking whether an email is a valid format
	public static class RegexUtilities
	{
		public static bool IsValidEmail(string candidate)
		{
			if (String.IsNullOrWhiteSpace(candidate)) return false;
			bool invalid = false;
			// Use IdnMapping class to convert Unicode domain names. 
			try
			{
				candidate = Regex.Replace(candidate, @"(@)(.+)$",
				(match) =>
				{
					// IdnMapping class with default property values.
					IdnMapping idn = new IdnMapping();

					string domainName = match.Groups[2].Value;
					try
					{
						domainName = idn.GetAscii(domainName);
					}
					catch (ArgumentException)
					{
						invalid = true;
					}
					return match.Groups[1].Value + domainName;
				},
				RegexOptions.None,
				TimeSpan.FromMilliseconds(200));
			}
			catch (RegexMatchTimeoutException)
			{
				return false;
			}

			if (invalid) return false;

			// Return true if strIn is in valid e-mail format. 
			try
			{
				return Regex.IsMatch(candidate,
					  @"^(?("")(""[^""]+?""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
					  @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9]{2,17}))$",
					  RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
			}
			catch (RegexMatchTimeoutException)
			{
				return false;
			}
		}
	}
}