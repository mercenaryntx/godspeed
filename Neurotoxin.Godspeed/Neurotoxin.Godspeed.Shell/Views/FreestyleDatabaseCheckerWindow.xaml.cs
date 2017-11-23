using System;
using System.Windows;
using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Neurotoxin.Godspeed.Shell.ViewModels;

namespace Neurotoxin.Godspeed.Shell.Views
{
    public partial class FreestyleDatabaseCheckerWindow : IView<FreestyleDatabaseCheckerViewModel>
    {
        public FreestyleDatabaseCheckerViewModel ViewModel
        {
            get { return DataContext as FreestyleDatabaseCheckerViewModel; }
            private set { DataContext = value; }
        }

        public FreestyleDatabaseCheckerWindow(FreestyleDatabaseCheckerViewModel viewModel)
        {
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            Owner = Application.Current.MainWindow;
            InitializeComponent();
            ViewModel = viewModel;
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (!ViewModel.HasMissingFolders) TabControl.SelectedIndex = 1;
        }
    }
}