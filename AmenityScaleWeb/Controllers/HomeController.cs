using System.Web.Mvc;

namespace AmenityScaleWeb.Controllers
{
    public class HomeController : BaseController
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}