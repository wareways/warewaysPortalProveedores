using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Wareways.PortalProv.Controllers.CC
{
    public class ConfGrlController : Controller
    {
        Infraestructura.PortalProvEntities _Db = new Infraestructura.PortalProvEntities();

        // GET: ConfGrl
        [Authorize]
        public ActionResult Index()
        {
            if (!Servicios.ServicioSeguridad.ValidaPermisos(this.ControllerContext.RequestContext.RouteData.Values, User.Identity.Name)) return RedirectToAction("Permisos", "Home");

            var modelo = _Db.GEN_Configuracion.OrderBy(p=>p.ConfigNombre).ToList();
            

       
            return View(modelo);
        }

       

      


        [Authorize]
        public ActionResult Edit(string id)
        {
            if (!Servicios.ServicioSeguridad.ValidaPermisos(this.ControllerContext.RequestContext.RouteData.Values, User.Identity.Name)) return RedirectToAction("Permisos", "Home");

            var modelo = _Db.GEN_Configuracion.Where(p => p.ConfigNombre == id).ToList().FirstOrDefault();

            return View(modelo);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Infraestructura.GEN_Configuracion modelo)
        {
            if (!Servicios.ServicioSeguridad.ValidaPermisos(this.ControllerContext.RequestContext.RouteData.Values, User.Identity.Name)) return RedirectToAction("Permisos", "Home");

            var _Entidad = _Db.GEN_Configuracion.Where(p => p.ConfigNombre == modelo.ConfigNombre).ToList();

            if (_Entidad.Count == 1)
            {
                _Entidad[0].ConfigValor = modelo.ConfigValor;
                
                _Db.SaveChanges();
            }

            TempData["MensajeSuccess"] = "Se ha actualizado un parametro";
            return RedirectToAction("Index");
        }


       
      

    }
}