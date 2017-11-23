using System;
using System.Windows;
using System.Windows.Controls;
using Neurotoxin.Godspeed.Shell.Constants;
using Neurotoxin.Godspeed.Shell.Interfaces;
using Neurotoxin.Godspeed.Shell.Models;

namespace Neurotoxin.Godspeed.Shell.Views.Dialogs
{
    public partial class IoErrorDialog : ITransferErrorDialog
    {
        public TransferErrorDialogResult Result { get; private set; }

        public IoErrorDialog(Exception exception)
        {
            Owner = Application.Current.MainWindow;
            InitializeComponent();
            ErrorMessage.Text = exception.Message;
        }

        private void ButtonClick(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            switch (button.Name)
            {
                case "Retry":
                    Result = new TransferErrorDialogResult(ErrorResolutionBehavior.Retry);
                    break;
                case "Skip":
                    Result = new TransferErrorDialogResult(ErrorResolutionBehavior.Skip);
                    break;
                case "SkipAll":
                    Result = new TransferErrorDialogResult(ErrorResolutionBehavior.Skip, CopyActionScope.All);
                    break;
                case "Cancel":
                    Result = new TransferErrorDialogResult(ErrorResolutionBehavior.Cancel);
                    break;
            }
            DialogResult = true;
            Close();
        }
    }
}