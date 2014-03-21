using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biggy
{
  public class BiggyStore<T> : IBiggyStore<T>
  {

    public IList<T> Load()
    {
      throw new NotImplementedException();
    }

    public void SaveAll(IList<T> items)
    {
      throw new NotImplementedException();
    }

    public void Clear()
    {
      throw new NotImplementedException();
    }

    public T Add(T item)
    {
      throw new NotImplementedException();
    }

    public IEnumerable<T> Add(IEnumerable<T> items)
    {
      throw new NotImplementedException();
    }
  }
}
