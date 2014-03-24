using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biggy.SqlCe.Tests
{
    public class TableHelpers {
        private static SqlCeContext _db;

        public TableHelpers(string connStr) {
            _db = new SqlCeContext(connStr);
        }

        public bool TableExists(string tableName) {
            var sql = "select 1 from information_schema.tables where table_name = @0";
            return _db.Scalar(sql, tableName) != null;
        }

        public void DropTable(string tableName) {
            var sql = "drop table " + tableName;
            _db.Scalar(sql);
        }

        public void Execute(string sql, params object[] args) {
            _db.Execute(sql, args);
        }
    }
}
