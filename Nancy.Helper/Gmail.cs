
using System;
using System.Configuration;
using System.Net;
using System.Net.Mail;

namespace Nancy.Helper
{
	public class Gmail
	{
		public static readonly string SmtpPort = ConfigurationManager.AppSettings["GmailSmtpPort"] ?? "587";
		public static readonly string SmtpHost = ConfigurationManager.AppSettings["GmailSmtpHost"] ?? "smtp.gmail.com";
		public static readonly string GmailAccount = ConfigurationManager.AppSettings["GmailAccount"] ?? "communications@goodpractice.com";
		public static readonly string GmailPassword = ConfigurationManager.AppSettings["GmailPassword"] ?? "Bs77kwdAeJ4s";

		public static Tuple<bool, string> Send(string toAddress, string subject, string body, bool isBodyHtml = false)
		{
			try
			{
				var fromDisplayName = "GoodPractice";
				var from = new MailAddress(GmailAccount, fromDisplayName);
				var to = new MailAddress(toAddress);

				new SmtpClient()
				{
					Host = SmtpHost,
					Port = int.Parse(SmtpPort),
					EnableSsl = true,
					Credentials = new NetworkCredential(GmailAccount, GmailPassword)
				}
				.Send(new MailMessage(from, to)
				{
					Subject = subject,
					Body = body,
					IsBodyHtml = isBodyHtml
				});
				return new Tuple<bool, string>(true, "Success. Your mail has been sent.");
			}
			catch (Exception ex)
			{
				return new Tuple<bool, string>(false, "Error. Your mail has not been sent. [" + ex.Message + "]");
			}
		}
	}
}
