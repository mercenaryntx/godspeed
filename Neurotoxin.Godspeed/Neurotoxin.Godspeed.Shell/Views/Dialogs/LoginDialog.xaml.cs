using System;
using System.Windows;
using System.Windows.Input;
using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Neurotoxin.Godspeed.Shell.Interfaces;

namespace Neurotoxin.Godspeed.Shell.Views.Dialogs
{
    public partial class LoginDialog : IView<ILoginViewModel>
    {
        public ILoginViewModel ViewModel
        {
            get { return DataContext as ILoginViewModel; }
        }
        
        public LoginDialog(ILoginViewModel viewModel)
        {
            Owner = Application.Current.MainWindow;
            DataContext = viewModel;
            InitializeComponent();
            Loaded += OnLoaded;
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);
            if (e.Key != Key.Enter) return;
            e.Handled = true;
            DialogResult = true;
            Close();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Username.Focus();
            Loaded -= OnLoaded;
        }

        private void DefaultButtonClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
            ViewModel.Username = string.Empty;
            ViewModel.Password = string.Empty;
        }
    }
}