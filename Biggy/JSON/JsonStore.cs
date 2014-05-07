using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Biggy.JSON
{
  [JsonConverter(typeof(BiggyListSerializer))]
  public class JsonStore<T> : FileSystemStore<T> where T : new() {

    public JsonStore(string dbPath = "current", string dbName = "") 
     :base(dbPath, dbName) { }

    public override List<T> Load(Stream stream) {
      var reader = new StreamReader(stream);
      var json = "[" + reader.ReadToEnd().Replace(Environment.NewLine, ",") + "]";
      return JsonConvert.DeserializeObject<List<T>>(json);
    }

    public override void Append(Stream stream, List<T> items) {
      using (var writer = new StreamWriter(stream)) {
        foreach (var item in items) {
          var json = JsonConvert.SerializeObject(item);
          writer.WriteLine(json);
        }
      }
    }

    public override void SaveAll(Stream stream, List<T> items) {
      using (var outstream = new StreamWriter(stream)) {
        var writer = new JsonTextWriter(outstream);
        var serializer = JsonSerializer.CreateDefault();
        // Invoke custom serialization in BiggyListSerializer
        var biggySerializer = new BiggyListSerializer();
        biggySerializer.WriteJson(writer, items, serializer);
      }
    }

  }
}
