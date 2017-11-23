using System.ComponentModel;
using System.Windows;
using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Neurotoxin.Godspeed.Shell.ViewModels;

namespace Neurotoxin.Godspeed.Shell.Views
{
    public partial class FtpTraceWindow : IView<FtpTraceViewModel>
    {
        public FtpTraceViewModel ViewModel
        {
            get { return DataContext as FtpTraceViewModel; }
            private set { DataContext = value; }
        }

        public FtpTraceWindow(FtpTraceViewModel viewModel)
        {
            InitializeComponent();
            ViewModel = viewModel;
        }
    }
}