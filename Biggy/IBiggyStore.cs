using System.Collections.Generic;
using System.Linq;

namespace Biggy
{
  public interface IBiggyStore<T> {
    List<T> Load();
    //void SaveAll(List<T> items);
    void Clear();
    T Add(T item);
    IList<T> Add(List<T> items);
    T Update(T item);
    T Remove(T item);
    IList<T> Remove(List<T> items);
  }

  public interface IQueryableBiggyStore<T> : IBiggyStore<T> {
    IQueryable<T> AsQueryable();
  }
}