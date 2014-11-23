using System;
using System.IO;
using System.Linq;
using Biggy.Data.Sqlite;
using NUnit.Framework;

namespace Tests.Sqlite
{
    [TestFixture()]
    [Category("SQLite DbCore")]
    public class SqliteDbCore_Tests
    {
        private SqliteDbCore _db;
        private string _filename = "";

        [SetUp]
        public void init()
        {
            _db = new SqliteDbCore("BiggyTestSqliteDbCore");
            _filename = _db.DBFilePath;
            DropCreateTestTables();
        }

        [TearDown]
        public void Cleanup()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            File.Delete(_filename);
        }

        private void DropCreateTestTables()
        {
            string propertyTableSql = ""
              + "CREATE TABLE Property (Id INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL, Name text, Address text)";
            _db.TryDropTable("Property");
            _db.TransactDDL(propertyTableSql);

            string BuildingTableSql = ""
              + "CREATE TABLE Building ( BIN text PRIMARY KEY NOT NULL, Identifier text, PropertyId int )";
            _db.TryDropTable("Building");
            if (!_db.TableExists("Building"))
            {
                _db.TransactDDL(BuildingTableSql);
            }

            string UnitTableSql = ""
              + "CREATE TABLE unit ( unit_id INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL, BIN TEXT, unit_no TEXT )";
            _db.TryDropTable("unit");
            _db.TransactDDL(UnitTableSql);

            string WorkOrderTableSql = ""
              + "CREATE TABLE wk_order ( wo_id INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL, desc text)";
            _db.TryDropTable("wk_order");
            _db.TransactDDL(WorkOrderTableSql);
        }

        [Test()]
        public void Loads_Table_Names_From_Db()
        {
            Assert.True(_db.DbTableNames.Count > 0);
        }

        [Test()]
        public void Loads_Column_Data_From_Db()
        {
            Assert.True(_db.DbColumnsList.Count > 0);
        }

        [Test()]
        public void Creates_Table_Mapping_for_Type()
        {
            var testTableMapping = _db.getTableMappingFor<Property>();
            Assert.True(testTableMapping.MappedTypeName == "Property" && testTableMapping.ColumnMappings.Count() == 3);
        }

        [Test()]
        public void Check_If_Table_Exists()
        {
            bool existingTablePresent = _db.TableExists("Property");
            bool nonsenseTableExists = _db.TableExists("Nonsense");
            Assert.True(existingTablePresent && !nonsenseTableExists);
        }

        [Test()]
        public void Maps_Properties_to_Proper_Cased_Columns()
        {
            bool allPropertiesMapped = false;
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