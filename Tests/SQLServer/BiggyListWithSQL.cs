using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Biggy;
using Biggy.SQLServer;

namespace Tests.SQLServer {

  [Trait("Biggy List with SQL Server", "")]
  public class BiggyListWithSQL
  {
    string _connectionStringName = "chinook";
    DbCache _hostDb;
    IBiggyStore<Client> _clientStore;
    IBiggy<Client> _clients;

    public BiggyListWithSQL() {
      _hostDb = new SQLServerCache("chinook");
      // Build a table to play with from scratch each time:
      if (_hostDb.TableExists("Client")) {
        _hostDb.DropTable("Client");
      }

      var columnDefs = new List<string>();
      columnDefs.Add("ClientId int IDENTITY(1,1) PRIMARY KEY NOT NULL");
      columnDefs.Add("LastName Text NOT NULL");
      columnDefs.Add("FirstName Text NOT NULL");
      columnDefs.Add("Email Text NOT NULL");
      _hostDb.CreateTable("Client", columnDefs);

      _clientStore = new SQLServerStore<Client>(_hostDb);
      _clients = new BiggyList<Client>(_clientStore);
    }


    [Fact(DisplayName = "Loads Empty List")]
    public void Loads_Empty_List() {
      Assert.True(_clients.Count() == 0);
    }


    [Fact(DisplayName = "Adds an Item to the list and Store")]
    public void Adds_Item_To_List_And_Store() {
      // Just in case:
      _clients.Clear();
      var newClient = new Client() { LastName = "Atten", FirstName = "John", Email = "jatten@example.com" };
      _clients.Add(newClient);
      
      // Open a new instance, to see if the item was added to the backing store as well:
      var altClientList = new BiggyList<Client>(_clientStore);
      Assert.True(altClientList.Count() > 0);
    }


    [Fact(DisplayName = "Updates an Item from the list and Store")]
    public void Updates_Item_From_List_And_Store() {
      // Just in case:
      _clients.Clear();
      var newClient = new Client() { LastName = "Atten", FirstName = "John", Email = "jatten@example.com" };
      _clients.Add(newClient);
      _clients = new BiggyList<Client>(_clientStore);
      var updateMe = _clients.FirstOrDefault();
      updateMe.LastName = "Appleseed";
      _clients.Update(updateMe);

      // Open a new instance, to see if the item was added to the backing store as well:
      var altClientList = new BiggyList<Client>(new SQLServerStore<Client>(_connectionStringName));
      var updated = altClientList.FirstOrDefault(c => c.LastName == "Appleseed");
      Assert.True(updated != null && updated.LastName == "Appleseed");
    }


    [Fact(DisplayName = "Removes an Item from the list and Store")]
    public void Removes_Item_From_List_And_Store() {
      // Just in case:
      _clients.Clear();
      var newClient = new Client() { LastName = "Atten", FirstName = "John", Email = "jatten@example.com" };
      _clients.Add(newClient);

      int firstCount = _clients.Count();

      // Open a new instance, to see if the item was added to the backing store as well:
      var altClientList = new BiggyList<Client>(_clientStore);
      var deleteMe = altClientList.FirstOrDefault();
      altClientList.Remove(deleteMe);
      Assert.True(firstCount > 0 && altClientList.Count() == 0);
    }


    [Fact(DisplayName = "Adds Many Items to the List and Store")]
    public void Adds_Many_Items_To_List_And_Store() {
      int INSERT_QTY = 10;
      // Just in case:
      _clients.Clear();
      var addThese = new List<Client>();
      for (int i = 0; i < INSERT_QTY; i++) {
        var newClient = new Client() { LastName = string.Format("LastName {0}", i), FirstName = "John", Email = "jatten@example.com" };
        addThese.Add(newClient);
      }
      _clients.Add(addThese);

      // Open a new instance, to see if the item was added to the backing store as well:
      var altClientList = new BiggyList<Client>(_clientStore);
      Assert.True(altClientList.Count() == INSERT_QTY);
    }


    [Fact(DisplayName = "Removes Many Items from the List and Store")]
    public void Removes_Many_Items_From_List_And_Store() {
      int INSERT_QTY = 10;

      // Just in case:
      _clients.Clear();
      var addThese = new List<Client>();
      for (int i = 0; i < INSERT_QTY; i++) {
        var newClient = new Client() { LastName = string.Format("LastName {0}", i), FirstName = "John", Email = "jatten@example.com" };
        addThese.Add(newClient);
      }
      _clients.Add(addThese);

      // Open a new instance, to see if the item was added to the backing store as well:
      var altClientArray = new BiggyList<Client>(_clientStore).ToArray();

      var removeThese = new List<Client>();
      for (int i = 0; i < 5; i++) {
        removeThese.Add(altClientArray[i]);
      }

      _clients.Remove(removeThese);
      // Reload the list:
      _clients = new BiggyList<Client>(_clientStore);
      Assert.True(_clients.Count() == (INSERT_QTY - removeThese.Count()));
    }


    [Fact(DisplayName = "Clears List List and Store")]
    public void Clears_All_Items_From_List_And_Store() {
      int INSERT_QTY = 10;

      // Just in case:
      _clients.Clear();
      var addThese = new List<Client>();
      for (int i = 0; i < INSERT_QTY; i++) {
        var newClient = new Client() { LastName = string.Format("LastName {0}", i), FirstName = "John", Email = "jatten@example.com" };
        addThese.Add(newClient);
      }
      _clients.Add(addThese);

      // Reload the list:
      _clients = new BiggyList<Client>(_clientStore);
      int loadedCount = _clients.Count();
      _clients.Clear();

      // Reload the list:
      _clients = new BiggyList<Client>(_clientStore);
      int clearedCount = _clients.Count();
      Assert.True(loadedCount == INSERT_QTY && clearedCount == 0);
    }
  }
}
