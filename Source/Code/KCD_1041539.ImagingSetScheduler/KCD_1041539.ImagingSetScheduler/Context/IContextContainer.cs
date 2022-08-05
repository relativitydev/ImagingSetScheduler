using KCD_1041539.ImagingSetScheduler.Helper;
using Relativity.API;

namespace KCD_1041539.ImagingSetScheduler.Context
{
    public interface IContextContainer
    {
		/// <summary>
		/// Create an instance of IDBContext and read the value.
		/// </summary>
		/// <returns>Property value of MasterDbContext.</returns>
		IDBContext MasterDbContext { get; }

		/// <summary>
		/// Create an instance of IServicesMgr and read the value.
		/// </summary>
		/// <returns>Property value of ServicesMgr.</returns>
		IServicesMgr ServicesMgr { get; }

		/// <summary>
		/// Create an instance of IAPILog and read the value.
		/// </summary>
		/// <returns>Property value of Logger.</returns>
		IAPILog Logger { get; }

		/// <summary>
		/// Create an instance of IInstanceSettingManager and read the value.
		/// </summary>
		/// <returns>Property value of InstanceSettingManager.</returns>
		IInstanceSettingManager InstanceSettingManager { get; }

		/// <summary>
		/// Create an instance of IServicesProxyFactory and read the value.
		/// </summary>
		/// <returns>Property value of ServicesProxyFactory.</returns>
		IServicesProxyFactory ServicesProxyFactory { get; }

		/// <summary>
		/// Create an instance of ExecutionIdentity and read the value.
		/// </summary>
		/// <returns>Property value of Identity.</returns>
		ExecutionIdentity Identity { get; }
	}
}
