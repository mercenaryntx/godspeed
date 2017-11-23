using Microsoft.Practices.Unity;
using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Neurotoxin.Godspeed.Shell.Events;
using Neurotoxin.Godspeed.Shell.Interfaces;

namespace Neurotoxin.Godspeed.Shell.ViewModels
{
    public abstract class CommonViewModelBase : ViewModelBase
    {
        public readonly IWindowManager WindowManager;
        public readonly IResourceManager ResourceManager;

        protected CommonViewModelBase()
        {
            WindowManager = Container.Resolve<IWindowManager>();
            ResourceManager = Container.Resolve<IResourceManager>();
        }

        public override void RaiseCanExecuteChanges()
        {
            base.RaiseCanExecuteChanges();
            EventAggregator.GetEvent<RaiseCanExecuteChangesEvent>().Publish(new RaiseCanExecuteChangesEventArgs(this));
        }
    }
}