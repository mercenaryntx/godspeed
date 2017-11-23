namespace Neurotoxin.Godspeed.Core.Constants
{
    public enum LicenseType
    {
        Unused = 0x0000,
        Unrestricted = 0xFFFF,
        ConsoleProfileLicense = 0x0009,
        WindowsProfileLicense = 0x0003,
        ConsoleLicense = 0xF000,
        MediaFlags = 0xE000,
        KeyVaultPrivileges = 0xD000,
        HyperVisorFlags = 0xC000,
        UserPrivileges = 0xB000
    }
}