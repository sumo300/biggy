using NUnit.Framework;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using Biggy.Data.Sqlite;
using Biggy.Core;

namespace Tests {
  [TestFixture()]
  [Category("SQLite Document Store")]
  public class sqliteDocumentStoreWithIntKeyAttribute {

    SqliteDbCore _db;

    [SetUp]
    public void init() {
      _db = new SqliteDbCore("BiggyTest");
    }

    [Test()]
    public void Creates_table_with_int_id() {

      // The Widget test object has an int field named Identifier. This will not be matched 
      // without an attribute decoration, and horrible plague and pesilence will result. Also, 
      // the attribute IsAuto property is set to false, so the field will NOT be a serial key. 

      _db.TryDropTable("widgetdocuments");
      var widgetstore = new SqliteDocumentStore<WidgetDocuments>(_db);
      bool exists = _db.TableExists(widgetstore.TableName);
      Assert.IsTrue(exists);
    }


    [Test()]
    public void Inserts_record_with_int_id() {
      _db.TryDropTable("widgetdocuments");
      var widgetstore = new SqliteDocumentStore<WidgetDocuments>(_db);
      var newWidget = new WidgetDocuments { Identifier = 100, Category = "Brass"  };
      widgetstore.Add(newWidget);

      var foundWidget = widgetstore.TryLoadData().FirstOrDefault();
      Assert.IsTrue(foundWidget != null && foundWidget.Identifier == 100);
    }

    [Test()]
    public void Inserts_range_of_records_with_int_id() {
      _db.TryDropTable("widgetdocuments");
      var widgetstore = new SqliteDocumentStore<WidgetDocuments>(_db);
      var myBatch = new List<WidgetDocuments>();
      int qtyToAdd = 10;
      for (int i = 1; i <= qtyToAdd; i++) {
        myBatch.Add(new WidgetDocuments { Identifier = i, Category = "Plastic # " + i });
      }
      widgetstore.Add(myBatch);
      var companies = widgetstore.TryLoadData();
      Assert.IsTrue(companies.Count == qtyToAdd);
    }

    [Test()]
    public void Updates_record_with_int_id() {
      _db.TryDropTable("widgetdocuments");
      var widgetstore = new SqliteDocumentStore<WidgetDocuments>(_db);
      var newWidget = new WidgetDocuments { Identifier = 100, Category = "Brass" };
      widgetstore.Add(newWidget);

      // Now go fetch the record again and update:
      string newCategory = "Gold";
      var foundWidget = widgetstore.TryLoadData().FirstOrDefault();
      foundWidget.Category = newCategory;
      widgetstore.Update(foundWidget);
      Assert.IsTrue(foundWidget != null && foundWidget.Category == newCategory);
    }

    [Test()]
    public void Updates_range_of_records_with_int_id() {
      _db.TryDropTable("widgetdocuments");
      var widgetstore = new SqliteDocumentStore<WidgetDocuments>(_db);
      var myBatch = new List<WidgetDocuments>();
      int qtyToAdd = 10;
      for (int i = 1; i <= qtyToAdd; i++) {
        myBatch.Add(new WidgetDocuments { Identifier = i, Category = "Plastic # " + i });
      }
      widgetstore.Add(myBatch);

      // Re-load, and update:
      var companies = widgetstore.TryLoadData();
      for (int i = 0; i < qtyToAdd; i++) {
        companies.ElementAt(i).Category = "Silver # " + i;
      }
      widgetstore.Update(companies);

      // Reload, and check updated names:
      companies = widgetstore.TryLoadData().Where(c => c.Category.StartsWith("Silver")).ToList();
      Assert.IsTrue(companies.Count == qtyToAdd);
    }

    [Test()]
    public void Deletes_record_with_int_id() {
      _db.TryDropTable("widgetdocuments");
      var widgetstore = new SqliteDocumentStore<WidgetDocuments>(_db);
      var newWidget = new WidgetDocuments { Identifier = 100, Category = "Brass" };
      widgetstore.Add(newWidget);

      // Load from back-end:
      var companies = widgetstore.TryLoadData();
      int qtyAdded = companies.Count;

      // Delete:
      var foundWidget = companies.FirstOrDefault();
      widgetstore.Delete(foundWidget);

      int remaining = widgetstore.TryLoadData().Count;
      Assert.IsTrue(qtyAdded == 1 && remaining == 0);
    }

    [Test()]
    public void Deletes_range_of_records_with_int_id() {
      _db.TryDropTable("widgetdocuments");
      var widgetstore = new SqliteDocumentStore<WidgetDocuments>(_db);
      var myBatch = new List<WidgetDocuments>();
      int qtyToAdd = 10;
      for (int i = 0; i < qtyToAdd; i++) {
        myBatch.Add(new WidgetDocuments { Identifier = i, Category = "Plastic # " + i });
      }
      widgetstore.Add(myBatch);

      // Re-load from back-end:
      var companies = widgetstore.TryLoadData();
      int qtyAdded = companies.Count;

      // Select 5 for deletion:
      int qtyToDelete = 5;
      var deleteThese = new List<WidgetDocuments>();
      for (int i = 0; i < qtyToDelete; i++) {
        deleteThese.Add(companies.ElementAt(i));
      }

      // Delete:
      widgetstore.Delete(deleteThese);
      int remaining = widgetstore.TryLoadData().Count;
      Assert.IsTrue(qtyAdded == qtyToAdd && remaining == qtyAdded - qtyToDelete);
    }

    [Test()]
    public void Deletes_all_records_with_int_id() {
      _db.TryDropTable("widgetdocuments");
      var widgetstore = new SqliteDocumentStore<WidgetDocuments>(_db);
      var myBatch = new List<WidgetDocuments>();
      int qtyToAdd = 10;
      for (int i = 0; i < qtyToAdd; i++) {
        myBatch.Add(new WidgetDocuments { Identifier = i, Category = "Plastic # " + i });
      }
      widgetstore.Add(myBatch);

      // Re-load from back-end:
      var companies = widgetstore.TryLoadData();
      int qtyAdded = companies.Count;
      
      // Delete:
      widgetstore.DeleteAll();
      int remaining = widgetstore.TryLoadData().Count;
      Assert.IsTrue(qtyAdded == qtyToAdd && remaining == 0);
    }
  }
}

