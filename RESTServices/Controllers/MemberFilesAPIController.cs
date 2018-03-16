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
using System.Data.Entity.Validation;
using System.Diagnostics;

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
    /// 9. PostMemberFile - Post new member file
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
            FileList.Add(2, "LoanAgreement");
            FileList.Add(3, "GuaranteeCertificate");
            FileList.Add(4, "Amortisation");
            FileList.Add(5, "ChequeCopy");
            FileList.Add(6, "Eligibility");
            FileList.Add(7, "RequestLetter");
            FileList.Add(8, "EmployerLetter");
            FileList.Add(9, "Quotation");
            FileList.Add(10, "Payslip");
            FileList.Add(11, "BankAccStatement");
            FileList.Add(12, "LoanStatement");
            FileList.Add(13, "VNPFStatement");
            FileList.Add(14, "StandingOrder");
            FileList.Add(15, "CustomerID");

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
        /// Returns File Status Id
        /// </summary>
        /// <param name="FileNo"></param>
        /// <returns></returns>
        [Route("api/MemberFilesAPI/GetFileStatusId/{FileNo:int}")]
        public int GetFileStatusId(int FileNo)
        {
            var StatusId = db.MemberFile.Where(x => x.FileNo == FileNo).Select(x => x.FStatusId);
            if(StatusId != null)
            {
                return StatusId.First();
            }
            return 0;
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
        /// Post new member file to database
        /// </summary>
        /// <param name="memberFile">MembeFile</param>
        /// <returns></returns>
        // POST: api/MemberFilesAPI
        [ResponseType(typeof(MemberFile))]
        public IHttpActionResult PostMemberFile(MemberFile memberFile)
        {
            memberFile.OfficeId = User.Identity.GetUserId();
            var errors = ModelState.Values.SelectMany(v => v.Errors);
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            //Create Transaction
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    #region Transaction 1 begins
                    //Create new member File
                    if (memberFile.ChequeCopy != null)
                    {
                        memberFile.FStatusId = 2;
                    }

                    db.MemberFile.Add(memberFile);
                    db.SaveChanges();
                    #endregion Transaction 1 ends

                    #region Transaction 2 begins
                    //Create new file reference
                    var UserId = User.Identity.GetUserId();
                    var data = db.MemberFile.Where(x => x.OfficeId == UserId && x.MemberNo == memberFile.MemberNo)
                                                         .OrderByDescending(x => x.FileNo)
                                                         .Select(x => x.FileNo)
                                                         .ToList();

                    FileReferences NewFileRef = new FileReferences()
                    {
                        DateCreated = memberFile.DateCreated,
                        OfficerId = UserId,
                        MemberNo = memberFile.MemberNo,
                        FileNo = data.First(),
                        FileStatus = "Not Finalized"
                    };

                    db.FileReferences.Add(NewFileRef);
                    db.SaveChanges();
                    #endregion Transaction 2 ends
                    
                    //Commit Transaction
                    transaction.Commit();
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
                    //Rollback Transaction
                    transaction.Rollback();
                }
            }
            return CreatedAtRoute("DefaultApi", new { id = memberFile.FileNo }, memberFile);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileUpdateDTO"></param>
        /// <returns></returns>
        // PUT: api/MemberFilesAPI/5
        [Route("api/MemberFilesApi/UpdateFile/")]
        [ResponseType(typeof(void))]
        public IHttpActionResult PutMemberFile(FileUpdateDTO fileUpdateDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            //Create Transaction
            using (var transaction = db.Database.BeginTransaction())
            {
                var file = db.FileReferences.Where(x => x.FileNo == fileUpdateDTO.FileNo).First();
                try
                {
                    #region Transaction 1 begins
                    //Update MemberFileTable
                    if (file == null)
                    {
                        return BadRequest();
                    }
                    var fileToUpdate = db.MemberFile.Where(x => x.FileNo == file.FileNo).First();
                    fileToUpdate.ChequeCopy = fileUpdateDTO.ChequeCopy;
                    fileToUpdate.FStatusId = 2;
                    db.SaveChanges();
                    #endregion Transaction 1 ends

                    #region Transaction 2 begins
                    //Update FileReferences Table
                    var RefToUpdate = db.FileReferences.Where(x => x.FileNo == file.FileNo).First();
                    RefToUpdate.FileStatus = "Finalized";
                    db.SaveChanges();
                    #endregion Transaction 2 ends

                    //Commit Transaction
                    transaction.Commit();
                }
                catch (DbUpdateConcurrencyException)
                {
                    //Rollback Transaction
                    transaction.Rollback();
                    if (!MemberFileExists(file.FileNo))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return StatusCode(HttpStatusCode.NoContent);
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