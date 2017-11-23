using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using Microsoft.Practices.Unity;
using Neurotoxin.Godspeed.Shell.Constants;
using Neurotoxin.Godspeed.Shell.Database.Models;
using Neurotoxin.Godspeed.Shell.Events;
using Neurotoxin.Godspeed.Shell.Exceptions;
using Neurotoxin.Godspeed.Shell.Extensions;
using Neurotoxin.Godspeed.Shell.Interfaces;
using Neurotoxin.Godspeed.Shell.Models;
using Neurotoxin.Godspeed.Shell.Views.Dialogs;
using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Neurotoxin.Godspeed.Presentation.Infrastructure.Constants;
using System.Linq;
using ServiceStack.OrmLite;
using Resx = Neurotoxin.Godspeed.Shell.Properties.Resources;
using Microsoft.Practices.Composite;

namespace Neurotoxin.Godspeed.Shell.ViewModels
{
    public class ConnectionsViewModel : PaneViewModelBase
    {
        private IStoredConnectionViewModel _previouslyFocusedItem;
        private readonly IDbContext _dbContext;

        #region Properties

        public FtpContentViewModel ConnectedFtp { get; private set; }

        private const string ITEMS = "Items";
        private ObservableCollection<IStoredConnectionViewModel> _items;
        public ObservableCollection<IStoredConnectionViewModel> Items
        {
            get { return _items; }
            private set { _items = value; NotifyPropertyChanged(ITEMS); }
        }

        private const string SELECTEDITEM = "SelectedItem";
        private IStoredConnectionViewModel _selectedItem;
        public IStoredConnectionViewModel SelectedItem
        {
            get { return _selectedItem; }
            set { _selectedItem = value; NotifyPropertyChanged(SELECTEDITEM); }
        }

        #endregion

        #region ConnectCommand

        public DelegateCommand<object> ConnectCommand { get; private set; }

        private bool CanExecuteConnectCommand(object cmdParam)
        {
            if (ConnectedFtp != null) return false;

            var mouseEvent = cmdParam as EventInformation<MouseEventArgs>;
            if (mouseEvent != null)
            {
                var e = mouseEvent.EventArgs;
                var dataContext = ((FrameworkElement)e.OriginalSource).DataContext;
                if (!(dataContext is IStoredConnectionViewModel)) return false;
            }

            var keyEvent = cmdParam as EventInformation<KeyEventArgs>;
            if (keyEvent != null)
            {
                var e = keyEvent.EventArgs;
                var dataContext = ((FrameworkElement)e.OriginalSource).DataContext;
                if (!(dataContext is IStoredConnectionViewModel)) return false;
                return e.Key == Key.Enter;
            }

            return SelectedItem != null;
        }

        private void ExecuteConnectCommand(object cmdParam)
        {
            var keyEvent = cmdParam as EventInformation<KeyEventArgs>;
            if (keyEvent != null) keyEvent.EventArgs.Handled = true;

            if (SelectedItem is NewConnectionPlaceholderViewModel)
            {
                Edit();
            }
            else if (SelectedItem is FtpConnectionItemViewModel)
            {
                ConnectedFtp = FtpConnect(SelectedItem);
            }
        }

        #endregion

        public ConnectionsViewModel(IDbContext dbContext)
        {
            _dbContext = dbContext;

            ConnectCommand = new DelegateCommand<object>(ExecuteConnectCommand, CanExecuteConnectCommand);
            Items = new ObservableCollection<IStoredConnectionViewModel>();

            EventAggregator.GetEvent<ConnectionDetailsChangedEvent>().Subscribe(OnConnectionDetailsChanged);
        }

        public override void LoadDataAsync(LoadCommand cmd, LoadDataAsyncParameters cmdParam, Action<PaneViewModelBase> success = null, Action<PaneViewModelBase, Exception> error = null)
        {
            base.LoadDataAsync(cmd, cmdParam, success, error);
            switch (cmd)
            {
                case LoadCommand.Load:
                    using (var db = _dbContext.Open())
                    {
                        Items.AddRange(db.Get<FtpConnection>().Select(c => new FtpConnectionItemViewModel(c)));
                    }
                    var add = new NewConnectionPlaceholderViewModel();
                    Items.Add(add);
                    break;
                case LoadCommand.Restore:
                    Save(cmdParam.Payload as FtpConnectionItemViewModel);
                    ConnectedFtp = null;
                    break;
            }
            if (success != null) success.Invoke(this);
        }

