using Neurotoxin.Godspeed.Presentation.Infrastructure;

namespace Neurotoxin.Godspeed.Shell.Interfaces
{
    public interface IDialog<T> : IView<T> where T : IDialogViewModelBase
    {
    }
}