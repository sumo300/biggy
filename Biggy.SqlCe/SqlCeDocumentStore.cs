using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biggy.SqlCe
{
    public class SqlCeDocumentStore<T> : BiggyDocumentStore<T> where T : new()
    {
        public SqlCeDocumentStore(BiggyRelationalContext context) : base(context) { }
        public SqlCeDocumentStore(BiggyRelationalContext context, string tableName) : base(context, tableName) { }

        public SqlCeDocumentStore(string connectionStringName) : base(new SqlCeContext(connectionStringName)) { }
        public SqlCeDocumentStore(string connectionStringName, string tableName) : base(new SqlCeContext(connectionStringName), tableName) { }

        // Need overidable SetModel to set specific RelationalStore

        public override T Insert(T item)
        {
            var expando = SetDataForDocument(item);
            expando = Model.Insert(expando);
            if (Model.PrimaryKeyMapping.IsAutoIncementing)
            {
                // need to update
                var newId = Model.GetPrimaryKey(expando);
                this.SetPrimaryKey(item, newId);
                // update document body of autoinc Pk value (insert and update should go within transaction, wait for Biggy transactions)
                Update(item);
            }
            return item;
        }

        public override List<T> BulkInsert(List<T> items)
        {
            // For now we havn't BulkInsert so lets do this simplest way
            return items.Select(this.Insert).ToList();
        }

        public override T Update(T item)
        {
            var expando = SetDataForDocument(item);
            Model.Update(expando);
            return item;
        }

        public override T Delete(T item)
        {
            Model.Delete(item);
            return item;
        }

        public override List<T> Delete(List<T> items)
        {
            Model.Delete(items);
            return items;
        }

        protected override List<T> TryLoadData()
        {
            try
            {
                return this.LoadAll();
            }
            catch (System.Data.SqlServerCe.SqlCeException x)
            {
                if (x.Message.StartsWith("The specified table does not exist."))
                {
                    //create the table
                    var idType = Model.PrimaryKeyMapping.IsAutoIncementing
                               ? " int identity(1,1)" : " nvarchar(255)";
                    var sql = string.Format(
                        "CREATE TABLE {0} ({1} {2} primary key not null, body ntext not null);",
                        this.TableMapping.DelimitedTableName, this.PrimaryKeyMapping.DelimitedColumnName, idType);
                    this.Model.Execute(sql);

                    return TryLoadData();
                }
                else { throw; }
            }
        }
    }
}
