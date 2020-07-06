using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Wareways.PortalProv.Infraestructura;

namespace Wareways.PortalProv.Controllers
{
    public class admretencionesController : Controller
    {
        PortalProvEntities _Db = new PortalProvEntities();


        [Authorize(Roles = "Oficina")]
        public ActionResult Index()
        {
            var model = new Models.PP.RetencionesOficinaModel();
            model.L_Retenciones = ObtenerRetencionesPorusuario();

            return View(model);
        }

        private List<V_PPROV_Retenciones_Oficina> ObtenerRetencionesPorusuario()
        {
            var _UserName = User.Identity.Name;
            var _Datos = _Db.V_PPROV_Retenciones_Oficina.ToList();

            return _Datos;
        }
    }
}