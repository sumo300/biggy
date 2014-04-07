using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Biggy;

namespace Biggy.SqlCe.Tests
{
  [Trait("SQL Server Context", "")]
  public class SqlCeContextTest
  {
    string _connectionStringName = "chinook";
    SqlCeCache _context;

    public SqlCeContextTest()
    {
      _context = new SqlCeCache(_connectionStringName);
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
      columnDefs.Add("id int IDENTITY(1,1) PRIMARY KEY");
      columnDefs.Add("name nTEXT");

      _context.CreateTable("test_table", columnDefs);
      Assert.True(_context.TableExists("test_table") && _context.DbTableNames.Contains("test_table"));
    }
  }
}
