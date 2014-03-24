using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Biggy.SqlCe.Tests
{
    [Trait("Data manipulation with Sql Ce", "")]
    public class SqlCeTableCRUD {
        public string _connectionStringName = "chinook";
        private string _sampleTableName = "Clowns";

        public SqlCeTableCRUD() {
            var th = new TableHelpers(_connectionStringName);
            if (th.TableExists(_sampleTableName)) {
                th.DropTable(_sampleTableName);
            }
            var sql = string.Format("create table {0} ( {1}Id int not null identity primary key, Name nvarchar(20))",
                    _sampleTableName, _sampleTableName.TrimEnd("s".ToCharArray()));

            th.Execute(sql);
            th.Execute(string.Format("insert into {0}(Name) values (@0)", _sampleTableName), "Biggy");
        }

        [Fact(DisplayName = "Insert row and get its Pk back")]
        public void InsertAndGetId() {
            var clown = new Clown { Name = "clown#1" };
            var db = new SqlCeStore<Clown>(_connectionStringName);

            Assert.Equal(default(int), clown.ClownId);

            // waiting for #60
            var inserted = db.Insert(clown);
            Assert.True(inserted.ClownId > 0);
        }

        [Fact(DisplayName = "Find will find our clown in DB")]
        public void GetClownFromDB() {
            var db = new SqlCeStore<Clown>(_connectionStringName);
            var biggy = db.Find<Clown>(key: 1);

            Assert.NotNull(biggy);
            Assert.Equal("Biggy", biggy.Name);
        }

        [Fact(DisplayName = "Update will change something in DB")]
        public void UpdateClown() {
            string newName = "pnowosie";
            var db = new SqlCeStore<Clown>(_connectionStringName);
            var biggy = db.Find<Clown>(key: 1);

            biggy.Name = newName;
            db.Update(biggy);

            //Assert.NotNull(db.FirstOrDefault<Clown>("Name = @0", newName));
        }

        [Fact(DisplayName = "Delete - it time to say goodbye")]
        public void DeleteClown() {
            var db = new SqlCeStore<Clown>(_connectionStringName);

            //db.Delete(key: 1);
            Assert.Null(db.Find<Clown>(1));
        }

        [Fact(DisplayName = "BulkInsert - list of items can be added at once")]
        public void BulkInsertClowns() {
            int howMany = 20;
            var db = new SqlCeStore<Clown>(_connectionStringName);
            var clowns = Enumerable.Range(1, howMany)
                            .Select(ind => new Clown { Name = "clown#"+ ind })
                            .ToList();
            db.BulkInsert(clowns);
            Assert.Equal(1+howMany, db.Count());
        }


        class Clown
        {
            public int ClownId { get; set; }
            public string Name { get; set; }
        }
    }
}
