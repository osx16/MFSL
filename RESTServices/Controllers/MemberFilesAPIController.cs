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
    // List of Controller and Non-Controller API methods used
    // 1. GetAllMemberFile - Gets all File References for all Customers
    // 2. FetchFile - Fetch specific file for a user based on File Id and Flag
    // 3. GetFileForUser - Get file references created by current user
    // 4. GetFileByMemberNo - Get file reference(s) for member
    // 5. GetFileRefByFileNo - Get file reference by file number
    // 6. GetMyFileByMemberNo - Get file references for member by member no under an officer 
    // 7. GetMemberInfoByNo - Get member info by member number
    // 8. GetAllPendingApproval - Get All Pending Approval File References
    // 9. SearchPendingApproval - Gets specific file reference where status is pending approval
    //    based on member number
    // 10. GetAllAwaitingInput - Get All Awaiting Input File References
    // 11. SearchAwaitingInput - Gets specific file reference where status is awaiting input
    // 12. GetAllAwaitingPayment - Get All Awaiting Payment File References
    // 13. SearchAwaitingPayment - Gets specific file reference where status is Awaiting Input
    // 14. GetAllAwaitingCollateral - Get All Awaiting Collateral File References
    // 15. SearchAwaitingCollateral - Gets specific file reference where status is Awaiting Collateral
    //     based on member number
    // 16. PostMemberFile - Post new member file
    // 17. UpdateLoanApp - Update Loan Application file
    // 18. UpdatePaymentAdvice - Update Payment Advice File
    // 19. UpdatePaymentAndCheque - Update Payment Advice and Cheque Copy File
    // 20. UploadCollateral - Update Collateral File
    // 21. DeleteMemberFile - Unused (Reserve)
    // 22. Dispose (Non-Controller Method) - Dispose controller data
    // 23. MemberFileExists (Non-Controller Method) - Returns 1 if Member File exists

    /// <summary>
    /// Member Files API Controller
    /// </summary>
    public class MemberFilesAPIController : ApiController
    {
        private MFSLEntities db = new MFSLEntities();
        ApplicationDbContext context;
        AccountController Account;
        /// <summary>
        /// Constructor
        /// </summary>
        public MemberFilesAPIController()
        {
            Account = new AccountController();
            context = new ApplicationDbContext();
        }

        /// <summary>
        /// Gets all File References for all Customers
        /// </summary>
        [HttpGet]
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
            FileList.Add(15, "OffsetLetter");
            FileList.Add(16, "CustomerID");
            FileList.Add(17, "PaymentAdvice"); 
            FileList.Add(18, "Collateral");
            FileList.Add(19, "PaymentRequest"); //Special case
            FileList.Add(20, "LoanStatement"); //Special case
            FileList.Add(21, "ReconciliationSheet"); //Special case
            FileList.Add(22, "RefundChequeCopy");
            FileList.Add(23, "MaintenanceForm");



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
        [HttpGet]
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
        [HttpGet]
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
        [HttpGet]
        [Route("api/MemberFilesAPI/GetFileRefByFileNo/{FileNo:int}")]
        public IQueryable<FileReferences> GetFileRefByFileNo(int FileNo)
        {
            var fileRef = db.FileReferences.Where(x => x.FileNo == FileNo).Distinct();
            if (fileRef.Count() == 0)
            {
                return null;
            }
            return fileRef;
        }

        /// <summary>
        /// Get file references for member by member no under an officer 
        /// </summary>
        /// <param name="MemberNo"> Member No</param>
        /// <returns></returns>
        [HttpGet]
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
        [HttpGet]
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
        [HttpGet]
        [Route("api/MemberFilesAPI/GetMemberInfoByNo/{MemberNo:int}")]
        //[ResponseType(typeof(MemberFile))]
        public IEnumerable<vnpf_> GetMemberInfoByNo(int MemberNo)
        {
            return db.vnpf_.Where(f => f.VNPF_Number == MemberNo);
        }

        /// <summary>
        /// Get All Pending Approval File References
        /// </summary>
        /// <returns></returns>

        [HttpGet]
        [Route("api/MemberFilesAPI/GetAllPendingApproval")]
        public IEnumerable<FileReferences> GetAllPendingApproval()
        {
            string FileStatus = "Pending Approval";
            var OfficerID = User.Identity.GetUserId();
            int branchId = db.Officers.Where(x => x.OfficerId == OfficerID).Select(x => x.BranchId).First();
            var branchLocation = db.Branches.Where(x => x.BranchId == branchId).Select(x => x.BranchLocation).First();
            RolesAPIController roleApi = new RolesAPIController();
            var role = roleApi.GetRoleForThisUser();
            if(role == "Admin" || role == "General Manager")
            {
                return db.FileReferences.Where(x => x.FileStatus == FileStatus).OrderByDescending(x=>x.DateCreated);
            }
            else if(role == "SIO Operation" || role == "SIO Marketing")
            {
                return db.FileReferences
                         .Where(x => x.Branch == branchLocation && x.FileStatus == FileStatus)
                         .OrderByDescending(x => x.DateCreated);
            }
            else if(role == "SIO Branch Operation")
            {
                return db.FileReferences.Where(x => (x.Branch == "Tanna" || x.Branch == "Malekula" || x.Branch == "Santo") 
                         && x.FileStatus == FileStatus).OrderByDescending(x => x.DateCreated);
            }
            return db.FileReferences
                     .Where(f => f.OfficerId == OfficerID && f.FileStatus == FileStatus)
                     .OrderByDescending(x => x.DateCreated);
        }

        /// <summary>
        /// Gets specific file reference where status is pending approval
        /// based on member number
        /// </summary>
        /// <param name="memberNo">Member Number</param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/MemberFilesAPI/SearchPendingApproval/{memberNo:int}")]
        public IEnumerable<FileReferences> SearchPendingApproval(int memberNo)
        {
            string FileStatus = "Pending Approval";
            var OfficerID = User.Identity.GetUserId();
            int branchId = db.Officers.Where(x => x.OfficerId == OfficerID).Select(x => x.BranchId).First();
            var branchLocation = db.Branches.Where(x => x.BranchId == branchId).Select(x => x.BranchLocation).First();
            RolesAPIController roleApi = new RolesAPIController();
            var role = roleApi.GetRoleForThisUser();
            if (role == "Admin" || role == "General Manager")
            {
                return db.FileReferences
                         .Where(x => x.FileStatus == FileStatus && x.MemberNo == memberNo)
                         .OrderByDescending(x => x.DateCreated);
            }
            else if (role == "SIO Operation" || role == "SIO Marketing")
            {
                return db.FileReferences
                         .Where(x => x.Branch == branchLocation && x.FileStatus == FileStatus && x.MemberNo == memberNo)
                         .OrderByDescending(x => x.DateCreated);
            }
            else if (role == "SIO Branch Operation")
            {
                return db.FileReferences
                         .Where(x => (x.Branch == "Tanna" || x.Branch == "Malekula" || x.Branch == "Santo") && 
                         x.FileStatus == FileStatus && x.MemberNo == memberNo)
                         .OrderByDescending(x => x.DateCreated);
            }
            return db.FileReferences
                     .Where(f => f.OfficerId == OfficerID && f.FileStatus == FileStatus && f.MemberNo == memberNo)
                     .OrderByDescending(x => x.DateCreated);
        }

        /// <summary>
        /// Gets specific file reference where status is Demand
        /// </summary>
        /// <param name="memberNo"> Member Number</param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/MemberFilesAPI/SearchDemands/{memberNo:int}")]
        public IEnumerable<FileReferences> SearchDemands(int memberNo)
        {
            string FileStatus = "Demand";
            var OfficerID = User.Identity.GetUserId();
            int branchId = db.Officers.Where(x => x.OfficerId == OfficerID).Select(x => x.BranchId).First();
            var branchLocation = db.Branches.Where(x => x.BranchId == branchId).Select(x => x.BranchLocation).First();
            RolesAPIController roleApi = new RolesAPIController();
            var role = roleApi.GetRoleForThisUser();
            if (role == "Admin" || role == "General Manager")
            {
                return db.FileReferences
                         .Where(x => x.FileStatus == FileStatus && x.MemberNo == memberNo)
                         .OrderByDescending(x => x.DateCreated);
            }
            else if (role == "SIO Operation" || role == "SIO Marketing")
            {
                return db.FileReferences
                         .Where(x => x.Branch == branchLocation && x.FileStatus == FileStatus && x.MemberNo == memberNo)
                         .OrderByDescending(x => x.DateCreated);
            }
            else if (role == "SIO Branch Operation")
            {
                return db.FileReferences
                         .Where(x => (x.Branch == "Tanna" || x.Branch == "Malekula" || x.Branch == "Santo") &&
                         x.FileStatus == FileStatus && x.MemberNo == memberNo)
                         .OrderByDescending(x => x.DateCreated);
            }
            return db.FileReferences
                     .Where(f => f.OfficerId == OfficerID && f.FileStatus == FileStatus && f.MemberNo == memberNo)
                     .OrderByDescending(x => x.DateCreated);
        }

        /// <summary>
        /// Gets specific file reference where status is Arrears
        /// </summary>
        /// <param name="memberNo">Member Number</param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/MemberFilesAPI/SearchArrears/{memberNo:int}")]
        public IEnumerable<FileReferences> SearchArrears(int memberNo)
        {
            string FileStatus = "Arrears";
            var OfficerID = User.Identity.GetUserId();
            int branchId = db.Officers.Where(x => x.OfficerId == OfficerID).Select(x => x.BranchId).First();
            var branchLocation = db.Branches.Where(x => x.BranchId == branchId).Select(x => x.BranchLocation).First();
            RolesAPIController roleApi = new RolesAPIController();
            var role = roleApi.GetRoleForThisUser();
            if (role == "Admin" || role == "General Manager" || role == "IO Restructure")
            {
                return db.FileReferences
                         .Where(x => x.FileStatus == FileStatus && x.MemberNo == memberNo)
                         .OrderByDescending(x => x.DateCreated);
            }
            else if (role == "SIO Operation" || role == "SIO Marketing")
            {
                return db.FileReferences
                         .Where(x => x.Branch == branchLocation && x.FileStatus == FileStatus && x.MemberNo == memberNo)
                         .OrderByDescending(x => x.DateCreated);
            }
            else if (role == "SIO Branch Operation")
            {
                return db.FileReferences
                         .Where(x => (x.Branch == "Tanna" || x.Branch == "Malekula" || x.Branch == "Santo") &&
                         x.FileStatus == FileStatus && x.MemberNo == memberNo)
                         .OrderByDescending(x => x.DateCreated);
            }
            return db.FileReferences
                     .Where(f => f.OfficerId == OfficerID && f.FileStatus == FileStatus && f.MemberNo == memberNo)
                     .OrderByDescending(x => x.DateCreated);
        }

        /// <summary>
        /// Gets specific file reference where status is Refund
        /// </summary>
        /// <param name="memberNo">Member Number</param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/MemberFilesAPI/SearchRefunds/{memberNo:int}")]
        public IEnumerable<FileReferences> SearchRefunds(int memberNo)
        {
            string FileStatus = "Refund";
            var OfficerID = User.Identity.GetUserId();
            int branchId = db.Officers.Where(x => x.OfficerId == OfficerID).Select(x => x.BranchId).First();
            var branchLocation = db.Branches.Where(x => x.BranchId == branchId).Select(x => x.BranchLocation).First();
            RolesAPIController roleApi = new RolesAPIController();
            var role = roleApi.GetRoleForThisUser();
            if (role == "Admin" || role == "General Manager" || role == "Accounts Payable" || role == "Accounts Receivable" || role == "Sr. Finance Officer")
            {
                return db.FileReferences
                         .Where(x => x.FileStatus == FileStatus && x.MemberNo == memberNo)
                         .OrderByDescending(x => x.DateCreated);
            }
            else if (role == "SIO Operation" || role == "SIO Marketing")
            {
                return db.FileReferences
                         .Where(x => x.Branch == branchLocation && x.FileStatus == FileStatus && x.MemberNo == memberNo)
                         .OrderByDescending(x => x.DateCreated);
            }
            else if (role == "SIO Branch Operation")
            {
                return db.FileReferences
                         .Where(x => (x.Branch == "Tanna" || x.Branch == "Malekula" || x.Branch == "Santo") &&
                         x.FileStatus == FileStatus && x.MemberNo == memberNo)
                         .OrderByDescending(x => x.DateCreated);
            }
            return db.FileReferences
                     .Where(f => f.OfficerId == OfficerID && f.FileStatus == FileStatus && f.MemberNo == memberNo)
                     .OrderByDescending(x => x.DateCreated);
        }

        /// <summary>
        /// Gets specific file reference where status is Maintenance
        /// </summary>
        /// <param name="memberNo">Member Number</param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/MemberFilesAPI/SearchMaintenance/{memberNo:int}")]
        public IEnumerable<FileReferences> SearchMaintenance(int memberNo)
        {
            string FileStatus = "Maintenance";
            var OfficerID = User.Identity.GetUserId();
            int branchId = db.Officers.Where(x => x.OfficerId == OfficerID).Select(x => x.BranchId).First();
            var branchLocation = db.Branches.Where(x => x.BranchId == branchId).Select(x => x.BranchLocation).First();
            RolesAPIController roleApi = new RolesAPIController();
            var role = roleApi.GetRoleForThisUser();
            if (role == "Admin" || role == "General Manager" || role == "IO Restructure")
            {
                return db.FileReferences
                         .Where(x => x.FileStatus == FileStatus && x.MemberNo == memberNo)
                         .OrderByDescending(x => x.DateCreated);
            }
            else if (role == "SIO Operation" || role == "SIO Marketing")
            {
                return db.FileReferences
                         .Where(x => x.Branch == branchLocation && x.FileStatus == FileStatus && x.MemberNo == memberNo)
                         .OrderByDescending(x => x.DateCreated);
            }
            else if (role == "SIO Branch Operation")
            {
                return db.FileReferences
                         .Where(x => (x.Branch == "Tanna" || x.Branch == "Malekula" || x.Branch == "Santo") &&
                         x.FileStatus == FileStatus && x.MemberNo == memberNo)
                         .OrderByDescending(x => x.DateCreated);
            }
            return db.FileReferences
                     .Where(f => f.OfficerId == OfficerID && f.FileStatus == FileStatus && f.MemberNo == memberNo)
                     .OrderByDescending(x => x.DateCreated);
        }

        /// <summary>
        /// Get All Awaiting Input File References
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("api/MemberFilesAPI/GetAllAwaitingInput")]
        public IEnumerable<FileReferences> GetAllAwaitingInput()
        {
            string FileStatus = "Awaiting Input";
            var OfficerID = User.Identity.GetUserId();
            int branchId = db.Officers.Where(x => x.OfficerId == OfficerID).Select(x => x.BranchId).First();
            var branchLocation = db.Branches.Where(x => x.BranchId == branchId).Select(x => x.BranchLocation).First();
            RolesAPIController roleApi = new RolesAPIController();
            var role = roleApi.GetRoleForThisUser();
            if (role == "Admin" || role == "General Manager")
            {
                return db.FileReferences
                         .Where(x => x.FileStatus == FileStatus)
                         .OrderByDescending(x => x.DateCreated);
            }
            else if (role == "SIO Operation" || role == "SIO Marketing")
            {
                return db.FileReferences
                         .Where(x => x.Branch == branchLocation && x.FileStatus == FileStatus)
                         .OrderByDescending(x => x.DateCreated);
            }
            else if (role == "SIO Branch Operation")
            {
                return db.FileReferences
                         .Where(x => (x.Branch == "Tanna" || x.Branch == "Malekula" || x.Branch == "Santo") && x.FileStatus == FileStatus)
                         .OrderByDescending(x => x.DateCreated);
            }
            return db.FileReferences
                     .Where(f => f.OfficerId == OfficerID && f.FileStatus == FileStatus)
                     .OrderByDescending(x => x.DateCreated);
        }

        /// <summary>
        /// Get All Demand File References
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("api/MemberFilesAPI/GetAllDemands")]
        public IEnumerable<FileReferences> GetAllDemands()
        {
            string FileStatus = "Demand";
            var OfficerID = User.Identity.GetUserId();
            int branchId = db.Officers.Where(x => x.OfficerId == OfficerID).Select(x => x.BranchId).First();
            var branchLocation = db.Branches.Where(x => x.BranchId == branchId).Select(x => x.BranchLocation).First();
            RolesAPIController roleApi = new RolesAPIController();
            var role = roleApi.GetRoleForThisUser();
            if (role == "Admin" || role == "General Manager")
            {
                return db.FileReferences
                         .Where(x => x.FileStatus == FileStatus)
                         .OrderByDescending(x => x.DateCreated);
            }
            else if (role == "SIO Operation" || role == "SIO Marketing")
            {
                return db.FileReferences
                         .Where(x => x.Branch == branchLocation && x.FileStatus == FileStatus)
                         .OrderByDescending(x => x.DateCreated);
            }
            else if (role == "SIO Branch Operation")
            {
                return db.FileReferences
                         .Where(x => (x.Branch == "Tanna" || x.Branch == "Malekula" || x.Branch == "Santo") && x.FileStatus == FileStatus)
                         .OrderByDescending(x => x.DateCreated);
            }
            return db.FileReferences
                     .Where(f => f.OfficerId == OfficerID && f.FileStatus == FileStatus)
                     .OrderByDescending(x => x.DateCreated);
        }

        /// <summary>
        /// Get All Arrears File References
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("api/MemberFilesAPI/GetAllArrears")]
        public IEnumerable<FileReferences> GetAllArrears()
        {
            string FileStatus = "Arrears";
            var OfficerID = User.Identity.GetUserId();
            int branchId = db.Officers.Where(x => x.OfficerId == OfficerID).Select(x => x.BranchId).First();
            var branchLocation = db.Branches.Where(x => x.BranchId == branchId).Select(x => x.BranchLocation).First();
            RolesAPIController roleApi = new RolesAPIController();
            var role = roleApi.GetRoleForThisUser();
            if (role == "Admin" || role == "General Manager" ||  role == "IO Restructure")
            {
                return db.FileReferences
                         .Where(x => x.FileStatus == FileStatus)
                         .OrderByDescending(x => x.DateCreated);
            }
            else if (role == "SIO Operation" || role == "SIO Marketing")
            {
                return db.FileReferences
                         .Where(x => x.Branch == branchLocation && x.FileStatus == FileStatus)
                         .OrderByDescending(x => x.DateCreated);
            }
            else if (role == "SIO Branch Operation")
            {
                return db.FileReferences
                         .Where(x => (x.Branch == "Tanna" || x.Branch == "Malekula" || x.Branch == "Santo") && x.FileStatus == FileStatus)
                         .OrderByDescending(x => x.DateCreated);
            }
            return db.FileReferences
                     .Where(f => f.OfficerId == OfficerID && f.FileStatus == FileStatus)
                     .OrderByDescending(x => x.DateCreated);
        }

        /// <summary>
        /// Get All refunds File References
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("api/MemberFilesAPI/GetAllRefunds")]
        public IEnumerable<FileReferences> GetAllRefunds()
        {
            string FileStatus = "Refund";
            var OfficerID = User.Identity.GetUserId();
            int branchId = db.Officers.Where(x => x.OfficerId == OfficerID).Select(x => x.BranchId).First();
            var branchLocation = db.Branches.Where(x => x.BranchId == branchId).Select(x => x.BranchLocation).First();
            RolesAPIController roleApi = new RolesAPIController();
            var role = roleApi.GetRoleForThisUser();
            if (role == "Admin" || role == "General Manager" || role == "Accounts Payable" || role == "Accounts Receivable" || role == "Sr. Finance Officer")
            {
                return db.FileReferences
                         .Where(x => x.FileStatus == FileStatus)
                         .OrderByDescending(x => x.DateCreated);
            }
            else if (role == "SIO Operation" || role == "SIO Marketing" || role == "IO Validation")
            {
                return db.FileReferences
                         .Where(x => x.Branch == branchLocation && x.FileStatus == FileStatus)
                         .OrderByDescending(x => x.DateCreated);
            }
            else if (role == "SIO Branch Operation")
            {
                return db.FileReferences
                         .Where(x => (x.Branch == "Tanna" || x.Branch == "Malekula" || x.Branch == "Santo") && x.FileStatus == FileStatus)
                         .OrderByDescending(x => x.DateCreated);
            }
            return db.FileReferences
                     .Where(f => f.OfficerId == OfficerID && f.FileStatus == FileStatus)
                     .OrderByDescending(x => x.DateCreated);
        }

        /// <summary>
        /// Get All maintenance File References
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("api/MemberFilesAPI/GetAllMaintenance")]
        public IEnumerable<FileReferences> GetAllMaintenance()
        {
            string FileStatus = "Maintenance";
            var OfficerID = User.Identity.GetUserId();
            int branchId = db.Officers.Where(x => x.OfficerId == OfficerID).Select(x => x.BranchId).First();
            var branchLocation = db.Branches.Where(x => x.BranchId == branchId).Select(x => x.BranchLocation).First();
            RolesAPIController roleApi = new RolesAPIController();
            var role = roleApi.GetRoleForThisUser();
            if (role == "Admin" || role == "General Manager" || role == "IO Restructure")
            {
                return db.FileReferences
                         .Where(x => x.FileStatus == FileStatus)
                         .OrderByDescending(x => x.DateCreated);
            }
            else if (role == "SIO Operation" || role == "SIO Marketing")
            {
                return db.FileReferences
                         .Where(x => x.Branch == branchLocation && x.FileStatus == FileStatus)
                         .OrderByDescending(x => x.DateCreated);
            }
            else if (role == "SIO Branch Operation")
            {
                return db.FileReferences
                         .Where(x => (x.Branch == "Tanna" || x.Branch == "Malekula" || x.Branch == "Santo") && x.FileStatus == FileStatus)
                         .OrderByDescending(x => x.DateCreated);
            }
            return db.FileReferences
                     .Where(f => f.OfficerId == OfficerID && f.FileStatus == FileStatus)
                     .OrderByDescending(x => x.DateCreated);
        }

        /// <summary>
        /// Gets specific file reference where status is awaiting input
        /// based on member number
        /// </summary>
        /// <param name="memberNo">Member Number</param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/MemberFilesAPI/SearchAwaitingInput/{memberNo:int}")]
        public IEnumerable<FileReferences> SearchAwaitingInput(int memberNo)
        {
            string FileStatus = "Awaiting Input";
            var OfficerID = User.Identity.GetUserId();
            int branchId = db.Officers.Where(x => x.OfficerId == OfficerID).Select(x => x.BranchId).First();
            var branchLocation = db.Branches.Where(x => x.BranchId == branchId).Select(x => x.BranchLocation).First();
            RolesAPIController roleApi = new RolesAPIController();
            var role = roleApi.GetRoleForThisUser();
            if (role == "Admin" || role == "General Manager")
            {
                return db.FileReferences
                         .Where(x => x.FileStatus == FileStatus && x.MemberNo == memberNo)
                         .OrderByDescending(x => x.DateCreated);
            }
            else if (role == "SIO Operation" || role == "SIO Marketing")
            {
                return db.FileReferences
                         .Where(x => x.Branch == branchLocation && x.FileStatus == FileStatus && x.MemberNo == memberNo)
                         .OrderByDescending(x => x.DateCreated);
            }
            else if (role == "SIO Branch Operation")
            {
                return db.FileReferences
                         .Where(x => (x.Branch == "Tanna" || x.Branch == "Malekula" || x.Branch == "Santo") 
                         && x.FileStatus == FileStatus && x.MemberNo == memberNo)
                         .OrderByDescending(x => x.DateCreated);
            }
            return db.FileReferences
                     .Where(f => f.OfficerId == OfficerID 
                     && f.FileStatus == FileStatus && f.MemberNo == memberNo)
                     .OrderByDescending(x => x.DateCreated);
        }


        /// <summary>
        /// Get All Awaiting Payment File References
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("api/MemberFilesAPI/GetAllAwaitingPayment")]
        public IEnumerable<FileReferences> GetAllAwaitingPayment()
        {
            string FileStatus = "Awaiting Payment";
            var OfficerID = User.Identity.GetUserId();
            int branchId = db.Officers.Where(x => x.OfficerId == OfficerID).Select(x => x.BranchId).First();
            var branchLocation = db.Branches.Where(x => x.BranchId == branchId).Select(x => x.BranchLocation).First();
            RolesAPIController roleApi = new RolesAPIController();
            var role = roleApi.GetRoleForThisUser();
            if (role == "Admin" || role == "General Manager" || role == "Accounts Payable" || role == "Accounts Receivable" || role == "Sr. Finance Officer")
            {
                return db.FileReferences
                         .Where(x => x.FileStatus == FileStatus)
                         .OrderByDescending(x => x.DateCreated);
            }
            else if (role == "SIO Operation" || role == "SIO Marketing")
            {
                return db.FileReferences
                         .Where(x => x.Branch == branchLocation && x.FileStatus == FileStatus)
                         .OrderByDescending(x => x.DateCreated);
            }
            else if (role == "SIO Branch Operation")
            {
                return db.FileReferences
                         .Where(x => (x.Branch == "Tanna" || x.Branch == "Malekula" || x.Branch == "Santo") && x.FileStatus == FileStatus)
                         .OrderByDescending(x => x.DateCreated);
            }
            return db.FileReferences
                     .Where(f => f.OfficerId == OfficerID && f.FileStatus == FileStatus)
                     .OrderByDescending(x => x.DateCreated);
        }

        /// <summary>
        /// Gets specific file reference where status is Awaiting Input
        /// based on member number
        /// </summary>
        /// <param name="memberNo">Member Number</param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/MemberFilesAPI/SearchAwaitingPayment/{memberNo:int}")]
        public IEnumerable<FileReferences> SearchAwaitingPayment(int memberNo)
        {
            string FileStatus = "Awaiting Payment";
            var OfficerID = User.Identity.GetUserId();
            int branchId = db.Officers.Where(x => x.OfficerId == OfficerID).Select(x => x.BranchId).First();
            var branchLocation = db.Branches.Where(x => x.BranchId == branchId).Select(x => x.BranchLocation).First();
            RolesAPIController roleApi = new RolesAPIController();
            var role = roleApi.GetRoleForThisUser();
            if (role == "Admin" || role == "General Manager" || role == "Accounts Payable" || role == "Accounts Receivable" || role == "Sr. Finance Officer")
            {
                return db.FileReferences
                         .Where(x => x.FileStatus == FileStatus && 
                         x.MemberNo == memberNo).OrderByDescending(x => x.DateCreated);
            }
            else if (role == "SIO Operation" || role == "SIO Marketing")
            {
                return db.FileReferences
                         .Where(x => x.Branch == branchLocation && 
                         x.FileStatus == FileStatus && x.MemberNo == memberNo)
                         .OrderByDescending(x => x.DateCreated);
            }
            else if (role == "SIO Branch Operation")
            {
                return db.FileReferences
                         .Where(x => (x.Branch == "Tanna" || x.Branch == "Malekula" || x.Branch == "Santo")
                         && x.FileStatus == FileStatus && x.MemberNo == memberNo)
                         .OrderByDescending(x => x.DateCreated);
            }
            return db.FileReferences
                     .Where(f => f.OfficerId == OfficerID && f.FileStatus == FileStatus && f.MemberNo == memberNo)
                     .OrderByDescending(x => x.DateCreated);
        }

        /// <summary>
        /// Get All Awaiting Collateral File References
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("api/MemberFilesAPI/GetAllAwaitingCollateral")]
        public IEnumerable<FileReferences> GetAllAwaitingCollateral()
        {
            string FileStatus = "Awaiting Collateral";
            var OfficerID = User.Identity.GetUserId();
            int branchId = db.Officers.Where(x => x.OfficerId == OfficerID).Select(x => x.BranchId).First();
            var branchLocation = db.Branches.Where(x => x.BranchId == branchId).Select(x => x.BranchLocation).First();
            RolesAPIController roleApi = new RolesAPIController();
            var role = roleApi.GetRoleForThisUser();
            if (role == "Admin" || role == "General Manager" || role == "Collateral Officer" || 
                role == "Accounts Payable" || role == "Accounts Receivable" || role == "Sr. Finance Officer")
            {
                return db.FileReferences
                         .Where(x => x.FileStatus == FileStatus)
                         .OrderByDescending(x => x.DateCreated);
            }
            else if (role == "SIO Operation" || role == "SIO Marketing")
            {
                return db.FileReferences
                         .Where(x => x.Branch == branchLocation && x.FileStatus == FileStatus)
                         .OrderByDescending(x => x.DateCreated);
            }
            else if (role == "SIO Branch Operation")
            {
                return db.FileReferences
                         .Where(x => (x.Branch == "Tanna" || x.Branch == "Malekula" || x.Branch == "Santo") && x.FileStatus == FileStatus)
                         .OrderByDescending(x => x.DateCreated);
            }
            return db.FileReferences
                     .Where(f => f.OfficerId == OfficerID && f.FileStatus == FileStatus)
                     .OrderByDescending(x => x.DateCreated);
        }

        /// <summary>
        /// Gets specific file reference where status is Awaiting Collateral
        /// based on member number
        /// </summary>
        /// <param name="memberNo">Member Number</param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/MemberFilesAPI/SearchAwaitingCollateral/{memberNo:int}")]
        public IEnumerable<FileReferences> SearchAwaitingCollateral(int memberNo)
        {
            string FileStatus = "Awaiting Collateral";
            var OfficerID = User.Identity.GetUserId();
            int branchId = db.Officers.Where(x => x.OfficerId == OfficerID).Select(x => x.BranchId).First();
            var branchLocation = db.Branches.Where(x => x.BranchId == branchId).Select(x => x.BranchLocation).First();
            RolesAPIController roleApi = new RolesAPIController();
            var role = roleApi.GetRoleForThisUser();
            if (role == "Admin" || role == "General Manager" || role == "Collateral Officer" || 
                role == "Accounts Payable" || role == "Accounts Receivable" || role == "Sr. Finance Officer")
            {
                return db.FileReferences
                         .Where(x => x.FileStatus == FileStatus && x.MemberNo == memberNo)
                         .OrderByDescending(x => x.DateCreated);
            }
            else if (role == "SIO Operation" || role == "SIO Marketing")
            {
                return db.FileReferences
                         .Where(x => x.Branch == branchLocation && 
                         x.FileStatus == FileStatus && x.MemberNo == memberNo)
                         .OrderByDescending(x => x.DateCreated);
            }
            else if (role == "SIO Branch Operation")
            {
                return db.FileReferences
                         .Where(x => (x.Branch == "Tanna" || x.Branch == "Malekula" || x.Branch == "Santo") && 
                         x.FileStatus == FileStatus && x.MemberNo == memberNo)
                         .OrderByDescending(x => x.DateCreated);
            }
            return db.FileReferences
                     .Where(f => f.OfficerId == OfficerID && f.FileStatus == FileStatus && f.MemberNo == memberNo)
                     .OrderByDescending(x => x.DateCreated);
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
                    bool isCommitted = false;
                    #region Transaction 1 begins
                    //Create new member File
                    db.MemberFile.Add(memberFile);
                    db.SaveChanges();
                    #endregion Transaction 1 ends

                    #region Transaction 2 begins
                    //Create new file reference
                    var UserId = User.Identity.GetUserId();
                    int branchId = db.Officers.Where(x => x.OfficerId == UserId).Select(x => x.BranchId).First();
                    var branchLocation = db.Branches.Where(x => x.BranchId == branchId).Select(x => x.BranchLocation).First();
                    var query = db.Officers.Where(x => x.OfficerId == UserId).Select(i => new { i.EmpFname, i.EmpLname }).ToList();
                    string officerName = query[0].EmpFname + " " + query[0].EmpLname;
                    int fileNo = db.MemberFile.Where(x => x.OfficeId == UserId && x.MemberNo == memberFile.MemberNo)
                                                         .OrderByDescending(x => x.FileNo)
                                                         .Select(x => x.FileNo)
                                                         .First();

                    FileReferences NewFileRef = new FileReferences()
                    {
                        DateCreated = memberFile.DateCreated,
                        Branch = branchLocation,
                        OfficerId = UserId,
                        MemberNo = memberFile.MemberNo,
                        FileNo = fileNo,
                        FileStatus = "Pending Approval",
                        EmployerType = memberFile.EmployerType,
                        Officer = officerName
                    };

                    db.FileReferences.Add(NewFileRef);
                    db.SaveChanges();
                    #endregion Transaction 2 ends
                    
                    //Commit Transaction
                    transaction.Commit();
                    isCommitted = true;

                    if (isCommitted)
                    {
                        //Send email
                        EmailController api = new EmailController();
                        List<string> addressList = new List<string>();
                        var clientName = db.vnpf_.Where(x => x.VNPF_Number == memberFile.MemberNo).Select(x => x.Member_Fullname).First();
                        string emailBody = "You have a loan request pending approval from " + officerName + " for " + clientName + " (" + memberFile.MemberNo + ").";


                        if (branchLocation == "Port Vila")
                        {
                            var SIOMarketingRoleId = context.Roles.Where(x => x.Name == "SIO Marketing").Select(x => x.Id).First();
                            var SIOOperationRoleId = context.Roles.Where(x => x.Name == "SIO Operation").Select(x => x.Id).First();
                            var SIOMarketingEmailAddress = context.Users.Where(x => x.Roles.Any(u => u.RoleId.Equals(SIOMarketingRoleId))).Select(i => i.Email).First();
                            var SIOOperationEmailAddress = context.Users.Where(x => x.Roles.Any(u => u.RoleId.Equals(SIOOperationRoleId))).Select(i => i.Email).First();
                            addressList.Add(SIOMarketingEmailAddress);
                            addressList.Add(SIOOperationEmailAddress);
                            api.SendPendingApprovalNotif1(officerName,emailBody, addressList);
                        }
                        else
                        {
                            var SIOBranchOperationRoleId = context.Roles.Where(x => x.Name == "SIO Branch Operation").Select(x => x.Id).First();
                            var SIOBranchOperationEmailAddress = context.Users.Where(x => x.Roles.Any(u => u.RoleId.Equals(SIOBranchOperationRoleId))).Select(i => i.Email).First();
                            api.SendPendingApprovalNotif2(emailBody, SIOBranchOperationEmailAddress);

                        }
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
                    //Rollback Transaction
                    transaction.Rollback();
                }
            }
            return CreatedAtRoute("DefaultApi", new { id = memberFile.FileNo }, memberFile);
        }

        /// <summary>
        /// Update Loan Application file
        /// </summary>
        /// <param name="fileUpdateDTO">FileUpdateDTO</param>
        /// <returns></returns>
        [AcceptVerbs("PUT")]
        [Route("api/MemberFilesApi/UpdateLoanApp/")]
        [ResponseType(typeof(void))]
        public IHttpActionResult UpdateLoanApp(FileUpdateDTO fileUpdateDTO)
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
                    bool isCommitted = false;
                    #region Transaction 1 begins
                    //Update MemberFileTable
                    if (file == null)
                    {
                        return BadRequest();
                    }
                    var fileToUpdate = db.MemberFile.Where(x => x.FileNo == file.FileNo).First();
                    fileToUpdate.LoanApplication = fileUpdateDTO.LoanApplication;
                    fileToUpdate.FStatusId = 2;
                    db.SaveChanges();
                    #endregion Transaction 1 ends

                    #region Transaction 2 begins
                    //Update FileReferences Table
                    var RefToUpdate = db.FileReferences.Where(x => x.FileNo == file.FileNo).First();
                    RefToUpdate.LoanApprover = fileUpdateDTO.LoanApprover;
                    RefToUpdate.LoanApprovalDate = fileUpdateDTO.LoanApprovalDate;
                    RefToUpdate.FileStatus = "Awaiting Input";
                    db.SaveChanges();
                    #endregion Transaction 2 ends

                    //Commit Transaction
                    transaction.Commit();
                    isCommitted = true;
                    if (isCommitted)
                    {
                        var UserId = User.Identity.GetUserId();
                        var query = db.Officers.Where(x => x.OfficerId == UserId).Select(i => new { i.EmpFname, i.EmpLname }).ToList();
                        string ApproverName = query[0].EmpFname + " " + query[0].EmpLname;
                        var OfficerEmail = context.Users.Where(x => x.Id == file.OfficerId).Select(e => e.Email).First();
                        var clientName = db.vnpf_.Where(x => x.VNPF_Number == file.MemberNo).Select(x => x.Member_Fullname).First();                   
                        string emailBody = "Your loan request for member, "+ clientName + " ("+file.MemberNo+ ")" + " has been approved by " + ApproverName + "\non " + fileUpdateDTO.LoanApprovalDate.ToString() + ".";
                        EmailController api = new EmailController();
                        api.SendConfirmLoanApprovalNotif(clientName, emailBody, OfficerEmail);
                    }
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
        /// Update Payment Advice File
        /// </summary>
        /// <param name="fileUpdateDTO">FileUpdateDTO</param>
        /// <returns></returns>
        // PUT: api/MemberFilesAPI/5
        [AcceptVerbs("PUT")]
        [Route("api/MemberFilesApi/UpdatePaymentAdvice/")]
        [ResponseType(typeof(void))]
        public IHttpActionResult UpdatePaymentAdvice(FileUpdateDTO fileUpdateDTO)
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
                    bool isCommitted = false;
                    #region Transaction 1 begins
                    //Update MemberFileTable
                    if (file == null)
                    {
                        return BadRequest();
                    }
                    var fileToUpdate = db.MemberFile.Where(x => x.FileNo == file.FileNo).First();
                    fileToUpdate.PaymentAdvice = fileUpdateDTO.PaymentAdvice;
                    fileToUpdate.FStatusId = 3;
                    var result = db.SaveChanges();
                    #endregion Transaction 1 ends

                    #region Transaction 2 begins
                    //Update FileReferences Table
                    var RefToUpdate = db.FileReferences.Where(x => x.FileNo == file.FileNo).First();
                    RefToUpdate.FileStatus = "Awaiting Payment";
                    var result2 = db.SaveChanges();
                    #endregion Transaction 2 ends

                    //Commit Transaction
                    transaction.Commit();
                    isCommitted = true;
                    if (isCommitted)
                    {
                        EmailController api = new EmailController();
                        List<string> addressList = new List<string>();
                        var clientName = db.vnpf_.Where(x => x.VNPF_Number == fileToUpdate.MemberNo).Select(x => x.Member_Fullname).First();
                        string emailBody = "You have a loan request awaiting payment from " + RefToUpdate.Officer + " for " + clientName + " (" + fileToUpdate.MemberNo + ").";
                        var AccountsPayableRoleId = context.Roles.Where(x => x.Name == "Accounts Payable").Select(x => x.Id).First();
                        var AccountReceivableRoleId = context.Roles.Where(x => x.Name == "Accounts Receivable").Select(x => x.Id).First();
                        var SrFinanceOfficerRoleId = context.Roles.Where(x => x.Name == "Sr. Finance Officer").Select(x => x.Id).First();
                        //var AccountsPayableEmailAddress = context.Users.Where(x => x.Roles.Any(u => u.RoleId.Equals(AccountsPayableRoleId))).Select(i => i.Email).First();
                        //var AccountReceivableEmailAddress = context.Users.Where(x => x.Roles.Any(u => u.RoleId.Equals(AccountReceivableRoleId))).Select(i => i.Email).First();
                        //var SrFinanceOfficerEmailAddress = context.Users.Where(x => x.Roles.Any(u => u.RoleId.Equals(SrFinanceOfficerRoleId))).Select(i => i.Email).First();
                        //addressList.Add(AccountsPayableEmailAddress);
                        //addressList.Add(AccountReceivableEmailAddress);
                        //addressList.Add(SrFinanceOfficerEmailAddress);
                        api.SendAwaitingPaymentNotif(RefToUpdate.Officer, emailBody, addressList);

                    }
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
        /// Update Payment Advice and Cheque Copy File
        /// </summary>
        /// <param name="fileUpdateDTO">FileUpdateDTO</param>
        /// <returns></returns>
        // PUT: api/MemberFilesAPI/5
        [AcceptVerbs("PUT")]
        [Route("api/MemberFilesApi/UpdatePaymentAndCheque/")]
        [ResponseType(typeof(void))]
        public IHttpActionResult UpdatePaymentAndCheque(FileUpdateDTO fileUpdateDTO)
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
                    bool isCommitted = false;
                    #region Transaction 1 begins
                    //Update MemberFileTable
                    if (file == null)
                    {
                        return BadRequest();
                    }
                    var fileToUpdate = db.MemberFile.Where(x => x.FileNo == file.FileNo).First();
                    fileToUpdate.PaymentAdvice = fileUpdateDTO.PaymentAdvice;
                    fileToUpdate.ChequeCopy = fileUpdateDTO.ChequeCopy;
                    fileToUpdate.FStatusId = 4;
                    db.SaveChanges();
                    #endregion Transaction 1 ends

                    #region Transaction 2 begins
                    //Update FileReferences Table
                    var RefToUpdate = db.FileReferences.Where(x => x.FileNo == file.FileNo).First();
                    RefToUpdate.PaymentOfficer = fileUpdateDTO.PaymentOfficer;
                    RefToUpdate.PaymentDate = fileUpdateDTO.PaymentDate;
                    RefToUpdate.FileStatus = "Awaiting Collateral";
                    db.SaveChanges();
                    #endregion Transaction 2 ends

                    //Commit Transaction
                    transaction.Commit();
                    isCommitted = true;
                    if (isCommitted)
                    {
                        EmailController api = new EmailController();
                        var UserId = User.Identity.GetUserId();
                        var query = db.Officers.Where(x => x.OfficerId == UserId).Select(i => new { i.EmpFname, i.EmpLname }).ToList();
                        string ApproverName = query[0].EmpFname + " " + query[0].EmpLname;
                        var OfficerEmail = context.Users.Where(x => x.Id == file.OfficerId).Select(e => e.Email).First();
                        var clientName = db.vnpf_.Where(x => x.VNPF_Number == file.MemberNo).Select(x => x.Member_Fullname).First();
                        string emailBody = "Your loan request for member, " + clientName + " (" + file.MemberNo + ")" + " has been paid by " + ApproverName + "\non " + fileUpdateDTO.PaymentDate.ToString() + ".";
                        api.SendRequestPaymentConfirmationNotif(clientName, emailBody, OfficerEmail);

                        List<string> addressList = new List<string>();
                        string emailBody2 = "You have a loan request awaiting collateral certifate from " + RefToUpdate.Officer + " for " + clientName + " (" + fileToUpdate.MemberNo + ").";
                        var CollateralOfficerRoleId = context.Roles.Where(x => x.Name == "Collateral Officer").Select(x => x.Id).First();
                        var CollateralOfficerEmailAddress = context.Users.Where(x => x.Roles.Any(u => u.RoleId.Equals(CollateralOfficerRoleId))).Select(i => i.Email).First();
                        addressList.Add(CollateralOfficerEmailAddress);
                        api.SendAwaitingCollateralNotif(RefToUpdate.Officer, emailBody2, addressList);
                    }
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
        /// Update Collateral File
        /// </summary>
        /// <param name="fileUpdateDTO">FileUpdateDTO</param>
        /// <returns></returns>
        // PUT: api/MemberFilesAPI/5
        [AcceptVerbs("PUT")]
        [Route("api/MemberFilesApi/UploadCollateral/")]
        [ResponseType(typeof(void))]
        public IHttpActionResult UploadCollateral(FileUpdateDTO fileUpdateDTO)
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
                    bool isCommitted = false;
                    #region Transaction 1 begins
                    //Update MemberFileTable
                    if (file == null)
                    {
                        return BadRequest();
                    }
                    var fileToUpdate = db.MemberFile.Where(x => x.FileNo == file.FileNo).First();
                    fileToUpdate.Collateral = fileUpdateDTO.CollateralCertificate;
                    fileToUpdate.FStatusId = 5;
                    db.SaveChanges();
                    #endregion Transaction 1 ends

                    #region Transaction 2 begins
                    //Update FileReferences Table
                    var RefToUpdate = db.FileReferences.Where(x => x.FileNo == file.FileNo).First();
                    RefToUpdate.CollateralOfficer = fileUpdateDTO.CollateralOfficer;
                    RefToUpdate.CollateralDate = fileUpdateDTO.CollateralDate;
                    RefToUpdate.FileStatus = "Finalized";
                    db.SaveChanges();
                    #endregion Transaction 2 ends

                    //Commit Transaction
                    transaction.Commit();
                    isCommitted = true;
                    if (isCommitted)
                    {
                        var UserId = User.Identity.GetUserId();
                        var query = db.Officers.Where(x => x.OfficerId == UserId).Select(i => new { i.EmpFname, i.EmpLname }).ToList();
                        string ApproverName = query[0].EmpFname + " " + query[0].EmpLname;
                        var OfficerEmail = context.Users.Where(x => x.Id == file.OfficerId).Select(e => e.Email).First();
                        var clientName = db.vnpf_.Where(x => x.VNPF_Number == file.MemberNo).Select(x => x.Member_Fullname).First();
                        string emailBody = "Loan request collateral certificate for member, " + clientName + " (" + file.MemberNo + ")" + " has been attached by " + ApproverName + "\non " + fileUpdateDTO.CollateralDate.ToString() + ".";
                        EmailController api = new EmailController();
                        api.SendCollateralConfirmationNotif(clientName, emailBody, OfficerEmail);
                    }
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