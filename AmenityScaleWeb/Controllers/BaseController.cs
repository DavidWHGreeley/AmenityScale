using System;
using System.Web.Mvc;
/// <summary>
/// Version         Date        Coder                   Remarks
/// 0.1             2026-21-02 Greeley                  Use Environment Variables
///
namespace AmenityScaleWeb.Controllers
{
    public class BaseController : Controller
    {
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // If your NOT using Windows Environment Variable, Edit the string after the ?? - "Your_Key_Here"
            ViewBag.GoogleMapsKey = Environment.GetEnvironmentVariable("GOOGLE_MAPS_API_KEY") ?? "Your_Key_Here";
            base.OnActionExecuting(filterContext);
        }
    }
}