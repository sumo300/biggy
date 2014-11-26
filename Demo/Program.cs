using System;
using System.Linq;

namespace Demo {
  internal class Program {
    private static void Main(string[] args) {
      //// POSTGRES DEMOS: +++++++++++++++++++++++++++++++++++++++++++++++++++++

      //pgRelationalDemo _pgRelationalDemo = new pgRelationalDemo();
      //pgDocumentDemo _pgDocumentDemo = new pgDocumentDemo();

      //_pgRelationalDemo.Run();
      //Console.WriteLine("");

      //_pgDocumentDemo.Run();
      //Console.WriteLine("");

      //// SQL LITE DEMOS: ++++++++++++++++++++++++++++++++++++++++++++++++++++++

      //sqliteRelationalDemo _sqliteRelationalDemo = new sqliteRelationalDemo();
      //sqliteDocumentDemo _sqliteDocumentDemo = new sqliteDocumentDemo();

      //_sqliteRelationalDemo.Run();
      //Console.WriteLine("");

      //_sqliteDocumentDemo.Run();
      //Console.WriteLine("");

      // JSON DEMOS: ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

      //jsonPocoDemo _jsonPocoDemo = new jsonPocoDemo();
      //jsonDocumentDemo _jsonDocumentDemo = new jsonDocumentDemo();

      //_jsonPocoDemo.Run();
      //Console.WriteLine("");

      //_jsonDocumentDemo.Run();
      //Console.WriteLine("");

      // Playground/Spike: ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

      Playground _playground = new Playground();
      _playground.Run();

      Console.Read();
    }
  }
}