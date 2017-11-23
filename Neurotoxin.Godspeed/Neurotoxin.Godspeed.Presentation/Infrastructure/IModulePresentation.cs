using System.Windows.Controls;

namespace Neurotoxin.Godspeed.Presentation.Infrastructure
{
    public interface IModulePresentation
    {
        /// <summary>
        /// Current view presented by the module.
        /// </summary>
        IView GetView(string viewName);
    }
}