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
    public class RefundsAPIController : ApiController
    {
        private MFSLEntities db = new MFSLEntities();
        ApplicationDbContext context;
        AccountController Account;

        public RefundsAPIController()
        {
            Account = new AccountController();
            context = new ApplicationDbContext();
        }

        [AcceptVerbs("POST")]
        [Route("api/RefundsAPI/PostToRefunds/")]
        [ResponseType(typeof(void))]
        public IHttpActionResult PostToRefunds(RefundsBindingModel bindingModel)
        {
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
                    var fileRef = db.FileReferences.Where(x => x.FileNo == bindingModel.FileNo).First();
                    if (fileRef == null)
                    {
                        return BadRequest();
                    }
                    bool isCommitted = false;
                    string currentStatus = fileRef.FileStatus;
                    #region Transaction 1 begins
                    //Create new refund file
                    Refunds newRefund = new Refunds()
                    {
                        FileNo = bindingModel.FileNo,
                        RequestDate = bindingModel.RequestDate,
                        PaymentRequest = bindingModel.PaymentRequest,
                        LoanStatement = bindingModel.LoanStatement,
                        ReconciliationSheet = bindingModel.ReconciliationSheet,
                    };
                    db.Refunds.Add(newRefund);
                    db.SaveChanges();
                    #endregion Transaction 1 ends

                    #region Transaction 2 begins
                    //MemberFiles.FStatusId should be included
                    var fileToUpdate = db.MemberFile.Where(x => x.FileNo == bindingModel.FileNo).First();
                    fileToUpdate.FStatusId = 8;
                    db.SaveChanges();
                    #endregion End of transaction 2

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
                        api.SendEmailAlert(fileRef.MemberNo,currentStatus,fileRef.FileStatus,fileRef.Comment,UserEmail,officerName,branchLocation);
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
    }
}