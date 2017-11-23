using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neurotoxin.Godspeed.Shell.Constants;

namespace Neurotoxin.Godspeed.Shell.Tests.Dummies
{
    public class FakingRules
    {
        public Range TreeDepth { get; set; }
        public Range ItemCount { get; set; }
        public Dictionary<int, Range> ItemCountOnLevel { get; set; }
        public ItemType[] ItemTypes { get; set; }
        public Dictionary<int, ItemType[]> ItemTypesOnLevel { get; set; }

        public Range GetItemCount(int? level = null)
        {
            if (level == null) return ItemCount;
            if (ItemCountOnLevel != null && ItemCountOnLevel.ContainsKey(level.Value)) return ItemCountOnLevel[level.Value];
            return ItemCount;
        }

        public ItemType[] GetItemTypes(int? level = null)
        {
            if (level == null) return ItemTypes;
            if (ItemTypesOnLevel != null && ItemTypesOnLevel.ContainsKey(level.Value)) return ItemTypesOnLevel[level.Value];
            return ItemTypes;
        }

    }
}
