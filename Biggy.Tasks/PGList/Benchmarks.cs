using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Biggy.Postgres;

namespace Biggy.Perf.PGList
{
  public class Benchmarks
  {
    static string _connectionStringName = "chinookPG";
    public static void Run()
    {
      var sw = new Stopwatch();
      Console.WriteLine("===========================================================");
      Console.WriteLine("Postgres - SOME FANCY QUERYING");
      Console.WriteLine("===========================================================");


      var _db = new PGCache(_connectionStringName);
      IBiggy<Artist> _artists;
      IBiggy<Album> _albums;
      IBiggy<Track> _tracks;

      Console.WriteLine("Loading up Artists from Chinook...");
      sw.Start();
      _artists = new BiggyList<Artist>(new PGStore<Artist>(_db));
      sw.Stop();
      Console.WriteLine("\tLoaded {0} Artist records in {1} ms", _artists.Count(), sw.ElapsedMilliseconds);


      Console.WriteLine("Loading up Albums from Chinook...");
      sw.Reset();
      sw.Start();
      _albums = new BiggyList<Album>(new PGStore<Album>(_db));
      sw.Stop();
      Console.WriteLine("\tLoaded {0} Albums in {1} ms", _artists.Count(), sw.ElapsedMilliseconds);


      Console.WriteLine("Loading up tracks from Chinook...");
      sw.Reset();
      sw.Start();
      _tracks = new BiggyList<Track>(new PGStore<Track>(_db));
      sw.Stop();
      Console.WriteLine("\tLoaded {0} Tracks in {1} ms", _tracks.Count(), sw.ElapsedMilliseconds);


      Console.WriteLine("Grab the record for AC/DC...");
      sw.Reset();
      sw.Start();
      var acdc = _artists.FirstOrDefault(a => a.Name == "AC/DC");
      sw.Stop();
      Console.WriteLine("\tFound AC/DC from memory in {0} ms", sw.ElapsedMilliseconds);


      Console.WriteLine("Find all the albums by AC/DC ...");
      sw.Reset();
      sw.Start();
      var acdcAlbums = _albums.Where(a => a.ArtistId == acdc.ArtistId);
      sw.Stop();
      Console.WriteLine("\tFound All {0} AC/DC albums from memory in {1} ms", acdcAlbums.Count(), sw.ElapsedMilliseconds);

      Console.WriteLine("Find all the Tracks from Albums by AC/DC ...");
      sw.Reset();
      sw.Start();
      var acdcTracks = from t in _tracks
                       join a in acdcAlbums on t.AlbumId equals a.AlbumId
                       select t;
      sw.Stop();
      Console.WriteLine("\tFound All {0} tracks by ACDC using in-memory JOIN in {1} ms:", acdcTracks.Count(), sw.ElapsedMilliseconds);
      foreach (var track in acdcTracks)
      {
        Console.WriteLine("\t-{0}", track.Name);
      }
      Console.WriteLine(Environment.NewLine);
      Console.WriteLine("===========================================================");
      Console.WriteLine("POSTGES - BASIC CRUD OPERATIONS");
      Console.WriteLine("===========================================================");

      IBiggy<Customer> _customers;


      sw.Reset();
      Console.WriteLine("Loading up customers from Chinook...");
      sw.Start();
      _customers = new BiggyList<Customer>(new PGStore<Customer>(_db));
      sw.Stop();
      Console.WriteLine("\tLoaded {0} records in {1}ms", _customers.Count(), sw.ElapsedMilliseconds);

      sw.Reset();
      Console.WriteLine("INSERTING a NEW Customer into Chinook...");
      var newCustomer = new Customer() { LastName = "Atten", FirstName = "John", Email = "xivSolutions@example.com" };
      sw.Start();
      _customers.Add(newCustomer);
      sw.Stop();
      Console.WriteLine("\tWrote 1 record for a new count of {0} records in {1} ms", _customers.Count(), sw.ElapsedMilliseconds);

      sw.Reset();
      Console.WriteLine("UPDATING the new Customer record in Chinook...");
      newCustomer.FirstName = "Fred";
      sw.Start();
      _customers.Update(newCustomer);
      sw.Stop();
      Console.WriteLine("\tUpdated 1 record for a new count of {0} records in {1} ms", _customers.Count(), sw.ElapsedMilliseconds);

      sw.Reset();
      Console.WriteLine("DELETE the new Customer record in Chinook...");
      sw.Start();
      _customers.Remove(newCustomer);
      sw.Stop();
      Console.WriteLine("\tDeleted 1 record for a new count of {0} records in {1} ms", _customers.Count(), sw.ElapsedMilliseconds);


      Console.WriteLine(Environment.NewLine);
      Console.WriteLine("===========================================================");
      Console.WriteLine("POSTGES - BULK INSERTS AND DELETIONS");
      Console.WriteLine("===========================================================");

      Console.WriteLine("Creating Test Table...");
      if(_db.TableExists("client"))
      {
        _db.DropTable("client");
      }
      var columnDefs = new List<string>();
      columnDefs.Add("client_id serial PRIMARY KEY NOT NULL");
      columnDefs.Add("last_name Text NOT NULL");
      columnDefs.Add("first_name Text NOT NULL");
      columnDefs.Add("email Text NOT NULL");
      _db.CreateTable("client", columnDefs);

      IBiggy<Client> _clients;

      sw.Reset();
      int INSERT_QTY = 10000;
      Console.WriteLine("BULK INSERTING  {0} client records in Chinook...", INSERT_QTY);
      _clients = new BiggyList<Client>(new PGStore<Client>(_db));

      var inserts = new List<Client>();
      for (int i = 0; i < INSERT_QTY; i++)
      {
        inserts.Add(new Client() { LastName = string.Format("Atten {0}", i.ToString()), FirstName = "John", Email = "xivSolutions@example.com" });
      }
      sw.Start();
      var inserted = _clients.Add(inserts);
      sw.Stop();
      Console.WriteLine("\tInserted {0} records in {1} ms", inserted.Count(), sw.ElapsedMilliseconds);

      sw.Reset();
      Console.WriteLine("Loading up Bulk inserted CLients from Chinook...");
      sw.Start();
      _clients = new BiggyList<Client>(new PGStore<Client>(_db));
      sw.Stop();
      Console.WriteLine("\tLoaded {0} records in {1}ms", _clients.Count(), sw.ElapsedMilliseconds);


      sw.Reset();
      Console.WriteLine("DELETING added records from Chinook...");
      var toRemove = _clients.Where(x => x.Email == "xivSolutions@example.com");
      sw.Start();
      var removed = _clients.Remove(toRemove.ToList());
      sw.Stop();
      Console.WriteLine("\tDeleted {0} records in {1}ms", removed.Count(), sw.ElapsedMilliseconds);
    }
  }
}
