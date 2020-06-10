using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Wareways.PortalProv.Filters;
using Wareways.PortalProv.Infraestructura;

namespace Wareways.PortalProv.API
{
    public class PagoController : ApiController
    {
        PortalProvEntities _Db = new PortalProvEntities();

        // GET api/<controller>
        [BasicAuthentication]
        public List<v_PPROV_FacturasIngresadasPorUsuario> Get(string Username) 
        {
            
            var _Datos = _Db.v_PPROV_FacturasIngresadasPorUsuario.Where(p => p.UserName == Username && p.TrsfrDate != null).ToList();
            return _Datos;            
        }

        //// GET api/<controller>/5
        //public string Get(int id)
        //{
        //    return "value";
        //}

        //// POST api/<controller>
        //public void Post([FromBody] string value)
        //{
        //}

        //// PUT api/<controller>/5
        //public void Put(int id, [FromBody] string value)
        //{
        //}

        //// DELETE api/<controller>/5
        //public void Delete(int id)
        //{
        //}
    }
}