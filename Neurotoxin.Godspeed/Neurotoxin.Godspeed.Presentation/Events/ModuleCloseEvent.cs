using Microsoft.Practices.Composite.Presentation.Events;
using Neurotoxin.Godspeed.Presentation.Infrastructure;

namespace Neurotoxin.Godspeed.Presentation.Events
{
    // When a Tab needs to be closed
    public class ModuleCloseEvent : CompositePresentationEvent<ModuleCloseEventArgs> { }

    /// <summary>
    /// Argument type of ModuleCloseEvent
    /// </summary>
    public class ModuleCloseEventArgs
    {
        /// <summary>
        /// The ModuleLoadInfo that describes which tab needs to be closed
        /// </summary>
        public ModuleLoadInfo LoadInfo { get; set; }
        /// <summary>
        /// If true the tab will be closed without user confirmation
        /// </summary>
        public bool Force { get; set; }

        public ModuleCloseEventArgs(ModuleLoadInfo loadInfo)
            : this(loadInfo, false)
        {
        }

        public ModuleCloseEventArgs(ModuleLoadInfo loadInfo, bool force)
        {
            LoadInfo = loadInfo;
            Force = force;
        }
    }
}