        public override void SetActive()
        {
            base.SetActive();
            SelectedItem = SelectedItem ?? _previouslyFocusedItem ?? Items.FirstOrDefault();
        }

        public override void Refresh(bool refreshCache)
        {
        }

        protected override void OnActivePaneChanged(ActivePaneChangedEventArgs e)
        {
            base.OnActivePaneChanged(e);
            if (e.ActivePane == this) return;
            _previouslyFocusedItem = SelectedItem;
            SelectedItem = null;
        }

        private void OnConnectionDetailsChanged(ConnectionDetailsChangedEventArgs e)
        {
            var ftpConnection = e.Connection as FtpConnectionItemViewModel;
            if (ftpConnection != null) Save(ftpConnection);
        }

        public void Edit()
        {
            FtpConnectionItemViewModel newItem;
            bool replace;
            var ftpconn = SelectedItem as FtpConnectionItemViewModel;
            if (ftpconn != null)
            {
                newItem = ftpconn.Clone();
                replace = true;
            } 
            else if (SelectedItem is NewConnectionPlaceholderViewModel)
            {
                var model = new FtpConnection
                                {
                                    Port = 21,
                                    ConnectionImage = (int) ConnectionImage.Fat,
                                };
                newItem = new FtpConnectionItemViewModel(model);
                replace = false;
            }
            else
            {
                throw new NotSupportedException();
            }

            var dialog = new NewConnectionDialog(Items.OfType<FtpConnectionItemViewModel>().Select(item => item.Name).ToList(), newItem);
            if (dialog.ShowDialog() != true) return;

            newItem.Name = newItem.Name.Trim();
            if (newItem.Username != null) newItem.Username = newItem.Username.Trim();
            if (newItem.Password != null) newItem.Password = newItem.Password.Trim();
            if (replace) Items.Remove(ftpconn);

            var i = 0;
            while (i < Items.Count - 1 && String.Compare(newItem.Name, Items[i].Name, StringComparison.InvariantCultureIgnoreCase) == 1) i++;
            Items.Insert(i, newItem);
            Save(newItem);
            SelectedItem = newItem;
        }

        public void Delete()
        {
            var ftpconn = SelectedItem as FtpConnectionItemViewModel;
            if (ftpconn == null) return;
            Items.Remove(SelectedItem);
            ftpconn.Model.MarkDeleted();
            Save(ftpconn);
        }

        private FtpContentViewModel FtpConnect(IStoredConnectionViewModel connection)
        {
            IsBusy = true;
            ProgressMessage = string.Format(Resx.ConnectingToFtp, connection.Name);
            var connectedFtp = Container.Resolve<FtpContentViewModel>();
            connectedFtp.LoadDataAsync(LoadCommand.Load, new LoadDataAsyncParameters(Settings.Clone("/"), connection), FtpConnectSuccess, FtpConnectError);
            return connectedFtp;
        }

        private void FtpConnectSuccess(PaneViewModelBase pane)
        {
            IsBusy = false;
            EventAggregator.GetEvent<OpenNestedPaneEvent>().Publish(new OpenNestedPaneEventArgs(this, pane));
        }

        private void FtpConnectError(PaneViewModelBase pane, Exception exception)
        {
            IsBusy = false;
            var connectionName = ((FtpContentViewModel)pane).Connection.Name;
            if (exception is SomethingWentWrongException)
            {
                WindowManager.ShowErrorMessage(exception);
            } 
            else
            {
                WindowManager.ShowMessage(Resx.ConnectionFailed, string.Format(Resx.CantConnectToFtp, connectionName, exception.Message));
            }
            ConnectedFtp = null;
        }

        private void Save(FtpConnectionItemViewModel connection)
        {
            if (connection == null) return;
            using (var db = _dbContext.Open())
            {
                db.Persist(connection.Model);
            }
        }

        public override object Close(object data)
        {
            Save(data as FtpConnectionItemViewModel);
            return base.Close(data);
        }
    }
}