using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Biggy.SqlCe.Tests {
  [Trait("SQL CE Compact column mapping", "")]
  public class SqlCEColumnMapping {
    public string _connectionStringName = "chinook";

    [Fact(DisplayName = "Maps Pk if specified by attribute")]
    public void MapingSpecifiedPk() {
      var db = new SqlCeStore<Album>(_connectionStringName);  //, "Album"

      var pkMap = db.PrimaryKeyMapping;
      Assert.Equal("Id", pkMap.PropertyName);
      Assert.Equal("AlbumId", pkMap.ColumnName);
      Assert.True(pkMap.IsAutoIncementing);
    }

    [Fact(DisplayName = "Maps Pk even if wasn't specified by attribute")]
    public void MapingNotSpecifiedPk() {
      var db = new SqlCeStore<Genre>(_connectionStringName);  //, "Genre"

      var pkMap = db.PrimaryKeyMapping;
      Assert.Equal("Id", pkMap.PropertyName);
      Assert.Equal("GenreId", pkMap.ColumnName);
      Assert.True(pkMap.IsAutoIncementing);
    }
  }
}
