using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MFSL.Helpers;

namespace MFSL.Controllers
{
    /// <summary>
    /// Controller Methods:
    /// 1. SignOut - Logs user out of system
    /// </summary>
    public class LogoutController : Controller
    {
        /// <summary>
        /// Disposes User credentials & configs then redirects to login page
        /// </summary>
        public ActionResult SignOut()
        {
            Settings.AccessToken = "";
            Settings.Username = "";
            Settings.Password = "";
            Settings.RoleForThisUser = "";
            Settings.UserFirstName = "";
            return RedirectToAction("SysUser","Login");
        }
    }
}