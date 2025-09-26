using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Wareways.PortalProv.Infraestructura;

namespace Wareways.PortalProv.Controllers
{
    public class EntregasController : Controller
    {
        PortalProvEntities _Db = new PortalProvEntities();
        Servicios.VServicio vServicio = new Servicios.VServicio();


        [Authorize]
        public ActionResult Index(DateTime? Fecha_Inicial, DateTime? Fecha_Final)
        {
            Int32 empresaId = Int32.Parse( ConfigurationManager.AppSettings["WWPortal_EmpresaId"]);

            var model = new Models.PP.EntregasModel();
            model.Fecha_Inicial = Fecha_Inicial == null ? DateTime.Now.AddDays(-60).Date : (DateTime)Fecha_Inicial;
            model.Fecha_Final = Fecha_Final == null ? DateTime.Now.Date : (DateTime)Fecha_Final;
            model.L_Documentos = ObtenerPagosPorusuario(empresaId,   model.Fecha_Inicial, model.Fecha_Final.Date.AddDays(1));

            return View(model);
        }

        private List<WWSP_ListadoEntregasPorUsuario_Result> ObtenerPagosPorusuario( Int32 EmpresaId,   DateTime? FechaInicial, DateTime? FechaFinal)
        {
            var retencionMin = DateTime.Now.Date.AddMonths(-6);
            _Db.Database.CommandTimeout = 300;
            var _Datos =  _Db.WWSP_ListadoEntregasPorUsuario( EmpresaId,FechaInicial, FechaFinal, User.Identity.Name  ).Where(p=>p.DocDate >= retencionMin).ToList();

            return _Datos;
        }


    }
}