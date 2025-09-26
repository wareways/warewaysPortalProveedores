using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Wareways.PortalProv.Models.OC
{
    public class OC_EmitirModel
    {
		public OC_EmitirModel()
		{
			Detalle = new List<OC_DetalleModel>();
			Adjuntos = new List<Infraestructura.FEL_DocAdjunto>();
		}

		public String TipoAdjuntoDoc { get; set; }
		public List<Infraestructura.FEL_DocAdjunto> Adjuntos { get; set; }

		public Boolean CollapseDatosGenerales { get; set; }

		public List<Infraestructura.FEL_DocHistorico> Histotico { get; set;  }

		public String CCobligatorio { get; set; }

		//se define Contenedores
		public Guid Fel_Unique { get; set; }

		public Int32? EmpresaId { get; set; }

		public int Fel_Correlativo { get; set; }
		public string Establecimiento_Codigo { get; set; }
		public string TipoDoc { get; set; }

		[DataType(DataType.Date)]
		[DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
		public DateTime FechaEmision { get; set; }
		[DataType(DataType.Date)]
		[DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
		public DateTime? FechaEntrega { get; set; }
		public string Nit { get; set; }
		public string CardCode { get; set; }

		public string CardName { get; set; }
		public string Address { get; set; }
		
		public string Moneda { get; set; }
		

		[DisplayFormat(DataFormatString = "{0:0.######}")]
		public decimal TasaCambio { get; set; }

		[DisplayFormat(DataFormatString = "{0:0.##}")]
		[Range(0, int.MaxValue, ErrorMessage = "El campo Total No puede ser menor a 0")]
		public decimal Total { get; set; }

		public string Tipo_Detalle { get; set; }
		public string Estado { get; set; }

		public List<OC_DetalleModel> Detalle { get; set; }

		public OC_DetalleModel ModalNuevo { get; set; }
		public OC_DetalleModel ModalEditar { get; set; }

		public String Fel_Firma { get; set; }

		public Boolean SoloLectura { get; set; }

		public string CMP_OrigenSerie { get; set; }
		public string CMP_OrigenNumero { get; set; }
		public string CMP_OrigenFirma { get; set; }
		public string CMP_OrigenFecha { get; set; }
		[DataType(DataType.Date)]
		[DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
		public DateTime CMP_OrigenFechaInput { get; set; }
		public string CMP_Motivo { get; set; }

		[DisplayFormat(DataFormatString = "{0:0.##}")]
		public String MetododePago { get; set; }

		public String NumeroRetencion { get; set; }

		[Range(0, int.MaxValue, ErrorMessage = "El campo Monto Retencion No puede ser menor a 0")]
		[DisplayFormat(DataFormatString = "{0:0.##}")]
		public Decimal? MontoRetencion { get; set; }

		public bool EliminarDetalle { get; set; }
		//Detalle Producto	
		public string CodigodeBarras { get; set; }
		public string Producto_Codigo { get; set; }
		public string Producto_Nombre { get; set; }
		public string CodigodeBarrasConfirmacion { get; set; }
		public string Producto_CodigoConfirmacion { get; set; }

		[DisplayFormat(DataFormatString = "{0:0.##}")]
		public decimal Producto_Precio { get; set; }
		//Buscar Cliente
		public string Cliente_Buscar { get; set; }
		//Vendedores
		public int? SlpCode { get; set; }
		public string SlpName { get; set; }

		[Range(0, int.MaxValue, ErrorMessage = "El campo Monto Pagado No puede ser menor a 0")]
		[DisplayFormat(DataFormatString = "{0:0.##}")]
		public decimal? MontoPagado { get; set; }

		public string Rece_Cellular { get; set; }

		public string SyncSapId { get; set; }

		public string TipoTarjeta { get; set; }
		public string NoDeposito { get; set; }

		public string TarjetaValid { get; set; }
		[Range(1, 12, ErrorMessage = "01 - 12")]
		public string TarjetaValidMes { get; set; }
		[Range(00, 99, ErrorMessage = "01 - 99")]
		public string TarjetaValidAnio { get; set; }

		public string TarjetaAuto { get; set; }
		public string CuentaBancoDepTra { get; set; }
		public string SyncPagoId { get; set; }
		public string SyncPagoDate { get; set; }
		//Editar Correo
		public string CorreoAnterior { get; set; }
		public string CorreoNuevo { get; set; }

		public string Referencia { get; set; }
		public string Propietario { get; set; }
		public string Comentario { get; set; }
		public string AutorizadoPor { get; set; }

		[DataType(DataType.Date)]
		[DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
		public DateTime? AutorizadoEl { get; set; }

		[DataType(DataType.Date)]
		[DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
		public DateTime? EnviadoAuto { get; set; }

		public Guid? Departamento { get; set; }

		public String CardCodeSel { get; set;  }

		public String CreadoPor { get; set; }
		public DateTime? CreadoEl { get; set; }
		public String ActualizadoPor { get; set; }
		public DateTime? ActualizadoEl { get; set; }

		public Int32? EntregaNo { get; set; }
		public DateTime? EntregaEl { get; set; }
		public String EntregaPor { get; set; }
        public List<string> Regiones { get;  set; }

		public String importE { get; set; }
        public String EntregaMultiple { get;  set; }
		public Decimal EntregaSaldo { get; set; }

		public string InactivoMensaje { get; set; }
		public string Inactivo { get; set; }
	}
    
}