using System;
using Microsoft.Practices.Composite.Presentation.Events;

namespace Neurotoxin.Godspeed.Presentation.Events
{
    // Occurs when an exception has been thrown
    public class ExceptionEvent : CompositePresentationEvent<Exception> { }
}