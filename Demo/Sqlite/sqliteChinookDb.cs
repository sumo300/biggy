using System;
using System.Linq;
using Biggy.Core;

namespace Demo
{
    public class sqliteChinookDb : ChinookDbBase
    {
        private sqliteDbCore _db;

        public IDbCore Database { get { return _db; } }

        public sqliteChinookDb(string connectionstringName, bool dropCreateTables = false)
        {
            _db = new sqliteDbCore(connectionstringName);
            if (dropCreateTables)
            {
                this.DropCreateAll();
            }
            this.LoadData();
        }

        public override IDataStore<T> CreateRelationalStoreFor<T>()
        {
            return _db.CreateRelationalStoreFor<T>();
        }

        public override IDataStore<T> CreateDocumentStoreFor<T>()
        {
            return _db.CreateDocumentStoreFor<T>();
        }

        public override void DropCreateAll()
        {
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
}