using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security.Provider;
using Microsoft.TeamFoundation.Server;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Wareways.PortalProv.Infraestructura;
using Wareways.PortalProv.Models.PP;

namespace Wareways.PortalProv.Controllers
{
    public class PresentadosController : Controller
    {
        PortalProvEntities _Db = new PortalProvEntities();
        Servicios.VServicio vServicio = new Servicios.VServicio();


        [Authorize]
        // GET: Presentados
        public ActionResult Index()
        {


            Int32 empresaSelId = Int32.Parse(ConfigurationManager.AppSettings["WWPortal_EmpresaId"]);

            var model = new Models.PP.PresentadosModel();
            model.L_Documentos = ObtenerDocumentosPorUsuario(empresaSelId);
            return View(model);
        }

        [Authorize]
        [HttpPost]
        public ActionResult Index(PresentadosController model)
        {

            return View();
        }
        [Authorize]
        public ActionResult Nuevo()
        {
            Models.PP.PresentadosModel modelo = new Models.PP.PresentadosModel();
            modelo.ModoActivo = "Nuevo_Paso1";
            modelo.Nuevo = new PPROV_Documento();
            modelo.Nuevo.Doc_TasaCambio = 1;

            return View(modelo);
        }


        [Authorize]
        [HttpPost]
        public ActionResult Nuevo(Models.PP.PresentadosModel modelo, HttpPostedFileBase filefac, HttpPostedFileBase fileoc, FormCollection collection, string CargarCotizacion, HttpPostedFileBase filecotiza)
        {
            // Cargar Catalogos
            ViewBag.Usuario_Empresas = _Db.SP_PPROV_PermisosCodigosProv_Usuario(User.Identity.Name).Select(p => new { p.Empresa_Id, p.AliasName }).Distinct().ToList();
            modelo.Usuario_Moneda = _Db.GEN_CatalogoDetalle.Where(p => p.Catalogo_Id == (int)Servicios.TipoCatalogo.Moneda).OrderBy(p => p.Orden).ToList();



            //// Seccion Carga Cotizacion Proveedor
            //if (!String.IsNullOrEmpty(CargarCotizacion))
            //{
            //    try
            //    {
            //        if (filecotiza.ContentLength > 0)
            //        {
            //            if (modelo.Nuevo_Pdf_Cotizacion == null) modelo.Nuevo_Pdf_Cotizacion = string.Format("Cot_{0}.pdf", Guid.NewGuid().ToString());
            //            // Upload Files to Server
            //            var _ServerPath = Server.MapPath(@"~/Cargados/" + modelo.Nuevo_CardCode + "/");
            //            if (!System.IO.Directory.Exists(_ServerPath)) Directory.CreateDirectory(_ServerPath);

            //            String _Cotiza_Path = System.IO.Path.Combine(_ServerPath, modelo.Nuevo_Pdf_Cotizacion);
            //            filecotiza.SaveAs(_Cotiza_Path);

            //            TempData["MensajeSuccess"] = "Cotización Cargada con Exito";
            //            ModelState.Clear();
            //            return View(modelo);
            //        }
            //        else
            //        {
            //            modelo.Nuevo_Pdf_Cotizacion = null;

            //            TempData["MensajeDanger"] = "No Existe o error con el archivo Cargado";
            //            ModelState.Clear();
            //            return View(modelo);
            //        }

            //    }
            //    catch 
            //    {
            //        modelo.Nuevo_Pdf_Cotizacion = null;

            //        TempData["MensajeDanger"] = "No Existe o error con el archivo Cargado";
            //        ModelState.Clear();
            //        return View(modelo);
            //    }


            //}


            if (modelo.Nuevo_Pdf_Facturas == null) modelo.Nuevo_Pdf_Facturas = string.Format("FAC_{0}.pdf", Guid.NewGuid().ToString());
            if (modelo.Nuevo_Pdf_OC == null) modelo.Nuevo_Pdf_OC = string.Format("OC_{0}.pdf", Guid.NewGuid().ToString());
            if (modelo.Nuevo == null) modelo.Nuevo = new PPROV_Documento() { Doc_EmpresaId = 1 };

            if (modelo.ModoActivo == "Nuevo_Paso2")
            {
                if (modelo.ExcedenteMaximo == 0)
                {
                    var _lstMontoExedeFatura = _Db.GEN_Configuracion.Where(p => p.ConfigNombre == "MontoExedeFactura").ToList();
                    if (_lstMontoExedeFatura.Count == 1) modelo.ExcedenteMaximo = decimal.Parse(_lstMontoExedeFatura[0].ConfigValor);

                }

                string _ErrorValidaDatos = ValidarDatos(modelo);
                if (_ErrorValidaDatos == "")
                {
                    modelo.Nuevo.Doc_CardCorde = modelo.Nuevo_CardCode;

                    modelo.Nuevo.Doc_OC_Multiple = "";
                    if (modelo.MultiplesOrdenes.Contains(",")) modelo.Nuevo.Doc_OC_Multiple = modelo.MultiplesOrdenes.TrimEnd(',');
                    if (modelo.Nuevo.Doc_EM_Multiple.Contains(",")) modelo.Nuevo.Doc_EM_Multiple = modelo.Nuevo.Doc_EM_Multiple.TrimEnd(',');


                    modelo.Nuevo_CardName = modelo.Nuevo_CardName;
                    modelo.Nuevo.Doc_Estado = "Enviado";
                    modelo.Nuevo.Doc_FechaCarga = DateTime.Now;
                    modelo.Nuevo.Doc_Id = Guid.NewGuid();
                    modelo.Nuevo.Doc_TipoDocumento = "FACT";
                    modelo.Nuevo.Doc_UsuarioCarga = User.Identity.Name;
                    modelo.Nuevo.Doc_PdfFactura = String.Format("/cargados/{0}/{1}", modelo.Nuevo_CardCode, modelo.Nuevo_Pdf_Facturas);
                    modelo.Nuevo.Doc_PdfOC = String.Format("/cargados/{0}/{1}", modelo.Nuevo_CardCode, modelo.Nuevo_Pdf_OC);
                    try
                    {
                        modelo.Nuevo.Doc_PdfCotiza = (modelo.Nuevo_Pdf_Cotizacion.Contains(@"\\")) ? modelo.Nuevo_Pdf_Cotizacion : String.Format("/cargados/{0}/{1}", modelo.Nuevo_CardCode, modelo.Nuevo_Pdf_Cotizacion);
                    }
                    catch { }
                    modelo.Nuevo.Doc_Serie = modelo.Nuevo.Doc_Serie.TrimStart().TrimEnd();
                    modelo.Nuevo.Doc_Numero = modelo.Nuevo.Doc_Numero.TrimStart().TrimEnd();

                    try
                    {
                        var numoc = modelo.Nuevo.Doc_NumeroOC;
                        var lstoc = _Db.FEL_Doc.Where(p => p.Correlativo == numoc).ToList();
                        foreach (var item in lstoc)
                        {
                            modelo.Nuevo.Doc_OC_TipoDoc = item.Tipo_Detalle;
                            modelo.Nuevo.Doc_ActivoFijo = item.ActivoFijo;
                        }
                        if (lstoc.Count == 0)
                        {                            
                            modelo.Nuevo.Doc_OC_TipoDoc = "Externo";
                            modelo.Nuevo.Doc_ActivoFijo = false;

                        }
                    }
                    catch { }

                  


                    _Db.PPROV_Documento.Add(modelo.Nuevo);
                    _Db.SaveChanges();


                    return RedirectToAction("Index");
                }
                else
                {
                    if (modelo.DetalleEntregas.Count == 0)
                    {
                        if (string.IsNullOrEmpty(modelo.MultiplesOrdenes)) modelo.MultiplesOrdenes = modelo.Nuevo.Doc_NumeroOC.ToString();
                        var _DetalleEntrega = vServicio.Get_EntregasCOM_SAP_ByCardCode(modelo.Nuevo_CardCode, "2000-01-01", DateTime.Now.ToString("yyyy-MM-dd"));
                        var _multiplesEntregas = "";
                        foreach (var _Item in _DetalleEntrega)
                        {
                            _Item.Entrega_Usuario = "0";
                            if (modelo.Nuevo.Doc_EM_Multiple.Split(',').ToArray().Contains(_Item.Entrega_DocNum.ToString()))
                            {
                                _Item.Entrega_Usuario = "1";
                                _multiplesEntregas = _multiplesEntregas + _Item.Entrega_DocNum + ",";
                            }

                        }
                        modelo.DetalleEntregas = _DetalleEntrega;
                        try { modelo.Nuevo.Doc_EM_MontoSum = _DetalleEntrega.Where(p => p.Entrega_Usuario == "1").Sum(p => p.Entrega_DocTotal); } catch { }
                        modelo.Nuevo.Doc_EM_Multiple = _multiplesEntregas;

                    }

                    TempData["MensajeDanger"] = _ErrorValidaDatos;
                    ModelState.Clear();
                    return View(modelo);
                }


            }


            if (modelo.ModoActivo == "Nuevo_Paso1")
            {
                try
                {
                    // valida de Haya Seleccionado Adjunto
                    if (filefac == null || fileoc == null)
                    {
                        TempData["MensajeDanger"] = "Para poder registrar su factura debe de Ingresar los 2 documentos";
                        ModelState.Clear();
                        return View(modelo);
                    }


                    if (filefac.ContentLength > 0 && fileoc.ContentLength > 0)
                    {
                        // Validar que sea el mismo archivo
                        if (filefac.FileName == fileoc.FileName)
                        {
                            TempData["MensajeDanger"] = "No se Puede Carga el Documento 2 veces..";
                            ModelState.Clear();
                            return View(modelo);
                        }
                        if (!System.IO.Path.GetExtension(filefac.FileName.ToLower()).Contains("pdf") ||
                           !System.IO.Path.GetExtension(fileoc.FileName.ToLower()).Contains("pdf")
                            )
                        {
                            TempData["MensajeDanger"] = "Solo se pueden cargar archivos PDF";
                            ModelState.Clear();
                            return View(modelo);
                        }



                        // Tratar Obtener Numero de Orden Compra
                        Boolean _EntregaRequerida = true;
                        String _NumeroOc = ObtenerOCfromDPF(ParsePdf(fileoc), ref _EntregaRequerida);
                        // Obtener Datos de OC
                        if (_NumeroOc == "")
                        {
                            TempData["MensajeDanger"] = "Orden de Compra no Detectado, debe de subir el archivo PDF Original Enviado Por Correo, no Escaneado";
                            ModelState.Clear();
                            return View(modelo);
                        }

                        else // Si Logro Detectar la Orden
                        {
                            List<SP_EntregasCOM_SAP_Result> _InfoOrden = new List<SP_EntregasCOM_SAP_Result>();
                            var _DatosOrden = vServicio.Get_DatosOrdenCompra(_NumeroOc);
                            if (_DatosOrden.Count > 0)
                            {
                                if (_NumeroOc.Contains("-") && _NumeroOc.Count() > 0) _NumeroOc = _DatosOrden.First().DocNum.ToString();
                                _InfoOrden = vServicio.Get_EntregasCOM_SAP(_NumeroOc, "2000-01-01", DateTime.Now.ToString("yyyy-MM-dd")).ToList();
                            }


                            if (_InfoOrden.Count > 0) modelo.Nuevo_Pdf_Informe = _InfoOrden[0].Entrega_AdjuntosUrl;
                            if (_DatosOrden.Count == 1)
                            {
                                var _InfoEntrega = vServicio.Get_EntregasCOM_SAP(_NumeroOc, "2000-01-01", DateTime.Now.ToString("yyyy-MM-dd")).ToList();
                                var _DetalleEntrega = vServicio.Get_EntregasCOM_SAP_ByCardCode(_InfoOrden[0].Orden_CardCode, "2000-01-01", DateTime.Now.ToString("yyyy-MM-dd"));
                                _DetalleEntrega = _DetalleEntrega.Where(p => p.Orden_DocType == _InfoEntrega[0].Orden_DocType).ToList();
                                string _multiplesEntregas = "";
                                decimal _montoEntrega = 0;
                                var TieneEntregaActiva = false;
                                foreach (var _Item in _DetalleEntrega)
                                {
                                    _Item.Entrega_Usuario = "0";
                                    if (_Item.Entrega_DocNum == _InfoOrden[0].Entrega_DocNum)
                                    {
                                        _Item.Entrega_Usuario = "1";
                                        _multiplesEntregas = _multiplesEntregas + _Item.Entrega_DocNum + ",";
                                        _montoEntrega += (Decimal)_Item.Entrega_DocTotal;
                                        TieneEntregaActiva = true;
                                    }
                                }
                                foreach (var _Item in _DetalleEntrega)
                                {
                                    _Item.Entrega_Usuario = "0";
                                    if (_Item.Orden_DocNum.ToString() == _NumeroOc && _montoEntrega == 0)
                                    {
                                        _Item.Entrega_Usuario = "1";
                                        _multiplesEntregas = _multiplesEntregas + _Item.Entrega_DocNum + ",";
                                        _montoEntrega += (Decimal)_Item.Entrega_DocTotal;

                                    }

                                }

                                if (TieneEntregaActiva == false)
                                {
                                    TempData["MensajeDanger"] = "No existe Entrega Abierta, para esta OC";
                                    ModelState.Clear();
                                    return View(modelo);
                                }

                                modelo.DetalleEntregas = _DetalleEntrega;
                                modelo.Nuevo.Doc_EM_Multiple = _multiplesEntregas;
                                modelo.Nuevo.Doc_EM_MontoSum = _montoEntrega;


                                var _CardCodeAutorizados = Obtener_CardCode_AutorizadasPorUsuario();
                                if (_CardCodeAutorizados.Contains(_DatosOrden[0].CardCode))
                                {
                                    modelo.Nuevo_CardCode = _DatosOrden[0].CardCode;
                                    modelo.Nuevo_CardName = _DatosOrden[0].CardName;
                                    modelo.Nuevo.Doc_EmpresaId = _DatosOrden[0].BPLId;
                                    modelo.Nuevo.Doc_MontoNeto = _montoEntrega;
                                    modelo.Nuevo.Doc_NumeroOC = _DatosOrden[0].DocNum;
                                    modelo.MultiplesOrdenes = _DatosOrden[0].DocNum.ToString() + ',';
                                    modelo.Nuevo.SolicitanteOC = _DatosOrden[0].UsuarioSolicitante;
                                    modelo.Nuevo.Doc_Moneda = _DatosOrden[0].DocCur;
                                    modelo.OrdenAdjunto = _InfoOrden[0].Orden_Adjuntos;
                                    modelo.OrdenAdjuntoUrl = _InfoOrden[0].Orden_AdjuntosUrl;
                                    modelo.EntregaAdjunto = _InfoOrden[0].Entrega_Adjuntos;
                                    modelo.EntregaAdjuntoUrl = _InfoOrden[0].Entrega_AdjuntosUrl;
                                }
                                else
                                {
                                    TempData["MensajeDanger"] = "Codigo de Proveedor " + _DatosOrden[0].CardCode + "en Orden de Compra no pertenece a su usuario, Revise el Archivo y suba la Orden Correcta, o solicite los Permisos";
                                    ModelState.Clear();
                                    return View(modelo);
                                }

                                // Validar Fecha Maxima pago
                                var _UserId = System.Web.HttpContext.Current.User.Identity.GetUserId();
                                var _CardCode = _DatosOrden[0].CardCode;


                                if (_InfoOrden[0].Orden_DocType == "S")
                                {
                                    var _FechasMax = _Db.SP_PPROV_DeteccionFecha_PresentacionMaxServ(Guid.Parse(_UserId)).Where(p => p.CardCode == _CardCode && p.Dia_Maximo > 0).ToList();
                                    if (_FechasMax.Count() > 0)
                                    {
                                        if (DateTime.Now.Day > _FechasMax[0].Dia_Maximo)
                                        {
                                            TempData["MensajeDanger"] = "No se pueden presentar documentos despues de la Fecha maxima " + _FechasMax[0].Dia_Maximo.ToString() + " de " + _FechasMax[0].MesActual + " para el proveedor " + _DatosOrden[0].CardCode + " - " + _DatosOrden[0].CardName + "";
                                            ModelState.Clear();
                                            return View(modelo);
                                        }

                                    }
                                }
                                else
                                {
                                    var _FechasMax = _Db.SP_PPROV_DeteccionFecha_PresentacionMax(Guid.Parse(_UserId)).Where(p => p.CardCode == _CardCode && p.Dia_Maximo > 0).ToList();
                                    if (_FechasMax.Count() > 0)
                                    {
                                        if (DateTime.Now.Day > _FechasMax[0].Dia_Maximo)
                                        {
                                            TempData["MensajeDanger"] = "No se pueden presentar documentos despues de la Fecha maxima " + _FechasMax[0].Dia_Maximo.ToString() + " de " + _FechasMax[0].MesActual + " para el proveedor " + _DatosOrden[0].CardCode + " - " + _DatosOrden[0].CardName + "";
                                            ModelState.Clear();
                                            return View(modelo);
                                        }

                                    }
                                }

                                


                                if (modelo.EntregaAdjunto == null || modelo.EntregaAdjunto == 0 || Int32.Parse(_NumeroOc) == 3017496)
                                {
                                    if (_EntregaRequerida)
                                    {
                                        TempData["MensajeDanger"] = "No existe Entrega de Servicio o Producto de la Orden " + _NumeroOc + " ,Comuniquese con el departamento que realizo la compra";
                                        ModelState.Clear();
                                        return View(modelo);
                                    }

                                }



                                //if (modelo.OrdenAdjunto == null || modelo.OrdenAdjunto == 0)
                                //{
                                //    TempData["MensajeWarning"] = "No se Encontro Cotización Adjunta, Favor Adjunte su Cotización";
                                //}
                                else
                                {
                                    modelo.Nuevo_Pdf_Cotizacion = _InfoOrden[0].Orden_AdjuntosUrl;
                                }

                            }
                            else
                            {
                                TempData["MensajeWarning"] = "Numero de Orden de Compra no Encontrada o Cancelada";
                                ModelState.Clear();
                                return View(modelo);
                            }

                        }


                        // Upload Files to Server
                        var _ServerPath = Server.MapPath(@"~/Cargados/" + modelo.Nuevo_CardCode + "/");
                        if (!System.IO.Directory.Exists(_ServerPath)) Directory.CreateDirectory(_ServerPath);

                        String _Fact_Path = System.IO.Path.Combine(_ServerPath, modelo.Nuevo_Pdf_Facturas);
                        filefac.SaveAs(_Fact_Path);

                        String _OC_Path = System.IO.Path.Combine(_ServerPath, modelo.Nuevo_Pdf_OC);
                        fileoc.SaveAs(_OC_Path);
                        modelo.ModoActivo = "Nuevo_Paso2";


                    }
                    else
                    {
                        TempData["MensajeWarning"] = "Debe de Cargar la Factura y la Orden de Compra";
                    }


                }
                catch (Exception ex)
                {
                    ViewBag.Message = ex.Message;

                    ModelState.Clear();

                    return View(modelo);
                }
            }


            ModelState.Clear();
            return View(modelo);
        }

