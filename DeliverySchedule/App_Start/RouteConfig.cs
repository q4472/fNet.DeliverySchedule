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
                url: "deliveryschedule/f3/save2",
                defaults: new { controller = "F3", action = "Save2" }
            );

            routes.MapRoute(
                name: null,
                url: "deliveryschedule/f3/save",
                defaults: new { controller = "F3", action = "Save" }
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
