using System;
using System.Collections.Generic;
using System.Linq;
using Biggy.Core;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace Biggy.Data.Json
{
    public class AzureStore<T> : IDataStore<T> where T : new()
    {
        private readonly CloudStorageAccount storageAccount;
        private readonly CloudTableClient tableAccount;

        public AzureStore(string connectionString)
        {
            this.storageAccount = CloudStorageAccount.Parse(connectionString);
            this.tableAccount = this.storageAccount.CreateCloudTableClient();
        }

        public int Add(T item)
        {
            throw new NotImplementedException();
        }

        public int Add(IEnumerable<T> items)
        {
            throw new NotImplementedException();
        }

        public int Delete(T item)
        {
            throw new NotImplementedException();
        }

        public int Delete(IEnumerable<T> items)
        {
            throw new NotImplementedException();
        }

        public int DeleteAll()
        {
            throw new NotImplementedException();
        }

        public List<T> TryLoadData()
        {
            throw new NotImplementedException();
        }

        public int Update(T item)
        {
            throw new NotImplementedException();
        }

        public int Update(IEnumerable<T> items)
        {
            throw new NotImplementedException();
        }
    }
}