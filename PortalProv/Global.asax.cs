using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;
using System.Web.SessionState;
using Wareways.PortalProv.Filters;
using Wareways.PortalProv.Infraestructura;

namespace Wareways.PortalProv
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            //GlobalConfiguration.Configuration.MessageHandlers.Add(new APIKeyHandler());
            GlobalConfiguration.Configuration.Formatters.XmlFormatter.SupportedMediaTypes.Clear();

        }
        public void Application_PreRequestHandlerExecute(Object source, EventArgs e)
        {
            HttpApplication application = (HttpApplication)source;
            HttpContext context = application.Context;

            PortalProvEntities _Db = new PortalProvEntities();

            // use an if statement to make sure the request is not for a static file (js/css/html etc.)
            if (context != null && context.Session != null)
            {
                if (HttpContext.Current.Session["MenuList"] == null)
                {
                    var _UserName = User.Identity.Name;
                    if ( _UserName != "")
                    {
                        var _Menus = _Db.V_GEN_MenuDisplay.AsNoTracking().Where(P => P.UserName == _UserName).OrderBy(p => p.Menu_Orden).ToList();
                        Servicios.ServicioSeguridad.RegistraVariablesSession(_Menus, _UserName);
                    }
                    
                }
            }
        }
    }

    
}
