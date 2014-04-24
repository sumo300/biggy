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
  public class JsonStore<T> : IBiggyStore<T>, IUpdateableBiggyStore<T>, IQueryableBiggyStore<T> where T : new() {
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
      if (String.IsNullOrWhiteSpace(dbName)) {
        var thingyType = this.GetType().GenericTypeArguments[0].Name;
        this.DbName = Inflector.Inflector.Pluralize(thingyType).ToLower();
      } else {
        this.DbName = dbName.ToLower();
      }
      this.DbFileName = this.DbName + ".json";
      this.SetDataDirectory(dbPath);
      //_items = this.TryLoadFileData(this.DbPath);
    }

    public void SetDataDirectory(string dbPath) {
      var dataDir = dbPath;
      if (dbPath == "current") {
        var currentDir = Directory.GetCurrentDirectory();
        if (currentDir.EndsWith("Debug") || currentDir.EndsWith("Release")) {
          var projectRoot = Directory.GetParent(@"..\..\").FullName;
          dataDir = Path.Combine(projectRoot, "Data");
        }
      } else {
        dataDir = Path.Combine(dbPath, "Data");
      }
      Directory.CreateDirectory(dataDir);
      this.DbDirectory = dataDir;
    }

    public List<T> TryLoadFileData(string path) {
      List<T> result = new List<T>();
      if (File.Exists(path)) {
        //format for the deserializer...
        var json = "[" + File.ReadAllText(path).Replace(Environment.NewLine, ",") + "]";
        result = JsonConvert.DeserializeObject<List<T>>(json);
      }

      // Make sure the internal list is not reference-equal to that returned to the caller:
      _items = result.ToList();
      if (ReferenceEquals(_items, result)) {
        throw new Exception("Yuck!");
      }
      return result;
    }

    public T AddItem(T item) {
      var json = JsonConvert.SerializeObject(item);
      //append the to the file
      using (var writer = File.AppendText(this.DbPath)) {
        writer.WriteLine(json);
      }
      _items.Add(item);
      return item;
    }

    public List<T> AddRange(List<T> items) {

      //append the to the file
      using (var writer = File.AppendText(this.DbPath)) {
        foreach(var item in items)
        {
          var json = JsonConvert.SerializeObject(item);
          writer.WriteLine(json);
          _items.Add(item);
        }
      }
      return items;
    }

    public virtual T UpdateItem(T item) {
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
      return item;
    }

    public virtual T RemoveItem(T item) {
      _items.Remove(item);
      this.FlushToDisk();
      return item;
    }

    public virtual List<T> RemoveRange(List<T> items) {
      foreach (var item in items) {
        _items.Remove(item);
      }
      this.FlushToDisk();
      return items;
    }

    public bool FlushToDisk() {
      var completed = false;
      // Serialize json directly to the output stream
      using (var outstream = new StreamWriter(this.DbPath)) {
        var writer = new JsonTextWriter(outstream);
        var serializer = JsonSerializer.CreateDefault();
        // Invoke custom serialization in BiggyListSerializer
        var biggySerializer = new BiggyListSerializer();
        biggySerializer.WriteJson(writer, _items, serializer);
        completed = true;
      }
      return completed;
    }

    //public bool FlushToDisk() {
    //  var completed = false;
    //  // Serialize json directly to the output stream
    //  using (var outstream = new StreamWriter(this.DbPath)) {
    //    var writer = new JsonTextWriter(outstream);
    //    var serializer = JsonSerializer.CreateDefault();
    //    // Invoke custom serialization in BiggyListSerializer
    //    serializer.Serialize(writer, _items);
    //    completed = true;
    //  }
    //  return completed;
    //}

    // IBIGGYSTORE IMPLEMENTATION:

    List<T> IBiggyStore<T>.Load() {
      _items = new List<T>();
      return this.TryLoadFileData(this.DbPath);
    }

    void IBiggyStore<T>.SaveAll(List<T> items) {
      throw new NotImplementedException();
    }

    void IBiggyStore<T>.Clear() {
      _items = new List<T>();
      this.FlushToDisk();
    }

    T IBiggyStore<T>.Add(T item) {
      return this.AddItem(item);
    }

    List<T> IBiggyStore<T>.Add(List<T> items) {
      return this.AddRange(items);
    }

    // IUPDATEABLEBIGGYSTORE IMPLEMENTATION:

    T IUpdateableBiggyStore<T>.Update(T item) {
      return this.UpdateItem(item);
    }

    T IUpdateableBiggyStore<T>.Remove(T item) {
      return this.RemoveItem(item);
    }

    List<T> IUpdateableBiggyStore<T>.Remove(List<T> items) {
      return this.RemoveRange(items);
    }

    // IQUERYABLESTORE IMPLEMENTATION:

    IQueryable<T> IQueryableBiggyStore<T>.AsQueryable() {
      return _items.AsQueryable();
    }
  }
}
