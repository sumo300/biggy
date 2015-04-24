using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Biggy.Data.Sqlite;
using NUnit.Framework;

namespace Tests.Sqlite {
    [TestFixture()]
    [Category("SQLite Document Store")]
    public class SqliteDocumentStoreWithLongKeyAttribute {
        private SqliteDbCore _db;
        private string _filename = "";
        private const long TestId = (long) int.MaxValue + 1;

        [SetUp]
        public void init() {
            _db = new SqliteDbCore("BiggyTestSQLiteDocuments");
            _filename = _db.DBFilePath;
        }

        [TearDown]
        public void Cleanup() {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            File.Delete(_filename);
        }

        [Test()]
        public void Creates_table_with_long_id() {
            // The Bowling test object has a long field named Id. This will not be matched
            // without an attribute decoration, and horrible plague and pesilence will result. Also,
            // the attribute IsAuto property is set to false, so the field will NOT be a serial key.

            _db.TryDropTable("bowlingdocuments");
            var documentStore = new SqliteDocumentStore<BowlingDocuments>(_db);
            bool exists = _db.TableExists(documentStore.TableName);
            Assert.IsTrue(exists);
        }

        [Test()]
        public void Inserts_record_with_long_id() {
            _db.TryDropTable("bowlingdocuments");
            var documentStore = new SqliteDocumentStore<BowlingDocuments>(_db);
            var newItem = new BowlingDocuments { Id = TestId, Name = "Seabass", Score = "300" };
            documentStore.Add(newItem);

            var foundItem = documentStore.TryLoadData().FirstOrDefault();
            Assert.IsTrue(foundItem != null && foundItem.Id == TestId);
        }

        [Test()]
        public void Inserts_range_of_records_with_long_id() {
            _db.TryDropTable("bowlingdocuments");
            var documentStore = new SqliteDocumentStore<BowlingDocuments>(_db);
            var myBatch = new List<BowlingDocuments>();
            int qtyToAdd = 10;
            for (long i = TestId; i <= TestId + qtyToAdd - 1; i++) {
                myBatch.Add(new BowlingDocuments { Id = i, Name = "Seabass # " + i, Score = (250 + i).ToString()});
            }
            documentStore.Add(myBatch);
            var companies = documentStore.TryLoadData();
            Assert.IsTrue(companies.Count == qtyToAdd);
        }

        [Test()]
        public void Updates_record_with_long_id() {
            _db.TryDropTable("bowlingdocuments");
            var documentStore = new SqliteDocumentStore<BowlingDocuments>(_db);
            var newItem = new BowlingDocuments { Id = TestId, Name = "Seabass", Score = "300" };
            documentStore.Add(newItem);

            // Now go fetch the record again and update:
            string newScore = "250";
            var foundItem = documentStore.TryLoadData().FirstOrDefault();
            foundItem.Score = newScore;
            documentStore.Update(foundItem);
            Assert.IsTrue(foundItem != null && foundItem.Score == newScore);
        }

        [Test()]
        public void Updates_range_of_records_with_long_id() {
            _db.TryDropTable("bowlingdocuments");
            var documentStore = new SqliteDocumentStore<BowlingDocuments>(_db);
            var myBatch = new List<BowlingDocuments>();
            int qtyToAdd = 10;
            for (long i = TestId; i <= TestId + qtyToAdd - 1; i++) {
                myBatch.Add(new BowlingDocuments { Id = i, Name = "Seabass # " + i, Score = (250 + i).ToString() });
            }
            documentStore.Add(myBatch);

            // Re-load, and update:
            var companies = documentStore.TryLoadData();
            for (int i = 0; i < qtyToAdd; i++) {
                companies.ElementAt(i).Score = (200 + i).ToString();
            }
            documentStore.Update(companies);

            // Reload, and check updated names:
            companies = documentStore.TryLoadData().Where(c => c.Score.StartsWith("20")).ToList();
            Assert.IsTrue(companies.Count == qtyToAdd);
        }

        [Test()]
        public void Deletes_record_with_long_id() {
            _db.TryDropTable("bowlingdocuments");
            var documentStore = new SqliteDocumentStore<BowlingDocuments>(_db);
            var newItem = new BowlingDocuments { Id = TestId, Name = "Seabass", Score = "300"};
            documentStore.Add(newItem);

            // Load from back-end:
            var companies = documentStore.TryLoadData();
            int qtyAdded = companies.Count;

            // Delete:
            var foundItem = companies.FirstOrDefault();
            documentStore.Delete(foundItem);

            int remaining = documentStore.TryLoadData().Count;
            Assert.IsTrue(qtyAdded == 1 && remaining == 0);
        }

        [Test()]
        public void Deletes_range_of_records_with_long_id() {
            _db.TryDropTable("bowlingdocuments");
            var documentStore = new SqliteDocumentStore<BowlingDocuments>(_db);
            var myBatch = new List<BowlingDocuments>();
            int qtyToAdd = 10;
            for (long i = TestId; i < TestId + qtyToAdd; i++) {
                myBatch.Add(new BowlingDocuments { Id = i, Name = "Seabass # " + i, Score = (250 + i).ToString() });
            }
            documentStore.Add(myBatch);

            // Re-load from back-end:
            var companies = documentStore.TryLoadData();
            int qtyAdded = companies.Count;

            // Select 5 for deletion:
            const int qtyToDelete = 5;
            var deleteThese = new List<BowlingDocuments>();
            for (int i = 0; i < qtyToDelete; i++) {
                deleteThese.Add(companies.ElementAt(i));
            }

            // Delete:
            documentStore.Delete(deleteThese);
            int remaining = documentStore.TryLoadData().Count;
            Assert.IsTrue(qtyAdded == qtyToAdd && remaining == qtyAdded - qtyToDelete);
        }

        [Test()]
        public void Deletes_all_records_with_long_id() {
            _db.TryDropTable("bowlingdocuments");
            var documentStore = new SqliteDocumentStore<BowlingDocuments>(_db);
            var myBatch = new List<BowlingDocuments>();
            const int qtyToAdd = 10;
            for (long i = TestId; i < TestId + qtyToAdd; i++) {
                myBatch.Add(new BowlingDocuments { Id = i, Name = "Seabass # " + i, Score = (250 + i).ToString() });
            }
            documentStore.Add(myBatch);

            // Re-load from back-end:
            var companies = documentStore.TryLoadData();
            int qtyAdded = companies.Count;

            // Delete:
            documentStore.DeleteAll();
            int remaining = documentStore.TryLoadData().Count;
            Assert.IsTrue(qtyAdded == qtyToAdd && remaining == 0);
        }
    }
}