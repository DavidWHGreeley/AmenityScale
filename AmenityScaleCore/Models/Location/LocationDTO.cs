/// <summary>
/// Version         Date        Coder                   Remarks
/// 0.1             2015-26-01  Greeley & Patrick       Connected to Database using similar logic to Clays DataAccess Project.
/// 0.1.1           2026-12-03  Cody                    Added LocationWithScore for saving

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AmenityScaleCore.Data;

namespace AmenityScaleCore.Models.Location
{
    public class LocationDTO
    {
        public int LocationID { get; set; }
        public string LocationName { get; set; }
        public string StreetNumber { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public int SubdivisionID { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }

        // "Point" or "WKT"
        public string GeometryType { get; set; }
        public string LocationWKT { get; set; }
    }

    public class LocationWithScoreDTO : LocationDTO
    {
        public double CalculatedScore { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
