using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MFSL.Helpers;

namespace MFSL.Controllers
{
    public class LogoutController : Controller
    {
        // GET: Logout
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult SignOut()
        {
            Settings.AccessToken = "";
            Settings.Username = "";
            Settings.Password = "";
            Settings.RoleForThisUser = "";
            Settings.UserLastName = "";
            return RedirectToAction("SysUser","Login");
        }
    }
}