using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Wareways.PortalProv.Infraestructura;

namespace Wareways.PortalProv.Controllers
{
    public class admPagosController : Controller
    {
        Infraestructura.PortalProvEntities _Db = new Infraestructura.PortalProvEntities();

        [Authorize(Roles = "Oficina")]
        public ActionResult Index(DateTime? Fecha_Inicial , DateTime? Fecha_Final)
        {
            var model = new Models.PP.PagosOficinaModel();
            model.Fecha_Inicial = Fecha_Inicial == null ? DateTime.Now.AddDays(-60).Date : (DateTime)Fecha_Inicial;
            model.Fecha_Final = Fecha_Final == null ? DateTime.Now.Date : (DateTime)Fecha_Final;
            model.L_Documentos = ObtenerPagosPorusuario(model.Fecha_Inicial, model.Fecha_Final.Date.AddDays(1));

            return View(model);
        }

        private List<v_PPROV_FacturasIngresadas_Oficina> ObtenerPagosPorusuario(DateTime? FechaInicial , DateTime? FechaFinal)
        {            
            var _Datos = _Db.v_PPROV_FacturasIngresadas_Oficina.Where(p => p.TrsfrDate != null 
                            && p.TrsfrDate >= FechaInicial && p.TrsfrDate <= FechaFinal
                            ).ToList();

            return _Datos;
        }
    }
}