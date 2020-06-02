using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Wareways.PortalProv.Controllers
{
    public class ScheduleController : Controller
    {
        // GET: Schedule
        public ActionResult cada5minutos()
        {
            return new HttpStatusCodeResult(System.Net.HttpStatusCode.Accepted, "Datos Procesados");
        }

        public ActionResult cadahora()
        {
            return new HttpStatusCodeResult(System.Net.HttpStatusCode.Accepted, "Datos Procesados");
        }
        public ActionResult cada6horas()
        {
            return new HttpStatusCodeResult(System.Net.HttpStatusCode.Accepted, "Datos Procesados");
        }

        public ActionResult T1159()
        {
            return new HttpStatusCodeResult(System.Net.HttpStatusCode.Accepted, "Datos Procesados");
        }
        public ActionResult T2159()
        {
            return new HttpStatusCodeResult(System.Net.HttpStatusCode.Accepted, "Datos Procesados");
        }
    }
}