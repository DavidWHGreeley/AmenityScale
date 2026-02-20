using AmenityScaleCore.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace AmenityScaleWeb.Controllers
{
    public class AmenitiesController : ApiController
    {
        private readonly AmenityDataAccess _amenityDataAccess = new AmenityDataAccess();

        [HttpGet]
        // Set web address that will run theGetAmenitiesInRadius function when loaded
        [Route("api/NearbyAmenities")]
        // Origin coordinates are inputs, as well as radius if we want to have variable search area sizes
        public IHttpActionResult GetAmenitiesInRadius(decimal lat, decimal lng, decimal radius)
        {
            // Get amenities within radius
            var nearbyAmenities = _amenityDataAccess.GetAmenitiesInRadius(lat, lng, radius);
            return Ok(nearbyAmenities);
        }
    }
}
