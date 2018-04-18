using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using RESTServices.Models;
namespace RESTServices.Controllers
{
    /// <summary>
    /// Email Controller handles all email messaging
    /// </summary>
    public class EmailController : Controller
    {
        private MFSLEntities db = new MFSLEntities();
        ApplicationDbContext context;
        AccountController Account;

        public EmailController()
        {
            Account = new AccountController();
            context = new ApplicationDbContext();
        }

        [NonAction]
        public void SendEmailAlert(int memberNo, string prevStatus, string newStatus, string comment, string officerEmail, string officerName, string branchLocation)
        {
            List<string> addressList = new List<string>();
            var clientName = db.vnpf_.Where(x => x.VNPF_Number == memberNo).Select(x => x.Member_Fullname).First();
            //This message will be send to issuing officer
            string emailBody1 = "You've changed the loan status for member " + clientName + " (" + memberNo + "), from " +
                                prevStatus + " \nto " + newStatus + " on " + DateTime.Now.ToString() + ".\n" +
                                "Your comment: " + comment;
            //This message will be sent to marketing and operations
            string emailBody2 = "Loan status for member " + clientName + " (" + memberNo + "), has been changed from " +
                                prevStatus + " \nto " + newStatus + " by " + officerName + " on " + DateTime.Now.ToString() + ".\n" +
                                "Officer's comment: " + comment;

            SendChangedStatusNotif(newStatus, emailBody1, officerEmail);
            if (branchLocation == "Port Vila")
            {
                var SIOMarketingRoleId = context.Roles.Where(x => x.Name == "SIO Marketing").Select(x => x.Id).First();
                var SIOOperationRoleId = context.Roles.Where(x => x.Name == "SIO Operation").Select(x => x.Id).First();
                var SIOMarketingEmailAddress = context.Users.Where(x => x.Roles.Any(u => u.RoleId.Equals(SIOMarketingRoleId))).Select(i => i.Email).First();
                var SIOOperationEmailAddress = context.Users.Where(x => x.Roles.Any(u => u.RoleId.Equals(SIOOperationRoleId))).Select(i => i.Email).First();

                addressList.Add(SIOMarketingEmailAddress);
                addressList.Add(SIOOperationEmailAddress);
                BroadCastChangedStatusNotif1(newStatus, emailBody2, addressList);
                AlertRestructure(newStatus,comment,memberNo,officerName,clientName);
                AlertValidation(newStatus,comment, memberNo, officerName, clientName);
            }
            else
            {
                var SIOBranchOperationRoleId = context.Roles.Where(x => x.Name == "SIO Branch Operation").Select(x => x.Id).First();
                var SIOBranchOperationEmailAddress = context.Users.Where(x => x.Roles.Any(u => u.RoleId.Equals(SIOBranchOperationRoleId))).Select(i => i.Email).First();
                BroadCastChangedStatusNotif2(newStatus, emailBody2, SIOBranchOperationEmailAddress);
                AlertRestructure(newStatus,comment, memberNo, officerName, clientName);
            }
        }

        [NonAction]
        public void AlertRestructure(string fileStatus, string comment, int memberNo, string officerName, string clientName)
        {
            if (fileStatus == "Maintenance")
            {
                var IORestructureRoleId = context.Roles.Where(x => x.Name == "IO Restructure").Select(x => x.Id).First();
                var IORestructureEmailAddress = context.Users.Where(x => x.Roles.Any(u => u.RoleId.Equals(IORestructureRoleId))).Select(i => i.Email).First();
                //AlertRestructureOfficer
                string emailBody = "You have a file pending maintenance from " + officerName + "for member, " + clientName + " (" + memberNo + "). \n"+
                "Officer's comment: " + comment;
                AlertRestructureOfficer(officerName, emailBody, IORestructureEmailAddress);
            }
        }

        [NonAction]
        public void AlertValidation(string fileStatus,string comment, int memberNo, string officerName, string clientName)
        {
            if (fileStatus == "Refund")
            {
                var IOValidationRoleId = context.Roles.Where(x => x.Name == "IO Validation").Select(x => x.Id).First();
                var IOValidationEmailAddress = context.Users.Where(x => x.Roles.Any(u => u.RoleId.Equals(IOValidationRoleId))).Select(i => i.Email).First();
                //AlertValidationOfficer
                string emailBody = "You have a file pending refund from " + officerName + "for member, " + clientName + " (" + memberNo + "). \n" +
                "Officer's comment: " + comment;
                AlertValidationOfficer(officerName, emailBody, IOValidationEmailAddress);
            }
        }

