using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Biggy.Postgres;
using Biggy.Core;
using Demo.Models;

namespace Demo {
  public class pgChinookDb : ChinookDbBase {

    pgDbCore _db;
    public IDbCore Database { get { return _db; } }

    public pgChinookDb(string connectionstringName, bool dropCreateTables = false) {
      _db = new pgDbCore(connectionstringName);
      if (dropCreateTables) {
        this.DropCreateAll();
      }
      this.LoadData();
    }

    public override IDataStore<T> CreateRelationalStoreFor<T>() {
      return _db.CreateRelationalStoreFor<T>();
    }

    public override IDataStore<T> CreateDocumentStoreFor<T>() {
      return _db.CreateDocumentStoreFor<T>();
    }

    public override void DropCreateAll() {
      const string SQL_TRACKS_TABLE = ""
        + "CREATE TABLE track ( track_id SERIAL PRIMARY KEY, album_id INTEGER NOT NULL, name text NOT NULL, composer TEXT );";
      const string SQL_ARTISTS_TABLE = ""
        + "CREATE TABLE artist ( artist_id SERIAL PRIMARY KEY NOT NULL, name text NOT NULL );";
      const string SQL_ALBUMS_TABLE = ""
        + "CREATE TABLE album ( album_id SERIAL PRIMARY KEY NOT NULL, artist_id integer NOT NULL, title text NOT NULL );";

      _db.TryDropTable("artist");
      _db.TryDropTable("album");
      _db.TryDropTable("track");
      _db.TryDropTable("artistdocuments");

      int result = _db.TransactDDL(SQL_ARTISTS_TABLE + SQL_ALBUMS_TABLE + SQL_TRACKS_TABLE);
    }

  }
}
