using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace AmenityScale.Models.Amenity
{
    public class AmenityCreateDTO
    {
        public string Name { get; set; }
        public int? CategoryID { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public int? SubdivisionID { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
    }
}
