using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;
using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Microsoft.Practices.ObjectBuilder2;

namespace Neurotoxin.Godspeed.Presentation.ViewModels
{
    public class TreeItemViewModel : ViewModelBase
    {
        private const string NAME = "Name";
        private string _name;
        public string Name
        {
            get { return _name; }
            set { _name = value; NotifyPropertyChanged(NAME); }
        }

        private const string TITLE = "Title";
        private string _title;
        public string Title
        {
            get { return _title; }
            set { _title = value; NotifyPropertyChanged(TITLE); }
        }

        private const string THUMBNAIL = "THUMBNAIL";
        private ImageSource _thumbnail;
        public ImageSource Thumbnail
        {
            get { return _thumbnail; }
            set { _thumbnail = value; NotifyPropertyChanged(THUMBNAIL); }
        }

        public TreeItemViewModel Parent { get; private set; }

        private const string CHILDREN = "Children";
        private ObservableCollection<TreeItemViewModel> _children;
        public ObservableCollection<TreeItemViewModel> Children
        {
            get { return _children; }
            set { _children = value; NotifyPropertyChanged(CHILDREN); }
        }

        private const string ISDIRECTORY = "IsDirectory";
        private bool _isDirectory;
        public bool IsDirectory
        {
            get { return _isDirectory; }
            set { _isDirectory = value; NotifyPropertyChanged(ISDIRECTORY); }
        }

        private const string ISCHECKED = "IsChecked";
        private bool? _isChecked;
        public bool? IsChecked
        {
            get { return _isChecked; }
            set
            {
                var v = value ?? false;
                _isChecked = v; 
                if (Children != null) Children.ForEach(c => c.IsChecked = v);
                if (Parent != null) Parent.UpdateIsChecked();
                NotifyPropertyChanged(ISCHECKED);
            }
        }

        public object Content { get; set; }

        public TreeItemViewModel()
        {
            Children = new ObservableCollection<TreeItemViewModel>();
        }

        public TreeItemViewModel(TreeItemViewModel parent) : this()
        {
            Parent = parent;
        }

        private void UpdateIsChecked()
        {
            if (Children == null || !Children.Any()) return;
            var check = 0;
            var uncheck = 0;
            foreach (var child in Children)
            {
                if (child.IsChecked == true)
                    check++;
                else
                    uncheck++;
                if (check > 0 && uncheck > 0) break;
            }
            if (check == 0) _isChecked = false;
            else if (uncheck == 0) _isChecked = true;
            else _isChecked = null;
            NotifyPropertyChanged(ISCHECKED);
        }

    }
}