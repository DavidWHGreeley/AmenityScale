/// <summary>
/// Version         Date        Coder                   Remarks
/// 0.1             2026-03-21  Cody                    Isochrone Create and Read endpoints


using AmenityScaleCore.Data;
using System.Collections.Generic;
using System.Web.Http;

namespace AmenityScaleWeb.API
{
    public class IsochronesController : ApiController
    {
        private readonly IsochroneDataAccess _isochroneDataAccess = new IsochroneDataAccess();

        public class IsochroneRing
        {
            public int TravelTime { get; set; }
            public string Wkt { get; set; }
        }

        public class SaveIsochroneRequest
        {
            public int LocationID { get; set; }
            public List<IsochroneRing> Rings { get; set; }
        }

        [HttpPost]
        [Route("api/isochrones")]
        public IHttpActionResult Create([FromBody] SaveIsochroneRequest request)
        {
            if (request == null || request.Rings == null)
                return BadRequest("Request body is required.");

            foreach (var ring in request.Rings)
            {
                _isochroneDataAccess.Create(request.LocationID, ring.TravelTime, ring.Wkt);
            }

            return Ok(new { saved = request.Rings.Count });
        }

        [HttpGet]
        [Route("api/isochrones/{locationID}")]
        public IHttpActionResult Read(int locationID)
        {
            var isochrones = _isochroneDataAccess.Read(locationID);
            return Ok(isochrones);
        }
    }
}