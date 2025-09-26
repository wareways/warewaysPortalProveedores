using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;


namespace Wareways.PortalProv.Controllers
{
    public class FechaRecepcionFacturasController : Controller
    {
        Wareways.PortalProv.Infraestructura.PortalProvEntities _Db = new Infraestructura.PortalProvEntities();


        // GET: Catalogo
        [Authorize]
        public ActionResult Index(String CodigoCatalogoSeleccionado)
        {
            if (!Servicios.ServicioSeguridad.ValidaPermisos(this.ControllerContext.RequestContext.RouteData.Values, User.Identity.Name)) return RedirectToAction("Permisos", "Home");

            List<Infraestructura.V_PROV_EmpresaUltiDia_Rece> _Modelo = new List<Infraestructura.V_PROV_EmpresaUltiDia_Rece>();

            _Modelo = _Db.V_PROV_EmpresaUltiDia_Rece.AsNoTracking().OrderBy(p=>p.Anio).ThenBy(p=>p.Mes).ThenBy(p=>p.CardCode).ToList();

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



            var Modelo = new Infraestructura.PPROV_EmpresaUltiDia_Rece();
            Modelo.Activo = true;
            Modelo.Anio = DateTime.Now.Year.ToString();
            Modelo.Mes = DateTime.Now.Month;
            Modelo.CardCode = "";
            Modelo.Dia = 25;
            Modelo.Mensaje = "";
            Modelo.DiaServ = 27;





            return View(Modelo);
        }

        // POST: Catalogo/Create
        [HttpPost]
        public ActionResult Create(Infraestructura.PPROV_EmpresaUltiDia_Rece Modelo)
        {
            try
            {
                Modelo.Id = Guid.NewGuid();
                if (Modelo.CardCode == null) Modelo.CardCode = "";
                Modelo.CardCode = Modelo.CardCode.Split(' ')[0];

                _Db.PPROV_EmpresaUltiDia_Rece.Add(Modelo);
                _Db.SaveChanges();


                // TODO: Add insert logic here
                TempData["MensajeSuccess"] = "Datos Agregados con Exito";
                return RedirectToAction("Index");

            }
            catch (Exception ex)
            {
                return View();
            }
        }

        [Authorize]
        // GET: Catalogo/Edit/5
        public ActionResult Edit(string id)
        {
            if (!Servicios.ServicioSeguridad.ValidaPermisos(this.ControllerContext.RequestContext.RouteData.Values, User.Identity.Name)) return RedirectToAction("Permisos", "Home");

            try
            {
                var Modelo = _Db.PPROV_EmpresaUltiDia_Rece.Where(p => p.Id.ToString() == id).FirstOrDefault();
                return View(Modelo);
            }
            catch { return RedirectToAction("Index"); }
            
            
            return View();

        }



        // POST: Catalogo/Edit/5
        [HttpPost]
        [Authorize]
        public ActionResult Edit(Infraestructura.PPROV_EmpresaUltiDia_Rece Modelo)
        {
            if (!Servicios.ServicioSeguridad.ValidaPermisos(this.ControllerContext.RequestContext.RouteData.Values, User.Identity.Name)) return RedirectToAction("Permisos", "Home");

            try
            {
                var _Editar = _Db.PPROV_EmpresaUltiDia_Rece.Where(p => p.Id == Modelo.Id).FirstOrDefault();

                _Editar.Mensaje = Modelo.Mensaje;
                _Editar.Dia = Modelo.Dia;
                _Editar.Activo = Modelo.Activo;
                _Editar.DiaServ = Modelo.DiaServ;
                
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
