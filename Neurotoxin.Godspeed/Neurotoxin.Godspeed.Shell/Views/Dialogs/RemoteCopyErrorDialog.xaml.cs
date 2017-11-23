using System;
using System.Text;
using System.Windows;

namespace Neurotoxin.Godspeed.Shell.Views.Dialogs
{
    public partial class RemoteCopyErrorDialog
    {
        public bool TurnOffRemoteCopy { get; private set; }

        public RemoteCopyErrorDialog(Exception ex)
        {
            if (Application.Current.MainWindow.IsVisible) Owner = Application.Current.MainWindow;
            InitializeComponent();
            var sb = new StringBuilder();
            sb.Append("Cannot initiate remote connection. ");
            var e = ex;
            while (e != null)
            {
                sb.Append(e.Message);
                sb.Append(" ");
                e = ex.InnerException;
            }
            sb.AppendLine();
            sb.Append("Please check that the server supports Remote Copy (See documentation)");
            ErrorMessage.Text = sb.ToString();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            Continue.Focus();
        }

        private void TurnOffButtonClick(object sender, RoutedEventArgs e)
        {
            TurnOffRemoteCopy = true;
            OkButtonClick(sender, e);
        }
    }
}