/// <summary>
/// Version         Date        Coder                   Remarks
/// 0.1             2026-07-20  Greeley                 Class for data coming from the frontend
/// 


namespace AmenityScaleCore.Models.AmenitiesInRadius
{
    public class userLocationRequestDTO
    {
        public double lat { get; set; }
        public double lng { get; set; }
        public double radius { get; set; }
    }
}
