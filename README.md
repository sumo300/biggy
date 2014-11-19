# Biggy: A Very Fast Document/Relational Query Tool with Full LINQ Compliance

*11/17/2014 NOTE: Biggy is in transition. Biggy was originally started by Rob Conery in February of 2014. Over the course of several months we tried on a lot of ideas, and picked up a bunch of fantastic contributors. I had a ton of fun working with Rob on Biggy, and learned a lot. I always enjoy his take on things because his ideas challenge convention, and embrace simplicity.*

*We have been working on some changes in the basic structure of the project the past few months, and some of those changes have now been pushed up. The previous Biggy code has been moved to a branch named `biggyv1' in this repo. There was a lot of great code there, but the project was growing out of control, and developing a bad case of "creeping featuritis."* 

*Branch `master` now reflects a pared-back version with some structural improvements, and will be the main branch going forward. We will be keeping the `biggyv1` branch around, because there is some great stuff there, which may come in handy as we go.* 

*As of 11/17/2014, I am updating this README to reflect where the project is now. I hope to have this finished in the next few days.*

This project started life as an implementation of ICollection<T> that persisted itself to a file using JSON seriliazation. That quickly evolved into using Postgres as a JSON store, and then a host of other data stores. What we ended up with is the fastest data tool you can use.

Data is loaded into memory when your application starts, and you query it with Linq. That's it. It loads incredibly fast (100,000 records in about 1 second) and from there will sync your in-memory list with whatever store you choose. 

Out of the box, Biggy supports [Postgresql](http://www.postgresql.org/) and [SQLite](http://www.sqlite.org/), as well as persistence to simple file-based JSON storage. However, Biggy is designed to be extensible, and you can use the `IDataStore` interface to support any other store you need. 

What Biggy does is represent an ICollection<T> over your datastore. You define your store, and inject the store into the list at initialization.

## File-Based JSON Store

At the simplest level, Biggy can be used against a simple JSON file. The performance of the JSON Store is lickety-split fast.

```csharp
public partial class Artist {
  public int ArtistId { get; set; }
  public string Name { get; set; }
}
  
var store = new JsonStore<Artist>();
var artists = new BiggyList<Artist>(store);

artists.Add(new Artist { ArtistId = 1, Name = "The Wipers" });
artists.Add(new Artist { ArtistId = 2, Name = "The Fastbacks" });

foreach(var artist in artists) { 
  Console.WriteLine("Id: {0} Name: {1}", artist.ArtistId, artist.Name);
}

```

The above code will do a number of things:

 - Creates a file named "artists.json" in a default directory in your project root named "Data."
 - Add each artist to the ICollection implementation `artists` as well as to the file artists.json.
 
If we go to the ../../Data folder, we find artists.json:
 
```js
[{"ArtistId":1,"Name":"The Wipers"},{"ArtistId":2,"Name":"The Fastbacks"}]
```

When Biggy loads the data it deserializes it from the backing store and you can access it just like any ICollection<T>. Similarly, Modifications to the items in the ICollection are persisted back into the backing store. 

## Complex Documents

Above we saw a couple very simple documents stored as a JSON file. We could just as easily work with a more complex object:

```csharp
public class ArtistDocument {
  public ArtistDocument() {
    this.Albums = new List<Album>();
  }
  public int ArtistDocumentId { get; set; }
  public string Name { get; set; }
  public List<Album> Albums;
}
  
public partial class Album {
  public Album() {
    //this.Tracks = new HashSet<Track>();
  }
  public int AlbumId { get; set; }
  public string Title { get; set; }
  public int ArtistId { get; set; }
}
```

Here, we might use another JSON store with our BiggyList:
```csharp
  //this will create a Data/artistdocuments.json file in your project/site root:
  var newArtist = new ArtistDocument { ArtistDocumentId = 3, Name = "Nirvana" };
  newArtist.Albums.Add(new Album { AlbumId = 1, ArtistId = 3, Title = "Bleach" });
  newArtist.Albums.Add(new Album { AlbumId = 2, ArtistId = 3, Title = "Incesticide" });

