using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Wareways.PortalProv.Servicios
{
    public class Correlativo
    {
        

        public Int32 SolicitudNumeroSiguiente(Servicios.TipoCorrelativos Correlativo,String TipoSolicitud)
        {
            Int32 _Retorna = 0;
            Infraestructura.PortalProvEntities _Db = new Infraestructura.PortalProvEntities();

            

            if (Correlativo == TipoCorrelativos.Solicitud)
            {
                Infraestructura.GEN_Configuracion _Item = new Infraestructura.GEN_Configuracion();
                if (TipoSolicitud == "Personal")
                {
                    _Item = _Db.GEN_Configuracion.Where(p => p.ConfigNombre == "Correlativo_Solicitud_Personal").ToList().First();
                } else
                {
                    _Item = _Db.GEN_Configuracion.Where(p => p.ConfigNombre == "Correlativo_Solicitud_Grupo").ToList().First();
                }
                
                _Retorna = Int32.Parse( _Item.ConfigValor);
                _Item.ConfigValor = (_Retorna + 1).ToString();
                _Db.SaveChanges();
            }

            return _Retorna;
        }
    }
}