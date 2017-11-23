using Neurotoxin.Godspeed.Shell.Interfaces;
using Neurotoxin.Godspeed.Shell.ViewModels;

namespace Neurotoxin.Godspeed.Shell.Views.Dialogs
{
    public partial class WriteErrorDialog : IDialog<WriteErrorDialogViewModel>
    {
        public WriteErrorDialogViewModel ViewModel
        {
            get { return DataContext as WriteErrorDialogViewModel; }
            private set { SetViewModel(value); }
        }

        public WriteErrorDialog(WriteErrorDialogViewModel viewModel)
        {
            ViewModel = viewModel;
            InitializeComponent();
        }
    }
}