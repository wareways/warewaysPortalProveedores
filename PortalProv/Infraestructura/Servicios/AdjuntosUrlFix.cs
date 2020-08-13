using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Web;

namespace Wareways.PortalProv.Infraestructura.Servicios
{
    public class AdjuntosUrlFix
    {
        public static String  Url (String Ruta)
        {
            try
            {
                if (Ruta.Contains(@"\\"))
                {
                    return "/Servicios/Sap_AdjuntoDescarga.aspx?FileName=" + @HttpUtility.UrlEncode(Ruta);
                }
                else
                {
                    return Ruta;
                }
            }
            catch {
                return "";
            }

           

        }
    }
}