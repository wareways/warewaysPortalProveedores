using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Wareways.PortalProv.Controllers
{
    public class EmpresaController : Controller
    {
        Infraestructura.PortalProvEntities _Db = new Infraestructura.PortalProvEntities();
        

        public ActionResult Seleccionar(String posturl)
        {

            var _UserName = User.Identity.Name;
            var oUsuario = _Db.AspNetUsers.Where(p => p.UserName == _UserName).First();
            var arrEmppresaPermiso = _Db.PPROV_UsuarioEmpresa.Where(p => p.UserId == oUsuario.Id).Select(p => p.EmpresaId).ToArray();

            // AutoSelectEmpresa Por Variable Config
            if (ConfigurationManager.AppSettings["WWPortal_EmpresaId"] != "")
            {
                var id = Int32.Parse( ConfigurationManager.AppSettings["WWPortal_EmpresaId"]);
                //posturl = ConfigVariablesSession(id, posturl, arrEmppresaPermiso);
                if (arrEmppresaPermiso.Count() == 0)
                {
                    Session.Add("EmpresaSelId", "0");
                }

                if (String.IsNullOrEmpty(posturl)) posturl = Url.Action("Index", "Home");
                return Redirect(posturl);
            }
            // Sitio MultiEmpresa
            var modelo = _Db.V_PPROV_Empresas.Where(p => arrEmppresaPermiso.Contains(p.Empresa_Id)).ToList();
            ViewBag.PostUrl = posturl;

            return View(modelo);
        }

        public ActionResult Seleccionado(Int32 id, String posturl)
        {
            var _UserName = User.Identity.Name;
            var oUsuario = _Db.AspNetUsers.Where(p => p.UserName == _UserName).First();
            var arrEmppresaPermiso = _Db.PPROV_UsuarioEmpresa.Where(p => p.UserId == oUsuario.Id).Select(p => p.EmpresaId).ToArray();

            //posturl = ConfigVariablesSession(id, posturl, arrEmppresaPermiso);
            if (arrEmppresaPermiso.Count() == 0)
            {
                Session.Add("EmpresaSelId", "0");
            }
            if (posturl == "") posturl = Url.Action("Index", "Home");
            return Redirect(posturl);
        }

        private string ConfigVariablesSession(int id, string posturl, int[] arrEmppresaPermiso)
        {
            if (arrEmppresaPermiso.Contains(id))
            {
                var oEmpresa = _Db.V_PPROV_Empresas.Where(p => p.Empresa_Id == id).First();
                Session.Add("EmpresaSelId", id);
                Session.Add("EmpresaSelDB", oEmpresa.SAP_Database);
                Session.Add("EmpresaSelLogo", oEmpresa.Logo);
                Session.Add("EmpresaSelName", oEmpresa.Empresa_Name);
                Session.Add("ScreenColor", oEmpresa.Tema);
            }
            else
            {
                posturl = "";
            }

            return posturl;
        }
    }
}