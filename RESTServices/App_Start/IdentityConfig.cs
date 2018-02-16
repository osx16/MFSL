using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using RESTServices.Models;
using System.Text;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System;

namespace RESTServices
{
    // Configure the application user manager used in this application. UserManager is defined in ASP.NET Identity and is used by the application.

    public class ApplicationUserManager : UserManager<ApplicationUser>
    {
        public ApplicationUserManager(IUserStore<ApplicationUser> store)
            : base(store)
        {
        }

        public static ApplicationUserManager Create(IdentityFactoryOptions<ApplicationUserManager> options, IOwinContext context)
        {
            var manager = new ApplicationUserManager(new UserStore<ApplicationUser>(context.Get<ApplicationDbContext>()));
            // Configure validation logic for usernames
            manager.UserValidator = new UserValidator<ApplicationUser>(manager)
            {
                AllowOnlyAlphanumericUserNames = false,
                RequireUniqueEmail = true
            };
            // Configure validation logic for passwords
            manager.PasswordValidator = new PasswordValidator
            {
                RequiredLength = 6,
                RequireNonLetterOrDigit = true,
                RequireDigit = true,
                RequireLowercase = true,
                RequireUppercase = true,
            };
            var dataProtectionProvider = options.DataProtectionProvider;
            if (dataProtectionProvider != null)
            {
                manager.UserTokenProvider = new DataProtectorTokenProvider<ApplicationUser>(dataProtectionProvider.Create("ASP.NET Identity"));
            }
            return manager;
        }

        public class EmailService : IIdentityMessageService
        {
            public Task SendAsync(IdentityMessage message)
            {
                // Credentials:
                var credentialUserName = "samsamson2016@gmail.com";
                var sentFrom = "samsamson2016@gmail.com";
                var pwd = "1215Jean.b45";

                // Configure the client:
                System.Net.Mail.SmtpClient client =
                    new System.Net.Mail.SmtpClient("smtp.gmail.com");

                client.Port = 587;
                client.DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network;
                client.UseDefaultCredentials = false;

                // Create the credentials:
                System.Net.NetworkCredential credentials =
                    new System.Net.NetworkCredential(credentialUserName, pwd);

                client.EnableSsl = true;
                client.Credentials = credentials;

                // Create the message:
                var mail =
                    new System.Net.Mail.MailMessage(sentFrom, message.Destination);

                mail.Subject = message.Subject;
                mail.Body = message.Body;

                // Send:
                //return client.SendMailAsync(mail);
                client.SendCompleted += (s, e) => {
                    client.Dispose();
                };
                return client.SendMailAsync(mail);
            }
        }
    }
}
