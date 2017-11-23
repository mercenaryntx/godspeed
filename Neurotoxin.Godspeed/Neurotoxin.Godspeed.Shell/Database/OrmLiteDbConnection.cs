using System.Data;

namespace Neurotoxin.Godspeed.Shell.Database
{
    public class OrmLiteDbConnection : IDbConnection
    {
        private IDbConnection _connection;
        private IDbTransaction _transaction;

        public OrmLiteDbConnection(IDbConnection connection, bool transaction)
        {
            _connection = connection;
            if (transaction) _transaction = BeginTransaction();
        }

        #region IDbConnection members

        public string ConnectionString { get; set; }
        public int ConnectionTimeout { get; private set; }
        public string Database { get; private set; }
        public ConnectionState State { get; private set; }

        public IDbTransaction BeginTransaction()
        {
            return _connection.BeginTransaction();
        }

        public IDbTransaction BeginTransaction(IsolationLevel il)
        {
            return _connection.BeginTransaction(il);
        }

        public void Close()
        {
        }

        public void ChangeDatabase(string databaseName)
        {
            _connection.ChangeDatabase(databaseName);
        }

        public IDbCommand CreateCommand()
        {
            return _connection.CreateCommand();
        }

        public void Open()
        {
        }

        #endregion

        public void Dispose()
        {
            if (_transaction != null)
            {
                _transaction.Commit();
                _transaction.Dispose();
                _transaction = null;
            }
            _connection.Dispose();
            _connection = null;
        }
    }
}