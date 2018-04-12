using Microsoft.AspNet.Identity;
using RESTServices.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
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

        public UpdateFilesAPIController()
        {
            Account = new AccountController();
            context = new ApplicationDbContext();
        }

        /// <summary>
        /// Check if file is in progress
        /// </summary>
        /// <param name="fileNo"> File Number</param>
        /// <returns>true or false</returns>
        [HttpGet]
        [Route("api/UpdateFilesAPI/CheckFileProgress/{fileNo:int}")]
        public bool CheckFileProgress(int fileNo)
        {
            RolesAPIController api = new RolesAPIController();
            var role = api.GetRoleForThisUser();
            if (role == "Admin")
            {
                return true;
            }
            var loanApprover = db.FileReferences.Where(x => x.FileNo == fileNo).Select(x => x.Approver).First();
            var paymentOfficer = db.FileReferences.Where(x => x.FileNo == fileNo).Select(x => x.PaymentOfficer).First();
            var collateralOfficer = db.FileReferences.Where(x => x.FileNo == fileNo).Select(x => x.CollateralOfficer).First();
            if( loanApprover != null && paymentOfficer != null && collateralOfficer != null)
            {
                return true;
            }
            return false;
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
            if(UserRole != "Admin")
            {
                return db.FileStatus.Where
                    (
                        u => !u.FStatus.Contains("Pending Approval") && 
                             !u.FStatus.Contains("Awaiting Input") &&
                             !u.FStatus.Contains("Awaiting Payment") &&
                             !u.FStatus.Contains("Awaiting Collateral") &&
                             !u.FStatus.Contains("Finalilzed")
                    )
                    .Select(x => x.FStatus).ToList();
            }
            return db.FileStatus.Select(x => x.FStatus).ToList();
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
                var file = db.FileReferences.Where(x => x.FileNo == fileUpdateDTO.FileNo).First();
                string CurrentStatus = file.FileStatus;
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
                    else if (fileUpdateDTO.FileStatus == "Restructure")
                    {
                        fileToUpdate.FStatusId = 8;
                    }

                    db.SaveChanges();
                    #endregion Transaction 1 ends

                    #region Transaction 2 begins
                    //Update FileReferences Table
                    var RefToUpdate = db.FileReferences.Where(x => x.FileNo == file.FileNo).First();
                    RefToUpdate.FileStatus = fileUpdateDTO.FileStatus;
                    RefToUpdate.Comment = fileUpdateDTO.Comment + " \n [Date: " + DateTime.Now.ToString()+"]";
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

                        //Send email
                        EmailController api = new EmailController();
                        List<string> addressList = new List<string>();
                        var clientName = db.vnpf_.Where(x => x.VNPF_Number == RefToUpdate.MemberNo).Select(x => x.Member_Fullname).First();
                        //This message will be sent to marketing and operations
                        string emailBody = "Loan status for member " + clientName + " (" + RefToUpdate.MemberNo + "), has been changed from " +
                                            CurrentStatus + " \nto " + RefToUpdate.FileStatus + " by " + officerName + " on " + DateTime.Now.ToString() + ".\n" +
                                            "Officer's comment: " + RefToUpdate.Comment;

                        string emailBody2 = "You've changed the loan status for member " + clientName + " (" + RefToUpdate.MemberNo + "), from " +
                                            CurrentStatus + " \nto " + RefToUpdate.FileStatus + " on " + DateTime.Now.ToString() + ".\n" +
                                            "Your comment: " + RefToUpdate.Comment;

                        api.SendChangedStatusNotif(RefToUpdate.FileStatus, emailBody2, UserEmail);
                        if (branchLocation == "Port Vila")
                        {
                            var SIOMarketingRoleId = context.Roles.Where(x => x.Name == "SIO Marketing").Select(x => x.Id).First();
                            var SIOOperationRoleId = context.Roles.Where(x => x.Name == "SIO Operation").Select(x => x.Id).First();
                            var SIOMarketingEmailAddress = context.Users.Where(x => x.Roles.Any(u => u.RoleId.Equals(SIOMarketingRoleId))).Select(i => i.Email).First();
                            var SIOOperationEmailAddress = context.Users.Where(x => x.Roles.Any(u => u.RoleId.Equals(SIOOperationRoleId))).Select(i => i.Email).First();
                            addressList.Add(SIOMarketingEmailAddress);
                            addressList.Add(SIOOperationEmailAddress);
                            api.BroadCastChangedStatusNotif1(RefToUpdate.FileStatus, emailBody, addressList);
                        }
                        else
                        {
                            var SIOBranchOperationRoleId = context.Roles.Where(x => x.Name == "SIO Branch Operation").Select(x => x.Id).First();
                            var SIOBranchOperationEmailAddress = context.Users.Where(x => x.Roles.Any(u => u.RoleId.Equals(SIOBranchOperationRoleId))).Select(i => i.Email).First();
                            api.BroadCastChangedStatusNotif2(RefToUpdate.FileStatus, emailBody, emailBody);
                        }
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

        private bool MemberFileExists(int id)
        {
            return db.MemberFile.Count(e => e.FileNo == id) > 0;
        }
    }
}
