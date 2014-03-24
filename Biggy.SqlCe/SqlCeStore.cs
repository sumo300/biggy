using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biggy.SqlCe
{
    public class SqlCeStore<T> : BiggyRelationalStore<T> where T : new()
    {
        public SqlCeStore(SqlCeContext context) : base(context) { }
        public SqlCeStore(string connectionString) : base(new SqlCeContext(connectionString)) { }

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

        public override List<T> BulkInsert(List<T> items)
        {
            // It can be done much better, see: http://sqlcebulkcopy.codeplex.com/
            // but for now... do it simples way possible
            Execute(items.Select(this.CreateInsertCommand));
            return items;
        }
    }
}
