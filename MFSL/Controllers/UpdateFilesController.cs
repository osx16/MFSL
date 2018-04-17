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
        string url3 = "http://localhost:64890/api/RefundsAPI/";

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
                int statusCode = JsonConvert.DeserializeObject<int>(respnse);
                if (statusCode == 404)
                {
                    return PartialView("_NoResultsFound");
                }
                else if (statusCode == 200)
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
                else if (statusCode == 423)
                {
                    ViewBag.Status = "pending";
                    return PartialView("_FileLocked");
                }
                else if(statusCode == 401)
                {
                    ViewBag.Status = "unauthorized";
                    return PartialView("_FileLocked");
                }
                ViewBag.Status = "error";
                return PartialView("_NoResultsFound");
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
        public async Task<ActionResult> UpdateFileStatus([Bind(Include = "FileNo,Officer,MemberNo,FileStatus,PaymentRequest," + 
        "LoanStatement,ReconciliationSheet,MaintenanceForm,Comment")] UpdateFileStatusViewModel file)
        {
            if (ModelState.IsValid)
            {
                if (file.PaymentRequest != null && file.LoanStatement != null && file.ReconciliationSheet != null)
                {
                    var PR = new MemoryStream();
                    var LS = new MemoryStream();
                    var RS = new MemoryStream();

                    file.PaymentRequest.InputStream.CopyTo(PR);
                    file.LoanStatement.InputStream.CopyTo(LS);
                    file.ReconciliationSheet.InputStream.CopyTo(RS);

                    RefundsBindingModel bindingModel = new RefundsBindingModel()
                    {
                        RequestDate = DateTime.Now,
                        FileNo = file.FileNo,
                        PaymentRequest = PR.ToArray(),
                        LoanStatement = LS.ToArray(),
                        ReconciliationSheet = RS.ToArray(),
                        FileStatus = file.FileStatus,
                        Comment = file.Comment
                    };

                    var jsonObj = JsonConvert.SerializeObject(bindingModel);
                    HttpContent httpContent = new StringContent(jsonObj);
                    httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    var resp = await client.PostAsync(url3+ "PostToRefunds", httpContent);

                    if (resp.IsSuccessStatusCode)
                    {
                        ModelState.Clear();
                        ViewBag.Confirmation = 1;
                        return View();                       
                    }
                }
                else if(file.MaintenanceForm != null)
                {
                    var M = new MemoryStream();
                    file.MaintenanceForm.InputStream.CopyTo(M);

                    MaintenanceBindingModel dto = new MaintenanceBindingModel()
                    {
                        FileNo = file.FileNo,
                        MaintenanceForm = M.ToArray(),
                        FileStatus = file.FileStatus,
                        Comment = file.Comment                      
                    };

                    var jsonObj = JsonConvert.SerializeObject(dto);
                    HttpContent httpContent = new StringContent(jsonObj);
                    httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    var resp = await client.PutAsync(url2 + "UpdateMaintenanceFile", httpContent);
                    if (resp.IsSuccessStatusCode)
                    {
                        ModelState.Clear();
                        ViewBag.Confirmation = 1;
                        return View();
                    }
                }

                FileReferences model = new FileReferences()
                {
                    FileNo = file.FileNo,
                    FileStatus = file.FileStatus,
                    Comment = file.Comment
                };

                var json = JsonConvert.SerializeObject(model);
                HttpContent content = new StringContent(json);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                var response = await client.PutAsync(url2 + "UpdateFileStatus", content);
                if (response.IsSuccessStatusCode)
                {
                    ModelState.Clear();
                    ViewBag.Confirmation = 1;
                    return View();
                }
            }

            ViewBag.Status = "error";
            return View();
        }
    }
}