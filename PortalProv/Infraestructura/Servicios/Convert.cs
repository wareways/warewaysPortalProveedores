

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;



namespace Wareways.PortalProv.Servicios
{
    public class Convert
    {
        Wareways.PortalProv.Infraestructura.PortalProvEntities _Db = new Infraestructura.PortalProvEntities();

        static JsonSerializerSettings Jsonsettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            MissingMemberHandling = MissingMemberHandling.Ignore
        };


       
        // -----------
        // Grupos
        // -----------
        //public List<Models.CC.GrupoModel> Convert_Grupos_EF_Model(List<Infraestructura.CC_Grupo> _Lista)
        //{
        //    var _Serializado = Newtonsoft.Json.JsonConvert.SerializeObject(_Lista);
        //    var _Return =   Newtonsoft.Json.JsonConvert.DeserializeObject<List<Models.CC.GrupoModel>>(_Serializado, Jsonsettings);

        //    var _Rutas = _Db.CC_Ruta.ToList();
        //    foreach (var _item in _Return)
        //    {
        //        var _Econtrador = _Rutas.Where(p => p.Ruta_Id == _item.Ruta_Asignada).ToList();
        //        _item.Ruta_AsignadaNombre = "No Asignada";
        //        if (_Econtrador.Count == 1) _item.Ruta_AsignadaNombre = _Econtrador[0].Ruta_Nombre;
        //    }

        //    return _Return;

        //}

      
       
    }
}