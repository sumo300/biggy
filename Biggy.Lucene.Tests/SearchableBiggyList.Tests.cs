using System;
using System.Collections.Generic;
using System.IO;
using Biggy.JSON;
using Newtonsoft.Json;
using Xunit;
using Film = Biggy.Lucene.Tests.Helpers.Film;

namespace Biggy.Lucene.Tests
{
    [Trait("Lucene Store", "")]
    public class LuceneStoreTests : IDisposable
    {
        private readonly LuceneStoreDecorator<Film> _store;

        public LuceneStoreTests()
        {
            AppDomain.CurrentDomain.SetData("DataDirectory", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ""));
            _store = new LuceneStoreDecorator<Film>(new JsonStore<Film>(dbName: "film"), false);
            _store.Clear();
            _store.Add(GetSomeFilms());
        }

        [Fact(DisplayName = "Finds films")]
        public void Search_Film_No_Fieldname_Specified()
        {
            var results = _store.Search("thugs OR 13", 10);

            Assert.Equal(3, results.Count);

            Console.WriteLine(JsonConvert.SerializeObject(results, Formatting.Indented));
        }

        [Fact(DisplayName = "Finds matching films using fieldname provided")]
        public void Search_Films_In_Field()
        {
            var results = _store.Search("Title: raid", 10);

            Assert.Equal(2, results.Count);

            Console.WriteLine(JsonConvert.SerializeObject(results, Formatting.Indented));
        }

        [Fact(DisplayName = "Finds matching films in field using boolean search")]
        public void Search_Films_In_Field_Using_Boolean_Search()
        {
            var results = _store.Search("Title: raid OR Title: assassins", 10);

            Assert.Equal(3, results.Count);

            Console.WriteLine(JsonConvert.SerializeObject(results, Formatting.Indented));
        }

        private List<Film> GetSomeFilms()
        {
            return new List<Film>()
            {
                new Film
                {
                    FilmId = 1,
                    Title = "The Raid",
                    Description = "A S.W.A.T. team becomes trapped in a tenement run by a ruthless mobster and his army of killers and thugs.",
                    Length = 101,
                    ReleaseYear = 2012
                },
                new Film
                {
                    FilmId = 2,
                    Title = "The Raid 2",
                    Description = "Only a short time after the first raid, Rama goes undercover with the thugs of " +
                                  "Jakarta and plans to bring down the syndicate and uncover the corruption within his police force.",
                    Length = 150,
                    ReleaseYear = 2014
                },
                new Film
                {
                    FilmId = 4,
                    Title = "13 Assassins",
                    Description = "A group of assassins come together for a suicide mission to kill an evil lord.",
                    Length = 141,
                    ReleaseYear = 2010
                }

            };
        }

        public void Dispose()
        {
            _store.Dispose();
        }
    }

}
