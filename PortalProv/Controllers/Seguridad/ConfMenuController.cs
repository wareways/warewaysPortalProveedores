using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Wareways.PortalProv.Controllers.Seguridad
{
    public class ConfMenuController : Controller
    {
        Infraestructura.PortalProvEntities _Db = new Infraestructura.PortalProvEntities();

        // GET: ConfMenu
        [Authorize]
        public ActionResult Index()
        {
            if (!Servicios.ServicioSeguridad.ValidaPermisos(this.ControllerContext.RequestContext.RouteData.Values, User.Identity.Name)) return RedirectToAction("Permisos", "Home");

            var Modelo = _Db.GEN_Menu.OrderBy(p=>p.Menu_Orden).ToList();
            return View(Modelo);
        }

        [Authorize]
        public ActionResult Details(String id)
        {
            if (!Servicios.ServicioSeguridad.ValidaPermisos(this.ControllerContext.RequestContext.RouteData.Values, User.Identity.Name)) return RedirectToAction("Permisos", "Home");

            var Modelo = _Db.GEN_Menu.Where(p => p.Menu_Id.ToString() == id).ToList();
            if (Modelo.Count == 0) return RedirectToAction("Index");

            ViewBag.Permisos = _Db.V_GEN_RolesDelMenu.AsNoTracking().Where(p => p.Menu_Id.ToString() == id).ToList();

            return View(Modelo.First());
        }

        [Authorize]
        public ActionResult Permisos(Guid MenuId, Guid RoleId, String Accion)
        {
            if (!Servicios.ServicioSeguridad.ValidaPermisos(this.ControllerContext.RequestContext.RouteData.Values, User.Identity.Name)) return RedirectToAction("Permisos", "Home");


            if (Accion == "Add")
            {
                try
                {
                    var Rol = _Db.AspNetRoles.Find(RoleId.ToString());
                    var Menu = _Db.GEN_Menu.Find(MenuId);
                    Menu.AspNetRoles.Add(Rol);

                    _Db.SaveChanges();
                }
                catch  { }
            }
            if (Accion == "Delete")
            {
                try
                {
                    _Db.GEN_Menu.Find(MenuId).AspNetRoles.Remove(_Db.AspNetRoles.Find(RoleId.ToString()));
                    //var _Eliminar = _Db.Menu_Roles.Where(p => p.Menu_Id == MenuId && p.Role_Id == RoleId.ToString());
                    //_Db.Menu_Roles.RemoveRange(_Eliminar);
                    _Db.SaveChanges();
                }
                catch  { }
            }

            return RedirectToAction("Details", new { id = MenuId });
        }

    }
}