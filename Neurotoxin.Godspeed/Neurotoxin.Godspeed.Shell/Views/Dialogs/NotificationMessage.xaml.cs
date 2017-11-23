using System;
using System.Windows;

namespace Neurotoxin.Godspeed.Shell.Views.Dialogs
{
    [Flags]
    public enum NotificationMessageFlags
    {
        None = 0x0,
        NonClosable = 0x1
    }

    public partial class NotificationMessage
    {
        public NotificationMessage(string title, string message, NotificationMessageFlags flags)
        {
            if (Application.Current.MainWindow.IsVisible) Owner = Application.Current.MainWindow;
            DataContext = this;
            InitializeComponent();
            Message.Text = message;
            Title = title;
            if (flags.HasFlag(NotificationMessageFlags.NonClosable))
            {
                CloseButtonVisibility = Visibility.Collapsed;
            } 
            else
            {
                Loaded += OnLoaded;
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            Ok.Focus();
            Loaded -= OnLoaded;
        }

    }
}