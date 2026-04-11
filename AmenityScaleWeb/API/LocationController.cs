using AmenityScaleCore.Data;
using AmenityScaleCore.Models.Location;
using System.Web.Http;

namespace AmenityScaleWeb.Controllers
{
    public class LocationController : ApiController
    {
        private readonly LocationDataAccess _locationDataAccess = new LocationDataAccess();

        [HttpGet]
        [Route("api/locations")]
        public IHttpActionResult GetLocations()
        {
            var locations = _locationDataAccess.ReadAll();
            return Ok(locations);
        }

        [HttpPost]
        [Route("api/locations")]
        public IHttpActionResult CreateLocation([FromBody] LocationDTO dto)
        {
            var locationId = _locationDataAccess.Create(dto);
            return Ok(locationId);
        }

        [HttpPut]
        [Route("api/locations")]
        public IHttpActionResult UpdateLocation([FromBody] LocationDTO dto)
        {
            _locationDataAccess.Update(dto);
            return Ok();
        }

        [HttpDelete]
        [Route("api/locations/{id}")]
        public IHttpActionResult DeleteLocation(int id)
        {
            _locationDataAccess.Delete(id);
            return Ok();
        }
    }
}