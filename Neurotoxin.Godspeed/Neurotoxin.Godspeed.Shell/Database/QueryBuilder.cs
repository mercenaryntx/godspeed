using System;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Neurotoxin.Godspeed.Core.Extensions;
using Neurotoxin.Godspeed.Shell.Database.Attributes;
using ServiceStack.DataAnnotations;

namespace Neurotoxin.Godspeed.Shell.Database
{
    public class QueryBuilder<T>
    {
        private readonly string _primaryKey;
        private readonly Type _primaryKeyType;
        private readonly StringBuilder _stringBuilder = new StringBuilder();
        private const string Comma = ",";
        private const string Before = " \"";
        private const string After = "\"";
        private bool _isSelectSet;
        private bool _isWhereSet;

        public Type TableType { get; private set; }

        public QueryBuilder()
        {
            TableType = typeof (T);
            var pk = TableType.GetProperties().FirstOrDefault(pi => pi.HasAttribute<PrimaryKeyAttribute>());
            if (pk == null) return;
            _primaryKey = pk.Name;
            _primaryKeyType = pk.PropertyType;
        }

        public QueryBuilder<T> Select(params string[] fieldNames)
        {
            _isSelectSet = true;
            _stringBuilder.Append("SELECT");
            if (fieldNames.Length == 0)
            {
                fieldNames = (from pi in TableType.GetProperties()
                              let a = pi.GetAttribute<IgnoreOnReadAttribute>()
                              where a == null
                              select pi.Name).ToArray();
            }

            var first = true;
            foreach (var fieldName in fieldNames)
            {
                if (!first) _stringBuilder.Append(Comma);
                _stringBuilder.Append(Before);
                _stringBuilder.Append(fieldName);
                _stringBuilder.Append(After);
                first = false;
            }
            _stringBuilder.Append(" FROM \"");
            _stringBuilder.Append(TableType.Name);
            _stringBuilder.Append(After);

            return this;
        }

        public QueryBuilder<T> ById(object key)
        {
            EnsureWhere();
            var isString = _primaryKeyType == typeof(string);
            _stringBuilder.Append(Before);
            _stringBuilder.Append(_primaryKey);
            _stringBuilder.Append("\" = ");
            if (isString) _stringBuilder.Append("'");
            _stringBuilder.Append(key);
            if (isString) _stringBuilder.Append("'");

            return this;
        } 

        private void EnsureWhere()
        {
            if (_isWhereSet) return;
            _stringBuilder.Append(" WHERE");
            _isWhereSet = true;
        }

        public string Build()
        {
            if (!_isSelectSet) Select();
            OrderByAttribute attribute = null;
            var orderBy = TableType.GetProperties().FirstOrDefault(pi => (attribute = pi.GetAttribute<OrderByAttribute>()) != null);
            if (orderBy != null)
            {
                _stringBuilder.Append(" ORDER BY ");
                _stringBuilder.Append(orderBy.Name);
                if (attribute.Direction == ListSortDirection.Descending) _stringBuilder.Append(" DESC");
            }
            return _stringBuilder.ToString();
        }
    }
}