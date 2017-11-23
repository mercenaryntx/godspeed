using System.Windows;
using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Neurotoxin.Godspeed.Shell.Interfaces;

namespace Neurotoxin.Godspeed.Shell.Views.Dialogs
{
    public partial class TreeSelectionDialog : IView<ITreeSelectionViewModel>
    {
        public ITreeSelectionViewModel ViewModel
        {
            get { return DataContext as ITreeSelectionViewModel; }
            private set { DataContext = value; }
        }

        public TreeSelectionDialog(ITreeSelectionViewModel viewModel)
        {
            SizeToContent = SizeToContent.Manual;
            ViewModel = viewModel;
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Ok.Focus();
        }
    }
}