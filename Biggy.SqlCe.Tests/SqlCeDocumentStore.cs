using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Biggy;

namespace Biggy.SqlCe.Tests {
  [Trait("SQL CE Document Store", "")]
  public class SqlCeDocumentStoreTest {
    string _connectionStringName = "chinook";
    IBiggyStore<ClientDocument> clientDocs;
    IBiggyStore<MonkeyDocument> monkeyDocs;

    public SqlCeDocumentStoreTest() {
      var context = new SqlCeCache(_connectionStringName);

      // Build a table to play with from scratch each time:
      if (context.TableExists("ClientDocuments")) {
        context.DropTable("ClientDocuments");
      }
      if (context.TableExists("MonkeyDocuments")) {
        context.DropTable("MonkeyDocuments");
      }
      clientDocs = new SqlCeDocumentStore<ClientDocument>(_connectionStringName);
      monkeyDocs = new SqlCeDocumentStore<MonkeyDocument>(_connectionStringName);
    }


    [Fact(DisplayName = "Creates a store with a serial PK if one doesn't exist")]
    public void Creates_Document_Table_With_Serial_PK_If_Not_Present() {
      Assert.True(clientDocs.Load().Count() == 0);
    }


    [Fact(DisplayName = "Creates a store with a string PK if one doesn't exist")]
    public void Creates_Document_Table_With_String_PK_If_Not_Present() {
      Assert.True(monkeyDocs.Load().Count() == 0);
    }

    [Fact(DisplayName = "Adds a document with a serial PK")]
    public void Adds_Document_With_Serial_PK() {
      var newCustomer = new ClientDocument {
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
    public void Updates_Document_With_Serial_PK() {
      var newCustomer = new ClientDocument {
        Email = "rob@tekpub.com",
        FirstName = "Rob",
        LastName = "Conery"
      };
      clientDocs.Add(newCustomer);
      int idToFind = newCustomer.ClientDocumentId;
      // Go find the new record after reloading:

      var updateMe = clientDocs.Load().FirstOrDefault(cd => cd.ClientDocumentId == idToFind);
      // Update:
      updateMe.FirstName = "Bill";
      clientDocs.Update(updateMe);
      // Go find the updated record after reloading:
      var updated = clientDocs.Load().FirstOrDefault(cd => cd.ClientDocumentId == idToFind);
      Assert.True(updated.FirstName == "Bill");
    }


    [Fact(DisplayName = "Deletes a document with a serial PK")]
    public void Deletes_Document_With_Serial_PK() {
      var newCustomer = new ClientDocument {
        Email = "rob@tekpub.com",
        FirstName = "Rob",
        LastName = "Conery"
      };
      clientDocs.Add(newCustomer);
      // Count after adding new:
      int initialCount = clientDocs.Load().Count();
      var removed = clientDocs.Remove(newCustomer);
      // Count after removing and reloading:
      int finalCount = clientDocs.Load().Count();
      Assert.True(finalCount < initialCount);
    }


    [Fact(DisplayName = "Bulk-Inserts new records as JSON documents with string key")]
    public void Bulk_Inserts_Documents_With_String_PK() {
      int INSERT_QTY = 100;

      var addRange = new List<MonkeyDocument>();
      for (int i = 0; i < INSERT_QTY; i++) {
        addRange.Add(new MonkeyDocument { Name = "MONKEY " + i, Birthday = DateTime.Today, Description = "The Monkey on my back" });
      }

      monkeyDocs.Add(addRange);
      var inserted = monkeyDocs.Load();
      Assert.True(inserted.Count() == INSERT_QTY);
    }

    [Fact(DisplayName = "Bulk-Inserts new records as JSON documents with serial int key")]
    void Bulk_Inserts_Documents_With_Serial_PK() {
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
      clientDocs.Add(bulkList);

      var inserted = clientDocs.Load();
      Assert.True(inserted.Count() == INSERT_QTY);
    }

    [Fact(DisplayName = "Non Auto-Incrementing Pk should not be overwritten")]
    void Non_Auto_Incrementing_Pk_should_not_be_overwritten() {
      string monkeyPk = "Monkey#1";
      var monkey = new MonkeyDocument { Name = monkeyPk, Birthday = DateTime.Now };

      monkeyDocs.Add(monkey);
      Assert.Equal(monkeyPk, monkeyDocs.Load().Single().Name);
    }

    [Fact(DisplayName = "Removing list of documents from db")]
    void Removing_list_of_documents() {
        var startDate = new DateTime(1980, 1, 1);
        var addRange = new List<ClientDocument>();
        int i = 0;
        while (++i < 30) {
            addRange.Add(new ClientDocument {
                FirstName = "Client"+i,
                Email = string.Concat("client", i, "@host.email")
            });
        }
        clientDocs.Add(addRange);

        var clients = addRange.Skip(10).Take(9).ToList();
        clientDocs.Remove(clients);

        clients = clientDocs.Load();
        Assert.Equal(20, clients.Count);
    }

    [Fact(DisplayName = "Removing list of documents from db, non-autoincrement Pk")]
    void Removing_list_of_documents_no_auto_Pk() {
      var startDate = new DateTime(1980, 1, 1);
      var addRange = new List<MonkeyDocument>();
      int i=0;
      while (++i < 30) {
        addRange.Add(new MonkeyDocument {
          Name = "Monkey"+i.ToString(),
          Birthday = startDate.AddYears(i)
        });
      }
      monkeyDocs.Add(addRange);

      var monkeys = addRange.Where(m => m.Birthday.Year > 1988 && m.Birthday.Year < 1999).ToList();
      monkeyDocs.Remove(monkeys);

      monkeys = monkeyDocs.Load();
      Assert.False(monkeys.Any(m => m.Birthday.Year > 1988 && m.Birthday.Year < 1999));
    }

  }
}
