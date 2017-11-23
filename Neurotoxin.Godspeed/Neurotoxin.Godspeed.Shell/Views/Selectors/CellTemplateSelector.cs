using System.Windows;
using System.Windows.Controls;
using Neurotoxin.Godspeed.Presentation.Extensions;
using Neurotoxin.Godspeed.Shell.Constants;
using Neurotoxin.Godspeed.Shell.Views.Controls;

namespace Neurotoxin.Godspeed.Shell.Views.Selectors
{
    public class CellTemplateSelector : DataTemplateSelector
    {
        public string SelectorMember { get; set; }
        public DataTemplate TitleTemplate { get; set; }
        public DataTemplate NameTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var grid = ((FrameworkElement)container).FindAncestor<FileListPaneListView>();
            var vm = grid.DataContext;
            var pi = vm.GetType().GetProperty(SelectorMember);
            var columnMode = pi.GetValue(vm, null) as ColumnMode?;
            return columnMode == ColumnMode.Name ? NameTemplate : TitleTemplate;
        }
    }
}