using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Wareways.PortalProv.Models.OC
{
	public class OC_DetalleModel
	{
		public Guid Fel_Unique { get; set; }

		public int Linea { get; set; }
		public string TipoDet { get; set; }


		[Range(0, int.MaxValue, ErrorMessage = "El campo Cantidad debe ser mayor a 0")]
		public decimal Cantidad { get; set; }

		[Required]
		public string Descripcion { get; set; }

		//Detalle Producto
		public string CodigodeBarras { get; set; }
		public string Producto_Codigo { get; set; }






		public string UnidadMedida { get; set; }

		[Range(0.001, int.MaxValue, ErrorMessage = "El campo Precio debe ser mayor a 0.001")]
		public decimal PrecioUnitario { get; set; }


		public decimal Descuentos { get; set; }


		[Range(0, int.MaxValue, ErrorMessage = "El campo Total debe ser mayor a 0")]
		public decimal TotalLinea { get; set; }


		public decimal Impuestos { get; set; }
		public DateTime? DateAudit { get; set; }
		public string UserNameAudit { get; set; }

		public Guid UniqueId { get; set; }
		//Confirmar
		public decimal CostoPromedio { get; set; }
		public decimal CostoTotal { get; set; }
		public decimal GananciaMonto { get; set; }
		public decimal GananciaPorcentaje { get; set; }
		public String TipoImpuesto { get; set; }
		public String CentoCosto { get; set; }
		public String CentoCostoArray { get; set; }

		public String CuentaContable { get; set; }

		public String Bodega { get; set; }

	}
}