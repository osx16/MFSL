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
    /// <summary>
    /// Controller Methods:
    /// 1. ActiveUsers - Get list of active userse
    /// 2. Register - Register new user
    /// 3. ForgotPassword - For resetting new password for user
    /// 4. ForgotPasswordConfirmation - View to confirm password reset
    /// 5. ResetPassword - Reset Password View
    /// 6. ResetPasswordConfirmation - View for reset password confirmation
    /// 7. ResetPasswordError - Returns ResetPasswordError View
    /// </summary>
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

        public ActionResult AddNewMember()
        {
            if (Settings.AccessToken == "")
            {
                return RedirectToAction("SignOut", "Logout");
            }
            else if (DateTime.UtcNow.AddSeconds(10) > Settings.AccessTokenExpirationDate)
            {
                return RedirectToAction("SignOut", "Logout");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddNewMember([Bind(Include = "Member_Fullname,Loan_Number,VNPF_Number")] NewMemberViewModel model)
        {
            if (ModelState.IsValid)
            {
                var newMember = new vnpf_()
                {
                    Member_Fullname = model.Member_Fullname,
                    Loan_Number = model.Loan_Number,
                    VNPF_Number = model.VNPF_Number
                };

                var MyClient = new HttpClient();
                MyClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Settings.AccessToken);
                var json = JsonConvert.SerializeObject(newMember);
                HttpContent content = new StringContent(json);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                var response = await client.PostAsync(Constants.BaseApiAddress + "api/Account/PostMember", content);
                if (response.IsSuccessStatusCode)
                {
                    ModelState.Clear();
                    ViewBag.Confirmation = 1;
                    return View();
                }

            }
            ViewBag.Confirmation = 0;
            return View();
        }
        /// <summary>
        /// Gets list of active users of the system
        /// by calling web api service
        /// </summary>
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

        /// <summary>
        /// Returns New User Creation Form
        /// </summary>
        /// <returns>New User Creation Form</returns>
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
        /// <summary>
        /// Register Post Method
        /// </summary>
        /// <param name="model">RegisterBindingModel</param>
        /// <returns></returns>
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
        /// <summary>
        /// Returns ForgotPassword View
        /// </summary>
        /// <returns>Forgot Password View</returns>
        public ActionResult ForgotPassword()
        {
            if (Settings.AccessToken == "")
            {
                return RedirectToAction("SignOut", "Logout");
            }
            return View();
        }
        /// <summary>
        /// Forgot Password Post Method
        /// </summary>
        /// <param name="model">ForgotPasswordViewModel</param>
        /// <returns></returns>
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
                            string EmailBody = "Please reset your password by clicking this url: " + callbackUrl;
                            email.SendEmailNotification(EmailBody, model.Email);
                        }
                        catch (Exception ex) { Debug.WriteLine(ex.Message); return RedirectToAction("InternalServerError", "Error"); }
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
            }
            // If we got this far, something failed, redisplay form
            return View(model);
        }
        /// <summary>
        /// Returns Forgot Password Confirmation View
        /// </summary>
        /// <returns>Forgot Password Confirmation View</returns>
        public ActionResult ForgotPasswordConfirmation()
        {
            ViewBag.Seconds = System.DateTime.UtcNow.Second;
            return View();
        }

        /// <summary>
        /// Returns Reset Password View
        /// </summary>
        /// <param name="code"> user generated code for password reset</param>
        /// <returns></returns>
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            return code == null ? View("ResetPasswordError") : View();
        }

        /// <summary>
        /// Post method for reset password view
        /// </summary>
        /// <param name="model">ResetPasswordViewModel</param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var json = JsonConvert.SerializeObject(model);
                HttpContent content = new StringContent(json);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                try
                {
                    var Response = await client.PostAsync(url + "ResetPassword", content);
                    //Debug.WriteLine(response);
                    if (Response.IsSuccessStatusCode)
                    {
                        return RedirectToAction("ResetPasswordConfirmation", "Account");
                    }
                    else
                    {
                        return RedirectToAction("ResetPasswordError", "Account");
                    }
                }
                catch (HttpRequestException e)
                {
                    Debug.WriteLine("\nException Caught!");
                    Debug.WriteLine("Message :{0} ", e.Message);
                }
            }
            // If we got this far, something failed, redisplay form
            return View(model);
        }
        /// <summary>
        /// Returns password reset view
        /// </summary>
        /// <returns>ResetPasswordConfirmation view</returns>
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }
        /// <summary>
        /// Returns Reset Password Error View
        /// </summary>
        /// <returns>Reset Password Error View</returns>
        public ActionResult ResetPasswordError()
        {
            return View();
        }

    }
}