        [NonAction]
        public void AlertRestructureOfficer(string subjInfo, string emailBody, string emailAddress)
        {
            MailMessage mailMessage = new MailMessage();
            mailMessage.From = new MailAddress("samsamson2016@gmail.com");
            //mailMessage.To.Add(addressList.First());
            //mailMessage.To.Add(addressList.Last());
            mailMessage.To.Add("s11075775@student.usp.ac.fj");
            mailMessage.Subject = "[DO NOT REPLY] Pending Maintenance - " + subjInfo;
            mailMessage.Body = emailBody;
            //mailMessage.IsBodyHtml = true;

            SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", 587);
            smtpClient.UseDefaultCredentials = false;
            smtpClient.Credentials = new System.Net.NetworkCredential()
            {
                UserName = "samsamson2016@gmail.com",
                Password = "1215Jean.b45"
            };
            smtpClient.EnableSsl = true;
            try
            {
                smtpClient.Send(mailMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        [NonAction]
        public void AlertValidationOfficer(string subjInfo, string emailBody, string emailAddress)
        {
            MailMessage mailMessage = new MailMessage();
            mailMessage.From = new MailAddress("samsamson2016@gmail.com");
            //mailMessage.To.Add(addressList.First());
            //mailMessage.To.Add(addressList.Last());
            mailMessage.To.Add("s11075775@student.usp.ac.fj");
            mailMessage.Subject = "[DO NOT REPLY] Pending Refund-" + subjInfo;
            mailMessage.Body = emailBody;
            //mailMessage.IsBodyHtml = true;

            SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", 587);
            smtpClient.UseDefaultCredentials = false;
            smtpClient.Credentials = new System.Net.NetworkCredential()
            {
                UserName = "samsamson2016@gmail.com",
                Password = "1215Jean.b45"
            };
            smtpClient.EnableSsl = true;
            try
            {
                smtpClient.Send(mailMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Send Pending Approval Notification within Main Office Port Vila
        /// </summary>
        /// <param name="subjInfo"></param>
        /// <param name="emailbody"></param>
        /// <param name="addressList"></param>
        [NonAction]
        public void SendPendingApprovalNotif1(string subjInfo, string emailbody, List<string> addressList)
        {
            MailMessage mailMessage = new MailMessage();
            mailMessage.From = new MailAddress("samsamson2016@gmail.com");
            //mailMessage.To.Add(addressList.First());
            //mailMessage.To.Add(addressList.Last());
            mailMessage.To.Add("s11075775@student.usp.ac.fj");
            mailMessage.Subject = "[DO NOT REPLY] Pending Loan Request - " +subjInfo;
            mailMessage.Body = emailbody;
            //mailMessage.IsBodyHtml = true;

            SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", 587);
            smtpClient.UseDefaultCredentials = false;
            smtpClient.Credentials = new System.Net.NetworkCredential()
            {
                UserName = "samsamson2016@gmail.com",
                Password = "1215Jean.b45"
            };
            smtpClient.EnableSsl = true;
            try
            {
                smtpClient.Send(mailMessage);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }

        /// <summary>
        /// Send Pending Approval Notification to Operation Santo Branch
        /// </summary>
        /// <param name="emailbody"></param>
        /// <param name="emailAddress"></param>
        [NonAction]
        public void SendPendingApprovalNotif2(string emailbody, string emailAddress)
        {
            MailMessage mailMessage = new MailMessage();
            mailMessage.From = new MailAddress("samsamson2016@gmail.com");
            mailMessage.To.Add("s11075775@student.usp.ac.fj");
            mailMessage.Subject = "[DO NOT REPLY] Pending Loan Request";
            mailMessage.Body = emailbody;

            SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", 587);
            smtpClient.UseDefaultCredentials = false;
            smtpClient.Credentials = new System.Net.NetworkCredential()
            {
                UserName = "samsamson2016@gmail.com",
                Password = "1215Jean.b45"
            };
            smtpClient.EnableSsl = true;
            try
            {
                smtpClient.Send(mailMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }

        /// <summary>
        /// Send loan approval confirmation
        /// </summary>
        /// <param name="memberInfo">Member info</param>
        /// <param name="emailbody">Email body</param>
        /// <param name="emailAddress">Email address</param>
        [NonAction]
        public void SendConfirmLoanApprovalNotif(string memberInfo, string emailbody, string emailAddress)
        {
            MailMessage mailMessage = new MailMessage();
            mailMessage.From = new MailAddress("samsamson2016@gmail.com");
            //mailMessage.To.Add(emailAddress);
            mailMessage.To.Add("s11075775@student.usp.ac.fj");
            mailMessage.Subject = "[DO NOT REPLY] Loan Request Approved for " + memberInfo;
            mailMessage.Body = emailbody;

            SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", 587);
            smtpClient.UseDefaultCredentials = false;
            smtpClient.Credentials = new System.Net.NetworkCredential()
            {
                UserName = "samsamson2016@gmail.com",
                Password = "1215Jean.b45"
            };
            smtpClient.EnableSsl = true;
            try
            {
                smtpClient.Send(mailMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }

        /// <summary>
        /// Send Awaiting payment notification to payment officer
        /// </summary>
        /// <param name="subjInfo"></param>
        /// <param name="emailbody"></param>
        /// <param name="addressList"></param>
        [NonAction]
        public void SendAwaitingPaymentNotif(string subjInfo, string emailbody, List<string> addressList)
        {
            MailMessage mailMessage = new MailMessage();
            mailMessage.From = new MailAddress("samsamson2016@gmail.com");
            //mailMessage.To.Add(addressList[0]);
            //mailMessage.To.Add(addressList[1]);
            //mailMessage.To.Add(addressList[2]);
            mailMessage.To.Add("s11075775@student.usp.ac.fj");
            mailMessage.Subject = "[DO NOT REPLY] Loan Request Awaiting Payment - " + subjInfo;
            mailMessage.Body = emailbody;
            //mailMessage.IsBodyHtml = true;

            SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", 587);
            smtpClient.UseDefaultCredentials = false;
            smtpClient.Credentials = new System.Net.NetworkCredential()
            {
                UserName = "samsamson2016@gmail.com",
                Password = "1215Jean.b45"
            };
            smtpClient.EnableSsl = true;
            try
            {
                smtpClient.Send(mailMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }

        /// <summary>
        /// Send loan payment confirmation
        /// </summary>
        /// <param name="memberInfo">Member info</param>
        /// <param name="emailbody">Email body</param>
        /// <param name="emailAddress">Email address</param>
        [NonAction]
        public void SendRequestPaymentConfirmationNotif(string memberInfo, string emailbody, string emailAddress)
        {
            MailMessage mailMessage = new MailMessage();
            mailMessage.From = new MailAddress("samsamson2016@gmail.com");
            //mailMessage.To.Add(emailAddress);
            mailMessage.To.Add("s11075775@student.usp.ac.fj");
            mailMessage.Subject = "[DO NOT REPLY] Loan Request Paid for " + memberInfo;
            mailMessage.Body = emailbody;

            SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", 587);
            smtpClient.UseDefaultCredentials = false;
            smtpClient.Credentials = new System.Net.NetworkCredential()
            {
                UserName = "samsamson2016@gmail.com",
                Password = "1215Jean.b45"
            };
            smtpClient.EnableSsl = true;
            try
            {
                smtpClient.Send(mailMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }

        [NonAction]
        public void SendAwaitingCollateralNotif(string subjInfo, string emailbody, List<string> addressList)
        {
            MailMessage mailMessage = new MailMessage();
            mailMessage.From = new MailAddress("samsamson2016@gmail.com");
            //mailMessage.To.Add(addressList[0]);
            //mailMessage.To.Add(addressList[1]);
            //mailMessage.To.Add(addressList[2]);
            mailMessage.To.Add("s11075775@student.usp.ac.fj");
            mailMessage.Subject = "[DO NOT REPLY] Loan Request Awaiting Collateral - " + subjInfo;
            mailMessage.Body = emailbody;
            //mailMessage.IsBodyHtml = true;

            SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", 587);
            smtpClient.UseDefaultCredentials = false;
            smtpClient.Credentials = new System.Net.NetworkCredential()
            {
                UserName = "samsamson2016@gmail.com",
                Password = "1215Jean.b45"
            };
            smtpClient.EnableSsl = true;
            try
            {
                smtpClient.Send(mailMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="memberInfo"></param>
        /// <param name="emailbody"></param>
        /// <param name="emailAddress"></param>
        [NonAction]
        public void SendCollateralConfirmationNotif(string memberInfo, string emailbody, string emailAddress)
        {
            MailMessage mailMessage = new MailMessage();
            mailMessage.From = new MailAddress("samsamson2016@gmail.com");
            //mailMessage.To.Add(emailAddress);
            mailMessage.To.Add("s11075775@student.usp.ac.fj");
            mailMessage.Subject = "[DO NOT REPLY] Collateral Certificate Uploaded for " + memberInfo;
            mailMessage.Body = emailbody;

            SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", 587);
            smtpClient.UseDefaultCredentials = false;
            smtpClient.Credentials = new System.Net.NetworkCredential()
            {
                UserName = "samsamson2016@gmail.com",
                Password = "1215Jean.b45"
            };
            smtpClient.EnableSsl = true;
            try
            {
                smtpClient.Send(mailMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }

        [NonAction]
        public void BroadCastChangedStatusNotif1(string subjInfo, string emailbody, List<string> addressList)
        {
            MailMessage mailMessage = new MailMessage();
            mailMessage.From = new MailAddress("samsamson2016@gmail.com");
            //mailMessage.To.Add(addressList.First());
            //mailMessage.To.Add(addressList.Last());
            mailMessage.To.Add("s11075775@student.usp.ac.fj");
            mailMessage.Subject = "[DO NOT REPLY] Customer Loan Status Changed to " + subjInfo;
            mailMessage.Body = emailbody;
            //mailMessage.IsBodyHtml = true;

            SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", 587);
            smtpClient.UseDefaultCredentials = false;
            smtpClient.Credentials = new System.Net.NetworkCredential()
            {
                UserName = "samsamson2016@gmail.com",
                Password = "1215Jean.b45"
            };
            smtpClient.EnableSsl = true;
            try
            {
                smtpClient.Send(mailMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }

        [NonAction]
        public void BroadCastChangedStatusNotif2(string subjInfo, string emailbody, string emailAddress)
        {
            MailMessage mailMessage = new MailMessage();
            mailMessage.From = new MailAddress("samsamson2016@gmail.com");
            //mailMessage.To.Add(emailAddress);
            mailMessage.To.Add("s11075775@student.usp.ac.fj");
            mailMessage.Subject = "[DO NOT REPLY] Customer Loan Status Changed to " + subjInfo;
            mailMessage.Body = emailbody;
            //mailMessage.IsBodyHtml = true;

            SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", 587);
            smtpClient.UseDefaultCredentials = false;
            smtpClient.Credentials = new System.Net.NetworkCredential()
            {
                UserName = "samsamson2016@gmail.com",
                Password = "1215Jean.b45"
            };
            smtpClient.EnableSsl = true;
            try
            {
                smtpClient.Send(mailMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }

        [NonAction]
        public void SendChangedStatusNotif(string subjInfo, string emailbody, string emailAddress)
        {
            MailMessage mailMessage = new MailMessage();
            mailMessage.From = new MailAddress("samsamson2016@gmail.com");
            //mailMessage.To.Add(addressList.First());
            //mailMessage.To.Add(addressList.Last());
            mailMessage.To.Add("s11075775@student.usp.ac.fj");
            mailMessage.Subject = "[DO NOT REPLY] Customer Loan Status Changed to " + subjInfo;
            mailMessage.Body = emailbody;
            //mailMessage.IsBodyHtml = true;

            SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", 587);
            smtpClient.UseDefaultCredentials = false;
            smtpClient.Credentials = new System.Net.NetworkCredential()
            {
                UserName = "samsamson2016@gmail.com",
                Password = "1215Jean.b45"
            };
            smtpClient.EnableSsl = true;
            try
            {
                smtpClient.Send(mailMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }
    }
}