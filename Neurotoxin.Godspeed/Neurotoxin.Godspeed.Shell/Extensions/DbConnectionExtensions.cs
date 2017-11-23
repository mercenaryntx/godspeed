using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using Neurotoxin.Godspeed.Shell.Constants;
using Neurotoxin.Godspeed.Shell.Database;
using Neurotoxin.Godspeed.Shell.Database.Models;
using ServiceStack.Data;
using ServiceStack.OrmLite;
using System.Linq;
using DataException = System.Data.DataException;

namespace Neurotoxin.Godspeed.Shell.Extensions
{
    public static class DbConnectionExtensions
    {
        public static List<T> Get<T>(this IDbConnection db, Action<T> action = null) where T : ModelBase
        {
            var sql = new QueryBuilder<T>().Build();
            return db.Select<T>(sql).Select(item =>
                                                {
                                                    item.ItemState = ItemState.Persisted;
                                                    if (action != null) action.Invoke(item);
                                                    return item;
                                                }).ToList();
        }

        public static TField ReadField<TTable,TField>(this IDbConnection db, string key, string fieldName)
        {
            var qb = new QueryBuilder<TTable>();
            var sql = qb.Select(fieldName).ById(key).Build();
            var res = db.Select<TTable>(sql);
            switch (res.Count)
            {
                case 1:
                    var type = typeof (TTable);
                    return (TField) type.GetProperty(fieldName).GetValue(res.First(), null);
                case 0:
                    throw new DataException(string.Format("Row with the key \"{0}\" doesn't exist in the table \"{1}\"", key, qb.TableType.Name));
                default:
                    throw new DataException(string.Format("Key violation error. Result set contains more than one rows. (Key: {0}, Table: {1})", key, qb.TableType.Name));
            }
        }

        public static void UpdateOnly<T>(this IDbConnection db, T model, ICollection<string> updateFields)
        {
            db.Exec(dbCmd =>
            {
                if (OrmLiteConfig.UpdateFilter != null) OrmLiteConfig.UpdateFilter(dbCmd, model);
                var flag = OrmLiteConfig.DialectProvider.PrepareParameterizedUpdateStatement<T>(dbCmd, updateFields);
                if (string.IsNullOrEmpty(dbCmd.CommandText)) return 0;
                var definition = ModelDefinition<T>.Definition;
                var param = OrmLiteConfig.DialectProvider.GetParam();
                dbCmd.CommandText = string.Format("{0} WHERE {1} = {2}", dbCmd.CommandText, OrmLiteConfig.DialectProvider.GetQuotedColumnName(definition.PrimaryKey.FieldName), param);
                OrmLiteConfig.DialectProvider.SetParameterValues<T>(dbCmd, model);
                var dbDataParameter = dbCmd.CreateParameter();
                dbDataParameter.ParameterName = param;
                dbDataParameter.Value = definition.PrimaryKey.GetValue(model);
                dbCmd.Parameters.Add(dbDataParameter);
                var num = dbCmd.ExecuteNonQuery();
                if (flag && num == 0) throw new OptimisticConcurrencyException();
                return num;
            });
        }

        public static void Persist<T>(this IDbConnection db, T model) where T : ModelBase
        {
            switch (model.ItemState)
            {
                case ItemState.New:
                    db.Save(model);
                    break;
                case ItemState.Dirty:
                    db.UpdateOnly(model, model.DirtyFields.ToList());
                    break;
                case ItemState.Deleted:
                    var definition = ModelDefinition<T>.Definition;
                    db.DeleteById<T>(definition.PrimaryKey.GetValue(model));
                    break;
            }
            Debug.WriteLine(db.GetLastSql());
        }
    }
}