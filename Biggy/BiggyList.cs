using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Biggy
{
    public class BiggyList<T> : IBiggy<T>
    {
        private readonly IBiggyStore<T> _store;
        private readonly IQueryableBiggyStore<T> _queryableStore;
        private readonly IUpdateableBiggyStore<T> _updateableBiggyStore; 
        private readonly IList<T> _items;

        public BiggyList(IBiggyStore<T> store)
        {
            _store = store;
            _queryableStore = _store as IQueryableBiggyStore<T>;
            _updateableBiggyStore = _store as IUpdateableBiggyStore<T>;
            _items = _store.Load();
        }

        public virtual IEnumerator<T> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public virtual void Clear()
        {
            _store.Clear();
            _items.Clear();
            Fire(Changed, items:null);
        }

        public virtual int Count()
        {
            return _items.Count;
        }

        public virtual T Update(T item)
        {
            if (_updateableBiggyStore != null)
            {
                _updateableBiggyStore.Update(item);
            }
            else
            {
                _store.SaveAll(_items);    
            }            
            Fire(Changed, item: item);
            return item;
        }

        public virtual T Remove(T item)
        {
            _items.Remove(item);
            if (_updateableBiggyStore != null)
            {
                _updateableBiggyStore.Remove(item);
            }
            else
            {
                _store.SaveAll(_items);
            }
            return item;
        }

        public virtual T Add(T item)
        {
            _store.Add(item);
            Fire(ItemAdded, item: item);
            return item;
        }

        public virtual IList<T> Add(IList<T> items)
        {
            _store.Add(items);
            Fire(ItemAdded, items: items);
            return items;
        }

        public virtual IQueryable<T> AsQueryable()
        {
            if (_store is IQueryableBiggyStore<T>)
            {
                return ((IQueryableBiggyStore<T>) _store).AsQueryable();
            }
            return _items.AsQueryable();
        }

        protected virtual void Fire(EventHandler<BiggyEventArgs<T>> @event, T item = default(T), IList<T> items=null)
        {
            if (@event != null)
            {
                var args = new BiggyEventArgs<T> {Item = item, Items = items};                
                @event(this, args);
            }
        }
     
        public event EventHandler<BiggyEventArgs<T>> ItemRemoved;
        public event EventHandler<BiggyEventArgs<T>> ItemAdded;
        public event EventHandler<BiggyEventArgs<T>> Changed;
        public event EventHandler<BiggyEventArgs<T>> Loaded;
        public event EventHandler<BiggyEventArgs<T>> Saved;
    }
}