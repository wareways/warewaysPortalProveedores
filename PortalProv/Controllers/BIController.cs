using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.Identity;

using System.Web.Mvc;

namespace Wareways.PortalProv.Controllers
{
    public class BIController : Controller
    {
        [Authorize]
        public ActionResult test()
        {
            return View();
        }
    }
}