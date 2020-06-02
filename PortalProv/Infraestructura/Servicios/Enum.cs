using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Wareways.PortalProv.Servicios
{
    public enum TipoCatalogo
    {
        TipoDocumento = 1,
        EstadoCivil = 2,
        TipoVivienda = 3,
        PeriodoTiempo = 4,
        Municipio = 6,
        Departamento = 5,
        Escolaridad = 7,
        ClasificacionCliente = 8,
        SolicitudTipo = 9,
        SolicitudDetalleTipo = 10,
        Desembolso_Haber = 11,
        Desembolso_Debe = 12,
        Desembolso_TipoPago = 13,
        Desembolso_Bancos = 14,
        ConceptoCaja = 15,
        EstadosCiclos = 16,
        MotivoVisita = 17,
        
    }

    public enum TipoCorrelativos
    {
        Solicitud = 1
    }
    
}