        [Authorize]
        public ActionResult ObtenerMensajes(Guid id)
        {
            var model = _Db.PPROV_Nota.Where(p => p.Doc_Id == id).OrderByDescending(p => p.Nota_Fecha).ToList();
            if (User.IsInRole("Proveedor"))
            {
                foreach (var _item in model)
                {
                    _item.Revisada = true;
                    _item.Revisada_Fecha = DateTime.Now;
                    _item.Revisada_Por = User.Identity.Name;
                }
                _Db.SaveChanges();
            }


            return View(model);
        }

        private string ValidarDatos(PresentadosModel modelo)
        {
            var _Retorna = "";
            //if (String.IsNullOrEmpty(  modelo.Nuevo_Pdf_Cotizacion) )
            //{
            //    _Retorna = "No se Encontro Cotización Adjunta, Favor Adjunte su Cotización";
            //}
            if (String.IsNullOrEmpty(modelo.Nuevo.Doc_Autorizacion))
            {
                _Retorna = "Debe de Llevar informacion de la Autorizacion de la Factura";
            }
            if (String.IsNullOrEmpty(modelo.Nuevo.Doc_Serie))
            {
                _Retorna = "Debe de Llevar informacion de la Serie de la Factura";
            }
            if (String.IsNullOrEmpty(modelo.Nuevo.Doc_Numero))
            {
                _Retorna = "Debe de Llevar informacion de la Numero de la Factura";
            }
            if (String.IsNullOrEmpty(modelo.Nuevo.Doc_Fecha.ToString()))
            {
                _Retorna = "Debe de Llevar informacion de la Fecha de la Factura";
            }
            if (modelo.Nuevo.Doc_Moneda != "QTZ" && modelo.Nuevo.Doc_TasaCambio < 1)
            {
                _Retorna = "Debe de Ingrear la tasa de Cambio para factura en Dolares";
            }

            if ((modelo.Nuevo.Doc_EM_MontoSum - modelo.Nuevo.Doc_MontoNeto) > modelo.ExcedenteMaximo)
            {
                _Retorna = String.Format("El excedente no debe ser mayor de {0}", modelo.ExcedenteMaximo);
            }
            if (modelo.Nuevo.Doc_EM_MontoSum < modelo.Nuevo.Doc_MontoNeto)
            {
                _Retorna = String.Format("La factura no puede ser mayor que la entrega", modelo.ExcedenteMaximo);
            }
            
            if (modelo.Nuevo.Doc_Fecha.Value.Month != DateTime.Now.Month)
            {
                _Retorna = String.Format("Solo de pueden presentar facturas del mes actual");
            }


            // Verificar si la Factura Ya esta presentada
            var DectetaDusplicados = _Db.PPROV_Documento.Where(p => p.Doc_Serie == modelo.Nuevo.Doc_Serie &&
                                                               p.Doc_Numero == modelo.Nuevo.Doc_Numero &&
                                                               p.Doc_Estado != "Rechazado").ToList();
            if (DectetaDusplicados.Count() > 0)
            {
                _Retorna = String.Format("Ya existe un documento registrado en el Proceso {0}, Verifique que no este subiendo documentos duplicados", DectetaDusplicados[0].Doc_Estado  );
            }


            return _Retorna;
        }

