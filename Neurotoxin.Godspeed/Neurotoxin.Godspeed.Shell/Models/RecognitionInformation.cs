using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neurotoxin.Godspeed.Shell.Constants;

namespace Neurotoxin.Godspeed.Shell.Models
{
    public class RecognitionInformation
    {
        public string Pattern { get; private set; }
        public string Title { get; private set; }
        public TitleType TitleType { get; private set; }
        public ItemType ItemTypeFlags { get; private set; }

        public RecognitionInformation(string pattern, string title, TitleType titleType = TitleType.Unknown, ItemType itemTypeFlags = ItemType.Directory)
        {
            Pattern = pattern;
            Title = title;
            TitleType = titleType;
            ItemTypeFlags = itemTypeFlags;
        }
    }
}