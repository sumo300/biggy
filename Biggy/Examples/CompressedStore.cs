using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using Biggy;

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
  public class CompressedStore<T> : Biggy.FileSystemStore<T> where T : new() {
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
    // from compressed file. Lol even append works well ;-)
    public override List<T> Load(Stream stream) {
      using (var gzip = new GZipStream(stream, CompressionMode.Decompress))
        return _fs.Load(gzip);
    }

    public override void SaveAll(Stream stream, List<T> items) {
      using (var gzip = new GZipStream(stream, CompressionLevel.Fastest))
        _fs.SaveAll(gzip, items);
    }

    public override void Append(Stream stream, List<T> items) {
      using (var gzip = new GZipStream(stream, CompressionLevel.Fastest))
        _fs.Append(gzip, items);
    }
  }
}
