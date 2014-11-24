using System;
using System.Linq;
using Biggy.Core;

namespace Tests {
  // Class and property names match Table and Column names
  // Db Table has Auto-int PK
  public class Property {
    public int Id { get; set; }
    public string Name { get; set; }
    public string Address { get; set; }
  }

  // Class with string PK:
  public class Building {
    public string BIN { get; set; }
    public int PropertyId { get; set; }
    public string Identifier { get; set; }
  }

  // Class for table with pg-idiomatic names:

  // Table name: unit
  public class Unit {
    // Column name: unit_id
    public int UnitId { get; set; }

    // column name: building_id
    public string BIN { get; set; }

    // Matches unit_no
    public string UnitNo { get; set; }
  }

  // Class for table with mis-matched names:

  // Table name: bedroom_size
  [DbTable("wk_order")]
  public class WorkOrder {
    // Matches bedroom_size_id
    [DbColumn("wo_id")]
    public int WorkOrderId { get; set; }

    // matches qty_bedrooms
    [DbColumn("desc")]
    public string Description { get; set; }
  }
}