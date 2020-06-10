using Org.BouncyCastle.Asn1.Mozilla;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Wareways.PortalProv.Models.PP
{
    public class PresentadosModel
    {
        public List<Infraestructura.V_PPROV_DocumentosPorUsuario> L_Documentos { get; set; }
        public string Nuevo_CardCode { get; set; }
        public string Nuevo_Pdf_Facturas { get; set; }
        public String Nuevo_Pdf_OC { get; set; }
        public string ModoActivo { get; set; }
        public List<Infraestructura.V_PPROV_Empresas> Usuario_Empresas { get; set; }
        public List<Infraestructura.GEN_CatalogoDetalle> Usuario_Moneda  {get;set;}

        public Infraestructura.PPROV_Documento Nuevo { get; set; }
    }
}