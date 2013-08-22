
namespace Raven.Helper
{
	using Raven.Client;
	using Raven.Client.Document;
	using Raven.Client.Linq;
	using System.Linq;
	using System.Threading;

	public static class Extensions
	{

		public static int QuickCount<T>(this IRavenQueryable<T> results)
		{
			RavenQueryStatistics stats;
			results.Statistics(out stats).Take(0).ToArray();
			return stats.TotalResults;
		}

		public static void ClearStaleIndexes(this IDocumentSession db)
		{
			while (((DocumentSession)db).DatabaseCommands.GetStatistics().StaleIndexes.Length != 0) Thread.Sleep(10);
		}

	}
}
