using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Biggy.Postgres;
using Biggy.SQLServer;

namespace Biggy.Perf
{
  public class MyOtherDb : SQLServerContext
  {
    public IBiggy<Artist> Artists { get; protected set; }
    public IBiggy<Album> Albums { get; protected set; }
    public IBiggy<Track> Tracks { get; protected set; }
    public IBiggy<Customer> Customers { get; protected set; }
    public IBiggy<ArtistWithAlbums> ArtistsWithAlbumsDocuments { get; protected set; }

    public MyOtherDb()
      : base("chinook")
    {
      this.Artists = this.CreateBiggyList<Artist>();
      this.Albums = this.CreateBiggyList<Album>();
      this.Tracks = this.CreateBiggyList<Track>();
      this.Customers = this.CreateBiggyList<Customer>();
      this.ArtistsWithAlbumsDocuments = this.CreateBiggyDocumentList<ArtistWithAlbums>();
    }

    public IBiggy<T> CreateBiggyList<T>() where T : new() {
      var newStore = new SQLServerStore<T>(this);
      return this.CreateBiggyList<T>(newStore);
    }

    public IBiggy<T> CreateBiggyDocumentList<T>() where T : new() {
      var newStore = new SQLDocumentStore<T>(this);
      return this.CreateBiggyList<T>(newStore);
    }

    public IBiggy<T> CreateBiggyList<T>(IBiggyStore<T> store) where T : new() {
      return new BiggyList<T>(store);
    }
  }


  public class ArtistWithAlbums : Artist
  {
    public ArtistWithAlbums()
    {
      this.Albums = new List<Album>();
    }
    public List<Album> Albums { get; set; }
  }



  public class SQLIBiggyDocumentUsageExamples
  {
    public static void Run() {

      MyOtherDb _myDatabase;
      var sw = new Stopwatch();
      Console.WriteLine("===========================================================");
      Console.WriteLine("SQL SERVER DOCUMENTS - SOME FANCY COMPLEX DOCUMENT STUFF");
      Console.WriteLine("===========================================================");

      // Start clean with no existing table:
      var temp = new SQLServerContext("chinook");
      if(temp.TableExists("ArtistWithAlbums"))
      {
        temp.DropTable("ArtistWithAlbums");
      }

      Console.WriteLine("Initialize Context...");
      sw.Start();
      _myDatabase = new MyOtherDb();
      sw.Stop();
      Console.WriteLine("\tLoaded initial context with all tables in {0} ms", sw.ElapsedMilliseconds);


      Console.WriteLine("Retreive artists and albums from Memory and write to a complex document store...");
      sw.Start();
      var artists = _myDatabase.Artists;
      var albums = _myDatabase.Albums;

      var list = new List<ArtistWithAlbums>();
      foreach(var artist in artists)
      {
        var artistAlbums = from a in albums
                           where a.ArtistId == artist.ArtistId
                           select a;
        var newArtistWithAlbums = new ArtistWithAlbums()
        {
          ArtistId = artist.ArtistId,
          Name = artist.Name,
          Albums = artistAlbums.ToList()
        };
      list.Add(newArtistWithAlbums);
      }

      _myDatabase.ArtistsWithAlbumsDocuments.Add(list);
      sw.Stop();
      Console.WriteLine("\tAdded {0} Artist + Album records as complex documents in {1} ms", artists.Count(), sw.ElapsedMilliseconds);

      Console.WriteLine("Retreive artists and albums from Complex document store and hydrate");

      // Re-hydrate the store, just to be sure:
      _myDatabase = new MyOtherDb();

      sw.Reset();
      sw.Start();
      foreach(var artist in _myDatabase.ArtistsWithAlbumsDocuments)
      {
        Console.WriteLine("\t{0}", artist.Name);
        var albumNames = from a in artist.Albums select a.Title;
        foreach(string name in albumNames)
        {
          string useName = name;
          if(name.Length > 10) {
            useName = name.Remove(10, name.Length - 10);
          }
          Console.WriteLine("\t\t{0} ...", useName);
        }
      }
      sw.Stop();
      Console.WriteLine("\tRetreived and Re-Hydrated/wrote to console {0} Artist + Album records from complex documents in {1} ms", artists.Count(), sw.ElapsedMilliseconds);

    }
  }
}