```
Here, we created an artist document, and nested a couple of albums in the `Albums` property. This is persisted to our JSON store in a file named artistdocuments.json like so:

```js
[{"Albums":[{"AlbumId":1,"Title":"Bleach","ArtistId":3},
{"AlbumId":2,"Title":"Incesticide","ArtistId":3}],
"ArtistDocumentId":3,"Name":"Nirvana"}]
```

You can query using LINQ:

```csharp

// This immediately load any existing artist documents from the json file:
var artists = new BiggyList<ArtistDocument>(store);

// This query never hits the disk - it uses LINQ directly in memory:
var someArtist = artists.FirstOrDefault(a => a.Name == "Nirvana");
var someAlbum = someArtist.Albums.FirstOrDefault(a => a.Title.Contains("Incest"));

// Update:
someAlbum.Title = "In Utero";

//this writes to disk in a single flush - so it's fast too
artists.Update(someArtist);
```

## Relational Database Engines
If your needs grow beyond storage to a flat JSON file, you can easily use SQLite or Posgres as a backing store for both document structures and standard relational table data. 

If we use `pgDocumentStore` or `sqliteDocumentstore`, our objects are serialilzed into JSON and stored in the `body` field of a record. 

if we use `pgRelationalStore` or `sqliteRelationalStore`, data is read/written to the source table as we would expect. 

Our primary relational store of choice is, and has been, Postgresql. Not only is Postgres an amzing database, it has a JSON datatype right out of the box, which lends itself easily to the document storage aspects of Biggy. 

In between the flat JSON file and a full-blown Posgres install is SQLite. SQLite is a file-based local relational storate option offering some really nice performance characteristics. 

Best part for both, they are FREE, cross-platform, and open source. 

## SQLite
Using SQLite with Biggy is almost as simple as using a flat JSON file:

```csharp
      // This will create a new SQLite database file named "TestDb.db in ../../Data
      // if a database file b y that name doesn;t already exist:
      var store = new sqliteDocumentStore<ArtistDocument>("TestDb");

      // This will create a table named artistdocuments in TestDb if one doesn't already exist:
      var artistDocs = new BiggyList<ArtistDocument>(store);

      var newArtist = new ArtistDocument { ArtistDocumentId = 1, Name = "Metallica" };
      newArtist.Albums.Add(new Album { AlbumId = 1, ArtistId = newArtist.ArtistDocumentId, Title = "Kill 'Em All" });
      newArtist.Albums.Add(new Album { AlbumId = 2, ArtistId = newArtist.ArtistDocumentId, Title = "Ride the Lightning" });

      // This will add a record to the artistdocuments table:
      artistDocs.Add(newArtist);
```

The code above will create a SQLite database file named `TestDb.db` in our <Project Root>/Data directory if a db by that name does not already exist, and then also create a table named artistdocuments (again, if one doesn't already exist). 

## Get All Relational With It...
Biggy works with relational data too. When we want to read/write standard relational data, we use the RelationalStore implementation instead of DocumentStore. 

We could pull down the SQLite version of [Chinook Database](http://chinookdatabase.codeplex.com/), drop it in our ../../Data directory, and work with some ready-to-use sample data to do some fancy querying with LINQ:

```csharp
var artistStore = new sqliteRelationalStore<Artist>("Chinook");
var albumStore = new sqliteRelationalStore<Album>("Chinook");

// Loads all data from the Artist table into memory:
var artists = new BiggyList<Artist>(artistStore);

// Loads all data from the Album table into memory:
var albums = new BiggyList<Album>(albumStore);

// Find all the albums by a particular artist using LINQ:
var artistAlbums = from a in albums
                    join ar in artists on a.ArtistId equals ar.ArtistId
                    where ar.Name == "AC/DC"
                    select a;
```

## Postgresql
All of the above works with Postgres as well, except that Postgres, of course, doesn't store files in your project. 

If we want to use Buiggy with a Posgres store, all we need to do is pass it the name of the connection string as defined in the `App.config` or `Web.config` file in our project.

### Define a connection string in App.config:
```xml
<configuration>
  <startup>
    <supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.0"/>
  </startup>
  <connectionStrings>
    <add name="chinook-pg" connectionString="server=localhost;user id=biggy;password=password;database=chinook"/>
  </connectionStrings>
