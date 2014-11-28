using System;
using System.Collections.Generic;
using System.Linq;
using Biggy.Core;

namespace Biggy.Data.Azure
{
    public sealed class AzureStore<T> : IDataStore<T>
        where T : new()
    {
        private readonly IAzureDataProvider dataProvider;
        private List<T> items;

        public AzureStore(string connectionString)
            : this(new AzureBlobCore(connectionString))
        {
        }

        internal AzureStore(IAzureDataProvider prodataProvidervider)
        {
            this.dataProvider = dataProvider;
            this.items = this.TryLoadData();
        }

        public int Add(T item)
        {
            this.items.Add(item);

            this.SynchroniseWithStore();

            return 1;
        }

        public int Add(IEnumerable<T> items)
        {
            this.items.AddRange(items);

            this.SynchroniseWithStore();

            return items.Count();
        }

        public int Delete(T item)
        {
            this.DeleteItem(item);

            this.SynchroniseWithStore();

            return 1;
        }

        public int Delete(IEnumerable<T> items)
        {
            var count = 0;

            foreach (var item in items)
            {
                this.DeleteItem(item);
                count++;
            }

            this.SynchroniseWithStore();

            return count;
        }

        public int DeleteAll()
        {
            var count = this.items.Count;

            this.items.Clear();
            this.SynchroniseWithStore();

            return count;
        }

        public int Update(T item)
        {
            this.UpdateItem(item);

            this.SynchroniseWithStore();

            return 1;
        }

        public int Update(IEnumerable<T> items)
        {
            var count = items.Count();
            foreach (var item in items)
            {
                this.Update(item);
            }

            this.SynchroniseWithStore();

            return count;
        }

        public List<T> TryLoadData()
        {
            var result = new List<T>();
            try
            {
                result = this.dataProvider
                    .GetAll<T>()
                    .ToList();
            }
            catch
            {
            }

            return result;
        }

        private void SynchroniseWithStore()
        {
            this.dataProvider.SaveAll<T>(this.items.ToArray());
        }

        private void UpdateItem(T item)
        {
            if (this.items.Contains(item))
            {
                var itemFromList = this.items.ElementAt(this.items.IndexOf(item));
                CompareReferencesToItem(item, itemFromList);
            }
        }

        private void DeleteItem(T item)
        {
            this.items.Remove(item);
        }

        private void CompareReferencesToItem(T item, T itemFromList)
        {
            if (!ReferenceEquals(itemFromList, item))
            {
                // The items are "equal" but do not refer to the same instance.                    // Somebody overrode Equals on the type passed as an argument. Replace:
                ReplaceItemInList(item);
            }
        }

        private void ReplaceItemInList(T item)
        {
            int index = this.items.IndexOf(item);
            this.items.RemoveAt(index);
            this.items.Insert(index, item);
        }
    }
}