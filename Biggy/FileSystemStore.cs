using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biggy
{
  public abstract class FileSystemStore<T> : IBiggyStore<T> where T : new() {
    protected internal List<T> _items;

    public virtual string DbDirectory { get; set; }
    public virtual string DbFileName { get; set; }
    public virtual string DbName { get; set; }

    public virtual string DbPath {
      get {
        return Path.Combine(DbDirectory, DbFileName);
      }
    }

    public virtual bool HasDbFile {
      get {
        return File.Exists(DbPath);
      }
    }

    protected FileSystemStore(string dbPath = "current", string dbName = "") {
      if (String.IsNullOrWhiteSpace(dbName)) {
        var thingyType = this.GetType().GenericTypeArguments[0].Name;
        this.DbName = Inflector.Inflector.Pluralize(thingyType).ToLower();
      } else {
        this.DbName = dbName.ToLower();
      }
      this.DbFileName = GetFileName(this.DbName);
      this.SetDataDirectory(dbPath);
    }

    public virtual void SetDataDirectory(string dbPath) {
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

    protected abstract string GetFileName(string dbname);
    
    public abstract List<T> Load(Stream stream); 

    public abstract void SaveAll(Stream stream, List<T> items);

    public abstract void Append(Stream stream, List<T> items);

    // IBIGGYSTORE IMPLEMENTATION:

    List<T> IBiggyStore<T>.Load() {
      List<T> result = new List<T>();
      if (File.Exists(DbPath))
      using (var stream = File.OpenRead(DbPath)) {
        result = Load(stream);
      }

      // Make sure the internal list is not reference-equal to that returned to the caller:
      _items = result.ToList();
      if (ReferenceEquals(_items, result)) {
        throw new Exception("Yuck!");
      }
      return result;
    }

    void IBiggyStore<T>.SaveAll(List<T> items) {
      using (var stream = new FileStream(DbPath, FileMode.Create))
        SaveAll(stream, items);
    }

    void IBiggyStore<T>.Clear() {
      _items = new List<T>();
      ((IBiggyStore<T>)this).SaveAll(_items);
    }

    T IBiggyStore<T>.Add(T item) {
      using (var stream = new FileStream(DbPath, FileMode.Append)) {
        Append(stream, new List<T> { item });
      }
      _items.Add(item);
      return item;
    }

    List<T> IBiggyStore<T>.Add(List<T> items) {
      using (var stream = new FileStream(DbPath, FileMode.Append)) {
        Append(stream, items);
      }
      _items.AddRange(items);
      return items;
    }

    // IUPDATEABLEBIGGYSTORE IMPLEMENTATION:

    T IBiggyStore<T>.Update(T item) {
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
          _items[index] = item;
        }
        // Otherwise, the item passed is reference-equal. item now refers to it. Process as normal
      }
      ((IBiggyStore<T>)this).SaveAll(_items);
      
      // The item in the list now refers to the item passed in, including updated data:
      return item;
    }

    T IBiggyStore<T>.Remove(T item) {
      _items.Remove(item);
      ((IBiggyStore<T>)this).SaveAll(_items);
      return item;
    }

    List<T> IBiggyStore<T>.Remove(List<T> items) {
      foreach (var item in items) {
        _items.Remove(item);
      }
      ((IBiggyStore<T>)this).SaveAll(_items);
      return items;
    }
  }
}
