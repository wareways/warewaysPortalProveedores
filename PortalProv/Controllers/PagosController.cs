using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Wareways.PortalProv.Infraestructura;

namespace Wareways.PortalProv.Controllers
{
    public class PagosController : Controller
    {
        PortalProvEntities _Db = new PortalProvEntities();

        [Authorize]
        public ActionResult Index()
        {
            var model = new Models.PP.PagosModel();
            model.L_Documentos = ObtenerPagosPorusuario();

            return View(model);
        }

        private List<v_PPROV_FacturasIngresadasPorUsuario> ObtenerPagosPorusuario()
        {
            var _UserName = User.Identity.Name;

            var _Datos = new List<v_PPROV_FacturasIngresadasPorUsuario>();

            using (var client = new HttpClient())
            {
                var Url = string.Format("{0}://{1}{2}{3}",
              System.Web.HttpContext.Current.Request.Url.Scheme,
              System.Web.HttpContext.Current.Request.Url.Host,
              System.Web.HttpContext.Current.Request.Url.Port == 80 ? string.Empty : ":" + System.Web.HttpContext.Current.Request.Url.Port,
              System.Web.HttpContext.Current.Request.ApplicationPath);
                client.BaseAddress = new Uri(Url+"/api/");
                //HTTP GET
                var byteArray = Encoding.ASCII.GetBytes("andy:password");
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));


                var responseTask = client.GetAsync(string.Format("pago?username={0}", User.Identity.Name));
                responseTask.Wait();

                var result = responseTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    var readTask = result.Content.ReadAsAsync<List<v_PPROV_FacturasIngresadasPorUsuario>>();
                    readTask.Wait();

                    _Datos = readTask.Result;
                }
                else //web api sent error response 
                {
                    //log response status here..                    
                    ModelState.AddModelError(string.Empty, "Server error. Please contact administrator.");
                }
            }
            
            return _Datos;
        }
    }
}