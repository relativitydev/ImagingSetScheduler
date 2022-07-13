using System;
using System.Collections.Generic;
using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using KCD_1041539.ImagingSetScheduler.Context;
using KCD_1041539.ImagingSetScheduler.Helper;
using Relativity.API;

namespace KCD_1041539.ImagingSetScheduler.IoC
{
    public class WindsorFactory
    {
        public IWindsorContainer GetWindsorContainer(IAgentHelper agentHelper)
        {
            var container = new WindsorContainer();
            container.Kernel.AddFacility<TypedFactoryFacility>();

            container.Register(
                Component.For<IAgentHelper>()
                    .Forward<IHelper>()
                    .UsingFactoryMethod(() => agentHelper)
                    .LifestyleSingleton());

            container.Register(
                Component
                    .For<IContextContainerFactory>()
                    .ImplementedBy<ContextContainerFactory>()
                    .LifestyleTransient());
            container.Register(Component.For<IObjectManagerHelper>().ImplementedBy<ObjectManagerHelper>().LifestyleTransient());

            return container;
        }
    }
}
