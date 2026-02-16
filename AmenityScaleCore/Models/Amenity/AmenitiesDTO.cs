/// <summary>
/// Version         Date        Coder                   Remarks
/// 0.1             2015-26-01  Greeley                 Init DTO
/// 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmenityScaleCore.Models.Amenity
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

        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }

        // "Point" or "WKT"
        public string GeometryType { get; set; }
        public string LocationWKT { get; set; }
    }

}
