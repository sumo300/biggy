using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biggy.SqlCe
{
    /**
    public class __SqlCeDocumentList<T> : Biggy.DBDocumentList<T> where T : new() {

        public override void SetModel() {
          this.Model = new SqlCeTable<dynamic>(this.ConnectionStringName, this.TableName);
        }

        public SqlCeDocumentList(string connectionStringName) :
          base(connectionStringName) { }

        public override int AddRange(List<T> items) {
            // For now we havn't BulkInsert so lets do this simplest way
            items.ForEach(this.Add);
            return items.Count;
        }

        public override void Add(T item) {
            var expando = SetDataForDocument(item);
            expando = Model.Insert(expando);
            if (Model.PrimaryKeyMapping.IsAutoIncementing) {
                // need to update
                var newId = Model.GetPrimaryKey(expando);
                Model.SetPrimaryKey(item, newId);
                // update document body of autoinc Pk value (insert and update should go within transaction, wait for Biggy transactions)
                Update(item);
            }
            base.Add(item);
        }

        public override int Update(T item) {
            var expando = SetDataForDocument(item);
            Model.Update(expando);
            return base.Update(item);
        }


        protected override void TryLoadData()
        {
            try {
                this.Reload();
            }
            catch (System.Data.SqlServerCe.SqlCeException x) {
                if (x.Message.StartsWith("The specified table does not exist.")) {
                    //create the table
                    var idType = Model.PrimaryKeyMapping.IsAutoIncementing 
                               ? " int identity(1,1)" : " nvarchar(255)";
                    var sql = string.Format(
                        "CREATE TABLE {0} ({1} {2} primary key not null, body ntext not null);",
                        Model.DelimitedTableName, Model.PrimaryKeyMapping.DelimitedColumnName, idType);
                    this.Model.Execute(sql);

                    TryLoadData();
                }
                else { throw; }
            }
        }
    }
    */
}
