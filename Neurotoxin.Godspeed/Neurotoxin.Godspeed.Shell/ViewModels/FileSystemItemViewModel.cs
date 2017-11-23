using System;
using Neurotoxin.Godspeed.Core.Constants;
using Neurotoxin.Godspeed.Shell.Constants;
using Neurotoxin.Godspeed.Shell.Models;

namespace Neurotoxin.Godspeed.Shell.ViewModels
{
    public class FileSystemItemViewModel : CommonViewModelBase
    {
        private readonly FileSystemItem _model;
        public FileSystemItem Model
        {
            get { return _model; }
        }

        public string Path
        {
            get { return _model.Path; }
        }

        public string FullPath
        {
            get { return _model.FullPath; }
        }

        internal const string TITLE = "Title";
        public string Title
        {
            get { return _model.Title; }
            set { _model.Title = value; NotifyPropertyChanged(TITLE); }
        }

        internal const string NAME = "Name";
        public string Name
        {
            get { return _model.Name; }
            set { _model.Name = value; NotifyPropertyChanged(NAME); }
        }

        internal const string THUMBNAIL = "Thumbnail";
        public byte[] Thumbnail
        {
            get { return _model.Thumbnail; }
            set { _model.Thumbnail = value; NotifyPropertyChanged(THUMBNAIL); }
        }

        public string ComputedName
        {
            get { return Title ?? Name; }
        }

        public bool HasThumbnail
        {
            get { return _model.Thumbnail != null && !IsUpDirectory; }
        }

        public ItemType Type
        {
            get { return _model.Type; }
        }

        private const string TITLETYPE = "TitleType";
        public TitleType TitleType
        {
            get { return _model.TitleType; }
            set { _model.TitleType = value; NotifyPropertyChanged(TITLETYPE); }
        }

        private const string CONTENTTYPE = "ContentType";
        public ContentType ContentType
        {
            get { return _model.ContentType; }
            set { _model.ContentType = value; NotifyPropertyChanged(CONTENTTYPE); }
        }

        private const string SIZE = "Size";
        public long? Size
        {
            get { return _model.Size; }
            set { _model.Size = value; NotifyPropertyChanged(SIZE); }
        }

        public long ComputedSize
        {
            get { return Size ?? 0; }
        }

        public DateTime Date
        {
            get { return _model.Date; }
        }

        private const string ISSELECTED = "IsSelected";
        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set { _isSelected = value; NotifyPropertyChanged(ISSELECTED); }
        }

        public bool IsUpDirectory { get; set; }

        public bool IsCached
        {
            get { return _model.IsCached; }
        }

        public bool IsLocked
        {
            get { return _model.IsLocked; }
        }

        public string LockMessage
        {
            get { return _model.LockMessage; }
        }

        private const string ISREFRESHING = "IsRefreshing";
        private bool _isRefreshing;
        public bool IsRefreshing
        {
            get { return _isRefreshing; }
            set { _isRefreshing = value; NotifyPropertyChanged(ISREFRESHING); }
        }

        private const string ISGAME = "IsGame";
        public bool IsGame
        {
            get { return TitleType == TitleType.Game; }
        }

        private const string ISPROFILE = "IsProfile";
        public bool IsProfile
        {
            get { return TitleType == TitleType.Profile; }
        }

        private const string ISXEX = "IsXex";
        public bool IsXex
        {
            get
            {
                var ext = System.IO.Path.GetExtension(Name);
                return ext != null && ext.ToLower() == ".xex";
            }
        }

        public bool IsCompressedFile
        {
            get
            {
                var ext = System.IO.Path.GetExtension(Path).ToLower();
                return (ext == ".zip" || ext == ".rar" || ext == ".tar" || ext == ".tar.gz" || ext == ".7z");
            }
        }

        public bool IsIso
        {
            get
            {
                var ext = System.IO.Path.GetExtension(Path).ToLower();
                return ext == ".iso";
            }
        }

        private static string CONTENT = "Content";
        private object _content;
        public object Content
        {
            get { return _content; }
            set { _content = value; NotifyPropertyChanged(CONTENT); }
        }

        public FileSystemItemViewModel(FileSystemItem model)
        {
            if (model == null) throw new ArgumentNullException("model");
            _model = model;
        }

        public void NotifyModelChanges()
        {
            NotifyPropertyChanged(TITLE);
            NotifyPropertyChanged(TITLETYPE);
            NotifyPropertyChanged(CONTENTTYPE);
            NotifyPropertyChanged(ISGAME);
            NotifyPropertyChanged(ISPROFILE);
            NotifyPropertyChanged(ISXEX);
            NotifyPropertyChanged(SIZE);
            NotifyPropertyChanged(THUMBNAIL);
        }

        public FileSystemItemViewModel Clone()
        {
            return new FileSystemItemViewModel(Model.Clone());
        }
    }
}