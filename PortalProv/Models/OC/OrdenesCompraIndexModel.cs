using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using Wareways.PortalProv.Infraestructura;

namespace Wareways.PortalProv.Models.OC
{
    public class OrdenesCompraIndexModel
    {
        public List<SP_GetUserOC_Result> Datos { get; set;} 

        public String EstadoSel { get; set; }

        public int NoBorrador { get; set; }
        public int NoAutorizar { get; set; }
        public int NoAprobado { get; set; }
        public int NoGenerado { get; set; }
        public int NoCancelado { get; set; }

        public int NoRevision { get; set; }

        public int NoAKIol { get; set; }

        public int NoAprobadoEspecial { get; set; }

        public DataSet DatosAKIOL { get; set; }


    }
}