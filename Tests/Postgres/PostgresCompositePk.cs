using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Biggy;
using Biggy.Postgres;

namespace Tests.Postgres {

 

  [Trait("Postgres Store with Composite PKs", "")]
  public class PostgresStoreWithCompositePk {
    string _connectionStringName = "chinookPG";
    IBiggyStore<Property> _propertyStore;
    IBiggyStore<Building> _buildingStore;
    public PostgresStoreWithCompositePk() {
      var context = new PGCache(_connectionStringName);

      // Build a table to play with from scratch each time:
      if (context.TableExists("property")) {
        context.DropTable("property");
      }

      if (context.TableExists("building")) {
        context.DropTable("building");
      }

      var columnDefs = new List<string>();
      columnDefs.Add("property_id integer NOT NULL");
      columnDefs.Add("building_id integer NOT NULL");
      columnDefs.Add("name Text NOT NULL");
      columnDefs.Add("PRIMARY KEY (property_id, building_id)");
      context.CreateTable("building", columnDefs);

      var cache = new PGCache("chinookPG");
      _propertyStore = new PGStore<Property>(cache);
      _buildingStore = new PGStore<Building>(cache);

    }

    [Fact(DisplayName = "Initializes store with Composite Keys")]
    public void Intialize_With_Injected_Context() {
      Assert.True(_buildingStore != null);
    }

    [Fact(DisplayName = "Inserts single record with composite key")]
    public void Inserts_Record_With_Composite_PK() {
      var newBuilding = new Building { PropertyId = 1, BuildingId = 1, Name = "Building A" };
      _buildingStore.Add(newBuilding);

      var fetchBuilding = _buildingStore.Load().First();
      Assert.True(fetchBuilding.PropertyId == 1 && fetchBuilding.BuildingId == 1 && fetchBuilding.Name == "Building A");
    }


    [Fact(DisplayName = "Inserts multiple records with composite keys")]
    public void Inserts_Multiple_Records_With_Composite_PK() {
      var list = new List<Building>();
      for (int i = 1; i <= 10; i++) {
        list.Add(new Building { PropertyId = 1, BuildingId = i, Name = "Building " + i });
      }
      _buildingStore.Add(list);

      var fetchBuildings = _buildingStore.Load();
      var checkBuilding = fetchBuildings.Last();
      Assert.True(fetchBuildings.Count() == 10 && checkBuilding.BuildingId == 10);
    }


    [Fact(DisplayName = "Updates single record with composite key")]
    public void Updates_Record_With_Composite_PK() {
      var list = new List<Building>();
      for (int i = 1; i <= 10; i++) {
        list.Add(new Building { PropertyId = 1, BuildingId = i, Name = "Building " + i });
      }
      _buildingStore.Add(list);

      var updateMe = _buildingStore.Load().FirstOrDefault(b => b.PropertyId == 1 && b.BuildingId == 3);
      updateMe.Name = "Updated Number 3";
      _buildingStore.Update(updateMe);

      var fetchBuilding = _buildingStore.Load().FirstOrDefault(b => b.PropertyId == 1 && b.BuildingId == 3);
      Assert.True(fetchBuilding.PropertyId == 1 && fetchBuilding.BuildingId == 3 && fetchBuilding.Name == "Updated Number 3");
    }

    [Fact(DisplayName = "Deletes single record with composite key")]
    public void Deletes_Single_Record_With_Composite_PK() {
      var list = new List<Building>();
      for (int i = 1; i <= 10; i++) {
        list.Add(new Building { PropertyId = 1, BuildingId = i, Name = "Building " + i });
      }
      int initialCount = list.Count;
      _buildingStore.Add(list);

      var deleteMe = _buildingStore.Load().FirstOrDefault(b => b.PropertyId == 1 && b.BuildingId == 3);
      _buildingStore.Remove(deleteMe);

      var fetchBuildings = _buildingStore.Load();
      Assert.True(initialCount == 10 && fetchBuildings.Count() == 9);
    }

    [Fact(DisplayName = "Deletes range of records with composite keys")]
    public void Deletes_Range_of_Records_With_Composite_PK() {
      var list = new List<Building>();
      for (int i = 1; i <= 10; i++) {
        list.Add(new Building { PropertyId = 1, BuildingId = i, Name = "Building " + i });
      }
      int initialCount = list.Count;
      _buildingStore.Add(list);

      var deleteMe = _buildingStore.Load().Where(b => b.PropertyId == 1 && b.BuildingId > 5);
      _buildingStore.Remove(deleteMe.ToList());

      var fetchBuildings = _buildingStore.Load();
      Assert.True(initialCount == 10 && fetchBuildings.Count() == 5);
    }
  }
}
