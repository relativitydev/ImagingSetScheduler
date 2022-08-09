using KCD_1041539.ImagingSetScheduler.Helper;
using Relativity.API;

namespace KCD_1041539.ImagingSetScheduler.Context
{
    public class ContextContainer : IContextContainer
    {
        public IDBContext MasterDbContext { get; }
        public IServicesMgr ServicesMgr { get; }
        public IAPILog Logger { get; }
        public IInstanceSettingManager InstanceSettingManager { get; }
        public IServicesProxyFactory ServicesProxyFactory { get; }
		public ExecutionIdentity Identity { get; }

        public ContextContainer(IDBContext masterDbContext, IServicesMgr servicesMgr, IAPILog logger, IInstanceSettingManager instanceSettingManager, IServicesProxyFactory servicesProxyFactory, ExecutionIdentity identity)
        {
            MasterDbContext = masterDbContext;
            ServicesMgr = servicesMgr;
            Logger = logger;
            InstanceSettingManager = instanceSettingManager;
            ServicesProxyFactory = servicesProxyFactory;
            Identity = identity;
        }
    }
}
