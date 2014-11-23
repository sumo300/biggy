namespace Demo.Models
{
    using System;

    public partial class Artist
    {
        public Artist()
        {
            //this.Albums = new HashSet<Album>();
        }

        public int ArtistId { get; set; }

        public string Name { get; set; }

        //public virtual ICollection<Album> Albums { get; set; }
    }
}