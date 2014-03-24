using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Biggy.SqlCe
{
    /**
    public class SqlCeTable<T> : Biggy.SQLServer.SQLServerTable<T> where T : new() 
    {
        public SqlCeTable(string connectionStringName)
                         : base(connectionStringName) { }

        public SqlCeTable(string connectionStringName,
                          string tableName = "",
                          string primaryKeyField = "",
                          bool pkIsIdentityColumn = true)
                          : base(connectionStringName, tableName) { }


        public override T Insert(T item) {
            if (BeforeSave(item)) {
                object newId = null;
                var cmd = (DbCommand)_createInsertCommand(item);

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

        public override int BulkInsert(List<T> items) {
            // It can be done much better, see: http://sqlcebulkcopy.codeplex.com/
            // but for now... do it simples way possible
            return Execute(items.Select(_createInsertCommand));
        }

        protected override bool columnIsAutoIncrementing(string columnName) {
            const long AUTOINC_MAGIC = 1L; 
            var sql = @"select AUTOINC_INCREMENT 
                          from information_schema.columns
                         where TABLE_NAME = @0 and COLUMN_NAME = @1";
            var result = this.Scalar(sql, this.TableName, columnName);
            return result is long && AUTOINC_MAGIC == (long)result;
        }

        protected override DbConnection OpenConnection() {
            var connection = new System.Data.SqlServerCe.SqlCeConnection(this.ConnectionString);
            connection.Open();
            return connection;
        }

        //HACK! Invoking private member from base (DBTable) class
        // DBTable.CreateInsertCommand is very handy but BuildCommands requires too much effort to
        // make it build Insert cmd, so for now we can invoke CreateInsertCommand directly via reflection
        private MethodInfo _createInsertCmdBaseMethod = null;
        private DbCommand _createInsertCommand(T insertItem) {
            _createInsertCmdBaseMethod = _createInsertCmdBaseMethod ?? GetBaseMethod("CreateInsertCommand");
            //MethodInfo gInfo = mInfo.MakeGenericMethod(typeof(T));
            object oCmd = _createInsertCmdBaseMethod.Invoke(this, new object[] { insertItem });

            Debug.Assert(oCmd is DbCommand);
            return (DbCommand)oCmd;
        }

        private MethodInfo GetBaseMethod(string methodName) {
            // try to find private member of base class
            var dbTableType = this.GetType().BaseType.BaseType;
            Debug.Assert(dbTableType.Name.StartsWith("DBTable"));

            return dbTableType.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance );
        }
    }
    */
}
