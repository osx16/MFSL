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
        string url = "http://localhost:64890/api/RolesAPI/";
        public UserController()
        {
            client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
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

        //public async Task<string> GetRoleForUser()
        //{
        //    string role = "";
        //    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Settings.AccessToken);
        //    HttpResponseMessage responseMessage = await client.GetAsync(url + "GetRoleForThisUser");
        //    if (responseMessage.IsSuccessStatusCode)
        //    {
        //        var responseData = responseMessage.Content.ReadAsStringAsync().Result;

        //        role = JsonConvert.DeserializeObject<string>(responseData);
        //    }

        //    return role;
        //}

        public string GetRoleForThisUser()
        {
            var role = Settings.RoleForThisUser;
            return role;
        }
    }
}