using Microsoft.AspNet.Identity;
using RESTServices.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;

namespace RESTServices.Controllers
{
    /// <summary>
    /// Update Files Controller - Handles File Status Updates
    /// </summary>
    public class UpdateFilesAPIController : ApiController
    {
        private MFSLEntities db = new MFSLEntities();
        ApplicationDbContext context;
        AccountController Account;
        /// <summary>
        /// Constructor
        /// </summary>
        public UpdateFilesAPIController()
        {
            Account = new AccountController();
            context = new ApplicationDbContext();
        }

        /// <summary>
        /// Check if file is in progress
        /// </summary>
        /// <param name="fileNo"> File Number</param>
        /// <returns>200 or 401 or 423</returns>
        [HttpGet]
        [Route("api/UpdateFilesAPI/CheckFileProgress/{fileNo:int}")]
        public int CheckFileProgress(int fileNo)
        {
            try
            {
                var file = db.FileReferences.Where(x => x.FileNo == fileNo).First();
                if (file != null)
                {
                    RolesAPIController api = new RolesAPIController();
                    var role = api.GetRoleForThisUser();
                    if (role == "Admin")
                    {
                        return 200; //Ok
                    }

                    var userId = User.Identity.GetUserId();
                    var officerId = db.FileReferences.Where(x => x.FileNo == fileNo).Select(o => o.OfficerId).First();
                    var loanApprover = db.FileReferences.Where(x => x.FileNo == fileNo).Select(x => x.LoanApprover).First();
                    var paymentOfficer = db.FileReferences.Where(x => x.FileNo == fileNo).Select(x => x.PaymentOfficer).First();
                    var collateralOfficer = db.FileReferences.Where(x => x.FileNo == fileNo).Select(x => x.CollateralOfficer).First();
                    if (role == "IO Restructure" || userId == officerId)
                    {
                        if (loanApprover != null && paymentOfficer != null && collateralOfficer != null)
                        {
                            return 200; //Ok
                        }
                        else
                        {
                            return 423; //Locked
                        }
                    }
                    return 401; //Unauthorized
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e.InnerException);
                if (e.InnerException == null)
                {
                    return 404; //Not Found
                }

            }

            return 500; //Internal Server Error
        }
        /// <summary>
        /// Returns all file Status
        /// </summary>
        /// <returns>List of file statuses</returns>
        [HttpGet]
        [Route("api/UpdateFilesAPI/GetAllFileStatus")]
        public List<string> GetAllFileStatus()
        {
            RolesAPIController api = new RolesAPIController();
            var UserRole = api.GetRoleForThisUser();
            if(UserRole != "Admin" && UserRole != "IO Restructure")
            {
                return db.FileStatus.Where
                    (
                        u => !u.FStatus.Contains("Pending Approval") && 
                             !u.FStatus.Contains("Awaiting Input") &&
                             !u.FStatus.Contains("Awaiting Payment") &&
                             !u.FStatus.Contains("Awaiting Collateral") &&
                             !u.FStatus.Contains("Finalilzed") &&
                             !u.FStatus.Contains("Arrears Cleared") &&
                             !u.FStatus.Contains("Paid Refund")
                    )
                    .Select(x => x.FStatus).ToList();
            }
            if (UserRole == "IO Restructure")
            {
                return db.FileStatus.Where
                (
                    u => !u.FStatus.Contains("Pending Approval") &&
                         !u.FStatus.Contains("Awaiting Input") &&
                         !u.FStatus.Contains("Awaiting Payment") &&
                         !u.FStatus.Contains("Awaiting Collateral") &&
                         !u.FStatus.Contains("Finalilzed") &&
                         !u.FStatus.Contains("Paid Refund")
                )
                .Select(x => x.FStatus).ToList();
            }
            return db.FileStatus.Select(x => x.FStatus).ToList();
        }

        [AcceptVerbs("PUT")]
        [Route("api/UpdateFilesAPI/UpdateMaintenanceFile/")]
        [ResponseType(typeof(void))]
        public IHttpActionResult UpdateMaintenanceFile(MaintenanceBindingModel bindingModel)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors);
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    var fileRef = db.FileReferences.Where(x => x.FileNo == bindingModel.FileNo).First();
                    if (fileRef == null)
                    {
                        return BadRequest();
                    }
                    bool isCommitted = false;
                    string currentStatus = fileRef.FileStatus;

                    #region Transaction 1 begins
                    var fileToUpdate = db.MemberFile.Where(x => x.FileNo == bindingModel.FileNo).First();
                    fileToUpdate.MaintenanceForm = bindingModel.MaintenanceForm;
                    fileToUpdate.FStatusId = 1008;
                    db.SaveChanges();
                    #endregion Transaction 1 ends

                    #region Transaction 3 begins
                    //Update FileReferences Table
                    fileRef.FileStatus = bindingModel.FileStatus;
                    fileRef.Comment = bindingModel.Comment + " \n [Date: " + DateTime.Now.ToString() + "]";
                    db.SaveChanges();
                    #endregion End of Transaction 3

