namespace Demo.Models {
  using System;
  using System.Collections.Generic;

  public partial class AlbumDocument {
    public AlbumDocument() {
      this.Tracks = new List<Track>();
    }

    public int AlbumId { get; set; }
    public string Title { get; set; }
    public int ArtistId { get; set; }
    public virtual List<Track> Tracks { get; set; }
  }
}