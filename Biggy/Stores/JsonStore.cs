using Newtonsoft.Json;
using Piggy.Execution;
using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Linq;

namespace Piggy {
  public class JsonStore<T> where T : new() {
    public string DbDirectory { get; set; }
    public string DbFileName { get; set; }
    public string DbName { get; set; }
    internal List<T> _items;

    public string DbPath {
      get {
        return Path.Combine(DbDirectory, DbFileName);
      }
    }

    public bool HasDbFile {
      get {
        return File.Exists(DbPath);
      }
    }

    public JsonStore(string dbPath = "current", string dbName = "") {
      _items = new List<T>();
      if (String.IsNullOrWhiteSpace(dbName)) {
        var thingyType = new T().GetType().Name;
        this.DbName = Inflector.Inflector.Pluralize(thingyType).ToLower();
      } else {
        this.DbName = dbName.ToLower();
      }
      this.DbFileName = this.DbName + ".json";
      this.SetDataDirectory(dbPath);
    }

    public void SetDataDirectory(string dbPath) {
      var dataDir = dbPath;
      if (dbPath == "current") {
        var currentDir = Directory.GetCurrentDirectory();
        if (currentDir.EndsWith("Debug") || currentDir.EndsWith("Release")) {
          var projectRoot = Directory.GetParent(@"..\..\").FullName;
          dataDir = Path.Combine(projectRoot, "Data");
        }
      }
      Directory.CreateDirectory(dataDir);
      this.DbDirectory = dataDir;
    }

    public List<T> TryLoadData() {
      List<T> result = new List<T>();
      if (File.Exists(this.DbPath)) {
        //format for the deserializer...
        var json = File.ReadAllText(this.DbPath);

        //var json = "[" + File.ReadAllText(this.DbPath).Replace(Environment.NewLine, ",") + "]";
        result = JsonConvert.DeserializeObject<List<T>>(json);
      }

      // Make sure the internal list is not reference-equal to that returned to the caller:
      _items = result.ToList();
      if (ReferenceEquals(_items, result)) {
        throw new Exception("Yuck!");
      }
      return result;
    }

    public int Add(T item) {
      _items.Add(item);
      this.FlushToDisk();
      return 1;
    }

    public int Add(IEnumerable<T> items) {
      if (items.Count() == 0) {
        return 0;
      }
      _items.AddRange(items);
      this.FlushToDisk();
      return items.Count();
    }

    public virtual int Update(T item) {
      T itemFromList = default(T);
      if (!_items.Contains(item)) {
        // Figure out what to do here. Retreive Key From Store and evaluate? Throw for now:
        throw new InvalidOperationException(
          @"The list does not contain a reference to the object passed as an argument. 
          Make sure you are passing a valid reference, or override Equals on the type being passed.");
      } else {
        itemFromList = _items.ElementAt(_items.IndexOf(item));
        if (!ReferenceEquals(itemFromList, item)) {
          // The items are "equal" but do not refer to the same instance. 
          // Somebody overrode Equals on the type passed as an argument. Replace:
          int index = _items.IndexOf(item);
          _items.RemoveAt(index);
          _items.Insert(index, item);
        }
        // Otherwise, the item passed is reference-equal. item now refers to it. Process as normal
      }
      this.FlushToDisk();

      // The item in the list now refers to the item passed in, including updated data:
      return 1;
    }

    public int Update(IEnumerable<T> items) {
      foreach(var item in items) {
        T itemFromList = default(T);
        if (!_items.Contains(item)) {
          // Figure out what to do here. Retreive Key From Store and evaluate? Throw for now:
          throw new InvalidOperationException(
            @"The list does not contain a reference to the object passed as an argument. 
            Make sure you are passing a valid reference, or override Equals on the type being passed.");
        } else {
          itemFromList = _items.ElementAt(_items.IndexOf(item));
          if (!ReferenceEquals(itemFromList, item)) {
            // The items are "equal" but do not refer to the same instance. 
            // Somebody overrode Equals on the type passed as an argument. Replace:
            int index = _items.IndexOf(item);
            _items.RemoveAt(index);
            _items.Insert(index, item);
          }
          // Otherwise, the item passed is reference-equal. item now refers to it. Process as normal
        }
        this.FlushToDisk();
      }
      return items.Count();
    }

    public virtual int Delete(T item) {
      _items.Remove(item);
      this.FlushToDisk();
      return 1;
    }

    public virtual int Delete(IEnumerable<T> items) {
      int removed = items.Count();
      foreach (var item in items) {
        _items.Remove(item);
      }
      this.FlushToDisk();
      return removed;
    }

    public virtual int DeleteAll() {
      int removed = _items.Count;
      _items.Clear();
      this.FlushToDisk();
      return removed;
    }

    public bool FlushToDisk() {
      var completed = false;
      // Serialize json directly to the output stream
      using (var outstream = new StreamWriter(this.DbPath)) {
        var writer = new JsonTextWriter(outstream);
        var serializer = JsonSerializer.CreateDefault();
        serializer.Serialize(writer, _items);
        completed = true;
      }
      return completed;
    }

    public int ImportJson(string path) {
      List<T> result = new List<T>();
      if (File.Exists(path)) {
        //format for the deserializer...
        var json = File.ReadAllText(path);
        result = JsonConvert.DeserializeObject<List<T>>(json);
        _items.AddRange(result);
      }
      return result.Count;
    }
  }

  internal class PiggyListSerializer : JsonConverter {
    public override bool CanConvert(System.Type objectType) {
      throw new System.NotImplementedException();
    }

    public override object ReadJson(JsonReader reader, System.Type objectType, object existingValue, JsonSerializer serializer) {
      throw new System.NotImplementedException();
    }

    // Custom Biggylist serialization which simply writes each object, separated by newlines, to the output
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
      var list = value as IEnumerable;

      // Loop over all items in the list
      foreach (var item in list) {
        // Serialize the object to the writer
        serializer.Serialize(writer, item);

        // Separate with newline characters
        writer.WriteRaw("\r\n");
      }
    }
  }
}
