namespace Demo.Models {
  using System;

  public partial class Track {
    public int TrackId { get; set; }
    public int AlbumId { get; set; }
    private string Composer { get; set; }
    public string Name { get; set; }
  }
}