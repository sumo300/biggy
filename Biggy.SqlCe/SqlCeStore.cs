using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Biggy.Extensions;

namespace Biggy.SqlCe
{
    public class SqlCeStore<T> : Biggy.SQLServer.SQLServerStore<T> where T : new() {
        
        public SqlCeStore(DbCache dbCache) : base(dbCache) { }
        public SqlCeStore(string connectionString) : base(new SqlCeCache(connectionString)) { }

        public override DbConnection OpenConnection() {
            return this.Cache.OpenConnection();
        }

        public override T Insert(T item) {
            if (BeforeSave(item)) {
                object newId = null;
                var cmd = (DbCommand)this.CreateInsertCommand(item);

                using (var conn = cmd.Connection)
                using (var tx = conn.BeginTransaction()) {
                    cmd.Transaction = tx;
                    int rowsCnt = cmd.ExecuteNonQuery();

                    // retrieve new Id
                    cmd.CommandText = "select @@Identity";
                    newId = cmd.ExecuteScalar();

                    if (rowsCnt == 1 && newId != null)
                        tx.Commit();
                    //TODO: ?? else return null;
                }
                this.SetPrimaryKey(item, newId);
                Inserted(item);
            }
            return item;
        }

        // It can be done much better, see: http://sqlcebulkcopy.codeplex.com/, but this shouldn't be very bad.
        // I also tried to make one generic BulkInserter for store and documentStore but there are many generic/dynamic things so maybe not worth it 
        public override List<T> BulkInsert(List<T> items) {
            if (false == items.Any()) return items;

            DBTableMapping dbtmap = this.tableMapping;
            var pkMap = dbtmap.PrimaryKeyMapping.First();//HACK: Now everywhere is assumed there is single column Pk

            var first = items.First();
            var insertCmd = this.CreateInsertCommand(first);

            // Reuse a connection and commands object, SqlCe has a limit of open sessions
            using (var conn = insertCmd.Connection)
            using (var tx = conn.BeginTransaction()) {
                insertCmd.Transaction = tx;

                DbCommand newIdQuery = null;
                if (pkMap.IsAutoIncementing) {
                    newIdQuery = this.CreateCommand("select @@Identity", conn);
                    newIdQuery.Transaction = tx;
                }

                foreach (var item in items) {
                    if (false == object.ReferenceEquals(first, item)) {
                        var pValues = GetInsertParamValues(pkMap, item);
                        System.Diagnostics.Debug.Assert(insertCmd.Parameters.Count == pValues.Count());
                        for (int i = 0; i < insertCmd.Parameters.Count; ++i) {
                            insertCmd.Parameters[i].Value = pValues[i]; // reuse cmd object, copy new values
                        }
                    }
                    insertCmd.ExecuteNonQuery();

                    if (pkMap.IsAutoIncementing) {
                        var newId = newIdQuery.ExecuteScalar();
                        this.SetPrimaryKey(item, newId);
                    }
                }
                tx.Commit();
            }
            return items;
        }

        private object[] GetInsertParamValues(DbColumnMapping pkMap, T insertedItem) {
            var expando = insertedItem.ToExpando();
            var settings = (IDictionary<string, object>)expando;
            var mappedPkPropertyName = pkMap.PropertyName;
            if (pkMap.IsAutoIncementing) {
                var col = settings.FirstOrDefault(x => x.Key.Equals(mappedPkPropertyName, StringComparison.OrdinalIgnoreCase));
                settings.Remove(col);
            }

            return settings.Values.ToArray();
        }
    }
}
