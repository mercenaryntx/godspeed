using System;
using Neurotoxin.Godspeed.Shell.Database.Models;

namespace Neurotoxin.Godspeed.Shell.ViewModels
{
    public class FavoriteFolderViewModel : CommonViewModelBase, IMenuItemViewModel
    {
        private readonly FavoriteFolder _model;

        public FavoriteFolder Model
        {
            get { return _model; }
        }

        private const string NAME = "Name";
        public string Name
        {
            get { return _model.Name; }
            set { _model.Name = value; NotifyPropertyChanged(NAME); }
        }

        private const string PATH = "Path";
        public string Path
        {
            get { return _model.Path; }
            set { _model.Path = value; NotifyPropertyChanged(PATH); }
        }

        private const string ISCURRENT = "IsCurrent";
        private bool _isCurrent;

        public bool IsCurrent
        {
            get { return _isCurrent; }
            set { _isCurrent = value; NotifyPropertyChanged(ISCURRENT); }
        }

        public FavoriteFolderViewModel(FavoriteFolder model)
        {
            if (model == null) throw new ArgumentNullException("model");
            _model = model;
        }
    }
}