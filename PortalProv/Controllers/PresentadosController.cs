using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using Microsoft.Owin.Security.Provider;
using Microsoft.TeamFoundation.Server;
using System;
using System.Collections.Generic;
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

        [Authorize]
        // GET: Presentados
        public ActionResult Index()
        {
            var model = new Models.PP.PresentadosModel();            
            model.L_Documentos = ObtenerDocumentosPorUsuario();
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


            return View(modelo);
        }


        [Authorize]
        [HttpPost]
        public ActionResult Nuevo( Models.PP.PresentadosModel modelo, HttpPostedFileBase filefac, HttpPostedFileBase fileoc, FormCollection collection)
        {

            // Cargar Catalogos
            ViewBag.Usuario_Empresas = _Db.SP_PPROV_PermisosCodigosProv_Usuario(User.Identity.Name).Select(p => new { p.Empresa_Id, p.AliasName }).Distinct().ToList();
            


            modelo.Usuario_Moneda = _Db.GEN_CatalogoDetalle.Where(p => p.Catalogo_Id == (int)Servicios.TipoCatalogo.Moneda).OrderBy(p => p.Orden).ToList();

            
            if (modelo.Nuevo_Pdf_Facturas == null) modelo.Nuevo_Pdf_Facturas = string.Format("FAC_{0}.pdf", Guid.NewGuid().ToString());
            if( modelo.Nuevo_Pdf_OC == null)   modelo.Nuevo_Pdf_OC = string.Format("OC_{0}.pdf", Guid.NewGuid().ToString());
            if (modelo.Nuevo == null)  modelo.Nuevo = new PPROV_Documento() {  Doc_EmpresaId = 1};


            if (modelo.ModoActivo == "Nuevo_Paso2")
            {
                string _ErrorValidaDatos = ValidarDatos(modelo);
                if (_ErrorValidaDatos == "")
                {
                    modelo.Nuevo.Doc_CardCorde = modelo.Nuevo_CardCode;
                    modelo.Nuevo.Doc_Estado = "Enviado";
                    modelo.Nuevo.Doc_FechaCarga = DateTime.Now;
                    modelo.Nuevo.Doc_Id = Guid.NewGuid();
                    modelo.Nuevo.Doc_TipoDocumento = "FACT";
                    modelo.Nuevo.Doc_UsuarioCarga = User.Identity.Name;
                    modelo.Nuevo.Doc_PdfFactura = String.Format("/cargados/{0}/{1}", modelo.Nuevo_CardCode, modelo.Nuevo_Pdf_Facturas);
                    modelo.Nuevo.Doc_PdfOC = String.Format("/cargados/{0}/{1}", modelo.Nuevo_CardCode, modelo.Nuevo_Pdf_OC); ;

                    _Db.PPROV_Documento.Add(modelo.Nuevo);
                    _Db.SaveChanges();


                    return RedirectToAction("Index");
                }
                else
                {
                    ViewBag.Message_Paso2 = _ErrorValidaDatos;
                }


            }


            if (modelo.ModoActivo == "Nuevo_Paso1")
            {
                try
                {
                 
                    if (filefac.ContentLength > 0 && fileoc.ContentLength > 0)
                    {
                        // Validar que sea el mismo archivo
                        if (filefac.FileName == fileoc.FileName)
                        {
                            ViewBag.Message = "No puede cargar el mismo documento, 2 veces en el sistema";
                            ModelState.Clear();
                            return View(modelo);
                        }
                        



                        // Tratar Obtener Numero de Orden Compra
                        String _NumeroOc = ObtenerOCfromDPF(ParsePdf(fileoc));
                        // Obtener Datos de OC
                        if ( _NumeroOc == "")
                        {
                            modelo.Nuevo_CardCode = Obtener_CardCode_Usuario_Primero();
                        }
                        
                        else // Si Logro Detectar la Orden
                        {
                            var _DatosOrden = _Db.SP_PPROV_DatosOrdenCompra(Int32.Parse(_NumeroOc)).ToList();
                            if ( _DatosOrden.Count == 1)
                            {
                                //if ( Obtener_CardCode_AutorizadasPorUsuario().Contains(_DatosOrden[0].CardCode )   )
                                //{
                                    modelo.Nuevo_CardCode = _DatosOrden[0].CardCode;
                                    modelo.Nuevo.Doc_EmpresaId = _DatosOrden[0].BPLId;
                                    modelo.Nuevo.Doc_MontoNeto = _DatosOrden[0].DocTotal;
                                    modelo.Nuevo.Doc_NumeroOC = _DatosOrden[0].DocNum;
                                    modelo.Nuevo.SolicitanteOC = _DatosOrden[0].UsuarioSolicitante;
                                    modelo.Nuevo.Doc_Moneda = _DatosOrden[0].DocCur;
                                  
                                //} else

                                //{
                                //    ViewBag.Message = "La Orden de Compra no pertenece a su Usuario, Archivo Incorrecto";
                                //    ModelState.Clear();
                                //    return View(modelo);
                                //}

                                
                            } else
                            {
                                ViewBag.Message = "Numero de Orden de Compra no Encontrada o Cancelada";
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
                        ViewBag.Message = "Debe de Cargar la Factura y la Orden de Compra";
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
            if( User.IsInRole("Proveedor") )
            {
                foreach ( var _item in model)
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
            return "";
        }

        private List<V_PPROV_DocumentosPorUsuario> ObtenerDocumentosPorUsuario()
        {
            var _UserName = User.Identity.Name;
            var _Datos = _Db.V_PPROV_DocumentosPorUsuario.Where(p => p.UserName == _UserName).ToList();

            return _Datos;
        }


        private string Obtener_CardCode_Usuario_Primero()
        {
            var _Datos = _Db.v_PPROV_Usuario_Proveedor.AsNoTracking().Where(p => p.UserName == User.Identity.Name).ToList();
            return _Datos[0].CardCode;
        }
        private string[] Obtener_CardCode_AutorizadasPorUsuario()
        {
            return _Db.v_PPROV_Usuario_Proveedor.Where(p => p.UserName == User.Identity.Name).Select(p=>p.CardCode).ToArray();
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


        private string ObtenerOCfromDPF(string v)
        {
            String _NumeroOC = "";
            if (v.Contains("División envases Lacoplast") || v.Contains("POLIMEROS Y TECNOLOGIA, S.A.") || v.Contains("POLYTEC INTERNACIONAL, S.A."))
            {
                var SplitContenido = v.Split('\n');
                for (int i = 0; i < 4; i++)
                {
                    if (SplitContenido[i].Contains("Orden de Servicio"))
                    {
                        return SplitContenido[i].Split(' ')[3];
                    }
                    if (SplitContenido[i].Contains("ORDEN DE COMPRA No."))
                    {
                        return SplitContenido[i + 1];
                    }

                }
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