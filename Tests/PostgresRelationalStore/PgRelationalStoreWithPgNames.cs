using NUnit.Framework;
using System;
using System.Linq;
using System.Collections.Generic;
using Biggy.Data.Postgres;
using Biggy.Core;

namespace Tests.Postgres {
  [TestFixture()]
  [Category("PG Relational Store")]
  public class PgRelationalStoreWithPgNames {

    PgDbCore _db;

    [SetUp]
    public void init() {
      _db = new PgDbCore("biggy_test");
      DropCreateTestTables();
    }

    void DropCreateTestTables() {
      string UnitTableSql = ""
        + "CREATE TABLE \"unit\" ( \"unit_id\" serial NOT NULL, \"BIN\" text, \"unit_no\" text, CONSTRAINT pk_unit_unit_id PRIMARY KEY (\"unit_id\"))";
      _db.TryDropTable("unit");
      _db.TransactDDL(UnitTableSql);
    }

    [Test()]
    public void Relational_Store_Inserts_record_with_pg_names() {
      var UnitStore = new PgRelationalStore<Unit>(_db);
      var newUnit = new Unit { BIN = "OR03-01", UnitNo = "A-101" };
      UnitStore.Add(newUnit);

      var foundUnit = UnitStore.TryLoadData().FirstOrDefault();
      Assert.IsTrue(foundUnit != null && foundUnit.UnitId == 1);
    }

    [Test()]
    public void Relational_Store_Inserts_range_of_records_with_pg_names() {
      var UnitStore = new PgRelationalStore<Unit>(_db);
      var myBatch = new List<Unit>();
      int qtyToAdd = 10;
      for (int i = 1; i <= qtyToAdd; i++) {
        var newUnit = new Unit { BIN = "OR04-" + i, UnitNo = "B-10" + i };
        myBatch.Add(newUnit);
      }
      UnitStore.Add(myBatch);
      var companies = UnitStore.TryLoadData();
      Assert.IsTrue(companies.Count == qtyToAdd);
    }

    [Test()]
    public void Relational_Store_Updates_record_with_pg_names() {
      var UnitStore = new PgRelationalStore<Unit>(_db);
      var newUnit = new Unit { BIN = "OR05-01", UnitNo = "C-101" };
      UnitStore.Add(newUnit);

      // Now go fetch the record again and update:
      string newName = "Updated-401";
      var foundUnit = UnitStore.TryLoadData().FirstOrDefault();
      foundUnit.UnitNo = newName;
      UnitStore.Update(foundUnit);
      Assert.IsTrue(foundUnit != null && foundUnit.UnitNo == newName);
    }

    [Test()]
    public void Relational_Store_Updates_range_of_records_with_pg_names() {
      var UnitStore = new PgRelationalStore<Unit>(_db);
      var myBatch = new List<Unit>();
      int qtyToAdd = 10;
      for (int i = 1; i <= qtyToAdd; i++) {
        var newUnit = new Unit { BIN = "OR06-" + i, UnitNo = "D-10" + i };
        myBatch.Add(newUnit);
      }
      UnitStore.Add(myBatch);

      // Re-load, and update:
      var companies = UnitStore.TryLoadData();
      for (int i = 0; i < qtyToAdd; i++) {
        companies.ElementAt(i).UnitNo = "Updated-50" + i;
      }
      UnitStore.Update(companies);

      // Reload, and check updated names:
      companies = UnitStore.TryLoadData().Where(c => c.UnitNo.Contains("Updated")).ToList();
      Assert.IsTrue(companies.Count == qtyToAdd);
    }

    [Test()]
    public void Relational_Store_Deletes_record_with_pg_names() {
      var UnitStore = new PgRelationalStore<Unit>(_db);
      var newUnit = new Unit { BIN = "OR07-01", UnitNo = "E-101" };
      UnitStore.Add(newUnit);

      // Load from back-end:
      var companies = UnitStore.TryLoadData();
      int qtyAdded = companies.Count;

      // Delete:
      var foundUnit = companies.FirstOrDefault();
      UnitStore.Delete(foundUnit);

      int remaining = UnitStore.TryLoadData().Count;
      Assert.IsTrue(qtyAdded == 1 && remaining == 0);
    }

    [Test()]
    public void Relational_Store_Deletes_range_of_records_with_pg_names() {
      var UnitStore = new PgRelationalStore<Unit>(_db);
      var myBatch = new List<Unit>();
      int qtyToAdd = 10;
      for (int i = 0; i < qtyToAdd; i++) {
        var newUnit = new Unit { BIN = "OR08-" + i, UnitNo = "F-10" + i };
        myBatch.Add(newUnit);
      }
      UnitStore.Add(myBatch);

      // Re-load from back-end:
      var companies = UnitStore.TryLoadData();
      int qtyAdded = companies.Count;

      // Select 5 for deletion:
      int qtyToDelete = 5;
      var deleteThese = companies.Where(c => c.UnitId <= qtyToDelete);

      // Delete:
      UnitStore.Delete(deleteThese);
      int remaining = UnitStore.TryLoadData().Count;
      Assert.IsTrue(qtyAdded == qtyToAdd && remaining == qtyAdded - qtyToDelete);
    }

    [Test()]
    public void Relational_Store_Deletes_all_records_with_pg_names() {
      var UnitStore = new PgRelationalStore<Unit>(_db);
      var myBatch = new List<Unit>();
      int qtyToAdd = 10;
      for (int i = 0; i < qtyToAdd; i++) {
        var newUnit = new Unit { BIN = "OR09-" + i, UnitNo = "G-10" + i };
        myBatch.Add(newUnit);
      }
      UnitStore.Add(myBatch);

      // Re-load from back-end:
      var companies = UnitStore.TryLoadData();
      int qtyAdded = companies.Count;

      // Delete:
      UnitStore.DeleteAll();
      int remaining = UnitStore.TryLoadData().Count;
      Assert.IsTrue(qtyAdded == qtyToAdd && remaining == 0);
    }
  }
}

