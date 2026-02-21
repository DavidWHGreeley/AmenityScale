using AmenityScaleCore.Data;
using AmenityScaleCore.Models.AmenitiesInRadius;
using AmenityScaleWeb.Services;
using System.Collections.Generic;
using System.Web.Http;

/// <summary>
/// Version         Date        Coder                   Remarks
/// 0.1             2026-02-16  Patrick                 Initial get amenties
/// 0.2             26-02-16    Cody                    calculate Score.
/// 0.3             26-02-20    Greeley                 Changed lat lng radius to use a DTO. 
/// 


namespace AmenityScaleWeb.Controllers
{
    public class AmenitiesController : ApiController
    {
        private readonly AmenityDataAccess _amenityDataAccess = new AmenityDataAccess();

        [HttpGet]
        // Set web address that will run theGetAmenitiesInRadius function when loaded
        [Route("api/GetAmenitiesInRadius")]
        // Origin coordinates are inputs, as well as radius if we want to have variable search area sizes
        public IHttpActionResult GetAmenitiesInRadius([FromUri] userLocationRequestDTO userLocation)
        {
            // Get amenities within radius
            var nearbyAmenities = _amenityDataAccess.GetAmenitiesInRadius(userLocation);

            // Early out if there is no Amenities. 
            if (nearbyAmenities.Count == 0)
            {
                return Ok(new { amenities = new List<object>(), score = 0 });
            }

            var score = CalculateScore.CalcuateAmenityScore(nearbyAmenities, userLocation.radius);

            return Ok(new { amenities = nearbyAmenities, score });

        }
    }
}