                    transaction.Commit();
                    isCommitted = true;
                    if (isCommitted)
                    {
                        var UserId = User.Identity.GetUserId();
                        var UserEmail = context.Users.Where(x => x.Id == UserId).Select(e => e.Email).First();
                        var query = db.Officers.Where(x => x.OfficerId == UserId).Select(i => new { i.EmpFname, i.EmpLname }).ToList();
                        string officerName = query[0].EmpFname + " " + query[0].EmpLname;
                        int branchId = db.Officers.Where(x => x.OfficerId == UserId).Select(x => x.BranchId).First();
                        var branchLocation = db.Branches.Where(x => x.BranchId == branchId).Select(x => x.BranchLocation).First();

                        //Send email alert
                        EmailController api = new EmailController();
                        api.SendEmailAlert(fileRef.MemberNo, currentStatus, fileRef.FileStatus, fileRef.Comment, UserEmail, officerName, branchLocation);
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
                    return BadRequest();
                }
            }
            return Ok();
        }

        /// <summary>
        /// Update file status by file number
        /// </summary>
        /// <param name="fileUpdateDTO">fileUpdateDTO</param>
        /// <returns></returns>
        [AcceptVerbs("PUT")]
        [Route("api/UpdateFilesAPI/UpdateFileStatus/")]
        [ResponseType(typeof(void))]
        public IHttpActionResult UpdateFileStatus(UpdateFileStatusViewModel fileUpdateDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            //Create Transaction
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    var fileRef = db.FileReferences.Where(x => x.FileNo == fileUpdateDTO.FileNo).First();
                    if (fileRef == null)
                    {
                        return BadRequest();
                    }
                    bool isCommitted = false;
                    #region Transaction 1 begins
                    //Update MemberFileTable
                    string currentStatus = fileRef.FileStatus;
                    var fileToUpdate = db.MemberFile.Where(x => x.FileNo == fileRef.FileNo).First();
                    //Need to refactor this piece - create a dictionary
                    if(fileUpdateDTO.FileStatus == "Pending Approval")
                    {
                        fileToUpdate.FStatusId = 1;
                    }
                    else if (fileUpdateDTO.FileStatus == "Awaiting Input")
                    {
                        fileToUpdate.FStatusId = 2;
                    }
                    else if (fileUpdateDTO.FileStatus == "Awaiting Payment")
                    {
                        fileToUpdate.FStatusId = 3;
                    }
                    else if (fileUpdateDTO.FileStatus == "Awaiting Collateral")
                    {
                        fileToUpdate.FStatusId = 4;
                    }
                    else if (fileUpdateDTO.FileStatus == "Finalilzed")
                    {
                        fileToUpdate.FStatusId = 5;
                    }
                    else if (fileUpdateDTO.FileStatus == "Demand")
                    {
                        fileToUpdate.FStatusId = 6;
                    }
                    else if (fileUpdateDTO.FileStatus == "Arrears")
                    {
                        fileToUpdate.FStatusId = 7;
                    }
                    else if (fileUpdateDTO.FileStatus == "Paid Refund")
                    {
                        fileToUpdate.FStatusId = 1009;
                    }
                    else if (fileUpdateDTO.FileStatus == "Arrears Cleared")
                    {
                        fileToUpdate.FStatusId = 1010;
                    }
                    db.SaveChanges();
                    #endregion Transaction 1 ends

                    #region Transaction 2 begins
                    //Update FileReferences Table
                    fileRef.FileStatus = fileUpdateDTO.FileStatus;
                    fileRef.Comment = fileUpdateDTO.Comment + " \n [Date: " + DateTime.Now.ToString()+"]";
                    db.SaveChanges();
                    #endregion Transaction 2 ends

                    //Commit Transaction
                    transaction.Commit();
                    isCommitted = true;
                    if (isCommitted)
                    {
                        var UserId = User.Identity.GetUserId();
                        var UserEmail = context.Users.Where(x => x.Id == UserId).Select(e => e.Email).First();
                        var query = db.Officers.Where(x => x.OfficerId == UserId).Select(i => new { i.EmpFname, i.EmpLname }).ToList();
                        string officerName = query[0].EmpFname + " " + query[0].EmpLname;
                        int branchId = db.Officers.Where(x => x.OfficerId == UserId).Select(x => x.BranchId).First();
                        var branchLocation = db.Branches.Where(x => x.BranchId == branchId).Select(x => x.BranchLocation).First();

                        //Send email alert
                        EmailController api = new EmailController();
                        api.SendEmailAlert(fileRef.MemberNo, currentStatus, fileRef.FileStatus, fileRef.Comment, UserEmail,officerName,branchLocation);
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    //Rollback Transaction
                    transaction.Rollback();
                    return BadRequest();
                }
            }
            return StatusCode(HttpStatusCode.NoContent);
        }
    }
}
