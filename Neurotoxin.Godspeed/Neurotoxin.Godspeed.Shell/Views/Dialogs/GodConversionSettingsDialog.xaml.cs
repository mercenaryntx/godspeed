using System;
using System.Windows;
using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Neurotoxin.Godspeed.Shell.Interfaces;
using Neurotoxin.Godspeed.Shell.ViewModels;

namespace Neurotoxin.Godspeed.Shell.Views.Dialogs
{
    public partial class GodConversionSettingsDialog : IView<GodConversionSettingsViewModel>
    {
        public GodConversionSettingsViewModel ViewModel
        {
            get { return DataContext as GodConversionSettingsViewModel; }
            private set { DataContext = value; }
        }

        public GodConversionSettingsDialog(GodConversionSettingsViewModel viewModel)
        {
            Owner = Application.Current.MainWindow;
            InitializeComponent();
            ViewModel = viewModel;
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Title.Focus();
        }
    }
}