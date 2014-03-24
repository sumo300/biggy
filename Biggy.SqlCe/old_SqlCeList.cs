using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biggy.SqlCe
{
    /**
    public class SqlCeList<T> : Biggy.DBList<T> where T : new() {
        protected SqlCeTable<T> SqlCeModel {
            get { return (SqlCeTable<T>)this.Model; }
        }

        public override void SetModel() {
          this.Model = new SqlCeTable<T>(this.ConnectionStringName, this.TableName);
        }

        public SqlCeList(string connectionStringName, string tableName = "guess") :
          base(connectionStringName, tableName) { }
    }
    */
}
