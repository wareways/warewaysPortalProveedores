using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Wareways.PortalProv.Servicios;

namespace Wareways.PortalProv.Controllers
{
    [Authorize]
    public class DepartamentoController : Controller
    {
        Infraestructura.PortalProvEntities _Db = new Infraestructura.PortalProvEntities();
        VServicio vServicio = new VServicio();

        [Authorize(Roles = "Administradores")]
        public ActionResult Index()
        {
          
            var _EmpresaId = ConfigurationManager.AppSettings["WWPortal_EmpresaId"];
            var modelo = _Db.PPROV_Departamento.Where(p => p.Empresa_Id == _EmpresaId);

            return View(modelo);
        }

        public ActionResult Nuevo(String NuevoNombre)
        {
            
            var _EmpresaId = ConfigurationManager.AppSettings["WWPortal_EmpresaId"]; 

            if (string.IsNullOrEmpty(NuevoNombre))
            {
                @TempData["MensajeWarning"] = "No se Puede crear Departamento con Nombre Vacio";
            }
            else
            {
                if (_Db.PPROV_Departamento.Where(p => p.DepartmentName == NuevoNombre && p.Empresa_Id == _EmpresaId).Count() > 0)
                {
                    @TempData["MensajeWarning"] = "Nombre Ya existe";
                }
                else
                {
                    var modelo = _Db.PPROV_Departamento.Add(new Infraestructura.PPROV_Departamento { DepartmentId = Guid.NewGuid(), DepartmentName = NuevoNombre, Empresa_Id = _EmpresaId });
                    _Db.SaveChanges();
                    @TempData["MensajeSuccess"] = "Departamento Agregado con Exito";
                }
            }
          

            return RedirectToAction("index");
        }

        public ActionResult Editar(String DeptoNuevo, String DepartamentoId)
        {
            
            var _EmpresaId = ConfigurationManager.AppSettings["WWPortal_EmpresaId"];

            if (string.IsNullOrEmpty(DeptoNuevo))
            {
                @TempData["MensajeWarning"] = "No se Puede crear Departamento con Nombre Vacio";
            }
            else
            {
                if (_Db.PPROV_Departamento.Where(p => p.DepartmentName == DeptoNuevo && p.Empresa_Id == _EmpresaId).Count() > 0)
                {
                    @TempData["MensajeWarning"] = "Nombre Ya existe";
                }
                else
                {
                    var _DepartamentoId = Guid.Parse(DepartamentoId);
                    var modelo = _Db.PPROV_Departamento.Where(p=>p.DepartmentId == _DepartamentoId && p.Empresa_Id == _EmpresaId ).ToList();
                    modelo[0].DepartmentName = DeptoNuevo;
                    _Db.SaveChanges();
                    @TempData["MensajeSuccess"] = "Departamento Modificado con Exito";
                }
            }


            return RedirectToAction("index");
        }
    }
}