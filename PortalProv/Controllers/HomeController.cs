using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Configuration;
using System.Text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;

namespace Wareways.PortalProv.Controllers
{
    public class HomeController : Controller
    {
        Infraestructura.PortalProvEntities _Db = new Infraestructura.PortalProvEntities();

        
        public ActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                if (Session["MenuList"]== null)
                {
                    var _MenusPermitidos = (from l in _Db.V_GEN_MenuDisplay.AsNoTracking() where l.UserName == User.Identity.Name orderby l.Menu_Orden select l).ToList();
                    Session["MenuList"] = _MenusPermitidos;                    
                }
                if (Session["UserName"] == null)
                {
                    Session["UserName"] = User.Identity.Name;
                }
            }

            
            String _Servidor = System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString.Split(';')[0].Split('=')[1];
            Session.Add("ConexionDB",(_Servidor == ".") ? "Local" : "Productivo");
            Session["ScreenColor"] = (_Servidor == ".") ? "hold-transition skin-yellow " : "hold-transition skin-blue ";

            ViewBag.VB_Servidor = (_Servidor == ".") ? "Local" : "Productivo";

            if (User.Identity.Name != "")
            {
                var _Estadisticas = _Db.SP_PPROV_STATS_FacturacionUltimoAnio(User.Identity.Name).ToList();
                ViewBag.DataTotal = string.Join(",", _Estadisticas.Select(p => p.Total).ToArray());
                ViewBag.DataCantidad = string.Join(",", _Estadisticas.Select(p => p.Cantidad).ToArray());
                ViewBag.DataTitulo = string.Join(",", _Estadisticas.Select(p => @"'" +p.Anio.ToString() + "-" + p.mes.ToString()+ @"'").ToArray());

                var _Indicadores = _Db.SP_PPROV_Indicadores_Usuario(User.Identity.Name).ToList();
                ViewBag.IndicadoresTop = _Indicadores.Where(p=>p.Grupo == "HomeTop").OrderBy(p=>p.Orden).ToList();
                ViewBag.IndicadoresBottom = _Indicadores.Where(p => p.Grupo == "HomeBottom").OrderBy(p => p.Orden).ToList();

            }


            return View();
        }

        public ActionResult Permisos()
        {
            ViewBag.Message = "No tiene Persmisos para ver esta Pagina.";

            return View();
        }

        public ActionResult PermisosDatos()
        {
            ViewBag.Message = "No tiene Permisos para ver los datos Solicitados.";

            return View();
        }


        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        [Authorize]
        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        

        

        

    }
}