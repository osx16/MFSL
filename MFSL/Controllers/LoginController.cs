using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MFSL.Services;
using System.Threading.Tasks;
using MFSL.Helpers;
using MFSL.Models;

namespace MFSL.Controllers
{
    public class LoginController : Controller
    {
        ApiServices _apiServices = new ApiServices();
        // GET: Login
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult SysUser()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SysUser([Bind(Include = "Username,Password")] User user)
        {

            Settings.AccessToken = await _apiServices.LoginAsync(user.Username, user.Password);
            ViewBag.LoginErrorMessage = "";
            var AccessToken = Settings.AccessToken;

            if (AccessToken != "")
            {
                return RedirectToAction("Dashboard", "MemberFiles");
            }
            else
            {
                ViewBag.LoginErrorMessage = "The Password or username is incorrect! Please try again.";
            }
            
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult test(string username, string password)
        {
            return View();
        }
    }
}