using KCD_1041539.ImagingSetScheduler.Helper;
using Relativity.API;

namespace KCD_1041539.ImagingSetScheduler.Context
{
    public interface IContextContainer
    {
        IDBContext MasterDbContext { get; }
        IServicesMgr ServicesMgr { get; }
        IAPILog Logger { get; }
        IInstanceSettingManager InstanceSettingManager { get; }
        IServicesProxyFactory ServicesProxyFactory { get; }
    }
}
