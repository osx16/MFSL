using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;

namespace MFSL.Controllers
{
    public class EmailController : Controller
    {
        [NonAction]
        public void SendEmailNotification(String emailbody, string emailAddress)
        {
            MailMessage mailMessage = new MailMessage("samsamson2016@gmail.com", emailAddress);
            mailMessage.Subject = "System Email Testing";
            mailMessage.Body = emailbody;

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