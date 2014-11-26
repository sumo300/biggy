using System;
using System.Collections.Generic;
using System.Linq;

namespace Biggy.Data.Azure
{
    internal interface IAzureDataProvider
    {
        IEnumerable<T> GetAll<T>();

        void SaveAll<T>(IEnumerable<T> items);
    }
}