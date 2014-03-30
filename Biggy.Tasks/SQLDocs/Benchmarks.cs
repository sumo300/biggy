using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Biggy.Postgres;
using Biggy.SQLServer;

namespace Biggy.Perf.SQLDocument {
  //public class MyOtherDb : SQLServerHost
  //{
  //  public IBiggy<Artist> Artists { get; protected set; }
  //  public IBiggy<Album> Albums { get; protected set; }
  //  public IBiggy<Track> Tracks { get; protected set; }
  //  public IBiggy<Customer> Customers { get; protected set; }
  //  public IBiggy<ArtistWithAlbums> ArtistsWithAlbumsDocuments { get; protected set; }

  //  public MyOtherDb()
  //    : base("chinook")
  //  {
  //    this.Artists = this.CreateBiggyList<Artist>();
  //    this.Albums = this.CreateBiggyList<Album>();
  //    this.Tracks = this.CreateBiggyList<Track>();
  //    this.Customers = this.CreateBiggyList<Customer>();
  //    this.ArtistsWithAlbumsDocuments = this.CreateBiggyDocumentList<ArtistWithAlbums>();
  //  }

  //  public IBiggy<T> CreateBiggyList<T>() where T : new() {
  //    var newStore = new SQLServerStore<T>(this);
  //    return this.CreateBiggyList<T>(newStore);
  //  }

  //  public IBiggy<T> CreateBiggyDocumentList<T>() where T : new() {
  //    var newStore = new SQLDocumentStore<T>(this);
  //    return this.CreateBiggyList<T>(newStore);
  //  }

  //  public IBiggy<T> CreateBiggyList<T>(IBiggyStore<T> store) where T : new() {
  //    return new BiggyList<T>(store);
  //  }
  //}


  public class ArtistWithAlbums : Artist {
    public ArtistWithAlbums() {
      this.Albums = new List<Album>();
    }
    public List<Album> Albums { get; set; }
  }


  public class Benchmarks {
    public static void Run() {
      var sw = new Stopwatch();
      var _myDatabase = new SQLServerHost("chinook");

      Console.WriteLine("===========================================================");
      Console.WriteLine("SQL SERVER DOCUMENTS - INSERT A BUNCH OF DOCUMENTS");
      Console.WriteLine("===========================================================");


      // Build a table to play with from scratch each time:
      if(_myDatabase.TableExists("ClientDocuments")) {
        _myDatabase.DropTable("ClientDocuments");
      }

      IBiggyStore<ClientDocument> clientDocStore = new SQLDocumentStore<ClientDocument>("chinook");
      IBiggy<ClientDocument> clientDocs = new BiggyList<ClientDocument>(clientDocStore);
      int INSERT_MODEST_QTY = 10000;

      Console.WriteLine("Insert {0} records as documents...", INSERT_MODEST_QTY);
      var addThese = new List<ClientDocument>();
      for(int i = 0; i < INSERT_MODEST_QTY; i++)
      {
        addThese.Add(new ClientDocument {
          LastName = "Atten",
          FirstName = "John",
          Email = "jatten@example.com"
        });
      }
      sw.Start();
      clientDocs.Add(addThese);
      sw.Stop();
      Console.WriteLine("Inserted {0} records as documents in {1} ms", INSERT_MODEST_QTY, sw.ElapsedMilliseconds);



      Console.WriteLine("===========================================================");
      Console.WriteLine("SQL SERVER DOCUMENTS - SOME FANCY COMPLEX DOCUMENT STUFF");
      Console.WriteLine("===========================================================");

      // Start clean with no existing table:
      var temp = new SQLServerHost("chinook");
      if (temp.TableExists("ArtistWithAlbums")) {
        temp.DropTable("ArtistWithAlbums");
      }


      Console.WriteLine("Retreive artists, albums, and tracks from Db...");
      sw.Reset();
      sw.Start();
      IBiggyStore<Artist> _artistStore = new SQLServerStore<Artist>(_myDatabase);
      IBiggyStore<Album> _albumStore = new SQLServerStore<Album>(_myDatabase);
      IBiggyStore<Track> _trackStore = new SQLServerStore<Track>(_myDatabase);

      IBiggy<Artist> _artists = new BiggyList<Artist>(_artistStore);
      IBiggy<Album> _albums = new BiggyList<Album>(_albumStore);
      IBiggy<Track> _tracks = new BiggyList<Track>(_trackStore);
      sw.Stop();

      Console.WriteLine("Query each artists albums and write to complex document store...");

      var list = new List<ArtistWithAlbums>();
      foreach (var artist in _artists) {
        var artistAlbums = from a in _albums
                           where a.ArtistId == artist.ArtistId
                           select a;
        var newArtistWithAlbums = new ArtistWithAlbums() {
          ArtistId = artist.ArtistId,
          Name = artist.Name,
          Albums = artistAlbums.ToList()
        };
        list.Add(newArtistWithAlbums);
      }

      var docStore = new SQLDocumentStore<ArtistWithAlbums>(_myDatabase);
      var artistWithAlbumsDocuments = new BiggyList<ArtistWithAlbums>(docStore);
      artistWithAlbumsDocuments.Add(list);

      sw.Stop();
      Console.WriteLine("Added {0} Artist + Album records as complex documents in {1} ms", _artists.Count(), sw.ElapsedMilliseconds);

      //Console.WriteLine("Retreive artists and albums from Complex document store and hydrate");

      //// Re-hydrate the store, just to be sure:
      //_myDatabase = new SQLServerHost("chinook");

      //sw.Reset();
      //sw.Start();
      //artistWithAlbumsDocuments = new BiggyList<ArtistWithAlbums>(docStore);
      //foreach (var artist in artistWithAlbumsDocuments) {
      //  Console.WriteLine("\t{0}", artist.Name);
      //  var albumNames = from a in artist.Albums select a.Title;
      //  foreach (string name in albumNames) {
      //    string useName = name;
      //    if (name.Length > 10) {
      //      useName = name.Remove(10, name.Length - 10);
      //    }
      //    Console.WriteLine("\t\t{0} ...", useName);
      //  }
      //}
      //sw.Stop();
      //Console.WriteLine("\tRetreived and Re-Hydrated/wrote to console {0} Artist + Album records from complex documents in {1} ms", _artists.Count(), sw.ElapsedMilliseconds);
    }
  }
}
