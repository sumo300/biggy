using System;
using System.Diagnostics;
using System.Linq;

namespace Demo
{
    public class jsonPocoDemo
    {
        private jsonChinookDb _testDb;
        private jsonChinookDb _chinookDb;

        private string _testDbName = "JsonTestData";
        private string _chinookDbName = "ChinookData";

        public void Run()
        {
            Console.WriteLine("JSON Relational Demo - TEST DATA");
            Console.WriteLine("====================================");
            Console.WriteLine("Initialize Test Db");

            var sw = new Stopwatch();

            sw.Start();
            _testDb = new jsonChinookDb(_testDbName, dropCreateTables: true);
            sw.Stop();
            Console.WriteLine("Initialized and reset JSON database in {0} MS", sw.ElapsedMilliseconds);

            Console.WriteLine("Write some test data...");
            var sampleArtists = SampleData.GetSampleArtists(qty: 1000);
            var sampleAlbums = SampleData.GetSampleAlbums(qtyPerArtist: 5);
            var sampleTracks = SampleData.GetSampleTracks(qtyPerAlbum: 8);

            sw.Reset();
            sw.Start();
            _testDb.Artists.Add(sampleArtists);
            sw.Stop();
            Console.WriteLine("Wrote {0} artist records in {1} ms", sampleArtists.Count, sw.ElapsedMilliseconds);

            sw.Reset();
            sw.Start();
            _testDb.Albums.Add(sampleAlbums);
            sw.Stop();
            Console.WriteLine("Wrote {0} album records in {1} ms", sampleAlbums.Count, sw.ElapsedMilliseconds);

            sw.Reset();
            sw.Start();
            _testDb.Tracks.Add(sampleTracks);
            sw.Stop();
            Console.WriteLine("Wrote {0} track records in {1} ms", sampleTracks.Count, sw.ElapsedMilliseconds);

            Console.WriteLine("");
            Console.WriteLine("Re-Initialize Db and read all that data from back-end...");

            sw.Reset();
            sw.Start();
            _testDb.LoadData();
            sw.Stop();
            Console.WriteLine("Read all data from store in {0} ms", sw.ElapsedMilliseconds);
            Console.WriteLine("{0} Artists", _testDb.Artists.Count);
            Console.WriteLine("{0} Albums", _testDb.Albums.Count);
            Console.WriteLine("{0} Tracks", _testDb.Tracks.Count);

            Console.WriteLine("");
            Console.WriteLine("JSON Relational Demo - CHINOOK DATA");
            Console.WriteLine("=======================================");

            Console.WriteLine("Now let's use some actual data from Chinook Db...");

            sw.Reset();
            sw.Start();
            _chinookDb = new jsonChinookDb(_chinookDbName);
            sw.Stop();
            Console.WriteLine("Initialized Chinook data in {0} ms - loaded:", sw.ElapsedMilliseconds);
            Console.WriteLine("{0} Artists", _chinookDb.Artists.Count);
            Console.WriteLine("{0} Albums", _chinookDb.Albums.Count);
            Console.WriteLine("{0} Tracks", _chinookDb.Tracks.Count);

            Console.WriteLine("");
            Console.WriteLine("Some fancy Querying - CHINOOK DATA");
            Console.WriteLine("==================================");

            Console.WriteLine("");
            Console.WriteLine("Find all albums by a particular Artist using LINQ join");
            string artistToFind = "Metallica";

            sw.Reset();
            sw.Start();
            var artistAlbums = (from a in _chinookDb.Albums
                                join ar in _chinookDb.Artists on a.ArtistId equals ar.ArtistId
                                where ar.Name == artistToFind
                                select a).ToList();
            // We use .ToList() because the objects aren't enumerated until used - we want a rough perf measurement to fetch the objects.
            sw.Stop();

            Console.WriteLine("\tArtist: {0}:", artistToFind);
            foreach (var album in artistAlbums)
            {
                Console.WriteLine("\t  -{0}", album.Title);
            }
            Console.WriteLine("Found {0} albums for {1} out of {2} in {3} ms", artistAlbums.Count(), artistToFind, _chinookDb.Albums.Count, sw.ElapsedMilliseconds);

            Console.WriteLine("");
            Console.WriteLine("Find all tracks by a particular Artist using triple LINQ join");
            artistToFind = "AC/DC";
            sw.Reset();
            sw.Start();
            var artistTracks = (from t in _chinookDb.Tracks
                                join al in _chinookDb.Albums on t.AlbumId equals al.AlbumId
                                join ar in _chinookDb.Artists on al.ArtistId equals ar.ArtistId
                                where ar.Name == artistToFind
                                select t).ToList();

            Console.WriteLine("\tArtist: {0}:", artistToFind);
            foreach (var track in artistTracks)
            {
                Console.WriteLine("\t  -{0}", track.Name);
            }
            Console.WriteLine("Found {0} tracks for {1} from {2} albums among {3} total tracks in {4} ms", artistTracks.Count(), artistToFind, _chinookDb.Albums.Count, _chinookDb.Tracks.Count, sw.ElapsedMilliseconds);
        }
    }
}