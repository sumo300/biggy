using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biggy {
  public class QueryableBiggylist<T> : BiggyList<T>, IQueryableBiggyList<T> where T : new() {

    IQueryableBiggyStore<T> _queryableStore;

    public QueryableBiggylist(IQueryableBiggyStore<T> store)
      : base(store) {
        _queryableStore = store;
    }

    public IQueryable<T> AsQueryable() {
      return _queryableStore.AsQueryable();
    }
  }
}
