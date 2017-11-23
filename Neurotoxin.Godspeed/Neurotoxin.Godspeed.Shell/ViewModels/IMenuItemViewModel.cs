using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Neurotoxin.Godspeed.Shell.Interfaces;

namespace Neurotoxin.Godspeed.Shell.ViewModels
{
    public interface IMenuItemViewModel : IViewModel
    {
        string Name { get; set; }
    }
}