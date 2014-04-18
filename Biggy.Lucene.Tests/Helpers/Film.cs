using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biggy.Lucene.Tests.Helpers
{
    public class Film
    {
        [PrimaryKey]
        public int FilmId { get; set; }

        [FullText]
        public string Title { get; set; }

        [FullText]
        public string Description { get; set; }
        
        public int ReleaseYear { get; set; }
        
        public int Length { get; set; }
    }
}
