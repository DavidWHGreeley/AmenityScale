using AmenityScaleCore.Models.Amenity;
/// <summary>
/// Version         Date        Coder                   Remarks
/// 0.1             2026-07-02  Greeley                 Response DTO from the database Get amenities in radius
/// 0.2             2026-20-02  Greeley                 Updated cause server is returning if it's negative amenity
/// 

namespace AmenityScaleCore.Models.AmenitiesInRadius
{
    public class AmenitiesInRadiusDTO : AmenityDTO
    {
        public double DistanceInMeters { get; set; }
        public int IsNegative { get; set; }
    }
}
