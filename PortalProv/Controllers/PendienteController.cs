using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Wareways.PortalProv.Infraestructura;

namespace Wareways.PortalProv.Controllers
{
    public class PendienteController : Controller
    {
        PortalProvEntities _Db = new PortalProvEntities();

        [Authorize]
        public ActionResult Index()
        {
            var model = new Models.PP.PagosModel();
            model.L_Documentos = ObtenerPagosPorusuario();

            return View(model);
        }

        private List<v_PPROV_FacturasIngresadasPorUsuario> ObtenerPagosPorusuario()
        {
            var _UserName = User.Identity.Name;
            var _Datos = _Db.v_PPROV_FacturasIngresadasPorUsuario.Where(p => p.UserName == _UserName && p.TrsfrDate == null).ToList();

            return _Datos;
        }
    }
}