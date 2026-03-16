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
/// 0.5             2026-03-14  Cody                    GetFullNeighbourhoodScore sending click data to db


namespace AmenityScaleWeb.Controllers
{
    public class AmenitiesController : ApiController
    {
        private readonly AmenityDataAccess _amenityDataAccess = new AmenityDataAccess();
        private readonly LocationDataAccess _locationDataAccess = new LocationDataAccess();

        // Helps organize input WKT strings
        public class WKTRequest
        {
            public string wkt1 { get; set; }
            public string wkt2 { get; set; }
            public string wkt3 { get; set; }
            public string wkt4 { get; set; }

            public double lat { get; set; }
            public double lng { get; set; }
            public string streetNumber { get; set; }
            public string street { get; set; }
            public string city { get; set; }
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
            var uniqueAmenities = rings.OrderBy(ring => ring.Key).SelectMany(ring => ring.Value).Distinct().ToList();

            // Save User Score to Location Table

            _locationDataAccess.Create(new LocationDTO
            {
                LocationName = $"{request.streetNumber} {request.street}",
                StreetNumber = request.streetNumber,
                Street = request.street,
                City = request.city,
                SubdivisionID = 1,
                Latitude = (decimal)request.lat,
                Longitude = (decimal)(request.lng),
                CalculatedScore = totalScore
            });
            // Round score to 2 decimal places
            return Ok(new { amenities = uniqueAmenities, totalScore = System.Math.Round(totalScore, 2) });
        }

    }

}
