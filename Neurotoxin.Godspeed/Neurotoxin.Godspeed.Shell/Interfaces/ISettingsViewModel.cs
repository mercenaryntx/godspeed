using System.Globalization;
using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Neurotoxin.Godspeed.Shell.Constants;

namespace Neurotoxin.Godspeed.Shell.Interfaces
{
    public interface ISettingsViewModel
    {
        bool UseUnity { get; set; }
        int ProfileExpiration { get; set; }
        bool ProfileInvalidation { get; set; }
        int RecognizedGameExpiration { get; set; }
        int PartiallyRecognizedGameExpiration { get; set; }
        int UnrecognizedGameExpiration { get; set; }
        int XboxLiveContentExpiration { get; set; }
        bool XboxLiveContentInvalidation { get; set; }
        int UnknownContentExpiration { get; set; }
        bool UseVersionChecker { get; set; }
        bool VerifyFileHashAfterFtpUpload { get; set; }
        FsdContentScanTrigger FsdContentScanTrigger { get; set; }
        bool UseRemoteCopy { get; set; }
        CultureInfo Language { get; set; }
        bool DisableCustomChrome { get; set; }
        DelegateCommand ClearCacheCommand { get; }
        void SaveChanges();
    }
}