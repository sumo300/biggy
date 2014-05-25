using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Biggy.JSON;
using Xunit;
using Film = Biggy.Lucene.Tests.Models.Film;

namespace Biggy.Lucene.Tests {
  [Trait("Lucene Store", "")]
  public class LuceneStoreDecoratorTests : IDisposable {
    private readonly LuceneStoreDecorator<Film> _store;
    private readonly List<Film> _films = new List<Film>() {
      new Film {
          FilmId = 1,
          Title = "The Raid",
          Description = "A S.W.A.T. team becomes trapped in a tenement run by a ruthless mobster and his army of killers and thugs.",
          Length = 101,
          ReleaseYear = 2012
      },
      new Film {
          FilmId = 2,
          Title = "The Raid 2",
          Description = "Only a short time after the first raid, Rama goes undercover with the thugs of " +
                        "Jakarta and plans to bring down the syndicate and uncover the corruption within his police force.",
          Length = 150,
          ReleaseYear = 2014
      },
      new Film {
          FilmId = 3,
          Title = "13 Assassins",
          Description = "A group of assassins come together for a suicide mission to kill an evil lord.",
          Length = 141,
          ReleaseYear = 2010
      }
    };

    public LuceneStoreDecoratorTests() {
      AppDomain.CurrentDomain.SetData("DataDirectory", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ""));
      _store = new LuceneStoreDecorator<Film>(new JsonStore<Film>(dbName: "film"), false);
      _store.Clear();
      _store.Add(_films);
    }

    [Fact(DisplayName = "Finds films")]
    public void Search_Film_No_Fieldname_Specified() {
      var results = _store.Search("thugs OR 13");
      Assert.Equal(3, results.Count);
      results.Dump();
    }

    [Fact(DisplayName = "Finds matching films using fieldname provided")]
    public void Search_Films_In_Field() {
      var results = _store.Search("Title: raid");
      Assert.Equal(2, results.Count);
      results.Dump();
    }

    [Fact(DisplayName = "Finds matching films in field using boolean search")]
    public void Search_Films_In_Field_Using_Boolean_Search() {
      var results = _store.Search("Title: raid OR Title: assassins");
      Assert.Equal(3, results.Count);
      results.Dump();
    }

    [Fact(DisplayName = "Find's more items like one one passed in")]
    public void MoreLikeThis_Find_Items_Similar_To_Item_Passed_In() {
      // TODO: Fix this test

      var itemToMatch = _films.First(); /* Raid */
      var results = _store.MoreLikeThis(itemToMatch, 1, 3);
      Assert.Equal(1, results.Count);
      results.Dump();
    }

    [Fact(DisplayName = "Updated film no longer matches search criteria")]
    public void Update_Fim_Shouldnt_Match_Search_Criteria() {
      var filmToUpdate = _store.Load().First(x => x.FilmId == 3 /* 13 Assassins */);
      filmToUpdate.Title = filmToUpdate.Title.Replace("Assassins", "ninjas");
      _store.Update(filmToUpdate);
      var results = _store.Search("Title: raid OR Title: assassins");
      Assert.Equal(2, results.Count);
      results.Dump();
    }

    [Fact(DisplayName = "Removed film no longer matches search criteria")]
    public void Removed_Fim_Shouldnt_Match_Search_Criteria() {
      var filmToRemove = _store.Load().First(x => x.FilmId == 2 /* Raid 2 */);
      _store.Remove(filmToRemove);
      var results = _store.Search("raid");
      Assert.Equal(1, results.Count);
      results.Dump();
    }

    public void Dispose() {
      _store.Dispose();
    }
  }
}
