using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;


namespace Wareways.PortalProv.Controllers
{
    public class CatalogoController : Controller
    {
        Wareways.PortalProv.Infraestructura.PortalProvEntities _Db = new Infraestructura.PortalProvEntities();


        // GET: Catalogo
        [Authorize]
        public ActionResult Index(String CodigoCatalogoSeleccionado)
        {
            if (!Servicios.ServicioSeguridad.ValidaPermisos(this.ControllerContext.RequestContext.RouteData.Values, User.Identity.Name)) return RedirectToAction("Permisos", "Home");

            var _Catalogo = _Db.GEN_Catalogo.OrderBy(p => p.Catalogo_Orden).ToList();
            ViewBag.Catalogo = _Catalogo;

            Models.CC.CatalogoIndexModel _Modelo = new Models.CC.CatalogoIndexModel();

            if (CodigoCatalogoSeleccionado != null)  _Modelo.CodigoCatalogoSeleccionado = Int32.Parse(CodigoCatalogoSeleccionado);

            if (_Modelo.CodigoCatalogoSeleccionado == null)
            {
                _Modelo.CodigoCatalogoSeleccionado = _Catalogo[0].Catalogo_Id;
            }



            ViewBag.VB_TextoPadre = _Catalogo.Where(p=>p.Catalogo_Id == _Modelo.CodigoCatalogoSeleccionado).ToList()[0].Nombre_ParentId;
            ViewBag.VB_TextoParametro = _Catalogo.Where(p => p.Catalogo_Id == _Modelo.CodigoCatalogoSeleccionado).ToList()[0].Nombre_Parametro;

            _Modelo.ListaCatalogoDetalle = _Db.GEN_CatalogoDetalle.Where(p => p.Catalogo_Id == _Modelo.CodigoCatalogoSeleccionado).ToList();          
            return View(_Modelo);
        }

        [HttpPost]        
        [Authorize]
        public ActionResult Index( Models.CC.CatalogoIndexModel _Modelo)
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

       



        // GET: Catalogo/Create
        public ActionResult Create( String id)
        {
            if (!Servicios.ServicioSeguridad.ValidaPermisos(this.ControllerContext.RequestContext.RouteData.Values, User.Identity.Name)) return RedirectToAction("Permisos", "Home");

            if (id == null) return RedirectToAction("Index");

            var Modelo = new Infraestructura.GEN_CatalogoDetalle();
            Modelo.Catalogo_Id = Int32.Parse(id);
            try
            {
                Modelo.Orden = (Int32)_Db.GEN_CatalogoDetalle.Where(p => p.Catalogo_Id == Modelo.Catalogo_Id).Max(p => p.Orden) + 1;
            } catch
            {
                Modelo.Orden = 1;
            }
         

            return View(Modelo);
        }

        // POST: Catalogo/Create
        [HttpPost]
        public ActionResult Create(Infraestructura.GEN_CatalogoDetalle Modelo)
        {
            try
            {
                Modelo.Sistema = false;
                _Db.GEN_CatalogoDetalle.Add(Modelo);
                _Db.SaveChanges();


                // TODO: Add insert logic here
                TempData["MensajeSuccess"] = "Datos Agregados con Exito";
                return RedirectToAction("Index", new Models.CC.CatalogoIndexModel { CodigoCatalogoSeleccionado = Modelo.Catalogo_Id });
                
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

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "No se Incluyeron los Parametros Necesarios");
            }

            var _CatalogoDetalle = _Db.GEN_CatalogoDetalle.Where(p => p.CatalogoDetalleId.ToString() == id).ToList();
            if ( _CatalogoDetalle.Count() == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "No se Econtro el Codigo de Catalogo");

            }
            Int32 _CatalogoId = (Int32)_CatalogoDetalle[0].Catalogo_Id;

            ViewBag.VB_TextoPadre = _Db.GEN_Catalogo.Where(p => p.Catalogo_Id == _CatalogoId).ToList()[0].Nombre_ParentId;
            ViewBag.VB_TextoParametro = _Db.GEN_Catalogo.Where(p => p.Catalogo_Id == _CatalogoId).ToList()[0].Nombre_Parametro;


            return View(_CatalogoDetalle.First());
            
        }


        
        // POST: Catalogo/Edit/5
        [HttpPost]
        [Authorize]
        public ActionResult Edit(Infraestructura.GEN_CatalogoDetalle Modelo)
        {
            if (!Servicios.ServicioSeguridad.ValidaPermisos(this.ControllerContext.RequestContext.RouteData.Values, User.Identity.Name)) return RedirectToAction("Permisos", "Home");

            try
            {
                var _ListaCatalogoDetalle = _Db.GEN_CatalogoDetalle.Where(p => p.CatalogoDetalleId == Modelo.CatalogoDetalleId).ToList();
                if (_ListaCatalogoDetalle.Count() == 1)
                {
                    _ListaCatalogoDetalle[0].Nombre = Modelo.Nombre;
                    _ListaCatalogoDetalle[0].Valor = Modelo.Valor;
                    _ListaCatalogoDetalle[0].Orden = Modelo.Orden;
                    _ListaCatalogoDetalle[0].ParentValor = Modelo.ParentValor;
                    _ListaCatalogoDetalle[0].Parametro1 = Modelo.Parametro1;
                    _ListaCatalogoDetalle[0].Sistema = (_ListaCatalogoDetalle[0].Sistema == null) ? false : _ListaCatalogoDetalle[0].Sistema;
                    _Db.SaveChanges();
                }

                TempData["MensajeSuccess"] = "Datos Actualizaodos con Exito";
                return RedirectToAction("Index", new Models.CC.CatalogoIndexModel { CodigoCatalogoSeleccionado = _ListaCatalogoDetalle[0].Catalogo_Id });
            }
            catch
            {
                return View();
            }
        }

        
    }
}
