using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Neurotoxin.Godspeed.Modules.ProfileEditor.ViewModels;
using Neurotoxin.Godspeed.Presentation.Infrastructure;

namespace Neurotoxin.Godspeed.Modules.ProfileEditor.Views
{
    public partial class ProfileEditorView : ModuleViewBase
    {
        private new ProfileEditorViewModel ViewModel
        {
            get { return (ProfileEditorViewModel)base.ViewModel; }
        }

        public static ProfileEditorView Current { get; set; }

        public ProfileEditorView(ProfileEditorViewModel viewModel)
        {
            InitializeComponent();
            this.DataContext = viewModel;
            this.Loaded += View_Loaded;
        }

        void View_Loaded(object sender, RoutedEventArgs e)
        {
        }

        public override bool Close()
        {
            ProfileEditorViewModel viewModel = (ProfileEditorViewModel)ViewModel;
            //if (viewModel.KeepDirty()) return false;
            //viewModel.ResetChanges();
            return base.Close();
        }
    }
}