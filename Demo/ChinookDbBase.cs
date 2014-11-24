using System;
using System.Linq;
using Biggy.Core;
using Demo.Models;

namespace Demo {
  public abstract class ChinookDbBase {
    public BiggyList<Artist> Artists { get; set; }
    public BiggyList<Album> Albums { get; set; }
    public BiggyList<Track> Tracks { get; set; }
    public BiggyList<ArtistDocument> ArtistDocuments { get; set; }

    public virtual void LoadData() {
      this.Artists = new BiggyList<Artist>(CreateRelationalStoreFor<Artist>());
      this.Albums = new BiggyList<Album>(CreateRelationalStoreFor<Album>());
      this.Tracks = new BiggyList<Track>(CreateRelationalStoreFor<Track>());
      this.ArtistDocuments = new BiggyList<ArtistDocument>(CreateDocumentStoreFor<ArtistDocument>());
    }

    public abstract void DropCreateAll();
    public abstract IDataStore<T> CreateRelationalStoreFor<T>() where T : new();
    public abstract IDataStore<T> CreateDocumentStoreFor<T>() where T : new();
  }
}