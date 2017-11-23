using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Markup;
using System.Collections.ObjectModel;
using Neurotoxin.Godspeed.Presentation.Attributes;
using Neurotoxin.Godspeed.Presentation.Events;
using Neurotoxin.Godspeed.Presentation.Infrastructure.Constants;

namespace Neurotoxin.Godspeed.Presentation.Infrastructure
{
    public abstract class ModuleViewModelBase : ViewModelBase
    {
        #region String constants

        public const string ISINPROGRESS = "IsInProgress";
        public const string ISINDIALOG = "IsInDialog";
        public const string SELECTEDITEM = "SelectedItem";
        public const string SELECTEDITEMS = "SelectedItems";

        #endregion

        #region Fields

        protected int asyncCounter;
        protected object asyncCounterSyncObj = new object();

        #endregion

        #region Commands

        public DelegateCommand<object> SubmitCommand { get; set; }
        public DelegateCommand<object> CloseDialogCommand { get; set; }

        protected virtual void ExecuteSubmitCommand(object commandParameter)
        {
            Submit();
        }

        protected virtual bool CanExecuteSubmitCommand(object commandParameter)
        {
            return !IsInProgress;
        }

        protected virtual void ExecuteCloseDialogCommand(object commandParameter)
        {
            CloseDialog(commandParameter);
        }

        protected virtual bool CanExecuteCloseDialogCommand(object commandParameter)
        {
            return IsInDialog;
        }

        #endregion

        #region Properties

        public const string LOADINFO = "LoadInfo";
        private ModuleLoadInfo loadInfo;
        public ModuleLoadInfo LoadInfo
        {
            get { return loadInfo; }
            set
            {
                loadInfo = value;
                if (ModuleLoadInfoChanged != null) ModuleLoadInfoChanged(this, new EventArgs());
                NotifyPropertyChanged(LOADINFO);
            }
        }

        private bool isInProgress;
        public bool IsInProgress
        {
            get
            {
                return isInProgress;
            }
            set
            {
                isInProgress = value;
                NotifyPropertyChanged("IsInProgress");
                NotifyPropertyChanged("IsNotInProgress");
                RaiseCanExecuteChanges();
            }
        }

        public bool IsNotInProgress
        {
            get { return !IsInProgress; }
            set { IsInProgress = !value; }
        }

        public bool IsInDialog
        {
            get { return false; }
        }

        public virtual string WindowTitle
        {
            get { return LoadInfo.Title; }
        }

        private int loadingQueueLength;
        public int LoadingQueueLength
        {
            get { return loadingQueueLength; }
            set { loadingQueueLength = value; NotifyPropertyChanged("LoadingQueueLength"); }
        }

        private int loadingProgress;
        public int LoadingProgress
        {
            get { return loadingProgress; }
            set { loadingProgress = value; NotifyPropertyChanged("LoadingProgress"); }
        }

        private string loadingInfo = String.Empty;
        public string LoadingInfo
        {
            get { return loadingInfo; }
            set { loadingInfo = value; NotifyPropertyChanged("LoadingInfo"); }
        }

        private string statusBarText = String.Empty;
        public string StatusBarText
        {
            get { return statusBarText; }
            set { statusBarText = value; NotifyPropertyChanged("StatusBarText"); }
        }

        internal const string ERRORS = "Errors";
        private ObservableCollection<Exception> errors = new ObservableCollection<Exception>();
        public ObservableCollection<Exception> Errors
        {
            get { return errors; }
        }

        #endregion

        #region Constructor

        public ModuleViewModelBase()
        {
            SubmitCommand = new DelegateCommand<object>(ExecuteSubmitCommand, CanExecuteSubmitCommand);
            CloseDialogCommand = new DelegateCommand<object>(ExecuteCloseDialogCommand, CanExecuteCloseDialogCommand);
        }

        #endregion

        #region Methods

        public virtual object GetModel()
        {
            return null;
        }

        /// <summary>
        /// Indicates the VM contains a Model that has dirty values.
        /// </summary>
        /// <returns></returns>
        public abstract bool HasDirty();

        /// <summary>
        /// Forces to reset every dirty flags in the Model
        /// </summary>
        protected abstract void ResetDirtyFlags();

        /// <summary>
        /// Returns true if the given property is dirty in the Model
        /// </summary>
        /// <param name="propertyName">POCO property name</param>
        /// <returns></returns>
        public abstract bool IsDirty(string propertyName);

