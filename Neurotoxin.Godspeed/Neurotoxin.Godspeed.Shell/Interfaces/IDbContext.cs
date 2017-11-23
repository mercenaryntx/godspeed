using System.Data;

namespace Neurotoxin.Godspeed.Shell.Interfaces
{
    public interface IDbContext
    {
        IDbConnection Open();
        IDbConnection Open(bool transaction);
    }
}