using System;
using System.Collections.Generic;
using System.Linq;
using Biggy.Core.Interfaces;

namespace Biggy.Data.Azure
{
    public sealed class AzureStore<T> : ISynchronisedDataStore<T>
        where T : new()
    {
        private readonly IAzureDataProvider dataProvider;

        public AzureStore(string connectionString)
            : this(new AzureBlobCore(connectionString))
        {
        }

        internal AzureStore(IAzureDataProvider prodataProvidervider)
        {
            this.dataProvider = dataProvider;
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

        public void UpdateFromStore()
        {
            throw new NotImplementedException();
        }

        public void UpdateStore()
        {
            throw new NotImplementedException();
        }
    }
}