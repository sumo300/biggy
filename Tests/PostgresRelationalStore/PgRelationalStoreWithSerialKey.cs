using NUnit.Framework;
using System;
using System.Linq;
using System.Collections.Generic;
using Biggy.Data.Postgres;
using Biggy.Core;

namespace Tests {
  [TestFixture()]
  [Category("PG Relational Store")]
  public class PgRelationalStoreWithSerialKey {

    pgDbCore _db;

    [SetUp]
    public void init() {
      _db = new pgDbCore("biggy_test");
      DropCreateTestTables();
    }

    void DropCreateTestTables() {
      string propertyTableSql = ""
        + "CREATE TABLE \"Property\" ( \"Id\" serial NOT NULL, \"Name\" text, \"Address\" text, CONSTRAINT pk_property_id PRIMARY KEY (\"Id\"))";
      _db.TryDropTable("Property");
      _db.TransactDDL(propertyTableSql);     
    }

    [Test()]
    public void Relational_Store_Inserts_record_with_serial_id() {
      var propertyStore = new pgRelationalStore<Property>(_db);
      var newProperty = new Property { Name = "Watergate Apartments", Address = "2639 I St NW, Washington, D.C. 20037" };
      propertyStore.Add(newProperty);

      var foundProperty = propertyStore.TryLoadData().FirstOrDefault();
      Assert.IsTrue(foundProperty != null && foundProperty.Id == 1);
    }

    [Test()]
    public void Relational_Store_Inserts_range_of_records_with_serial_id() {
      var propertyStore = new pgRelationalStore<Property>(_db);
      var myBatch = new List<Property>();
      int qtyToAdd = 10;
      for (int i = 1; i <= qtyToAdd; i++) {
        var newProperty = new Property { Name = "New Apartment " + i, Address = "Some Street in a Lonely Town" };
        myBatch.Add(newProperty);
      }
      propertyStore.Add(myBatch);
      var companies = propertyStore.TryLoadData();
      Assert.IsTrue(companies.Count == qtyToAdd);
    }

    [Test()]
    public void Relational_Store_Updates_record_with_serial_id() {
      var PropertyStore = new pgRelationalStore<Property>(_db);
      var newProperty = new Property { Name = "John's Luxury Apartments", Address = "16 Property Parkway, Portland, OR 97204" };
      PropertyStore.Add(newProperty);

      // Now go fetch the record again and update:
      string newName = "John's Low-Rent Apartments";
      var foundProperty = PropertyStore.TryLoadData().FirstOrDefault();

      int idToFind = foundProperty.Id;
      foundProperty.Name = newName;
      PropertyStore.Update(foundProperty);
      foundProperty = PropertyStore.TryLoadData().FirstOrDefault(p => p.Id == idToFind);

      Assert.IsTrue(foundProperty != null && foundProperty.Name == newName);
    }

    [Test()]
    public void Relational_Store_Updates_range_of_records_with_serial_id() {
      var PropertyStore = new pgRelationalStore<Property>(_db);
      var myBatch = new List<Property>();
      int qtyToAdd = 10;
      for (int i = 1; i <= qtyToAdd; i++) {
        myBatch.Add(new Property { Name = "John's Luxury Townhomes" + i, Address = i + " Property Parkway, Portland, OR 97204" });
      }
      PropertyStore.Add(myBatch);

      // Re-load, and update:
      var companies = PropertyStore.TryLoadData();
      for (int i = 0; i < qtyToAdd; i++) {
        companies.ElementAt(i).Name = "John's Low-Rent Brick Homes " + i;
      }
      PropertyStore.Update(companies);

      // Reload, and check updated names:
      companies = PropertyStore.TryLoadData().Where(c => c.Name.Contains("Low-Rent")).ToList();
      Assert.IsTrue(companies.Count == qtyToAdd);
    }

    [Test()]
    public void Relational_Store_Deletes_record_with_serial_id() {
      var PropertyStore = new pgRelationalStore<Property>(_db);
      var newProperty = new Property { Name = "John's High-End Apartments", Address = "16 Property Parkway, Portland, OR 97204" };
      PropertyStore.Add(newProperty);

      // Load from back-end:
      var companies = PropertyStore.TryLoadData();
      int qtyAdded = companies.Count;

      // Delete:
      var foundProperty = companies.FirstOrDefault();
      PropertyStore.Delete(foundProperty);

      int remaining = PropertyStore.TryLoadData().Count;
      Assert.IsTrue(qtyAdded == 1 && remaining == 0);
    }

    [Test()]
    public void Relational_Store_Deletes_range_of_records_with_serial_id() {
      var PropertyStore = new pgRelationalStore<Property>(_db);
      var myBatch = new List<Property>();
      int qtyToAdd = 10;
      for (int i = 0; i < qtyToAdd; i++) {
        myBatch.Add(new Property { Name = "Boardwalk " + i, Address = i + " Property Parkway, Portland, OR 97204" });
      }
      PropertyStore.Add(myBatch);

      // Re-load from back-end:
      var companies = PropertyStore.TryLoadData();
      int qtyAdded = companies.Count;

      // Select 5 for deletion:
      int qtyToDelete = 5;
      var deleteThese = companies.Where(c => c.Id <= qtyToDelete);

      // Delete:
      PropertyStore.Delete(deleteThese);
      int remaining = PropertyStore.TryLoadData().Count;
      Assert.IsTrue(qtyAdded == qtyToAdd && remaining == qtyAdded - qtyToDelete);
    }

    [Test()]
    public void Relational_Store_Deletes_all_records_with_serial_id() {
      var PropertyStore = new pgRelationalStore<Property>(_db);
      var myBatch = new List<Property>();
      int qtyToAdd = 10;
      for (int i = 0; i < qtyToAdd; i++) {
        myBatch.Add(new Property { Name = "Park Place" + i, Address = i + " Property Parkway, Portland, OR 97204" });
      }
      PropertyStore.Add(myBatch);

      // Re-load from back-end:
      var companies = PropertyStore.TryLoadData();
      int qtyAdded = companies.Count;

      // Delete:
      PropertyStore.DeleteAll();
      int remaining = PropertyStore.TryLoadData().Count;
      Assert.IsTrue(qtyAdded == qtyToAdd && remaining == 0);
    }
  }
}

