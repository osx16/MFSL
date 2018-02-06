using MFSL.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Mvc;

namespace MFSL.Controllers
{
    public class UsersController : Controller
    {
        HttpClient client;
        //The URL of the WEB API Service
        string url = "http://localhost:64890/api/Account/";
        public UsersController()
        {
            client = new HttpClient();
            client.BaseAddress = new Uri(url);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Settings.AccessToken);
        }
        // GET: Users
        public ActionResult ActiveUsers()
        {

            return View();
        }
    }
}