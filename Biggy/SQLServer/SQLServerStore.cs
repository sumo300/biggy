using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biggy.SQLServer
{
  public class SQLServerStore<T> : BiggyRelationalStore<T> where T : new()
  {
    public SQLServerStore(SQLServerContext context) : base(context) { }
    public SQLServerStore(string connectionString) : base(new SQLServerContext(connectionString)) { }
  }
}
