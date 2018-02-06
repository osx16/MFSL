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
using MFSL.Models;
using System.Diagnostics;
using MFSL.ViewModels;

namespace MFSL.Controllers
{
    public class AccountController : Controller
    {
        HttpClient client;
        //The URL of the WEB API Service
        string url = "http://localhost:64890/api/Account/";
        public AccountController()
        {
            client = new HttpClient();
            client.BaseAddress = new Uri(url);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Settings.AccessToken);
        }
        // GET: Account
        public async Task<ActionResult> ActiveUsers()
        {
            HttpResponseMessage responseMessage = await client.GetAsync(url + "GetUsers");
            if (responseMessage.IsSuccessStatusCode)
            {
                var responseData = responseMessage.Content.ReadAsStringAsync().Result;
                var model = JsonConvert.DeserializeObject<IEnumerable<Officers>>(responseData);
                return View(model);
            }
            return View();
        }

        public async Task<ActionResult> Register()
        {

            HttpResponseMessage responseMessage = await client.GetAsync("http://localhost:64890/api/RolesAPI/GetRoles");
            if (responseMessage.IsSuccessStatusCode)
            {
                var responseData = responseMessage.Content.ReadAsStringAsync().Result;
                ViewBag.Roles = new SelectList(JsonConvert.DeserializeObject<List<string>>(responseData));
                return View();
            }
            return View("Error");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterBindingModel model)
        {
            if (ModelState.IsValid)
            {
                model.DateCreated = DateTime.Now;
                var json = JsonConvert.SerializeObject(model);
                HttpContent content = new StringContent(json);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                var response = await client.PostAsync(url + "Register", content);
                //Debug.WriteLine(response);
                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("ActiveUsers");
                }
                else
                {
                    return RedirectToAction("InternalServerError", "Error");
                }                   
            }
            return View();
        }
    }
}