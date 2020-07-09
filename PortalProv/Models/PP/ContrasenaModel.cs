using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Wareways.PortalProv.Infraestructura;

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

    public class PPROV_ContrasenaModel
    {
        public Guid Contrasena_Id { get; set; }
        public int Contrasena_Numero { get; set; }
        public int Empresa_Id { get; set; }
        [DataType(DataType.Date)]        
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime Contrasena_Fecha { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime Contrasena_Fecha_Estimada { get; set; }
        public string Contrasena_Usuario { get; set; }
        public string Contrasena_Estado { get; set; }

        public string Contrasena_CardCode { get; set; }
        public List<PPROV_Documento> ListaDocumentos { get;  set; }
        public string CambioEstado { get; set; }
        public List<PPROV_Retencion> ListaRetenciones { get; internal set; }
    }
}