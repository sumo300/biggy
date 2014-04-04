using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Biggy.JSON;
using Biggy.Postgres;

namespace Biggy.Perf.FileStore {
  public static class Benchmarks {

    public static void Run() {
      Console.WriteLine("Loading from File Store...");
      var clients = new BiggyList<Client>(new JsonStore<Client>("clients"));
      clients.Clear();
      var sw = new Stopwatch();

      // Write a modest-sized batch to a file:
      int SMALL_BATCH_QTY = 1000;

      Console.WriteLine("Write {0} documents", SMALL_BATCH_QTY);
      var addRange = new List<Client>();
      for (int i = 0; i < SMALL_BATCH_QTY; i++) {
        addRange.Add(new Client() { ClientId = i, LastName = string.Format("Atten {0}", i.ToString()), FirstName = "John", Email = "xivSolutions@example.com" });
      }

      sw.Start();
      clients.Add(addRange);
      sw.Stop();
      Console.WriteLine("Just inserted {0} as documents in {1} ms", clients.Count(), sw.ElapsedMilliseconds);


      int LARGE_BATCH_QTY = 100000;
      Console.WriteLine("Write {0} documents", LARGE_BATCH_QTY);
      sw.Reset();
      clients.Clear();

      // Write a Bigger-sized batch to a file:

      // Reset the temp List;
      addRange = new List<Client>();
      for (int i = 0; i < LARGE_BATCH_QTY; i++) {
        addRange.Add(new Client() { ClientId = i, LastName = string.Format("Atten {0}", i.ToString()), FirstName = "John", Email = "xivSolutions@example.com" });
      }

      sw.Start();
      clients.Add(addRange);
      sw.Stop();
      Console.WriteLine("Just inserted {0} as documents in {1} ms", clients.Count(), sw.ElapsedMilliseconds);

      sw.Reset();
      sw.Start();
      Console.WriteLine("Querying Middle 400 Documents");
      var found = clients.Where(x => x.ClientId > 100 && x.ClientId < 500);
      sw.Stop();
      Console.WriteLine("Queried {0} documents in {1}ms", found.Count(), sw.ElapsedMilliseconds);
    }

  }
}

