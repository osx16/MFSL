using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using RESTServices.Models;
using Microsoft.AspNet.Identity;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.IO;
using System.Net.Http.Headers;

namespace RESTServices.Controllers
{
    public class MemberFilesAPIController : ApiController
    {
        private MFSLEntities db = new MFSLEntities();
        private string OfficerID = System.Web.HttpContext.Current.User.Identity.GetUserId();

        [Route("api/MemberFilesAPI/GetAll")]
        public IQueryable<FileReferences> GetAllMemberFile()
        {
            return db.FileReferences;
        }

        [HttpGet]
        [Route("api/MemberFilesAPI/FetchFile/")]
        //[ResponseType(typeof(MemberFile))]
        public HttpResponseMessage FetchFile(string id, string flag)
        {
            int FileNo = Convert.ToInt32(id);
            int FileTypeId = Convert.ToInt32(flag);
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

            if (FileList.ContainsKey(FileTypeId))
            {
                filename = FileList[FileTypeId];
                Console.WriteLine(filename);
            }

            string connectionString = @"Data Source=JAS;Initial Catalog=MFSL;Integrated Security=True;
                                      MultipleActiveResultSets=True;Application Name=EntityFramework";
            string commandText = @"SELECT " + filename +
                                ".PathName(), GET_FILESTREAM_TRANSACTION_CONTEXT() FROM MFSL.dbo.MemberFile WHERE FileNo = @FileNo;";
            string serverPath;
            byte[] serverTxn;
            byte[] buffer = new Byte[1024 * 512];

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

            HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
            result.Content = new ByteArrayContent(buffer);
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            return result;
        }//End of DownloadFile Method

        [Route("api/MemberFilesAPI/GetFileForUser")]
        public IQueryable<FileReferences> GetFileForUser()
        {
            var UserId = User.Identity.GetUserId();
            return db.FileReferences.Where(x=>x.OfficerId == UserId)
                                    .OrderByDescending(x=>x.DateCreated);
        }

        [Route("api/MemberFilesAPI/GetFileByMemberNo/{MemberNo:int}")]
        public IQueryable<FileReferences> GetFileByMemberNo(int MemberNo)
        {
            var data = db.FileReferences.Where(x => x.MemberNo == MemberNo);
            return data;
        }

        [Route("api/MemberFilesAPI/GetMyFileByMemberNo/{MemberNo:int}")]
        public IQueryable<FileReferences> GetMyFileByMemberNo(int MemberNo)
        {
            var UserId = User.Identity.GetUserId();
            return db.FileReferences.Where(x => x.MemberNo == MemberNo && x.OfficerId == UserId);
        }

        [Route("api/MemberFilesAPI/GetMemberInfoByNo/{MemberNo:int}")]
        //[ResponseType(typeof(MemberFile))]
        public IEnumerable<vnpf_> GetMemberInfoByNo(int MemberNo)
        {
            return db.vnpf_.Where(f => f.VNPF_Number == MemberNo);
        }

        // GET: api/MemberFilesAPI/5
        [ResponseType(typeof(MemberFile))]
        public IHttpActionResult GetMemberFile(int id)
        {
            MemberFile memberFile = db.MemberFile.Find(id);
            if (memberFile == null)
            {
                return NotFound();
            }

            return Ok(memberFile);
        }

        // PUT: api/MemberFilesAPI/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutMemberFile(int id, MemberFile memberFile)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != memberFile.FileNo)
            {
                return BadRequest();
            }

            db.Entry(memberFile).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MemberFileExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/MemberFilesAPI
        [ResponseType(typeof(MemberFile))]
        public IHttpActionResult PostMemberFile(MemberFile memberFile)
        {
            memberFile.OfficeId = User.Identity.GetUserId();
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.MemberFile.Add(memberFile);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = memberFile.FileNo }, memberFile);
        }

        [Route("api/MemberFilesAPI/PostReference")]
        [ResponseType(typeof(MemberFile))]
        public IHttpActionResult PostReference(FileReferences fileRef)
        {
            var UserId = User.Identity.GetUserId();
            fileRef.OfficerId = UserId;
            var DateCreated = fileRef.DateCreated;
            var MemberNo = fileRef.MemberNo;
            var data = db.MemberFile.Where(x => x.OfficeId == UserId &&
                                             x.MemberNo == MemberNo)
                                             .OrderByDescending(x=>x.FileNo)
                                             .Select(x => x.FileNo)
                                             .ToList();
            fileRef.FileNo = data.First();
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.FileReferences.Add(fileRef);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = fileRef.FileNo }, fileRef);
        }

        // DELETE: api/MemberFilesAPI/5
        [ResponseType(typeof(MemberFile))]
        public IHttpActionResult DeleteMemberFile(int id)
        {
            MemberFile memberFile = db.MemberFile.Find(id);
            if (memberFile == null)
            {
                return NotFound();
            }

            db.MemberFile.Remove(memberFile);
            db.SaveChanges();

            return Ok(memberFile);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool MemberFileExists(int id)
        {
            return db.MemberFile.Count(e => e.FileNo == id) > 0;
        }
    }
}