using System;
using System.ComponentModel;
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Unity;

namespace Neurotoxin.Godspeed.Presentation.Infrastructure
{
    public abstract class ViewModelBase : IViewModel
    {
        protected readonly IUnityContainer Container;

        public IEventAggregator EventAggregator { get; private set; }
        public IWorkHandler WorkHandler { get; private set; }

        public bool IsDisposed { get; private set; }

        private const string ISBUSY = "IsBusy";
        private bool _isBusy;
        public bool IsBusy
        {
            get { return _isBusy; }
            set { _isBusy = value; NotifyPropertyChanged(ISBUSY); }
        }

        public ViewModelBase()
        {
            Container = UnityInstance.Container;
            EventAggregator = Container.Resolve<IEventAggregator>();
            WorkHandler = Container.Resolve<IWorkHandler>();
        }

        /// <summary>
        /// Forces to re-evaluate CanExecute on the commands 
        /// </summary>
        public virtual void RaiseCanExecuteChanges()
        {
        }

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises the PropertyChanged event.
        /// </summary>
        protected virtual void NotifyPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
            RaiseCanExecuteChanges();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public virtual void Dispose()
        {
            IsDisposed = true;
        }

    }
}