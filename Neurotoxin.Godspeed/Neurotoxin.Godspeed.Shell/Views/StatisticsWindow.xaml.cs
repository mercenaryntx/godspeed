using System.Windows;
using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Neurotoxin.Godspeed.Shell.ViewModels;

namespace Neurotoxin.Godspeed.Shell.Views
{
    public partial class StatisticsWindow : IView<StatisticsViewModel>
    {
        public StatisticsViewModel ViewModel
        {
            get { return this.DataContext as StatisticsViewModel; }
            private set { this.DataContext = value; }
        }

        public StatisticsWindow(StatisticsViewModel viewModel)
        {
            Owner = Application.Current.MainWindow;
            InitializeComponent();
            ViewModel = viewModel;
        }
    }
}