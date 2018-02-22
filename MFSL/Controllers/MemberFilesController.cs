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

namespace MFSL.Controllers
{
    /// <summary>
    /// Controller Methods:
    /// 1. RenderInfoView - Renders brief info about customer
    /// 2. Dashboard - Renders Dashboard of the system
    /// 3. Recent - Renders history of files created by an officer
    /// 4. FetchFile - Fetch a particular file based on number and file type
    /// 5. MyFiles - Search facility for an officer's files
    /// 6. SearchFiles - Search facility for all customer files
    /// 7. GetMemberInfoByNum - Gets member details by member number
    /// 8. NewFile - Create new file for member
    /// </summary>
    public class MemberFilesController : Controller
    {

        ApiServices _apiServices = new ApiServices();
        HttpClient client;

        //The URL of the WEB API Service
        string url = "http://localhost:64890/api/MemberFilesAPI";

        public MemberFilesController()
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
            if (Settings.AccessToken == "")
            {
                return RedirectToAction("SignOut", "Logout");
            }
            return PartialView("_Info");
        }
        /// <summary>
        /// Returns Dashboard View
        /// </summary>
        /// <returns>Dashboard View</returns>
        public ActionResult Dashboard()
        {
            if (Settings.AccessToken == "")
            {
                return RedirectToAction("SignOut", "Logout");
            }
            else if (DateTime.UtcNow.AddHours(1) > Settings.AccessTokenExpirationDate)
            {
                return RedirectToAction("SignOut", "Logout");
            }
            return View();
        }
        /// <summary>
        /// Returns View For File History for Officer
        /// </summary>
        /// <returns>History of files created by user</returns>
        public ActionResult Recent()
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
        /// <summary>
        /// Calls web api to return a particular file
        /// </summary>
        /// <param name="id">File Number</param>
        /// <param name="flag">Flag represents file type</param>
        /// <returns>PDF File</returns>
        public async Task<ActionResult> FetchFile(string id, string flag)
        {
            if (Settings.AccessToken == "")
            {
                return RedirectToAction("SignOut", "Logout");
            }
            else if (DateTime.UtcNow.AddSeconds(10) > Settings.AccessTokenExpirationDate)
            {
                return RedirectToAction("SignOut", "Logout");
            }
            HttpResponseMessage responseMsg = await client.GetAsync(url + "/FetchFile?id="+ id + "&flag=" + flag);
            if (responseMsg.IsSuccessStatusCode)
            {
                try
                {
                    var resData = responseMsg.Content.ReadAsByteArrayAsync().Result;
                    return File(new MemoryStream(resData), "application/pdf");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            return RedirectToAction("Error", "NotFound");

        }//End of FetchFile Method

        /// <summary>
        /// Search facility to invoke web api to return files for officer
        /// Search results are filtered into pages
        /// </summary>
        /// <param name="memberNo"> Member Number</param>
        /// <param name="page">Page number</param>
        /// <returns>Member File Details</returns>
        public async Task<ActionResult> MyFiles(string memberNo, int? page)
        {
            if (Settings.AccessToken == "")
            {
                return RedirectToAction("SignOut", "Logout");
            }
            else if (DateTime.UtcNow.AddSeconds(10) > Settings.AccessTokenExpirationDate)
            {
                return RedirectToAction("SignOut", "Logout");
            }
            int pageSize = 5;
            int pageIndex = 1;
            pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;

            if (!String.IsNullOrEmpty(memberNo))
            {
                ViewBag.MemberNo = memberNo;
                int id = 0;
                bool isValid = Int32.TryParse(memberNo, out id);
                HttpResponseMessage responseMsg = await client.GetAsync(url + "/GetMyFileByMemberNo/" + id);
                if (responseMsg.IsSuccessStatusCode)
                {
                    var resData = responseMsg.Content.ReadAsStringAsync().Result;
                    var data = JsonConvert.DeserializeObject<List<FileReferences>>(resData);
                    ViewBag.TotalFiles = data.Count;
                    IPagedList<FileReferences> filteredList = (data).ToPagedList(pageIndex, pageSize);
                    return PartialView("_MyFileDetails", filteredList);
                }
            }

            HttpResponseMessage responseMessage = await client.GetAsync(url + "/GetFileForUser");
            if (responseMessage.IsSuccessStatusCode)
            {
                var responseData = responseMessage.Content.ReadAsStringAsync().Result;
                var fileData = JsonConvert.DeserializeObject<List<FileReferences>>(responseData);
                IPagedList<FileReferences> files = (fileData).ToPagedList(pageIndex, pageSize);
                ViewBag.TotalFiles = fileData.Count;
                return PartialView("_MyFileDetails", files);
            }
            return View("Error");
        }

        /// <summary>
        /// Search facility to search for any member's files
        /// Search result is filtered into pages
        /// </summary>
        /// <param name="memberNo">Member number</param>
        /// <param name="page">Page number</param>
        /// <returns>Member File Details</returns>
        [AcceptVerbs(HttpVerbs.Get)]
        public async Task<ActionResult> SearchFiles(string memberNo, int? page)
        {
            if (Settings.AccessToken == "")
            {
                return RedirectToAction("SignOut", "Logout");
            }
            else if (DateTime.UtcNow.AddSeconds(10) > Settings.AccessTokenExpirationDate)
            {
                return RedirectToAction("SignOut", "Logout");
            }
            int pageSize = 5;
            int pageIndex = 1;
            pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;

            if (!String.IsNullOrEmpty(memberNo))
            {
                ViewBag.MemberNo = memberNo;
                int id = 0;
                bool isValid = Int32.TryParse(memberNo, out id);
                HttpResponseMessage responseMsg = await client.GetAsync(url + "/GetFileByMemberNo/" + id);
                if (responseMsg.IsSuccessStatusCode)
                {
                    var resData = responseMsg.Content.ReadAsStringAsync().Result;
                    var data = JsonConvert.DeserializeObject<List<FileReferences>>(resData);
                    ViewBag.TotalFiles = data.Count;
                    IPagedList<FileReferences> filteredList = (data).ToPagedList(pageIndex, pageSize);
                    return PartialView("_FileDetails", filteredList);
                }
            }

            HttpResponseMessage responseMessage = await client.GetAsync(url + "/GetAll");
            if (responseMessage.IsSuccessStatusCode)
            {
                var responseData = responseMessage.Content.ReadAsStringAsync().Result;
                var fileData = JsonConvert.DeserializeObject<List<FileReferences>>(responseData);
                ViewBag.TotalFiles = fileData.Count;
                IPagedList<FileReferences> files = (fileData).ToPagedList(pageIndex, pageSize);
                return PartialView("_FileDetails", files);
            }
            return View("Error");
        }

        /// <summary>
        /// Call Web API that returns customer's details
        /// </summary>
        /// <param name="MemberNum">Member number</param>
        /// <returns>Customer Details</returns>
        [AcceptVerbs(HttpVerbs.Get)]
        public async Task<ActionResult> GetMemberInfoByNum(string MemberNum)
        {
            if (Settings.AccessToken == "")
            {
                return RedirectToAction("SignOut", "Logout");
            }
            else if (DateTime.UtcNow.AddSeconds(10) > Settings.AccessTokenExpirationDate)
            {
                return RedirectToAction("SignOut", "Logout");
            }
            int id = 0;
            bool isValid = Int32.TryParse(MemberNum, out id);

            // var member = db.vnpf_.Where(s => s.VNPF_Number == id);
            HttpResponseMessage responseMessage = await client.GetAsync(url + "/GetMemberInfoByNo/" + id);
            if (responseMessage.IsSuccessStatusCode)
            {
                var responseData = responseMessage.Content.ReadAsStringAsync().Result;
                var fileData = JsonConvert.DeserializeObject<IEnumerable<vnpf_>>(responseData);
                return PartialView("_MemberInfo", fileData);
            }

            return View("Error");
        }
        /// <summary>
        /// Returns Form for creating a new file for customer
        /// </summary>
        /// <returns>New File Creation Form</returns>
        public ActionResult NewFile()
        {
            if(Settings.AccessToken == "")
            {
                return RedirectToAction("SignOut", "Logout");
            }
            else if (DateTime.UtcNow.AddSeconds(10) > Settings.AccessTokenExpirationDate)
            {
                return RedirectToAction("SignOut", "Logout");
            }
            return View();
        }

        /// <summary>
        /// Post method for New Customer File
        /// </summary>
        /// <param name="File">FileViewModel</param>
        /// <returns>HTTP Status Message</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> NewFile(
            [Bind(Include = "MemberNo,LoanApplication,OfferLetter," +
            "LoanAgreement,AcceptanceOffer,GuaranteeCertificate," +
            "Amortisation,ChequeCopy,Eligibility,Quotation,Payslip," +
            "LoanStatement,VNPFStatement,Other,FStatusId,")] FileViewModel File)

        {
            if (ModelState.IsValid)
            {
                var LApp = new MemoryStream(); var OL = new MemoryStream(); var LAgrmt = new MemoryStream();
                var AO = new MemoryStream(); var GC = new MemoryStream(); var Amo = new MemoryStream();
                var CC = new MemoryStream(); var Elig = new MemoryStream(); var Quote = new MemoryStream();
                var PS = new MemoryStream(); var LS = new MemoryStream(); var VS = new MemoryStream();
                var Other = new MemoryStream();

                File.LoanApplication.InputStream.CopyTo(LApp); File.OfferLetter.InputStream.CopyTo(OL); File.LoanAgreement.InputStream.CopyTo(LAgrmt);
                File.AcceptanceOffer.InputStream.CopyTo(AO); File.GuaranteeCertificate.InputStream.CopyTo(GC);
                File.Amortisation.InputStream.CopyTo(Amo); File.ChequeCopy.InputStream.CopyTo(CC);
                File.Eligibility.InputStream.CopyTo(Elig); File.Quotation.InputStream.CopyTo(Quote);
                File.Payslip.InputStream.CopyTo(PS); File.LoanStatement.InputStream.CopyTo(LS);
                File.VNPFStatement.InputStream.CopyTo(VS); File.Other.InputStream.CopyTo(Other);
                ViewBag.MemberNo = File.MemberNo;
                var NewMemberFile = new MemberFile
                {
                    DateCreated = System.DateTime.Now,
                    MemberNo = File.MemberNo,
                    LoanApplication = LApp.ToArray(),
                    OfferLetter = OL.ToArray(),
                    LoanAgreement = LAgrmt.ToArray(),
                    AcceptanceOffer = AO.ToArray(),
                    GuaranteeCertificate = GC.ToArray(),
                    Amortisation = Amo.ToArray(),
                    ChequeCopy = CC.ToArray(),
                    Eligibility = Elig.ToArray(),
                    Quotation = Quote.ToArray(),
                    Payslip = PS.ToArray(),
                    LoanStatement = LS.ToArray(),
                    VNPFStatement = VS.ToArray(),
                    Other = Other.ToArray(),
                    FStatusId = 1
                };

                var isSuccess = await _apiServices.CreateNewFile(NewMemberFile);

                if (!isSuccess)
                {
                    ErrorController err = new ErrorController();
                    err.CouldNotCreateFile();
                }
                else
                {
                    var isSuccess2 = await _apiServices.CreateNewRef(NewMemberFile);

                    return RedirectToAction("Recent");
                }
            }
            return View();
        }
    }
}
