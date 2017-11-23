using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using Neurotoxin.Godspeed.Shell.Interfaces;

namespace Neurotoxin.Godspeed.Shell.Primitives
{
    public abstract class DialogBase : BorderlessWindow
    {
        public static readonly DependencyProperty CloseButtonVisibilityProperty = DependencyProperty.Register("CloseButtonVisibility", typeof(Visibility), typeof(DialogBase), new PropertyMetadata(Visibility.Visible));
        public static readonly DependencyProperty IconVisibilityProperty = DependencyProperty.Register("IconVisibility", typeof(Visibility), typeof(DialogBase), new PropertyMetadata(Visibility.Visible));

        public Visibility CloseButtonVisibility
        {
            get { return (Visibility)GetValue(CloseButtonVisibilityProperty); }
            set { SetValue(CloseButtonVisibilityProperty, value); }
        }

        public Visibility IconVisibility
        {
            get { return (Visibility)GetValue(IconVisibilityProperty); }
            set { SetValue(IconVisibilityProperty, value); }
        }

        public void Close(bool dialogResult)
        {
            DialogResult = dialogResult;
            Close();
        }

        protected void SetViewModel(IDialogViewModelBase viewModel)
        {
            var oldVm = DataContext as IDialogViewModelBase;
            if (oldVm != null) oldVm.Closing -= OnViewModelClosing;
            DataContext = viewModel;
            if (viewModel != null) viewModel.Closing += OnViewModelClosing;
        }

        protected override void Initialize()
        {
            base.Initialize();
            ResizeMode = ResizeMode.NoResize;
            SizeToContent = SizeToContent.Height;
            if (!App.ShellInitialized || !UserSettings.DisableCustomChrome) Style = (Style)Application.Current.Resources["Dialog"];
            PreviewKeyDown += OnPreviewKeyDown;

            if (App.ShellInitialized && Application.Current.MainWindow.IsVisible)
            {
                WindowStartupLocation = WindowStartupLocation.CenterOwner;
                Owner = Application.Current.MainWindow;
            }
            else
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }
            Closing += OnDialogClosing;
        }

        protected virtual void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!IsVisible || !IsActive || e.Key != Key.Escape) return;
            e.Handled = true;
            DialogResult = false;
            Close();
        }

        protected virtual void OkButtonClick(object sender, RoutedEventArgs e)
        {
            Close(true);
        }

        protected virtual void CancelButtonClick(object sender, RoutedEventArgs e)
        {
            Close(false);
        }

        private void OnViewModelClosing(object sender, EventArgs eventArgs)
        {
            Close();
        }

        private void OnDialogClosing(object sender, CancelEventArgs e)
        {
            var vm = DataContext as IDialogViewModelBase;
            if (vm != null) vm.Close();
        }
    }
}