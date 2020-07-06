using Microsoft.TeamFoundation.Client.Reporting;
using System;
using System.Collections.Generic;
using System.EnterpriseServices;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Wareways.PortalProv.Infraestructura;

namespace Wareways.PortalProv.Controllers
{   

    public class AdmContrasenaController : Controller
    {
        Infraestructura.PortalProvEntities _Db = new Infraestructura.PortalProvEntities();
       
        [Authorize(Roles = "Oficina")]        
        public ActionResult Index()
        {
            var model = new Models.PP.ContrasenaOficinaModel();
            model.L_Documentos = ObtenerDocumentosPorUsuario();
            return View(model);            
        }

        private List<V_PPROV_Contrasena_Oficina> ObtenerDocumentosPorUsuario()
        {
            var _UserName = User.Identity.Name;
            var _Datos = _Db.V_PPROV_Contrasena_Oficina.ToList();

            return _Datos;
        }
    }
}