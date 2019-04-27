using System.Web.Mvc;
using System.Web.Routing;

namespace DeliverySchedule
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: null,
                url: "deliveryschedule/f3/fileupload",
                defaults: new { controller = "F3", action = "FileUpload" }
            );

            routes.MapRoute(
                name: null,
                url: "deliveryschedule/f3/corr",
                defaults: new { controller = "F3", action = "Corr" }
            );

            routes.MapRoute(
                name: null,
                url: "deliveryschedule/f3",
                defaults: new { controller = "F3", action = "Index" }
            );

            routes.MapRoute(
                name: null,
                url: "{*pathInfo}",
                defaults: new { controller = "Home", action = "Index" }
            );
        }
    }
}
