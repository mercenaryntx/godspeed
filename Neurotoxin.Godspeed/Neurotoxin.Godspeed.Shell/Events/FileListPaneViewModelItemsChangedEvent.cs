using System.Collections;
using System.Collections.Specialized;
using Microsoft.Practices.Composite.Presentation.Events;
using Neurotoxin.Godspeed.Presentation.Events;

namespace Neurotoxin.Godspeed.Shell.Events
{
    public class FileListPaneViewModelItemsChangedEvent : CompositePresentationEvent<FileListPaneViewModelItemsChangedEventArgs> { }

    public class FileListPaneViewModelItemsChangedEventArgs : NotifyCollectionChangedEventArgs, IPayload
    {
        public object Sender { get; private set; }

        public FileListPaneViewModelItemsChangedEventArgs(NotifyCollectionChangedAction action, object sender) : base(action)
        {
            Sender = sender;
        }

        public FileListPaneViewModelItemsChangedEventArgs(NotifyCollectionChangedAction action, object changedItem, object sender) : base(action, changedItem)
        {
            Sender = sender;
        }

        public FileListPaneViewModelItemsChangedEventArgs(NotifyCollectionChangedAction action, object changedItem, int index, object sender) : base(action, changedItem, index)
        {
            Sender = sender;
        }

        public FileListPaneViewModelItemsChangedEventArgs(NotifyCollectionChangedAction action, IList changedItems, object sender) : base(action, changedItems)
        {
            Sender = sender;
        }

        public FileListPaneViewModelItemsChangedEventArgs(NotifyCollectionChangedAction action, IList changedItems, int startingIndex, object sender) : base(action, changedItems, startingIndex)
        {
            Sender = sender;
        }

        public FileListPaneViewModelItemsChangedEventArgs(NotifyCollectionChangedAction action, object newItem, object oldItem, object sender) : base(action, newItem, oldItem)
        {
            Sender = sender;
        }

        public FileListPaneViewModelItemsChangedEventArgs(NotifyCollectionChangedAction action, object newItem, object oldItem, int index, object sender) : base(action, newItem, oldItem, index)
        {
            Sender = sender;
        }

        public FileListPaneViewModelItemsChangedEventArgs(NotifyCollectionChangedAction action, IList newItems, IList oldItems, object sender) : base(action, newItems, oldItems)
        {
            Sender = sender;
        }

        public FileListPaneViewModelItemsChangedEventArgs(NotifyCollectionChangedAction action, IList newItems, IList oldItems, int startingIndex, object sender) : base(action, newItems, oldItems, startingIndex)
        {
            Sender = sender;
        }

        public FileListPaneViewModelItemsChangedEventArgs(NotifyCollectionChangedAction action, object changedItem, int index, int oldIndex, object sender) : base(action, changedItem, index, oldIndex)
        {
            Sender = sender;
        }

        public FileListPaneViewModelItemsChangedEventArgs(NotifyCollectionChangedAction action, IList changedItems, int index, int oldIndex, object sender) : base(action, changedItems, index, oldIndex)
        {
            Sender = sender;
        }
    }
}