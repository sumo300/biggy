using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Biggy.Core
{
    public abstract class RelationalStoreBase<T> : IDataStore<T> where T : new()
    {
        public DBTableMapping TableMapping { get; set; }

        public IDbCore Database { get; set; }

        public RelationalStoreBase(IDbCore dbCore)
        {
            this.Database = dbCore;
            this.TableMapping = this.Database.getTableMappingFor<T>();
        }

        public abstract int Add(IEnumerable<T> items);

        public abstract int Add(T item);

        public abstract int Delete(IEnumerable<T> items);

        public abstract int Delete(T item);

        public abstract int DeleteAll();

        public abstract List<T> TryLoadData();

        public abstract int Update(IEnumerable<T> items);

        public abstract int Update(T item);

        public bool KeyIsAutoIncrementing
        {
            get
            {
                if (this.TableMapping.PrimaryKeyMapping[0].IsAutoIncementing)
                {
                    return true;
                }
                return false;
            }
        }

        protected virtual T MapReaderToObject<T>(IDataReader reader) where T : new()
        {
            var item = new T();
            var props = item.GetType().GetProperties();
            foreach (var property in props)
            {
                if (this.TableMapping.ColumnMappings.ContainsPropertyName(property.Name))
                {
                    string mappedColumn = this.TableMapping.ColumnMappings.FindByProperty(property.Name).ColumnName;
                    int ordinal = reader.GetOrdinal(mappedColumn);
                    var val = reader.GetValue(ordinal);
                    if (val.GetType() != typeof(DBNull))
                    {
                        property.SetValue(item, Convert.ChangeType(reader.GetValue(ordinal), property.PropertyType), null);
                    }
                }
            }
            return item;
        }
    }
}