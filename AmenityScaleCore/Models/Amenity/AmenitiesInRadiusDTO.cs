using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AmenityScaleCore.Models.Amenity;

namespace AmenityScaleCore.Models.Test
{
    public class AmenitiesInRadiusDTO : AmenityDTO
    {
        public double DistanceInMeters { get; set; }
    }
}
