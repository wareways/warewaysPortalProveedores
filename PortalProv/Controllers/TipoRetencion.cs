using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;


namespace Wareways.PortalProv.Controllers
{
    public class TipoRetencionController : Controller
    {
        Wareways.PortalProv.Infraestructura.PortalProvEntities _Db = new Infraestructura.PortalProvEntities();


        // GET: Catalogo
        [Authorize]
        public ActionResult Index(String CodigoCatalogoSeleccionado)
        {
            if (!Servicios.ServicioSeguridad.ValidaPermisos(this.ControllerContext.RequestContext.RouteData.Values, User.Identity.Name)) return RedirectToAction("Permisos", "Home");

            List<Infraestructura.PPROV_RetencionTipo> _Modelo = new List<Infraestructura.PPROV_RetencionTipo>();

            _Modelo = _Db.PPROV_RetencionTipo.ToList();

            return View(_Modelo);
        }

        [HttpPost]
        [Authorize]
        public ActionResult Index(Models.CC.CatalogoIndexModel _Modelo)
        {
            if (!Servicios.ServicioSeguridad.ValidaPermisos(this.ControllerContext.RequestContext.RouteData.Values, User.Identity.Name)) return RedirectToAction("Permisos", "Home");

            var _Catalogo = _Db.GEN_Catalogo.OrderBy(p => p.Catalogo_Orden).ToList();

            ViewBag.Catalogo = _Catalogo;
            if (_Modelo.CodigoCatalogoSeleccionado == null)
            {
                _Modelo.CodigoCatalogoSeleccionado = _Catalogo[0].Catalogo_Id;
            }

            ViewBag.VB_TextoPadre = _Catalogo.Where(p => p.Catalogo_Id == _Modelo.CodigoCatalogoSeleccionado).ToList()[0].Nombre_ParentId;
            ViewBag.VB_TextoParametro = _Catalogo.Where(p => p.Catalogo_Id == _Modelo.CodigoCatalogoSeleccionado).ToList()[0].Nombre_Parametro;


            _Modelo.ListaCatalogoDetalle = _Db.GEN_CatalogoDetalle.Where(p => p.Catalogo_Id == _Modelo.CodigoCatalogoSeleccionado).ToList();
            return View(_Modelo);
        }


        public ActionResult Create()
        {
            if (!Servicios.ServicioSeguridad.ValidaPermisos(this.ControllerContext.RequestContext.RouteData.Values, User.Identity.Name)) return RedirectToAction("Permisos", "Home");



            var Modelo = new Infraestructura.PPROV_RetencionTipo();
            Modelo.Retencion_Tipo = _Db.PPROV_RetencionTipo.Max(l => l.Retencion_Tipo) + 1;


            return View(Modelo);
        }

        // POST: Catalogo/Create
        [HttpPost]
        public ActionResult Create(Infraestructura.PPROV_RetencionTipo Modelo)
        {
            try
            {
                Modelo.Retencion_Activo = true;
                _Db.PPROV_RetencionTipo.Add(Modelo);
                _Db.SaveChanges();


                // TODO: Add insert logic here
                TempData["MensajeSuccess"] = "Datos Agregados con Exito";
                return RedirectToAction("Index");

            }
            catch
            {
                return View();
            }
        }

        [Authorize]
        // GET: Catalogo/Edit/5
        public ActionResult Edit(string id)
        {
            if (!Servicios.ServicioSeguridad.ValidaPermisos(this.ControllerContext.RequestContext.RouteData.Values, User.Identity.Name)) return RedirectToAction("Permisos", "Home");

            var Modelo = _Db.PPROV_RetencionTipo.Find( Int32.Parse(  id));
            if (Modelo == null)
            {
                return RedirectToAction("Index");
            }
            return View(Modelo);

        }



        // POST: Catalogo/Edit/5
        [HttpPost]
        [Authorize]
        public ActionResult Edit(Infraestructura.PPROV_RetencionTipo Modelo)
        {
            if (!Servicios.ServicioSeguridad.ValidaPermisos(this.ControllerContext.RequestContext.RouteData.Values, User.Identity.Name)) return RedirectToAction("Permisos", "Home");

            try
            {
                var _Editar = _Db.PPROV_RetencionTipo.Find(Modelo.Retencion_Tipo);

                _Editar.Retencion_Nombre = Modelo.Retencion_Nombre;
                _Db.SaveChanges();

                TempData["MensajeSuccess"] = "Datos Actualizaodos con Exito";
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }


    }
}
