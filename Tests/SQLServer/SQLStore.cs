using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Biggy;
using Biggy.SQLServer;

namespace Tests.SQLServer
{
  [Trait("SQL Server Store", "")]
  public class SQLStore {
    string _connectionStringName = "chinook";
    IBiggyStore<Client> _biggyStore;
    IUpdateableBiggyStore<Client> _updateableStore;
    IQueryableBiggyStore<Client> _queryableStore;
    SQLServerStore<Client> _sqlStore;

    public SQLStore() {
      var _cache = new SQLServerCache(_connectionStringName);

      // Build a table to play with from scratch each time:
      if(_cache.TableExists("Client")) {
        _cache.DropTable("Client");
      }
      var columnDefs = new List<string>();
      columnDefs.Add("ClientId int IDENTITY(1,1) PRIMARY KEY NOT NULL");
      columnDefs.Add("LastName Text NOT NULL");
      columnDefs.Add("FirstName Text NOT NULL");
      columnDefs.Add("Email Text NOT NULL");

      _cache.CreateTable("Client", columnDefs);
    }
    

    [Fact(DisplayName = "Initializes with Injected Cache")]
    public void Intialize_With_Injected_Context() {
      var context = new SQLServerCache(_connectionStringName);
      _sqlStore = new SQLServerStore<Client>(context);
      Assert.True(_sqlStore != null && _sqlStore.Cache.DbTableNames.Count > 0);
    }


    [Fact(DisplayName = "Initializes with Connection String Name")]
    public void Intialize_With_Connection_String_Name() {
      _sqlStore = new SQLServerStore<Client>(_connectionStringName);
      Assert.True(_sqlStore != null && _sqlStore.Cache.DbTableNames.Count > 0);
    }


    [Fact(DisplayName = "IBiggyStore Adds a Record")]
    public void IBiggyStore_Adds_Record() {
      _biggyStore = new SQLServerStore<Client>(_connectionStringName);
      var newClient = new Client() { LastName = "Atten", FirstName = "John", Email = "jatten@example.com" };
      _biggyStore.Add(newClient);
      Assert.True(newClient.ClientId > 0);
    }


    [Fact(DisplayName = "IBiggyStore Adds a Bunch of Records")]
    public void IBiggyStore_Adds_Many_Records() {
      _biggyStore = new SQLServerStore<Client>(_connectionStringName);
      var insertThese = new List<Client>();

      for(int i = 0; i < 10; i++) {
        var newClient = new Client() { LastName = string.Format("LastName {0}", i), FirstName = "John", Email = "jatten@example.com" };
        insertThese.Add(newClient);
      }
      _biggyStore.Add(insertThese);
      var newClients = _biggyStore.Load();
      Assert.True(newClients.Count > 0);
    }


    [Fact(DisplayName = "IQueryableBiggyStore Finds a Record")]
    public void IQueryableBiggyStore_Finds_Record() {
      _queryableStore = new SQLServerStore<Client>(_connectionStringName);
      var newClient = new Client() { LastName = "Atten", FirstName = "John", Email = "jatten@example.com" };
      _queryableStore.Add(newClient);
      var foundClient = _queryableStore.AsQueryable().FirstOrDefault(c => c.LastName == "Atten");
      Assert.True(foundClient.LastName == "Atten");
    }


    [Fact(DisplayName = "IUpdateableBiggyStore Adds a Record")]
    public void IUpdateableBiggyStore_Updates_Record() {
      _updateableStore = new SQLServerStore<Client>(_connectionStringName) as IUpdateableBiggyStore<Client>;
      var newClient = new Client() { LastName = "Atten", FirstName = "John", Email = "jatten@example.com" };
      _updateableStore.Add(newClient);

      // Stow the id so we can reload, then update (just to be SURE!!)
      int idToFind = newClient.ClientId;
      newClient = _updateableStore.Load().FirstOrDefault(c => c.ClientId == idToFind);
      newClient.FirstName = "John Paul";
      newClient.LastName = "Jones";
      _updateableStore.Update(newClient);

      var updatedClient = _updateableStore.Load().FirstOrDefault(c => c.ClientId == idToFind);
      Assert.True(updatedClient.LastName == "Jones");
    }


    [Fact(DisplayName = "IUpdateableBiggyStore Deletes a Record")]
    public void IUpdateableBiggyStore_Deletes_Record() {
      _updateableStore = new SQLServerStore<Client>(_connectionStringName) as IUpdateableBiggyStore<Client>;
      var newClient = new Client() { LastName = "Atten", FirstName = "John", Email = "jatten@example.com" };
      _updateableStore.Add(newClient);

      // Stow the id so we can reload, then update (just to be SURE!!)
      int idToFind = newClient.ClientId;
      newClient = _updateableStore.Load().FirstOrDefault(c => c.ClientId == idToFind);

      var deleteMe = _updateableStore.Load().FirstOrDefault(c => c.ClientId == idToFind);
      _updateableStore.Remove(deleteMe);
      var clients = _updateableStore.Load();
      Assert.True(clients.Count == 0);
    }


    [Fact(DisplayName = "IUpdateableBiggyStore Deletes a Bunch of Records")]
    public void IUpdateableBiggyStore_Deletes_Many_Records() {
      _updateableStore = new SQLServerStore<Client>(_connectionStringName) as IUpdateableBiggyStore<Client>;
      var insertThese = new List<Client>();

      for (int i = 0; i < 10; i++) {
        var newClient = new Client() { LastName = string.Format("LastName {0}", i), FirstName = "John", Email = "jatten@example.com" };
        insertThese.Add(newClient);
      }
      _updateableStore.Add(insertThese);
      var newClients = _updateableStore.Load();

      _updateableStore.Remove(newClients);
      newClients = _updateableStore.Load();
      Assert.True(newClients.Count == 0);
    }


    [Fact(DisplayName = "Pulls things dynamically")]
    public void PullsThingsDynamically() {
      var list = new SQLServerStore<dynamic>(_connectionStringName);
      var results = list.Query(@"select Artist.Name AS ArtistName, Track.Name, Track.UnitPrice
                                   from Artist inner join
                                   Album on Artist.ArtistId = Album.ArtistId inner join
                                   Track on Album.AlbumId = Track.AlbumId
                                   where (Artist.Name = @0)", "ac/dc");
      Assert.True(results.Count() > 0);
    }



  }
}
