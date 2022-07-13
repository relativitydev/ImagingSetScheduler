using KCD_1041539.ImagingSetScheduler.Helper;
using Relativity.API;

namespace KCD_1041539.ImagingSetScheduler.Context
{
    public class ContextContainerFactory : IContextContainerFactory
    {
        private readonly IHelper _helper;

        public ContextContainerFactory(IHelper helper)
        {
            _helper = helper;
        }

        public IContextContainer BuildContextContainer()
        {
            IDBContext masterDbContext = _helper.GetDBContext(-1);
            IServicesMgr servicesMgr = _helper.GetServicesManager();
            IAPILog logger = _helper.GetLoggerFactory().GetLogger();
            IInstanceSettingManager instanceSettingManager = new InstanceSettingManager(_helper.GetInstanceSettingBundle());
            IServicesProxyFactory servicesProxyFactory = new ServicesProxyFactory(servicesMgr);

            return new ContextContainer(
                masterDbContext,
                servicesMgr,
                logger,
                instanceSettingManager,
                servicesProxyFactory);
        }
    }
}
