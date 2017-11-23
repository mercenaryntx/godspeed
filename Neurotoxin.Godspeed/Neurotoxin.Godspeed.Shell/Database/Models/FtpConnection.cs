using System.ComponentModel;
using Neurotoxin.Godspeed.Shell.Constants;
using Neurotoxin.Godspeed.Shell.Database.Attributes;
using Neurotoxin.Godspeed.Shell.Interfaces;
using ServiceStack.DataAnnotations;

namespace Neurotoxin.Godspeed.Shell.Database.Models
{
    public class FtpConnection : ModelBase, IStoredConnection
    {
        [Index]
        [PrimaryKey]
        [AutoIncrement]
        public int Id { get; set; }

        private string _name;
        [Index]
        [OrderBy(ListSortDirection.Ascending)]
        [StringLength(255)]
        public string Name
        {
            get { return _name; }
            set { _name = value; SetDirtyFlag("Name"); }
        }

        private int _connectionImage;
        public int ConnectionImage
        {
            get { return _connectionImage; }
            set { _connectionImage = value; SetDirtyFlag("ConnectionImage"); }
        }

        private string _address;
        [StringLength(45)]
        public string Address
        {
            get { return _address; }
            set { _address = value; SetDirtyFlag("Address"); }
        }

        private int _port;
        public int Port
        {
            get { return _port; }
            set { _port = value; SetDirtyFlag("Port"); }
        }

        private string _username;
        [StringLength(50)]
        public string Username
        {
            get { return _username; }
            set { _username = value; SetDirtyFlag("Username"); }
        }

        private string _password;
        [StringLength(50)]
        public string Password
        {
            get { return _password; }
            set { _password = value; SetDirtyFlag("Password"); }
        }

        private string _defaultPath;
        public string DefaultPath
        {
            get { return _defaultPath; }
            set { _defaultPath = value; SetDirtyFlag("DefaultPath"); }
        }

        private bool _usePassiveMode;
        public bool UsePassiveMode
        {
            get { return _usePassiveMode; }
            set { _usePassiveMode = value; SetDirtyFlag("UsePassiveMode"); }
        }

        private bool _isHttpAccessDisabled;
        public bool IsHttpAccessDisabled
        {
            get { return _isHttpAccessDisabled; }
            set { _isHttpAccessDisabled = value; SetDirtyFlag("IsHttpAccessDisabled"); }
        }

        private string _httpUsername;
        [StringLength(50)]
        public string HttpUsername
        {
            get { return _httpUsername; }
            set { _httpUsername = value; SetDirtyFlag("HttpUsername"); }
        }

        private string _httpPassword;
        [StringLength(50)]
        public string HttpPassword
        {
            get { return _httpPassword; }
            set { _httpPassword = value; SetDirtyFlag("HttpPassword"); }
        }

        public FtpConnection Clone(ItemState? itemState = null)
        {
            return new FtpConnection
                       {
                           Id = Id,
                           Name = Name,
                           ConnectionImage = ConnectionImage,
                           Address = Address,
                           Port = Port,
                           Username = Username,
                           Password = Password,
                           DefaultPath = DefaultPath,
                           UsePassiveMode = UsePassiveMode,
                           IsHttpAccessDisabled = IsHttpAccessDisabled,
                           HttpUsername = HttpUsername,
                           HttpPassword = HttpPassword,
                           ItemState = itemState ?? ItemState
                       };
        }
    }
}