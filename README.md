# Biggy: A Very Fast Document/Relational Query Tool with Full LINQ Compliance

*11/17/2014 NOTE: Biggy is in transition. We have been working on some changes in the basic structure of the project the past few months, and some of those changes have now been pushed up. The previous Biggy code has been moved to a branch named `biggyv1' in this repo. There was a lot of great code there, but the project was growing out of control, and developing a bad case of "creeping featuritis."* 

*Branch `master` now reflects a pared-back version with some structural improvements, and will be the main branch going forward. We will be keeping the `biggyv1` branch around, because there is some great stuff there, which may come in handy as we go.* 

*As of 11/17/2014, I am updating this README to reflect where the project is now. I hope to have this finished in the next few days.*

This project started life as an implementation of ICollection<T> that persisted itself to a file using JSON seriliazation. That quickly evolved into using Postgres as a JSON store, and then SQL Server. What we ended up with is the fastest data tool you can use.

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

## Documents

Above we saw a couple very simple documents stored as a JSON file. We could just as easily work with a more complex object:

```csharp
  public class ArtistDocument {
    public ArtistDocument() {
      this.Albums = new List<AlbumDocument>();
    }
    public int ArtistDocumentId { get; set; }
    public string Name { get; set; }
    public List<AlbumDocument> Albums;
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
If your needs grow beyond storage to a flat JSON file, you can easily use SQLite or Posgres as a backing store for both document structures and standard relational table data:

.... Add updated Relational DB Discussion ....


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


