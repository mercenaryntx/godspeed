using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Unity;
using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Neurotoxin.Godspeed.Shell.Commands;
using Neurotoxin.Godspeed.Shell.Constants;
using Neurotoxin.Godspeed.Shell.Events;
using Neurotoxin.Godspeed.Shell.Interfaces;
using Neurotoxin.Godspeed.Shell.Views.Controls;

namespace Neurotoxin.Godspeed.Shell.Views
{
    public partial class FileListPane : UserControl, IView<IFileListPaneViewModel>
    {
        public IFileListPaneViewModel ViewModel
        {
            get { return (IFileListPaneViewModel)DataContext; }
            private set { DataContext = value; }
        }

        public FileListPane()
        {
            InitializeComponent();
            var eventAggregator = UnityInstance.Container.Resolve<IEventAggregator>();
            eventAggregator.GetEvent<ActivePaneChangedEvent>().Subscribe(OnActivePaneChanged);
            eventAggregator.GetEvent<FileListPaneViewModelContentSortedEvent>().Subscribe(OnContentSorted);

            DriveDropdown.SelectionChanged += DriveDropdownOnSelectionChanged;
        }

        private void DriveDropdownOnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext == null) return;
            var selectedDrive = ((IFileListPaneViewModel) DataContext).Drive;
            if (DriveDropdown.SelectedValue == selectedDrive) return;
            DriveDropdown.SelectedValue = selectedDrive;
        }

        private void OnActivePaneChanged(ActivePaneChangedEventArgs e)
        {
            if (DataContext != e.ActivePane) return;
            var view = GetView();
            var listView = view as FileListPaneListView;
            if (listView != null) listView.SetFocusToTheFirstCellOfCurrentRow();

            var contentView = view as FileListPaneContentView;
            if (contentView != null) contentView.SetFocusToCurrentRow();
        }

        private void OnContentSorted(FileListPaneViewModelContentSortedEventArgs e)
        {
            if (DataContext != e.Sender) return;
            var listView = GetView() as FileListPaneListView;
            if (listView != null) listView.PerformSort();
        }

        private DependencyObject GetView()
        {
            return VisualTreeHelper.GetChild(VisualTreeHelper.GetChild(View, 0), 0);
        }
    }
}