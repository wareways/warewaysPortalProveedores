using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Wareways.PortalProv.Models;

namespace Wareways.PortalProv.Servicios
{
    public class ServicioSeguridad
    {
        static Infraestructura.PortalProvEntities _Db = new Infraestructura.PortalProvEntities();

        public static Boolean ValidaPermisos(RouteValueDictionary _Valores, String _UserName)
        {
            Boolean _Retorna = false;
            var _Controller = _Valores["Controller"].ToString();
            var _Action = _Valores["Action"].ToString();

            var _Menus = _Db.V_GEN_MenuDisplay.AsNoTracking().Where(P => P.UserName == _UserName).OrderBy(p=>p.Menu_Orden).ToList();
            if (HttpContext.Current.Session["MenuList"] == null )
            {
                RegistraVariablesSession(_Menus, _UserName);
            }
            
            if (_Menus.Where(p=>p.Menu_Url == @"/" + _Controller).ToList().Count > 0) _Retorna = true;

            return _Retorna;

            
        }

        internal static void RegistraVariablesSession(List<Infraestructura.V_GEN_MenuDisplay> _MenusPermitidos, string _Email)
        {
            
            var _Usuario = _Db.AspNetUsers.AsNoTracking().Where(p => p.UserName == _Email).ToList().First();

            var _Roles = (from l in _Db.V_GEN_UsuarioRoles where l.Active == true && l.UserId == _Usuario.Id select l).ToList();

            HttpContext.Current.Session["MenuList"] = _MenusPermitidos;
            HttpContext.Current.Session["UserName"] = _Usuario.UserName;
            HttpContext.Current.Session["UsuariosRoles"] = _Roles;


            String _Servidor = System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString.Split(';')[0].Split('=')[1];
            HttpContext.Current.Session["ConexionDB"] = (_Servidor == ".") ? "Local" : "Productivo";
            HttpContext.Current.Session["ScreenColor"] = (_Servidor == ".") ? "skin-yellow" : "skin-blue";
        }
    }
}