using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Wareways.PortalProv.Models;
using System.Net;
using System.IO;
using System.Web.UI;
using System.Text;
using Wareways.PortalProv.Infraestructura;
using Wareways.PortalProv.Servicios;
using System.Configuration;

namespace Wareways.PortalProv.Controllers.Seguridad
{
    [Authorize]
    public class UsuarioController : Controller
    {
        Infraestructura.PortalProvEntities _Db = new Infraestructura.PortalProvEntities();
        VServicio vServicio = new VServicio();

        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        Servicios.Export _ServExport = new Servicios.Export();

        [Authorize(Roles = "Administradores")]
        public ActionResult Index()
        {
            if (Session["EmpresaSelId"] == null)
            {
                Wareways.PortalProv.Servicios.ServicioSeguridad.CheckSession(User.Identity.Name);
            }

            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            // Verificacion de Permisos
            if (!Servicios.ServicioSeguridad.ValidaPermisos(this.ControllerContext.RequestContext.RouteData.Values, User.Identity.Name)) return RedirectToAction("Permisos", "Home");

            var _Lista = _Db.AspNetUsers;
            return View(_Lista);
        }

       

       

        

        [Authorize(Roles = "Administradores")]
        [HttpPost]
        public ActionResult AgregaCodProv(int EmpresaId, string Id, string CardCode)
        {
            try {
                _Db.PPROV_UsuarioProveedor.Add(new Infraestructura.PPROV_UsuarioProveedor { UserId = Id, CardCode = CardCode.Split(' ')[0], Empresa_Id = EmpresaId });
                _Db.SaveChanges();
            } catch { }
            

            return RedirectToAction("Edit",new { id = Id });
        }

        [Authorize(Roles = "Administradores")]
        [HttpPost]
        public ActionResult AgregaCodDepto(int EmpresaId, string Id, String DepartmentId, bool Autorizar, bool Crear)
        {
            try
            {
                Guid _DepartmentId = _Db.SP_GetDepartmentAll(EmpresaId).Where(p => p.DepartmentName == DepartmentId).First().DepartmentId;

                _Db.PPROV_UsuarioDepartamento.Add(new Infraestructura.PPROV_UsuarioDepartamento { DepartmentId = _DepartmentId, Autorizar = Autorizar, Crear = Crear, Empresa_Id = EmpresaId, UserId = Id});
                _Db.SaveChanges();
            }
            catch { }


            return RedirectToAction("Edit", new { id = Id });
        }

        [Authorize(Roles = "Administradores")]
        public ActionResult QuitarCodProv(int EmpresaId, string Id, string CardCode)
        {
            try
            {
                var _borrar = _Db.PPROV_UsuarioProveedor.Where(p => p.Empresa_Id == EmpresaId && p.CardCode == CardCode && p.UserId == Id).ToList();
                _Db.PPROV_UsuarioProveedor.RemoveRange(_borrar);
                _Db.SaveChanges();
            }
            catch { }


            return RedirectToAction("Edit", new { id = Id });
        }

        [Authorize(Roles = "Administradores")]
        public ActionResult QuitarCodDepto(int EmpresaId, string Id, Guid DepartmentId)
        {
            try
            {
                var _borrar = _Db.PPROV_UsuarioDepartamento.Where(p => p.Empresa_Id == EmpresaId && p.DepartmentId == DepartmentId && p.UserId == Id).ToList();
                _Db.PPROV_UsuarioDepartamento.RemoveRange(_borrar);
                _Db.SaveChanges();
            }
            catch { }


            return RedirectToAction("Edit", new { id = Id });
        }


        [Authorize(Roles = "Administradores")]
        public ActionResult Edit(string id)
        {
            if (Session["EmpresaSelId"] == null)
            {

                Wareways.PortalProv.Servicios.ServicioSeguridad.CheckSession(User.Identity.Name);                
            }

            // Verificacion de Permisos
            if (!Servicios.ServicioSeguridad.ValidaPermisos(this.ControllerContext.RequestContext.RouteData.Values, User.Identity.Name)) return RedirectToAction("Permisos", "Home");
            

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Infraestructura.AspNetUsers aspNetUsers = _Db.AspNetUsers.Where(p => p.Id == id).ToList().First();

            ViewBag.RolesAsignados = _Db.V_GEN_UsuarioRoles.AsNoTracking().Where(P => P.UserId == aspNetUsers.Id).ToList();
            ViewBag.RolesDisponibles = _Db.V_GEN_UsuarioRoles_Diponibles.AsNoTracking().Where(P => P.UserId == aspNetUsers.Id).ToList();
            ViewBag.OficinaEmpesasPermiso = _Db.SP_PPROV_ADM_EmpresasOficina(aspNetUsers.UserName).ToList();
            ViewBag.PermisosUsuarioCodigosProv = _Db.SP_PPROV_PermisosCodigosProv_Usuario(aspNetUsers.UserName).ToList();
            ViewBag.PermisosUsuarioCodigosDepto = vServicio.GetUserDepartment(aspNetUsers.UserName).ToList();
            ViewBag.Empresas = _Db.V_PPROV_Empresas.ToList();
            try { ViewBag.EmpresasFiltro = _Db.V_PPROV_Empresas.ToList().Where(p => p.Empresa_Id.ToString() == ConfigurationManager.AppSettings["WWPortal_EmpresaId"]  ).ToList(); }
            catch { ViewBag.EmpresasFiltro = _Db.V_PPROV_Empresas.ToList(); }
            
            if (aspNetUsers == null)
            {
                return HttpNotFound();
            }
            return View(aspNetUsers);
        }

