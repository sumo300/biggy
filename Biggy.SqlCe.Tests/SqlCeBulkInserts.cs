using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Biggy;

namespace Biggy.SqlCe.Tests {
  [Trait("SQL CE Store", "")]
  public class SqlCeBulkInsertsTest {
    string _connectionStringName = "chinook";
    IBiggyStore<Client> _biggyStore;
    IBiggyStore<ClientDocument> _clientDocs;

    // SqlCe has an open session limit - bulk inserts have to be precisely implemented

    public SqlCeBulkInsertsTest() {
      var context = new SqlCeCache(_connectionStringName);

      // Build a table to play with from scratch each time:
      if (context.TableExists("Client")) {
        context.DropTable("Client");
      }
      var columnDefs = new List<string>();
      columnDefs.Add("ClientId int IDENTITY(1,1) PRIMARY KEY NOT NULL");
      columnDefs.Add("LastName nText NOT NULL");
      columnDefs.Add("FirstName nText NOT NULL");
      columnDefs.Add("Email nText NOT NULL");

      context.CreateTable("Client", columnDefs);

      // Build a table to play with from scratch each time:
      if (context.TableExists("ClientDocuments")) {
        context.DropTable("ClientDocuments");
      }
      if (context.TableExists("MonkeyDocuments")) {
        context.DropTable("MonkeyDocuments");
      }
    }


    [Fact(DisplayName = "IBiggyStore BulkInsert")]
    public void IBiggyStore_Add_A_Lot_Of_Records() {
      _biggyStore = new SqlCeStore<Client>(_connectionStringName);
      int recordsCnt = 500;
      List<Client> clients = new List<Client>(500);
      foreach (var i in Enumerable.Range(0, recordsCnt)) {
        clients.Add(new Client {
          Email = "client" + i + "@domain.com",
          FirstName = "client#" + i,
          LastName = "last#" + i
        });
      }

      var result = _biggyStore.Add(clients);
      // check for Pks
      Assert.Equal(recordsCnt, result.Last().ClientId);

      var newList = _biggyStore.Load();
      Assert.Equal(recordsCnt, newList.Count);
    }


    [Fact(DisplayName = "IBiggyDocumentStore BulkInsert")]
    public void IBiggyDocumentStore_Add_A_Lot_Of_Records() {
      _clientDocs = new SqlCeDocumentStore<ClientDocument>(_connectionStringName);
      int recordsCnt = 500;
      List<ClientDocument> clients = new List<ClientDocument>(500);
      foreach (var i in Enumerable.Range(0, recordsCnt)) {
        clients.Add(new ClientDocument {
          Email = "client" + i + "@domain.com",
          FirstName = "client#" + i,
          LastName = "last#" + i
        });
      }

      var result = _clientDocs.Add(clients);
      // check for Pks
      Assert.Equal(recordsCnt, result.Last().ClientDocumentId);

      var newList = _clientDocs.Load();
      Assert.Equal(recordsCnt, newList.Count);
    }

    [Fact(DisplayName = "Bulk-Inserts new records as JSON documents with string key")]
    public void Bulk_Inserts_Documents_With_String_PK() {
      int INSERT_QTY = 100;

      var addRange = new List<MonkeyDocument>();
      for (int i = 0; i < INSERT_QTY; i++) {
        addRange.Add(new MonkeyDocument { Name = "MONKEY " + i, Birthday = DateTime.Today, Description = "The Monkey on my back" });
      }
      IBiggyStore<MonkeyDocument> monkeyDocuments = new SqlCeDocumentStore<MonkeyDocument>(_connectionStringName);
      var inserted = monkeyDocuments.Add(addRange);

      // Reload, make sure everything was persisted:
      var monkeys = new BiggyList<MonkeyDocument>(new SqlCeDocumentStore<MonkeyDocument>(_connectionStringName));
      Assert.True(monkeys.Count() == INSERT_QTY);
    }

  }
}
