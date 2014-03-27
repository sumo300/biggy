using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Biggy;
using Biggy.Postgres;

namespace Tests.Postgres
{
  [Trait("PG Context", "")]
  public class PostgresContext
  {
    string _connectionStringName = "chinookPG";
    PGHost _context;

    public PostgresContext()
    {
      _context = new PGHost(_connectionStringName);
      if(_context.TableExists("test_table"))
      {
        _context.DropTable("test_table");
      }
    }

    [Fact(DisplayName = "Confirms a table does not exist")]
    public void Creates_New_Table_With_Arbitrary_SQL()
    {
      var exists = _context.TableExists("test_table");
      Assert.False(exists);
    }

    [Fact(DisplayName = "Executes Arbitrary SQL")]
    public void Execute_Abritrary_SQL()
    {
      var columnDefs = new List<string>();
      columnDefs.Add("id serial PRIMARY KEY");
      columnDefs.Add("name TEXT");

      _context.CreateTable("test_table", columnDefs);
      Assert.True(_context.TableExists("test_table") && _context.DbTableNames.Contains("test_table"));
    }
  }
}
