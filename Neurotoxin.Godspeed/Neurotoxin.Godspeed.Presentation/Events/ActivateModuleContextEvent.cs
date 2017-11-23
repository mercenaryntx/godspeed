using Microsoft.Practices.Composite.Presentation.Events;
using Neurotoxin.Godspeed.Presentation.Infrastructure;

namespace Neurotoxin.Godspeed.Presentation.Events
{
    // When the active Tab has been changed and the selected module's properties needs to be showed this event will be invoked.
    public class ActivateModuleContextEvent : CompositePresentationEvent<ModuleViewModelBase> { }
    // When the current Tab has been closed it notifies the properties windows about the viewmodel's disposing
    public class InactivateModuleContextEvent : CompositePresentationEvent<ModuleViewModelBase> { }
}