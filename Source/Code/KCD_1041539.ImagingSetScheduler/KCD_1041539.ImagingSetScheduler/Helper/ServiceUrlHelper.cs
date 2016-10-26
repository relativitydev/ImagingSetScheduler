using System;
using System.Reflection;
using KCD_1041539.ImagingSetScheduler.Database;
using Relativity.API;

namespace KCD_1041539.ImagingSetScheduler.Helper
{
	public static class ServiceUrlHelper
	{
		private static SqlQueryHelper SqlQueryHelper { get; set; }

		static ServiceUrlHelper()
		{
			SqlQueryHelper = new SqlQueryHelper();
		}

		public static IServicesMgr SetupServiceUrl(IDBContext eddsDbContext, IAgentHelper agentHelper)
		{
			IServicesMgr svcMgr = agentHelper.GetServicesManager();

			//make sure the service url is populated. this is important because agents are required to run on the web server and the service url will be not be populated in this setup.
			EnsureHelperStatus(svcMgr, eddsDbContext);

			return svcMgr;
		}

		public static void EnsureHelperStatus(IServicesMgr servicesMgr, IDBContext eddsDbContext)
		{
			var serviceUrl = servicesMgr.GetServicesURL();
			if (serviceUrl == null)
			{
				//set relativity services api url
				var relativityServicesApiUrl = SqlQueryHelper.GetRelativityServicesApiUrl(eddsDbContext);
				SetPrivateFieldValue(servicesMgr, "_rsapiUri", new Uri(relativityServicesApiUrl));

				//set relativity kepler api url
				var relativityKeplerApiUrl = SqlQueryHelper.GetRelativityServicesApiUrl(eddsDbContext);
				SetPrivateFieldValue(servicesMgr, "_keplerUri", new Uri(relativityKeplerApiUrl));
			}
		}

		private static void SetPrivateFieldValue<T>(this object obj, string propName, T val)
		{
			if (obj == null) throw new ArgumentNullException("obj");
			Type t = obj.GetType();
			FieldInfo fi = null;
			while (fi == null && t != null)
			{
				fi = t.GetField(propName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
				t = t.BaseType;
			}
			if (fi == null) throw new ArgumentOutOfRangeException("propName", string.Format("Field {0} was not found in Type {1}", propName, obj.GetType().FullName));
			fi.SetValue(obj, val);
		}
	}
}