</configuration>
```
Now, assuming we have the Chinook Database for Postgres defined in our PG database, we can do the following:
```csharp
var artistStore = new pgRelationalStore<Artist>("chinook-pg");
var albumStore = new pgRelationalStore<Album>("chinook-pg");
var trackStore = new pgRelationalStore<Track>("chinook-pg");

// Loads all data from the Artist table into memory:
var artists = new BiggyList<Artist>(artistStore);

// Loads all data from the Album table into memory:
var albums = new BiggyList<Album>(albumStore);

// Loads all data from the Album table into memory:
var tracks = new BiggyList<Track>(trackStore);


// Find all the tracks by a particular artist using LINQ:
var artistTracks = (from t in tracks
                    join al in albums on t.AlbumId equals al.AlbumId
                    join ar in artists on al.ArtistId equals ar.ArtistId
                    where ar.Name == "Black Sabbath"
                    select t).ToList();
```
## Cache Schema Info

Of course, the idea behind Biggy is to load data in memory once, and then read/write away as your application needs. The only time Biggy hits the disk is during Writes, which makes things extremely fast (the tripe-joined LINQ query above returns in a less than 5 milliseconds). 

Biggy works against standard relational tables by reading and caching schema info. Instead of doign this three times like we did above, we can use the DBCore object, and load all that once, during initialization. Then we can inject each store into our separate BiggyLists, and all reference the same schema info:

```csharp
// Loads and caches connection and schema info needed for all tables and columns
var _db = new pgDbCore("chinook-pg");

var artists = new BiggyList<Artist>(_db.CreateRelationalStoreFor<Artist>());
var albums = new BiggyList<Album>(_db.CreateRelationalStoreFor<Album>());
var tracks = new BiggyList<Track>(_db.CreateRelationalStoreFor<Track>());


// Find all the tracks by a particular artist using LINQ:
var artistTracks = (from t in tracks
                    join al in albums on t.AlbumId equals al.AlbumId
                    join ar in artists on al.ArtistId equals ar.ArtistId
                    where ar.Name == "Black Sabbath"
                    select t).ToList();
```

...To Be Continued...

## What It's Good For

A document-centric, "NoSQL"-style of development is great for high-read, quick changing things. Products, Customers, Promotions and Coupons - these things get read from the database continually and it's sort of silly. Querying in-memory makes perfect sense for this use case. For these you could use one of the document storage ideas above.could 

A relational, write-oriented transactional situation is great for "slowly changing over time" records - like Orders, Invoices, SecurityLogs, etc. For this you could use a regular relational table using the PGTable or SQLServerTable as you see fit.

## Strategies




## A Note on Speed and Memory

Some applications have a ton of data and for that, Biggy might not be the best fit if you need to read from that ton of data consistently. We've focused on prying apart data into two camps: High Read, and High Write.

We're still solidifying our benchmarks, but in-memory read is about as fast as you can get. Our writes are getting there too - currently we can drop 100,000 documents to disk in about 2 seconds - which isn't so bad. We can write 10,000 records to Postgres and SQL Server in about 500ms - again not bad.

So if you want to log with Biggy - go for it! Just understand that if you use a `DBList<T>`, it assumes you want to read too so it will store the contents in memory as well as on disk. If you don't need this, just use a `DBTable<T>` (Postgres or SQLServer) and write your heart out.

You might also wonder about memory use. Since you're storing everything in memory - for a small web app this might be a concern. Currently the smallest, free sites on Azure allow you 1G RAM. Is this enough space for your data? [Borrowing from Karl Seguin](http://openmymind.net/redis.pdf):

> I do feel that some developers have lost touch with how little space data can take. The Complete Works of William
Shakespeare takes roughly 5.5MB of storage

The entire customer, catalog, logging, and sales history of Tekpub was around 6MB. If you're bumping up against your data limit - just move from an in-memory list to a regular table object (as shown above) and you're good to go.


## Wanna Help?

Please do! Here's what we ask of you:

 - If you've found a bug, please log it in the Issue list. 
 - If you want to fork and fix (thanks!) - please fork then open a branch on your fork specifically for this issue. Give it a nice name.
 - Make the fix and then in your final commit message please use the Github magic syntax ("Closes #X" or Fixes etc) so we can tie your PR to you and your issue
 - Please please please verify your bug or issue with a test (we use XUnit and it's simple to get going)

Thanks so much!


