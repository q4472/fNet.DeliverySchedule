using System;
using System.Web.Mvc;

namespace DeliverySchedule.Controllers
{
    public class HomeController : Controller
    {
        public Object Index()
        {
            Object v = null;
            if (ControllerContext.HttpContext.IsDebuggingEnabled)
                v = View("~/Views/Home/Index.cshtml"); // _ViewStart.cshtml
            else
                v = PartialView("~/Views/Home/Index.cshtml");
            return v;
        }
    }
}