using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Neurotoxin.Godspeed.Shell.Interfaces;

namespace Neurotoxin.Godspeed.Shell.Views.Controls
{
    public partial class PaneHeader : UserControl
    {
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(PaneHeader));
        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register("Command", typeof(IDelegateCommand), typeof(PaneHeader));

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value);}
        }

        public IDelegateCommand Command
        {
            get { return (IDelegateCommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        public PaneHeader()
        {
            InitializeComponent();
        }

        private void ShowEditBox()
        {
            var vm = DataContext as IFileListPaneViewModel;
            if (vm != null) vm.IsInPathEditMode = true;
            EditBox.Text = Text;
            EditBox.Visibility = Visibility.Visible;
            EditBox.SelectAll();
            EditBox.Focus();
        }

        private void HideEditBox()
        {
            var vm = DataContext as IFileListPaneViewModel;
            if (vm != null) vm.IsInPathEditMode = false;
            EditBox.Visibility = Visibility.Collapsed;
        }

        private void OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ShowEditBox();
            e.Handled = true;
        }

        private void EditBox_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Escape && e.Key != Key.Enter) return;
            if (e.Key == Key.Enter && Command != null)
            {
                Command.Execute(EditBox.Text);
            }
            HideEditBox();
            e.Handled = true;
        }

        private void EditBox_OnLostFocus(object sender, RoutedEventArgs e)
        {
            HideEditBox();
            e.Handled = true;
        }
    }
}
