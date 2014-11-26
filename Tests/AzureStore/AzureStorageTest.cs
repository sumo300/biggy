using System;
using System.Collections.Generic;
using System.Linq;
using Biggy.Data.Azure;
using NUnit.Framework;

namespace Tests.AzureStore
{
    [TestFixture]
    public class AzureStorageTest
    {
        private string azureStorageConnection;

        [Test]
        public void Azure_Connect_To_Storage_And_Save()
        {
            // Given
            var azureCore = new AzureBlobCore("azure_dev");
            var exmapleItmes = AzureStorageTest.GetExampleItems();

            // When
            azureCore.SaveAll(exmapleItmes);

            // Then
            Assert.Fail();
        }

        [SetUp]
        public void Initialise()
        {
        }

        private static List<InstrumentDocuments> GetExampleItems()
        {
            int quantityToAdd = 10;
            var items = new List<InstrumentDocuments>(quantityToAdd);

            for (int i = 0; i < quantityToAdd; i++)
            {
                items.Add(new InstrumentDocuments
                {
                    Id = "USA #" + i,
                    Category = "String # " + i,
                    Type = "Guitar"
                });
            }

            return items;
        }
    }
}