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
using Lucene.Net.Search;
using Lucene.Net.Store;
using Directory = Lucene.Net.Store.Directory;
using Version = Lucene.Net.Util.Version;

namespace Biggy.Lucene
{
    /// <summary>
    /// A helper for lucene index operations
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class LuceneIndexer<T> : IDisposable where T : new()
    {
        private const int CommitDocumentsLimit = 100;

        private readonly IndexWriter _indexWriter;

        public LuceneIndexer(bool useRamDirectory)
        {
            // Create directory
            string path = AppDomain.CurrentDomain.GetData("DataDirectory").ToString();
            var luceneDirectory = new DirectoryInfo(Path.Combine(path, "LuceneIndex"));
            Directory directory = useRamDirectory ? (Directory) new RAMDirectory() : new SimpleFSDirectory(luceneDirectory);
            directory.ClearLock("write");

            // Create index writer
            _indexWriter = new IndexWriter(directory, new StandardAnalyzer(Version.LUCENE_30), IndexWriter.MaxFieldLength.UNLIMITED);

            // Figure out the fields to index
            var dummyObj = new T();
            List<PropertyInfo> foundProps = dummyObj.LookForCustomAttribute(typeof (FullTextAttribute));
            FullTextFields = foundProps.Select(x => x.Name).ToArray();
            PrimaryKeyField = PrimaryKeyProperty();
            ItemCache = new ConcurrentDictionary<string, T>();
        }

        public string[] FullTextFields { get; set; }
        public string PrimaryKeyField { get; set; }

        /// <summary>
        /// All the items in the index with the PrimaryKey as the Key
        /// </summary>
        public ConcurrentDictionary<string, T> ItemCache { get; set; }

        public IndexSearcher GetSearcher()
        {
            return new IndexSearcher(_indexWriter.GetReader());
        }

        public T AddDocumentToIndex(T item)
        {
            string identifier;
            Document doc = BuildDoc(item, out identifier);

            _indexWriter.AddDocument(doc);

            ItemCache.TryAdd(identifier, item);

            CommitIfRequired();

            return item;
        }

        public void AddDocumentsToIndex(IEnumerable<T> items)
        {
            items.AsParallel().ForAll(x => AddDocumentToIndex(x));
        }

        public T UpdateDocumentInIndex(T item)
        {
            string identifier;
            Document updatedDocment = BuildDoc(item, out identifier);

            _indexWriter.UpdateDocument(new Term(PrimaryKeyField, identifier), updatedDocment);

            CommitIfRequired();

            return item;
        }

        public T DeleteDocumentFromIndex(T item)
        {
            string identifier = GetIdentifier(item);
            _indexWriter.DeleteDocuments(new Term(PrimaryKeyField, identifier));

            CommitIfRequired();

            return item;
        }

        public void DeleteDocumentsFromIndex(IEnumerable<T> items)
        {
            items.AsParallel().ForAll(x => DeleteDocumentFromIndex(x));
        }

        public void DeleteAll()
        {
            ItemCache.Clear();
            _indexWriter.DeleteAll();
        }

        private Document BuildDoc(T item, out string identifier)
        {
            var doc = new Document();

            // Since we dont store the whole object in lucene (only the freetextfields) we need to store an identifier
            // which will allow us to match search results to objects in the array
            identifier = GetIdentifier(item);
            doc.Add(new Field(PrimaryKeyField, identifier, Field.Store.YES, Field.Index.NOT_ANALYZED));

            foreach (string fieldName in FullTextFields)
            {
                var value = GetPropValue(item, fieldName) as string;
                doc.Add(new Field(fieldName, value, Field.Store.YES, Field.Index.ANALYZED));
            }

            return doc;
        }

        public string GetIdentifier(T obj)
        {
            return GetPropValue(obj, PrimaryKeyField).ToString();
        }

        private void CommitIfRequired()
        {
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
        }

        private static object GetPropValue(T src, string propName)
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

        public void Dispose()
        {
            _indexWriter.Dispose();
        }
    }
}