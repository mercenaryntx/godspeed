using System;
using System.ComponentModel;
using Microsoft.Practices.Composite.Events;

namespace Neurotoxin.Godspeed.Presentation.Infrastructure
{
    public interface IViewModel : INotifyPropertyChanged, IDisposable
    {
        IEventAggregator EventAggregator { get; }
        IWorkHandler WorkHandler { get; }
        bool IsBusy { get; set; }
    }
}