using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace DNBase.Models
{
    public class AlbumModel
    {
        [Key]
        public int AlbumID { get; set; }
        public string AlbumName { get; set; }
        public int ArtistID { get; set; }
        public ArtistModel Artist { get; set; }

        public ICollection<TrackModel> Tracks { get; set; }

    }
}
