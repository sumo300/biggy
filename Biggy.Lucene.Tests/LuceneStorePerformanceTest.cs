using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Biggy.JSON;
using Biggy.Lucene.Tests.Models;
using PagedList;
using Xunit;

namespace Biggy.Lucene.Tests
{
    [Trait("Lucene Store", "")]
    public class LuceneStorePerformanceTest
    {
        private string[] keywords = { "dolor", "ipsum", "amet", "luctus" };
        private readonly LuceneStoreDecorator<Film> _store;

        public LuceneStorePerformanceTest()
        {
            AppDomain.CurrentDomain.SetData("DataDirectory", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ""));
            _store = new LuceneStoreDecorator<Film>(new JsonStore<Film>(dbName: "film"), false);
            _store.Clear();
        }

        private void WarmUpLucene()
        {
            // We warm up the index which essentially allows and Lucene / OS filesystem caching to kick in 
            // and should reflect most search hits
            _store.Search("dolor");
        }

        [Fact(DisplayName = "Lucene pre-warmed disk based index is faster than in memory contains")]
        public void Searching_With_Lucene_Faster_Than_Contains()
        {
            int numberOfDocsToIndex = 10000;

            // Store a bunch of stuff into lucene
            var filmsToIndex = new List<Film>();
            for (int i = 0; i < numberOfDocsToIndex; i++)
            {
                filmsToIndex.Add(new Film
                {
                    FilmId = i,
                    Title = i + " Lorem ipsum dolor sit amet, consectetur adipiscing elit.",
                    Description = 1 + " Lorem ipsum dolor sit amet, consectetur adipiscing elit. Duis non augue non tellus luctus interdum. Etiam suscipit nisi erat, et consequat magna ullamcorper vitae. Maecenas et dolor non velit ornare accumsan. Fusce pulvinar orci ac nibh varius, vel ullamcorper nisl ultricies. Donec eu rhoncus dui, at placerat est. Mauris ut posuere lacus, sed varius urna. Integer sit amet risus in justo porttitor accumsan et id quam. In bibendum congue urna eget volutpat. Aliquam interdum elit ut nisl consectetur iaculis. Duis id nunc vulputate, suscipit quam sit amet, fringilla diam. Donec et magna nulla. Nullam molestie tristique enim, ut eleifend lacus. Vestibulum ultrices, risus et semper tristique, diam risus vulputate neque, in viverra augue tortor vel nisl."
                });
            }

            _store.Add(filmsToIndex);
            WarmUpLucene();

            var stopwatch = new Stopwatch();

            stopwatch.Start();
            IPagedList<Film> results = _store.Search("Description: ipsum", pageSize: 25);
            stopwatch.Stop();

            long ellapsedLuceneTime = stopwatch.ElapsedMilliseconds;

            stopwatch.Restart();
            IPagedList<Film> results2 = DescriptionContainsSearch("ipsum");
            stopwatch.Stop();

            long ellapsedContainsTime = stopwatch.ElapsedMilliseconds;

            Assert.True(results.TotalItemCount == results2.TotalItemCount);

            new
            {
                LuceneTime = ellapsedLuceneTime,
                Contains = ellapsedContainsTime
            }.Dump();

            Assert.True(ellapsedLuceneTime < ellapsedContainsTime);
        }

        private IPagedList<Film> DescriptionContainsSearch(string keywords)
        {
            var results = _store.AsQueryable().Where(x => x.Description.Contains("ipsum"));
            return new PagedList<Film>(results, 1, 25);
        }
    }
}