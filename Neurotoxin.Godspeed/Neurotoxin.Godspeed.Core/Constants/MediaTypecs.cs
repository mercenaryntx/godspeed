namespace Neurotoxin.Godspeed.Core.Constants
{
    public enum MediaTypecs : uint
    {
	    HardDisk = 0x00000001,
	    DvdX2 = 0x00000002,
	    DvdCd = 0x00000004,
	    Dvd5 = 0x00000008,
	    Dvd9 = 0x00000010,
	    SystemFlash = 0x00000020,
	    MemoryUnit = 0x00000080,
	    MassStorageDevice = 0x00000100,
	    SmbFileSystem = 0x00000200,
	    DirectFromRam = 0x00000400,
	    InsecurePackage = 0x01000000,
	    SaveGamePackage = 0x02000000,
	    OfflineSignedPackage = 0x04000000,
	    LiveSignedPackage = 0x08000000,
        XboxPlatformPackage = 0x10000000
    }
}