using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlServerCe;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Biggy.Extensions;
/*
namespace Biggy.SqlCe
{
    // intended for single implementation for SqlCe Store and DocumentStore
    // but doesn't work because of generic/dynamic mismatches(?)
    // keep this code maybe someone will be able to get it work(?)
    internal class BulkInserter<T> where T : new()
    {
        private readonly BiggyRelationalStore<T> _store;
        private readonly Func<T, dynamic> _docTransform;

        private readonly bool _isDocumentStore;

        public BulkInserter(BiggyRelationalStore<T> store, Func<T, dynamic> docTransform)
        {
            _store = store;
            _isDocumentStore = docTransform != null;
            _docTransform = docTransform ?? (item => item.ToExpando());
        }

        public List<T> BulkInsert(List<T> items)
        {
            if (false == items.Any()) return items;

            DBTableMapping dbtmap = _store.tableMapping;
            var pkMap = dbtmap.PrimaryKeyMapping.First();//HACK: Now Create Insert/Update cmd methods assumes there is single column Pk
            
            var first = ToExpando(items.First());
            var insertCmd = _store.CreateInsertCommand(first); // doesn't work for <T>, strange error: CreateInsertCommand is inaccessible due to it's protection level
            

            // Reuse a connection and commands object, SqlCe has a limit of open sessions
            using (var conn = insertCmd.Connection)
            using (var tx = conn.BeginTransaction())
            {
                PrepareCommand(insertCmd, conn, tx); 

                DbCommand newIdQuery = null, updateCmd = null;
                if (pkMap.IsAutoIncementing) {
                    newIdQuery = _store.CreateCommand("select @@Identity", conn);
                    PrepareCommand(newIdQuery, conn, tx);
                }
                if (_isDocumentStore)
                {
                    updateCmd = _store.CreateUpdateCommand(first);
                    PrepareCommand(updateCmd, conn, tx);
                }


                foreach (var item in items) {
                    if (false == object.ReferenceEquals(first, item)) {
                        var insItem = ToExpando(item);
                        var pValues = GetInsertParamValues(pkMap, insItem);
                        System.Diagnostics.Debug.Assert(insertCmd.Parameters.Count == pValues.Count());
                        for (int i=0; i<insertCmd.Parameters.Count; ++i) {
                            insertCmd.Parameters[i].Value = pValues[i];
                        }
                    }
                    insertCmd.ExecuteNonQuery();

                    if (pkMap.IsAutoIncementing) {
                        var newId = newIdQuery.ExecuteScalar();
                        _store.SetPrimaryKey(item, newId);

                        if (_isDocumentStore) {
                            var updItem = ToExpando(item);
                            updateCmd.Parameters[0].Value = ((dynamic)updItem).body as string;
                            updateCmd.Parameters[1].Value = newId;
                            updateCmd.ExecuteNonQuery();
                        }
                    }
                }
                tx.Commit();
            }
            return items;
        }

        private static void PrepareCommand(DbCommand updateCmd, dynamic conn, dynamic tx) {
            updateCmd.Connection = conn;
            updateCmd.Transaction = tx;
        }

        private List<object> GetInsertParamValues(DbColumnMapping pkMap, dynamic insertedItem) {
            var settings = (IDictionary<string, object>)insertedItem;
            var result = new List<object>();
            var mappedPkPropertyName = pkMap.PropertyName;
            if (pkMap.IsAutoIncementing) {
                var col = settings.FirstOrDefault(x => x.Key.Equals(mappedPkPropertyName, StringComparison.OrdinalIgnoreCase));
                settings.Remove(col);
            }
            foreach (var prop in settings) {
                result.Add(prop.Value);
            }

            return result;
        }

        private dynamic ToExpando(T item) {
            return _docTransform(item);
        }
    }
}
*/