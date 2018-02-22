using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MFSL.Services;
using System.Threading.Tasks;
using MFSL.Helpers;
using MFSL.Models;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MFSL.Controllers
{
    /// <summary>
    /// Controller Methods:
    /// 1. SysUser - Returns Login View
    /// 2. SysUser[Post] - Posts login details to web api
    /// </summary>
    public class LoginController : Controller
    {
        ApiServices _apiServices = new ApiServices();
        HttpClient client,client2;

        //The URL of the WEB API Service
        string url = "http://localhost:64890/api/RolesAPI/";
        string url2 = "http://localhost:64890/api/Account/";
        public LoginController()
        {
            client = new HttpClient();
            client2 = new HttpClient();
            client.BaseAddress = new Uri(url);
            client2.BaseAddress = new Uri(url2);
            client.DefaultRequestHeaders.Accept.Clear();
            client2.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client2.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        /// <summary>
        /// Returns Login View
        /// </summary>
        /// <returns>User Login View</returns>
        public ActionResult SysUser()
        {
            return View();
        }

        /// <summary>
        /// Posts login details to web api
        /// </summary>
        /// <param name="user">User</param>
        /// <returns>Access Token</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SysUser([Bind(Include = "Username,Password")] User user)
        {
            var keyValues = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("username", user.Username),
                new KeyValuePair<string, string>("password", user.Password),
                new KeyValuePair<string, string>("grant_type", "password")
            };
            //Call to web api
            var request = new HttpRequestMessage(
                HttpMethod.Post, Constants.BaseApiAddress + "Token");

            request.Content = new FormUrlEncodedContent(keyValues);

            var client = new HttpClient();
            try
            {
                //call to web api service
                var response = await client.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();
                JObject jwtDynamic = JsonConvert.DeserializeObject<dynamic>(content);
                var accessTokenExpiration = jwtDynamic.Value<DateTime>(".expires");
                Settings.AccessToken = jwtDynamic.Value<string>("access_token");
                Settings.AccessTokenExpirationDate = accessTokenExpiration;
                //Debug.WriteLine(accessTokenExpiration);
                //Debug.WriteLine(content);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                return null;
            }
            if (Settings.AccessToken != "")
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Settings.AccessToken);
                client2.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Settings.AccessToken);
                HttpResponseMessage responseMsg = await client2.GetAsync(url2+ "GetDetailsForUser");
                if (responseMsg.IsSuccessStatusCode)
                {
                    var responseData = responseMsg.Content.ReadAsStringAsync().Result;

                    var data = JsonConvert.DeserializeObject<Officers>(responseData);
                    Settings.UserFirstName = data.EmpFname;
                    Settings.UserMidName = data.EmpMname;
                    Settings.UserLastName = data.EmpLname;
                    Settings.VNPFNo = data.VnpfNo;
                    Settings.LoanNo = data.LoanNo;
                    Settings.DateRegistered = data.DateCreated;
                }

                HttpResponseMessage responseMessage = await client.GetAsync(url + "GetRoleForThisUser");
                if (responseMessage.IsSuccessStatusCode)
                {
                    var responseData = responseMessage.Content.ReadAsStringAsync().Result;

                    Settings.RoleForThisUser = JsonConvert.DeserializeObject<string>(responseData);
                }
                return RedirectToAction("Dashboard", "MemberFiles");
            }
            else
            {
                ViewBag.LoginErrorMessage = "The Password or username is incorrect! Please try again.";
            }
            
            return View();
        }
    }
}