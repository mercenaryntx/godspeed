using System;
using System.ComponentModel;

namespace Neurotoxin.Godspeed.Shell.Database.Attributes
{
    public class OrderByAttribute : Attribute
    {
        public ListSortDirection Direction { get; private set; }

        public OrderByAttribute(ListSortDirection direction)
        {
            Direction = direction;
        }
    }
}
