using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Biggy;

namespace Biggy.SqlCe.Tests
{
  [Trait("SQL Server Document Store", "")]
  public class SQLServerDocumentStore
  {
    string _connectionStringName = "chinook";
    IBiggyStore<Client> _biggyStore;
    IUpdateableBiggyStore<Client> _updateableStore;
    IQueryableBiggyStore<Client> _queryableStore;
    SqlCeStore<Client> _sqlStore;

    SqlCeDocumentStore<ClientDocument> clientDocs;
    SqlCeDocumentStore<MonkeyDocument> monkeyDocs;

    public SQLServerDocumentStore()
    {
      var context = new SqlCeContext(_connectionStringName);

      // Build a table to play with from scratch each time:
      if(context.TableExists("ClientDocuments")) {
        context.DropTable("ClientDocuments");
      }
      if (context.TableExists("MonkeyDocuments")) {
        context.DropTable("MonkeyDocuments");
      }
      clientDocs = new SqlCeDocumentStore<ClientDocument>(_connectionStringName);
      monkeyDocs = new SqlCeDocumentStore<MonkeyDocument>(_connectionStringName);
    }


    [Fact(DisplayName = "Creates a store with a serial PK if one doesn't exist")]
    public void Creates_Document_Table_With_Serial_PK_If_Not_Present()
    {
      //clientDocs = new SQLDocumentStore<ClientDocument>(_connectionStringName);
      var queryable = clientDocs as IQueryableBiggyStore<ClientDocument>;
      Assert.True(queryable.AsQueryable().Count() == 0);
    }


    [Fact(DisplayName = "Creates a store with a string PK if one doesn't exist")]
    public void Creates_Document_Table_With_String_PK_If_Not_Present()
    {
      //monkeyDocs = new SQLDocumentStore<MonkeyDocument>(_connectionStringName);
      var queryable = monkeyDocs as IQueryableBiggyStore<MonkeyDocument>;
      Assert.True(queryable.AsQueryable().Count() == 0);
    }

    [Fact(DisplayName = "Adds a document with a serial PK")]
    public void Adds_Document_With_Serial_PK()
    {
      var newCustomer = new ClientDocument
      {
        Email = "rob@tekpub.com",
        FirstName = "Rob",
        LastName = "Conery"
      };

      IBiggyStore<ClientDocument> docStore = clientDocs as IBiggyStore<ClientDocument>;
      docStore.Add(newCustomer);
      docStore.Load();
      Assert.Equal(1, docStore.Load().Count());
    }

    [Fact(DisplayName = "Updates a document with a serial PK")]
    public void Updates_Document_With_Serial_PK()
    {
      var newCustomer = new ClientDocument
      {
        Email = "rob@tekpub.com",
        FirstName = "Rob",
        LastName = "Conery"
      };
      var docStore = clientDocs as IUpdateableBiggyStore<ClientDocument>;
      docStore.Add(newCustomer);
      int idToFind = newCustomer.ClientDocumentId;
      // Go find the new record after reloading:

      var updateMe = docStore.Load().FirstOrDefault(cd => cd.ClientDocumentId == idToFind);
      // Update:
      updateMe.FirstName = "Bill";
      docStore.Update(updateMe);
      // Go find the updated record after reloading:
      var updated = docStore.Load().FirstOrDefault(cd => cd.ClientDocumentId == idToFind);
      Assert.True(updated.FirstName == "Bill");
    }


    [Fact(DisplayName = "Deletes a document with a serial PK")]
    public void Deletes_Document_With_Serial_PK()
    {
      var newCustomer = new ClientDocument
      {
        Email = "rob@tekpub.com",
        FirstName = "Rob",
        LastName = "Conery"
      };
      var docStore = clientDocs as IUpdateableBiggyStore<ClientDocument>;
      docStore.Add(newCustomer);
      // Count after adding new:
      int initialCount = docStore.Load().Count();
      var removed = docStore.Remove(newCustomer);
      // Count after removing and reloading:
      int finalCount = docStore.Load().Count();
      Assert.True(finalCount < initialCount);
    }


    [Fact(DisplayName = "Bulk-Inserts new records as JSON documents with string key")]
    public void Bulk_Inserts_Documents_With_String_PK()
    {
      var updateable = monkeyDocs as IUpdateableBiggyStore<MonkeyDocument>;
      int INSERT_QTY = 100;

      var addRange = new List<MonkeyDocument>();
      for (int i = 0; i < INSERT_QTY; i++)
      {
        addRange.Add(new MonkeyDocument { Name = "MONKEY " + i, Birthday = DateTime.Today, Description = "The Monkey on my back" });
      }

      updateable.Add(addRange);
      var inserted = updateable.Load();
      Assert.True(inserted.Count() == INSERT_QTY);
    }

    [Fact(DisplayName = "Bulk-Inserts new records as JSON documents with serial int key")]
    void Bulk_Inserts_Documents_With_Serial_PK() {
      var updateable = clientDocs as IUpdateableBiggyStore<ClientDocument>;
      int INSERT_QTY = 100;
      var bulkList = new List<ClientDocument>();
      for (int i = 0; i < INSERT_QTY; i++) {
        var newClientDocument = new ClientDocument {
          FirstName = "ClientDocument " + i,
          LastName = "Test",
          Email = "jatten@example.com"
        };
        bulkList.Add(newClientDocument);
      }
      updateable.Add(bulkList);

      var inserted = updateable.Load();
      Assert.True(inserted.Count() == INSERT_QTY);
    }

  }
}
