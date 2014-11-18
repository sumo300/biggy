using NUnit.Framework;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using Biggy.Sqlite;

namespace Tests {
  [TestFixture()]
  [Category("SQLite Document Store")]
  public class sqliteDocumentStoreWithAutoIntKey {

    sqliteDbCore _db;

    [SetUp]
    public void init() {
      _db = new sqliteDbCore("BiggyTest");
    }

    [Test()]
    public void Creates_table_with_serial_id() {

      // The Company test object has an int field named simply "Id". Without any 
      // Attribute decoration, this should result in a table with a serial int PK. 

      // NOTE: Gotta go look to see if the field is a serial in or not...
      //var db = new CommandRunner("chinook");

      _db.TryDropTable("companydocuments");
      var companyStore = new sqliteDocumentStore<CompanyDocuments>(_db);
      bool exists = _db.TableExists(companyStore.TableName);
      Assert.IsTrue(exists);
    }

    [Test()]
    public void Inserts_record_with_serial_id() {
      _db.TryDropTable("companydocuments");
      var companyStore = new sqliteDocumentStore<CompanyDocuments>(_db);
      var newCompany = new CompanyDocuments { Name = "John's Coal Mining Supplies", Address = "16 Company Parkway, Portland, OR 97204" };
      companyStore.Add(newCompany);

      var foundCompany = companyStore.TryLoadData().FirstOrDefault();
      Assert.IsTrue(foundCompany != null && foundCompany.Id == 1);
    }

    [Test()]
    public void Inserts_range_of_records_with_serial_id() {
      _db.TryDropTable("companydocuments");
      var companyStore = new sqliteDocumentStore<CompanyDocuments>(_db);
      var myBatch = new List<CompanyDocuments>();
      int qtyToAdd = 10;
      for (int i = 1; i <= qtyToAdd; i++) {
        myBatch.Add(new CompanyDocuments { Name = "Company Store #" + i, Address = i + " Company Parkway, Portland, OR 97204" });
      }
      companyStore.Add(myBatch);
      var companies = companyStore.TryLoadData();
      Assert.IsTrue(companies.Count == qtyToAdd);
    }

    [Test()]
    public void Updates_record_with_serial_id() {
      _db.TryDropTable("companydocuments");
      var companyStore = new sqliteDocumentStore<CompanyDocuments>(_db);
      var newCompany = new CompanyDocuments { Name = "John's Coal Mining Supplies", Address = "16 Company Parkway, Portland, OR 97204" };
      companyStore.Add(newCompany);

      // Now go fetch the record again and update:
      string newName = "John's Guitars";
      var foundCompany = companyStore.TryLoadData().FirstOrDefault();
      foundCompany.Name = newName;
      companyStore.Update(foundCompany);
      Assert.IsTrue(foundCompany != null && foundCompany.Name == newName);
    }

    [Test()]
    public void Updates_range_of_records_with_serial_id() {
      _db.TryDropTable("companydocuments");
      var companyStore = new sqliteDocumentStore<CompanyDocuments>(_db);
      var myBatch = new List<CompanyDocuments>();
      int qtyToAdd = 10;
      for (int i = 1; i <= qtyToAdd; i++) {
        myBatch.Add(new CompanyDocuments { Name = "Company Store #" + i, Address = i + " Company Parkway, Portland, OR 97204" });
      }
      companyStore.Add(myBatch);

      // Re-load, and update:
      var companies = companyStore.TryLoadData();
      for (int i = 0; i < qtyToAdd; i++) {
        companies.ElementAt(i).Name = "Guitar Store # " + i;
      }
      companyStore.Update(companies);

      // Reload, and check updated names:
      companies = companyStore.TryLoadData().Where(c => c.Name.StartsWith("Guitar Store")).ToList();
      Assert.IsTrue(companies.Count == qtyToAdd);
    }

    [Test()]
    public void Deletes_record_with_serial_id() {
      _db.TryDropTable("companydocuments");
      var companyStore = new sqliteDocumentStore<CompanyDocuments>(_db);
      var newCompany = new CompanyDocuments { Name = "John's Coal Mining Supplies", Address = "16 Company Parkway, Portland, OR 97204" };
      companyStore.Add(newCompany);

      // Load from back-end:
      var companies = companyStore.TryLoadData();
      int qtyAdded = companies.Count;

      // Delete:
      var foundCompany = companies.FirstOrDefault();
      companyStore.Delete(foundCompany);

      int remaining = companyStore.TryLoadData().Count;
      Assert.IsTrue(qtyAdded == 1 && remaining == 0);
    }

    [Test()]
    public void Deletes_range_of_records_with_serial_id() {
      _db.TryDropTable("companydocuments");
      var companyStore = new sqliteDocumentStore<CompanyDocuments>(_db);
      var myBatch = new List<CompanyDocuments>();
      int qtyToAdd = 10;
      for (int i = 0; i < qtyToAdd; i++) {
        myBatch.Add(new CompanyDocuments { Name = "Company Store #" + i, Address = i + " Company Parkway, Portland, OR 97204" });
      }
      companyStore.Add(myBatch);

      // Re-load from back-end:
      var companies = companyStore.TryLoadData();
      int qtyAdded = companies.Count;

      // Select 5 for deletion:
      int qtyToDelete = 5;
      var deleteThese = companies.Where(c => c.Id <= qtyToDelete);

      // Delete:
      companyStore.Delete(deleteThese);
      int remaining = companyStore.TryLoadData().Count;
      Assert.IsTrue(qtyAdded == qtyToAdd && remaining == qtyAdded - qtyToDelete);
    }

    [Test()]
    public void Deletes_all_records_with_serial_id() {
      _db.TryDropTable("companydocuments");
      var companyStore = new sqliteDocumentStore<CompanyDocuments>(_db);
      var myBatch = new List<CompanyDocuments>();
      int qtyToAdd = 10;
      for (int i = 0; i < qtyToAdd; i++) {
        myBatch.Add(new CompanyDocuments { Name = "Company Store #" + i, Address = i + " Company Parkway, Portland, OR 97204" });
      }
      companyStore.Add(myBatch);

      // Re-load from back-end:
      var companies = companyStore.TryLoadData();
      int qtyAdded = companies.Count;
      
      // Delete:
      companyStore.DeleteAll();
      int remaining = companyStore.TryLoadData().Count;
      Assert.IsTrue(qtyAdded == qtyToAdd && remaining == 0);
    }
  }
}

