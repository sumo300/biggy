using System;
using System.Collections.Generic;
using System.Linq;
using Biggy.Core;

namespace Biggy.Data
{
    public sealed class InMemoryStore<T> : IDataStore<T>
        where T : new()
    {
        private readonly List<T> items;

        public InMemoryStore()
        {
            this.items = new List<T>();
        }

        public int Add(T item)
        {
            this.items.Add(item);

            return 1;
        }

        public int Add(IEnumerable<T> items)
        {
            this.items.AddRange(items);

            return 1;
        }

        public int Delete(T item)
        {
            if (this.items.Any(x => x.Equals(item)))
            {
                this.items.Remove(item);
            }
            return 1;
        }

        public int Delete(IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                this.Delete(item);
            }

            return 1;
        }

        public int DeleteAll()
        {
            this.items.Clear();

            return 1;
        }

        public List<T> TryLoadData()
        {
            return this.items;
        }

        public int Update(T item)
        {
            this.Delete(item);
            return this.Add(item);
        }

        public int Update(IEnumerable<T> items)
        {
            this.Delete(items);
            return this.Add(items);
        }
    }
}