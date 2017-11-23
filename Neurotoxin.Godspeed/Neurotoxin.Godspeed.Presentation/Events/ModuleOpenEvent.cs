using Microsoft.Practices.Composite.Presentation.Events;
using Neurotoxin.Godspeed.Presentation.Infrastructure;

namespace Neurotoxin.Godspeed.Presentation.Events
{
    // When a Tab needs to be opened
    public class ModuleOpenEvent : CompositePresentationEvent<ModuleOpenEventArgs> { }

    /// <summary>
    /// Argument type of ModuleOpenEvent
    /// </summary>
    public class ModuleOpenEventArgs
    {
        public ModuleLoadInfo LoadInfo { get; set; }

        public ModuleOpenEventArgs(ModuleLoadInfo loadInfo)
        {
            LoadInfo = loadInfo;
        }
    }
}