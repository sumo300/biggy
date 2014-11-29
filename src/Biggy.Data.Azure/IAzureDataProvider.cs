using System;
using System.Collections.Generic;
using System.Linq;
using Biggy.Core;

namespace Biggy.Data.Azure
{
    internal interface IAzureDataProvider
    {
        IDataStore<T> CreateStoreFor<T>()
            where T : new();

        IEnumerable<T> GetAll<T>();

        void SaveAll<T>(IEnumerable<T> items);
    }
}