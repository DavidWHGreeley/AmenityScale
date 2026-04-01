using AmenityScaleCore.Data;
using System.Web.Http;

namespace AmenityScaleWeb.API
{
    public class UsersController : ApiController
    {
        private readonly BattleDataAccess _battleDataAccess = new BattleDataAccess();
    }
}