using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using MFSL.Models;
using MFSL.ViewModels;
using System.IO;
using PagedList;
using MFSL.Services;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.Collections.Generic;
using MFSL.Helpers;
using System.Net;
using System.Diagnostics;

namespace MFSL.Controllers
{
    public class UpdateFilesController : Controller
    {
        ApiServices _apiServices = new ApiServices();
        HttpClient client;

        //Web API Web URL
        string url = "http://localhost:64890/api/MemberFilesAPI/";
        string url2 = "http://localhost:64890/api/UpdateFilesAPI/";

        public UpdateFilesController()
        {
            client = new HttpClient();
            client.BaseAddress = new Uri(url);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Settings.AccessToken);
        }

        /// <summary>
        /// Return Modal view for displaying customer info
        /// </summary>
        /// <returns>Customer Info View</returns>
        public ActionResult RenderInfoView()
        {
            if (Settings.AccessToken == "" || DateTime.UtcNow.AddHours(1) > Settings.AccessTokenExpirationDate)
            {
                return RedirectToAction("SignOut", "Logout");
            }
            return PartialView("_Info2");
        }

        public ActionResult LoadRefundPanel()
        {
            if (Settings.AccessToken == "" || DateTime.UtcNow.AddHours(1) > Settings.AccessTokenExpirationDate)
            {
                return RedirectToAction("SignOut", "Logout");
            }
            return PartialView("_Refund");
        }

        public ActionResult LoadMaintenancePanel()
        {
            if (Settings.AccessToken == "" || DateTime.UtcNow.AddHours(1) > Settings.AccessTokenExpirationDate)
            {
                return RedirectToAction("SignOut", "Logout");
            }
            return PartialView("_Maintenance");
        }

        public async Task<ActionResult> GetFileByFileNo(int? fileNo)
        {
            if (fileNo != null)
            {
                HttpResponseMessage replyMsg = await client.GetAsync(url2 + "CheckFileProgress/" + fileNo);
                var respnse = replyMsg.Content.ReadAsStringAsync().Result;
                var isSuccess = JsonConvert.DeserializeObject<bool>(respnse);
                if (isSuccess)
                {
                    var obj = new FileReferences();
                    ViewBag.FileStatuses = "";
                    HttpResponseMessage responseMessage = await client.GetAsync(url + "GetFileRefByFileNo/" + fileNo);
                    HttpResponseMessage responseMsg = await client.GetAsync(url2 + "/GetAllFileStatus");
                    if (responseMessage.IsSuccessStatusCode)
                    {
                        var responseData = responseMessage.Content.ReadAsStringAsync().Result;
                        var list = JsonConvert.DeserializeObject<List<FileReferences>>(responseData);
                        if (list != null)
                        {
                            foreach (var i in list)
                            {
                                obj = i;
                            }
                        }
                        else
                        {
                            return PartialView("_NoResultsFound");
                        }
                    }
                    if (responseMsg.IsSuccessStatusCode)
                    {
                        var responseData2 = responseMsg.Content.ReadAsStringAsync().Result;
                        ViewBag.FileStatuses = new SelectList(JsonConvert.DeserializeObject<List<string>>(responseData2));
                    }

                    UpdateFileStatusViewModel file = new UpdateFileStatusViewModel()
                    {
                        FileNo = obj.FileNo,
                        Officer = obj.Officer,
                        MemberNo = obj.MemberNo,
                        Comment = obj.Comment
                    };
                    return PartialView("_EditFileStatus", file);
                }
                ViewBag.Status = "Pending";
                return PartialView("_NotAllowed");
            }
            ViewBag.Status = "error";
            return PartialView("_NoResultsFound");
        }

        /// <summary>
        /// Returns File Status Update View
        /// </summary>
        /// <returns></returns>
        public ActionResult UpdateFileStatus()
        {
            if (Settings.AccessToken == "" || DateTime.UtcNow.AddHours(1) > Settings.AccessTokenExpirationDate)
            {
                return RedirectToAction("SignOut", "Logout");
            }
            ViewBag.FileStatus = "";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UpdateFileStatus([Bind(Include = "FileNo,Officer,MemberNo,FileStatus,Comment")] UpdateFileStatusViewModel file)
        {
            if (ModelState.IsValid)
            {
                FileReferences model = new FileReferences()
                {
                    FileNo = file.FileNo,
                    FileStatus = file.FileStatus,
                    Comment = file.Comment
                };

                var json = JsonConvert.SerializeObject(model);
                HttpContent content = new StringContent(json);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                try
                {
                    var response = await client.PutAsync(url2 + "UpdateFileStatus", content);
                    if (response.IsSuccessStatusCode)
                    {
                        ModelState.Clear();
                        ViewBag.Confirmation = 1;
                        return View();
                    }
                    Debug.WriteLine(response);
                }
                catch (HttpRequestException e)
                {
                    Debug.WriteLine("\nException Caught!");
                    Debug.WriteLine("Message :{0} ", e.Message);
                }

            }

            ViewBag.Status = "error";
            return View();
        }
    }
}