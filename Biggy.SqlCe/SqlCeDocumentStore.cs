using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biggy.SqlCe
{
    public class SqlCeDocumentStore<T> : BiggyDocumentStore<T> where T : new()
    {
        public SqlCeDocumentStore(BiggyRelationalContext context) : base(context) { }
        public SqlCeDocumentStore(BiggyRelationalContext context, string tableName) : base(context, tableName) { }

        public SqlCeDocumentStore(string connectionStringName) : base(new SqlCeContext(connectionStringName)) { }
        public SqlCeDocumentStore(string connectionStringName, string tableName) : base(new SqlCeContext(connectionStringName), tableName) { }


        public override T Insert(T item)
        {
            throw new NotImplementedException();
        }

        public override List<T> BulkInsert(List<T> items)
        {
            throw new NotImplementedException();
        }

        public override T Update(T item)
        {
            throw new NotImplementedException();
        }

        public override T Delete(T item)
        {
            throw new NotImplementedException();
        }

        public override List<T> Delete(List<T> items)
        {
            throw new NotImplementedException();
        }

        protected override List<T> TryLoadData()
        {
            throw new NotImplementedException();
        }
    }
}
