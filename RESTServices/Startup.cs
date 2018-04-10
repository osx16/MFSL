using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Owin;
using Owin;
using RESTServices.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Swashbuckle;

[assembly: OwinStartup(typeof(RESTServices.Startup))]

namespace RESTServices
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            CreateRolesandUsers();
            app.CreatePerOwinContext<ApplicationUserManager>(ApplicationUserManager.Create);
        }

        // In this method we will create default User roles and Admin user for login   
        private void CreateRolesandUsers()
        {
            ApplicationDbContext context = new ApplicationDbContext();

            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
            var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));

            // In Startup iam creating first Admin Role and creating a default Admin User    
            if (!roleManager.RoleExists("Admin"))
            {

                // first we create Admin role   
                var role = new Microsoft.AspNet.Identity.EntityFramework.IdentityRole();
                role.Name = "Admin";
                roleManager.Create(role);

                //Here we create a Admin super user who will maintain the website                  

                var user = new ApplicationUser();
                user.UserName = "jsamson";
                user.Email = "samsamson2016@gmail.com";

                string userPWD = "Password?1";

                var chkUser = UserManager.Create(user, userPWD);

                //Add default User to Role Admin   
                if (chkUser.Succeeded)
                {
                    var result1 = UserManager.AddToRole(user.Id, "Admin");

                }
            }

            // creating Creating Manager role    
            if (!roleManager.RoleExists("General Manager"))
            {
                var role = new Microsoft.AspNet.Identity.EntityFramework.IdentityRole();
                role.Name = "General Manager";
                roleManager.Create(role);

            }

            if (!roleManager.RoleExists("Chief Operation"))
            {
                var role = new Microsoft.AspNet.Identity.EntityFramework.IdentityRole();
                role.Name = "Chief Operation";
                roleManager.Create(role);
            }

            if (!roleManager.RoleExists("SIO Operation"))
            {
                var role = new Microsoft.AspNet.Identity.EntityFramework.IdentityRole();
                role.Name = "SIO Operation";
                roleManager.Create(role);
            }

            if (!roleManager.RoleExists("SIO Branch Operation"))
            {
                var role = new Microsoft.AspNet.Identity.EntityFramework.IdentityRole();
                role.Name = "SIO Branch Operation";
                roleManager.Create(role);
            }
  
            if (!roleManager.RoleExists("SIO Marketing"))
            {
                var role = new Microsoft.AspNet.Identity.EntityFramework.IdentityRole();
                role.Name = "SIO Marketing";
                roleManager.Create(role);
            }

            if (!roleManager.RoleExists("SIO"))
            {
                var role = new Microsoft.AspNet.Identity.EntityFramework.IdentityRole();
                role.Name = "SIO";
                roleManager.Create(role);
            }

            if (!roleManager.RoleExists("Investment Officer"))
            {
                var role = new Microsoft.AspNet.Identity.EntityFramework.IdentityRole();
                role.Name = "Investment Officer";
                roleManager.Create(role);
            }

            if (!roleManager.RoleExists("Intern"))
            {
                var role = new Microsoft.AspNet.Identity.EntityFramework.IdentityRole();
                role.Name = "Intern";
                roleManager.Create(role);
            }

            if (!roleManager.RoleExists("Sr. Finance Officer "))
            {
                var role = new Microsoft.AspNet.Identity.EntityFramework.IdentityRole();
                role.Name = "Sr. Finance Officer";
                roleManager.Create(role);
            }

            if (!roleManager.RoleExists("Accounts Receivable"))
            {
                var role = new Microsoft.AspNet.Identity.EntityFramework.IdentityRole();
                role.Name = "Accounts Receivable";
                roleManager.Create(role);
            }

            if (!roleManager.RoleExists("Accounts Payable"))
            {
                var role = new Microsoft.AspNet.Identity.EntityFramework.IdentityRole();
                role.Name = "Accounts Payable";
                roleManager.Create(role);
            }

            if (!roleManager.RoleExists("Collateral Officer"))
            {
                var role = new Microsoft.AspNet.Identity.EntityFramework.IdentityRole();
                role.Name = "Collateral Officer";
                roleManager.Create(role);
            }
        }
    }
}
