namespace Neurotoxin.Godspeed.Core.Constants
{
    public enum OnlineContentResumeState
    {
        FileHeadersNotReady = 0x46494C48,
        NewFolder = 0x666F6C64,
        NewFolderResumeAttempt1 = 0x666F6C31,
        NewFolderResumeAttempt2 = 0x666F6C32,
        NewFolderResumeAttemptUnknown = 0x666F6C3F,
        NewFolderResumeAttemptSpecific = 0x666F6C40
    }
}
