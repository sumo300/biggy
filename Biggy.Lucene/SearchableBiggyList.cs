using System.Collections.Generic;
using System.Reflection;
using System.Text;
using PagedList;

namespace Biggy.Lucene
{
    public class SearchableBiggyList<T> : BiggyList<T> where T : new()
    {
        private readonly LuceneStoreDecorator<T> _storeDecorator;

        public SearchableBiggyList(LuceneStoreDecorator<T> storeDecorator) : base(storeDecorator)
        {
            _storeDecorator = storeDecorator;
        }

        public IPagedList<T> Search(string query, int pageNo, int pageSize)
        {
            return _storeDecorator.Search(query, pageNo, pageSize);
        }
    }
}
