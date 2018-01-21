using System.Web.Mvc;
using System.Web.Routing;

namespace MFSL
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );

            routes.MapRoute("GetMemberInfoByNum",
                "memberfiles/getmemberinfobynum/",
                new { controller = "MemberFiles", action = "GetMemberInfoByNum" },
                new[] { "MFSL.Controllers" });
        }
    }
}
