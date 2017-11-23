using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Neurotoxin.Godspeed.Shell.Constants;
using Neurotoxin.Godspeed.Shell.ViewModels;
using System.Linq;

namespace Neurotoxin.Godspeed.Shell.Views.Dialogs
{
    public partial class NewConnectionDialog : IView<FtpConnectionItemViewModel>
    {
        public FtpConnectionItemViewModel ViewModel
        {
            get { return DataContext as FtpConnectionItemViewModel; }
            private set { DataContext = value; }
        }

        public ConnectionImage[] ConnectionImages { get; private set; }
        public List<string> ConnectionNames { get; private set; }

        public NewConnectionDialog(List<string> connectionNames, FtpConnectionItemViewModel viewModel)
        {
            Owner = Application.Current.MainWindow;
            ConnectionImages = Enum.GetValues(typeof(ConnectionImage)).Cast<ConnectionImage>().ToArray();
            ConnectionNames = connectionNames;
            InitializeComponent();
            ViewModel = viewModel;
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            ConnectionName.Focus();
        }

        protected override void OkButtonClick(object sender, RoutedEventArgs e)
        {
            if (HasError()) return;
            base.OkButtonClick(sender, e);
        }

        private bool HasError()
        {
            var result = false;
            var controls = new[] { ConnectionName, Address };
            foreach (var control in controls.Where(c => c.IsEnabled))
            {
                control.GetBindingExpression(TextBox.TextProperty).UpdateSource();
                if (Validation.GetHasError(control)) result = true;
            }
            return result;
        }
    }
}