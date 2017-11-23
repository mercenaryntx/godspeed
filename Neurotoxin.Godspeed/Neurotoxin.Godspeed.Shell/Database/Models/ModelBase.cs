using System;
using System.Collections.Generic;
using Neurotoxin.Godspeed.Shell.Constants;
using ServiceStack.DataAnnotations;

namespace Neurotoxin.Godspeed.Shell.Database.Models
{
    [Serializable]
    public abstract class ModelBase
    {
        [Ignore]
        public ItemState ItemState { get; set; }
        
        [Ignore]
        public HashSet<string> DirtyFields { get; private set; }

        public ModelBase()
        {
            DirtyFields = new HashSet<string>();
            ItemState = ItemState.New;
        }

        public virtual void Persisted()
        {
            ItemState = ItemState.Persisted;
            DirtyFields.Clear();
        }

        public virtual void MarkDeleted()
        {
            ItemState = ItemState.Deleted;
        }

        protected void SetDirtyFlag(string propertyName)
        {
            if (ItemState == ItemState.New) return;
            ItemState = ItemState.Dirty;
            DirtyFields.Add(propertyName);
        }

    }

}