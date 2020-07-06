using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Wareways.PortalProv.Models.PP
{
    public class ContrasenaModel
    {
        public List<Infraestructura.V_PPROV_Contrasena_PorUsuario> L_Documentos { get; set; }
    }

    public class ContrasenaOficinaModel
    {
        public List<Infraestructura.V_PPROV_Contrasena_Oficina> L_Documentos { get; set; }
    }
}