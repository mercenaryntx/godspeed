using System.Windows;
using Neurotoxin.Godspeed.Shell.Interfaces;
using Neurotoxin.Godspeed.Shell.ViewModels;

namespace Neurotoxin.Godspeed.Shell.Views.Dialogs
{
    public partial class ShutdownDialog : IDialog<ShutdownDialogViewModel>
    {
        public ShutdownDialogViewModel ViewModel 
        { 
            get { return DataContext as ShutdownDialogViewModel; }
            private set { SetViewModel(value); }
        }

        public ShutdownDialog(ShutdownDialogViewModel viewModel)
        {
            ViewModel = viewModel;
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            if (ShutdownNowButton.IsVisible) ShutdownNowButton.Focus();
            if (ShutdownNowSplitButton.IsVisible) ShutdownNowSplitButton.Focus();
        }

    }
}