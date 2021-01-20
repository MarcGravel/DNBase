using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace DNBase.Models
{
    public class ArtistModel
    {
        [Key]
        public int ArtistID { get; set; }
        public string ArtistName { get; set; }
        public string CountryOfOrigin { get; set; }
        public int SubgenreID { get; set; }
        public SubgenreModel Subgenre { get; set; }

        public ICollection<AlbumModel> Albums { get; set; }
    }
}
