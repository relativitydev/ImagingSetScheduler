using System;

namespace KCD_1041539.ImagingSetScheduler.Repository
{
	public interface IServicesProxyFactory
	{
		/// <summary>
		/// Create a service proxy as system.
		/// </summary>
		/// <typeparam name="T">The object type.</typeparam>
		/// <returns>A service proxy for that object type.</returns>
		T CreateServiceProxyAsSystem<T>() where T : IDisposable;

		/// <summary>
		/// Create a service proxy as user.
		/// </summary>
		/// <typeparam name="T">The object type.</typeparam>
		/// <returns>A service proxy for that object type.</returns>
		T CreateServiceProxy<T>() where T : IDisposable;
	}
}
