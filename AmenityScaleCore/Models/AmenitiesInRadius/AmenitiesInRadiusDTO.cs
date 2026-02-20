using AmenityScaleCore.Models.Amenity;

namespace AmenityScaleCore.Models.AmenitiesInRadius
{
    public class AmenitiesInRadiusDTO : AmenityDTO
    {
        public double DistanceInMeters { get; set; }
        public int IsNegative { get; set; }
    }
}
