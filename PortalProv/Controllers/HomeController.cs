using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Configuration;

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