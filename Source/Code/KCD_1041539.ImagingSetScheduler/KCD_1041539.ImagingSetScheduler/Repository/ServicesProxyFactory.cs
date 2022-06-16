using System;
using Relativity.API;

namespace KCD_1041539.ImagingSetScheduler.Repository
{
	public class ServicesProxyFactory : IServicesProxyFactory
	{
		private readonly IServicesMgr _servicesManager;

		public ServicesProxyFactory(IServicesMgr servicesManager)
		{
			_servicesManager = servicesManager;
		}

		public T CreateServiceProxyAsSystem<T>() where T : IDisposable
		{
			return _servicesManager.CreateProxy<T>(ExecutionIdentity.System);
		}

		public T CreateServiceProxy<T>() where T : IDisposable
		{
			return _servicesManager.CreateProxy<T>(ExecutionIdentity.CurrentUser);
		}
	}
}
