using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Configuration;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using Wareways.PortalProv.Infraestructura;
using Wareways.PortalProv.Models.PP;

namespace Wareways.PortalProv.Controllers
{
    public class admretencionesController : Controller
    {
        PortalProvEntities _Db = new PortalProvEntities();


        [Authorize(Roles = "Oficina")]
        public ActionResult Index()
        {

            try
            {
                ViewBag.MostrarRedireccion = false;
                if ( User.IsInRole("Retenciones") ) ViewBag.MostrarRedireccion = true;

                var model = new Models.PP.RetencionesOficinaModel();
                model.L_Retenciones = ObtenerRetencionesPorusuario();

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["MensajeDanger"] = (ex.InnerException == null) ? ex.Message : ex.InnerException.Message;
            }
            return RedirectToAction("Index");

        }

        [HttpGet]
        [Authorize(Roles = "Oficina")]
        public ActionResult Eliminar(Guid Retencion_Id)
        {
            try
            {
                _Db.SP_PPROV_Elimina_Retencion(Retencion_Id);

            }
            catch (Exception ex)
            {
                TempData["MensajeDanger"] = (ex.InnerException == null) ? ex.Message : ex.InnerException.Message;
            }
            return RedirectToAction("Index");
        }


        [Authorize(Roles = "Oficina")]
        public ActionResult Nuevo(Guid? Doc_Id)
        {
            var model = new Models.PP.RetencionesOficinaNuevo();
            try
            {
                model.Retencion_Fecha = DateTime.Now.Date;
                model._DocId = Doc_Id;
                if (Doc_Id != null) model.Retencion_CardCode = _Db.PPROV_Documento.Find(Doc_Id).Doc_CardCorde;
                model.Lista_TiposRet = _Db.PPROV_RetencionTipo.ToList();
                model.Lista_Moneda = _Db.GEN_CatalogoDetalle.Where(p => p.Catalogo_Id == (int)Servicios.TipoCatalogo.Moneda).OrderBy(p => p.Orden).ToList();
                model.Modo_Activo = "Paso1";
                ViewBag.EmpresaId = ConfigurationManager.AppSettings["WWPortal_EmpresaId"];

            }
            catch (Exception ex)
            {
                TempData["MensajeDanger"] = (ex.InnerException == null) ? ex.Message : ex.InnerException.Message;
            }

            return View(model);
        }



