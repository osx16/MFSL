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
                defaults: new { controller = "Login", action = "SysUser", id = UrlParameter.Optional }
            );

            routes.MapRoute("GetMemberInfoByNum",
                "memberfiles/getmemberinfobynum/",
                new { controller = "MemberFiles", action = "GetMemberInfoByNum" },
                new[] { "MFSL.Controllers" });

            routes.MapRoute("GetFileRefsByFileNo",
                "memberfiles/getfilerefsbyfileno/",
                new { controller = "MemberFiles", action = "GetFileRefsByFileNo" },
                new[] { "MFSL.Controllers" });

            routes.MapRoute("SysUser",
                "login/sysuser/",
                new { controller = "Login", action = "SysUser" },
                new[] { "MFSL.Controllers" });
        }

    }
}
