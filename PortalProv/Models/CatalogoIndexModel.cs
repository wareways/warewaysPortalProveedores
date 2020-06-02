using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Wareways.PortalProv.Models.CC
{
    public class CatalogoIndexModel
    {
        public Int32? CodigoCatalogoSeleccionado { get; set; }
        public List<Infraestructura.GEN_CatalogoDetalle> ListaCatalogoDetalle { get;set;}
    }
}