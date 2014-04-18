using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Biggy.Extensions;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Directory = Lucene.Net.Store.Directory;
using Version = Lucene.Net.Util.Version;

namespace Biggy.Lucene
{
    /// <summary>
    ///     Decorator class that adds lucene full text functionality to any store
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class LuceneStoreDecorator<T> : IBiggyStore<T>, IQueryableBiggyStore<T>, IDisposable where T : new()
    {
        private const int CommitDocumentsLimit = 100;
        private readonly IBiggyStore<T> _biggyStore;
        private readonly IQueryableBiggyStore<T> _queryableStore;
        private readonly string[] _fullTextFields;
        private readonly ConcurrentDictionary<string, T> _keyCache = new ConcurrentDictionary<string, T>();
        private readonly string _primaryKeyField;
        private readonly IndexWriter _indexWriter;

        public LuceneStoreDecorator(IBiggyStore<T> biggyStore, bool useRamDirectory = false)
        {
            _biggyStore = biggyStore;
            _queryableStore = _biggyStore as IQueryableBiggyStore<T>;

            // Create directory
            string path = AppDomain.CurrentDomain.GetData("DataDirectory").ToString();
            var luceneDirectory = new DirectoryInfo(Path.Combine(path, "LuceneIndex"));
            Directory directory = useRamDirectory ? (Directory)new RAMDirectory() : new SimpleFSDirectory(luceneDirectory);
            directory.ClearLock("write");

            // Create index writer
            _indexWriter = new IndexWriter(directory, new StandardAnalyzer(Version.LUCENE_30), IndexWriter.MaxFieldLength.UNLIMITED);

            // Figure out the fields to index
            var dummyObj = new T();
            List<PropertyInfo> foundProps = dummyObj.LookForCustomAttribute(typeof (FullTextAttribute));
            _fullTextFields = foundProps.Select(x => x.Name).ToArray();
            _primaryKeyField = PrimaryKeyProperty();
        }

        public List<T> Load()
        {
            ClearIndex();

            List<T> items = _biggyStore.Load();

            AddDocumentsToIndex(items);

            return Add(items);
        }

        public void SaveAll(List<T> items)
        {
            _biggyStore.SaveAll(items);
        }

        public void Clear()
        {
            ClearIndex();

            _biggyStore.Clear();
        }

        public T Add(T item)
        {
            return AddDocumentToIndex(_biggyStore.Add(item));
        }

        public List<T> Add(List<T> items)
        {
            AddDocumentsToIndex(items);

            return _biggyStore.Add(items);
        }

        public void Dispose()
        {
            _indexWriter.Dispose();
        }

        public IQueryable<T> AsQueryable()
        {
            return _queryableStore.AsQueryable();
        }

        private void AddDocumentsToIndex(IEnumerable<T> items)
        {
            items.AsParallel().ForAll(x => AddDocumentToIndex(x));
        }

        public List<T> Search(string query, int resultCount)
        {
            var queryParser = new MultiFieldQueryParser(Version.LUCENE_30, _fullTextFields, new StandardAnalyzer(Version.LUCENE_30));

            using (var indexSearcher = new IndexSearcher(_indexWriter.GetReader()))
            {
                Query searchQuery = queryParser.Parse(query);
                TopDocs hits = indexSearcher.Search(searchQuery, resultCount);

                var results = new List<T>();

                foreach (ScoreDoc scoreDoc in hits.ScoreDocs)
                {
                    Document doc = indexSearcher.Doc(scoreDoc.Doc);

                    string id = doc.Get(_primaryKeyField);
                    
                    // We need some way to match up lucene docs to the objects so we use dictionary here to keep the references
                    if (_keyCache.ContainsKey(id))
                        results.Add(_keyCache[id]);
                }

                return results;
            }
        }

        private T AddDocumentToIndex(T item)
        {
            var doc = new Document();

            // Since we dont store the whole object in lucene (only the freetextfields) we need to store an identifier
            // which will allow us to match search results to objects in the array
            string primaryKeyValue = GetPropValue(item, _primaryKeyField).ToString();
            doc.Add(new Field(_primaryKeyField, primaryKeyValue, Field.Store.YES, Field.Index.NOT_ANALYZED_NO_NORMS));

            foreach (string fieldName in _fullTextFields)
            {
                var value = GetPropValue(item, fieldName) as string;
                doc.Add(new Field(fieldName, value, Field.Store.NO, Field.Index.ANALYZED));
            }

            // TODO: If an object doesn't have an identifier should we use some sort of GUID here?
            _keyCache.TryAdd(primaryKeyValue, item);

            _indexWriter.AddDocument(doc);

            // Since we are using Lucene naer realtime search feature we don't have to commit all the time 
            // as the items are instantly searchable before they hit disk
            // So we are a bit lazy and commit when we really need to
            int numDocsAwaitingCommit = _indexWriter.NumRamDocs();
            if (numDocsAwaitingCommit >= CommitDocumentsLimit)
            {
                Task.Run(() =>
                {
                    _indexWriter.Commit();
                    _indexWriter.Optimize();
                });
            }

            return item;
        }

        private void ClearIndex()
        {
            _keyCache.Clear();
            _indexWriter.DeleteAll();
        }

        private static object GetPropValue(object src, string propName)
        {
            return src.GetType().GetProperty(propName).GetValue(src, null);
        }

        private string PrimaryKeyProperty()
        {
            // TODO: Duplicated code, maybe move to some helper
            string baseName = GetBaseName();
            PropertyInfo[] props = typeof (T).GetProperties();
            PropertyInfo conventionalKey = props.FirstOrDefault(x => x.Name.Equals("id", StringComparison.OrdinalIgnoreCase)) ??
                                           props.FirstOrDefault(x => x.Name.Equals(baseName + "ID", StringComparison.OrdinalIgnoreCase));

            if (conventionalKey == null)
            {
                PropertyInfo foundProp = props
                    .FirstOrDefault(p => p.GetCustomAttributes(false)
                        .Any(a => a.GetType() == typeof (PrimaryKeyAttribute)));

                if (foundProp != null)
                {
                    return foundProp.Name;
                }
            }
            else
                return conventionalKey.Name;

            throw new Exception("Primary key not found for " + baseName);
        }

        private string GetBaseName()
        {
            return typeof (T).Name;
        }
    }
}