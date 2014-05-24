using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Biggy;

namespace Biggy.SqlCe.Tests
{
  [Trait("SQL CE Store", "")]
  public class SqlCeStoreTest {
    string _connectionStringName = "chinook";
    IBiggyStore<Client> _biggyStore;
    SqlCeStore<Client> _sqlStore;

    public SqlCeStoreTest() {
      var context = new SqlCeCache(_connectionStringName);

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
    }
    

    [Fact(DisplayName = "Initializes with Injected Context")]
    public void Intialize_With_Injected_Context() {
      var context = new SqlCeCache(_connectionStringName);
      _sqlStore = new SqlCeStore<Client>(context);
      Assert.True(_sqlStore != null && _sqlStore.Cache.DbTableNames.Count > 0);
    }


    [Fact(DisplayName = "Initializes with Connection String Name")]
    public void Intialize_With_Connection_String_Name() {
      _sqlStore = new SqlCeStore<Client>(_connectionStringName);
      Assert.True(_sqlStore != null && _sqlStore.Cache.DbTableNames.Count > 0);
    }


    [Fact(DisplayName = "IBiggyStore Adds a Record")]
    public void IBiggyStore_Adds_Record() {
      _biggyStore = new SqlCeStore<Client>(_connectionStringName);
      var newClient = new Client() { LastName = "Atten", FirstName = "John", Email = "jatten@example.com" };
      _biggyStore.Add(newClient);
      Assert.True(newClient.ClientId > 0);
    }


    [Fact(DisplayName = "IBiggyStore Adds a Bunch of Records")]
    public void IBiggyStore_Adds_Many_Records() {
      _biggyStore = new SqlCeStore<Client>(_connectionStringName);
      var insertThese = new List<Client>();

      for(int i = 0; i < 10; i++) {
        var newClient = new Client() { LastName = string.Format("LastName {0}", i), FirstName = "John", Email = "jatten@example.com" };
        insertThese.Add(newClient);
      }
      _biggyStore.Add(insertThese);
      var newClients = _biggyStore.Load();
      Assert.True(newClients.Count > 0);
    }


    [Fact(DisplayName = "IUpdateableBiggyStore Adds a Record")]
    public void IUpdateableBiggyStore_Updates_Record() {
      _biggyStore = new SqlCeStore<Client>(_connectionStringName);
      var newClient = new Client() { LastName = "Atten", FirstName = "John", Email = "jatten@example.com" };
      _biggyStore.Add(newClient);

      // Stow the id so we can reload, then update (just to be SURE!!)
      int idToFind = newClient.ClientId;
      newClient = _biggyStore.Load().FirstOrDefault(c => c.ClientId == idToFind);
      newClient.FirstName = "John Paul";
      newClient.LastName = "Jones";
      _biggyStore.Update(newClient);

      var updatedClient = _biggyStore.Load().FirstOrDefault(c => c.ClientId == idToFind);
      Assert.True(updatedClient.LastName == "Jones");
    }


    [Fact(DisplayName = "IUpdateableBiggyStore Deletes a Record")]
    public void IUpdateableBiggyStore_Deletes_Record() {
      _biggyStore = new SqlCeStore<Client>(_connectionStringName);
      var newClient = new Client() { LastName = "Atten", FirstName = "John", Email = "jatten@example.com" };
      _biggyStore.Add(newClient);

      // Stow the id so we can reload, then update (just to be SURE!!)
      int idToFind = newClient.ClientId;
      newClient = _biggyStore.Load().FirstOrDefault(c => c.ClientId == idToFind);

      var deleteMe = _biggyStore.Load().FirstOrDefault(c => c.ClientId == idToFind);
      _biggyStore.Remove(deleteMe);
      var clients = _biggyStore.Load();
      Assert.True(clients.Count == 0);
    }


    [Fact(DisplayName = "IUpdateableBiggyStore Deletes a Bunch of Records")]
    public void IUpdateableBiggyStore_Deletes_Many_Records() {
      _biggyStore = new SqlCeStore<Client>(_connectionStringName);
      var insertThese = new List<Client>();

      for (int i = 0; i < 10; i++) {
        var newClient = new Client() { LastName = string.Format("LastName {0}", i), FirstName = "John", Email = "jatten@example.com" };
        insertThese.Add(newClient);
      }
      _biggyStore.Add(insertThese);
      var newClients = _biggyStore.Load();

      _biggyStore.Remove(newClients);
      newClients = _biggyStore.Load();
      Assert.True(newClients.Count == 0);
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



  }
}