        private List<WWSP_DocumentosPorUsuario_Result> ObtenerDocumentosPorUsuario(Int32 empresaId)
        {
            _Db.Database.CommandTimeout = 300;
            var _UserName = User.Identity.Name;
            var retencionMin = DateTime.Now.Date.AddMonths(-6);
            var _Datos = _Db.WWSP_DocumentosPorUsuario(empresaId, _UserName).Where(p => p.Doc_Fecha >= retencionMin).ToList();

            return _Datos;
        }


        private string Obtener_CardCode_Usuario_Primero()
        {
            _Db.Database.CommandTimeout = 300;
            var _Datos = _Db.v_PPROV_Usuario_Proveedor.AsNoTracking().Where(p => p.UserName == User.Identity.Name).ToList();
            return _Datos[0].CardCode;
        }
        private string[] Obtener_CardCode_AutorizadasPorUsuario()
        {
            _Db.Database.CommandTimeout = 300;
            return _Db.v_PPROV_Usuario_Proveedor.Where(p => p.UserName == User.Identity.Name).Select(p => p.CardCode).ToArray();
        }


        public string ParsePdf(string fileName)
        {

            using (PdfReader reader = new PdfReader(fileName))
            {
                StringBuilder sb = new StringBuilder();

                ITextExtractionStrategy strategy = new SimpleTextExtractionStrategy();
                for (int page = 0; page < reader.NumberOfPages; page++)
                {
                    string text = PdfTextExtractor.GetTextFromPage(reader, page + 1, strategy);
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        sb.Append(Encoding.UTF8.GetString(ASCIIEncoding.Convert(Encoding.Default, Encoding.UTF8, Encoding.Default.GetBytes(text))));
                    }
                }

                return sb.ToString();
            }
        }


