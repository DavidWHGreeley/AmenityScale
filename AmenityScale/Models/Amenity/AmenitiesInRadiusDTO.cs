using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AmenityScale.Models.Amenity;

namespace AmenityScale.Models.Test
{
    public class AmenitiesInRadiusDTO : AmenityDTO
    {
        public double DistanceInMeters { get; set; }
    }
}
