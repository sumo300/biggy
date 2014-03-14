using System;
using System.Collections.Generic;
using System.Linq;

namespace Biggy
{
    public interface IBiggy<T> : IEnumerable<T>
    {
        void Clear();
        int Count();
        T Update(T item);
        T Remove(T item);
        T Add(T item);
        IList<T> Add(IList<T> items);
        IQueryable<T> AsQueryable();

        event EventHandler<BiggyEventArgs<T>> ItemRemoved;
        event EventHandler<BiggyEventArgs<T>> ItemAdded;
        event EventHandler<BiggyEventArgs<T>> Changed;
        event EventHandler<BiggyEventArgs<T>> Loaded;
        event EventHandler<BiggyEventArgs<T>> Saved;
    }
}