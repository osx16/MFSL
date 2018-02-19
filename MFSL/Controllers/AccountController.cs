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
            if (Settings.AccessToken == "")
            {
                return RedirectToAction("SignOut", "Logout");
            }
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
            if (Settings.AccessToken == "")
            {
                return RedirectToAction("SignOut", "Logout");
            }

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

        public ActionResult ForgotPassword()
        {
            if (Settings.AccessToken == "")
            {
                return RedirectToAction("SignOut", "Logout");
            }
            return View();
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var json = JsonConvert.SerializeObject(model);
                HttpContent content = new StringContent(json);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                try
                {
                    var Response = await client.PostAsync(url + "ForgotPassword", content);
                    //Debug.WriteLine(response);
                    if (Response.IsSuccessStatusCode)
                    {
                        var responseData = Response.Content.ReadAsStringAsync().Result;
                        var ConfigInfo = JsonConvert.DeserializeObject<PasswordConfigModel>(responseData);
                        var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = ConfigInfo.UserId, code = ConfigInfo.Code }, protocol: Request.Url.Scheme);
                        try
                        {
                            EmailController email = new EmailController();
                            email.SendEmailNotification(callbackUrl, model.Email);
                        }
                        catch (Exception ex) { Debug.WriteLine(ex.Message); }
                        // return RedirectToAction("ForgotPasswordConfirmation", "Account");
                        return RedirectToAction("ForgotPasswordConfirmation");
                    }
                    else
                    {
                        return RedirectToAction("InternalServerError", "Error");
                    }
                }
                catch (HttpRequestException e)
                {
                    Debug.WriteLine("\nException Caught!");
                    Debug.WriteLine("Message :{0} ", e.Message);
                }

                //string UserEmail = model.Email;
                ////var response = await client.GetAsync(url + "ForgotPassword");
                //var response = await client.GetAsync(url + "GetPwdConfig/" + UserEmail);
                ////Debug.WriteLine(response);


                //// For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=320771
                //// Send an email with this link
                //// string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                //// var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);		
                //// await UserManager.SendEmailAsync(user.Id, "Reset Password", "Please reset your password by clicking <a href=\"" + callbackUrl + "\">here</a>");
                //// return RedirectToAction("ForgotPasswordConfirmation", "Account");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }
    }
}