        [HttpPost]
        [Authorize(Roles = "Oficina")]
        public ActionResult Nuevo(Models.PP.RetencionesOficinaNuevo model, HttpPostedFileBase filefac, FormCollection collection, string submit_Step1, string submit_Step2)
        {
            ViewBag.EmpresaId = ConfigurationManager.AppSettings["WWPortal_EmpresaId"];
            model.Lista_TiposRet = _Db.PPROV_RetencionTipo.ToList();
            model.Lista_Moneda = _Db.GEN_CatalogoDetalle.Where(p => p.Catalogo_Id == (int)Servicios.TipoCatalogo.Moneda).OrderBy(p => p.Orden).ToList();
            ViewBag.Usuario_Empresas = _Db.SP_PPROV_ADM_EmpresasOficina(User.Identity.Name).Select(p => new { p.Empresa_Id, p.Empresa_Name }).Distinct().ToList();

            try
            {
                if (model.Modo_Activo == "Paso2")
                {

                    if (ValidacionRetencion(model))
                    {
                        var _DocRef = _Db.PPROV_Documento.Find(model._DocId);

                        var _Nuevo = new PPROV_Retencion
                        {
                            Retencion_Fecha = model.Retencion_Fecha,
                            Retencion_Id = Guid.NewGuid(),
                            Retencion_Moneda = model.Retencion_Moneda,
                            Retencion_Monto = model.Retencion_Monto,
                            Retencion_Numero = model.Retencion_Numero,
                            Retencion_Pdf = model.Retencion_Pdf,
                            Retencion_Tipo = model.Retencion_Tipo,
                            Retencion_Usuario = User.Identity.Name,
                            Retencion_CardCode = model.Retencion_CardCode,

                        };
                        if (model._DocId == null)
                        {
                            _Nuevo.Retencion_CardCode = model.Retencion_CardCode.Split(' ')[0];
                            _Nuevo.Retencion_EmpresaId = model.Manual_Empresa;
                            _Nuevo.Manual_Empresa = Int32.Parse(ConfigurationManager.AppSettings["WWPortal_EmpresaId"]);
                            _Nuevo.Manual_FacNumero = model.Manual_FacNumero;
                            _Nuevo.Manual_Fac_Serie = model.Manual_Fac_Serie;
                        }
                        else
                        {
                            _Nuevo.Retencion_EmpresaId = _DocRef.Doc_EmpresaId;
                        }


                        _Db.PPROV_Retencion.Add(_Nuevo);
                        _Db.SaveChanges();
                        _Nuevo.PPROV_Documento.Add(_DocRef);
                        _Db.SaveChanges();
                        MandarCorreo_Retencion(_Nuevo.Retencion_Id);

                        if (model._DocId != null)
                        {
                            return RedirectToAction("index", "admpresentados", new { FiltroEstado = "Retenciones" });
                        }
                        return RedirectToAction("index");
                    }
                }


                if (model.Modo_Activo == "Paso1")
                {
                    if (model.Nuevo_Pdf_Name == null) model.Nuevo_Pdf_Name = string.Format("RET_{0}.pdf", Guid.NewGuid().ToString());
                    if (filefac.ContentLength != null)
                    {
                        // Upload Files to Server
                        var _ServerPath = Server.MapPath(@"~/Cargados/" + model.Retencion_CardCode + "/");
                        if (!System.IO.Directory.Exists(_ServerPath)) Directory.CreateDirectory(_ServerPath);

                        String _Fact_Path = System.IO.Path.Combine(_ServerPath, model.Nuevo_Pdf_Name);
                        filefac.SaveAs(_Fact_Path);
                        model.Modo_Activo = "Paso2";
                        model.Retencion_Fecha = DateTime.Now;
                        model.Retencion_Pdf = @"/Cargados/" + model.Retencion_CardCode + "/" + model.Nuevo_Pdf_Name;
                        ModelState.Clear();

                        if (model._DocId != null)
                        {
                            var _DocRef = _Db.PPROV_Documento.Find(model._DocId);
                            model.Retencion_CardCode = _DocRef.Doc_CardCorde;
                        }


                    }
                    else
                    {
                        TempData["MensajeDanger"] = "Debe de Cargar el PDF del la retencion";
                    }
                }

            }
            catch (Exception ex)
            {
                TempData["MensajeDanger"] = "No se Pudo Cargar el Archivo Indicado";
                ModelState.Clear();
                return View(model);
            }

            return View(model);

        }


        private bool ValidacionRetencion(RetencionesOficinaNuevo model)
        {
            return true;
        }

        public ActionResult TestRetencion(Guid id)
        {
            MandarCorreo_Retencion(id);

            return RedirectToAction("index");
        }

