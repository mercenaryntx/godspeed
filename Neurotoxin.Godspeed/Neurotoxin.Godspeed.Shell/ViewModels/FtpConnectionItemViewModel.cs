using System.Windows.Media;
using Microsoft.Practices.Unity;
using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Neurotoxin.Godspeed.Shell.Constants;
using Neurotoxin.Godspeed.Shell.Database.Models;
using Neurotoxin.Godspeed.Shell.Interfaces;
using Neurotoxin.Godspeed.Presentation.Extensions;
using Resx = Neurotoxin.Godspeed.Shell.Properties.Resources;

namespace Neurotoxin.Godspeed.Shell.ViewModels
{
    public class FtpConnectionItemViewModel : CommonViewModelBase, IStoredConnectionViewModel
    {
        public FtpConnection Model { get; private set; }

        private readonly int[] _portOptions = {21, 7564};
        public int[] PortOptions
        {
            get { return _portOptions; }
        }

        private const string NAME = "Name";
        public string Name
        {
            get { return Model.Name; }
            set { Model.Name = value ?? string.Empty; NotifyPropertyChanged(NAME); }
        }

        private const string CONNECTIONIMAGE = "ConnectionImage";
        public ConnectionImage ConnectionImage
        {
            get { return (ConnectionImage)Model.ConnectionImage; }
            set
            {
                Model.ConnectionImage = (int)value;
                _thumbnail = null;
                NotifyPropertyChanged(CONNECTIONIMAGE);
                NotifyPropertyChanged(THUMBNAIL);
            }
        }

        private const string THUMBNAIL = "Thumbnail";
        private ImageSource _thumbnail;
        public ImageSource Thumbnail
        {
            get
            {
                if (_thumbnail == null)
                {
                    var png = ResourceManager.GetContentByteArray(string.Format("/Resources/Connections/{0}.png", ConnectionImage));
                    _thumbnail = StfsPackageExtensions.GetBitmapFromByteArray(png);
                }
                return _thumbnail;
            }
        }

        private const string ADDRESS = "Address";
        public string Address
        {
            get { return Model.Address; }
            set { Model.Address = value ?? string.Empty; NotifyPropertyChanged(ADDRESS); }
        }

        private const string PORT = "Port";
        public int? Port
        {
            get { return Model.Port; }
            set { Model.Port = value ?? 0; NotifyPropertyChanged(PORT); }
        }

        private const string USERNAME = "Username";
        public string Username
        {
            get { return Model.Username; }
            set { Model.Username = value ?? string.Empty; NotifyPropertyChanged(USERNAME); }
        }

        private const string PASSWORD = "Password";
        public string Password
        {
            get { return Model.Password; }
            set { Model.Password = value ?? string.Empty; NotifyPropertyChanged(PASSWORD); }
        }

        private const string USEPASSIVEMODE = "UsePassiveMode";
        public bool UsePassiveMode
        {
            get { return Model.UsePassiveMode; }
            set { Model.UsePassiveMode = value; NotifyPropertyChanged(USEPASSIVEMODE); }
        }

        private const string ISHTTPACCESSDISABLED = "IsHttpAccessDisabled";
        public bool IsHttpAccessDisabled
        {
            get { return Model.IsHttpAccessDisabled; }
            set { Model.IsHttpAccessDisabled = value; NotifyPropertyChanged(ISHTTPACCESSDISABLED); }
        }

        private const string HTTPUSERNAME = "HttpUsername";
        public string HttpUsername
        {
            get { return Model.HttpUsername; }
            set { Model.HttpUsername = value ?? string.Empty; NotifyPropertyChanged(HTTPUSERNAME); }
        }

        private const string HTTPPASSWORD = "HttpPassword";
        public string HttpPassword
        {
            get { return Model.HttpPassword; }
            set { Model.HttpPassword = value ?? string.Empty; NotifyPropertyChanged(HTTPPASSWORD); }
        }

        private const string LOGININFO = "LoginInfo";
        private string _loginInfo;
        public string LoginInfo
        {
            get { return _loginInfo; }
            set { _loginInfo = value; NotifyPropertyChanged(LOGININFO); }
        }

        #region ChangeLoginCommand

        public DelegateCommand ChangeLoginCommand { get; set; }

        public void ExecuteChangeLoginCommand()
        {
            var loginViewModel = Container.Resolve<ILoginViewModel>();
            loginViewModel.Title = Resx.SetCredentials;
            loginViewModel.Message = Resx.LoginAs;
            loginViewModel.IsUseDefaultEnabled = true;
            loginViewModel.Username = Username;
            loginViewModel.Password = Password;
            var login = WindowManager.ShowLoginDialog(loginViewModel);
            if (login == null) return;
            Username = login.Username;
            Password = login.Password;
            SetLoginInfo();
        }

        #endregion

        public FtpConnectionItemViewModel(FtpConnection model)
        {
            Model = model;
            SetLoginInfo();
            ChangeLoginCommand = new DelegateCommand(ExecuteChangeLoginCommand);
        }

        public FtpConnectionItemViewModel Clone()
        {
            return new FtpConnectionItemViewModel(Model.Clone(ItemState.Persisted));
        }

        private void SetLoginInfo()
        {
            LoginInfo = string.IsNullOrEmpty(Username) && string.IsNullOrEmpty(Password) ? Resx.Default : string.Format("{0} / {1}", Username, Password);
        }
    }
}