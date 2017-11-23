using System.Windows.Input;

namespace Neurotoxin.Godspeed.Presentation.Infrastructure
{
    public interface IDelegateCommand : ICommand
    {
        void RaiseCanExecuteChanged();
    }

}