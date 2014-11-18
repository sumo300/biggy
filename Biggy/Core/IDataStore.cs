using System;
namespace Biggy.Core {
  public interface IDataStore<T>
   where T : new() {
    int Add(System.Collections.Generic.IEnumerable<T> items);
    int Add(T item);
    int Delete(System.Collections.Generic.IEnumerable<T> items);
    int Delete(T item);
    int DeleteAll();
    System.Collections.Generic.List<T> TryLoadData();
    int Update(System.Collections.Generic.IEnumerable<T> items);
    int Update(T item);
  }
}
