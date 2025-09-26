using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Wareways.PortalProv.Servicios;

namespace Wareways.PortalProv.Controllers
{
    public class UtilidadController : Controller
    {
        Infraestructura.PortalProvEntities _Db = new Infraestructura.PortalProvEntities();
        VServicio vServicio = new VServicio();

        [HttpPost]
        public JsonResult ObtenerProveedor(string Prefix)
        {

            var _Datos = (from c in vServicio.List_SapProveedor(Prefix)                          
                          select new { Name = c.CardCode +" | " + c.CardName + " | " + c.U_NIT, Id = c.CardCode }).ToList();

            return Json(_Datos, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Res_ObtenerDatosProveedor(string id)
        {            
            
            var ListaValoresClientes = vServicio.List_SapProveedor().Where(p => p.CardCode.ToString() == id).Select(p => new { p.CardCode, p.CardName,  p.U_NIT }).AsEnumerable();
            return Json(ListaValoresClientes);
        }

        //Autocomplete de Productos
        [HttpPost]
        public JsonResult ObtenerProducto(string Prefix, string WhsCode)
        {
            Prefix = Prefix.ToUpper();
            var _Datos = (from c in vServicio.List_SapProductos(WhsCode, "%"+Prefix+"%")                          
                          select new { Name = c.ItemCode + " | " + c.ItemName + " | "+ c.UOM + " | " + c.AvgPrice.ToString("##,###,###.00"), Id = c.ItemCode  });
            return Json(_Datos, JsonRequestBehavior.AllowGet);
        }

        //Autocomplete de Productos
        [HttpPost]
        public JsonResult ObtenerProductoCardCode(string Prefix, string WhsCode, string CardCode)
        {
            Prefix = Prefix.ToUpper();
            var _Datos = (from c in vServicio.List_SapProductosCardCode(WhsCode, "%" + Prefix + "%", CardCode)
                          select new { Name = c.ItemCode + " | " + c.ItemName + " | " + c.UOM + " | " + c.AvgPrice.ToString("##,###,###.00"), Id = c.ItemCode });
            return Json(_Datos, JsonRequestBehavior.AllowGet);
        }


    }
}