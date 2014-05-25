using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using MongoDB.Driver.Linq;

namespace Biggy.Mongo {
  public class MongoStore<T> : IBiggyStore<T>, IQueryableBiggyStore<T> {
    public MongoStore(string host,
                      string database = "biggy",
                      string collection = "list",
                      int port = 27017,
                      string username = null,
                      string password = null) {
      Initialize(host, port, database, collection, username, password);
    }


    public List<T> Load() {
      return _collection.FindAll().ToList();
    }

    public void SaveAll(List<T> items) {
      foreach (var item in items) {
        Update(item);
      }
    }

    public void Clear() {
      _collection.RemoveAll();
    }

    public T Add(T item) {
      _collection.Insert(item);
      return item;
    }

    public IList<T> Add(List<T> items) {
      _collection.InsertBatch(items);
      return items;
    }

    public IQueryable<T> AsQueryable() {
      return _collection.AsQueryable();
    }

    public T Update(T item) {
      _collection.Save(item);
      return item;
    }

    public T Remove(T item) {
      var query = Query.EQ("_id", ((dynamic)item).Id);
      _collection.Remove(query);
      return item;
    }

    public IList<T> Remove(List<T> items) {
      foreach (var item in items) {
        Remove(item);
      }
      return items;
    }

    private void Initialize(string host, int port, string database, string collection, string username, string password) {
      var clientSettings = CreateClientSettings(host, port, database, username, password);
      _client = new MongoClient(clientSettings);
      _server = _client.GetServer();
      _database = _server.GetDatabase(database);
      _collection = _database.GetCollection<T>(collection);
    }

    private static MongoClientSettings CreateClientSettings(string host, int port, string database,
                                                            string username, string password) {
      var clientSettings = new MongoClientSettings();
      clientSettings.Server = new MongoServerAddress(host, port);
      if (!String.IsNullOrEmpty(username) && !String.IsNullOrEmpty(password)) {
        var credential = MongoCredential.CreateMongoCRCredential(database, username, password);
        clientSettings.Credentials = new[] { credential };
      }
      return clientSettings;
    }

    MongoClient _client;
    MongoServer _server;
    MongoDatabase _database;
    MongoCollection<T> _collection;
  }
}