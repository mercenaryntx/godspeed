using System;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Neurotoxin.Godspeed.Shell.Commands;
using Neurotoxin.Godspeed.Shell.Constants;
using Neurotoxin.Godspeed.Shell.Interfaces;

namespace Neurotoxin.Godspeed.Shell.Views.Controls
{
    public partial class FileListPaneContentView : ListBox, IView<IFileListPaneViewModel>
    {
        public IFileListPaneViewModel ViewModel
        {
            get { return (IFileListPaneViewModel)DataContext; }
            private set { DataContext = value; }
        }

        public FileListPaneContentView()
        {
            InitializeComponent();
            ItemContainerGenerator.StatusChanged += ItemContainerGeneratorStatusChanged;
            SelectionChanged += OnSelectionChanged;
            CommandBindings.Add(new CommandBinding(FileListCommands.RenameTitleCommand, ExecuteRenameTitleCommand, CanExecuteRenameTitleCommand));
            CommandBindings.Add(new CommandBinding(FileListCommands.RenameFileSystemItemCommand, ExecuteRenameFileSystemItemCommand, CanExecuteRenameFileSystemItemCommand));
        }

        private void ItemContainerGeneratorStatusChanged(object sender, EventArgs e)
        {
            var generator = (ItemContainerGenerator)sender;
            if (generator.Status != GeneratorStatus.ContainersGenerated) return;
            SetFocusToCurrentRow();
        }

        public void SetFocusToCurrentRow()
        {
            if (ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated) return;
            if (SelectedItem == null) return;
            var row = ItemContainerGenerator.ContainerFromItem(SelectedItem) as ListBoxItem;
            if (row == null) return;
            try
            {
                row.Focus();
            }
            catch { }
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var currentRow = e.AddedItems.Count > 0 ? e.AddedItems[0] : null;
            if (currentRow == null) return;
            try
            {
                ScrollIntoView(currentRow);
            }
            catch { }
            SetFocusToCurrentRow();
        }

        private void CanExecuteRenameTitleCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            var currentRow = ViewModel.CurrentRow;
            e.CanExecute = currentRow != null && !currentRow.IsUpDirectory && currentRow.IsCached;
        }

        private void ExecuteRenameTitleCommand(object sender, ExecutedRoutedEventArgs e)
        {
            Rename(ColumnMode.Title);
        }

        private void CanExecuteRenameFileSystemItemCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            var currentRow = ViewModel.CurrentRow;
            e.CanExecute = currentRow != null && !currentRow.IsUpDirectory && !ViewModel.IsReadOnly;
        }

        private void ExecuteRenameFileSystemItemCommand(object sender, ExecutedRoutedEventArgs e)
        {
            Rename(ColumnMode.Name);
        }

        private void Rename(ColumnMode mode)
        {
            ViewModel.Rename(mode);
        }
    }
}