        /// <summary>
        /// Sends a RequestWindowCloseEvent event.
        /// </summary>
        /// <param name="dialogResult">The dialog result to be set when closing the dialog window.</param>
        protected void RequestCloseWindow(bool? dialogResult)
        {
            EventAggregator.GetEvent<RequestWindowCloseEvent>().Publish(new RequestWindowCloseEventArgs(this, dialogResult));
        }

        /// <summary>
        /// (Starts to) initialize the viewmodel with the specified parameters.
        /// </summary>
        //[HandleException]
        public abstract void LoadDataAsync(LoadCommand cmd, object cmdParam);

        public virtual void NotifyIndicator(DataLoadState state, string section, string message)
        {
            //eventAggregator.GetEvent<LogEvent>().Publish(new LoggerMessage(message));
            switch (state)
            {
                case DataLoadState.Loading:
                    LoadStarted(section, message);
                    break;
                case DataLoadState.Loaded:
                    LoadFinished(section, message);
                    break;
            }
        }

        protected virtual void OnModelRootChanged()
        {
            foreach (var modelProp in CollectModelProperties())
                NotifyPropertyChanged(modelProp.Name);
        }

        protected IEnumerable<PropertyInfo> CollectModelProperties()
        {
            return GetType().GetProperties().Where(s => s.GetCustomAttributes(typeof(ModelPropertyAttribute), true).Any());
        }


        /// <summary>
        /// The default action of every ViewModel
        /// </summary>
        public virtual void Submit()
        {
        }

        /// <summary>
        /// Closes the dialog if IsInDialog is true
        /// </summary>
        public virtual void CloseDialog(object arg)
        {
            //eventAggregator.GetEvent<ModulePopupCloseEvent>().Publish(arg);
        }

        /// <summary>
        /// Forces to re-evaluate CanExecute on the Prism commands 
        /// </summary>
        public override void RaiseCanExecuteChanges()
        {
            base.RaiseCanExecuteChanges();

            SubmitCommand.RaiseCanExecuteChanged();
            CloseDialogCommand.RaiseCanExecuteChanged();
        }

        public virtual FrameworkElement GetStatusBarTemplate()
        {
            //UNDONE zsbangha 2010-05-26: Assembly logic
            Stream templateStream = this.GetType().Assembly.GetManifestResourceStream("Mti.Mnp.Client.Wpf.Common.Templates.StatusBarTemplateBase.xaml");
            return XamlReader.Load(templateStream) as FrameworkElement;
        }

        protected const string SPACE = " ";

        public virtual void LoadStarted(string section, string msg)
        {
            if (asyncCounter == 0) LoadingProgress = 0;
            lock (asyncCounterSyncObj)
            {
                if (!IsDisposed)
                    RaiseDataLoading(++asyncCounter, section, msg);
            }
            LoadingInfo = msg;
            LoadingQueueLength = asyncCounter;
        }

        public virtual void LoadFinished(string section, string msg)
        {
            lock (asyncCounterSyncObj)
            {
                if (!IsDisposed)
                    RaiseDataLoaded(--asyncCounter, section, msg);
            }
            LoadingInfo = msg;
            LoadingProgress++;
        }

        public virtual bool IsLoadInfoSerializable()
        {
            return false;
        }

        public override void Dispose()
        {
            base.Dispose();
            lock (asyncCounterSyncObj)
            {
                if (asyncCounter > 0)
                {
                    int copyOfAsyncCounter = asyncCounter;
                    UIThread.Run(
                        () =>
                        {
                            for (; copyOfAsyncCounter > 0; copyOfAsyncCounter--)
                                RaiseDataLoaded(copyOfAsyncCounter, string.Empty, string.Empty);
                        });
                    asyncCounter = 0;
                }
            }
        }

        #endregion

        #region Events

        public event EventHandler ModuleLoadInfoChanged;

        public event DataLoadEventHandler DataLoading;
        protected virtual void RaiseDataLoading(int counter, string section, string msg)
        {
            var args = new DataLoadEventArgs(this, DataLoadState.Loading, section, msg);
            EventAggregator.GetEvent<DataLoadingEvent>().Publish(args);
            if (DataLoading != null) DataLoading(this, args);
        }

        public event DataLoadEventHandler DataLoaded;
        protected virtual void RaiseDataLoaded(int counter, string section, string msg)
        {
            var args = new DataLoadEventArgs(this, DataLoadState.Loaded, section, msg);
            EventAggregator.GetEvent<DataLoadedEvent>().Publish(args);
            if (DataLoaded != null) DataLoaded(this, args);
        }

        #endregion

        protected override void NotifyPropertyChanged(string propertyName)
        {
            base.NotifyPropertyChanged(propertyName);
            if (propertyName == LOADINFO) NotifyPropertyChanged(ISINDIALOG);
        }
    }
}