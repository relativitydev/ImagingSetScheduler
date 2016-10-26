using System;
using Relativity.API;
using kCura.Relativity.Client;

namespace KCD_1041539.ImagingSetSchedule.NUnit.Helper
{
	public class ServiceManager : IServicesMgr
	{
		private Uri Rsapiuri { get; set; }
		private RSAPIClient Client { get; set; }

		public ServiceManager(Uri uri, RSAPIClient client)
		{
			Rsapiuri = uri;
			Client = client;
		}

		public T CreateProxy<T>(ExecutionIdentity ident) where T : IDisposable
		{
			return (T)(object)Client;
		}

		public Uri GetRESTServiceUrl()
		{
			throw new NotImplementedException();
		}

		public Uri GetServicesURL()
		{
			return Rsapiuri;
		}
	}
}
