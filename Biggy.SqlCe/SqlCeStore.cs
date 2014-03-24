using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biggy.SqlCe
{
    public class SqlCeStore<T> : BiggyRelationalStore<T> where T : new()
    {
        public SqlCeStore(SqlCeContext context) : base(context) { }
        public SqlCeStore(string connectionString) : base(new SqlCeContext(connectionString)) { }
    }
}
