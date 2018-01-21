using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using MFSL.Models;
using MFSL.Repository;
using MFSL.ViewModels;
using System.IO;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using PagedList;

namespace MFSL.Controllers
{
    public class MemberFilesController : Controller
    {
        private IFileRepo _repository;
        private MFSLEntities db = new MFSLEntities();

        public MemberFilesController() : this(new FileRepo()){ }

        public MemberFilesController(FileRepo repository)
        {
            _repository = repository;
        }

        public ActionResult RenderInfoView()
        {
            return PartialView("_Info");
        }

        public ActionResult Dashboard()
        {
            return View();
        }

        public ActionResult Recent()
        {
            return View();
        }

        public ActionResult MyFiles(string memberNo, int? page)
        {
            ViewBag.MemberNo = memberNo;
            int pageSize = 5;
            int pageIndex = 1;
            pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;

            int id = 0;
            bool isValid = Int32.TryParse(memberNo, out id);

            pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
            string UserId = "79a0e692-d36e-455a-bfd9-32ed77c066b4";
            var memberFile = db.MemberFile.Include(m => m.FileStatus)
                            .Where(
                                    x => x.OfficeId.Equals(UserId)
                                   )
                            .OrderByDescending(x => x.DateCreated);
            ViewBag.TotalFiles = memberFile.Count();
            IPagedList<MemberFile> files = (memberFile).ToPagedList(pageIndex, pageSize);


            if (!String.IsNullOrEmpty(memberNo))
            {
                var list = _repository.FetchMyFilesByMemberNo(id, UserId);
                ViewBag.TotalFiles = list.Count;
                IPagedList<MemberFile> filteredList = (list).ToPagedList(pageIndex, pageSize);             
                return PartialView("_MyFileDetails", filteredList);
            }

            return PartialView("_MyFileDetails", files);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        // GET: Items
        public ActionResult SearchFiles(string memberNo, int? page)
        {
            ViewBag.MemberNo = memberNo;
            int pageSize = 5;
            int pageIndex = 1;
            pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;

            //if (String.IsNullOrEmpty(memberNo))
            //{
            //    throw new ArgumentNullException("memberNo");
            //}
            int id = 0;
            bool isValid = Int32.TryParse(memberNo, out id);
            var list = _repository.FetchAllMemberFiles();
            ViewBag.TotalFiles = list.Count;
            IPagedList<MemberFile> files = (list).ToPagedList(pageIndex, pageSize);
            
            if (!String.IsNullOrEmpty(memberNo))
            {
                var lst = _repository.FetchFileByMemberNo(id);
                ViewBag.TotalFiles = lst.Count;
                IPagedList<MemberFile> filteredList = (lst).ToPagedList(pageIndex, pageSize);
                return PartialView("_FileDetails", filteredList);
            }

            return PartialView("_FileDetails", files);
        }

        public ActionResult NewFile()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult NewFile(
            [Bind(Include = "MemberNo,LoanApplication,OfferLetter," +
            "LoanAgreement,AcceptanceOffer,GuaranteeCertificate," +
            "Amortisation,ChequeCopy,Eligibility,Quotation,Payslip," +
            "LoanStatement,VNPFStatement,Other,FStatusId,")] FileViewModel File)

        {
            if (ModelState.IsValid)
            {
                string UserId = "79a0e692-d36e-455a-bfd9-32ed77c066b4";
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
                    OfficeId = UserId,
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

                db.MemberFile.Add(NewMemberFile);
                try
                {
                    var result = db.SaveChanges();
                    if (result > 0)
                    {
                    }
                }
                catch (DbEntityValidationException dbEx)
                {
                    foreach (var validationErrors in dbEx.EntityValidationErrors)
                    {
                        foreach (var validationError in validationErrors.ValidationErrors)
                        {
                            Trace.TraceInformation("Property: {0} Error: {1}",
                                                    validationError.PropertyName,
                                                    validationError.ErrorMessage);
                        }
                    }
                }
                return RedirectToAction("Recent");
            }

            return View();
        }


        [AcceptVerbs(HttpVerbs.Get)]
        public async Task<ActionResult> GetMemberInfoByNum(string MemberNum)
        {
            if (String.IsNullOrEmpty(MemberNum))
            {
                throw new ArgumentNullException("MemberNum");
            }
            int id = 0;
            bool isValid = Int32.TryParse(MemberNum, out id);

            var member = db.vnpf_.Where(s => s.VNPF_Number == id);

            return PartialView("_MemberInfo", await member.ToListAsync());
        }



        // GET: MemberFiles/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MemberFile memberFile = await db.MemberFile.FindAsync(id);
            if (memberFile == null)
            {
                return HttpNotFound();
            }
            return View(memberFile);
        }

        // GET: MemberFiles/Create
        public ActionResult Create()
        {
            ViewBag.FStatusId = new SelectList(db.FileStatus, "FileStatusId", "FStatus");
            return View();
        }

