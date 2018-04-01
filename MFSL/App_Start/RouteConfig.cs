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

            routes.MapRoute("UploadLoanApproval",
                "memberfiles/uploadloanapproval/",
                new { controller = "MemberFiles", action = "UploadLoanApproval" },
                new[] { "MFSL.Controllers" });

            routes.MapRoute("UploadPaymentAdvice",
                "memberfiles/uploadpaymentadvice/",
                new { controller = "MemberFiles", action = "UploadPaymentAdvice" },
                new[] { "MFSL.Controllers" });

            routes.MapRoute("UploadPaymentAndCheque",
                "memberfiles/uploadpaymentandcheque/",
                new { controller = "MemberFiles", action = "UploadPaymentAndCheque" },
                new[] { "MFSL.Controllers" });

            routes.MapRoute("UploadCollateral",
                "memberfiles/uploadcollateral/",
                new { controller = "MemberFiles", action = "UploadCollateral" },
                new[] { "MFSL.Controllers" });

            routes.MapRoute("SysUser",
                "login/sysuser/",
                new { controller = "Login", action = "SysUser" },
                new[] { "MFSL.Controllers" });
        }

    }
}
