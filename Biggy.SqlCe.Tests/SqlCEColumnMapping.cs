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

        var pkMap = db.TableMapping.PrimaryKeyMapping;
      Assert.Single(pkMap);
      Assert.Equal("Id", pkMap[0].PropertyName);
      Assert.Equal("AlbumId", pkMap[0].ColumnName);
      Assert.True(pkMap[0].IsAutoIncementing);
    }

    [Fact(DisplayName = "Maps Pk even if wasn't specified by attribute")]
    public void MapingNotSpecifiedPk() {
      var db = new SqlCeStore<Genre>(_connectionStringName);  //, "Genre"

      var pkMap = db.TableMapping.PrimaryKeyMapping;
      Assert.Single(pkMap);
      Assert.Equal("Id", pkMap[0].PropertyName);
      Assert.Equal("GenreId", pkMap[0].ColumnName);
      Assert.True(pkMap[0].IsAutoIncementing);
    }

    [Fact(DisplayName = "Properly maps Pk when is not auto incrementing")]
    public void MapingNotAutoIncPk()
    {
        var db = new SqlCeDocumentStore<MonkeyDocument>(_connectionStringName);

        var pkMap = db.TableMapping.PrimaryKeyMapping;
        Assert.Single(pkMap);
        Assert.Equal("Name", pkMap[0].PropertyName);
        Assert.Equal("Name", pkMap[0].ColumnName);
        Assert.Equal(typeof(string), pkMap[0].DataType);
        Assert.False(pkMap[0].IsAutoIncementing);
    }
  }
}
