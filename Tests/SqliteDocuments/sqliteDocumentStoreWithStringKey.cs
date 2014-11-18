using NUnit.Framework;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using Biggy.Sqlite;
using Biggy.Core;

namespace Tests {
  [TestFixture()]
  [Category("SQLite Document Store")]
  public class sqliteDocumentStoreWithStringKey {

    sqliteDbCore _db;

    [SetUp]
    public void init() {
      _db = new sqliteDbCore("BiggyTest");
    }

    [Test()]
    public void Creates_table_with_string_id() {

      // The Instrument test object has an int field named simply Id. Without any 
      // Attribute decoration, this should result in a table with a serial int PK. 

      // NOTE: Gotta go look to see if the field is a serial in or not...
      //var db = new CommandRunner("chinook");
      var InstrumentStore = new sqliteDocumentStore<InstrumentDocuments>(_db);
      bool exists = _db.TableExists(InstrumentStore.TableName);
      Assert.IsTrue(exists);
    }

    [Test()]
    public void Inserts_record_with_string_id() {
      _db.TryDropTable("instrumentdocuments");
      var InstrumentStore = new sqliteDocumentStore<InstrumentDocuments>(_db);
      var newInstrument = new InstrumentDocuments { Id = "USA123", Category = "String", Type = "Guitar" };
      InstrumentStore.Add(newInstrument);

      var foundInstrument = InstrumentStore.TryLoadData().FirstOrDefault();
      Assert.IsTrue(foundInstrument != null && foundInstrument.Id == "USA123");
    }

    [Test()]
    public void Inserts_range_of_records_with_string_id() {
      _db.TryDropTable("instrumentdocuments");
      var InstrumentStore = new sqliteDocumentStore<InstrumentDocuments>(_db);
      var myBatch = new List<InstrumentDocuments>();
      int qtyToAdd = 10;
      for (int i = 1; i <= qtyToAdd; i++) {
        myBatch.Add(new InstrumentDocuments { Id = "USA #" + i, Category = "String # " + i, Type = "Guitar" });
      }
      InstrumentStore.Add(myBatch);
      var companies = InstrumentStore.TryLoadData();
      Assert.IsTrue(companies.Count == qtyToAdd);
    }

    [Test()]
    public void Updates_record_with_string_id() {
      _db.TryDropTable("instrumentdocuments");
      var InstrumentStore = new sqliteDocumentStore<InstrumentDocuments>(_db);
      var newInstrument = new InstrumentDocuments { Id = "USA123", Category = "String", Type = "Guitar" };
      InstrumentStore.Add(newInstrument);

      // Now go fetch the record again and update:
      string newType = "Banjo";
      var foundInstrument = InstrumentStore.TryLoadData().FirstOrDefault();
      foundInstrument.Type = newType;
      InstrumentStore.Update(foundInstrument);
      Assert.IsTrue(foundInstrument != null && foundInstrument.Type == newType);
    }

    [Test()]
    public void Updates_range_of_records_with_string_id() {
      _db.TryDropTable("instrumentdocuments");
      var InstrumentStore = new sqliteDocumentStore<InstrumentDocuments>(_db);
      var myBatch = new List<InstrumentDocuments>();
      int qtyToAdd = 10;
      for (int i = 1; i <= qtyToAdd; i++) {
        myBatch.Add(new InstrumentDocuments { Id = "USA #" + i, Category = "String # " + i, Type = "Guitar" });
      }
      InstrumentStore.Add(myBatch);

      // Re-load, and update:
      var companies = InstrumentStore.TryLoadData();
      for (int i = 0; i < qtyToAdd; i++) {
        companies.ElementAt(i).Type = "Banjo " + i;
      }
      InstrumentStore.Update(companies);

      // Reload, and check updated names:
      companies = InstrumentStore.TryLoadData().Where(c => c.Type.StartsWith("Banjo")).ToList();
      Assert.IsTrue(companies.Count == qtyToAdd);
    }

    [Test()]
    public void Deletes_record_with_string_id() {
      _db.TryDropTable("instrumentdocuments");
      var InstrumentStore = new sqliteDocumentStore<InstrumentDocuments>(_db);
      var newInstrument = new InstrumentDocuments { Id = "USA123", Category = "String", Type = "Guitar" };
      InstrumentStore.Add(newInstrument);

      // Load from back-end:
      var companies = InstrumentStore.TryLoadData();
      int qtyAdded = companies.Count;

      // Delete:
      var foundInstrument = companies.FirstOrDefault();
      InstrumentStore.Delete(foundInstrument);

      int remaining = InstrumentStore.TryLoadData().Count;
      Assert.IsTrue(qtyAdded == 1 && remaining == 0);
    }

    [Test()]
    public void Deletes_range_of_records_with_string_id() {
      _db.TryDropTable("instrumentdocuments");
      var InstrumentStore = new sqliteDocumentStore<InstrumentDocuments>(_db);
      var myBatch = new List<InstrumentDocuments>();
      int qtyToAdd = 10;
      for (int i = 0; i < qtyToAdd; i++) {
        myBatch.Add(new InstrumentDocuments { Id = "USA #" + i, Category = "String # " + i, Type = "Guitar" });
      }
      InstrumentStore.Add(myBatch);

      // Re-load from back-end:
      var companies = InstrumentStore.TryLoadData();
      int qtyAdded = companies.Count;

      // Select 5 for deletion:
      int qtyToDelete = 5;
      var deleteThese = new List<InstrumentDocuments>();
      for (int i = 0; i < qtyToDelete; i++) {
        deleteThese.Add(companies.ElementAt(i));
      }

      // Delete:
      InstrumentStore.Delete(deleteThese);
      int remaining = InstrumentStore.TryLoadData().Count;
      Assert.IsTrue(qtyAdded == qtyToAdd && remaining == qtyAdded - qtyToDelete);
    }

    [Test()]
    public void Deletes_all_records_with_string_id() {
      _db.TryDropTable("instrumentdocuments");
      var InstrumentStore = new sqliteDocumentStore<InstrumentDocuments>(_db);
      var myBatch = new List<InstrumentDocuments>();
      int qtyToAdd = 10;
      for (int i = 0; i < qtyToAdd; i++) {
        myBatch.Add(new InstrumentDocuments { Id = "USA #" + i, Category = "String # " + i, Type = "Guitar" });
      }
      InstrumentStore.Add(myBatch);

      // Re-load from back-end:
      var companies = InstrumentStore.TryLoadData();
      int qtyAdded = companies.Count;
      
      // Delete:
      InstrumentStore.DeleteAll();
      int remaining = InstrumentStore.TryLoadData().Count;
      Assert.IsTrue(qtyAdded == qtyToAdd && remaining == 0);
    }
  }
}

