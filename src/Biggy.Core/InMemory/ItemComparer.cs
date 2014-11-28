using System;
using System.Linq;

namespace Biggy.Core.InMemory
{
    public sealed class ItemComparer
    {
        public static IItemsAreEquals<T> CreateItemsComparer<T>(T item)
        {
            var containsPrimaryKeyAttribute = typeof(T)
                .GetProperties()
                .Any(prop =>
                    prop.GetCustomAttributes(true)
                        .Any(attr => attr is PrimaryKeyAttribute));

            return containsPrimaryKeyAttribute ?
                (IItemsAreEquals<T>)new ItemsAreEqualsByKey<T>(item) :
                (IItemsAreEquals<T>)new ItemsAreEqualsByReference<T>(item);
        }
    }
}