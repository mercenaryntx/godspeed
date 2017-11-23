using System;
using Neurotoxin.Godspeed.Shell.Database.Attributes;
using ServiceStack.DataAnnotations;

namespace Neurotoxin.Godspeed.Shell.Database.Models
{
    public class CacheItem : ModelBase
    {
        [Index]
        [PrimaryKey]
        [StringLength(40)]
        public string Id { get; set; }

        private DateTime? _date;
        public DateTime? Date
        {
            get { return _date; }
            set { _date = value; SetDirtyFlag("Date"); }
        }

        private DateTime? _expiration;
        public DateTime? Expiration 
        { 
            get { return _expiration; }
            set { _expiration = value; SetDirtyFlag("Expiration"); }
        }

        private long? _size;
        public long? Size
        {
            get { return _size; }
            set { _size = value; SetDirtyFlag("Size"); }
        }

        #region FileSystemItem properties

        private string _title;
        [StringLength(255)]
        public string Title
        {
            get { return _title; }
            set { _title = value; SetDirtyFlag("Title"); }
        }

        private byte[] _thumbnail;
        public byte[] Thumbnail
        {
            get { return _thumbnail; }
            set { _thumbnail = value; SetDirtyFlag("Thumbnail"); }
        }

        private int _type;
        public int Type
        {
            get { return _type; }
            set { _type = value; SetDirtyFlag("Type"); }
        }

        private int _titleType;
        public int TitleType
        {
            get { return _titleType; }
            set { _titleType = value; SetDirtyFlag("TitleType"); }
        }

        private int _contentType;
        public int ContentType
        {
            get { return _contentType; }
            set { _contentType = value; SetDirtyFlag("ContentType"); }
        }

        private int _recognitionState;
        public int RecognitionState
        {
            get { return _recognitionState; }
            set { _recognitionState = value; SetDirtyFlag("RecognitionState"); }
        }

        #endregion

        [IgnoreOnRead]
        public byte[] Content { get; set; }

        public override void Persisted()
        {
            base.Persisted();
            Content = null;
        }
    }
}