﻿using System.Web.Mvc;

namespace MFSL.Controllers
{
    public class ErrorController : Controller
    {
        public ActionResult InternalServerError()
        {
            return View();
        }

        public ActionResult NotFound()
        {
            return View();
        }

        public ActionResult CouldNotCreateFile()
        {
            return View();
        }
    }
}
