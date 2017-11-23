using System;
using System.Windows;
using System.Windows.Controls;
using Neurotoxin.Godspeed.Shell.Constants;
using Resx = Neurotoxin.Godspeed.Shell.Properties.Resources;

namespace Neurotoxin.Godspeed.Shell.Views.Dialogs
{
    public partial class DeleteConfirmationDialog
    {
        public DeleteConfirmationResult Result { get; private set; }

        public DeleteConfirmationDialog(string path)
        {
            InitializeComponent();
            Message.Text = string.Format(Resx.TheDirectoryIsNotEmpty, path);
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            Delete.Focus();
        }

        protected override void OkButtonClick(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            switch (button.Name)
            {
                case "Delete":
                    Result = DeleteConfirmationResult.Delete;
                    break;
                case "All":
                    Result = DeleteConfirmationResult.DeleteAll;
                    break;
                case "Skip":
                    Result = DeleteConfirmationResult.Skip;
                    break;
            }
            base.OkButtonClick(sender, e);
        }
    }
}