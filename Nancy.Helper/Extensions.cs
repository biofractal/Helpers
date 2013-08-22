using Nancy.ViewEngines.Razor;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Nancy.Helper
{
	public static class Extensions
	{
		#region String
		public static bool IsGuid(this string str)
		{
			Guid guid;
			return Guid.TryParse(str, out guid);
		}
		public static bool IsEmail(this string str)
		{
			return RegexUtilities.IsValidEmail(str);
		}
		public static dynamic ToDynamic(this string str)
		{
			return JsonConvert.DeserializeObject<dynamic>(str);
		}
		#endregion

		#region Http
		public static bool IsOK(this System.Net.HttpStatusCode code)
		{
			return code == System.Net.HttpStatusCode.OK;
		}

		public static bool IsOK(this Nancy.HttpStatusCode code)
		{
			return code == Nancy.HttpStatusCode.OK;
		}
		#endregion

		#region Nancy Request
		public static string GetParam(this Request request, string name)
		{
			if (request.Query[name].HasValue) return (string)request.Query[name];
			if (request.Form[name].HasValue) return (string)request.Form[name];
			return string.Empty;
		}
		#endregion

		#region Nancy Response
		public static dynamic GetContents(this Response response)
		{
			using (var stream = new MemoryStream())
			{
				using (var reader = new StreamReader(stream))
				{
					response.Contents(stream);
					stream.Position = 0;
					return reader.ReadToEnd().ToDynamic();
				}
			}
		}
		#endregion

		#region Nancy Module
		public static void NoCache(this NancyModule module)
		{
			module.After.AddItemToEndOfPipeline(x =>
			{
				x.Response.WithHeader("Cache-Control", "No-store");
			});
		}

		public static void CheckForIfNoneMatch(this NancyContext context)
		{
			var request = context.Request;
			var response = context.Response;

			string responseETag;
			if (!response.Headers.TryGetValue("ETag", out responseETag)) return;
			if (request.Headers.IfNoneMatch.Contains(responseETag))
			{
				context.Response = HttpStatusCode.NotModified;
			}
		}

		public static void CheckForIfModifiedSince(this NancyContext context)
		{
			var request = context.Request;
			var response = context.Response;

			string responseLastModified;
			if (!response.Headers.TryGetValue("Last-Modified", out responseLastModified)) return;
			DateTime lastModified;

			if (!request.Headers.IfModifiedSince.HasValue || !DateTime.TryParseExact(responseLastModified, "R", CultureInfo.InvariantCulture, DateTimeStyles.None, out lastModified)) return;
			if (lastModified <= request.Headers.IfModifiedSince.Value)
			{
				context.Response = HttpStatusCode.NotModified;
			}
		}


		#endregion

		#region Razor

		public static IHtmlString CheckBox<T>(this HtmlHelpers<T> helper, string id, bool isChecked = false, string @class = null)
		{
			return GetCheckableInput<T>("checkbox", id, isChecked, @class);
		}

		public static IHtmlString RadioButton<T>(this HtmlHelpers<T> helper, string id, bool isChecked = false, string @class = null)
		{
			return GetCheckableInput<T>("radio", id, isChecked, @class);
		}

		private static IHtmlString GetCheckableInput<T>(string type, string id, bool isChecked = false, string @class = null)
		{
			var html = new StringBuilder("<input type=\"" + type + "\" value=\"true\"");
			if (id != null)
			{
				html.Append(" name=\"" + id + "\"");
				html.Append(" id=\"" + id + "\"");
			}
			if (isChecked) html.Append(" checked");
			if (@class != null) html.Append(" class=\"" + @class + "\"");
			html.Append(" />");
			return new NonEncodedHtmlString(html.ToString());
		}

		#endregion

		#region Dictionary
		public static bool ContainsItems(this Dictionary<string, object> dictionary, params string[] keys)
		{
			if (dictionary == null || keys == null) return false;
			foreach (var key in keys)
			{
				if (!dictionary.ContainsKey(key)) return false;
				if (dictionary[key] == null) return false;
			}
			return true;
		}
		#endregion
	}
}
