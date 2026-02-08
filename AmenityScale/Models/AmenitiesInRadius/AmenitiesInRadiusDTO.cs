using AmenityScale.Models.Amenity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmenityScale.Models.AmenitiesInRadius
{
    public class AmenitiesInRadiusDTO : AmenityDTO
    {
        public double DistanceInMeters { get; set; }
    }
}
