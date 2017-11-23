using System.Collections.Generic;
using Microsoft.Practices.Composite.Presentation.Events;
using Neurotoxin.Godspeed.Core.Caching;
using Neurotoxin.Godspeed.Shell.Models;

namespace Neurotoxin.Godspeed.Shell.Events
{
    public class ShellInitializedEvent : CompositePresentationEvent<ShellInitializedEventArgs> { }

    public class ShellInitializedEventArgs
    {
    }
}