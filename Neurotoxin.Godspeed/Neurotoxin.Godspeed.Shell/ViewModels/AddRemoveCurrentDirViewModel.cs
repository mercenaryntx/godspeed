namespace Neurotoxin.Godspeed.Shell.ViewModels
{
    public class AddRemoveCurrentDirViewModel : CommonViewModelBase, IMenuItemViewModel
    {
        private const string NAME = "Name";
        private string _name;
        public string Name
        {
            get { return _name; }
            set { _name = value; NotifyPropertyChanged(NAME); }
        }
    }
}