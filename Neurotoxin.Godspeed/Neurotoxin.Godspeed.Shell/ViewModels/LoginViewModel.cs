using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Neurotoxin.Godspeed.Shell.Interfaces;

namespace Neurotoxin.Godspeed.Shell.ViewModels
{
    public class LoginViewModel : ViewModelBase, ILoginViewModel
    {
        private const string TITLE = "Title";
        private string _title;
        public string Title
        {
            get { return _title; }
            set { _title = value; NotifyPropertyChanged(TITLE); }
        }

        private const string MESSAGE = "Message";
        private string _message;
        public string Message
        {
            get { return _message; }
            set { _message = value; NotifyPropertyChanged(MESSAGE); }
        }

        private const string USERNAME = "Username";
        private string _username;
        public string Username
        {
            get { return _username; }
            set { _username = value; NotifyPropertyChanged(USERNAME); NotifyPropertyChanged(ISVALID); }
        }

        private const string PASSWORD = "Password";
        private string _password;
        public string Password
        {
            get { return _password; }
            set { _password = value; NotifyPropertyChanged(PASSWORD); NotifyPropertyChanged(ISVALID); }
        }

        private const string ISREMEMBERPASSWORDENABLED = "IsRememberPasswordEnabled";
        private bool _isRememberPasswordEnabled;
        public bool IsRememberPasswordEnabled
        {
            get { return _isRememberPasswordEnabled; }
            set { _isRememberPasswordEnabled = value; NotifyPropertyChanged(ISREMEMBERPASSWORDENABLED); }
        }

        private const string REMEMBERPASSWORD = "RememberPassword";
        private bool _rememberPassword;
        public bool RememberPassword
        {
            get { return _rememberPassword; }
            set { _rememberPassword = value; NotifyPropertyChanged(REMEMBERPASSWORD); }
        }

        private const string ISVALID = "IsValid";
        public bool IsValid
        {
            get { return !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Password); }
        }

        private const string ISUSEDEFAULTENABLED = "IsUseDefaultEnabled";
        private bool _isUseDefaultEnabled;
        public bool IsUseDefaultEnabled
        {
            get { return _isUseDefaultEnabled; }
            set { _isUseDefaultEnabled = value; NotifyPropertyChanged(ISUSEDEFAULTENABLED); }
        }

    }
}