        private string ObtenerOCfromDPF(string v, ref Boolean _EntregaRequerida)
        {
            String _NumeroOC = "";

            try
            {
                var _UserName = User.Identity.Name;
                var oUsuario = _Db.AspNetUsers.Where(p => p.UserName == _UserName).First();
                var arrEmppresaPermiso = _Db.v_PPROV_Usuario_Proveedor.Where(p => p.Id == oUsuario.Id).Select(p => p.Empresa_Id).Distinct().ToArray();
                // Empresa Seleccion / Deteccion
                var empresaSelId = 0;
                empresaSelId = Int32.Parse(ConfigurationManager.AppSettings["WWPortal_EmpresaId"]);
                var lstEmpresa = _Db.V_PPROV_Empresas.Where(p => arrEmppresaPermiso.Contains(p.Empresa_Id)).Where(p => p.Empresa_Id == 1).ToList();
                V_PPROV_Empresas oEnpresaSal = null;
                var SplitContenido = v.Split('\n');
                foreach (var item in lstEmpresa)
                { if (SplitContenido[2] == "NIT " + item.Nit) oEnpresaSal = item; }
                if (oEnpresaSal != null)
                {
                    _EntregaRequerida = oEnpresaSal.EntregaRequerida;
                    if (SplitContenido[0] == "ORDEN DE COMPRA " &&
                        SplitContenido[3].Contains("PBX: (+502)") &&
                        ( SplitContenido[5].ToUpper() == oEnpresaSal.EmpresaOCCheck.ToUpper() || SplitContenido[4].ToUpper() == oEnpresaSal.EmpresaOCCheck.ToUpper() )
                        )
                    {
                        return SplitContenido[1];
                    }
                }
                else
                {
                    if (lstEmpresa.Count() > 0) _EntregaRequerida = lstEmpresa.First().EntregaRequerida;
                    if (SplitContenido[0] == "ORDEN DE COMPRA" &&
                        SplitContenido[1].StartsWith("No.") &&
                        SplitContenido[1].Contains("-") &&
                        SplitContenido[3].StartsWith("Pagina ") &&
                        (
                            SplitContenido[5].StartsWith("Proveedor:") || SplitContenido[4].Contains("Proveedor:") 
                        ))
                    {
                        return SplitContenido[1].Split(' ')[1].Trim();
                    }
                }

            }
            catch (Exception ex)
            {

            }

            return _NumeroOC;
        }
        public string ParsePdf(HttpPostedFileBase _UB_Ordenes)
        {
            byte[] pdfbytes = null;
            BinaryReader rdr = new BinaryReader(_UB_Ordenes.InputStream);
            pdfbytes = rdr.ReadBytes((int)_UB_Ordenes.ContentLength);

            using (PdfReader reader = new PdfReader(pdfbytes))
            {
                StringBuilder sb = new StringBuilder();

                ITextExtractionStrategy strategy = new SimpleTextExtractionStrategy();
                for (int page = 0; page < reader.NumberOfPages; page++)
                {
                    string text = PdfTextExtractor.GetTextFromPage(reader, page + 1, strategy);
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        sb.Append(Encoding.UTF8.GetString(ASCIIEncoding.Convert(Encoding.Default, Encoding.UTF8, Encoding.Default.GetBytes(text))));
                    }
                }

                return sb.ToString();
            }
        }

    }
}