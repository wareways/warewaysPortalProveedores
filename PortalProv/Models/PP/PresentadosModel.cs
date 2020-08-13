using Org.BouncyCastle.Asn1.Mozilla;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Wareways.PortalProv.Models.PP
{
    public class PresentadosModel
    {
        public List<Infraestructura.V_PPROV_DocumentosPorUsuario> L_Documentos { get; set; }
        public string Nuevo_CardCode { get; set; }
        public string Nuevo_Pdf_Facturas { get; set; }
        public String Nuevo_Pdf_OC { get; set; }
        public String Nuevo_Pdf_Cotizacion { get; set; }
        public String Nuevo_Pdf_Informe { get; set; }

        public string ModoActivo { get; set; }
        public List<Infraestructura.V_PPROV_Empresas> Usuario_Empresas { get; set; }
        public List<Infraestructura.GEN_CatalogoDetalle> Usuario_Moneda  {get;set;}

        public Infraestructura.PPROV_Documento Nuevo { get; set; }
        public int? OrdenAdjunto { get; internal set; }
        public int? EntregaAdjunto { get; internal set; }
        public string EntregaAdjuntoUrl { get; internal set; }
        public string OrdenAdjuntoUrl { get; internal set; }

    }

    public class PresentadosModelOficina
    {
        public List<Infraestructura.V_PPROV_ResumenEstadoDocumento> L_Estados { get; set; }
        public List<Infraestructura.V_PPROV_Documentos_Oficina> L_Documentos { get; set; }

        public List<Infraestructura.PPROV_Documento> L_Retenciones { get;set;}

        public SelectList Nuevo_Estados_Asignar { get; set; }
        public string Nuevo_Estado_Seleccionado { get; set; }
        public string Nuevo_Estado_Comentario { get; set; }

    }
}