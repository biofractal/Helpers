

namespace Raven.Helper
{
	using FakeItEasy;
	using Raven.Client;
	using Raven.Client.Document;
	using Raven.Client.Indexes;
	using System;
	using System.Configuration;

	public interface IRavenSessionProvider
	{
		bool SessionInitialized { get; set; }
		void SaveChangesAfterRequest();
		IDocumentSession Get();
	}

	public class RavenSessionProvider<T> : IRavenSessionProvider
	{
		private static IDocumentStore documentStore;
		private IDocumentSession documentSession;
		public bool SessionInitialized { get; set; }

		public IDocumentSession Get()
		{
			SessionInitialized = true;
			documentSession = documentSession ?? (documentSession = DocumentStore.OpenSession());
			return documentSession;
		}

		public static IRavenSessionProvider Fake(T instanceToLoad)
		{
			var fakeDocumentSession = A.Fake<IDocumentSession>();
			A.CallTo(() => fakeDocumentSession.Load<T>(A<string>.Ignored)).Returns(instanceToLoad);
			var fakeRavenSessionProvider = A.Fake<IRavenSessionProvider>();
			A.CallTo(() => fakeRavenSessionProvider.Get()).Returns(fakeDocumentSession);
			return fakeRavenSessionProvider;
		}

		public void SaveChangesAfterRequest()
		{
			if (!this.SessionInitialized) return;
			documentSession.SaveChanges();
			documentSession.Dispose();
		}

		private static IDocumentStore DocumentStore
		{
			get
			{
				if (documentStore != null) return documentStore;
				lock (typeof(RavenSessionProvider<T>))
				{
					if (documentStore != null) return documentStore;

					documentStore = new DocumentStore
					{
						ConnectionStringName = ConnectionStringName
					};
					documentStore.Initialize();
					IndexCreation.CreateIndexes(typeof(T).Assembly, documentStore);
				}
				return documentStore;
			}
		}

		private static string ConnectionStringName
		{
			get
			{
				var customConnection = ConfigurationManager.ConnectionStrings[Environment.MachineName] != null;
				var connectionStringName = customConnection ? Environment.MachineName : "RavenDB";
				return connectionStringName;
			}
		}

	}
}
