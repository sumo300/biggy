using System;
using System.Collections.Generic;
using System.Linq;
using Biggy.Data.Azure;
using NUnit.Framework;

namespace Tests.Json
{
    [TestFixture()]
    [Category("Azure Store")]
    public class AzureStoreUsingBlobWithStringKey
    {
        [SetUp]
        public void Initialise()
        {
        }

        [Test()]
        public void AzureStore_Inserts_record_with_string_id()
        {
            var InstrumentStore = AzureStoreUsingBlobWithStringKey.GetAzureStore();
            var newInstrument = new InstrumentDocuments { Id = "USA123", Category = "String", Type = "Guitar" };
            InstrumentStore.Add(newInstrument);

            var foundInstrument = InstrumentStore.TryLoadData().FirstOrDefault(i => i.Id == "USA123");
            Assert.IsTrue(foundInstrument != null && foundInstrument.Id == "USA123");
        }

        [Test()]
        public void AzureStore_Inserts_range_of_records_with_string_id()
        {
            var InstrumentStore = AzureStoreUsingBlobWithStringKey.GetAzureStore();
            var myBatch = new List<InstrumentDocuments>();
            int qtyToAdd = 10;
            for (int i = 1; i <= qtyToAdd; i++)
            {
                myBatch.Add(new InstrumentDocuments { Id = "USA #" + i, Category = "String # " + i, Type = "Guitar" });
            }
            InstrumentStore.Add(myBatch);
            var companies = InstrumentStore.TryLoadData();
            Assert.IsTrue(companies.Count == qtyToAdd);
        }

        [Test()]
        public void AzureStore_Updates_record_with_string_id()
        {
            var InstrumentStore = AzureStoreUsingBlobWithStringKey.GetAzureStore();
            var newInstrument = new InstrumentDocuments { Id = "USA456", Category = "String", Type = "Guitar" };
            InstrumentStore.Add(newInstrument);

            // Now go fetch the record again and update:
            string newType = "Banjo";
            var foundInstrument = InstrumentStore.TryLoadData().FirstOrDefault(i => i.Id == "USA456");
            foundInstrument.Type = newType;
            InstrumentStore.Update(foundInstrument);
            Assert.IsTrue(foundInstrument != null && foundInstrument.Type == newType);
        }

        [Test()]
        public void AzureStore_Updates_range_of_records_with_string_id()
        {
            var InstrumentStore = AzureStoreUsingBlobWithStringKey.GetAzureStore();
            var myBatch = new List<InstrumentDocuments>();
            int qtyToAdd = 10;
            for (int i = 1; i <= qtyToAdd; i++)
            {
                myBatch.Add(new InstrumentDocuments { Id = "USA #" + i, Category = "String # " + i, Type = "Guitar" });
            }
            InstrumentStore.Add(myBatch);

            // Re-load, and update:
            var companies = InstrumentStore.TryLoadData();
            for (int i = 0; i < qtyToAdd; i++)
            {
                companies.ElementAt(i).Type = "Banjo " + i;
            }
            InstrumentStore.Update(companies);

            // Reload, and check updated names:
            companies = InstrumentStore.TryLoadData().Where(c => c.Type.StartsWith("Banjo")).ToList();
            Assert.IsTrue(companies.Count == qtyToAdd);
        }

        [Test()]
        public void AzureStore_Deletes_record_with_string_id()
        {
            var InstrumentStore = AzureStoreUsingBlobWithStringKey.GetAzureStore();
            var newInstrument = new InstrumentDocuments { Id = "USA789", Category = "String", Type = "Guitar" };
            InstrumentStore.Add(newInstrument);

            // Load from back-end:
            var companies = InstrumentStore.TryLoadData();
            int qtyAdded = companies.Count;

            // Delete:
            var foundInstrument = companies.FirstOrDefault(i => i.Id == "USA789");
            InstrumentStore.Delete(foundInstrument);

            int remaining = InstrumentStore.TryLoadData().Count;
            Assert.IsTrue(qtyAdded == 1 && remaining == 0);
        }

        [Test()]
        public void AzureStore_Deletes_range_of_records_with_string_id()
        {
            var InstrumentStore = AzureStoreUsingBlobWithStringKey.GetAzureStore();
            var myBatch = new List<InstrumentDocuments>();
            int qtyToAdd = 10;
            for (int i = 0; i < qtyToAdd; i++)
            {
                myBatch.Add(new InstrumentDocuments { Id = "USA #" + i, Category = "String # " + i, Type = "Guitar" });
            }
            InstrumentStore.Add(myBatch);

            // Re-load from back-end:
            var companies = InstrumentStore.TryLoadData();
            int qtyAdded = companies.Count;

            // Select 5 for deletion:
            int qtyToDelete = 5;
            var deleteThese = new List<InstrumentDocuments>();
            for (int i = 0; i < qtyToDelete; i++)
            {
                deleteThese.Add(companies.ElementAt(i));
            }

            // Delete:
            InstrumentStore.Delete(deleteThese);
            int remaining = InstrumentStore.TryLoadData().Count;
            Assert.IsTrue(qtyAdded == qtyToAdd && remaining == qtyAdded - qtyToDelete);
        }

        [Test()]
        public void AzureStore_Deletes_all_records_with_string_id()
        {
            var InstrumentStore = AzureStoreUsingBlobWithStringKey.GetAzureStore();
            var myBatch = new List<InstrumentDocuments>();
            int qtyToAdd = 10;
            for (int i = 0; i < qtyToAdd; i++)
            {
                myBatch.Add(new InstrumentDocuments { Id = "USA #" + i, Category = "String # " + i, Type = "Guitar" });
            }
            InstrumentStore.Add(myBatch);

            // Re-load from back-end:
            var companies = InstrumentStore.TryLoadData();
            int qtyAdded = companies.Count;

            // Delete:
            InstrumentStore.DeleteAll();
            int remaining = InstrumentStore.TryLoadData().Count;
            Assert.IsTrue(qtyAdded == qtyToAdd && remaining == 0);
        }

        private static object GetAzureStore()
        {
            return new AzureStore<InstrumentDocuments>("azure_dev");
        }
    }
}