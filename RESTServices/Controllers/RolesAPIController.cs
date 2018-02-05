using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using RESTServices.Models;
namespace RESTServices.Controllers
{
    public class RolesAPIController : ApiController
    {
        ApplicationDbContext context;
        public RolesAPIController()
        {
            context = new ApplicationDbContext();
        }

        [Route("api/RolesAPI/GetRoles")]
        public List<string> GetRoles()
        {
            var roles = context.Roles.Where(u => !u.Name.Contains("Admin")).Select(x=>x.Name).ToList();
            return roles;
        }
    }
}
