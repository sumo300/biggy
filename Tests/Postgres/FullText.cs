//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Biggy.Postgres;
//using Biggy;
//using Xunit;

//namespace Tests.Postgres {
//  [Trait("PG Full Text Searching", "")]
//  public class PGFullText {

//    IBiggy<Film> films;
//    public PGFullText() {
//      films = new BiggyList<Film>(new PGStore<Film>("chinook"));
//    }

//    [Fact(DisplayName = "Ad hoc TS Vector Query Returns Films")]
//    public void AdHocTSVector() {

//      var results = films.FullTextOnTheFly("monkey", "description", "title");
//      Assert.True(results.Count() > 0);

//    }

//    [Fact(DisplayName = "Tagged full text works wonders")]
//    public void TaggedFullText() {

//      var results = films.FullText("monkey");
//      Assert.True(results.Count() > 0);

//    }
//  }
//}