        [HttpPost]
        public JsonResult ObtenerPorveedores(string Prefix, Int32 EmpresaId)
        {
            var SAP_Database = "";
            try {
                SAP_Database = _Db.V_PPROV_Empresas.Where(p => p.Empresa_Id == EmpresaId).First().SAP_Database;                
            } catch (Exception ex) 
            { 
            }

            String Sql = string.Format("EXEC {0}..WW_PPROV_Proveedor '{1}'", SAP_Database, Prefix);
            var _DatosIni = _Db.Database.SqlQuery<V_PPROV_Proveedor>(Sql);
            var _Datos = (from c in _DatosIni select new { Name = c.CardCode + " " + c.CardName, Id = c.CardCode });

            return Json(_Datos, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult ObtenerDeptos(string Prefix, Int32 EmpresaId)
        {


            var _DatosIni = _Db.SP_GetDepartmentAll(EmpresaId).ToList();
            var _Datos = (from c in _DatosIni select new { Name = c.DepartmentName , Id = c.DepartmentId });

            return Json(_Datos, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administradores")]
        public ActionResult Edit(Infraestructura.AspNetUsers _Modelo)
        {
            if (!Servicios.ServicioSeguridad.ValidaPermisos(this.ControllerContext.RequestContext.RouteData.Values, User.Identity.Name)) return RedirectToAction("Permisos", "Home");

            

            if (ModelState.IsValid)
            {
                var _AspNetUser = _Db.AspNetUsers.Find(_Modelo.Id);
                _AspNetUser.Email = _Modelo.Email;
                _AspNetUser.Nombre = _Modelo.Nombre;
                _AspNetUser.PhoneNumber = _Modelo.PhoneNumber;
                _AspNetUser.Puesto = _Modelo.Puesto;
                _AspNetUser.RazonSocial = _Modelo.RazonSocial;
                _AspNetUser.Nit = _Modelo.Nit;

                _Db.SaveChanges();
                TempData["MensajeSuccess"] = "Datos Grabados Con Exito";
                return RedirectToAction("Edit",new { id = _Modelo.Id });
            }
            return View(_Modelo);
        }

        [Authorize(Roles = "Administradores")]
        public ActionResult AgregarEmpresaOficina (Int32 EmpresaId ,String  UserId )
        {
            try {
                _Db.PPROV_UsuarioEmpresa.Add(new Infraestructura.PPROV_UsuarioEmpresa { EmpresaId = EmpresaId, UserId = UserId });
                _Db.SaveChanges();

            } catch { }
            try
            {
                _Db.Database.ExecuteSqlCommand(string.Format("exec SP_AgregaPersmisoEmpresaSSO 1,'{0}'", UserId));
            }
            catch (Exception ex) { 
            }

            return RedirectToAction("Edit", new { id = UserId });
        }

        [Authorize(Roles = "Administradores")]
        public ActionResult QuitarEmpresaOficina(Int32 EmpresaId, String UserId)
        {
            try
            {
                var _Quitar = _Db.PPROV_UsuarioEmpresa.Where(p => p.UserId == UserId && p.EmpresaId == EmpresaId).ToList();
                _Db.PPROV_UsuarioEmpresa.RemoveRange(_Quitar);
                _Db.SaveChanges();

            }
            catch { }
            try {
                _Db.Database.ExecuteSqlCommand(string.Format("exec SP_AgregaPersmisoEmpresaSSO 0,'{0}'", UserId));
            }
            catch (Exception ex)
            {
            }

            return RedirectToAction("Edit", new { id = UserId });
        }

       


        [Authorize]
        [Authorize(Roles = "Administradores")]
        public async Task<ActionResult> CorreoConfirmacion(String UserId)
        {                       
            string body = string.Empty;
            using (StreamReader reader = new StreamReader(Server.MapPath("~/Infraestructura/Correos/ConfirmacionCorreo.html")))
            {
                body = reader.ReadToEnd();
            }
            string code = await UserManager.GenerateEmailConfirmationTokenAsync(UserId);
            var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = UserId, code = code }, protocol: Request.Url.Scheme);

            var oUsuario = _Db.AspNetUsers.Find(UserId);

            body = body.Replace("***UsuarioNombre***", oUsuario.Nombre +  " / " + oUsuario.RazonSocial );
            body = body.Replace("***UserName***", oUsuario.UserName);
            body = body.Replace("***PassTemp***", oUsuario.PassTemp);

            body = body.Replace("***EmpresaNombre***", "");

            body = body.Replace("***UrlLink***", callbackUrl
                );
            body = body.Replace("***EmpresaLogo***", "https://proveedores.aki.com.gt/Content/Logos/LogoOperadora.png");

            await UserManager.SendEmailAsync(UserId, "Portal Proveedores - Confirmacion Correo - " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")
                                             , body);
            TempData["MensajeWarning"] = "Correo de Usuario Nuevo Enviado...";
            return RedirectToAction("Edit", new { id = UserId });
        }


        [Authorize]
        [Authorize(Roles = "Administradores")]
        public ActionResult AddRole(String _roleid, String _Username)
        {
            // Verificacion de Permisos
            if (!Servicios.ServicioSeguridad.ValidaPermisos(this.ControllerContext.RequestContext.RouteData.Values, User.Identity.Name)) return RedirectToAction("Permisos", "Home");

            Infraestructura.AspNetUsers aspNetUsers = _Db.AspNetUsers.Where(p => p.Id == _Username).ToList().First();
            Infraestructura.AspNetRoles aspRoles = _Db.AspNetRoles.Where(p => p.Id == _roleid).ToList().First();

            AspNetUserRoles _nuevo = new AspNetUserRoles();
            _nuevo.RoleId = aspRoles.Id;
            _nuevo.UserId = aspNetUsers.Id;
            _Db.AspNetUserRoles.Add(_nuevo);
            //aspNetUsers.AspNetRoles.Add(aspRoles);
            _Db.SaveChanges();

            return RedirectToAction("Edit", new { id = _Username });
        }

        [Authorize]
        [Authorize(Roles = "Administradores")]
        public ActionResult Delete(string id)
        {
            // Verificacion de Permisos
            if (!Servicios.ServicioSeguridad.ValidaPermisos(this.ControllerContext.RequestContext.RouteData.Values, User.Identity.Name)) return RedirectToAction("Permisos", "Home");
            Infraestructura.AspNetUsers _Usuario = _Db.AspNetUsers.Find(id);

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            
            if (_Usuario == null)
            {
                return HttpNotFound();
            }            
            _Db.AspNetUsers.Remove(_Usuario);
            _Db.SaveChanges();

            return RedirectToAction("Index", "Usuario");
        }

        [Authorize]
        public ActionResult ExportExcel()
        {
            var _Usuarios = (from l in _Db.AspNetUsers
                             orderby l.Nombre
                             select new { CodigoUsuario = l.UserName, Nombre = l.Nombre, Pueto = l.Puesto, Correo = l.Email, Telefono = l.PhoneNumber, l.RazonSocial }).ToList();

            _ServExport.ToExcel(Response, _Usuarios, "ListadoUsuarios");
            return View();
        }


        [Authorize]
        public ActionResult RemoveRole(String _roleid, String _Username)
        {
            // Verificacion de Permisos
            if (!Servicios.ServicioSeguridad.ValidaPermisos(this.ControllerContext.RequestContext.RouteData.Values, User.Identity.Name)) return RedirectToAction("Permisos", "Home");

            Infraestructura.AspNetUsers aspNetUsers = _Db.AspNetUsers.Where(p => p.Id == _Username).ToList().First();
            Infraestructura.AspNetRoles aspRoles = _Db.AspNetRoles.Where(p => p.Id == _roleid).ToList().First();

            var Borrar = _Db.AspNetUserRoles.Where(p => p.UserId == _Username && p.RoleId == _roleid);
            _Db.AspNetUserRoles.RemoveRange(Borrar);
            //aspNetUsers.AspNetRoles.Remove(aspRoles);
            _Db.SaveChanges();

            return RedirectToAction("Edit", new { id = _Username });
        }

        [Authorize]
        public ActionResult ChangePassword(String _UserName)
        {
            // Verificacion de Permisos
            if (!Servicios.ServicioSeguridad.ValidaPermisos(this.ControllerContext.RequestContext.RouteData.Values, User.Identity.Name)) return RedirectToAction("Permisos", "Home");

            

            ChangePasswordViewModel _Modelo = new ChangePasswordViewModel();
            _Modelo.UserName = _UserName;
            _Modelo.OldPassword = "123456";

            return View(_Modelo);
        }

        //
        // POST: /Manage/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<ActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            // Verificacion de Permisos
            if (!Servicios.ServicioSeguridad.ValidaPermisos(this.ControllerContext.RequestContext.RouteData.Values, User.Identity.Name)) return RedirectToAction("Permisos", "Home");


            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var _UserItem = _Db.AspNetUsers.Where(p => p.UserName == model.UserName).ToList().First();

            string resetToken = await UserManager.GeneratePasswordResetTokenAsync( _UserItem.Id);
            IdentityResult passwordChangeResult = await UserManager.ResetPasswordAsync(_UserItem.Id, resetToken, model.NewPassword);

            
            if (passwordChangeResult.Succeeded)
            {
                
                return RedirectToAction("Index","Usuario");
            }

            AddErrors(passwordChangeResult);
            return View(model);
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }
    }


   

}