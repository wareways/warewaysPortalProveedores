using Microsoft.TeamFoundation.Server;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            modelo.Usuario_Empresas = _Db.V_PPROV_Empresas.OrderBy(p => p.Empresa_Id).ToList();
            modelo.Usuario_Moneda = _Db.GEN_CatalogoDetalle.Where(p => p.Catalogo_Id == (int)Servicios.TipoCatalogo.Moneda).OrderBy(p => p.Orden).ToList();

            modelo.Nuevo_CardCode = Obtener_CardCode_Usuario();
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
                    var _ServerPath = Server.MapPath(@"~/Cargados/" + modelo.Nuevo_CardCode + "/");
                    if (!System.IO.Directory.Exists(_ServerPath)) Directory.CreateDirectory(_ServerPath);

                    if (filefac.ContentLength > 0 && fileoc.ContentLength > 0)
                    {
                        String _Fact_Path = Path.Combine(_ServerPath, modelo.Nuevo_Pdf_Facturas);
                        filefac.SaveAs(_Fact_Path);

                        String _OC_Path = Path.Combine(_ServerPath, modelo.Nuevo_Pdf_OC);
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
                    ViewBag.Message = "Carga de Archivos Fallida";

                    ModelState.Clear();

                    return View(modelo);
                }
            }


            ModelState.Clear();
            return View(modelo);
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


        private string Obtener_CardCode_Usuario()
        {
            return _Db.v_PPROV_Usuario_Proveedor.Where(p => p.UserName == User.Identity.Name).First().CardCode;
        }


    }
}