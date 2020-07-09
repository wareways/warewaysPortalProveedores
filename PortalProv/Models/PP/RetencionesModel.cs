using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Wareways.PortalProv.Infraestructura;

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
    public class RetencionesOficinaNuevo
    {
        public Guid? _DocId { get; set; }

        public List<PPROV_RetencionTipo> Lista_TiposRet { get; set; }
        public List<GEN_CatalogoDetalle> Lista_Moneda { get; set; }
        public string Nuevo_Pdf_Name { get; set; }

        public Guid Retencion_Id { get; set; }
        public string Retencion_Numero { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime Retencion_Fecha { get; set; }
        public int Retencion_Tipo { get; set; }
        public string Retencion_Usuario { get; set; }
        public decimal Retencion_Monto { get; set; }
        public string Retencion_Moneda { get; set; }
        public string Retencion_Pdf { get; set; }
        public int Manual_FacNumero { get; set; }
        public string Manual_Fac_Serie { get; set; }
        public int Manual_Empresa { get; set; }
        public string Retencion_CardCode { get; set; }
    }
}