        // POST: MemberFiles/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "FileNo,DateCreated,OfficeId,MemberNo,LoanApplication,OfferLetter,LoanAgreement,AcceptanceOffer,GuaranteeCertificate,Amortisation,ChequeCopy,Eligibility,Quotation,Payslip,LoanStatement,VNPFStatement,Other,fileGUID,FStatusId")] MemberFile memberFile)
        {
            if (ModelState.IsValid)
            {
                db.MemberFile.Add(memberFile);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.FStatusId = new SelectList(db.FileStatus, "FileStatusId", "FStatus", memberFile.FStatusId);
            return View(memberFile);
        }

        // GET: MemberFiles/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MemberFile memberFile = await db.MemberFile.FindAsync(id);
            if (memberFile == null)
            {
                return HttpNotFound();
            }
            ViewBag.FStatusId = new SelectList(db.FileStatus, "FileStatusId", "FStatus", memberFile.FStatusId);
            return View(memberFile);
        }

        // POST: MemberFiles/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "FileNo,DateCreated,OfficeId,MemberNo,LoanApplication,OfferLetter,LoanAgreement,AcceptanceOffer,GuaranteeCertificate,Amortisation,ChequeCopy,Eligibility,Quotation,Payslip,LoanStatement,VNPFStatement,Other,fileGUID,FStatusId")] MemberFile memberFile)
        {
            if (ModelState.IsValid)
            {
                db.Entry(memberFile).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.FStatusId = new SelectList(db.FileStatus, "FileStatusId", "FStatus", memberFile.FStatusId);
            return View(memberFile);
        }

        // GET: MemberFiles/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MemberFile memberFile = await db.MemberFile.FindAsync(id);
            if (memberFile == null)
            {
                return HttpNotFound();
            }
            return View(memberFile);
        }

        // POST: MemberFiles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            MemberFile memberFile = await db.MemberFile.FindAsync(id);
            db.MemberFile.Remove(memberFile);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [HttpGet]
        public FileStreamResult DownloadFile(string id, string flag)
        {
            int FileNo = Convert.ToInt32(id);
            int fileTypeId = Convert.ToInt32(flag);
            string filename = "";
            Dictionary<int, string> FileList = new Dictionary<int, string>();
            FileList.Add(1, "LoanApplication");
            FileList.Add(2, "OfferLetter");
            FileList.Add(3, "LoanAgreement");
            FileList.Add(4, "AcceptanceOffer");
            FileList.Add(5, "GuaranteeCertificate");
            FileList.Add(6, "Amortisation");
            FileList.Add(7, "ChequeCopy");
            FileList.Add(8, "Eligibility");
            FileList.Add(9, "Quotation");
            FileList.Add(10, "Payslip");
            FileList.Add(11, "LoanStatement");
            FileList.Add(12, "VNPFStatement");
            FileList.Add(13, "Other");

            if (FileList.ContainsKey(fileTypeId))
            {
                filename = FileList[fileTypeId];
                Console.WriteLine(filename);
            }

            string connectionString = @"Data Source=JAS;Initial Catalog=MFSL;Integrated Security=True;
                                      MultipleActiveResultSets=True;Application Name=EntityFramework";
            string commandText = @"SELECT " +filename+ 
                                ".PathName(), GET_FILESTREAM_TRANSACTION_CONTEXT() FROM MFSL.dbo.MemberFile WHERE FileNo = @FileNo;";



            string serverPath;
            byte[] serverTxn;
            byte[] buffer = new Byte[1024 * 512];
            //byte[] buffer;

            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {

                try
                {
                    sqlConnection.Open();

                    SqlTransaction transaction = sqlConnection.BeginTransaction();
                    SqlCommand sqlCommand = new SqlCommand();
                    sqlCommand.Transaction = transaction;
                    sqlCommand.Connection = sqlConnection;
                    sqlCommand.CommandText = commandText;
                    sqlCommand.Parameters.Add("@FileNo", SqlDbType.Int).Value = FileNo;

                    using (SqlDataReader reader = sqlCommand.ExecuteReader())
                    {
                        reader.Read();
                        serverPath = reader.GetSqlString(0).Value;
                        serverTxn = reader.GetSqlBinary(1).Value;
                        reader.Close();
                    }

                    using (SqlFileStream sqlFileStream = new SqlFileStream(serverPath, serverTxn, FileAccess.Read))
                    {
                        buffer = new Byte[sqlFileStream.Length];
                        sqlFileStream.Read(buffer, 0, buffer.Length);
                        sqlFileStream.Close();
                    }

                    sqlCommand.Transaction.Commit();

                }
                catch (System.Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
                finally
                {
                    sqlConnection.Close();
                }

            }//End of SQL Connection Block

            return File(new MemoryStream(buffer), "application/pdf");
        }//End of DownloadFile Method

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
