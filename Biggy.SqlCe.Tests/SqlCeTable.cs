using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Biggy.SqlCe;
using Xunit;

namespace Biggy.SqlCe.Tests
{
    [Trait("SQL Server Compact DBTable CRUD", "")]
    public class SqlCeTable {
        public string _connectionStringName = "chinook";

        public SqlCeTable() {
            var exePath = new Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).LocalPath;
            var binPath = Path.GetDirectoryName(exePath);
            var dir = new DirectoryInfo(Path.Combine(binPath, @"..\..\App_Data"));

            // following doesn't work :( and we have to provide full path in connStr
            //AppDomain.CurrentDomain.SetData("DataDirectory", dir.FullName);
        }

        [Fact(DisplayName = "Fetches row count")]
        public void SimpleSelectStatement() {
            var artistTable = new SqlCeStore<Artist>(_connectionStringName);    //, "Artist"

            int cnt = artistTable.Count();
            Assert.Equal(275, cnt);
        }

        [Fact(DisplayName = "All data from table")]
        public void GetAllRows() {
            var artistTable = new SqlCeStore<Employee>(_connectionStringName);  //, "Employee"

            var employees = artistTable.All<Employee>();
            Assert.Equal(8, employees.Count());
        }

        [Fact(DisplayName = "Get rows with limit")]
        public void GetRowsLimited() {
            var artistTable = new SqlCeStore<Album>(_connectionStringName); //, "Album"

            var albums = artistTable.All<Album>(limit: 10);
            Assert.Equal(10, albums.Count());
        }

        [Fact(DisplayName = "Get rows with order")]
        public void GetRowsOrdered() {
            var employeeTable = new SqlCeStore<Employee>(_connectionStringName);    //, "Employee"

            var employees = employeeTable.All<Employee>(orderBy: "HireDate");
            Assert.Equal("Jane", employees.First().FirstName);

        }

        [Fact(DisplayName = "Fetch query result")]
        public void ComplexQueryTest() {
            var sql = @"select Artist.Name AS ArtistName, Track.Name, Track.UnitPrice
                        from Artist inner join
                        Album on Artist.ArtistId = Album.ArtistId inner join
                        Track on Album.AlbumId = Track.AlbumId
                        where (Artist.Name = @0)";

            var db = new SqlCeStore<dynamic>(_connectionStringName);
            var result = db.Query<dynamic>(sql, "ac/dc").ToList();

            Assert.True(result.Count > 0);
        }

        [Fact(DisplayName = "Gets table name from type name")]
        public void GetTableFromTypeName() {
            var db = new SqlCeStore<Album>(_connectionStringName);
           // Assert.Equal(typeof(Album).Name, db.TableName);
        }

        [Fact(DisplayName = "Should return first found or default")]
        public void FirstOrDefault() {
            var db = new SqlCeStore<Artist>(_connectionStringName); //, "Artist"
            //var artist = db.AsQueryable().FirstOrDefault<Artist>("name = @0", "ac/dc");
            Artist artist = null;

            Assert.NotNull(artist);

            //artist = db.FirstOrDefault<Artist>("name = @0", "there is not such thing");
            Assert.Null(artist);
        }

    }
}
