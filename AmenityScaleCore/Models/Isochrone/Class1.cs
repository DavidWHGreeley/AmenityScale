namespace AmenityScaleCore.Models.Isochrone
{
    public class IsochroneDTO
    {
        public int IsochroneID { get; set; }
        public int LocationID { get; set; }
        public int TravelTime { get; set; }
        public string PolygonWKT { get; set; }
    }
}
