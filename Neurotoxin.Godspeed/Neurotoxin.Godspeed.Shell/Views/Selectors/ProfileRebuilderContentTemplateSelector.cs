using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using Neurotoxin.Godspeed.Shell.ViewModels;

namespace Neurotoxin.Godspeed.Shell.Views.Selectors
{
    public class ProfileRebuilderContentTemplateSelector : DataTemplateSelector
    {
        public DataTemplate DefaultTemplate { get; set; }
        public DataTemplate FileStructureTemplate { get; set; }
        public DataTemplate GpdContentTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var tabItem = (ProfileRebuilderTabItemViewModel)item;
            if (tabItem.Content is ObservableCollection<FileEntryViewModel>) return FileStructureTemplate;
            if (tabItem.Content is GpdFileViewModel) return GpdContentTemplate;
            return DefaultTemplate;
        }
    }
}
