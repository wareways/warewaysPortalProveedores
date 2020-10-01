using iText.Forms.Xfdf;
using Microsoft.TeamFoundation.Client.Reporting;
using System;
using System.Collections.Generic;
using System.EnterpriseServices;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Web;
using System.Web.Mvc;
using Wareways.PortalProv.Infraestructura;
using Wareways.PortalProv.Models.PP;

namespace Wareways.PortalProv.Controllers
{

    public class AdmContrasenaController : Controller
    {
        Infraestructura.PortalProvEntities _Db = new Infraestructura.PortalProvEntities();

        [Authorize(Roles = "Oficina")]
        public ActionResult Index()
        {
            var model = new Models.PP.ContrasenaOficinaModel();
            model.L_Documentos = ObtenerDocumentosPorUsuario();
            return View(model);
        }

        public ActionResult TestCorreo(Guid id)
        {
            MandarCorreo_Contrasena(id);

            return RedirectToAction("index");
        }

        private void MandarCorreo_Contrasena(Guid id)
        {
            Infraestructura.FlexManDbEntities _Db_Flex = new FlexManDbEntities();
            string body = string.Empty;
            using (StreamReader reader = new StreamReader(Server.MapPath("~/Infraestructura/Correos/Contrasena.html")))
            {
                body = reader.ReadToEnd();
            }
            var _Contraseña = _Db.PPROV_Contrasena.Find(id);
            var _Proveedor = _Db.V_PPROV_Proveedor.Where(p => p.CardCode == _Contraseña.Contrasena_CardCode).ToList();
            var _Empresa = _Db.V_PPROV_Empresas.Where(p => p.Empresa_Id == _Contraseña.Empresa_Id).ToList();

            body = body.Replace("***EmpresaNombre***", _Empresa[0].AliasName);
            body = body.Replace("***EmpresaLogo***", _Empresa[0].Logo); 

            body = body.Replace("***NombreProveedor***", _Proveedor[0].CardCode + " " + _Proveedor[0].CardName);
            body = body.Replace("***CodigoContraseña***", _Contraseña.Contrasena_Numero.ToString());
            body = body.Replace("***Moneda***", _Contraseña.Contrasena_Moneda);
            body = body.Replace("***FechaEstiamda***", _Contraseña.Contrasena_Fecha_Estimada.ToString("dd/MM/yyyy"));

            body = body.Replace("***TotalDocumentos***", _Contraseña.PPROV_Documento.Count().ToString());

            var _Contenido_Valor = "<tr><td style='color:#933f24; padding:10px 0px; background-color: #f7a084;'>***ContenidovValor***</td></tr>";
            var _FilaInfoFac = "";
            var _FilaInfoOrden = "";
            var _FilaInfoMonto = "";
            foreach (var _Item in _Contraseña.PPROV_Documento)
            {
                _FilaInfoFac += _Contenido_Valor.Replace("***ContenidovValor***", _Item.Doc_Serie + " " + _Item.Doc_Numero);
                _FilaInfoOrden += _Contenido_Valor.Replace("***ContenidovValor***", _Item.Doc_NumeroOC.ToString());
                _FilaInfoMonto += _Contenido_Valor.Replace("***ContenidovValor***", string.Format("{0:#,###,###.00}", _Item.Doc_MontoNeto));
            }
            body = body.Replace("***FilaInfoFac***", _FilaInfoFac);
            body = body.Replace("***FilaInfoOrden***", _FilaInfoOrden);
            body = body.Replace("***FilaInfoMonto***", _FilaInfoMonto);

            var _CorreosDestino = _Db.SP_ObtenerCorreos_Por_CardCode(_Proveedor[0].CardCode).ToList();
            foreach (var _Correo in _CorreosDestino)
            {
                _Db_Flex.NotificacionesCola.Add(new NotificacionesCola
                {
                    Para = _Correo.Email,
                    Copia = "",
                    Asunto = "Portal Proveedores - Contraseña Generada - " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"),
                    Cuerpo = body,
                    EnviadoFecha = null,
                    EnviarHasta = DateTime.Now,
                    ProximoEnvio = null,
                    IntervaloMinutos = 0,
                    Sistema = "SS_Proveedores",
                    Parametro1 = "Contraseña",
                    Parametro2 = id.ToString(),
                    Id = Guid.NewGuid()

                });
            }
                
            _Db_Flex.SaveChanges();
            

        }


