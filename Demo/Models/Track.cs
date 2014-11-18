
namespace Demo.Models {

  using System;
  using System.Collections.Generic;

  public partial class Track {
    public int TrackId { get; set; }
    public int AlbumId { get; set; }
    string Composer { get; set; }
    public string Name { get; set; }
  }
}
