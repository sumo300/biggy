using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Biggy;

namespace Biggy.SqlCe.Tests
{
  [Trait("SQL Server Compact List Basic CRUD", "")]
  public class SqlCeList_CRUD
  {
    public string _connectionStringName = "chinook";
    SqlCeStore<Client> _Clients;

    // Runs before every test:
    public SqlCeList_CRUD() {
      // Drops and re-creates table each time:
      this.SetUpClientTable();
      _Clients = new SqlCeStore<Client>(_connectionStringName); //, tableName: "Clients"
    }

    [Fact(DisplayName = "Pulls things dynamically")]
    public void PullsThingsDynamically() {
        var list = new SqlCeStore<dynamic>(_connectionStringName);
        var results = list.Query(@"select Artist.Name AS ArtistName, Track.Name, Track.UnitPrice
                                   from Artist inner join
                                   Album on Artist.ArtistId = Album.ArtistId inner join
                                   Track on Album.AlbumId = Track.AlbumId
                                   where (Artist.Name = @0)", "ac/dc");
        Assert.True(results.Count() > 0);
    }

    [Fact(DisplayName = "Loads Empty Table into memory")]
    public void Loads_Data_Into_Memory() {
      Assert.True(_Clients.Count() == 0);
    }


    [Fact(DisplayName = "Adds a Record")]
    public void Adds_New_Record() {
      // How many to start with?
      int initialCount = _Clients.Count();
      var newClient = new Client() { FirstName = "John", LastName = "Atten", Email = "jatten@example.com" };
      _Clients.Insert(newClient);
      int idToFind = newClient.ClientId;
      _Clients = new SqlCeStore<Client>(_connectionStringName);     // , tableName: "Clients"
      var found = (_Clients as IQueryableBiggyStore<Client>).AsQueryable(). FirstOrDefault(c => c.ClientId == idToFind);
      Assert.True(found.Email == "jatten@example.com" && _Clients.Count() > initialCount);
    }


    [Fact(DisplayName = "Updates a record")]
    public void Updates_Record() {
      var newClient = new Client() { 
        FirstName = "John", 
        LastName = "Atten", 
        Email = "jatten@example.com" };
      _Clients.Insert(newClient);
      int idToFind = newClient.ClientId;
      //_Clients.Reload();
      //var found = _Clients.FirstOrDefault(c => c.ClientId == idToFind);

      //// After insert, no new record should be added:
      //int currentCount = _Clients.Count();
      //found.FirstName = "Jimi";
      //_Clients.Update(found);
      //_Clients.Reload();

      //Assert.True(found.FirstName == "Jimi" && _Clients.Count == currentCount);
    }


    int _qtyInserted = 100;
    [Fact(DisplayName = "Bulk Inserts Records")]
    public void Bulk_Inserts_Records() {
      int initialCount = _Clients.Count();
      var rangeToAdd = new List<Client>();

      for(int i = 0; i < _qtyInserted; i++) {
        var newClient = new Client() { 
          FirstName = string.Format("John{0}", i.ToString()), 
          LastName = "Atten", 
          Email = string.Format("jatten@example{0}.com", i.ToString()) };
        rangeToAdd.Add(newClient);
      }

      var addedClients = _Clients.BulkInsert(rangeToAdd);
      //_Clients.Reload();
      Assert.True(_Clients.All<Client>().Count() == initialCount + _qtyInserted);
    }


    [Fact(DisplayName = "Deletes a record")]
    public void Deletes_Record() {
      var newClient = new Client() {
        FirstName = "John",
        LastName = "Atten",
        Email = "jatten@example.com"
      };
      _Clients.Insert(newClient);
      int idToFind = newClient.ClientId;

      //_Clients.Reload();
      var found = (_Clients as IQueryableBiggyStore<Client>).AsQueryable()
          .FirstOrDefault(c => c.ClientId == idToFind);
      // After insert, no new record should be added:
      int initialCount = _Clients.Count();
      _Clients.Delete(found);
      //_Clients.Reload();
      Assert.True(_Clients.Count() < initialCount);
    }


    [Fact(DisplayName = "Deletes a range of records")]
    public void Deletes_Range() {
      var rangeToAdd = new List<Client>();
      for (int i = 0; i < _qtyInserted; i++) {
        var newClient = new Client() {
          FirstName = string.Format("John{0}", i.ToString()),
          LastName = "Atten",
          Email = string.Format("jatten@example{0}.com", i.ToString())
        };
        rangeToAdd.Add(newClient);
      }

      //int qtyAdded = _Clients.AddRange(rangeToAdd);
      //_Clients.Reload();
      //int initialCount = _Clients.Count;
      //var removeThese = _Clients.Where(c => c.Email.Contains("jatten@"));
      //_Clients.RemoveSet(removeThese);
      //Assert.True(_Clients.Count < initialCount);
    }


    // HELPER METHODS:


    void SetUpClientTable() {
      var th = new TableHelpers(_connectionStringName);
      if (th.TableExists("Clients")) {
          th.DropTable("Clients");
      }
      this.CreateClientsTable(th);
    }


    void CreateClientsTable(TableHelpers helpers) {
      string sql = ""
      + "CREATE TABLE Clients "
      + "(ClientId int IDENTITY(1,1) PRIMARY KEY NOT NULL, "
      + "[LastName] ntext NOT NULL, "
      + "firstName ntext NOT NULL, "
      + "Email ntext NOT NULL)";

      helpers.Execute(sql);
    }

  }
}
