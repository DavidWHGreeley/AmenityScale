/// <summary>
/// Version         Date        Coder                   Remarks
/// 0.1             2026-04-11  Greeley                 Admin controller - serves the admin dashboard view
/// </summary>
/// 

using System.Web.Mvc;

namespace AmenityScaleWeb.Controllers
{
    public class AdminController : Controller
    {
        // GET: Admin
        public ActionResult Index()
        {
            return View();
        }
    }
}