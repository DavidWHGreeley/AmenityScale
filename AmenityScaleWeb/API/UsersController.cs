using AmenityScaleCore.Data;
/// <summary>
/// Version         Date        Coder           Remarks
/// 0.1             2026-01-04  Greeley         Created user API 
/// 
using System.Web.Http;

namespace AmenityScaleWeb.API
{
    public class UsersController : ApiController
    {
        private readonly BattleDataAccess _battleDataAccess = new BattleDataAccess();

        [HttpPost]
        [Route("api/users")]
        public IHttpActionResult CreateUser([FromBody] string displayName)
        {
            if (string.IsNullOrWhiteSpace(displayName))
                return BadRequest("Display name is required.");

            var user = _battleDataAccess.CreateUser(displayName);
            return Ok(user);
        }
    }
}