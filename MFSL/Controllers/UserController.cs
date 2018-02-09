using MFSL.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace MFSL.Controllers
{
    public class UserController : Controller
    {
        HttpClient client;
        HttpClient client2;
        string url = "http://localhost:64890/api/RolesAPI/";
        string url2 = "http://localhost:64890/api/Account/";
        public UserController()
        {
            client = new HttpClient();
            client2 = new HttpClient();
            client.BaseAddress = new Uri(url);
            client2.BaseAddress = new Uri(url2);
            client.DefaultRequestHeaders.Accept.Clear();
            client2.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client2.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Settings.AccessToken);
            client2.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Settings.AccessToken);
        }
        // GET: User
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult RenderUserFrstName()
        {
            if (Settings.AccessToken == "")
            {
                return RedirectToAction("SignOut", "Logout");
            }
            ViewBag.UserFirstname = Settings.UserFirstName;
            return PartialView("_UserLastName");
        }

        public string GetRoleForThisUser()
        {
            var role = Settings.RoleForThisUser;
            return role;
        }

        public ActionResult MyProfile()
        {
            ViewBag.FirstName = Settings.UserFirstName;
            ViewBag.MiddleName = Settings.UserMidName;
            ViewBag.LastName = Settings.UserLastName;
            ViewBag.VNPFNo = Settings.VNPFNo;
            ViewBag.LoanNo = Settings.LoanNo;
            ViewBag.DateRegistered = Settings.DateRegistered;
            ViewBag.Role = Settings.RoleForThisUser;
            return PartialView("_UserProfile");
        }
    }
}