using System;
using Newtonsoft.Json;

namespace Biggy.Lucene.Tests {
  public static class TestHelperExtensions {
    public static object Dump(this object obj) {
      Console.WriteLine(JsonConvert.SerializeObject(obj, Formatting.Indented));
      return obj;
    }
  }
}