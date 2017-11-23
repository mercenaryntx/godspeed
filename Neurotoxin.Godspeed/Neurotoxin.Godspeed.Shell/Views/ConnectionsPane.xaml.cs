using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Neurotoxin.Godspeed.Shell.Views
{
    public partial class ConnectionsPane : UserControl
    {
        public ConnectionsPane()
        {
            InitializeComponent();
            ConnectionList.SelectionChanged += ConnectionListOnSelectionChanged;
            ConnectionList.ItemContainerGenerator.StatusChanged += ItemContainerGeneratorStatusChanged;
        }

        private void ConnectionListOnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = e.AddedItems.Count > 0 ? e.AddedItems[0] : null;
            if (item == null) return;
            SetFocusToSelectedItem();
        }

        private void SetFocusToSelectedItem()
        {
            if (ConnectionList.ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated || ConnectionList.SelectedItem == null) return;
            var item = ConnectionList.ItemContainerGenerator.ContainerFromItem(ConnectionList.SelectedItem) as ListBoxItem;
            if (item != null) item.Focus();
        }

        private void ItemContainerGeneratorStatusChanged(object sender, EventArgs e)
        {
            var generator = (ItemContainerGenerator)sender;
            if (generator.Status != GeneratorStatus.ContainersGenerated) return;
            SetFocusToSelectedItem();
        }
    }
}