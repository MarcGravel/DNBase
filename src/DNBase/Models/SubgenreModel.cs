using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;


namespace DNBase.Models
{
    public class SubgenreModel
    {
        [Key]
        public int SubgenreID { get; set; }
        public string SubgenreName { get; set; }

        public ICollection<ArtistModel> Artists { get; set; }
    }
}
