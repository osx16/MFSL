using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MFSL.Controllers
{
    public class UsersController : Controller
    {
        // GET: Users
        public ActionResult ActiveUsers()
        {
            return View();
        }
    }
}