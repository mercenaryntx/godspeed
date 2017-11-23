using System.Windows;
using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Neurotoxin.Godspeed.Shell.Interfaces;

namespace Neurotoxin.Godspeed.Shell.Views.Dialogs
{
    public partial class ProgressDialog : IView<IProgressViewModel>
    {
        public IProgressViewModel ViewModel
        {
            get { return DataContext as IProgressViewModel; }
            set { DataContext = value; }
        }

        public ProgressDialog(IProgressViewModel viewModel)
        {
            CloseButtonVisibility = Visibility.Collapsed;
            InitializeComponent();
            ViewModel = viewModel;
        }

    }
}