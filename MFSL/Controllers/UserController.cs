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
    /// <summary>
    /// Controller Methods:
    /// 1. RenderUserFrstName - Gets Users First Name
    /// 2. GetRoleForThisUser - Get User's role
    /// 3. MyProfile - Gets User Profile
    /// </summary>
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
        /// <summary>
        /// Gets Users First Name to dispay in header bar
        /// </summary>
        /// <returns>User First Name</returns>
        public ActionResult RenderUserFrstName()
        {
            if (Settings.AccessToken == "" || DateTime.UtcNow.AddHours(1) > Settings.AccessTokenExpirationDate)
            {
                return RedirectToAction("SignOut", "Logout");
            }
            ViewBag.UserFirstname = Settings.UserFirstName;
            return PartialView("_UserLastName");
        }
        /// <summary>
        /// Gets User Role to dispay in MyProfile
        /// </summary>
        /// <returns>User Role</returns>
        public string GetRoleForThisUser()
        {
            var role = Settings.RoleForThisUser;
            return role;
        }
        /// <summary>
        /// Brief summary info about user
        /// </summary>
        /// <returns>Profile Info</returns>
        public ActionResult MyProfile()
        {
            if (Settings.AccessToken == "" || DateTime.UtcNow.AddHours(1) > Settings.AccessTokenExpirationDate)
            {
                return RedirectToAction("SignOut", "Logout");
            }

            ViewBag.Branch = Settings.BranchName;
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