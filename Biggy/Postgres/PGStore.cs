using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biggy.Postgres
{
  public class PGStore<T> : BiggyRelationalStore<T> where T : new() {
    public PGStore(PGContext context) : base(context) { }
    public PGStore(string connectionString) : base(new PGContext(connectionString)) { }
  }
}
