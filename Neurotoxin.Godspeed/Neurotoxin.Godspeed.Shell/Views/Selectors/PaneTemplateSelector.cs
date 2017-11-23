using System.Windows;
using System.Windows.Controls;
using Neurotoxin.Godspeed.Shell.Interfaces;
using Neurotoxin.Godspeed.Shell.ViewModels;

namespace Neurotoxin.Godspeed.Shell.Views.Selectors
{
    public class PaneTemplateSelector : DataTemplateSelector
    {
        public DataTemplate DefaultTemplate { get; set; }
        public DataTemplate FileListTemplate { get; set; }
        public DataTemplate ConnectionsTemplate { get; set; }
        public DataTemplate ProfileRebuilderTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is ConnectionsViewModel) return ConnectionsTemplate;
            if (item is ProfileRebuilderViewModel) return ProfileRebuilderTemplate;
            if (item is IFileListPaneViewModel) return FileListTemplate;
            return DefaultTemplate;
        }
    }
}
