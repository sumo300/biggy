﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Biggy;
using Biggy.SQLServer;

namespace Tests.SQLServer
{
  [Trait("Database Column Mapping", "")]
  public class SQLServerList_Column_Mapping
  {
    // WHAT DOES THIS DO? Runs basic CRUD against a table with non-platform-conformant column names (underscores, spaces, case mismath, etc)

    public string _connectionStringName = "chinook";
    SQLServerList<Client> _clients;

    public SQLServerList_Column_Mapping() {
      // Set up a table with mangled column names:
      this.SetUpWTFTable();
      _clients = new SQLServerList<Client>(_connectionStringName, "WTF", "Client_Id");
	  }


    [Fact(DisplayName = "Test Table Exists")]
    public void Test_Table_Exists() {
      bool exists = this.TableExists("WTF");
      Assert.True(exists);
    }


    [Fact(DisplayName = "Loads Empty List From Table")]
    public void Loads_FromTable_With_Mangled_Column_Names() {
      Assert.True(_clients.Count == 0);
    }


    [Fact(DisplayName = "Adds a New Record")]
    public void Adds_New_Record() {
      int initialCount = _clients.Count;
      var newMonkey = new Client() { 
        LastName = "Jimbo", 
        FirstName = "Jones",
        Email = "jatten@example.com"
      };
      _clients.Add(newMonkey);
      int newID = newMonkey.ClientId;
      _clients = new SQLServerList<Client>(_connectionStringName, "WTF", "Client_Id");
      var found = _clients.FirstOrDefault(c => c.ClientId == newID);
      Assert.True(found.ClientId == newID && _clients.Count > initialCount);
    }


    [Fact(DisplayName = "Updates a Record")]
    public void Updates_Record() {
      var newMonkey = new Client() { 
        LastName = "Jones", 
        FirstName = "Davey",
        Email = "jatten@example.com"
      };
      _clients.Add(newMonkey);
      int currentCount = _clients.Count;
      int newID = newMonkey.ClientId;
      _clients = new SQLServerList<Client>(_connectionStringName, "WTF", "Client_Id");
      var found = _clients.FirstOrDefault(c => c.ClientId == newID);
      found.FirstName = "Mick";
      _clients.Update(found);
      Assert.True(found.ClientId == newID && _clients.Count == currentCount);
    }


    int _qtyInserted = 100;
    [Fact(DisplayName = "Bulk Inserts Records")]
    public void Bulk_Inserts_Records() {
      int initialCount = _clients.Count();
      var rangeToAdd = new List<Client>();
      for(int i = 0; i < _qtyInserted; i++) {
        var newCustomer = new Client() { 
          FirstName = string.Format("John{0}", i.ToString()), 
          LastName = "Atten",
          Email = "jatten@example.com"
        };
        rangeToAdd.Add(newCustomer);
      }
      int qtyAdded = _clients.AddRange(rangeToAdd);
      _clients.Reload();
      Assert.True(_clients.Count == initialCount + _qtyInserted);
    }


    [Fact(DisplayName = "Deletes a record")]
    public void Deletes_Record() {
      var newCustomer = new Client() {
        FirstName = "John",
        LastName = "Atten",
        Email = "jatten@example.com"
      };
      _clients.Add(newCustomer);
      int idToFind = newCustomer.ClientId;

      _clients.Reload();
      var found = _clients.FirstOrDefault(c => c.ClientId == idToFind);
      // After insert, no new record should be added:
      int initialCount = _clients.Count();
      _clients.Remove(found);
      _clients.Reload();
      Assert.True(_clients.Count < initialCount);
    }


    [Fact(DisplayName = "Deletes a range of records by Criteria")]
    public void Deletes_Range_Of_Records() {
      var newClient = new Client() {
        FirstName = "John",
        LastName = "Atten",
        Email = "jatten@example.com"
      };
      _clients.Add(newClient);

      _clients.Reload();
      int initialCount = _clients.Count;
      var removeThese = _clients.Where(c => c.Email.Contains("jatten@"));
      _clients.RemoveSet(removeThese);
      Assert.True(_clients.Count < initialCount);
    }


    // HELPER METHODS:


    void SetUpWTFTable() {
      bool exists = this.TableExists("WTF");
      if (exists) {
        this.DropTable("WTF");
      }
      this.CreateWTFTable();
    }


    void DropTable(string tableName) {
      string sql = string.Format("DROP TABLE {0}", tableName);
      var Model = new SQLServerTable<dynamic>(_connectionStringName);
      Model.Execute(sql);
    }


    bool TableExists(string tableName) {
      bool exists = false;
      string select = ""
          + "SELECT * FROM INFORMATION_SCHEMA.TABLES "
          + "WHERE TABLE_SCHEMA = 'dbo' "
          + "AND  TABLE_NAME = '{0}'";
      string sql = string.Format(select, tableName);
      var Model = new SQLServerTable<dynamic>(_connectionStringName);
      var query = Model.Query<dynamic>(sql);
      if (query.Count() > 0) {
        exists = true;
      }
      return exists;
    }


    void CreateWTFTable() {
      string sql = ""
      + "CREATE TABLE WTF "
      + "(Client_Id int IDENTITY(1,1) PRIMARY KEY NOT NULL, "
      + "[Last Name] Text NOT NULL, "
      + "first_name Text NOT NULL, "
      + "Email Text NOT NULL)";

      var Model = new SQLServerTable<dynamic>(_connectionStringName);
      Model.Execute(sql);
    }

  }

}
