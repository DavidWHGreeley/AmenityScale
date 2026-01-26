using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmenityScale.Models.Amenity
{
    public class AmenityDTO
    {
        public int AmenityID { get; set; }

        public string Name { get; set; }
        public string Street { get; set; }
        public string City { get; set; }

        public int CategoryID { get; set; }
        public string CategoryName { get; set; }

        public int SubdivisionID { get; set; }

        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }

        public string GeometryType { get; set; }   // "Point" or "WKT"
        public string LocationWKT { get; set; }
    }

    public class CategoryDTO
    {
        public int CategoryID { get; set; }
        public string Name { get; set; }
    }

    public class SubdivisionDTO
    {
        public int SubdivisionID { get; set; }
        public string Name { get; set; }
    }
}
