using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using Neurotoxin.Godspeed.Core.Extensions;
using Neurotoxin.Godspeed.Shell.Constants;
using Neurotoxin.Godspeed.Shell.Database.Models;
using Neurotoxin.Godspeed.Shell.Extensions;
using Neurotoxin.Godspeed.Shell.Interfaces;
using Neurotoxin.Godspeed.Shell.Models;
using System.Linq;
using ServiceStack.OrmLite;

namespace Neurotoxin.Godspeed.Shell.ContentProviders
{
    public class UserSettingsProvider : IUserSettingsProvider
    {
        private readonly IDbContext _dbContext;
        private readonly UserSettings _userSettings;
        private readonly List<string> _ignoredMessages = new List<string>();

        public Guid ClientId
        {
            get { return _userSettings.ClientId; }
            set { _userSettings.ClientId = value; }
        }

        public CultureInfo Language
        {
            get { return string.IsNullOrEmpty(_userSettings.Language) ? null : CultureInfo.GetCultureInfo(_userSettings.Language); }
            set { _userSettings.Language = value != null ? value.Name : null; }
        }

        public bool DisableCustomChrome
        {
            get { return _userSettings.DisableCustomChrome; }
            set { _userSettings.DisableCustomChrome = value; }
        }

        public bool DisableNotificationSound
        {
            get { return _userSettings.DisableNotificationSound; }
            set { _userSettings.DisableNotificationSound = value; }
        }

        public bool DisableFsdStatusPolling
        {
            get { return _userSettings.DisableFsdStatusPolling; }
            set { _userSettings.DisableFsdStatusPolling = value; }
        }

        public bool UseVersionChecker
        {
            get { return _userSettings.UseVersionChecker; }
            set { _userSettings.UseVersionChecker = value; }
        }

        public bool VerifyFileHashAfterFtpUpload
        {
            get { return _userSettings.VerifyFileHashAfterFtpUpload; }
            set { _userSettings.VerifyFileHashAfterFtpUpload = value; }
        }

        public FsdContentScanTrigger FsdContentScanTrigger
        {
            get { return (FsdContentScanTrigger)_userSettings.FsdContentScanTrigger; }
            set { _userSettings.FsdContentScanTrigger = (int)value; }
        }

        public bool UseRemoteCopy
        {
            get { return _userSettings.UseRemoteCopy; }
            set { _userSettings.UseRemoteCopy = value; }
        }

        public bool UseUnity
        {
            get { return _userSettings.UseUnity; }
            set { _userSettings.UseUnity = value; }
        }

        public int ProfileExpiration
        {
            get { return _userSettings.ProfileExpiration; }
            set { _userSettings.ProfileExpiration = value; }
        }

        public bool ProfileInvalidation
        {
            get { return _userSettings.ProfileInvalidation; }
            set { _userSettings.ProfileInvalidation = value; }
        }

        public int RecognizedGameExpiration
        {
            get { return _userSettings.RecognizedGameExpiration; }
            set { _userSettings.RecognizedGameExpiration = value; }
        }

        public int PartiallyRecognizedGameExpiration
        {
            get { return _userSettings.PartiallyRecognizedGameExpiration; }
            set { _userSettings.PartiallyRecognizedGameExpiration = value; }
        }

        public int UnrecognizedGameExpiration
        {
            get { return _userSettings.UnrecognizedGameExpiration; }
            set { _userSettings.UnrecognizedGameExpiration = value; }
        }

        public int XboxLiveContentExpiration
        {
            get { return _userSettings.XboxLiveContentExpiration; }
            set { _userSettings.XboxLiveContentExpiration = value; }
        }

        public bool XboxLiveContentInvalidation
        {
            get { return _userSettings.XboxLiveContentInvalidation; }
            set { _userSettings.XboxLiveContentInvalidation = value; }
        }

        public int UnknownContentExpiration
        {
            get { return _userSettings.UnknownContentExpiration; }
            set { _userSettings.UnknownContentExpiration = value; }
        }

        public string LeftPaneType
        {
            get { return _userSettings.LeftPaneType; }
            set { _userSettings.LeftPaneType = value; }
        }

        public FileListPaneSettings LeftPaneFileListPaneSettings
        {
            get { return new FileListPaneSettings(_userSettings.LeftPaneDirectory, _userSettings.LeftPaneSortByField, (ListSortDirection)_userSettings.LeftPaneSortDirection, (ColumnMode)_userSettings.LeftPaneDisplayColumnMode, (FileListPaneViewMode)_userSettings.LeftPaneViewMode); }
            set
            {
                _userSettings.LeftPaneDirectory = value.Directory;
                _userSettings.LeftPaneSortByField = value.SortByField;
                _userSettings.LeftPaneSortDirection = (int)value.SortDirection;
                _userSettings.LeftPaneDisplayColumnMode = (int)value.DisplayColumnMode;
                _userSettings.LeftPaneViewMode = (int)value.ViewMode;
            }
        }

        public string RightPaneType
        {
            get { return _userSettings.RightPaneType; }
            set { _userSettings.RightPaneType = value; }
        }

        public FileListPaneSettings RightPaneFileListPaneSettings
        {
            get { return new FileListPaneSettings(_userSettings.RightPaneDirectory, _userSettings.RightPaneSortByField, (ListSortDirection)_userSettings.RightPaneSortDirection, (ColumnMode)_userSettings.RightPaneDisplayColumnMode, (FileListPaneViewMode)_userSettings.RightPaneViewMode); }
            set 
            { 
                _userSettings.RightPaneDirectory = value.Directory;
                _userSettings.RightPaneSortByField = value.SortByField;
                _userSettings.RightPaneSortDirection = (int)value.SortDirection;
                _userSettings.RightPaneDisplayColumnMode = (int)value.DisplayColumnMode;
                _userSettings.RightPaneViewMode = (int)value.ViewMode;
            }
        }
        public bool? DisableUserStatisticsParticipation
        {
            get { return _userSettings.DisableUserStatisticsParticipation; }
            set { _userSettings.DisableUserStatisticsParticipation = value; }
        }

        public UserSettingsProvider(IDbContext dbContext)
        {
            _dbContext = dbContext;
            using(var db = _dbContext.Open())
            {
                _userSettings = db.Get<UserSettings>().First();
                _ignoredMessages = db.Select<IgnoredMessage>().Select(m => m.MessageHash).ToList();
            }
        }

        public void PersistData()
        {
            using (var db = _dbContext.Open())
            {
                db.Persist(_userSettings);
            }
        }

        public bool IsMessageIgnored(string message)
        {
            return _ignoredMessages.Contains(message.Hash());
        }

        public void IgnoreMessage(string message)
        {
            var m = new IgnoredMessage(message);
            _ignoredMessages.Add(m.MessageHash);
            using (var db = _dbContext.Open())
            {
                db.Save(m);    
            }
        }
    }
}