using System;
using System.Linq;
using Biggy.Core;
using Biggy.Data.Sqlite;
using NUnit.Framework;

namespace Tests
{
    [TestFixture()]
    [Category("SQLite DbCore")]
    public class sqliteDbCore_Tests
    {
        private IDbCore _db;

        [SetUp]
        public void init()
        {
            _db = new SqliteDbCore("BiggyTest");
        }

        private void DropCreateTestTables()
        {
            string propertyTableSql = ""
              + "CREATE TABLE Property (Id INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL, Name text, Address text)";
            _db.TryDropTable("Property");
            _db.ExecuteScalar(propertyTableSql);

            string BuildingTableSql = ""
              + "CREATE TABLE Building ( BIN text PRIMARY KEY NOT NULL, Identifier text, PropertyId int )";
            _db.TryDropTable("Building");
            if (!_db.TableExists("Building"))
            {
                _db.ExecuteScalar(BuildingTableSql);
            }

            string UnitTableSql = ""
              + "CREATE TABLE unit ( unit_id INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL, BIN TEXT, unit_no TEXT )";
            _db.TryDropTable("unit");
            _db.ExecuteScalar(UnitTableSql);

            string WorkOrderTableSql = ""
              + "CREATE TABLE wk_order ( wo_id INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL, \"desc\" text)";
            _db.TryDropTable("wk_order");
            _db.ExecuteScalar(WorkOrderTableSql);
        }

        [Test()]
        public void Loads_Table_Names_From_Db()
        {
            //IDbCore _db = new sqliteDbCore("BiggyTest");
            Assert.True(_db.DbTableNames.Count > 0);
        }

        [Test()]
        public void Loads_Column_Data_From_Db()
        {
            //IDbCore _db = new sqliteDbCore("BiggyTest");
            Assert.True(_db.DbColumnsList.Count > 0);
        }

        [Test()]
        public void Creates_Table_Mapping_for_Type()
        {
            //IDbCore _db = new sqliteDbCore("BiggyTest");
            var testTableMapping = _db.getTableMappingFor<Property>();
            Assert.True(testTableMapping.MappedTypeName == "Property" && testTableMapping.ColumnMappings.Count() == 3);
        }

        [Test()]
        public void Check_If_Table_Exists()
        {
            //IDbCore _db = new sqliteDbCore("BiggyTest");
            bool existingTablePresent = _db.TableExists("Property");
            bool nonsenseTableExists = _db.TableExists("Nonsense");
            Assert.True(existingTablePresent && !nonsenseTableExists);
        }

        [Test()]
        public void Maps_Properties_to_Proper_Cased_Columns()
        {
            bool allPropertiesMapped = false;
            //IDbCore _db = new sqliteDbCore("BiggyTest");
            var testTableMapping = _db.getTableMappingFor<Property>();
            var properties = typeof(Property).GetProperties();
            foreach (var property in properties)
            {
                var column = testTableMapping.ColumnMappings.FindByProperty(property.Name);
                allPropertiesMapped = true;
                if (column == null)
                {
                    allPropertiesMapped = false;
                    break;
                }
            }
            Assert.True(allPropertiesMapped);
        }

        [Test()]
        public void Maps_Properties_pg_Idiomatic_Columns()
        {
            bool allPropertiesMapped = false;
            //IDbCore _db = new sqliteDbCore("BiggyTest");

            // Unit class should map to unit table, with pg-standard column names:
            // UnitId => unit_id
            // BuildingId => building_id
            // UnitNo => unit_no

            var testTableMapping = _db.getTableMappingFor<Unit>();
            var properties = typeof(Unit).GetProperties();
            foreach (var property in properties)
            {
                var column = testTableMapping.ColumnMappings.FindByProperty(property.Name);
                allPropertiesMapped = true;
                if (column == null)
                {
                    allPropertiesMapped = false;
                    break;
                }
            }
            Assert.True(allPropertiesMapped);
        }

        [Test()]
        public void Maps_Properties_Using_Attributes()
        {
            bool allPropertiesMapped = false;
            //IDbCore _db = new sqliteDbCore("BiggyTest");

            // WorkOrder class should map to unit wk_order table, with mis-matched table and column names handled by attributes:
            // WorkOrder => wk_order
            // WorkOrderId => wo_id
            // Description => desc

            var testTableMapping = _db.getTableMappingFor<WorkOrder>();
            var properties = typeof(WorkOrder).GetProperties();
            foreach (var property in properties)
            {
                var column = testTableMapping.ColumnMappings.FindByProperty(property.Name);
                allPropertiesMapped = true;
                if (column == null)
                {
                    allPropertiesMapped = false;
                    break;
                }
            }
            Assert.True(allPropertiesMapped);
        }
    }
}