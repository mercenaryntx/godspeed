using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Neurotoxin.Godspeed.Shell.Constants;
using Neurotoxin.Godspeed.Shell.ViewModels;

namespace Neurotoxin.Godspeed.Shell.Views.Dialogs
{
    public partial class InputDialog
    {
        public InputDialog(InputDialogViewModel viewModel)
        {
            Owner = Application.Current.MainWindow;
            InitializeComponent();
            DataContext = viewModel;
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
            var vm = (InputDialogViewModel) DataContext;
            switch (vm.Mode)
            {
                case InputDialogMode.Text:
                    TextBox.Focus();
                    break;
                case InputDialogMode.ComboBox:
                    ComboBox.Focus();
                    break;
                case InputDialogMode.RadioGroup:
                    UIThread.BeginRun(() =>
                    {
                        var option = vm.Options.FirstOrDefault(o => o.Equals(vm.DefaultValue)) ?? vm.Options.First();
                        option.IsSelected = true;
                    });
                    break;
            }
        }
    }
}