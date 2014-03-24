using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biggy.SqlCe
{
    public class SqlCeContext : Biggy.SQLServer.SQLServerContext
    {
        public SqlCeContext(string connectionStringName) : base(connectionStringName) { }
        
    }
}
