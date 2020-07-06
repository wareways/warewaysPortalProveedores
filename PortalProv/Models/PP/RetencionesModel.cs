using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Wareways.PortalProv.Models.PP
{
    public class RetencionesModel
    {
        public List<Infraestructura.V_PPROV_RetencionesPorUsuario> L_Retenciones  { get; set; }
    }
    public class RetencionesOficinaModel
    {
        public List<Infraestructura.V_PPROV_Retenciones_Oficina> L_Retenciones { get; set; }
    }
}