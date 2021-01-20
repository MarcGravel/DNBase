using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DNBase.Models
{
    public class TrackModel
    {
        [Key]
        public int TrackID { get; set; }
        public string TrackName { get; set; }
        public int AlbumID { get; set; }
        public AlbumModel Album { get; set; }
        public ArtistModel Artist { get; set; }
    }
}
