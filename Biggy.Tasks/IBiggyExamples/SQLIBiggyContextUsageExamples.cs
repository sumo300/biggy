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
  public class MyDb : SQLServerContext
  {
    public IBiggy<Artist> Artists { get; protected set; }
    public IBiggy<Album> Albums { get; protected set; }
    public IBiggy<Track> Tracks { get; protected set; }
    public IBiggy<Customer> Customers { get; protected set; }
    public IBiggy<Client> Clients { get; protected set; }
    public IBiggy<ClientDocument> ClientDocuments { get; protected set; }

    public MyDb() : base("chinook") {
      this.Artists = this.CreateBiggyList<Artist>();
      this.Albums = this.CreateBiggyList<Album>();
      this.Tracks = this.CreateBiggyList<Track>();
      this.Customers = this.CreateBiggyList<Customer>();
      this.Clients = this.CreateBiggyList<Client>();
    }

    public IBiggy<T> CreateBiggyList<T>() where T : new() {
      var newStore = new SQLServerStore<T>(this);
      return this.CreateBiggyList<T>(newStore);
    }

    public IBiggy<T> CreateBiggyList<T>(IBiggyStore<T> store) where T : new() {
      return new BiggyList<T>(store);
    }
  }


  public class SQLIBiggyContextUsageExamples
  {
    public static void Run() {
      // SET-UP: For this exercise, we want a NEW client table each time:
      var temp = new SQLServerContext("chinook");
      if (temp.TableExists("Client")) {
        temp.DropTable("Client");
      }
      var columnDefs = new List<string>();
      columnDefs.Add("ClientId int IDENTITY(1,1) PRIMARY KEY NOT NULL");
      columnDefs.Add("LastName Text NOT NULL");
      columnDefs.Add("FirstName Text NOT NULL");
      columnDefs.Add("Email Text NOT NULL");
      temp.CreateTable("Client", columnDefs);


      MyDb _myDatabase;
      var sw = new Stopwatch();
      Console.WriteLine("===========================================================");
      Console.WriteLine("SQL SERVER - SOME FANCY QUERYING");
      Console.WriteLine("===========================================================");

      Console.WriteLine("Initialize Context...");
      sw.Start();
      _myDatabase = new MyDb();
      sw.Stop();
      Console.WriteLine("\tLoaded initial context with all tables in {0} ms", sw.ElapsedMilliseconds);


      Console.WriteLine("Retreive artists from Memory and write to the console...");
      sw.Start();
      var artists = _myDatabase.Artists.AsQueryable();
      sw.Stop();
      Console.WriteLine("Retrieved {0} Artist records in {1} ms", artists.Count(), sw.ElapsedMilliseconds);


      Console.WriteLine("Loading up Albums from Chinook...");
      sw.Reset();
      sw.Start();


      var albumQuery = from album in _myDatabase.Albums
                       join artist in _myDatabase.Artists
                       on album.ArtistId equals artist.ArtistId
                       select new { artist.Name, album.Title };
      sw.Stop();
      Console.WriteLine("\tLoaded {0} Albums in {1} ms", albumQuery.Count(), sw.ElapsedMilliseconds);


      Console.WriteLine("Grab the record for AC/DC...");
      sw.Reset();
      sw.Start();
      var acdc = _myDatabase.Artists.FirstOrDefault(a => a.Name == "AC/DC");
      sw.Stop();
      Console.WriteLine("\tFound AC/DC from memory in {0} ms", sw.ElapsedMilliseconds);


      Console.WriteLine("Find all the albums by AC/DC ...");
      sw.Reset();
      sw.Start();
      var acdcAlbums = _myDatabase.Albums.Where(a => a.ArtistId == acdc.ArtistId);
      sw.Stop();
      foreach(var album in acdcAlbums) {
        Console.WriteLine("\t" + album.Title);
      }
      Console.WriteLine("\tFound All {0} AC/DC albums from memory in {1} ms", acdcAlbums.Count(), sw.ElapsedMilliseconds);

      Console.WriteLine("Find all the Tracks from Albums by AC/DC ...");
      sw.Reset();
      sw.Start();
      var acdcTracks = from t in _myDatabase.Tracks
                       join a in acdcAlbums on t.AlbumId equals a.AlbumId
                       select t;
      sw.Stop();
      Console.WriteLine("\tFound All {0} tracks by ACDC using in-memory JOIN in {1} ms:", acdcTracks.Count(), sw.ElapsedMilliseconds);
      foreach (var track in acdcTracks) {
        Console.WriteLine("\t-{0}", track.Name);
      }
      Console.WriteLine(Environment.NewLine);
      Console.WriteLine("===========================================================");
      Console.WriteLine("SQL SERVER - BASIC CRUD OPERATIONS");
      Console.WriteLine("===========================================================");

      sw.Reset();
      Console.WriteLine("INSERTING a NEW Customer into Chinook...");
      var newCustomer = new Customer() { LastName = "Atten", FirstName = "John", Email = "xivSolutions@example.com" };
      sw.Start();
      _myDatabase.Customers.Add(newCustomer);
      sw.Stop();
      Console.WriteLine("\tWrote 1 record for a new count of {0} records in {1} ms", _myDatabase.Customers.Count(), sw.ElapsedMilliseconds);

      sw.Reset();
      Console.WriteLine("UPDATING the new Customer record in Chinook...");
      newCustomer.FirstName = "Fred";
      sw.Start();
      _myDatabase.Customers.Update(newCustomer);
      sw.Stop();
      Console.WriteLine("\tUpdated 1 record for a new count of {0} records in {1} ms", _myDatabase.Customers.Count(), sw.ElapsedMilliseconds);

      sw.Reset();
      Console.WriteLine("DELETE the new Customer record in Chinook...");
      sw.Start();
      _myDatabase.Customers.Remove(newCustomer);
      sw.Stop();
      Console.WriteLine("\tDeleted 1 record for a new count of {0} records in {1} ms", _myDatabase.Customers.Count(), sw.ElapsedMilliseconds);


      Console.WriteLine(Environment.NewLine);
      Console.WriteLine("===========================================================");
      Console.WriteLine("SQL SERVER - BULK INSERTS AND DELETIONS");
      Console.WriteLine("===========================================================");

      sw.Reset();
      int INSERT_QTY = 10000;
      Console.WriteLine("BULK INSERTING  {0} client records in Chinook...", INSERT_QTY);

      var inserts = new List<Client>();
      for (int i = 0; i < INSERT_QTY; i++) {
        inserts.Add(new Client() { LastName = string.Format("Atten {0}", i.ToString()), FirstName = "John", Email = "xivSolutions@example.com" });
      }
      sw.Start();
      var inserted = _myDatabase.Clients.Add(inserts);
      sw.Stop();
      Console.WriteLine("\tInserted {0} records in {1} ms", inserted.Count(), sw.ElapsedMilliseconds);

      sw.Reset();
      Console.WriteLine("Loading up Bulk inserted CLients from Chinook...");
      sw.Start();
      var clientsQueryable = _myDatabase.Clients.AsQueryable();
      sw.Stop();
      Console.WriteLine("\tLoaded {0} records in {1}ms", clientsQueryable.Count(), sw.ElapsedMilliseconds);


      sw.Reset();
      Console.WriteLine("DELETING added records from Chinook...");
      var toRemove = _myDatabase.Clients.Where(x => x.Email == "xivSolutions@example.com");
      sw.Start();
      var removed = _myDatabase.Clients.Remove(toRemove.ToList());
      sw.Stop();
      Console.WriteLine("\tDeleted {0} records in {1}ms", removed.Count(), sw.ElapsedMilliseconds);
    }
  }
}
