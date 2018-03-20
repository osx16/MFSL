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
    /// <summary>
    /// Controller Methods:
    /// 1. RenderInfoView - Renders brief info about customer
    /// 2. Dashboard - Renders Dashboard of the system
    /// 3. Recent - Renders history of files created by an officer
    /// 4. FetchFile - Fetch a particular file based on number and file type
    /// 5. MyFiles - Search facility for an officer's files
    /// 6. SearchFiles - Search facility for all customer files
    /// 7. GetMemberInfoByNum - Gets member details by member number
    /// 8. GetFileRefsByFileNo - Gets file reference by file number
    /// 9. NewFile - Create new file for member
    /// </summary>
    public class MemberFilesController : Controller
    {

        ApiServices _apiServices = new ApiServices();
        HttpClient client;

        //The URL of the WEB API Service
        string url = "http://localhost:64890/api/MemberFilesAPI/";

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
            HttpResponseMessage responseMsg = await client.GetAsync(url + "FetchFile?id="+ id + "&flag=" + flag);
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
            int pageSize = 10;
            int pageIndex = 1;
            pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;

            if (!String.IsNullOrEmpty(memberNo))
            {
                ViewBag.MemberNo = memberNo;
                int id = 0;
                bool isValid = Int32.TryParse(memberNo, out id);
                HttpResponseMessage responseMsg = await client.GetAsync(url + "GetMyFileByMemberNo/" + id);
                if (responseMsg.IsSuccessStatusCode)
                {
                    var resData = responseMsg.Content.ReadAsStringAsync().Result;
                    var data = JsonConvert.DeserializeObject<List<FileReferences>>(resData);
                    ViewBag.TotalFiles = data.Count;
                    IPagedList<FileReferences> filteredList = (data).ToPagedList(pageIndex, pageSize);
                    return PartialView("_MyFileDetails", filteredList);
                }
            }

            HttpResponseMessage responseMessage = await client.GetAsync(url + "GetFileForUser");
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
                HttpResponseMessage responseMsg = await client.GetAsync(url + "GetFileByMemberNo/" + id);
                if (responseMsg.IsSuccessStatusCode)
                {
                    var resData = responseMsg.Content.ReadAsStringAsync().Result;
                    var data = JsonConvert.DeserializeObject<List<FileReferences>>(resData);
                    ViewBag.TotalFiles = data.Count;
                    IPagedList<FileReferences> filteredList = (data).ToPagedList(pageIndex, pageSize);
                    return PartialView("_FileDetails", filteredList);
                }
            }

            HttpResponseMessage responseMessage = await client.GetAsync(url + "GetAll");
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
            HttpResponseMessage responseMessage = await client.GetAsync(url + "GetMemberInfoByNo/" + id);
            if (responseMessage.IsSuccessStatusCode)
            {
                var responseData = responseMessage.Content.ReadAsStringAsync().Result;
                var memberInfo = JsonConvert.DeserializeObject<IEnumerable<vnpf_>>(responseData);
                return PartialView("_MemberInfo", memberInfo);
            }

            return View("Error");
        }

        /// <summary>
        /// Gets file reference by FileNo
        /// </summary>
        /// <param name="FileNo"></param>
        /// <returns></returns>
        [AcceptVerbs(HttpVerbs.Get)]
        public async Task<ActionResult> GetFileRefsByFileNo(int fileNo)
        {
            if(Settings.AccessToken == "")
            {
                return RedirectToAction("SignOut", "Logout");
            }
            else if(DateTime.UtcNow.AddSeconds(10) > Settings.AccessTokenExpirationDate)
            {
                return RedirectToAction("SignOut", "Logout");
            }

            HttpResponseMessage responseMessage = await client.GetAsync(url + "GetFileRefByFileNo/" + fileNo);
            if (responseMessage.IsSuccessStatusCode)
            {
                var responseData = responseMessage.Content.ReadAsStringAsync().Result;
                var fileRefs = JsonConvert.DeserializeObject<IEnumerable<FileReferences>>(responseData);
                ViewBag.FileNo = fileNo;
                ViewBag.FileStatus = "";
                ViewBag.EmployerType = "";
                foreach(var i in fileRefs)
                {
                    ViewBag.FileStatus = i.FileStatus;
                    ViewBag.EmployerType = i.EmployerType;
                }
                return PartialView("_UserFiles", fileRefs);
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
            [Bind(Include = "MemberNo,LoanApplication," +
            "LoanAgreement,GuaranteeCertificate," +
            "Amortisation,Eligibility,RequestLetter,EmployerLetter,Quotation,Payslip," +
            "BankAccStatment,LoanStatement,VNPFStatement,StandingOrder,CustomerID,FStatusId,")] FileViewModel File)

        {
            if (ModelState.IsValid)
            {
                var LApp = new MemoryStream();
                var LAgrmt = new MemoryStream();
                var GC = new MemoryStream();
                var Amo = new MemoryStream();
                var Elig = new MemoryStream();
                var RL = new MemoryStream();
                var EL = new MemoryStream();
                var Quote = new MemoryStream();
                var PS = new MemoryStream();
                var BS = new MemoryStream();
                var LS = new MemoryStream();
                var VS = new MemoryStream();
                var SO = new MemoryStream();
                var CID = new MemoryStream();

                File.LoanApplication.InputStream.CopyTo(LApp);
                File.LoanAgreement.InputStream.CopyTo(LAgrmt);
                File.GuaranteeCertificate.InputStream.CopyTo(GC);
                File.Amortisation.InputStream.CopyTo(Amo);
                File.Eligibility.InputStream.CopyTo(Elig);
                File.RequestLetter.InputStream.CopyTo(RL);
                File.EmployerLetter.InputStream.CopyTo(EL);
                File.Quotation.InputStream.CopyTo(Quote);
                File.Payslip.InputStream.CopyTo(PS);
                File.BankAccStatment.InputStream.CopyTo(BS);
                File.LoanStatement.InputStream.CopyTo(LS);
                File.VNPFStatement.InputStream.CopyTo(VS);
                File.StandingOrder.InputStream.CopyTo(SO);
                File.CustomerID.InputStream.CopyTo(CID);

                ViewBag.MemberNo = File.MemberNo;
                var NewMemberFile = new MemberFile
                {
                    DateCreated = System.DateTime.Now,
                    MemberNo = File.MemberNo,
                    LoanApplication = LApp.ToArray(),
                    LoanAgreement = LAgrmt.ToArray(),
                    GuaranteeCertificate = GC.ToArray(),
                    Amortisation = Amo.ToArray(),
                    Eligibility = Elig.ToArray(),
                    RequestLetter = RL.ToArray(),
                    EmployerLetter = EL.ToArray(),
                    Quotation = Quote.ToArray(),
                    Payslip = PS.ToArray(),
                    BankAccStatement = BS.ToArray(),
                    LoanStatement = LS.ToArray(),
                    VNPFStatement = VS.ToArray(),
                    StandingOrder = SO.ToArray(),
                    CustomerID = CID.ToArray(),
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
                    //var isSuccess2 = await _apiServices.CreateNewRef(NewMemberFile);

                    return RedirectToAction("Recent");
                }
            }
            return View();
        }

        /// <summary>
        /// Returns Form for creating a new file for customer
        /// </summary>
        /// <returns>New File Creation Form</returns>
        public ActionResult NewFileGov()
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
        /// Post method for New Customer File
        /// </summary>
        /// <param name="File">FileViewModel</param>
        /// <returns>HTTP Status Message</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> NewFileGov(
            [Bind(Include = "MemberNo,LoanApplication," +
            "LoanAgreement,GuaranteeCertificate," +
            "Amortisation,Eligibility,RequestLetter,Quotation,Payslip," +
            "BankAccStatment,LoanStatement,VNPFStatement,CustomerID,FStatusId,")] GovFileViewModel File)

        {
            if (ModelState.IsValid)
            {
                var LApp = new MemoryStream();
                var LAgrmt = new MemoryStream();
                var GC = new MemoryStream();
                var Amo = new MemoryStream();
                var Elig = new MemoryStream();
                var RL = new MemoryStream();
                var Quote = new MemoryStream();
                var PS = new MemoryStream();
                var BS = new MemoryStream();
                var LS = new MemoryStream();
                var VS = new MemoryStream();
                var CID = new MemoryStream();

                File.LoanApplication.InputStream.CopyTo(LApp);
                File.LoanAgreement.InputStream.CopyTo(LAgrmt);
                File.GuaranteeCertificate.InputStream.CopyTo(GC);
                File.Amortisation.InputStream.CopyTo(Amo);
                File.Eligibility.InputStream.CopyTo(Elig);
                File.RequestLetter.InputStream.CopyTo(RL);
                File.Quotation.InputStream.CopyTo(Quote);
                File.Payslip.InputStream.CopyTo(PS);
                File.BankAccStatment.InputStream.CopyTo(BS);
                File.LoanStatement.InputStream.CopyTo(LS);
                File.VNPFStatement.InputStream.CopyTo(VS);
                File.CustomerID.InputStream.CopyTo(CID);

                ViewBag.MemberNo = File.MemberNo;
                var NewMemberFile = new MemberFile
                {
                    DateCreated = System.DateTime.Now,
                    MemberNo = File.MemberNo,
                    EmployerType = "GOVT",
                    LoanApplication = LApp.ToArray(),
                    LoanAgreement = LAgrmt.ToArray(),
                    GuaranteeCertificate = GC.ToArray(),
                    Amortisation = Amo.ToArray(),
                    Eligibility = Elig.ToArray(),
                    RequestLetter = RL.ToArray(),
                    Quotation = Quote.ToArray(),
                    Payslip = PS.ToArray(),
                    BankAccStatement = BS.ToArray(),
                    LoanStatement = LS.ToArray(),
                    VNPFStatement = VS.ToArray(),
                    CustomerID = CID.ToArray(),
                    FStatusId = 1
                };

                var isSuccess = await _apiServices.CreateNewFile(NewMemberFile);

                if (isSuccess)
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
        /// Returns Form for creating a new file for customer
        /// </summary>
        /// <returns>New File Creation Form</returns>
        public ActionResult NewFileNGO1()
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
        public async Task<ActionResult> NewFileNGO1(
        [Bind(Include = "MemberNo,LoanApplication," +
            "LoanAgreement,GuaranteeCertificate," +
            "Amortisation,Eligibility,RequestLetter,Quotation,Payslip," +
            "BankAccStatment,LoanStatement,VNPFStatement,StandingOrder,CustomerID,FStatusId,")] NGOFileViewModel1 File)

        {
            if (ModelState.IsValid)
            {
                var LApp = new MemoryStream();
                var LAgrmt = new MemoryStream();
                var GC = new MemoryStream();
                var Amo = new MemoryStream();
                var Elig = new MemoryStream();
                var RL = new MemoryStream();
                var Quote = new MemoryStream();
                var PS = new MemoryStream();
                var BS = new MemoryStream();
                var LS = new MemoryStream();
                var VS = new MemoryStream();
                var SO = new MemoryStream();
                var CID = new MemoryStream();

                File.LoanApplication.InputStream.CopyTo(LApp);
                File.LoanAgreement.InputStream.CopyTo(LAgrmt);
                File.GuaranteeCertificate.InputStream.CopyTo(GC);
                File.Amortisation.InputStream.CopyTo(Amo);
                File.Eligibility.InputStream.CopyTo(Elig);
                File.RequestLetter.InputStream.CopyTo(RL);
                File.Quotation.InputStream.CopyTo(Quote);
                File.Payslip.InputStream.CopyTo(PS);
                File.BankAccStatment.InputStream.CopyTo(BS);
                File.LoanStatement.InputStream.CopyTo(LS);
                File.VNPFStatement.InputStream.CopyTo(VS);
                File.StandingOrder.InputStream.CopyTo(SO);
                File.CustomerID.InputStream.CopyTo(CID);

                ViewBag.MemberNo = File.MemberNo;
                var NewMemberFile = new MemberFile
                {
                    DateCreated = System.DateTime.Now,
                    MemberNo = File.MemberNo,
                    EmployerType = "NGO1",
                    LoanApplication = LApp.ToArray(),
                    LoanAgreement = LAgrmt.ToArray(),
                    GuaranteeCertificate = GC.ToArray(),
                    Amortisation = Amo.ToArray(),
                    Eligibility = Elig.ToArray(),
                    RequestLetter = RL.ToArray(),
                    Quotation = Quote.ToArray(),
                    Payslip = PS.ToArray(),
                    BankAccStatement = BS.ToArray(),
                    LoanStatement = LS.ToArray(),
                    VNPFStatement = VS.ToArray(),
                    StandingOrder = SO.ToArray(),
                    CustomerID = CID.ToArray(),
                    FStatusId = 1
                };

                var isSuccess = await _apiServices.CreateNewFile(NewMemberFile);

                if (isSuccess)
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
        /// Returns Form for creating a new file for customer
        /// </summary>
        /// <returns>New File Creation Form</returns>
        public ActionResult NewFileNGO2()
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
        public async Task<ActionResult> NewFileNGO2(
            [Bind(Include = "MemberNo,LoanApplication," +
            "LoanAgreement,GuaranteeCertificate," +
            "Amortisation,Eligibility,RequestLetter,EmployerLetter,Quotation," +
            "BankAccStatment,LoanStatement,VNPFStatement,OffsetLetter,CustomerID,FStatusId,")] NGOFileViewModel2 File)

        {
            if (ModelState.IsValid)
            {
                var LApp = new MemoryStream();
                var LAgrmt = new MemoryStream();
                var GC = new MemoryStream();
                var Amo = new MemoryStream();
                var Elig = new MemoryStream();
                var RL = new MemoryStream();
                var EL = new MemoryStream();
                var Quote = new MemoryStream();
                var BS = new MemoryStream();
                var LS = new MemoryStream();
                var VS = new MemoryStream();
                var OL = new MemoryStream();
                var CID = new MemoryStream();

                File.LoanApplication.InputStream.CopyTo(LApp);
                File.LoanAgreement.InputStream.CopyTo(LAgrmt);
                File.GuaranteeCertificate.InputStream.CopyTo(GC);
                File.Amortisation.InputStream.CopyTo(Amo);
                File.Eligibility.InputStream.CopyTo(Elig);
                File.RequestLetter.InputStream.CopyTo(RL);
                File.EmployerLetter.InputStream.CopyTo(EL);
                File.Quotation.InputStream.CopyTo(Quote);
                File.BankAccStatment.InputStream.CopyTo(BS);
                File.LoanStatement.InputStream.CopyTo(LS);
                File.VNPFStatement.InputStream.CopyTo(VS);
                File.OffsetLetter.InputStream.CopyTo(OL);
                File.CustomerID.InputStream.CopyTo(CID);

                ViewBag.MemberNo = File.MemberNo;
                var NewMemberFile = new MemberFile
                {
                    DateCreated = System.DateTime.Now,
                    MemberNo = File.MemberNo,
                    EmployerType = "NGO2",
                    LoanApplication = LApp.ToArray(),
                    LoanAgreement = LAgrmt.ToArray(),
                    GuaranteeCertificate = GC.ToArray(),
                    Amortisation = Amo.ToArray(),
                    Eligibility = Elig.ToArray(),
                    RequestLetter = RL.ToArray(),
                    EmployerLetter = EL.ToArray(),
                    Quotation = Quote.ToArray(),
                    BankAccStatement = BS.ToArray(),
                    LoanStatement = LS.ToArray(),
                    VNPFStatement = VS.ToArray(),
                    OffsetLetter = OL.ToArray(),
                    CustomerID = CID.ToArray(),
                    FStatusId = 1
                };

                var isSuccess = await _apiServices.CreateNewFile(NewMemberFile);

                if (isSuccess)
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
        /// Update Member File
        /// </summary>
        /// <param name="fileNo"></param>
        /// <returns></returns>
        [AcceptVerbs(HttpVerbs.Get)]
        public async Task<ActionResult> UpdateMemberFile(int fileNo)
        {
            if (Settings.AccessToken == "")
            {
                return RedirectToAction("SignOut", "Logout");
            }
            else if (DateTime.UtcNow.AddSeconds(10) > Settings.AccessTokenExpirationDate)
            {
                return RedirectToAction("SignOut", "Logout");
            }

            FileUpdateViewModel model = new FileUpdateViewModel()
            {
                FileNo = fileNo
            };
            //Get File Reference
            var obj = new FileReferences();
            HttpResponseMessage responseMessage = await client.GetAsync(url + "GetFileRefByFileNo/" + fileNo);
            if (responseMessage.IsSuccessStatusCode)
            {
                var responseData = responseMessage.Content.ReadAsStringAsync().Result;
                try
                {
                    var list = JsonConvert.DeserializeObject<List<FileReferences>>(responseData);
                    foreach(var i in list)
                    {
                        obj = i;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }

            }

            if (obj.FileStatus == "Pending Approval")
            {
                ViewBag.FileStatus = obj.FileStatus;
                return PartialView("_FileUpdateMessage", model);
            }
            if (obj.FileStatus == "Awaiting Input")
            {
                ViewBag.FileStatus = obj.FileStatus;
                return PartialView("_UpdateFile", model);
            }
            if (obj.FileStatus == "Await Posting")
            {
                ViewBag.FileStatus = obj.FileStatus;
                return PartialView("_UpdateFile", model);
            }
            return PartialView("_FileUpdateMessage", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UpdateMemberFile([Bind(Include = "FileNo,LoanApplication,ChequeCopy")] FileUpdateViewModel File)
        {
            if (Settings.AccessToken == "")
            {
                return RedirectToAction("SignOut", "Logout");
            }
            else if (DateTime.UtcNow.AddSeconds(10) > Settings.AccessTokenExpirationDate)
            {
                return RedirectToAction("SignOut", "Logout");
            }

            if (ModelState.IsValid)
            {
                var LoanApplication = new MemoryStream();
                var ChequeCopy = new MemoryStream();
                File.LoanApplication.InputStream.CopyTo(LoanApplication);
                File.ChequeCopy.InputStream.CopyTo(ChequeCopy);

                var FileDTO = new FileUpdateDTO
                {
                    FileNo = File.FileNo,
                    LoanApplication = LoanApplication.ToArray(),
                    ChequeCopy = ChequeCopy.ToArray()
                };

                var json = JsonConvert.SerializeObject(FileDTO);
                HttpContent content = new StringContent(json);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                try
                {
                    var response = await client.PutAsync(url+ "UpdateFile/", content);
                    Debug.WriteLine(response);
                    if (response.IsSuccessStatusCode)
                    {
                        return RedirectToAction("Recent");
                    }
                }
                catch (HttpRequestException e)
                {
                    Debug.WriteLine("\nException Caught!");
                    Debug.WriteLine("Message :{0} ", e.Message);
                    return RedirectToAction("CouldNotUpdateFile", "Error");
                }
            }
            return RedirectToAction("InternalServerError", "Error");
        }
    }
}
