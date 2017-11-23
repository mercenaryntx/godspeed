using System.Data;
using System.IO;
using Neurotoxin.Godspeed.Shell.Interfaces;
using ServiceStack.Logging;
using ServiceStack.OrmLite;
using ServiceStack.OrmLite.Sqlite;

namespace Neurotoxin.Godspeed.Shell.Database
{
    public class OrmLiteDbContext : IDbContext
    {
        private string _path;
        private OrmLiteConnectionFactory _dbFactory;

        public OrmLiteDbContext()
        {
            SqliteOrmLiteDialectProvider.Instance = new SqliteOrmLiteDialectProviderEx();
            //LogManager.LogFactory = new OrmLiteLogFactory();
        }

        public IDbConnection Open()
        {
            return Open(false);
        }

        public IDbConnection Open(bool transaction)
        {
            if (string.IsNullOrEmpty(_path))
            {
                _path = Path.Combine(App.DataDirectory, "godspeed.db");
                _dbFactory = new OrmLiteConnectionFactory(_path, SqliteDialect.Provider);
            }
            return new OrmLiteDbConnection(_dbFactory.Open(), transaction);
        }
    }
}