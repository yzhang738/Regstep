using Ninject;
using Ninject.Web.Common;
using RSToolKit.Domain.Data;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace RSToolKit.WebUI.Infrastructure
{
    public class NinjectDependencyResolver : IDependencyResolver
    {
        private IKernel kernel;

        public NinjectDependencyResolver(IKernel kernelParam)
        {
            kernel = kernelParam;
            AddBindings();
        }

        public object GetService(Type serviceType)
        {
            return kernel.TryGet(serviceType);
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            return kernel.GetAll(serviceType);
        }

        private void AddBindings()
        {
            //kernel.Bind<IDataContext>().To<EFDbContext>().InRequestScope();
            //kernel.Bind<EFDbContext>().To<EFDbContext>().InRequestScope();
            //kernel.Bind<FormsRepository>().To<FormsRepository>().InRequestScope();
        }
    }
}