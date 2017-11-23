using System.Collections.Generic;

namespace Neurotoxin.Godspeed.Shell.Constants.Comparers
{
    public class ItemTypeComparer : IComparer<ItemType>
    {
        public int Compare(ItemType x, ItemType y)
        {
            if (x == y) return 0;
            if (x == ItemType.File) return 1;
            if (y == ItemType.File) return -1;
            return 0;
        }
    }
}