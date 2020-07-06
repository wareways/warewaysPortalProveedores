using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Wareways.PortalProv.Models.PP
{
    public class PagosModel
    {
        public List<Infraestructura.v_PPROV_FacturasIngresadasPorUsuario> L_Documentos { get; set; }
    }
    public class PagosOficinaModel
    {
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        [Display(Name = "Inicial")]
        public DateTime Fecha_Inicial { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        [Display(Name = "Final")]
        public DateTime Fecha_Final { get; set; }

        public List<Infraestructura.v_PPROV_FacturasIngresadas_Oficina> L_Documentos { get; set; }
    }

}