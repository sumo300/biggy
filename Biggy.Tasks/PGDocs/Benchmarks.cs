using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Biggy.Postgres;

namespace Biggy.Perf.PGDocuments {

  public class ArtistWithAlbums : Artist {
    public ArtistWithAlbums() {
      this.Albums = new List<Album>();
    }
    public List<Album> Albums { get; set; }
  }

  public class Benchmarks {
    public static void Run() {
      var sw = new Stopwatch();
      var _myDatabase = new PGCache("chinookPG");

      Console.WriteLine("===========================================================");
      Console.WriteLine("POSTGRES DOCUMENTS - INSERT A BUNCH OF DOCUMENTS");
      Console.WriteLine("===========================================================");


      // Build a table to play with from scratch each time:
      if(_myDatabase.TableExists("ClientDocuments")) {
        _myDatabase.DropTable("\"ClientDocuments\"");
      }

      IBiggyStore<ClientDocument> clientDocStore = new PGDocumentStore<ClientDocument>(_myDatabase);
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
      Console.WriteLine("POSTGRES DOCUMENTS - SOME FANCY COMPLEX DOCUMENT STUFF");
      Console.WriteLine("===========================================================");

      // Start clean with no existing table:
      if (_myDatabase.TableExists("ArtistWithAlbums")) {
        _myDatabase.DropTable("\"ArtistWithAlbums\"");
      }

      Console.WriteLine("Retreive artists, albums, and tracks from Db...");
      sw.Reset();
      sw.Start();
      IBiggyStore<Artist> _artistStore = new PGStore<Artist>(_myDatabase);
      IBiggyStore<Album> _albumStore = new PGStore<Album>(_myDatabase);
      IBiggyStore<Track> _trackStore = new PGStore<Track>(_myDatabase);

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

      var docStore = new PGDocumentStore<ArtistWithAlbums>(_myDatabase);
      var artistWithAlbumsDocuments = new BiggyList<ArtistWithAlbums>(docStore);
      artistWithAlbumsDocuments.Add(list);

      sw.Stop();
      Console.WriteLine("Added {0} Artist + Album records as complex documents in {1} ms", _artists.Count(), sw.ElapsedMilliseconds);

      Console.WriteLine("Retreive artists and albums from Complex document store and hydrate");

      sw.Reset();
      sw.Start();
      artistWithAlbumsDocuments = new BiggyList<ArtistWithAlbums>(docStore);
      sw.Stop();

      int artistCount = artistWithAlbumsDocuments.Count();
      int albumsCount = 0;
      foreach (var artist in artistWithAlbumsDocuments) {
        albumsCount += artist.Albums.Count();
      }
      Console.WriteLine("\tRetreived and Re-Hydrated {0} Artist + {1} Album records from complex documents in {2} ms",
        artistCount, albumsCount, sw.ElapsedMilliseconds);
    }
  }
}
