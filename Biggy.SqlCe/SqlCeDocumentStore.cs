using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biggy.SqlCe
{
    public class SqlCeDocumentStore<T> : BiggyDocumentStore<T> where T : new() {

        public SqlCeDocumentStore(string connectionStringName) : base(new SqlCeCache(connectionStringName)) { }
        public SqlCeDocumentStore(string connectionStringName, string tableName) : base(new SqlCeCache(connectionStringName), tableName) { }
        public SqlCeDocumentStore(DbCache dbCache) : base(dbCache, null) { }
        public SqlCeDocumentStore(DbCache dbCache, string tableName) : base(dbCache, tableName) { }


        public override BiggyRelationalStore<dynamic> getModel() {
            return new SqlCeStore<dynamic>(this.DbCache);
        }

        public override T Insert(T item) {
            var expando = SetDataForDocument(item);
            expando = Model.Insert(expando);
            if (Model.PrimaryKeyMapping.IsAutoIncementing) {
                var newId = Model.GetPrimaryKey(expando);
                this.SetPrimaryKey(item, newId);
                // update document body of autoinc Pk value (insert and update should go within transaction, wait for Biggy transactions)
                Update(item);
            }
            return item;
        }

        public override List<T> BulkInsert(List<T> items) {
            if (false == items.Any()) return items;
            var first = items.First();
            var expando = SetDataForDocument(first);
            string bodyPart, searchPart;
            
            var insertCmd = Model.CreateInsertCommand(expando);
            var updateCmd = Model.CreateUpdateCommand(expando);

            bool fetchNewId = Model.PrimaryKeyMapping.IsAutoIncementing;
            bool hasFullText = false;   // currently not working for: non-auto Pks, fulltext docs / search column

            // Reuse a connection and commands object, SqlCe has a limit of open sessions
            using (var conn = Model.Cache.OpenConnection())
            using (var tx = conn.BeginTransaction()) {
                // prepare commands
                var newIdQuery = Model.CreateCommand("select @@Identity", conn);
                newIdQuery.Transaction = tx;
                insertCmd.Connection = conn;
                insertCmd.Transaction = tx;
                updateCmd.Connection = conn;
                updateCmd.Transaction = tx;
                
                foreach (var item in items) {
                    if (false == object.ReferenceEquals(first, item)) {
                        expando = SetDataForDocument(item);
                    }
                    insertCmd.Parameters[0].Value = ((dynamic)expando).body as string;
                    insertCmd.ExecuteNonQuery();

                    if (true == fetchNewId) {
                        var newId = newIdQuery.ExecuteScalar();
                        SetPrimaryKey(item, newId);
                        expando = SetDataForDocument(item);
                        updateCmd.Parameters[0].Value = ((dynamic)expando).body as string;
                        updateCmd.Parameters[1].Value = newId;
                        updateCmd.ExecuteNonQuery();
                    }
                }
                tx.Commit();
            }
            return items;
        }

        public override T Update(T item) {
            var expando = SetDataForDocument(item);
            Model.Update(expando);
            return item;
        }

        public override T Delete(T item) {
            Model.Delete(item);
            return item;
        }

        public override List<T> Delete(List<T> items) {
            Model.Delete(items);
            return items;
        }

        protected override List<T> TryLoadData() {
            try {
                return this.LoadAll();
            }
            catch (System.Data.SqlServerCe.SqlCeException x) {
                if (x.Message.StartsWith("The specified table does not exist.")) {
                    //create the table
                    var idType = Model.PrimaryKeyMapping.IsAutoIncementing
                               ? " int identity(1,1)" : " nvarchar(255)";
                    string fullTextColumn = string.Empty;
                    if (this.FullTextFields.Length > 0) {
                        fullTextColumn = ", search ntext";
                    }
                    var sql = string.Format(
                        "CREATE TABLE {0} ({1} {2} primary key not null, body ntext not null{3});",
                        this.TableMapping.DelimitedTableName, this.PrimaryKeyMapping.DelimitedColumnName, idType, fullTextColumn);
                    this.Model.Execute(sql);

                    return TryLoadData();
                }
                else { throw; }
            }
        }
    }
}
