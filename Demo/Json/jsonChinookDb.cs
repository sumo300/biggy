using System;
using System.Linq;
using Biggy.Core;
using Biggy.Data.Json;

namespace Demo {
  public class jsonChinookDb : ChinookDbBase {

    private JsonDbCore _db;
    public JsonDbCore Database { get { return _db; } }

    public jsonChinookDb(bool dropCreateTables = false) {
      _db = new JsonDbCore();
      if (dropCreateTables) {
        this.DropCreateAll();
      }
      this.LoadData();
    }

    public jsonChinookDb(string dbName, bool dropCreateTables = false) {
      _db = new JsonDbCore(dbName);
      if (dropCreateTables) {
        this.DropCreateAll();
      }
      this.LoadData();
    }

    public override IDataStore<T> CreateRelationalStoreFor<T>() {
      return _db.CreateStoreFor<T>();
    }

    public override IDataStore<T> CreateDocumentStoreFor<T>() {
      return _db.CreateStoreFor<T>();
    }

    public override void DropCreateAll() {
      _db.TryDropTable("Artist");
      _db.TryDropTable("Album");
      _db.TryDropTable("Track");
      _db.TryDropTable("artistdocuments");
    }
  }
}