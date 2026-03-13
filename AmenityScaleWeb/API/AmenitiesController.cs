using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Results;
using AmenityScaleCore.Data;
using AmenityScaleCore.Models.AmenitiesInRadius;
using AmenityScaleCore.Models.Location;
using AmenityScaleWeb.Services;

/// <summary>
/// Version         Date        Coder                   Remarks
/// 0.1             2026-02-16  Patrick                 Initial get amenties
/// 0.2             26-02-16    Cody                    calculate Score.
/// 0.3             26-02-20    Greeley                 Changed lat lng radius to use a DTO. 
/// 0.4             2026-02-16  Patrick                 Updated for isochrones


namespace AmenityScaleWeb.Controllers
{
    public class AmenitiesController : ApiController
    {
        private readonly AmenityDataAccess _amenityDataAccess = new AmenityDataAccess();

        // Helps organize input WKT strings
        public class WKTRequest
        {
            public string wkt1 { get; set; }
            public string wkt2 { get; set; }
            public string wkt3 { get; set; }
            public string wkt4 { get; set; }
        }

        [HttpPost]
        // Set web address that will run the GetFullNeighborhoodScore function when loaded
        [Route("api/GetFullNeighborhoodScore")]
        public IHttpActionResult GetFullNeighborhoodScore([FromBody] WKTRequest request)
        {
            if (request == null) return BadRequest("Bad request");

            // Key:Value pairs are RingNumber:Amenities
            var rings = new Dictionary<int, List<AmenitiesInRadiusDTO>>();

            // Function to add a "ring"
            void AddRing(int minutes, string wkt)
            {
                if (!string.IsNullOrEmpty(wkt) && wkt.StartsWith("POLYGON"))
                {
                    rings.Add(minutes, _amenityDataAccess.GetAmenitiesInIsochrone(wkt));
                }
                else
                {
                    // Add empty list of amenities
                    rings.Add(minutes, new List<AmenitiesInRadiusDTO>());
                }
            }

            AddRing(1, request.wkt1);
            AddRing(2, request.wkt2);
            AddRing(3, request.wkt3);
            AddRing(4, request.wkt4);

            double totalScore = CalculateScore.CalculateTotalScore(rings);

            // Remove duplicate amenities between rings
            var uniqueAmenities = new List<AmenitiesInRadiusDTO>();
            var processedIds = new HashSet<int>();

            foreach (var ring in rings.Values)
            {
                foreach (var a in ring)
                {
                    // If the amenity ID can be added into the hash set, add the amenity to the output list
                    if (processedIds.Add(a.AmenityID)) uniqueAmenities.Add(a);
                }
            }

            // Round score to 2 decimal places
            return Ok(new { amenities = uniqueAmenities, totalScore = System.Math.Round(totalScore, 2) });
        }

        [HttpPost]
        [Route("api/SaveLocation")]
        public IHttpActionResult SaveLocation([FromBody] LocationWithScoreDTO request)
        {
            if (request == null) return BadRequest("Invalid Request");

            var location = new LocationWithScoreDTO
            {
                LocationName = request.LocationName,
                Latitude = (decimal?)request.Latitude,
                Longitude = (decimal?)request.Longitude,
                GeometryType = "POINT",
                LocationWKT = $"POINT({request.Longitude} {request.Latitude})",
                CalculatedScore = request.CalculatedScore,
                CreatedDate = DateTime.Now
            };

            // Save to database
            _amenityDataAccess.InsertLocation(location);

            return Ok(new { success = true });
        }

    }

}
