using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Neurotoxin.Godspeed.Presentation.Primitives;

namespace Neurotoxin.Godspeed.Presentation.Controls
{
    /// <summary>
    /// Interaction logic for StatusBar.xaml
    /// </summary>
    public partial class StatusBar : StatusBarBase
    {
        public StatusBar()
        {
            InitializeComponent();
        }

        public StatusBar(ModuleViewModelBase viewModel) : base(viewModel)
        {
            InitializeComponent();
        }
    }
}