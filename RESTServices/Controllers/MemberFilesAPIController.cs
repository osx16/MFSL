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
    /// <summary>
    /// List of Controller and Non-Controller methods used
    /// 1. GetAllMemberFile - Gets all File References for all Customers
    /// 2. FetchFile - Fetch specific file for a user based on File Id and Flag
    /// 3. GetFileForUser - Get file references created by current user
    /// 4. GetFileByMemberNo - Get file reference(s) for member
    /// 5. GetFileRefByFileNo - Get file reference by file number
    /// 6. GetMyFileByMemberNo - Get file references for member by member no under an officer 
    /// 7. GetMemberInfoByNo - Get member info by member number
    /// 8. PutMemberFile - Unused (Reserve)
    /// 9. PostMemberFile - Unused (Reserve)
    /// 10. PostReference - Post new file reference for customer to database
    /// 11. DeleteMemberFile - Unused (Reserve)
    /// 12. Dispose (Non-Controller Method) - Dispose controller data
    /// 13. MemberFileExists (Non-Controller Method) - Returns 1 if Member File exists
    /// </summary>
    public class MemberFilesAPIController : ApiController
    {
        private MFSLEntities db = new MFSLEntities();
        private string OfficerID = System.Web.HttpContext.Current.User.Identity.GetUserId();

        /// <summary>
        /// Gets all File References for all Customers
        /// </summary>
        [Route("api/MemberFilesAPI/GetAll")]
        public IQueryable<FileReferences> GetAllMemberFile()
        {
            return db.FileReferences;
        }

        /// <summary>
        /// Fetch specific file for a user based on File Id and Flag
        /// </summary>
        /// <param name="id">File Id</param>
        /// <param name="flag">Flag indicates type of document</param>
        [HttpGet]
        [Route("api/MemberFilesAPI/FetchFile/")]
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

        /// <summary>
        /// Get file references created by current user
        /// </summary>
        [Route("api/MemberFilesAPI/GetFileForUser")]
        public IQueryable<FileReferences> GetFileForUser()
        {
            var UserId = User.Identity.GetUserId();
            return db.FileReferences.Where(x=>x.OfficerId == UserId)
                                    .OrderByDescending(x=>x.DateCreated);
        }

        /// <summary>
        /// Get file reference(s) for member
        /// </summary>
        /// <param name="MemberNo">Member No</param>
        [Route("api/MemberFilesAPI/GetFileByMemberNo/{MemberNo:int}")]
        public IQueryable<FileReferences> GetFileByMemberNo(int MemberNo)
        {
            var data = db.FileReferences.Where(x => x.MemberNo == MemberNo);
            return data;
        }

        /// <summary>
        /// Get file reference by file number
        /// </summary>
        /// <param name="FileNo"></param>
        /// <returns></returns>
        [Route("api/MemberFilesAPI/GetFileRefByFileNo/{FileNo:int}")]
        public IQueryable<FileReferences> GetFileRefByFileNo(int FileNo)
        {
            var fileRef = db.FileReferences.Where(x => x.FileNo == FileNo);
            return fileRef;
        }

        /// <summary>
        /// Get file references for member by member no under an officer 
        /// </summary>
        /// <param name="MemberNo"> Member No</param>
        /// <returns></returns>
        [Route("api/MemberFilesAPI/GetMyFileByMemberNo/{MemberNo:int}")]
        public IQueryable<FileReferences> GetMyFileByMemberNo(int MemberNo)
        {
            var UserId = User.Identity.GetUserId();
            return db.FileReferences.Where(x => x.MemberNo == MemberNo && x.OfficerId == UserId);
        }
        /// <summary>
        /// Get member info by member number
        /// </summary>
        /// <param name="MemberNo"> Member Number</param>
        [Route("api/MemberFilesAPI/GetMemberInfoByNo/{MemberNo:int}")]
        //[ResponseType(typeof(MemberFile))]
        public IEnumerable<vnpf_> GetMemberInfoByNo(int MemberNo)
        {
            return db.vnpf_.Where(f => f.VNPF_Number == MemberNo);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="memberFile"></param>
        /// <returns></returns>
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
        /// <summary>
        /// Post new member file to database
        /// </summary>
        /// <param name="memberFile">MembeFile</param>
        /// <returns></returns>
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
        /// <summary>
        /// Post new file reference for customer to database
        /// </summary>
        /// <param name="fileRef"></param>
        /// <returns></returns>
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="disposing"></param>
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