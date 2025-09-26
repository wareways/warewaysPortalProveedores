using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Wareways.PortalProv.Infraestructura;

namespace Wareways.PortalProv.Controllers
{
    public class ContrasenaController : Controller
    {
        PortalProvEntities _Db = new PortalProvEntities();

        [Authorize]
        // GET: Presentados
        public ActionResult Index()
        {
            var model = new Models.PP.ContrasenaModel();
            model.L_Documentos = ObtenerDocumentosPorUsuario();
            return View(model);
        }

        [Authorize]
        [HttpPost]
        public ActionResult Index(PresentadosController model)
        {

            return View();
        }


        private List<V_PPROV_Contrasena_PorUsuario> ObtenerDocumentosPorUsuario()
        {
            var _UserName = User.Identity.Name;
            var retencionMin = DateTime.Now.Date.AddMonths(-6);
            _Db.Database.CommandTimeout = 300;
            var _Datos = _Db.V_PPROV_Contrasena_PorUsuario.Where(p => p.UserName == _UserName && p.Contrasena_Fecha >= retencionMin).ToList();

            return _Datos;
        }
    }
}