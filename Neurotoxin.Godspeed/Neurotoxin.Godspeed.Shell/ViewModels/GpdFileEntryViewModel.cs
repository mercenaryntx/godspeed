using System;
using Neurotoxin.Godspeed.Core.Constants;
using Neurotoxin.Godspeed.Presentation.Infrastructure;

namespace Neurotoxin.Godspeed.Shell.ViewModels
{
    internal class GpdFileEntryViewModel : ViewModelBase
    {
        public EntryType EntryType { get; private set; }
        public ulong Id { get; private set; }
        public string Name { get; private set; }

        public GpdFileEntryViewModel(EntryType entryType, ulong id, string name)
        {
            EntryType = entryType;
            Id = id;
            Name = name;
        }
    }
}