        private void MandarCorreo_Retencion(Guid id)
        {
            var EmpresaAsuntoCorreo = _Db.GEN_Empresa.First().EmpresaAsuntoCorreo;

            string body = string.Empty;
            using (StreamReader reader = new StreamReader(Server.MapPath("~/Infraestructura/Correos/Retencion.html")))
            {
                body = reader.ReadToEnd();
            }
            var _Retencion = _Db.PPROV_Retencion.Find(id);
            var _Proveedor = _Db.V_PPROV_Proveedor.Where(p => p.CardCode == _Retencion.Retencion_CardCode).ToList();
            var _EmpresaId = 0;
            if (_Retencion.PPROV_Documento.ToList()[0] == null)
            {
                _EmpresaId = (Int32)_Retencion.Manual_Empresa;
            }
            else { _EmpresaId =  (Int32)_Retencion.PPROV_Documento.ToList()[0].Doc_EmpresaId; }

            var _Empresa = _Db.V_PPROV_Empresas.Where(p => p.Empresa_Id == _EmpresaId).ToList();

            body = body.Replace("***EmpresaNombre***", _Empresa[0].AliasName);
            body = body.Replace("***EmpresaLogo***", _Empresa[0].Logo);

            body = body.Replace("***NombreProveedor***", _Proveedor[0].CardCode + " " + _Proveedor[0].CardName);
            body = body.Replace("***CodigoRetencion***", _Retencion.Retencion_Numero);
            body = body.Replace("***Moneda***", _Retencion.Retencion_Moneda);

            body = body.Replace("***UrlLink***", "https://proveedores.aki.com.gt/");




            var _Contenido_Valor = "<tr><td style='color:#933f24; padding:10px 0px; background-color: white;'>***ContenidovValor***</td></tr>";
            var _FilaInfoFac = "";
            var _FilaInfoTipo = "";
            var _FilaInfoMonto = "";
            foreach (var _Item in _Retencion.PPROV_Documento)
            {
                if (_Item  == null)
                {
                    _FilaInfoFac += _Contenido_Valor.Replace("***ContenidovValor***", _Retencion.Manual_Fac_Serie + " " + _Retencion.Manual_FacNumero);
                    _FilaInfoTipo += _Contenido_Valor.Replace("***ContenidovValor***", _Retencion.PPROV_RetencionTipo.Retencion_Nombre);
                    _FilaInfoMonto += _Contenido_Valor.Replace("***ContenidovValor***", string.Format("{0:#,###,###.00}", _Retencion.Retencion_Monto));
                } else
                {
                    _FilaInfoFac += _Contenido_Valor.Replace("***ContenidovValor***", _Item.Doc_Serie + " " + _Item.Doc_Numero);
                    _FilaInfoTipo += _Contenido_Valor.Replace("***ContenidovValor***", _Retencion.PPROV_RetencionTipo.Retencion_Nombre);
                    _FilaInfoMonto += _Contenido_Valor.Replace("***ContenidovValor***", string.Format("{0:#,###,###.00}", _Retencion.Retencion_Monto));
                }
                    
            }
            body = body.Replace("***FilaInfoFac***", _FilaInfoFac);
            body = body.Replace("***FilaInfoTipo***", _FilaInfoTipo);
            body = body.Replace("***FilaInfoMonto***", _FilaInfoMonto);

            var _CorreosDestino = _Db.SP_ObtenerCorreos_Por_CardCode(_Retencion.Retencion_CardCode).ToList();
            foreach (var _Correo in _CorreosDestino)
            {
            
                using (var message = new MailMessage())
                {
                    SmtpSection smtpSection = (SmtpSection)ConfigurationManager.GetSection("system.net/mailSettings/smtp");

                    message.To.Add(new MailAddress(_Correo.Email));
                    message.From = new MailAddress(smtpSection.From);
                    message.Subject = EmpresaAsuntoCorreo + " Portal Proveedores - Retencion Generada - " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                    message.Body = body;
                    message.IsBodyHtml = true; // change to true if body msg is in html

                    
                    using (var client = new SmtpClient(smtpSection.Network.Host))
                    {
                        System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                        NetworkCredential networkCred = new NetworkCredential(smtpSection.Network.UserName, smtpSection.Network.Password);
                        client.UseDefaultCredentials = smtpSection.Network.DefaultCredentials;
                        client.Port = smtpSection.Network.Port;
                        client.Credentials = networkCred;
                        client.EnableSsl = smtpSection.Network.EnableSsl;

                        try
                        {
                            client.Send(message); // Email sent
                        }
                        catch (Exception ex)
                        {
                            throw new Exception(ex.Message);
                        }
                    }
                }
            }
            
            


        }


        private List<V_PPROV_Retenciones_Oficina> ObtenerRetencionesPorusuario()
        {
            var _UserName = User.Identity.Name;
            var _Datos = _Db.V_PPROV_Retenciones_Oficina.Where(p => p.UserName == _UserName).ToList();

            return _Datos;
        }
    }
}