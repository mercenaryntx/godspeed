using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Microsoft.Practices.Composite.Presentation.Events;

namespace Neurotoxin.Godspeed.Presentation.Events
{
    /// <summary>
    /// Sets the Input-, and CommandBindings of the MenuItems to the Shell
    /// </summary>
    public class InitializeGesturesAndCommandsEvent : CompositePresentationEvent<MainMenuBindings> { }

    public class MainMenuBindings
    {
        public List<CommandBinding> CommandBindings { get; set; }
        public List<InputBinding> InputBindings { get; set; }

        public MainMenuBindings()
        {
            CommandBindings = new List<CommandBinding>();
            InputBindings = new List<InputBinding>();
        }
    }
}