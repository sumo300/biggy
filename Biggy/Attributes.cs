using System;
using System.Linq;

namespace Biggy.Core
{
    public class PrimaryKeyAttribute : Attribute
    {
        public bool IsAutoIncrementing { get; private set; }

        public PrimaryKeyAttribute(bool Auto)
        {
            this.IsAutoIncrementing = Auto;
        }
    }

    public class DbColumnAttribute : Attribute
    {
        public string Name { get; protected set; }

        public DbColumnAttribute(string name)
        {
            this.Name = name;
        }
    }

    public class DbTableAttribute : Attribute
    {
        public string Name { get; protected set; }

        public DbTableAttribute(string name)
        {
            this.Name = name;
        }
    }
}