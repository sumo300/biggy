using System;
using System.Linq;

namespace Biggy.Core.Interfaces
{
    public interface ISynchronisedDataStore<T> : IDataStore<T>
        where T : new()
    {
        void UpdateFromStore();

        void UpdateStore();
    }
}