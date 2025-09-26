using Microsoft.VisualStudio.Services.Organization.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Web;
using System.Web.Http;
using Wareways.PortalProv.Infraestructura.Servicios;
using Wareways.PortalProv.Servicios;

namespace Wareways.PortalProv.Controllers.Api
{
    [AllowAnonymous]    
    public class EnvioOCController : ApiController
    {
        // GET api/<controller>

        
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<controller>/5
        public string Get(String NoOc)
        {
            var xpath = HttpContext.Current.Server.MapPath("~");
            var t = new Thread(() => RevisionDatosOC(NoOc, xpath));
            t.Start();
           
            return "ok";
        }

        private void RevisionDatosOC(string NoOc, String  ServerPath)
        {
            if (!string.IsNullOrEmpty(NoOc))
            {
                VServicio vServ = new VServicio();
                var _datos = vServ.ObtenerOCAKI_SAP(Int32.Parse(NoOc));
                if (_datos.Count > 0)
                {
                    if (!string.IsNullOrEmpty(_datos[0].E_Mail) && _datos[0].EsAKIol == "S")
                    {
                        CrystalR sCrystal = new CrystalR();
                        sCrystal.GenerarOC(_datos[0].NumAtCard, _datos[0].E_Mail, _datos[0], ServerPath);
                        SaveResultado("Exito", "Correo Enviado a:  " + _datos[0].E_Mail + " el: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "R", Int32.Parse( NoOc));
                    }
                    SaveResultado("Error", "No exsite correo proveedor", "R", Int32.Parse(NoOc));
                }
                else
                {
                    SaveResultado("Error", "No exsite correo proveedor", "R", Int32.Parse(NoOc));
                }
            }
            
        }

        private void SaveResultado(string Tipo, string Error, string Estado, Int32 DocEntry)
        {
            VServicio vServ = new VServicio();
            if (Tipo == "Exito")
            {
                vServ.UpdateOCAKI_SAP(DocEntry, "N", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            }
            else
            {
                vServ.UpdateOCAKI_SAP(DocEntry, "N","Error");
            }
            
        }

        // POST api/<controller>
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<controller>/5
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<controller>/5
        public void Delete(int id)
        {
        }
    }
}