using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Biggy;

namespace Biggy.SqlCe.Tests
{
  [Trait("SQL Server Store", "")]
  public class SqlCeBulkInsertsTest {
    string _connectionStringName = "chinook";
    IBiggyStore<Client> _biggyStore;
    SqlCeDocumentStore<ClientDocument> _clientDocs;
    SqlCeStore<Client> _sqlStore;

    // SqlCe has an open session limit - bulk inserts have to be precisely implemented

    public SqlCeBulkInsertsTest()
    {
      var context = new SqlCeContext(_connectionStringName);

      // Build a table to play with from scratch each time:
      if(context.TableExists("Client")) {
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
    }
    

    [Fact(DisplayName = "IBiggyStore BulkInsert")]
    public void IBiggyStore_Add_A_Lot_Of_Records() {
      _biggyStore = new SqlCeStore<Client>(_connectionStringName);
      int recordsCnt = 500;
      List<Client> clients = new List<Client>(500);
      foreach (var i in Enumerable.Range(0, recordsCnt))
      {
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
    public void IBiggyDocumentStore_Add_A_Lot_Of_Records()
    {
      _clientDocs = new SqlCeDocumentStore<ClientDocument>(_connectionStringName);
      int recordsCnt = 500;
      List<ClientDocument> clients = new List<ClientDocument>(500);
      foreach (var i in Enumerable.Range(0, recordsCnt))
      {
          clients.Add(new ClientDocument {
              Email = "client" + i + "@domain.com",
              FirstName = "client#" + i,
              LastName = "last#" + i
          });
      }

      var result = _clientDocs.BulkInsert(clients);
      // check for Pks
      Assert.Equal(recordsCnt, result.Last().ClientDocumentId);

      var newList = _clientDocs.LoadAll();
      Assert.Equal(recordsCnt, newList.Count);
    }

  }
}
