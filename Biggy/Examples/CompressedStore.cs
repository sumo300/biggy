using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace Biggy.Examples
{
  // Here is an example of compressed file store decorator, which takes advantage of that 
  // JsonStore (or any file-based store) doesn't use file system directly. So we can simply 
  // add file compression above of other store, which knows how to serialize objects.
  //
  // USAGE (decorate file-base store with CompressedStore):
  //
  //         var gzipStore = new CompressedStore<Person>(new JsonStore<Person>());
  //         var biggyList = new BiggyList<Person>(gzipStore);
  //
  public class CompressedStore<T> : FileSystemStore<T>, IBiggyStore<T> where T : new() {
    // Decorated store which knows how to serialize (e.g. JsonStore)
    private readonly FileSystemStore<T> _fs;

    public CompressedStore(FileSystemStore<T> store) {
      _fs = store;
    }

    protected override string GetFileName(string dbname) {
      return dbname + ".gzip";
    }

    // Because file operations are done in FileSystemStore and decorated store (_fs) only uses
    // stream object to read/write, then we just need to inject compression stream when read/write
    // from compressed file. But Append won't work that way...
    public override List<T> Load(Stream stream) {
      using (var gzip = new GZipStream(stream, CompressionMode.Decompress))
        return _fs.Load(gzip);
    }

    public override void FlushToDisk(Stream stream, List<T> items) {
      using (var gzip = new GZipStream(stream, CompressionLevel.Fastest))
        _fs.FlushToDisk(gzip, items);
    }

    // However there is one catch here. JsonStore can append items to list without rewrite whole file 
    // and FileSystemStore calls Append method passing appendable stream to just append more lines to the file.
    // In compressed file I think this is no longer possible, so we need change this behaviour and flush
    // the entire file in case of adding items.
    T IBiggyStore<T>.Add(T item) {
      _items.Add(item);
      using (var stream = new FileStream(DbPath, FileMode.Create)) {
        FlushToDisk(stream, _items);
      }
      return item;
    }
 
    IList<T> IBiggyStore<T>.Add(List<T> items) {
      _items.AddRange(items);
      using (var stream = new FileStream(DbPath, FileMode.Create)) {
        FlushToDisk(stream, _items);
      }
      return items;
    }
 
    // We have changed above "Add" methods, that Append won't be called so we can ignore this method
    public override void Append(Stream stream, List<T> items) {
      throw new NotImplementedException();
    }
  }
}
