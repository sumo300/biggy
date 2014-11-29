using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Biggy.Core;
using Biggy.Data.Sqlite;
using Demo.Models;
using System.IO;
using System.Security.Cryptography;
using Biggy.Extensions;
using System.Data.SQLite;
using Biggy.Data.Postgres;

namespace Demo {
  public class Playground {

    //JsonDbCore _db = new JsonDbCore("Playground");
    SqliteDbCore _sqlitedb = new SqliteDbCore("testTrans");
    PgDbCore _pgDb = new PgDbCore("biggy_test");


    public void Run() {

      Console.WriteLine("Postgres Perf");
      Console.WriteLine("=============");


      PgDropCreate.DropCreateAll(_pgDb);
      var pgStore = new PgRelationalStore<Artist>(_pgDb);
      var pgMemoryArtists = new List<Artist>();

      var pgNewArtists = new List<Artist>();
      for (int i = 1; i <= 10000; i++) {
        pgNewArtists.Add(new Artist { Name = "New Artist " + i });
      }

      var sw = new System.Diagnostics.Stopwatch();
      sw.Start();
      pgStore.Add(pgNewArtists);
      sw.Stop();
      Console.WriteLine("Added {0} new Artists in {1} ms", pgNewArtists.Count, sw.ElapsedMilliseconds);

      sw.Reset();
      sw.Start();
      var pgAllArtists = pgStore.TryLoadData();
      sw.Stop();
      Console.WriteLine("Read {0} Artists from DB in {1} ms", pgAllArtists.Count, sw.ElapsedMilliseconds);

      foreach (var artist in pgAllArtists) {
        artist.Name = "UPDATED";
      }

      sw.Reset();
      sw.Start();
      pgStore.Update(pgAllArtists);
      sw.Stop();
      Console.WriteLine("Updated {0} new Artists in {1} ms", pgAllArtists.Count, sw.ElapsedMilliseconds);

      sw.Reset();
      sw.Start();
      int pgHowMany = pgStore.Delete(pgAllArtists);
      sw.Stop();
      Console.WriteLine("Delted {0} new Artists in {1} ms", pgHowMany, sw.ElapsedMilliseconds);

      Console.WriteLine("SQLite Perf");
      Console.WriteLine("===========");

      sqliteDropCreate.DropCreateAll(_sqlitedb);
      var sqliteStore = new SqliteRelationalStore<Artist>(_sqlitedb);
      var sqliteMemoryArtists = new List<Artist>();

      var sqliteNewArtists = new List<Artist>();
      for (int i = 1; i <= 10000; i++) {
        sqliteNewArtists.Add(new Artist { Name = "New Artist " + i });
      }

      sw.Reset();
      sw.Start();
      sqliteStore.Add(sqliteNewArtists);
      sw.Stop();
      Console.WriteLine("Added {0} new Artists in {1} ms", sqliteNewArtists.Count, sw.ElapsedMilliseconds);

      sw.Reset();
      sw.Start();
      var sqliteAllArtists = sqliteStore.TryLoadData();
      sw.Stop();
      Console.WriteLine("Read {0} Artists from DB in {1} ms", sqliteAllArtists.Count, sw.ElapsedMilliseconds);

      foreach (var artist in sqliteAllArtists) {
        artist.Name = "UPDATED";
      }

      sw.Reset();
      sw.Start();
      sqliteStore.Update(sqliteAllArtists);
      sw.Stop();
      Console.WriteLine("Updated {0} new Artists in {1} ms", sqliteAllArtists.Count, sw.ElapsedMilliseconds);

      sw.Reset();
      sw.Start();
      int sqliteHowMany = sqliteStore.Delete(sqliteAllArtists);
      sw.Stop();
      Console.WriteLine("Delted {0} new Artists in {1} ms", sqliteHowMany, sw.ElapsedMilliseconds);
    }

    // FOR SQLITE:
    //public void DropCreateAll() {
    //  const string SQL_TRACKS_TABLE = ""
    //    + "CREATE TABLE Track ( TrackId INTEGER PRIMARY KEY AUTOINCREMENT, AlbumId INT NOT NULL, Name text NOT NULL, Composer TEXT );";
    //  const string SQL_ARTISTS_TABLE = ""
    //    + "CREATE TABLE Artist ( ArtistId INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL, Name TEXT NOT NULL );";
    //  const string SQL_ALBUMS_TABLE = ""
    //    + "CREATE TABLE Album ( AlbumId INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL, ArtistId INT NOT NULL, Title text NOT NULL );";

    //  _db.TryDropTable("Artist");
    //  _db.TryDropTable("Album");
    //  _db.TryDropTable("Track");
    //  _db.TryDropTable("artistdocuments");

    //  int result = _db.TransactDDL(SQL_ARTISTS_TABLE + SQL_ALBUMS_TABLE + SQL_TRACKS_TABLE);
    //}


    //// FOR POSTGRES:
    //public void DropCreateAll() {
    //  const string SQL_TRACKS_TABLE = ""
    //    + "CREATE TABLE track ( track_id SERIAL PRIMARY KEY, album_id INTEGER NOT NULL, name text NOT NULL, composer TEXT );";
    //  const string SQL_ARTISTS_TABLE = ""
    //    + "CREATE TABLE artist ( artist_id SERIAL PRIMARY KEY NOT NULL, name text NOT NULL );";
    //  const string SQL_ALBUMS_TABLE = ""
    //    + "CREATE TABLE album ( album_id SERIAL PRIMARY KEY NOT NULL, artist_id integer NOT NULL, title text NOT NULL );";

    //  _db.TryDropTable("artist");
    //  _db.TryDropTable("album");
    //  _db.TryDropTable("track");
    //  _db.TryDropTable("artistdocuments");

    //  int result = _db.TransactDDL(SQL_ARTISTS_TABLE + SQL_ALBUMS_TABLE + SQL_TRACKS_TABLE);
    //}


    void memoryArtists_ItemAdded(object sender, BiggyEventArgs<Artist> e) {
      Console.WriteLine(e.Item.Name);
    }
  }

  public class sqliteDropCreate {
    public static void DropCreateAll(IDbCore _db) {
      const string SQL_TRACKS_TABLE = ""
        + "CREATE TABLE Track ( TrackId INTEGER PRIMARY KEY AUTOINCREMENT, AlbumId INT NOT NULL, Name text NOT NULL, Composer TEXT );";
      const string SQL_ARTISTS_TABLE = ""
        + "CREATE TABLE Artist ( ArtistId INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL, Name TEXT NOT NULL );";
      const string SQL_ALBUMS_TABLE = ""
        + "CREATE TABLE Album ( AlbumId INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL, ArtistId INT NOT NULL, Title text NOT NULL );";

      _db.TryDropTable("Artist");
      _db.TryDropTable("Album");
      _db.TryDropTable("Track");
      _db.TryDropTable("artistdocuments");

      int result = _db.TransactDDL(SQL_ARTISTS_TABLE + SQL_ALBUMS_TABLE + SQL_TRACKS_TABLE);
    }
  }


  public class PgDropCreate {
    public static void DropCreateAll(IDbCore _db) {
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
