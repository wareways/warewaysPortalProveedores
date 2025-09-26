using iText.Forms.Xfdf;
using Microsoft.VisualBasic;
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
using System.Xml;
using Wareways.PortalProv.Infraestructura;
using Wareways.PortalProv.Infraestructura.SAP;
using Wareways.PortalProv.Models.OC;
using Wareways.PortalProv.Models.PP;
using Wareways.PortalProv.Servicios;

namespace Wareways.PortalProv.Controllers
{
    public class admpresentadosController : Controller
    {
        PortalProvEntities _Db = new PortalProvEntities();

        VServicio vServicio = new VServicio();

        [Authorize(Roles = "Oficina")]
        public ActionResult ms_envRetenciones(FormCollection collection, PresentadosModelOficina modelo)
        {
            var _Select_Item = collection["Selec_Item"].Replace("true,false", "true");
            var _Select_Code = collection["Selec_Code"];
            var _Select_Prov = collection["Selec_Prov"];
            var _Select_Empresa = collection["Selec_Empresa"];

            var _Lista = new List<WWSP_Documentos_Oficina_Result>();
            for (int i = 0; i < _Select_Item.Split(',').Length; ++i)
            {
                if (_Select_Item.Split(',')[i] == "true")
                {
                    _Lista.Add(new WWSP_Documentos_Oficina_Result
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
                foreach (var Item in _Lista)
                {
                    //ProcesoCreacionLineasFact(Item.Doc_Id);

                    var oPresentdos = _Db.PPROV_Documento.Where(p => p.Doc_Id == Item.Doc_Id).First();
                    oPresentdos.Doc_Estado = "Retenciones";
                    _Db.SaveChanges();
                }


                TempData["MensajeSuccess"] = "Enviado para Retenciones";
                return RedirectToAction("Index"); //, new { id = Fel_Unique });
            }

            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Oficina")]
        public ActionResult ms_envIngSAP(FormCollection collection, PresentadosModelOficina modelo)
        {
            var _Select_Item = collection["Selec_Item"].Replace("true,false", "true");
            var _Select_Code = collection["Selec_Code"];
            var _Select_Prov = collection["Selec_Prov"];
            var _Select_Empresa = collection["Selec_Empresa"];

            var _Lista = new List<WWSP_Documentos_Oficina_Result>();
            for (int i = 0; i < _Select_Item.Split(',').Length; ++i)
            {
                if (_Select_Item.Split(',')[i] == "true")
                {
                    _Lista.Add(new WWSP_Documentos_Oficina_Result
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
                foreach (var Item in _Lista)
                {
                    //ProcesoCreacionLineasFact(Item.Doc_Id);

                    var oPresentdos = _Db.PPROV_Documento.Where(p => p.Doc_Id == Item.Doc_Id).First();
                    oPresentdos.Doc_Estado = "Ingreso SAP";
                    _Db.SaveChanges();
                }


                TempData["MensajeSuccess"] = "Enviado para Ingreso SAP";
                return RedirectToAction("Index"); //, new { id = Fel_Unique });
            }

            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Oficina")]
        public ActionResult ms_rechContras(FormCollection collection, PresentadosModelOficina modelo)
        {
            var _Select_Item = collection["Selec_Item"].Replace("true,false", "true");
            var _Select_Code = collection["Selec_Code"];
            var _Select_Prov = collection["Selec_Prov"];
            var _Select_Empresa = collection["Selec_Empresa"];

            var _Lista = new List<WWSP_Documentos_Oficina_Result>();
            for (int i = 0; i < _Select_Item.Split(',').Length; ++i)
            {
                if (_Select_Item.Split(',')[i] == "true")
                {
                    _Lista.Add(new WWSP_Documentos_Oficina_Result
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
                foreach (var Item in _Lista)
                {
                    var oPresentdos = _Db.PPROV_Documento.Where(p => p.Doc_Id == Item.Doc_Id).First();
                    oPresentdos.Doc_Estado = "Contraseña";
                    _Db.SaveChanges();
                }


                TempData["MensajeSuccess"] = "Documento Rechazado";
                return RedirectToAction("Index"); //, new { id = Fel_Unique });
            }

            return RedirectToAction("Index");
        }


        [Authorize(Roles = "Oficina")]
        public ActionResult ms_rechIngSap(FormCollection collection, PresentadosModelOficina modelo)
        {
            var _Select_Item = collection["Selec_Item"].Replace("true,false", "true");
            var _Select_Code = collection["Selec_Code"];
            var _Select_Prov = collection["Selec_Prov"];
            var _Select_Empresa = collection["Selec_Empresa"];

            var _Lista = new List<WWSP_Documentos_Oficina_Result>();
            for (int i = 0; i < _Select_Item.Split(',').Length; ++i)
            {
                if (_Select_Item.Split(',')[i] == "true")
                {
                    _Lista.Add(new WWSP_Documentos_Oficina_Result
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
                foreach (var Item in _Lista)
                {
                    var oPresentdos = _Db.PPROV_Documento.Where(p => p.Doc_Id == Item.Doc_Id).First();
                    oPresentdos.Doc_Estado = "Ingreso SAP";
                    _Db.SaveChanges();
                }


                TempData["MensajeSuccess"] = "Documento Rechazado";
                return RedirectToAction("Index"); //, new { id = Fel_Unique });
            }

            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Oficina")]
        public ActionResult ms_envRegisFact(FormCollection collection, PresentadosModelOficina modelo)
        {
            var _Select_Item = collection["Selec_Item"].Replace("true,false", "true");
            var _Select_Code = collection["Selec_Code"];
            var _Select_Prov = collection["Selec_Prov"];
            var _Select_Empresa = collection["Selec_Empresa"];

            var _Lista = new List<WWSP_Documentos_Oficina_Result>();
            for (int i = 0; i < _Select_Item.Split(',').Length; ++i)
            {
                if (_Select_Item.Split(',')[i] == "true")
                {
                    _Lista.Add(new WWSP_Documentos_Oficina_Result
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
                foreach (var Item in _Lista)
                {
                    ProcesaCreaFactSAP(Item.Doc_Id);
                }


                TempData["MensajeSuccess"] = "Enviado Registrada";
                return RedirectToAction("Index"); //, new { id = Fel_Unique });
            }

            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Oficina")]
        public ActionResult ms_envFactAut(FormCollection collection, PresentadosModelOficina modelo)
        {
            var _Select_Item = collection["Selec_Item"].Replace("true,false", "true");
            var _Select_Code = collection["Selec_Code"];
            var _Select_Prov = collection["Selec_Prov"];
            var _Select_Empresa = collection["Selec_Empresa"];

            var _Lista = new List<WWSP_Documentos_Oficina_Result>();
            for (int i = 0; i < _Select_Item.Split(',').Length; ++i)
            {
                if (_Select_Item.Split(',')[i] == "true")
                {
                    _Lista.Add(new WWSP_Documentos_Oficina_Result
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
                foreach (var Item in _Lista)
                {
                    ProcesoCreacionLineasFact(Item.Doc_Id);

                    var oPresentdos = _Db.PPROV_Documento.Where(p => p.Doc_Id == Item.Doc_Id).First();
                    oPresentdos.Doc_Estado = "Por Autorizar";
                    _Db.SaveChanges();
                }


                TempData["MensajeSuccess"] = "Enviado para Autorizacion";
                return RedirectToAction("Index"); //, new { id = Fel_Unique });
            }

            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Oficina")]
        public ActionResult ActualizaEncabezado(FormCollection collection)
        {
            var PDoc_Doc_Id = collection["PDoc.Doc_Id"];
            var xDoc_id = Guid.Parse(PDoc_Doc_Id);
            var PDocDoc_Serie = collection["PDoc.Doc_Serie"];
            var PDocDoc_Numero = collection["PDoc.Doc_Numero"];
            var PDoc_Doc_Autorizacion = collection["PDoc.Doc_Autorizacion"];
            var lst = _Db.PPROV_Documento.Where(p => p.Doc_Id == xDoc_id).ToList();
            foreach (var item in lst)
            {
                item.Doc_Serie = PDocDoc_Serie;
                item.Doc_Numero = PDocDoc_Numero;
                item.Doc_Autorizacion = PDoc_Doc_Autorizacion;
            }
            _Db.SaveChanges();

            return RedirectToAction("Detalle", new { id = PDoc_Doc_Id });
        }


        [Authorize(Roles = "Oficina")]
        public ActionResult Detalle(DetallePresentadosModel model, String id)
        {
            // Carga Inicial
            if (model.PDoc == null)
            {
                try { model.PDoc = _Db.PPROV_Documento.Where(p => p.Doc_Id.ToString() == id).First(); } catch { }
            }

            if (model.Generado == null)
            {
                try { model.Generado = _Db.SAP_Doc.Where(p => p.FEL_Unique == model.PDoc.Doc_Id).First(); } catch { }
                try { model.GeneradoDetalle = _Db.SAP_DocDetalle.Where(p => p.FEL_Unique == model.PDoc.Doc_Id).ToList(); } catch { }
            }

            model.CuentasTodas = vServicio.List_SapCuentasServicioTodas();

            var _ListaCentoCosto = (from d in vServicio.List_SapCentroCosto() select d).ToList();
            _ListaCentoCosto.Add(new oCentroCosto { ocrCode = "", ocrName = "" });
            model.CentroCostos = _ListaCentoCosto;


            return View(model);

        }

        [Authorize(Roles = "Oficina")]
        public ActionResult editDetalle(DetallePresentadosModel model, String id, string dt)
        {
            // Carga Inicial
            if (model.PDoc == null)
            {
                try { model.PDoc = _Db.PPROV_Documento.Where(p => p.Doc_Id.ToString() == id).First(); } catch { }
            }

            if (model.Generado == null)
            {
                try { model.Generado = _Db.SAP_Doc.Where(p => p.FEL_Unique == model.PDoc.Doc_Id).First(); } catch { }
                try { model.GeneradoDetalle = _Db.SAP_DocDetalle.Where(p => p.FEL_Unique == model.PDoc.Doc_Id && p.Linea.ToString() == dt).ToList(); } catch { }
            }

            var _ListaCentoCosto = (from d in vServicio.List_SapCentroCosto() select d).ToList();
            _ListaCentoCosto.Add(new oCentroCosto { ocrCode = "", ocrName = "" });
            model.CentroCostos = _ListaCentoCosto;

            model.CuentasTodas = (from l in vServicio.List_SapCuentasServicioTodas() select new Models.OC.oCuentaContableSap { AcctCode = l.AcctCode, AcctName = l.ActId + " " + l.AcctName, ActId = l.ActId }).ToList();

            var _ListaImpuesto = vServicio.List_SapImpuesto();
            model.lstImpuestos = _ListaImpuesto;
            model.Impuesto = "EXE";


            return View(model);

        }

        [Authorize(Roles = "Oficina")]
        public ActionResult updateAccount(String FEL_Unique, string Linea, string CuentaContable, string CentroCosto, string PrecioUnitario)
        {
            try
            {
                var ldet = _Db.SAP_DocDetalle.Where(p => p.FEL_Unique.ToString() == FEL_Unique && p.Linea.ToString() == Linea).ToList();
                var lenc = _Db.SAP_Doc.Where(p => p.FEL_Unique.ToString() == FEL_Unique).ToList();
                ldet[0].CuentaContable = CuentaContable;
                ldet[0].CentroCosto = CentroCosto;

                Decimal pu = 0;
                try { pu = decimal.Parse(PrecioUnitario); } catch { }
                //if (pu > 0) // precio unitario valido
                //{
                //    ldet[0].PrecioUnitario = pu;
                //    if (ldet[0].Cantidad > 0) // tiene cantidad
                //    {
                //        ldet[0].TotalLinea = pu * ldet[0].Cantidad;
                //        ldet[0].Impuestos = ldet[0].TotalLinea - Math.Round((ldet[0].TotalLinea / 1.12m), 2);
                //        ldet[0].TotalLinea = ldet[0].TotalLinea - ldet[0].Impuestos;
                //        lenc[0].Total = _Db.SAP_DocDetalle.Where(p => p.FEL_Unique.ToString() == FEL_Unique).Sum(p => p.TotalLinea + p.Impuestos);
                //        _Db.SaveChanges();
                //    }
                //    else
                //    {
                //        ldet[0].TotalLinea = pu * 1;
                //        ldet[0].Impuestos = ldet[0].Impuestos - Math.Round((ldet[0].TotalLinea / 1.12m), 2);
                //        ldet[0].TotalLinea = ldet[0].TotalLinea - ldet[0].Impuestos;
                //        lenc[0].Total = _Db.SAP_DocDetalle.Where(p => p.FEL_Unique.ToString() == FEL_Unique).Sum(p => p.TotalLinea + p.Impuestos);
                //        _Db.SaveChanges();
                //    }

                //    try {
                //        var PDoc = _Db.PPROV_Documento.Where(p => p.Doc_Id.ToString() == FEL_Unique).First();
                //        PDoc.Doc_MontoNeto = lenc[0].Total;
                //        _Db.SaveChanges();
                //    } catch { }

                //}


                _Db.SaveChanges();

                TempData["MensajeSuccess"] = "Actualizar Cuenta con exito";
            }
            catch { }

            return RedirectToAction("detalle", new { id = FEL_Unique });
        }


        [Authorize(Roles = "Oficina")]
        public ActionResult AgregarImpuestoAdic(String FEL_Unique, string Linea, string CuentaContable, string CentroCosto, string PrecioUnitario, string Descripcion, string Impuesto)
        {
            try
            {
                var ldetorg = _Db.SAP_DocDetalle.Where(p => p.FEL_Unique.ToString() == FEL_Unique && p.Linea.ToString() == Linea).ToList();
                var lenc = _Db.SAP_Doc.Where(p => p.FEL_Unique.ToString() == FEL_Unique).ToList();
                var nextline = _Db.SAP_DocDetalle.Where(p => p.FEL_Unique.ToString() == FEL_Unique).Max(p => p.Linea) + 1;

                SAP_DocDetalle ldet = new SAP_DocDetalle();
                ldet.FEL_Unique = ldetorg[0].FEL_Unique;
                ldet.Linea = nextline;
                ldet.TipoDet = ldetorg[0].TipoDet;
                ldet.Cantidad = 1;
                ldet.UnidadMedida = ldetorg[0].UnidadMedida;

                ldet.Descuentos = ldetorg[0].Descuentos;

                ldet.DateAudit = DateTime.Now;
                ldet.UserNameAudit = ldetorg[0].UserNameAudit;
                ldet.TipoImpuesto = Impuesto;
                ldet.ChkImpDetEnt = true;


                ldet.Descripcion = Descripcion;
                ldet.CuentaContable = CuentaContable;
                ldet.CentroCosto = CentroCosto;
                ldet.PrecioUnitario = Decimal.Parse(PrecioUnitario);
                ldet.Impuestos = ldet.TotalLinea - Math.Round((ldet.PrecioUnitario / 1.12m), 2);
                if (ldet.TipoImpuesto == "EXE")
                {
                    ldet.Impuestos = 0;
                }
                ldet.TotalLinea = Decimal.Parse(PrecioUnitario) - ldet.Impuestos;

                _Db.SAP_DocDetalle.Add(ldet);

                // Modificar la linea Original
                var totalnuevo = (ldetorg[0].PrecioUnitario * ldetorg[0].Cantidad) - Decimal.Parse(PrecioUnitario);

                ldetorg[0].Impuestos = 0;
                ldetorg[0].TotalLinea = (ldetorg[0].PrecioUnitario * ldetorg[0].Cantidad) - Decimal.Parse(PrecioUnitario);
                ldetorg[0].PrecioUnitario = Math.Round(ldetorg[0].TotalLinea / ldetorg[0].Cantidad, 4);

                _Db.SaveChanges();

                TempData["MensajeSuccess"] = "Actualizar Cuenta con exito";
            }
            catch { }

            return RedirectToAction("detalle", new { id = FEL_Unique });
        }


        [Authorize(Roles = "Oficina")]
        public ActionResult Index(string FiltroEstado)
        {


            Int32 empresaSelId = Int32.Parse(ConfigurationManager.AppSettings["WWPortal_EmpresaId"]);
            String _FiltroActivo = "Enviado";
            try
            {
                _FiltroActivo = Request.Cookies["FiltroEstado"].Value;
            }
            catch { }

            if (FiltroEstado != null || FiltroEstado == "")
            {
                if (FiltroEstado == "") FiltroEstado = "Enviado";
                var _Cookie = new HttpCookie("FiltroEstado", FiltroEstado);
                _Cookie.Expires = DateTime.MaxValue;
                Response.Cookies.Add(_Cookie);
                _FiltroActivo = FiltroEstado;
            }
            ViewBag.FiltroEstado = _FiltroActivo;

            List<Infraestructura.PPROV_Estado> _Estados = new List<PPROV_Estado>();
            _Estados.Add(new PPROV_Estado { Doc_Estado = "Enviado", Doc_Orden = 1 });
            _Estados.Add(new PPROV_Estado { Doc_Estado = "Rechazado", Doc_Orden = 6 });
            _Estados.Add(new PPROV_Estado { Doc_Estado = "Contraseña", Doc_Orden = 2 });
            _Estados.Add(new PPROV_Estado { Doc_Estado = "Aut. Contraseña", Doc_Orden = 3 });
            _Estados.Add(new PPROV_Estado { Doc_Estado = "Retenciones", Doc_Orden = 4 });
            _Estados.Add(new PPROV_Estado { Doc_Estado = "Ingreso SAP", Doc_Orden = 5 });

            var model = new Models.PP.PresentadosModelOficina();
            model.L_Estados = _Db.WWSP_ResumenEstadoDocumento(empresaSelId).OrderBy(p => p.Doc_Orden).ToList();
            model.L_Documentos = ObtenerDocumentosPorUsuario(_FiltroActivo, empresaSelId);
            model.Nuevo_Estados_Asignar = new SelectList(_Estados.OrderBy(p => p.Doc_Orden), "Doc_Estado", "Doc_Estado", _FiltroActivo);
            model.Nuevo_Estado_Seleccionado = _FiltroActivo;
            model.Nuevo_Estado_Comentario = "";

            ViewBag.MostrarRedireccion = true;
            if (_FiltroActivo == "Enviado" && !User.IsInRole("Enviado")) ViewBag.MostrarRedireccion = false;
            if (_FiltroActivo == "Contraseña" && !User.IsInRole("Contraseña")) ViewBag.MostrarRedireccion = false;
            if (_FiltroActivo == "Retenciones" && !User.IsInRole("Retenciones")) ViewBag.MostrarRedireccion = false;



            // filtro
            if (!string.IsNullOrEmpty(_FiltroActivo))
            {
                model.L_Documentos = model.L_Documentos.Where(p => p.Doc_Estado == _FiltroActivo).ToList();

            }
            if (_FiltroActivo == "Rechazado")
            {
                model.L_Documentos = model.L_Documentos.Where(p => p.Doc_FechaCarga >= DateAndTime.Now.AddMonths(-3)).ToList();
            }


            return View(model);
        }

        [HttpGet]
        public ActionResult LinksRetencionesDocto(Guid id)
        {
            var modelo = _Db.PPROV_Documento.Find(id).PPROV_Retencion.ToList();

            return View(modelo);

        }


        [HttpPost]
        [Authorize(Roles = "Oficina")]
        public ActionResult CambioEstado(Guid CambioEstado_Documento_Id, string Nuevo_Estado_Seleccionado, string Nuevo_Estado_Comentario)
        {
            var _Documento = _Db.PPROV_Documento.Find(CambioEstado_Documento_Id);
            if (_Documento.Doc_Estado != Nuevo_Estado_Seleccionado)
            {
                _Documento.Doc_Estado = Nuevo_Estado_Seleccionado;
                _Db.SaveChanges();
            }

            // No crea Nota si no tiene comentario
            if (Nuevo_Estado_Comentario != "")
            {
                PPROV_Nota _NuevaNota = new PPROV_Nota();
                _NuevaNota.Doc_Estado = Nuevo_Estado_Seleccionado;
                _NuevaNota.Doc_Id = _Documento.Doc_Id;
                _NuevaNota.Nota_Descripción = Nuevo_Estado_Comentario;
                _NuevaNota.Nota_Fecha = DateTime.Now;
                _NuevaNota.Nota_Id = Guid.NewGuid();
                _NuevaNota.Nota_Usuario = User.Identity.Name;
                _Db.PPROV_Nota.Add(_NuevaNota);
                _Db.SaveChanges();
                if (Nuevo_Estado_Seleccionado == "Rechazado") MandarCorreo_Rechazo(_NuevaNota.Doc_Id);
            }



            return RedirectToAction("index");
        }

        private List<WWSP_Documentos_Oficina_Result> ObtenerDocumentosPorUsuario(String estado, Int32 empresaId)
        {
            if (User.IsInRole("ForzarSync"))
            {
                if (estado == "Pendiente" || estado == "Pagado") _Db.Database.ExecuteSqlCommand("EXEC SP_Deteccion_Documentos_SAP_Periodica");
            }


            var _UserName = User.Identity.Name;
            var _Datos = _Db.WWSP_Documentos_Oficina(empresaId, _UserName).ToList();

            return _Datos;
        }

        public ActionResult TestRechazo(Guid id)
        {
            MandarCorreo_Rechazo(id);

            return RedirectToAction("index");
        }




        private void MandarCorreo_Rechazo(Guid id)
        {
            var EmpresaAsuntoCorreo = _Db.GEN_Empresa.First().EmpresaAsuntoCorreo;

            string body = string.Empty;
            using (StreamReader reader = new StreamReader(Server.MapPath("~/Infraestructura/Correos/Rechazo.html")))
            {
                body = reader.ReadToEnd();
            }
            var _Documento = _Db.PPROV_Documento.Find(id);
            var _Proveedor = _Db.V_PPROV_Proveedor.Where(p => p.CardCode == _Documento.Doc_CardCorde).ToList();
            var _Empresa = _Db.V_PPROV_Empresas.Where(p => p.Empresa_Id == _Documento.Doc_EmpresaId).ToList();

            body = body.Replace("***EmpresaNombre***", _Empresa[0].AliasName);
            body = body.Replace("***EmpresaLogo***", _Empresa[0].Logo);

            body = body.Replace("***NombreProveedor***", _Proveedor[0].CardCode + " " + _Proveedor[0].CardName);



            var _Contenido_Valor = "<tr><td style='color:#933f24; padding:10px 0px; background-color: #f7a084;'>***ContenidovValor***</td></tr>";
            var _FilaInfoFac = "";
            var _FilaInfoOrden = "";
            var _FilaInfoMonto = "";

            _FilaInfoFac += _Contenido_Valor.Replace("***ContenidovValor***", _Documento.Doc_Serie + " " + _Documento.Doc_Numero);
            _FilaInfoOrden += _Contenido_Valor.Replace("***ContenidovValor***", _Documento.Doc_NumeroOC.ToString());
            _FilaInfoMonto += _Contenido_Valor.Replace("***ContenidovValor***", string.Format("{0:#,###,###.00}", _Documento.Doc_MontoNeto));

            body = body.Replace("***FilaInfoFac***", _FilaInfoFac);
            body = body.Replace("***FilaInfoOrden***", _FilaInfoOrden);
            body = body.Replace("***FilaInfoMonto***", _FilaInfoMonto);

            var _CorreosDestino = _Db.SP_ObtenerCorreos_Por_CardCode(_Proveedor[0].CardCode).ToList();
            foreach (var _Correo in _CorreosDestino)
            {
                using (var message = new MailMessage())
                {
                    SmtpSection smtpSection = (SmtpSection)ConfigurationManager.GetSection("system.net/mailSettings/smtp");
                    message.To.Add(new MailAddress(_Correo.Email));
                    message.From = new MailAddress(smtpSection.From);
                    message.Subject = EmpresaAsuntoCorreo + " Portal Proveedores - Documento Rechadado, Mensaje enviado el " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                    message.Body = body;
                    message.IsBodyHtml = true; // change to true if body msg is in html


                    using (var client = new SmtpClient(smtpSection.Network.Host))
                    {
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
                            TempData["MensajeDanger"] = ex.Message;

                        }
                    }
                }
            }




        }
        public ActionResult EnviarContrasena(Guid Fel_Unique)
        {
            var oPresentdos = _Db.PPROV_Documento.Where(p => p.Doc_Id == Fel_Unique).First();
            oPresentdos.Doc_Estado = "Contraseña";
            _Db.SaveChanges();

            TempData["MensajeSuccess"] = "Enviado para Contraseña";
            return RedirectToAction("Index"); //, new { id = Fel_Unique });

        }

        public ActionResult EnviarRetencion(Guid Fel_Unique)
        {
            var oPresentdos = _Db.PPROV_Documento.Where(p => p.Doc_Id == Fel_Unique).First();
            oPresentdos.Doc_Estado = "Retenciones";
            _Db.SaveChanges();

            TempData["MensajeSuccess"] = "Enviado para Retenciones";
            return RedirectToAction("Index"); //, new { id = Fel_Unique });

        }
        public ActionResult EnviarIngresoSap(Guid Fel_Unique)
        {
            var oPresentdos = _Db.PPROV_Documento.Where(p => p.Doc_Id == Fel_Unique).First();
            oPresentdos.Doc_Estado = "Ingreso SAP";
            _Db.SaveChanges();

            TempData["MensajeSuccess"] = "Enviado para Ingreso SAP";
            return RedirectToAction("Index"); //, new { id = Fel_Unique });

        }

        public ActionResult EnviarVerificaFACT(Guid Fel_Unique)
        {
            var oPresentdos = _Db.PPROV_Documento.Where(p => p.Doc_Id == Fel_Unique).First();
            oPresentdos.Doc_Estado = "Por Autorizar";
            _Db.SaveChanges();

            TempData["MensajeSuccess"] = "Enviado para Autorizacion";
            return RedirectToAction("Index"); //, new { id = Fel_Unique });

        }

        public ActionResult AutorizarFACT(Guid Fel_Unique)
        {
            ProcesaCreaFactSAP(Fel_Unique);

            TempData["MensajeSuccess"] = "Autorizado y Generado";
            return RedirectToAction("Index"); //, new { id = Fel_Unique });
        }


        public ActionResult ObtenerXmlDocto(String Tipo, String DocEntry, string Filtro)
        {
            var _SAP_Server = ConfigurationManager.AppSettings["SapServer"];
            var _SAP_Lincese = ConfigurationManager.AppSettings["SapLicenseServer"];
            var _SAP_companydb = ConfigurationManager.AppSettings["SapCompanyDb"];
            var _SAP_DataBaseType = ConfigurationManager.AppSettings["SapDataBaseType"];
            var _SAP_dbuser = ConfigurationManager.AppSettings["SapDbUser"];
            var _SAP_dbpassword = ConfigurationManager.AppSettings["SapDbPassword"];
            var _SAP_user = ConfigurationManager.AppSettings["SapUser"];
            var _SAP_password = ConfigurationManager.AppSettings["SapPassword"];
            var _SAP_language = ConfigurationManager.AppSettings["SapLanguage"];

            var retorna = "";

            try
            {

                var txtSessID = "";
                try
                {

                    txtSessID = DiServer.Login(_SAP_Server, _SAP_companydb, _SAP_DataBaseType, _SAP_dbuser, _SAP_dbpassword,
                                                _SAP_user, _SAP_password, _SAP_language, _SAP_Lincese);
                    retorna = txtSessID;
                    if (!txtSessID.Contains("Error"))
                    {
                        if (string.IsNullOrEmpty(DocEntry))
                        {
                            var oInvoicesXML = DiServer.GetEmpySchema(txtSessID, "oPurchaseInvoices").OuterXml;
                            retorna = oInvoicesXML;
                        }
                        else
                        {
                            var oDcument = DiServer.GetByKey(txtSessID, String.Format("<Object>{0}</Object><{2}>{1}</{2}>", Tipo, DocEntry, Filtro)).OuterXml;
                            retorna = oDcument;
                        }
                        ViewBag.Xml = retorna;

                        // Logout
                        var _resultLogout = DiServer.Logout(txtSessID);

                    }
                    else
                    {
                        TempData["MensajeDanger"] = "Error " + txtSessID;
                    }
                }
                catch (Exception ex)
                {
                    TempData["MensajeDanger"] = "Error " + ex.Message;
                    var _resultLogout = DiServer.Logout(txtSessID);
                }


            }
            catch (Exception ex)
            {
                Console.WriteLine("Error " + ex.Message);
            }


            return this.Content(retorna, "text/xml");
        }

        private void ProcesaCreaFactSAP(Guid Fel_Unique)
        {
            var _SAP_Server = ConfigurationManager.AppSettings["SapServer"];
            var _SAP_Lincese = ConfigurationManager.AppSettings["SapLicenseServer"];
            var _SAP_companydb = ConfigurationManager.AppSettings["SapCompanyDb"];
            var _SAP_DataBaseType = ConfigurationManager.AppSettings["SapDataBaseType"];
            var _SAP_dbuser = ConfigurationManager.AppSettings["SapDbUser"];
            var _SAP_dbpassword = ConfigurationManager.AppSettings["SapDbPassword"];
            var _SAP_user = ConfigurationManager.AppSettings["SapUser"];
            var _SAP_password = ConfigurationManager.AppSettings["SapPassword"];
            var _SAP_language = ConfigurationManager.AppSettings["SapLanguage"];
            var _SAP_FACTSerie = ConfigurationManager.AppSettings["SapFACTSerie"];
            var _DimensionCC = ConfigurationManager.AppSettings["DimensionCC"];
            var _DimensionFieldXml = "CostingCode";
            if (_DimensionCC != "1") _DimensionFieldXml += _DimensionCC;

            var _OCoriginal = Fel_Unique;



            try
            {

                var txtSessID = "";
                try
                {
                    //var lstCuentasSAP = vServicio.List_SapCuentasTodas();

                    var oWFactura = _Db.SAP_Doc.Where(p => p.FEL_Unique == Fel_Unique).First();
                    var oWFacturaDetalle = _Db.SAP_DocDetalle.Where(p => p.FEL_Unique == Fel_Unique).ToList();
                    var tempdocinfo = _Db.PPROV_Documento.Where(p => p.Doc_Id == Fel_Unique).First();
                    //var lstDepartamento = _Db.PPROV_Departamento.Where(p => p.DepartmentId == oWFactura.DepartamentoId).ToList();

                    txtSessID = DiServer.Login(_SAP_Server, _SAP_companydb, _SAP_DataBaseType, _SAP_dbuser, _SAP_dbpassword,
                                                _SAP_user, _SAP_password, _SAP_language, _SAP_Lincese);
                    if (!txtSessID.Contains("Error"))
                    {
                        var serverPath = Server.MapPath("~");
                        var lstAdjunto = _Db.FEL_DocAdjunto.Where(p => p.FEL_Unique == oWFactura.FEL_Unique).OrderByDescending(p => p.AdjuntoFecha).ToList();
                        String noAttach = "";

                        var oInvoicesXML = DiServer.GetEmpySchema(txtSessID, "oPurchaseInvoices");
                        // definir NameSapce
                        XmlNamespaceManager nsmgr = new XmlNamespaceManager(oInvoicesXML.NameTable);
                        nsmgr.AddNamespace("a", oInvoicesXML.DocumentElement.NamespaceURI);

                        // Tipo Documento
                        oInvoicesXML.SelectSingleNode("//a:Object", nsmgr).InnerText = "oPurchaseInvoices";

                        // Encabezado
                        Console.WriteLine("Encabezado");
                        var oBpCode = oInvoicesXML.SelectNodes("//a:Series", nsmgr);
                        oBpCode.Item(0).InnerText = _SAP_FACTSerie;
                        //oInvoice.BPL_IDAssignedToInvoice = 1;  Requerido Solo multi-empresa
                        oInvoicesXML.SelectNodes("//a:NumAtCard", nsmgr).Item(0).InnerText = tempdocinfo.Doc_Serie + " " + tempdocinfo.Doc_Numero;
                        oInvoicesXML.SelectNodes("//a:CardCode", nsmgr).Item(0).InnerText = oWFactura.CardCode;
                        //oInvoicesXML.SelectNodes("//a:CardName", nsmgr).Item(0).InnerText = oWFactura.Rece_Nombre;
                        oInvoicesXML.SelectNodes("//a:DocDate", nsmgr).Item(0).InnerText = DateAndTime.Now.ToString("yyyyMMdd");
                        //oInvoicesXML.SelectNodes("//a:SalesPersonCode", nsmgr).Item(0).InnerText = oWFactura.SlpCode.Value.ToString();
                        //oInvoicesXML.SelectNodes("//a:DocDueDate", nsmgr).Item(0).InnerText = oWFactura.FechaEmision.ToString("yyyyMMdd");
                        oInvoicesXML.SelectNodes("//a:TaxDate", nsmgr).Item(0).InnerText = oWFactura.FechaEmision.ToString("yyyyMMdd");
                        if (oWFactura.Tipo_Detalle != "S")
                        {
                            oInvoicesXML.SelectNodes("//a:DocType", nsmgr).Item(0).InnerText = SAPbobsCOM.BoDocumentTypes.dDocument_Items.ToString();
                        }
                        else
                        {
                            oInvoicesXML.SelectNodes("//a:DocType", nsmgr).Item(0).InnerText = SAPbobsCOM.BoDocumentTypes.dDocument_Service.ToString();
                        }


                        oInvoicesXML.SelectNodes("//a:DocCurrency", nsmgr).Item(0).InnerText = (oWFactura.Moneda == "GTQ") ? "QTZ" : oWFactura.Moneda;

                        if (oWFactura.Comentario.Length > 250)
                        {
                            oInvoicesXML.SelectNodes("//a:Comments", nsmgr).Item(0).InnerText = oWFactura.Comentario.Substring(0, 250);
                        }
                        else
                        {
                            oInvoicesXML.SelectNodes("//a:Comments", nsmgr).Item(0).InnerText = oWFactura.Comentario;
                        }

                        if (oWFactura.Comentario.Length > 50)
                        {
                            oInvoicesXML.SelectNodes("//a:JournalMemo", nsmgr).Item(0).InnerText = oWFactura.Comentario.Substring(1, 50);
                        }
                        else
                        {
                            oInvoicesXML.SelectNodes("//a:JournalMemo", nsmgr).Item(0).InnerText = oWFactura.Comentario;
                        }







                        try
                        {
                            // User Fields
                            Console.WriteLine("UserFields");
                            oInvoicesXML.SelectNodes("//a:U_WW_SyncId", nsmgr).Item(0).InnerText = oWFactura.FEL_Unique.ToString();
                            oInvoicesXML.SelectNodes("//a:U_WW_SyncDate", nsmgr).Item(0).InnerText = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                            oInvoicesXML.SelectNodes("//a:U_WW_SyncNumero", nsmgr).Item(0).InnerText = oWFactura.Correlativo.ToString();

                            //oInvoicesXML.SelectNodes("//a:U_WW_Propietario", nsmgr).Item(0).InnerText = TraeNombreUsuario(oWFactura.CreadoPor);
                            //oInvoicesXML.SelectNodes("//a:U_WW_EnviaAuto", nsmgr).Item(0).InnerText = oWFactura.EnvioAuto.Value.ToString("yyyy-MM-dd HH:mm:ss");
                            oInvoicesXML.SelectNodes("//a:U_WW_Autoriza", nsmgr).Item(0).InnerText = oWFactura.ActualizadoPor;
                            oInvoicesXML.SelectNodes("//a:U_WW_AutorizaEl", nsmgr).Item(0).InnerText = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                            oInvoicesXML.SelectNodes("//a:U_WW_GeneradoEl", nsmgr).Item(0).InnerText = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        }
                        catch { }

                        try { oInvoicesXML.SelectNodes("//a:U_TIPO_DOCUMENTO", nsmgr).Item(0).InnerText = "ZZ"; } catch { }

                        //oInvoicesXML.SelectNodes("//a:U_FELUUID", nsmgr).Item(0).InnerText = tempdocinfo.Doc_Autorizacion;
                        //oInvoicesXML.SelectNodes("//a:U_FELSerie", nsmgr).Item(0).InnerText = tempdocinfo.Doc_Serie;
                        //oInvoicesXML.SelectNodes("//a:U_FELNumero", nsmgr).Item(0).InnerText = tempdocinfo.Doc_Numero;
                        //oInvoicesXML.SelectNodes("//a:U_FelFecha", nsmgr).Item(0).InnerText = tempdocinfo.Doc_Fecha.Value.ToString("yyyy-MM-dd HH:mm:ss");

                        try { oInvoicesXML.SelectNodes("//a:U_FacNom", nsmgr).Item(0).InnerText = oWFactura.Cardname; } catch { }
                        try { oInvoicesXML.SelectNodes("//a:U_FacSerie", nsmgr).Item(0).InnerText = tempdocinfo.Doc_Serie; } catch { }
                        try { oInvoicesXML.SelectNodes("//a:U_FacNum", nsmgr).Item(0).InnerText = tempdocinfo.Doc_Numero; } catch { }
                        try { oInvoicesXML.SelectNodes("//a:U_Facnum", nsmgr).Item(0).InnerText = tempdocinfo.Doc_Numero; } catch { }

                        try { oInvoicesXML.SelectNodes("//a:U_FacFecha", nsmgr).Item(0).InnerText = oWFactura.FechaEmision.ToString("yyyyMMdd"); } catch { }
                        try { oInvoicesXML.SelectNodes("//a:U_FacNit", nsmgr).Item(0).InnerText = oWFactura.Nit; } catch { }

                        var lRetenciones = vServicio.ObtenerDetalleRetencion(oWFactura.FEL_Unique);
                        Decimal Iva = 0m;
                        Decimal Isr = 0m;
                        foreach (var _ret in lRetenciones)
                        {
                            if (_ret.Retencion_Nombre == "IVA") Iva = _ret.Retencion_Monto;
                            if (_ret.Retencion_Nombre == "ISR") Isr = _ret.Retencion_Monto;
                        }

                        try { oInvoicesXML.SelectNodes("//a:WithholdingTaxData", nsmgr).Item(0).InnerXml = XmlRetenciones(Iva, Isr, oWFactura.Total); } catch { }


                        //var xDoctTotal = tempdocinfo.Doc_MontoNeto-Iva-Isr;
                        //oInvoicesXML.SelectNodes("//a:DocTotal", nsmgr).Item(0).InnerText = xDoctTotal.ToString();
                        var totalfactsap = oWFactura.SAP_DocDetalle.Sum(p => p.TotalLinea + p.Impuestos);

                        // la factura no pueder ser mayor que la entrega
                        if (tempdocinfo.Doc_MontoNeto > totalfactsap)
                        {
                            throw new Exception("La Factura no puede ser mayor que la entrega");
                        }

                        // el rango de diferente mayor de 10 
                        if ( ( totalfactsap - tempdocinfo.Doc_MontoNeto) > 10)
                        {
                            throw new Exception("La diferencia entre Factura y Entregas no puede ser mayor 10");
                        }

                        // Entrega mayor que Factura
                        if (tempdocinfo.Doc_MontoNeto < totalfactsap )
                        {
                            var dif = totalfactsap - tempdocinfo.Doc_MontoNeto;
                            oInvoicesXML.SelectNodes("//a:DiscountPercent", nsmgr).Item(0).InnerText = (dif * 100.00m / totalfactsap).ToString();
                        }



                        //try { oInvoicesXML.SelectNodes("//a:FederalTaxID", nsmgr).Item(0).InnerXml = "123456789012"; } catch { }
                        //oBpCode.Item(0).InnerText = "10";

                        //if (lstDepartamento.Count == 1) oInvoicesXML.SelectNodes("//a:U_WW_Departamento", nsmgr).Item(0).InnerText = lstDepartamento[0].DepartmentName;
                        Console.WriteLine("Grupo 1");

                        // detalle
                        Console.WriteLine("Mandando Detalle");
                        var _contadorLineaSap = 1;
                        // get ref to the Document_Lines
                        XmlNode oDocumentLines = oInvoicesXML.SelectSingleNode("//a:Document_Lines", nsmgr);
                        // get the first row 
                        XmlNode oFirstRow = oDocumentLines.FirstChild;

                        var lstprov = _Db.V_PPROV_Proveedor.Where(p => p.CardCode == tempdocinfo.Doc_CardCorde);


                        foreach (var _ItemFactura in oWFacturaDetalle)
                        {
                            var U_TipoA = "BB";
                            if (tempdocinfo.Doc_OC_TipoDoc == "Servicio") U_TipoA = "S";
                            if (lstprov.Count() == 1)
                            {
                                if (lstprov.First().VatGroup == "EXE") U_TipoA = "FP";
                            }

                            if (_contadorLineaSap == 1)
                            {
                                if (oWFactura.Tipo_Detalle == "S")
                                {
                                    oFirstRow.SelectSingleNode("a:ItemDescription", nsmgr).InnerXml = _ItemFactura.Descripcion;
                                    oFirstRow.SelectSingleNode("a:AccountCode", nsmgr).InnerXml = _ItemFactura.CuentaContable;
                                    oFirstRow.SelectSingleNode("a:" + _DimensionFieldXml, nsmgr).InnerXml = _ItemFactura.CentroCosto;
                                    //oFirstRow.SelectSingleNode("//a:AccountCode", nsmgr).InnerXml = lstCuentasSAP.Where(p => p.ActId == _ItemFactura.CuentaContable).First().AcctCode;
                                }
                                else
                                {
                                    oFirstRow.SelectSingleNode("a:ItemCode", nsmgr).InnerXml = _ItemFactura.CodigoProducto;
                                }
                                if (!String.IsNullOrEmpty(_ItemFactura.CentroCosto)) oFirstRow.SelectSingleNode("a:" + _DimensionFieldXml, nsmgr).InnerXml = _ItemFactura.CentroCosto;
                                oFirstRow.SelectSingleNode("a:Quantity", nsmgr).InnerXml = _ItemFactura.Cantidad.ToString();
                                oFirstRow.SelectSingleNode("a:WarehouseCode", nsmgr).InnerXml = _ItemFactura.Bodega;
                                oFirstRow.SelectSingleNode("a:PriceAfterVAT", nsmgr).InnerXml = _ItemFactura.PrecioUnitario.ToString();
                                oFirstRow.SelectSingleNode("a:TaxCode", nsmgr).InnerXml = _ItemFactura.TipoImpuesto;
                                if (lstprov.First().VatGroup == "EXE") oFirstRow.SelectSingleNode("a:TaxCode", nsmgr).InnerXml = "EXE";

                                oFirstRow.SelectSingleNode("a:BaseEntry", nsmgr).InnerXml = _ItemFactura.BaseEntry.ToString();
                                oFirstRow.SelectSingleNode("a:BaseType", nsmgr).InnerXml = _ItemFactura.BaseType.ToString();
                                oFirstRow.SelectSingleNode("a:BaseLine", nsmgr).InnerXml = _ItemFactura.BaseLine.ToString();
                                try { oFirstRow.SelectSingleNode("a:U_TipoA", nsmgr).InnerXml = U_TipoA; } catch { }

                            }
                            else
                            {

                                // copy the first row the th new one -> for getting the same structure
                                XmlNode oNewRow = oFirstRow.CloneNode(true);
                                // update the new row
                                if (oWFactura.Tipo_Detalle == "S")
                                {
                                    oNewRow.SelectSingleNode("a:ItemDescription", nsmgr).InnerXml = _ItemFactura.Descripcion;
                                    oNewRow.SelectSingleNode("a:AccountCode", nsmgr).InnerXml = _ItemFactura.CuentaContable;
                                    oNewRow.SelectSingleNode("a:" + _DimensionFieldXml, nsmgr).InnerXml = _ItemFactura.CentroCosto;

                                    //oNewRow.SelectSingleNode("//a:AccountCode", nsmgr).InnerXml = lstCuentasSAP.Where(p => p.ActId == _ItemFactura.CuentaContable).First().AcctCode;
                                }
                                else
                                {
                                    oNewRow.SelectSingleNode("a:ItemCode", nsmgr).InnerXml = _ItemFactura.CodigoProducto;
                                }
                                if (!String.IsNullOrEmpty(_ItemFactura.CentroCosto)) oNewRow.SelectSingleNode("a:" + _DimensionFieldXml, nsmgr).InnerXml = _ItemFactura.CentroCosto;

                                oNewRow.SelectSingleNode("a:Quantity", nsmgr).InnerXml = _ItemFactura.Cantidad.ToString();
                                oNewRow.SelectSingleNode("a:WarehouseCode", nsmgr).InnerXml = _ItemFactura.Bodega;
                                oNewRow.SelectSingleNode("a:PriceAfterVAT", nsmgr).InnerXml = _ItemFactura.PrecioUnitario.ToString();
                                oNewRow.SelectSingleNode("a:TaxCode", nsmgr).InnerXml = _ItemFactura.TipoImpuesto;
                                if (lstprov.First().VatGroup == "EXE") oNewRow.SelectSingleNode("a:TaxCode", nsmgr).InnerXml = "EXE";

                                oNewRow.SelectSingleNode("a:BaseEntry", nsmgr).InnerXml = _ItemFactura.BaseEntry.ToString();
                                oNewRow.SelectSingleNode("a:BaseType", nsmgr).InnerXml = _ItemFactura.BaseType.ToString();
                                oNewRow.SelectSingleNode("a:BaseLine", nsmgr).InnerXml = _ItemFactura.BaseLine.ToString();
                                try { oNewRow.SelectSingleNode("a:U_TipoA", nsmgr).InnerXml = U_TipoA; } catch { }

                                // add the new row to the DocumentLines object
                                oDocumentLines.AppendChild(oNewRow);
                            }


                            _contadorLineaSap += 1;
                        }


                        // Limpiar XML Vacios                        
                        XmlNode oCleanInvoicesXML = DiServer.RemoveEmptyNodes(oInvoicesXML);



                        // Add Quotation
                        XmlDocument oXmlReply;
                        if (vServicio.List_SapOPCHSync(oWFactura.FEL_Unique.ToString()).Count == 0)
                        {
                            oXmlReply = DiServer.AddInvoice(txtSessID, oCleanInvoicesXML.OuterXml);
                            string sRet = null;

                            // check for error
                            if (Strings.InStr(oXmlReply.InnerXml, "<env:Fault>", (Microsoft.VisualBasic.CompareMethod)(0)) > 0)
                            {
                                if (oXmlReply.InnerXml.Contains("<env:Reason>"))
                                {
                                    XmlNamespaceManager nsmgrsResp = new XmlNamespaceManager(oXmlReply.NameTable);
                                    nsmgrsResp.AddNamespace("a", oXmlReply.DocumentElement.NamespaceURI);
                                    sRet = oXmlReply.SelectSingleNode("//a:Text", nsmgrsResp).InnerXml;
                                }
                                else
                                { sRet = "Error: " + oXmlReply.InnerXml; }


                                TempData["MensajeDanger"] = "Error SAP: " + sRet;
                            }
                            else
                            {
                                // saves the Quotation key
                                String invoiceDocNum = oXmlReply.FirstChild.FirstChild.InnerText;
                                // Actualizacion de Web y Busqueda Factura
                                oWFactura.EntregaEl = DateTime.Now;
                                var FacturasEncontrada = vServicio.List_SapOPCHSync(oWFactura.FEL_Unique.ToString());
                                oWFactura.EntregaNo = FacturasEncontrada[0].DocNum;
                                oWFactura.EntregaPor = User.Identity.Name;
                                _Db.SaveChanges();
                                var oDoc = _Db.PPROV_Documento.Where(p => p.Doc_Id == oWFactura.FEL_Unique).First();
                                oDoc.Doc_Estado = "Registrada";
                                oDoc.Doc_NumeroFactProv = FacturasEncontrada[0].DocNum;
                                _Db.SaveChanges();

                                TempData["MensajeSuccess"] = "Entrega Generado en Sap";
                            }
                        }
                        else
                        {
                            // Actualizacion de Web y Busqueda Factura
                            oWFactura.EntregaEl = DateTime.Now;
                            var FacturasEncontrada = vServicio.List_SapOPCHSync(oWFactura.FEL_Unique.ToString());
                            oWFactura.EntregaNo = FacturasEncontrada[0].DocNum;
                            oWFactura.EntregaPor = User.Identity.Name;
                            _Db.SaveChanges();
                            var oDoc = _Db.PPROV_Documento.Where(p => p.Doc_Id == oWFactura.FEL_Unique).First();
                            oDoc.Doc_Estado = "Registrada";
                            oDoc.Doc_NumeroFactProv = FacturasEncontrada[0].DocNum;
                            _Db.SaveChanges();
                            if (oWFactura.SAP_DocDetalle.Where(p => p.ChkImpDetEnt == true).Count() == 1)
                            {
                                var noentrega = tempdocinfo.Doc_EM_Multiple.Split(',')[0].ToString();

                                CerrarEntrega(noentrega);
                            }


                            TempData["MensajeSuccess"] = "Entrega Corregida Generado en Sap";
                        }






                        // Logout
                        var _resultLogout = DiServer.Logout(txtSessID);

                    }
                    else
                    {
                        TempData["MensajeDanger"] = "Error " + txtSessID;
                    }
                }
                catch (Exception ex)
                {
                    TempData["MensajeDanger"] = "Error " + ex.Message;
                    var _resultLogout = DiServer.Logout(txtSessID);
                }


            }
            catch (Exception ex)
            {
                Console.WriteLine("Error " + ex.Message);
            }
        }

        public void CerrarEntrega(String EntregaDocNum)
        {
            var _SAP_Server = ConfigurationManager.AppSettings["SapServer"];
            var _SAP_Lincese = ConfigurationManager.AppSettings["SapLicenseServer"];
            var _SAP_companydb = ConfigurationManager.AppSettings["SapCompanyDb"];
            var _SAP_DataBaseType = ConfigurationManager.AppSettings["SapDataBaseType"];
            var _SAP_dbuser = ConfigurationManager.AppSettings["SapDbUser"];
            var _SAP_dbpassword = ConfigurationManager.AppSettings["SapDbPassword"];
            var _SAP_user = ConfigurationManager.AppSettings["SapUser"];
            var _SAP_password = ConfigurationManager.AppSettings["SapPassword"];
            var _SAP_language = ConfigurationManager.AppSettings["SapLanguage"];
            var _SAP_OCSerie = ConfigurationManager.AppSettings["SapENSerie"];

            Guid _OCoriginal = Guid.Empty;

            if (Session["EmpresaSelId"] == null)
            {
                Wareways.PortalProv.Servicios.ServicioSeguridad.CheckSession(User.Identity.Name);
            }

            try
            {

                var txtSessID = "";
                try
                {
                    var lstCuentasSAP = vServicio.List_SapCuentasServicio();

                    var entregaEncontrada = vServicio.List_SapOPDNByDocnum(EntregaDocNum);



                    txtSessID = DiServer.Login(_SAP_Server, _SAP_companydb, _SAP_DataBaseType, _SAP_dbuser, _SAP_dbpassword,
                                                _SAP_user, _SAP_password, _SAP_language, _SAP_Lincese);
                    if (!txtSessID.Contains("Error"))
                    {
                        String xmlString = string.Format(@"<BOM>
    <BO>
     <AdmInfo>
      <Object>oPurchaseDeliveryNotes</Object>
     </AdmInfo>
     <QueryParams>
      <DocEntry>{0}</DocEntry>
     </QueryParams>    
    </BO>
   </BOM>", entregaEncontrada[0].DocEntry);
                        XmlDocument oXmlReply;
                        oXmlReply = DiServer.CloseInvoice(txtSessID, xmlString);
                        string sRet = null;

                        // check for error
                        if (Strings.InStr(oXmlReply.InnerXml, "<env:Fault>", (Microsoft.VisualBasic.CompareMethod)(0)) > 0)
                        { // And (Not (sret.StartsWith("Error"))) Then
                            //sRet = "Error: " + oXmlReply.InnerXml;
                            //TempData["MensajeDanger"] = "Error SAP: " + sRet;
                        }
                        else
                        {
                            // saves the Quotation key
                            //String invoiceDocNum = oXmlReply.FirstChild.FirstChild.InnerText;
                            // Actualizacion de Web y Busqueda Factura
                            //oWFactura.EntregaEl = DateTime.Now;
                            //var FacturasEncontrada = vServicio.List_SapOPDNSync(oWFactura.FEL_Unique.ToString());
                            //oWFactura.EntregaNo = FacturasEncontrada[0].DocNum;
                            //oWFactura.EntregaPor = User.Identity.Name;
                            //_Db.SaveChanges();
                            //TempData["MensajeSuccess"] = "Entrega Cerrada en Sap";
                        }






                        // Logout
                        var _resultLogout = DiServer.Logout(txtSessID);

                    }
                    else
                    {
                        TempData["MensajeDanger"] = "Error " + txtSessID;
                    }
                }
                catch (Exception ex)
                {
                    TempData["MensajeDanger"] = "Error " + ex.Message;
                    var _resultLogout = DiServer.Logout(txtSessID);
                }


            }
            catch (Exception ex)
            {
                Console.WriteLine("Error " + ex.Message);
            }




        }

        public ActionResult CrearFactura(Guid Fel_Unique)
        {
            ProcesoCreacionLineasFact(Fel_Unique);

            TempData["MensajeSuccess"] = "Revision de Factura Generada con Exito";
            return RedirectToAction("Detalle", new { id = Fel_Unique });
        }

        private void ProcesoCreacionLineasFact(Guid Fel_Unique)
        {
            PortalProvEntities _dbc = new PortalProvEntities();
            if (_dbc.SAP_DocDetalle.Where(p => p.FEL_Unique == Fel_Unique).ToList().Count() == 0)
            {
                try
                {
                    var oPresentdos = _Db.PPROV_Documento.Where(p => p.Doc_Id == Fel_Unique).First();
                    SAP_Doc nFact = new SAP_Doc();

                    nFact.FEL_Unique = Fel_Unique;

                    var xLinea = -1;
                    foreach (var oEntregaId in oPresentdos.Doc_EM_Multiple.Split(','))
                    {
                        var lstOPDN = vServicio.List_EntregaProveedorOPDN(oEntregaId);
                        if (lstOPDN.Count == 1)
                        {
                            var lstPDN1 = vServicio.List_EntregaProveedorDetPDN1(lstOPDN[0].DocEntry.ToString());
                            // Llena datos de primera entrega
                            if (nFact.CardCode == null)
                            {
                                nFact.CardCode = lstOPDN[0].CardCode;
                                nFact.Cardname = lstOPDN[0].CardName;
                                nFact.Referencia = lstOPDN[0].NumAtCard;
                                nFact.TipoDoc = "Fact";
                                nFact.Nit = lstOPDN[0].U_FacNit;
                                nFact.Bodega = "Mix";
                                nFact.Comentario = lstOPDN[0].Comments;
                                nFact.Total = lstOPDN[0].VatSum + lstOPDN[0].DocTotal;
                                nFact.Moneda = lstOPDN[0].DocCur;
                                nFact.Tipo_Detalle = lstOPDN[0].DocType;
                                nFact.FechaEmision = oPresentdos.Doc_Fecha.Value;
                                nFact.CreadoEl = DateTime.Now;
                                nFact.CreadoPor = User.Identity.Name;
                                if (string.IsNullOrEmpty(nFact.Nit))
                                {
                                    nFact.Nit = "CF";
                                }


                                _Db.SAP_Doc.Add(nFact);

                            }
                            // Lenando los Detalles

                            foreach (var oitem in lstPDN1)
                            {
                                SAP_DocDetalle nFactDetalle = new SAP_DocDetalle();
                                xLinea = xLinea + 1;
                                nFactDetalle.BaseEntry = lstOPDN[0].DocEntry;
                                nFactDetalle.BaseLine = oitem.LineNum;
                                nFactDetalle.BaseType = Int32.Parse(lstOPDN[0].ObjType);

                                nFactDetalle.Cantidad = oitem.Quantity;
                                nFactDetalle.UserNameAudit = User.Identity.Name;
                                nFactDetalle.DateAudit = DateTime.Now;
                                if (!string.IsNullOrEmpty(oitem.OcrCode))
                                {
                                    nFactDetalle.CentroCosto = oitem.OcrCode;
                                }

                                if (lstOPDN[0].DocType == "S")
                                {
                                    nFactDetalle.CuentaContable = oitem.AcctCode;
                                    nFactDetalle.Descripcion = oitem.Dscription;
                                    nFactDetalle.UnidadMedida = "UND";
                                }
                                else
                                {
                                    nFactDetalle.CodigoProducto = oitem.ItemCode;
                                    nFactDetalle.Descripcion = oitem.Dscription;
                                    nFactDetalle.UnidadMedida = oitem.unitMsr;
                                    nFactDetalle.Bodega = oitem.WhsCode;
                                    if (nFactDetalle.UnidadMedida == null) { nFactDetalle.UnidadMedida = "UND"; }
                                }
                                nFactDetalle.FEL_Unique = nFact.FEL_Unique;
                                nFactDetalle.Linea = xLinea;
                                nFactDetalle.PrecioUnitario = oitem.PriceAfVAT;
                                nFactDetalle.TipoDet = lstOPDN[0].DocType;
                                nFactDetalle.TipoImpuesto = oitem.TaxCode;
                                nFactDetalle.TotalLinea = oitem.LineTotal;
                                nFactDetalle.Impuestos = oitem.VatSum;


                                _Db.SAP_DocDetalle.Add(nFactDetalle);
                                _Db.SaveChanges();



                            }
                        }



                    }
                }
                catch (Exception ex)
                {
                    TempData["MensajeDanger"] = "Error " + ex.Message;
                }
            }


        }

        private string TraeNombreUsuario(string creadoPor)
        {
            try
            {
                return _Db.AspNetUsers.Where(p => p.UserName == creadoPor).First().Nombre;
            }
            catch
            {
                return creadoPor;
            }

        }
        private String XmlRetenciones(Decimal Iva, Decimal Isr, Decimal TotalDoc)
        {
            String retorna = "";

            String CodIva = ConfigurationManager.AppSettings["RetImpIva"];
            if (String.IsNullOrEmpty(CodIva)) CodIva = "RIVA";
            String CodIsr5 = ConfigurationManager.AppSettings["RetImpIsr5"];
            if (String.IsNullOrEmpty(CodIsr5)) CodIsr5 = "ISR5";
            String CodIsr7 = ConfigurationManager.AppSettings["RetImpIsr7"];
            if (String.IsNullOrEmpty(CodIsr7)) CodIsr7 = "ISR7";


            //if (Iva > 0) retorna += FilaRetencionRenerar("RIVA", 594, (TotalDoc /1.12m)*0.15m  );
            if (Iva > 0) retorna += FilaRetencionRenerar(CodIva, Iva, 0);
            if (Isr > 0 && Isr > 1500)
            {
                retorna += FilaRetencionRenerar(CodIsr5, 1500, 0);
                retorna += FilaRetencionRenerar(CodIsr7, Isr - 1500, 0);
            }
            if (Isr > 0 && Isr <= 1500)
            {
                retorna += FilaRetencionRenerar(CodIsr5, Isr, 0);
            }



            return retorna;
        }

        private String FilaRetencionRenerar(String Codigo, Decimal MontoRetencion, Decimal TotalImpuesto)
        {
            String xmlBase = @"	
                <row><WTCode>**Codigo**</WTCode><WTAmount>**MontoRetencion**</WTAmount><TaxableAmount>**TotalImpuesto**</TaxableAmount></row>
";
            xmlBase = xmlBase.Replace("**Codigo**", Codigo);
            xmlBase = xmlBase.Replace("**MontoRetencion**", MontoRetencion.ToString());
            xmlBase = xmlBase.Replace("**TotalImpuesto**", TotalImpuesto.ToString());

            return xmlBase;
        }

    }



}