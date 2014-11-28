using System;
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
        }

        [Test()]
        public void AzureStore_Inserts_range_of_records_with_string_id()
        {
        }

        [Test()]
        public void AzureStore_Updates_record_with_string_id()
        {
        }

        [Test()]
        public void AzureStore_Updates_range_of_records_with_string_id()
        {
        }

        [Test()]
        public void AzureStore_Deletes_record_with_string_id()
        {
        }

        [Test()]
        public void AzureStore_Deletes_range_of_records_with_string_id()
        {
        }

        [Test()]
        public void AzureStore_Deletes_all_records_with_string_id()
        {
        }

        private static object GetAzureStore()
        {
            return new AzureStore<InstrumentDocuments>("azure_dev");
        }
    }
}