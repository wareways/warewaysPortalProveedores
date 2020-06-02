using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Wareways.PortalProv.Models;
using Microsoft.AspNet.Identity;

namespace Wareways.PortalProv.Controllers.Seguridad
{
    [Authorize]
    public class PerfilController : Controller
    {
        private Infraestructura.PortalProvEntities _Db = new Infraestructura.PortalProvEntities();
        Servicios.ServicioSeguridad _Seg = new Servicios.ServicioSeguridad();
        Servicios.Export _ServExport = new Servicios.Export();

        // GET: AspNetRoles
        public ActionResult Index()
        {
            if ( ! Servicios.ServicioSeguridad.ValidaPermisos(this.ControllerContext.RequestContext.RouteData.Values, User.Identity.Name)) return RedirectToAction("Permisos", "Home");
                
            var _Modelo = _Db.AspNetRoles.ToList();                                 
            return View(_Modelo);
        }

        // GET: AspNetRoles/Details/5
        public ActionResult Audit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var _llave = id.Split(';')[0];
            var _Tabla = id.Split(';')[1];
            var _Auditorias = (from l in _Db.GEN_Audit where l.PK.Contains(_llave) && l.TableName == _Tabla orderby l.UpdateDate descending select l);            
            
            return PartialView("Audit", _Auditorias);
        }

        // GET: AspNetRoles/Create
        public ActionResult Create()
        {
            if (!Servicios.ServicioSeguridad.ValidaPermisos(this.ControllerContext.RequestContext.RouteData.Values, User.Identity.Name)) return RedirectToAction("Permisos", "Home");

            Infraestructura.AspNetRoles _Modelo = new Infraestructura.AspNetRoles();
            _Modelo.Id = Guid.NewGuid().ToString();

            return View(_Modelo);
        }

        // POST: AspNetRoles/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Name,Active")] Infraestructura.AspNetRoles aspNetRoles)
        {
            if (!Servicios.ServicioSeguridad.ValidaPermisos(this.ControllerContext.RequestContext.RouteData.Values, User.Identity.Name)) return RedirectToAction("Permisos", "Home");
            
            if (ModelState.IsValid)
            {
                aspNetRoles.UserNameAudit = User.Identity.GetUserName();
                _Db.AspNetRoles.Add(aspNetRoles);
                _Db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(aspNetRoles);
        }

        // GET: AspNetRoles/Edit/5
        public ActionResult Edit(string id)
        {
            if (!Servicios.ServicioSeguridad.ValidaPermisos(this.ControllerContext.RequestContext.RouteData.Values, User.Identity.Name)) return RedirectToAction("Permisos", "Home");


            ViewBag.ListaMenus = _Db.V_GEN_RolesDelMenu.AsNoTracking().Where(p => p.Id == id).ToList();

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Infraestructura.AspNetRoles aspNetRoles = _Db.AspNetRoles.Find(id);
            if (aspNetRoles == null)
            {
                return HttpNotFound();
            }
            return View(aspNetRoles);
        }

        // POST: AspNetRoles/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Name,Active")] Infraestructura.AspNetRoles aspNetRoles)
        {
            if (!Servicios.ServicioSeguridad.ValidaPermisos(this.ControllerContext.RequestContext.RouteData.Values, User.Identity.Name)) return RedirectToAction("Permisos", "Home");

            if (ModelState.IsValid)
            {
                aspNetRoles.UserNameAudit = User.Identity.GetUserName();
                _Db.Entry(aspNetRoles).State = EntityState.Modified;
                _Db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(aspNetRoles);
        }

        [Authorize]
        public ActionResult ExportExcel()
        {
            if (!Servicios.ServicioSeguridad.ValidaPermisos(this.ControllerContext.RequestContext.RouteData.Values, User.Identity.Name)) return RedirectToAction("Permisos", "Home");

            var _Usuarios = (from l in _Db.AspNetRoles
                             orderby l.Name
                             select new {  Nombre = l.Name, Activo = l.Active}).ToList();

            _ServExport.ToExcel(Response, _Usuarios, "ListadoPerfiles");
            return View();
        }

        // GET: AspNetRoles/Delete/5
        public ActionResult Delete(string id)
        {
            if (!Servicios.ServicioSeguridad.ValidaPermisos(this.ControllerContext.RequestContext.RouteData.Values, User.Identity.Name)) return RedirectToAction("Permisos", "Home");

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Infraestructura.AspNetRoles aspNetRoles = _Db.AspNetRoles.Find(id);
            if (aspNetRoles == null)
            {
                return HttpNotFound();
            }
            return View(aspNetRoles);
        }

        // POST: AspNetRoles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            if (!Servicios.ServicioSeguridad.ValidaPermisos(this.ControllerContext.RequestContext.RouteData.Values, User.Identity.Name)) return RedirectToAction("Permisos", "Home");

            Infraestructura.AspNetRoles aspNetRoles = _Db.AspNetRoles.Find(id);
            _Db.AspNetRoles.Remove(aspNetRoles);
            _Db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _Db.Dispose();
            }
            base.Dispose(disposing);
        }
    }


}
