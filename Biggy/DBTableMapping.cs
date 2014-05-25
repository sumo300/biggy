using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biggy {
  public class DBTableMapping {
    string _delimiterFormatString;

    public DBTableMapping(string delimiterFormatString) {
      _delimiterFormatString = delimiterFormatString;
      this.ColumnMappings = new DbColumnMappingLookup(_delimiterFormatString);
      this.PrimaryKeyMapping = new List<DbColumnMapping>();
    }

    public string DBTableName { get; set; }
    public string MappedTypeName { get; set; }

    public string DelimitedTableName {
      get { return string.Format(_delimiterFormatString, this.DBTableName); }
    }

    public List<DbColumnMapping> PrimaryKeyMapping { get; set; }
    public DbColumnMappingLookup ColumnMappings { get; set; }
  }
}
