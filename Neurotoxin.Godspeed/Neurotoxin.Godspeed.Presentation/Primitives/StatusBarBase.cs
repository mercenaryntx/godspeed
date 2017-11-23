using System.Windows.Controls;
using System.Windows.Input;
using Neurotoxin.Godspeed.Presentation.Infrastructure;

namespace Neurotoxin.Godspeed.Presentation.Primitives
{
    public class StatusBarBase : UserControl
    {
        protected ModuleViewModelBase ViewModel
        {
            get { return this.DataContext as ModuleViewModelBase; }
        }

        public StatusBarBase() { }

        public StatusBarBase(ModuleViewModelBase viewModel)
        {
            this.DataContext = viewModel;
        }

        /// <summary>
        /// Shows of the caught Exception message
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnErrorStatusDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //if (ViewModel.LastError == null) return;
            //ExceptionHandler.ShowExceptionWindow(ViewModel);
        }
    }
}