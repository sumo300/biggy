using System;
using System.Linq;

namespace Demo {
  internal class Program {
    private static void Main(string[] args) {
      //// POSTGRES DEMOS: +++++++++++++++++++++++++++++++++++++++++++++++++++++


      pgRelationalDemo _pgRelationalDemo = new pgRelationalDemo();
      _pgRelationalDemo.Run();
      Console.WriteLine("");

      //pgDocumentDemo _pgDocumentDemo = new pgDocumentDemo();
      //_pgDocumentDemo.Run();
      //Console.WriteLine("");

      //// SQL LITE DEMOS: ++++++++++++++++++++++++++++++++++++++++++++++++++++++


      //sqliteRelationalDemo _sqliteRelationalDemo = new sqliteRelationalDemo();
      //_sqliteRelationalDemo.Run();
      //Console.WriteLine("");

      //sqliteDocumentDemo _sqliteDocumentDemo = new sqliteDocumentDemo();
      //_sqliteDocumentDemo.Run();
      //Console.WriteLine("");

      // JSON DEMOS: ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++


      //jsonPocoDemo _jsonPocoDemo = new jsonPocoDemo();
      //_jsonPocoDemo.Run();
      //Console.WriteLine("");

      //jsonDocumentDemo _jsonDocumentDemo = new jsonDocumentDemo();
      //_jsonDocumentDemo.Run();
      //Console.WriteLine("");

      // Playground/Spike: ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

      //Playground _playground = new Playground();
      //_playground.Run();

      Console.Read();
    }
  }
}