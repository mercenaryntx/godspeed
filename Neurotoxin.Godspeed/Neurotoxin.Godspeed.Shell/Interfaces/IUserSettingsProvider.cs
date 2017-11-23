using System;
using System.Globalization;
using Neurotoxin.Godspeed.Shell.Constants;
using Neurotoxin.Godspeed.Shell.Models;

namespace Neurotoxin.Godspeed.Shell.Interfaces
{
    public interface IUserSettingsProvider
    {
        Guid ClientId { get; set; }
        CultureInfo Language { get; set; }
        bool DisableCustomChrome { get; set; }
        bool DisableNotificationSound { get; set; }
        bool DisableFsdStatusPolling { get; set; }
        bool UseVersionChecker { get; set; }
        bool VerifyFileHashAfterFtpUpload { get; set; }
        FsdContentScanTrigger FsdContentScanTrigger { get; set; }
        bool UseRemoteCopy { get; set; }
        bool UseUnity { get; set; }
        int ProfileExpiration { get; set; }
        bool ProfileInvalidation { get; set; }
        int RecognizedGameExpiration { get; set; }
        int PartiallyRecognizedGameExpiration { get; set; }
        int UnrecognizedGameExpiration { get; set; }
        int XboxLiveContentExpiration { get; set; }
        bool XboxLiveContentInvalidation { get; set; }
        int UnknownContentExpiration { get; set; }
        string LeftPaneType { get; set; }
        FileListPaneSettings LeftPaneFileListPaneSettings { get; set; }
        string RightPaneType { get; set; }
        FileListPaneSettings RightPaneFileListPaneSettings { get; set; }
        bool? DisableUserStatisticsParticipation { get; set; }

        bool IsMessageIgnored(string message);
        void IgnoreMessage(string message);
        void PersistData();
    }
}