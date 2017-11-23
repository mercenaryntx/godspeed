using System.Windows;
using System.Windows.Controls;
using Neurotoxin.Godspeed.Shell.ViewModels;

namespace Neurotoxin.Godspeed.Shell.Views.Selectors
{
    public class MessageStyleSelector : StyleSelector
    {
        public Style DefaultStyle { get; set; }
        public Style NoMessagesStyle { get; set; }

        public override Style SelectStyle(object item, DependencyObject container)
        {
            if (item is NoMessagesViewModel) return NoMessagesStyle;
            return DefaultStyle;
        }
    }
}