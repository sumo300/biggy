using System;
using System.Collections.Generic;
using System.Linq;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Search.Similar;
using PagedList;
using Version = Lucene.Net.Util.Version;

namespace Biggy.Lucene
{
    /// <summary>
    ///     Decorator class that adds lucene full text functionality to any store
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class LuceneStoreDecorator<T> : IBiggyStore<T>, IDisposable where T : new()
    {
        private readonly IBiggyStore<T> _biggyStore;
        private readonly LuceneIndexer<T> _luceneIndexer;
        //private readonly IQueryableBiggyStore<T> _queryableStore;

        public LuceneStoreDecorator(IBiggyStore<T> biggyStore, bool useRamDirectory = false)
        {
            _biggyStore = biggyStore;
            //_queryableStore = _biggyStore as IQueryableBiggyStore<T>;
            _luceneIndexer = new LuceneIndexer<T>(useRamDirectory);
        }

        public List<T> Load()
        {
            List<T> items = _biggyStore.Load();

            _luceneIndexer.DeleteAll();
            _luceneIndexer.AddDocumentsToIndex(items);

            return items;
        }

        public void Clear()
        {
            _biggyStore.Clear();
            _luceneIndexer.DeleteAll();
        }

        public T Add(T item)
        {
            item = _biggyStore.Add(item);
            _luceneIndexer.AddDocumentToIndex(item);

            return item;
        }

        public List<T> Add(List<T> items)
        {
            items = _biggyStore.Add(items);
            _luceneIndexer.AddDocumentsToIndex(items);

            return items;
        }

        public void Dispose()
        {
            _luceneIndexer.Dispose();
        }

        //public IQueryable<T> AsQueryable()
        //{
        //    return _queryableStore.AsQueryable();
        //}

        public virtual T Update(T item)
        {
            if (_biggyStore != null)
            {
                _biggyStore.Update(item);
                _luceneIndexer.UpdateDocumentInIndex(item);
            }

            return item;
        }

        public virtual T Remove(T item)
        {
            if (_biggyStore != null)
            {
                _biggyStore.Remove(item);
                _luceneIndexer.DeleteDocumentFromIndex(item);
            }

            return item;
        }

        public List<T> Remove(List<T> items)
        {
            if (_biggyStore != null)
            {
                _biggyStore.Remove(items);
                _luceneIndexer.DeleteDocumentsFromIndex(items);
            }
            else
            {
                throw new InvalidOperationException("You must Implement IUpdatableBiggySotre to call this operation");
            }

            return items;
        }

        /// <summary>
        /// Uses lucenes MoreLikeThis feature to find items similar to the one passed in
        /// </summary>
        /// <param name="item">The item to find similar items</param>
        /// <param name="pageNo">Page number of the result set</param>
        /// <param name="pageSize">Number of items to return in the result set</param>
        /// <returns>Items similar to the one pased in</returns>
        public IPagedList<T> MoreLikeThis(T item, int pageNo, int pageSize)
        {
            using (IndexSearcher indexSearcher = _luceneIndexer.GetSearcher())
            {
                var itemId = _luceneIndexer.GetIdentifier(item);
                var docQuery = new TermQuery(new Term(_luceneIndexer.PrimaryKeyField, itemId));

                var docHit = indexSearcher.Search(docQuery, 1);

                if (docHit.ScoreDocs.Any())
                {
                    var moreLikeThis = new MoreLikeThis(indexSearcher.IndexReader)
                    {
                        MaxDocFreq = 0, 
                        MinTermFreq = 0
                    };

                    //moreLikeThis.SetFieldNames(_luceneIndexer.FullTextFields);
                    
                    var likeQuery = moreLikeThis.Like(docHit.ScoreDocs[0].Doc);

                    var query = new BooleanQuery
                    {
                        {likeQuery, Occur.MUST}, 
                        //{docQuery, Occur.MUST_NOT} // Exclude the doc we basing similar matches on
                    };

                    return Search(query, pageNo, pageSize, indexSearcher);
                }

                return NoResults(pageNo, pageSize);
            }
        }

        public IPagedList<T> NoResults(int pageNo, int pageSize)
        {
            return new StaticPagedList<T>(new List<T>(), pageNo, pageSize, 0);
        }

        public IPagedList<T> Search(string query, int pageNo = 1, int pageSize = 25)
        {
            var queryParser = new MultiFieldQueryParser(Version.LUCENE_30, _luceneIndexer.FullTextFields, new StandardAnalyzer(Version.LUCENE_30));

            using (IndexSearcher indexSearcher = _luceneIndexer.GetSearcher())
            {
                Query searchQuery = queryParser.Parse(query);

                return Search(searchQuery, pageNo, pageSize, indexSearcher);
            }
        }

        private IPagedList<T> Search(Query searchQuery, int pageNo, int pageSize, IndexSearcher indexSearcher)
        {
            TopDocs hits = indexSearcher.Search(searchQuery, pageNo * pageSize);

            var results = new List<T>();

            int startIndex = (pageNo - 1)*pageSize;
            int endIndex = pageNo*pageSize;
            if (hits.TotalHits < endIndex)
                endIndex = hits.TotalHits;

            for (int i = startIndex; i < endIndex; i++)
            {
                ScoreDoc scoreDoc = hits.ScoreDocs[i];
                Document doc = indexSearcher.Doc(scoreDoc.Doc);

                string id = doc.Get(_luceneIndexer.PrimaryKeyField);

                // We need some way to match up lucene docs to the objects 
                // so we use dictionary here to keep the references
                if (_luceneIndexer.ItemCache.ContainsKey(id))
                    results.Add(_luceneIndexer.ItemCache[id]);
            }

            return new StaticPagedList<T>(results, pageNo, pageSize, hits.TotalHits);;
        }
    }
}