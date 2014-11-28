using System;
using System.Collections.Generic;
using System.Linq;
using Biggy.Core;

namespace Demo.Models {
  public class ArtistDocument {
    public ArtistDocument() {
      this.Albums = new List<AlbumDocument>();
    }

    [PrimaryKey(Auto: false)]
    public int ArtistDocumentId { get; set; }
    public string Name { get; set; }
    public List<AlbumDocument> Albums;
  }
}