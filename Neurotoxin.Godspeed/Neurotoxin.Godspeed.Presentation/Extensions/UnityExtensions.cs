using System;
using System.Linq;
using Microsoft.Practices.Unity;
using Neurotoxin.Godspeed.Presentation.Infrastructure;

namespace Neurotoxin.Godspeed.Presentation.Extensions
{
    public static class UnityExtensions
    {
        public static TView Resolve<TView, TViewModel>(this IUnityContainer container, TViewModel vmInstance = null) 
            where TView : IView 
            where TViewModel : class, IViewModel
        {
            var viewType = typeof (TView);
            var vmType = typeof (TViewModel);
            var constructors = viewType.GetConstructors();
            if (!constructors.Any())
                throw new ArgumentException(string.Format("View type {0} doesn't have any public constructors", viewType.Name));
            if (constructors.Count() > 1) 
                throw new ArgumentException(string.Format("View type {0} cannot have multiple constructors", viewType.Name));

            var parameters = constructors.First().GetParameters();
            var vmParam = parameters.FirstOrDefault(p => p.ParameterType.IsAssignableFrom(vmType));
            if (vmParam == null)
                throw new ArgumentException(string.Format("View type {0} constructor doesn't have any IViewModel based parameter", viewType.Name));

            var vm = vmInstance ?? container.Resolve<TViewModel>();
            return container.Resolve<TView>(new ParameterOverride(vmParam.Name, vm));
        }
    }
}