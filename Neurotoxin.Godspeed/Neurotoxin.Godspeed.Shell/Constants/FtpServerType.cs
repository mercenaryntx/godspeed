using Neurotoxin.Godspeed.Core.Attributes;

namespace Neurotoxin.Godspeed.Shell.Constants
{
    public enum FtpServerType
    {
        Unknown,
        [StringValue("Minftpd ready")]
        MinFTPD,
        [StringValue("FSD FTPD ready")]
        FSD,
        [StringValue("F3 FTPD ready")]
        F3,
        [StringValue("XeXMenu FTPD 0.1, by XeDev")]
        XeXMenu,
        [StringValue("DLiFTPD 0.1")]
        DashLaunch,
        [StringValue("PS3 FTP Server")]
        PlayStation3,
        [StringValue("Microsoft FTP Service")]
        IIS,
        [StringValue("FtpDll Ready")]
        Aurora,
        [StringValue("FtpDll Ready")]
        AuroraAlpha,
        [StringValue("xFTPDll 0.1 powered by SlimFTPd")]
        FtpDll
    }
}