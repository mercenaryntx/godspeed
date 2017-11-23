using System;
using System.Data;
using ServiceStack.OrmLite.Sqlite;

namespace Neurotoxin.Godspeed.Shell.Database
{
    public class SqliteOrmLiteDialectProviderEx : SqliteOrmLiteDialectProvider
    {
        public override void OnAfterInitColumnTypeMap()
        {
            base.OnAfterInitColumnTypeMap();
            DbTypeMap.Set<DateTime>(DbType.DateTime, "DATETIME");
            DbTypeMap.Set<DateTime?>(DbType.DateTime, "DATETIME");
            DbTypeMap.Set<bool>(DbType.Boolean, "BOOLEAN");
        }
    }
}