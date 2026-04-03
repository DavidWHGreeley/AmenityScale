/// <summary>
/// Version         Date        Coder           Remarks
/// 0.1             2026-01-04  Greeley         Created all the battle APIs

using AmenityScaleCore.Data;
using AmenityScaleCore.Models.Battle;
using System;
using System.Web.Http;

namespace AmenityScaleWeb.API
{
    public class BattleController : ApiController
    {
        private readonly BattleDataAccess _battleDataAccess = new BattleDataAccess();


        [HttpPost]
        [Route("api/battles")]
        public IHttpActionResult CreateBattle([FromBody] int userID)
        {

            var battle = _battleDataAccess.CreateBattle(userID, DateTime.Now.AddHours(24));
            var shareUrl = $"{Request.RequestUri.GetLeftPart(UriPartial.Authority)}?code={battle.BattleCode}";

            return Ok(new { battle, shareUrl });
        }

        [HttpGet]
        [Route("api/battles/{code}")]
        public IHttpActionResult GetBattle(Guid code)
        {
            var battle = _battleDataAccess.GetByCode(code);

            if (battle == null)
                return NotFound();

            if (battle.Status != "open" || battle.ExpiresAt < DateTime.Now)
                return BadRequest("Battle is no longer active.");

            return Ok(battle);
        }

        [HttpPost]
        [Route("api/battles/{code}/join")]
        public IHttpActionResult JoinBattle(Guid code, [FromBody] BattleJoinRequestDTO request)
        {
            if (request == null)
                return BadRequest("Request body is required.");

            var leaderboard = _battleDataAccess.JoinBattle(code, request.UserID, request.LocationID);
            return Ok(leaderboard);
        }

        [HttpGet]
        [Route("api/battles/{code}/leaderboard")]
        public IHttpActionResult GetLeaderboard(Guid code)
        {
            var leaderboard = _battleDataAccess.GetLeaderboard(code);
            return Ok(leaderboard);
        }
    }
}