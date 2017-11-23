using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Windows.Media;
using Neurotoxin.Godspeed.Presentation.Extensions;
using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Neurotoxin.Godspeed.Shell.Constants;
using Neurotoxin.Godspeed.Shell.Events;
using Neurotoxin.Godspeed.Shell.Interfaces;
using Neurotoxin.Godspeed.Shell.Models;

namespace Neurotoxin.Godspeed.Shell.ViewModels
{
    public class UserMessageViewModel : CommonViewModelBase, IUserMessageViewModel
    {
        public string Message { get; private set; }
        public ImageSource Icon { get; private set; }
        public object CommandParameter { get; private set; }
        public MessageFlags Flags { get; private set; }

        private const string ISREAD = "IsRead";
        private bool _isRead;
        public bool IsRead
        {
            get { return _isRead; }
            set { _isRead = value; NotifyPropertyChanged(ISREAD); }
        }

        private const string ISCHECKED = "IsChecked";
        private bool _isChecked;
        public bool IsChecked
        {
            get { return _isChecked; }
            set { _isChecked = value; NotifyPropertyChanged(ISCHECKED); }
        }

        public UserMessageViewModel(string message, NotifyUserMessageEventArgs e)
        {
            Message = message;
            var png = ResourceManager.GetContentByteArray(string.Format("/Resources/{0}.png", e.Icon));
            Icon = StfsPackageExtensions.GetBitmapFromByteArray(png);
            CommandParameter = new UserMessageCommandParameter(this, e.Command, e.CommandParameter);
            Flags = e.Flags;
        }
    }
}