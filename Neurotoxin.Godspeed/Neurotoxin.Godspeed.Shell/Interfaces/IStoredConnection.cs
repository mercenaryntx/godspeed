using Neurotoxin.Godspeed.Shell.Constants;

namespace Neurotoxin.Godspeed.Shell.Interfaces
{
    public interface IStoredConnection
    {
        string Name { get; set; }
        int ConnectionImage { get; set; }
    }
}