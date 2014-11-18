using NUnit.Framework;
using System;
using System.Linq;
using System.Collections.Generic;
using Biggy.Postgres;
using Biggy.Core;

namespace Tests {
  [TestFixture()]
  [Category("PG Document Store")]
  public class PgDocumentStoreWithStringKeyAttribute {

    pgDbCore _db;

    [SetUp]
    public void init() {
      _db = new pgDbCore("biggy_test");
    }

    [Test()]
    public void Creates_table_with_string_id() {

      // The Guitar test object has a string field named simply SKU. This will not be matched 
      // without an attribute decoration, and horrible plague and pesilence will result.

      _db.TryDropTable("guitardocuments");
      var guitarstore = new pgDocumentStore<GuitarDocuments>(_db);
      bool exists = _db.TableExists(guitarstore.TableName);
      Assert.IsTrue(exists);
    }

    [Test()]
    public void Throws_if_string_key_attribute_is_auto() {

      // The ErrorGuitar test object is built to fail. It has a string key, with attribute indicating 
      // IsAuto = true. 

      _db.TryDropTable("errorguitardocuments");
      Assert.Throws<Exception>(new TestDelegate(TryCreateWithAutoStringKey));
    }

    public void TryCreateWithAutoStringKey() {
      var guitarstore = new pgDocumentStore<ErrorGuitarDocuments>(_db);
    }

    [Test()]
    public void Inserts_record_with_string_id() {
      _db.TryDropTable("guitardocuments");
      var guitarstore = new pgDocumentStore<GuitarDocuments>(_db);
      var newGuitar = new GuitarDocuments { Sku = "USA123", Make = "Gibson", Model = "Les Paul Custom"  };
      guitarstore.Add(newGuitar);

      var foundGuitar = guitarstore.TryLoadData().FirstOrDefault();
      Assert.IsTrue(foundGuitar != null && foundGuitar.Sku == "USA123");
    }

    [Test()]
    public void Inserts_range_of_records_with_string_id() {
      _db.TryDropTable("guitardocuments");
      var guitarstore = new pgDocumentStore<GuitarDocuments>(_db);
      var myBatch = new List<GuitarDocuments>();
      int qtyToAdd = 10;
      for (int i = 1; i <= qtyToAdd; i++) {
        myBatch.Add(new GuitarDocuments { Sku = "USA # " + i, Make = "Fender", Model = "Stratocaster" });
      }
      guitarstore.Add(myBatch);
      var companies = guitarstore.TryLoadData();
      Assert.IsTrue(companies.Count == qtyToAdd);
    }

    [Test()]
    public void Updates_record_with_string_id() {
      _db.TryDropTable("guitardocuments");
      var guitarstore = new pgDocumentStore<GuitarDocuments>(_db);
      var newGuitar = new GuitarDocuments { Sku = "USA123", Make = "Gibson", Model = "Les Paul Custom" };
      guitarstore.Add(newGuitar);

      // Now go fetch the record again and update:
      string newModel = "Explorer";
      var foundGuitar = guitarstore.TryLoadData().FirstOrDefault();
      foundGuitar.Model = newModel;
      guitarstore.Update(foundGuitar);
      Assert.IsTrue(foundGuitar != null && foundGuitar.Model == newModel);
    }

    [Test()]
    public void Updates_range_of_records_with_string_id() {
      _db.TryDropTable("guitardocuments");
      var guitarstore = new pgDocumentStore<GuitarDocuments>(_db);
      var myBatch = new List<GuitarDocuments>();
      int qtyToAdd = 10;
      for (int i = 1; i <= qtyToAdd; i++) {
        myBatch.Add(new GuitarDocuments { Sku = "USA # " + i, Make = "Fender", Model = "Stratocaster" });
      }
      guitarstore.Add(myBatch);

      // Re-load, and update:
      var companies = guitarstore.TryLoadData();
      for (int i = 0; i < qtyToAdd; i++) {
        companies.ElementAt(i).Model = "Jaguar " + i;
      }
      guitarstore.Update(companies);

      // Reload, and check updated names:
      companies = guitarstore.TryLoadData().Where(c => c.Model.StartsWith("Jaguar")).ToList();
      Assert.IsTrue(companies.Count == qtyToAdd);
    }

    [Test()]
    public void Deletes_record_with_string_id() {
      _db.TryDropTable("guitardocuments");
      var guitarstore = new pgDocumentStore<GuitarDocuments>(_db);
      var newGuitar = new GuitarDocuments { Sku = "USA123", Make = "Gibson", Model = "Les Paul Custom" };
      guitarstore.Add(newGuitar);

      // Load from back-end:
      var companies = guitarstore.TryLoadData();
      int qtyAdded = companies.Count;

      // Delete:
      var foundGuitar = companies.FirstOrDefault();
      guitarstore.Delete(foundGuitar);

      int remaining = guitarstore.TryLoadData().Count;
      Assert.IsTrue(qtyAdded == 1 && remaining == 0);
    }

    [Test()]
    public void Deletes_range_of_records_with_string_id() {
      _db.TryDropTable("guitardocuments");
      var guitarstore = new pgDocumentStore<GuitarDocuments>(_db);
      var myBatch = new List<GuitarDocuments>();
      int qtyToAdd = 10;
      for (int i = 0; i < qtyToAdd; i++) {
        myBatch.Add(new GuitarDocuments { Sku = "USA # " + i, Make = "Fender", Model = "Stratocaster" });
      }
      guitarstore.Add(myBatch);

      // Re-load from back-end:
      var companies = guitarstore.TryLoadData();
      int qtyAdded = companies.Count;

      // Select 5 for deletion:
      int qtyToDelete = 5;
      var deleteThese = new List<GuitarDocuments>();
      for (int i = 0; i < qtyToDelete; i++) {
        deleteThese.Add(companies.ElementAt(i));
      }

      // Delete:
      guitarstore.Delete(deleteThese);
      int remaining = guitarstore.TryLoadData().Count;
      Assert.IsTrue(qtyAdded == qtyToAdd && remaining == qtyAdded - qtyToDelete);
    }

    [Test()]
    public void Deletes_all_records_with_string_id() {
      _db.TryDropTable("guitardocuments");
      var guitarstore = new pgDocumentStore<GuitarDocuments>(_db);
      var myBatch = new List<GuitarDocuments>();
      int qtyToAdd = 10;
      for (int i = 0; i < qtyToAdd; i++) {
        myBatch.Add(new GuitarDocuments { Sku = "USA # " + i, Make = "Fender", Model = "Stratocaster" });
      }
      guitarstore.Add(myBatch);

      // Re-load from back-end:
      var companies = guitarstore.TryLoadData();
      int qtyAdded = companies.Count;
      
      // Delete:
      guitarstore.DeleteAll();
      int remaining = guitarstore.TryLoadData().Count;
      Assert.IsTrue(qtyAdded == qtyToAdd && remaining == 0);
    }
  }
}

