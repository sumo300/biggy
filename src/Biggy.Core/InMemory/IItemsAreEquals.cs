using System;
using System.Linq;

namespace Biggy.Core.InMemory {
  public interface IItemsAreEquals<T> {
    bool IsMatch(T item);
  }
}