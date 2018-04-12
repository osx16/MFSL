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
            mailMessage.Subject = "[DO NOT REPLY] Pending Loan Request-" +subjInfo;
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
            mailMessage.Subject = "[DO NOT REPLY] Loan Request Awaiting Payment-" + subjInfo;
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
            mailMessage.Subject = "[DO NOT REPLY] Loan Request Awaiting Collateral-" + subjInfo;
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
    }
}