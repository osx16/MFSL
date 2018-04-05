using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using RESTServices.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace RESTServices.Controllers
{
    /// <summary>
    /// Roles API Controller
    /// </summary>
    public class RolesAPIController : ApiController
    {
        ApplicationDbContext context;
        AccountController Account;
        /// <summary>
        /// Constructor
        /// </summary>
        public RolesAPIController()
        {
            Account = new AccountController();
            context = new ApplicationDbContext();
        }
        /// <summary>
        /// Get all roles for system users except admin
        /// </summary>
        /// <returns></returns>
        [Route("api/RolesAPI/GetRoles")]
        public List<string> GetRoles()
        {
            var roles = context.Roles.Where(u => !u.Name.Contains("Admin")).Select(x=>x.Name).ToList();
            return roles;
        }
        /// <summary>
        /// Get role for current user
        /// </summary>
        /// <returns></returns>
        [Route("api/RolesAPI/GetRoleForThisUser")]
        public string GetRoleForThisUser()
        {
            var UserId = User.Identity.GetUserId();
            var roleId = context.Users.Where(x=>x.Id == UserId).Select(u => u.Roles).First();
            var extract = roleId.First();
            var Id = extract.RoleId;
            var role = context.Roles.Where(r => r.Id == Id).Select(r => r.Name).First();
            return role;
        }
    }
}
