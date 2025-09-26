using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Wareways.PortalProv.Infraestructura;
using Wareways.PortalProv.Models.OC;

namespace Wareways.PortalProv.Models.PP
{
    public class DetallePresentadosModel
    {
        public PPROV_Documento PDoc { get; set; }

        public SAP_Doc Generado { get; set; }
        public List<SAP_DocDetalle> GeneradoDetalle { get; set; }

        public List<oCuentaContableSap> CuentasTodas { get; set; }
        public List<oCentroCosto> CentroCostos { get;  set; }
        public List<oImpuestoSap> lstImpuestos { get;  set; }

        public String Impuesto { get; set; }
    }
}