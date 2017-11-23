using System.Windows;
using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Neurotoxin.Godspeed.Shell.ViewModels;

namespace Neurotoxin.Godspeed.Shell.Views.Dialogs
{
    public partial class TransferProgressDialog : IView<ITransferManagerViewModel>
    {
        public const string BytesFormat = "{0:#,0} / {1:#,0} Bytes";
        public const string TitleFormat = "{0} ({1}%)";
        public const string FileCountFormat = "{0} / {1}";
        public const string SpeedFormat = "{0} KBps";
        public const string TimeFormat = "{0:hh\\:mm\\:ss} / {1:hh\\:mm\\:ss}";
        public const string ActionFormat = "{0}: ";

        public ITransferManagerViewModel ViewModel
        {
            get { return DataContext as ITransferManagerViewModel; }
            private set { DataContext = value; }
        }

        public TransferProgressDialog(ITransferManagerViewModel viewModel)
        {
            Owner = Application.Current.MainWindow;
            InitializeComponent();
            ViewModel = viewModel;
        }

        protected override void CancelButtonClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}