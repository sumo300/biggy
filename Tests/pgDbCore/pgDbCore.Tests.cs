using NUnit.Framework;
using System;
using System.Linq;
using System.Collections.Generic;
using Biggy.Postgres;
using Biggy.Core;

namespace Tests {
  [TestFixture()]
  [Category("PG DbCore")]
  public class PgDbCore_Tests {

    [SetUp]
    public void init() {
      DropCreateTestTables();
    }

    void DropCreateTestTables() {
      IDbCore _db = new pgDbCore("biggy_test");
      string propertyTableSql = ""
        + "CREATE TABLE \"Property\" ( \"Id\" serial NOT NULL, \"Name\" text, \"Address\" text, CONSTRAINT pk_property_id PRIMARY KEY (\"Id\"));";

      string BuildingTableSql = ""
        + "CREATE TABLE \"Building\" ( \"BIN\" text NOT NULL, \"Identifier\" text, \"PropertyId\" text, CONSTRAINT pk_building_bin PRIMARY KEY (\"BIN\"));";

      string WorkOrderTableSql = ""
        + "CREATE TABLE \"wk_order\" ( \"wo_id\" serial NOT NULL, \"desc\" text, CONSTRAINT pk_wk_order_wo_id PRIMARY KEY (\"wo_id\"));";

      string UnitTableSql = ""
        + "CREATE TABLE \"unit\" ( \"unit_id\" serial NOT NULL, \"BIN\" text, \"unit_no\" text, CONSTRAINT pk_unit_unit_id PRIMARY KEY (\"unit_id\"));";


      _db.TryDropTable("Property");
      _db.TryDropTable("Building");
      _db.TryDropTable("wk_order");
      _db.TryDropTable("unit");

      _db.TransactDDL(UnitTableSql);   
      _db.TransactDDL(propertyTableSql);
      _db.TransactDDL(BuildingTableSql);
      _db.TransactDDL(WorkOrderTableSql);    
    }

    [Test()]
    public void Loads_Table_Names_From_Db() {
      IDbCore db = new pgDbCore("biggy_test");
      Assert.True(db.DbTableNames.Count > 0);
    }

    [Test()]
    public void Loads_Column_Data_From_Db() {
      IDbCore db = new pgDbCore("biggy_test");
      Assert.True(db.DbColumnsList.Count > 0);
    }

    [Test()]
    public void Creates_Table_Mapping_for_Type() {
      IDbCore db = new pgDbCore("biggy_test");
      var testTableMapping = db.getTableMappingFor<Property>();
      Assert.True(testTableMapping.MappedTypeName == "Property" && testTableMapping.ColumnMappings.Count() == 3);
    }

    [Test()]
    public void Check_If_Table_Exists() {
      IDbCore db = new pgDbCore("biggy_test");
      bool existingTablePresent = db.TableExists("Property");
      bool nonsenseTableExists = db.TableExists("Nonsense");
      Assert.True(existingTablePresent && !nonsenseTableExists);
    }

    [Test()]
    public void Maps_Properties_to_Proper_Cased_Columns() {
      bool allPropertiesMapped = false;
      IDbCore db = new pgDbCore("biggy_test");
      var testTableMapping = db.getTableMappingFor<Property>();
      var properties = typeof(Property).GetProperties();
      foreach (var property in properties) {
        var column = testTableMapping.ColumnMappings.FindByProperty(property.Name);
        allPropertiesMapped = true;
        if (column == null) {
          allPropertiesMapped = false;
          break;
        }
      }
      Assert.True(allPropertiesMapped);
    }

    [Test()]
    public void Maps_Properties_pg_Idiomatic_Columns() {
      bool allPropertiesMapped = false;
      IDbCore db = new pgDbCore("biggy_test");

      // Unit class should map to unit table, with pg-standard column names:
      // UnitId => unit_id
      // BuildingId => building_id
      // UnitNo => unit_no

      var testTableMapping = db.getTableMappingFor<Unit>();
      var properties = typeof(Unit).GetProperties();
      foreach (var property in properties) {
        var column = testTableMapping.ColumnMappings.FindByProperty(property.Name);
        allPropertiesMapped = true;
        if (column == null) {
          allPropertiesMapped = false;
          break;
        }
      }
      Assert.True(allPropertiesMapped);
    }


    [Test()]
    public void Maps_Properties_Using_Attributes() {
      bool allPropertiesMapped = false;
      IDbCore db = new pgDbCore("biggy_test");

      // WorkOrder class should map to unit wk_order table, with mis-matched table and column names handled by attributes:
      // WorkOrder => wk_order
      // WorkOrderId => wo_id
      // Description => desc

      var testTableMapping = db.getTableMappingFor<WorkOrder>();
      var properties = typeof(WorkOrder).GetProperties();
      foreach (var property in properties) {
        var column = testTableMapping.ColumnMappings.FindByProperty(property.Name);
        allPropertiesMapped = true;
        if (column == null) {
          allPropertiesMapped = false;
          break;
        }
      }
      Assert.True(allPropertiesMapped);
    }

  }
}

