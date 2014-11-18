using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo {
  class Program {
    static void Main(string[] args) {

      pgRelationalDemo _pgRelationalDemo = new pgRelationalDemo();
      //pgDocumentDemo _pgDocumentDemo = new pgDocumentDemo();

      sqliteRelationalDemo _sqliteRelationalDemo = new sqliteRelationalDemo();
      //sqliteDocumentDemo _sqliteDocumentDemo = new sqliteDocumentDemo();

      //jsonPocoDemo _jsonPocoDemo = new jsonPocoDemo();
      //jsonDocumentDemo _jsonDocumentDemo = new jsonDocumentDemo();

      //_pgRelationalDemo.Run();
      //Console.WriteLine("");

      //_pgDocumentDemo.Run();
      //Console.WriteLine("");

      _sqliteRelationalDemo.Run();
      Console.WriteLine("");

      //_sqliteDocumentDemo.Run();
      //Console.WriteLine("");

      //_jsonPocoDemo.Run();
      //Console.WriteLine("");

      //_jsonDocumentDemo.Run();
      //Console.WriteLine("");


      Console.Read();
    }
  }
}
