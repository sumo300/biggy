using System.Collections.Generic;
using System.Linq;

namespace Biggy
{
    public interface IBiggyStore<T>
    {
        IList<T> Load();
        void SaveAll(IList<T> items);
        void Clear();     
        T Add(T item);
        IEnumerable<T> Add(IEnumerable<T> items);
    }

    public interface IUpdateableBiggyStore<T> : IBiggyStore<T>
    {
        T Update(T item);
        T Remove(T item);
    }

    public interface IQueryableBiggyStore<T> : IBiggyStore<T>
    {
        IQueryable<T> AsQueryable();
    }
}