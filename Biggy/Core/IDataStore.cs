using System;
using System.Collections.Generic;

namespace Biggy.Core
{
    public interface IDataStore<T>
        where T : new()
    {
        int Add(IEnumerable<T> items);

        int Add(T item);

        int Delete(IEnumerable<T> items);

        int Delete(T item);

        int DeleteAll();

        List<T> TryLoadData();

        int Update(System.Collections.Generic.IEnumerable<T> items);

        int Update(T item);
    }
}