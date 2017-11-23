using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Neurotoxin.Godspeed.Core.Extensions;
using Neurotoxin.Godspeed.Presentation.Extensions;
using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Neurotoxin.Godspeed.Shell.Commands;
using Neurotoxin.Godspeed.Shell.Constants;
using Neurotoxin.Godspeed.Shell.Interfaces;
using Neurotoxin.Godspeed.Shell.ViewModels;

namespace Neurotoxin.Godspeed.Shell.Views.Controls
{
    public partial class FileListPaneListView : DataGrid, IView<IFileListPaneViewModel>
    {
        public IFileListPaneViewModel ViewModel
        {
            get { return (IFileListPaneViewModel) DataContext; }
            private set { DataContext = value; }
        }

        public FileListPaneListView()
        {
            InitializeComponent();
            ItemContainerGenerator.StatusChanged += ItemContainerGeneratorStatusChanged;
            Sorting += GridOnSorting;
            SelectionChanged += GridOnSelectionChanged;
            DataContextChanged += GridOnDataContextChanged;
            CommandBindings.Add(new CommandBinding(FileListCommands.RenameTitleCommand, ExecuteRenameTitleCommand, CanExecuteRenameTitleCommand));
            CommandBindings.Add(new CommandBinding(FileListCommands.RenameFileSystemItemCommand, ExecuteRenameFileSystemItemCommand, CanExecuteRenameFileSystemItemCommand));
        }

        private void CanExecuteRenameTitleCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            var currentRow = ViewModel.CurrentRow;
            e.CanExecute = SanityCheckerViewModel.DataGridSupportsRenaming == true && currentRow != null && !currentRow.IsUpDirectory && currentRow.IsCached;
        }

        private void ExecuteRenameTitleCommand(object sender, ExecutedRoutedEventArgs e)
        {
            Rename(ColumnMode.Title);
        }

        private void CanExecuteRenameFileSystemItemCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            var currentRow = ViewModel.CurrentRow;
            e.CanExecute = SanityCheckerViewModel.DataGridSupportsRenaming == true && currentRow != null && !currentRow.IsUpDirectory && !ViewModel.IsReadOnly;
        }

        private void ExecuteRenameFileSystemItemCommand(object sender, ExecutedRoutedEventArgs e)
        {
            Rename(ColumnMode.Name);
        }

        private void Rename(ColumnMode mode)
        {
            var vm = ViewModel;
            var row = this.FindRowByValue(vm.CurrentRow);
            if (row == null) return;
            var cell = row.FirstCell();
            vm.EditColumnMode = mode;
            PreparingCellForEdit += GridOnPreparingCellForEdit;
            CellEditEnding += GridOnCellEditEnding;
            PreviewKeyDown += GridOnPreviewKeyDown;
            cell.IsEditing = true;
            vm.IsInEditMode = true;
        }

        private void GridOnPreparingCellForEdit(object sender, DataGridPreparingCellForEditEventArgs e)
        {
            e.EditingElement.LostFocus += GridCellEditingElementOnLostFocus;
        }

        private void GridOnCellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            var vm = ViewModel;
            var content = (ContentPresenter)e.EditingElement;
            content.LostFocus -= GridCellEditingElementOnLostFocus;
            CellEditEnding -= GridOnCellEditEnding;
            PreviewKeyDown -= GridOnPreviewKeyDown;
            if (e.EditAction != DataGridEditAction.Cancel)
            {
                var template = content.ContentTemplateSelector.SelectTemplate(null, content);
                var textBox = (TextBox) template.FindName("TitleEditBox", content);
                var newValue = textBox.Text.Trim();
                vm.Rename(vm.EditColumnMode, newValue);
            }
            vm.IsInEditMode = false;
        }

        private void GridOnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Escape) return;

            var grid = (DataGrid)sender;
            grid.CancelEdit();
        }

        private void GridCellEditingElementOnLostFocus(object sender, RoutedEventArgs e)
        {
            var cell = (ContentPresenter)sender;
            var grid = cell.FindAncestor<DataGrid>();
            grid.CancelEdit();
        }

        private void GridOnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            Dispatcher.Hooks.DispatcherInactive += HooksOnDispatcherInactive;
        }

        private static void GridOnSorting(object sender, DataGridSortingEventArgs e)
        {
            e.Handled = true;
        }

        private void GridOnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var currentRow = e.AddedItems.Count > 0 ? e.AddedItems[0] : null;
            if (currentRow == null) return;
            try
            {
                ScrollIntoView(currentRow);
            } 
            catch {}
            SetFocusToTheFirstCellOfCurrentRow();
        }

        private void HooksOnDispatcherInactive(object sender, EventArgs eventArgs)
        {
            Dispatcher.Hooks.DispatcherInactive -= HooksOnDispatcherInactive;
            PerformSort();
        }

        public void PerformSort()
        {
            var m = GetType().GetMethod("PerformSort", BindingFlags.Instance | BindingFlags.NonPublic);
            var vm = (IPaneViewModel)DataContext;
            if (vm == null) return;
            var settings = vm.Settings;
            var column = Columns.Single(c => c.SortMemberPath == settings.SortByField);
            column.SortDirection = settings.SortDirection == ListSortDirection.Ascending ? ListSortDirection.Descending : ListSortDirection.Ascending;
            m.Invoke(this, new object[] { column });
        }

        public void SetFocusToTheFirstCellOfCurrentRow()
        {
            if (!ViewModel.IsActive) return;
            if (ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated) return;
            if (SelectedItem == null) return;
            var row = ItemContainerGenerator.ContainerFromItem(SelectedItem) as DataGridRow;
            if (row == null) return;
            try
            {
                row.FirstCell().Focus();
            }
            catch { }
        }

        private void ItemContainerGeneratorStatusChanged(object sender, EventArgs e)
        {
            var generator = (ItemContainerGenerator)sender;
            if (generator.Status != GeneratorStatus.ContainersGenerated) return;
            SetFocusToTheFirstCellOfCurrentRow();
        }

        private void TitleEditBoxLoaded(object sender, RoutedEventArgs e)
        {
            var box = (TextBox)sender;
            box.SelectAll();
            box.Focus();
            box.ScrollToVerticalOffset(10);
        }

        private void OnContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            var row = (DataGridRow)sender;
            var item = (FileSystemItemViewModel)row.DataContext;
            if (item.Model.RecognitionState == RecognitionState.NotRecognized)
            {
                ((IFileListPaneViewModel)DataContext).Recognize(item);
            }
        }

        private void DataGridColumnHeaderOnClick(object sender, RoutedEventArgs e)
        {
            var column = ((DataGridColumnHeader)sender).Column;
            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control) && 
                column.SortMemberPath == FileListPaneViewModelBase<IFileManager>.COMPUTEDNAME)
            {
                ViewModel.SwitchTitleNameColumns(column);
            }
        }
    }
}