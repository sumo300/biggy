using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Biggy.Lucene
{
    public class SearchableBiggyList<T> : BiggyList<T> where T : new()
    {
        private readonly LuceneStoreDecorator<T> _storeDecorator;

        public SearchableBiggyList(LuceneStoreDecorator<T> storeDecorator) : base(storeDecorator)
        {
            _storeDecorator = storeDecorator;
        }

        public List<T> Search(string query, int resultCount)
        {
            return _storeDecorator.Search(query, resultCount);
        }
    }
}