        [Authorize(Roles = "Oficina")]
        public ActionResult Eliminar(Guid Contrasena_id)
        {
            var _Contrasena = _Db.PPROV_Contrasena.Find(Contrasena_id);
            foreach(var _item in _Contrasena.PPROV_Documento)
            {
                _item.Doc_Estado = "Contraseña";
                _item.Contrasena_Id = null;
            }
            _Db.SaveChanges();

            _Db.PPROV_Contrasena.Remove(_Contrasena);
            _Db.SaveChanges();


            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Oficina")]
        public ActionResult CreaContrasena(FormCollection collection, PresentadosModelOficina modelo)
        {
            var _Select_Item = collection["Selec_Item"].Replace("true,false", "true");
            var _Select_Code = collection["Selec_Code"];
            var _Select_Prov = collection["Selec_Prov"];
            var _Select_Empresa = collection["Selec_Empresa"];

            var _Lista = new List<V_PPROV_Documentos_Oficina>();
            for (int i = 0; i < _Select_Item.Split(',').Length; ++i)
            {
                if (_Select_Item.Split(',')[i] == "true")
                {
                    _Lista.Add(new V_PPROV_Documentos_Oficina
                    {
                        Doc_Id = Guid.Parse(_Select_Code.Split(',')[i])
                                                                ,
                        Doc_CardCorde = _Select_Prov.Split(',')[i]
                                                                ,
                        Doc_EmpresaId = int.Parse(_Select_Empresa.Split(',')[i])
                    });
                }
            }



            if (_Lista.Count > 0)
            {
                var _Doc = _Db.PPROV_Documento.Find(_Lista[0].Doc_Id);

                var _contraseña = new Infraestructura.PPROV_Contrasena();
                _contraseña.Empresa_Id = (int)_Lista.Select(p => p.Doc_EmpresaId).First();
                _contraseña.Contrasena_Estado = "Borrador";
                _contraseña.Contrasena_Fecha = DateTime.Now;
                _contraseña.Contrasena_Id = Guid.NewGuid();
                _contraseña.Contrasena_Numero = ObtenerCorrelativoContrasena(_contraseña.Empresa_Id);
                _contraseña.Contrasena_Usuario = User.Identity.Name;
                _contraseña.Contrasena_Fecha_Estimada = ObtenerFechaVencimiento(_Lista[0].Doc_CardCorde, _contraseña.Contrasena_Fecha);
                _contraseña.Contrasena_CardCode = _Doc.Doc_CardCorde;
                _contraseña.Contrasena_Moneda = _Doc.Doc_Moneda;
                _contraseña.Contrasena_EmpresaId = _Doc.Doc_EmpresaId;


                _Db.PPROV_Contrasena.Add(_contraseña);
                _Db.SaveChanges();
                foreach (var _Detalle in _Lista)
                {
                    _contraseña.PPROV_Documento.Add(_Db.PPROV_Documento.Find(_Detalle.Doc_Id));
                }
                _Db.SaveChanges();
                foreach (var _Detalle in _contraseña.PPROV_Documento)
                {
                    _Detalle.Doc_Estado = "Contraseña";
                }
                _Db.SaveChanges();
            }


            return RedirectToAction("Index", "admcontrasena");
        }

        [Authorize(Roles = "Oficina")]
        public ActionResult AsignarEstado(Guid Contrasena_Id, string CambioEstado)
        {
            var _Contraseña = _Db.PPROV_Contrasena.Find(Contrasena_Id);
            var _AnterioEstado = _Contraseña.Contrasena_Estado;
            _Contraseña.Contrasena_Estado = CambioEstado;
            _Db.SaveChanges();
            foreach(var _Document in _Contraseña.PPROV_Documento)
            {
                _Document.Doc_Estado = (_Document.Doc_Estado == "Contraseña") ? "Retenciones" : _Document.Doc_Estado;
                _Db.SaveChanges();
            }
            if (_AnterioEstado == "Borrador" && _Contraseña.Contrasena_Estado == "Activa") MandarCorreo_Contrasena(_Contraseña.Contrasena_Id);

            return RedirectToAction("Edit", new { id = Contrasena_Id });
        }

       


        [Authorize(Roles = "Oficina")]
        public ActionResult Edit(Guid id)
        {
            var contraseña = _Db.PPROV_Contrasena.Find(id);
            var model = new PPROV_ContrasenaModel
            {
                Contrasena_Estado = contraseña.Contrasena_Estado,
                Contrasena_Fecha = contraseña.Contrasena_Fecha,
                Contrasena_Fecha_Estimada = contraseña.Contrasena_Fecha_Estimada,
                Contrasena_Id = contraseña.Contrasena_Id,
                Contrasena_Numero = contraseña.Contrasena_Numero,
                Contrasena_Usuario = contraseña.Contrasena_Usuario,
                Contrasena_CardCode = contraseña.Contrasena_CardCode
            };
            ViewBag.Listado_Empresas = _Db.V_PPROV_Empresas.OrderBy(p => p.Empresa_Id).ToList();
            ViewBag.Proveedor = _Db.V_PPROV_Proveedor.Where(p => p.CardCode == model.Contrasena_CardCode).First();

            ViewBag.ListaEstado = new SelectList(new List<SelectListItem>
                {
                    new SelectListItem { Text = "Activa", Value = "Activa"},
                    new SelectListItem { Text = "Borrador", Value = "Borrador"}
                },"Text","Value","Activa");


            model.ListaDocumentos = contraseña.PPROV_Documento.ToList();
            var _Retenciones = new List<Infraestructura.PPROV_Retencion>();
            foreach ( var _Item in contraseña.PPROV_Documento)
            {
                _Retenciones.AddRange(_Item.PPROV_Retencion);
            }
            model.ListaRetenciones = _Retenciones.Distinct().ToList();

            return View(model);

        }

        [HttpPost]
        [Authorize(Roles = "Oficina")]
        public ActionResult Edit(PPROV_ContrasenaModel model)
        {
            var _Contrasena = _Db.PPROV_Contrasena.Find(model.Contrasena_Id);
            _Contrasena.Contrasena_Fecha_Estimada = model.Contrasena_Fecha_Estimada;
            _Db.SaveChanges();

            return RedirectToAction("Edit", "admcontrasena", new { id = model.Contrasena_Id });
        }

        private DateTime ObtenerFechaVencimiento(string cardCode, DateTime fechaInicio)
        {
            Int32 _RetornaDias = 30;
            DateTime _Retorna = fechaInicio;
            var _Proveedor = _Db.V_PPROV_Proveedor.AsNoTracking().Where(p => p.CardCode == cardCode).ToList();
            // Dias Credito del Proveedor Si es mayor a 30 dias
            if (_Proveedor.Count > 0) if (_Proveedor[0].DiasCredito > 0) _RetornaDias = _Proveedor[0].DiasCredito;
            // Agrega Dias
            _Retorna = _Retorna.Date.AddDays(_RetornaDias);
            // Corrección Proximo Viernes
            int num_days = System.DayOfWeek.Friday - _Retorna.DayOfWeek;
            if (num_days < 0) num_days += 7;
            _Retorna = _Retorna.AddDays(num_days);

            return _Retorna;
        }

        private int ObtenerCorrelativoContrasena(int empresa_Id)
        {
            Int32 _Retorna = 1;
            try { _Retorna = _Db.PPROV_Contrasena.Where(p => p.Empresa_Id == empresa_Id).Select(p => p.Contrasena_Numero).Max() + 1; } catch { }
            return _Retorna;
        }

        private List<V_PPROV_Contrasena_Oficina> ObtenerDocumentosPorUsuario()
        {
            var _UserName = User.Identity.Name;
            var _Datos = _Db.V_PPROV_Contrasena_Oficina.Where(p=>p.UserName == _UserName).ToList();

            return _Datos;
        }
    }
}