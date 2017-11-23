using System.Windows;
using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Neurotoxin.Godspeed.Shell.ViewModels;

namespace Neurotoxin.Godspeed.Shell.Views
{
    public partial class SettingsWindow : IView<SettingsViewModel>
    {
        public SettingsViewModel ViewModel
        {
            get { return this.DataContext as SettingsViewModel; }
            private set { this.DataContext = value; }
        }

        public SettingsWindow(SettingsViewModel viewModel)
        {
            Owner = Application.Current.MainWindow;
            InitializeComponent();
            ViewModel = viewModel;
        }

        protected override void OkButtonClick(object sender, RoutedEventArgs e)
        {
            ((SettingsViewModel) DataContext).SaveChanges();
            base.OkButtonClick(sender, e);
        }
    }
}