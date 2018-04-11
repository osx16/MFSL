using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;

namespace RESTServices.Controllers
{
    /// <summary>
    /// Email Controller handles all email messaging
    /// </summary>
    public class EmailController : Controller
    {
        /// <summary>
        /// Send Pending Approval Notification within Main Office Port Vila
        /// </summary>
        /// <param name="emailbody"> Email Body</param>
        /// <param name="addressList"> Address List</param>
        [NonAction]
        public void SendPendingApprovalNotif1(string subjInfo, string emailbody, List<string> addressList)
        {
            MailMessage mailMessage = new MailMessage();
            mailMessage.From = new MailAddress("samsamson2016@gmail.com");
            //mailMessage.To.Add(addressList.First());
            //mailMessage.To.Add(addressList.Last());
            mailMessage.To.Add("s11075775@student.usp.ac.fj");
            mailMessage.Subject = "[DO NOT REPLY] Pending Loan Application-" +subjInfo;
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
            mailMessage.Subject = "[DO NOT REPLY] Pending Loan Application";
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
            mailMessage.Subject = "[DO NOT REPLY] Loan Approved for " + memberInfo;
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
    }
}