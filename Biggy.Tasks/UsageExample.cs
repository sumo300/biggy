using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Biggy.SQLServer;

namespace Biggy.Perf {

  // Create a Factory Wrapper Class, consume from within app:
  public class SQLBiggyFactory {
    DbCache _cache;

    public SQLBiggyFactory(string connectionStringName) {
      _cache = new SQLServerCache(connectionStringName);
    }

    public IBiggyStore<T> CreateStoreFor<T>() where T : new() {
      return new SQLServerStore<T>(_cache);
    }

    public IBiggy<T> CreateBiggyListFor<T>() where T : new() {
      return new BiggyList<T>(this.CreateStoreFor<T>());
    }
  }

  public static class UsageExampleOne {

    public static void Run() {

      // Consume Factory Wrapper:
      var _myDb = new SQLBiggyFactory("chinook");
      var artists = _myDb.CreateBiggyListFor<Artist>();
      var albums = _myDb.CreateBiggyListFor<Album>();
      var tracks = _myDb.CreateBiggyListFor<Track>();

      foreach (var artist in artists) {
        Console.WriteLine(artist.Name);
      }
    }
  }


  // Create a Database wrapper (a "Context") specific to your app:

  public class SQLDatabase : SQLBiggyFactory {

    public IBiggy<Artist> Artists { get; set; }
    public IBiggy<Album> Albums { get; set; }
    public IBiggy<Track> Tracks { get; set; }

    public SQLDatabase(string connectionStringName) : base(connectionStringName) {
      this.Artists = this.CreateBiggyListFor<Artist>();
      this.Albums = this.CreateBiggyListFor<Album>();
      this.Tracks = this.CreateBiggyListFor<Track>();
    }
  }


  public static class UsageExampleTwo {

    public static void Run() {
      SQLDatabase _myDb = new SQLDatabase("chinook");

      foreach (var artist in _myDb.Artists) {
        Console.WriteLine(artist.Name);
      }
    }
  }
}
