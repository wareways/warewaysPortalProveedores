using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Wareways.PortalProv.Infraestructura;

namespace Wareways.PortalProv.Controllers
{
    public class RetencionesController : Controller
    {
        PortalProvEntities _Db = new PortalProvEntities();


        [Authorize]
        // GET: Retenciones
        public ActionResult Index()
        {
            var model = new Models.PP.RetencionesModel();
            model.L_Retenciones = ObtenerRetencionesPorusuario();

            return View(model);
        }

        private List<V_PPROV_RetencionesPorUsuario> ObtenerRetencionesPorusuario()
        {
            var _UserName = User.Identity.Name;
            var _Datos = _Db.V_PPROV_RetencionesPorUsuario.Where(p => p.UserName == _UserName).ToList();

            return _Datos;
        }
    }
}