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


namespace Wareways.PortalProv.Controllers.Seguridad
{
    [Authorize]
    public class UsuarioController : Controller
    {
        Infraestructura.PortalProvEntities _Db = new Infraestructura.PortalProvEntities();

        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        Servicios.Export _ServExport = new Servicios.Export();

        // GET: Usuario
        public ActionResult Index()
        {
            // Verificacion de Permisos
            if (!Servicios.ServicioSeguridad.ValidaPermisos(this.ControllerContext.RequestContext.RouteData.Values, User.Identity.Name)) return RedirectToAction("Permisos", "Home");

            var _Lista = _Db.AspNetUsers;
            return View(_Lista);
        }

        public ActionResult Edit(string id)
        {
            // Verificacion de Permisos
            if (!Servicios.ServicioSeguridad.ValidaPermisos(this.ControllerContext.RequestContext.RouteData.Values, User.Identity.Name)) return RedirectToAction("Permisos", "Home");
            

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Infraestructura.AspNetUsers aspNetUsers = _Db.AspNetUsers.Where(p => p.Id == id).ToList().First();

            ViewBag.RolesAsignados = _Db.V_GEN_UsuarioRoles.AsNoTracking().Where(P => P.UserId == aspNetUsers.Id).ToList();
            ViewBag.RolesDisponibles = _Db.V_GEN_UsuarioRoles_Diponibles.AsNoTracking().Where(P => P.UserId == aspNetUsers.Id).ToList();

            

            if (aspNetUsers == null)
            {
                return HttpNotFound();
            }
            return View(aspNetUsers);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
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

                _Db.SaveChanges();

                return RedirectToAction("Index");
            }
            return View(_Modelo);
        }

      
       

        


        [Authorize]
        public ActionResult AddRole(String _roleid, String _Username)
        {
            // Verificacion de Permisos
            if (!Servicios.ServicioSeguridad.ValidaPermisos(this.ControllerContext.RequestContext.RouteData.Values, User.Identity.Name)) return RedirectToAction("Permisos", "Home");

            Infraestructura.AspNetUsers aspNetUsers = _Db.AspNetUsers.Where(p => p.Id == _Username).ToList().First();
            Infraestructura.AspNetRoles aspRoles = _Db.AspNetRoles.Where(p => p.Id == _roleid).ToList().First();

            aspNetUsers.AspNetRoles.Add(aspRoles);
            _Db.SaveChanges();

            return RedirectToAction("Edit", new { id = _Username });
        }

        [Authorize]
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
                             select new { CodigoUsuario = l.UserName, Nombre = l.Nombre, Pueto = l.Puesto, Correo = l.Email, Telefono = l.PhoneNumber }).ToList();

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

            aspNetUsers.AspNetRoles.Remove(aspRoles);
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