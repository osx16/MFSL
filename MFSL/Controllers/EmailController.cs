using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;

namespace MFSL.Controllers
{
    /// <summary>
    /// Controller Methods:
    /// 1. SendEmailNotification for Password Reset
    /// </summary>
    public class EmailController : Controller
    {
        /// <summary>
        /// Sends Email Notification to user
        /// </summary>
        /// <param name="emailbody"> Email Body</param>
        /// <param name="emailAddress">Email Address</param>
        [NonAction]
        public void SendEmailNotification(String emailbody, string emailAddress)
        {
            MailMessage mailMessage = new MailMessage("samsamson2016@gmail.com", emailAddress);
            mailMessage.Subject = "[DO NOT REPLY] FMS Password Reset";
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
            smtpClient.Send(mailMessage);
        }
    }
}