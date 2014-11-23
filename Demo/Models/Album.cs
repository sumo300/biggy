namespace Demo.Models
{
    using System;
    using System.Collections.Generic;

    public partial class Album
    {
        public Album()
        {
            //this.Tracks = new HashSet<Track>();
        }

        public int AlbumId { get; set; }

        public string Title { get; set; }

        public int ArtistId { get; set; }

        //public virtual Artist Artist { get; set; }
        //public virtual ICollection<Track> Tracks { get; set; }
    }
}
