using iText.Forms.Xfdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Wareways.PortalProv.Infraestructura;

namespace Wareways.PortalProv.Controllers
{
    public class admpresentadosController : Controller
    {
        PortalProvEntities _Db = new PortalProvEntities();

        [Authorize(Roles = "Oficina")]
        public ActionResult Index(string FiltroEstado)
        {
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
            _Estados.Add(new PPROV_Estado { Doc_Estado = "Rechazado", Doc_Orden = 5 });
            _Estados.Add(new PPROV_Estado { Doc_Estado = "Contraseña", Doc_Orden = 2 });
            _Estados.Add(new PPROV_Estado { Doc_Estado = "Retenciones", Doc_Orden = 3 });
            _Estados.Add(new PPROV_Estado { Doc_Estado = "Ingreso SAP", Doc_Orden = 4 });

            var model = new Models.PP.PresentadosModelOficina();
            model.L_Estados = _Db.V_PPROV_ResumenEstadoDocumento.OrderBy(p => p.Doc_Orden).ToList();
            model.L_Documentos = ObtenerDocumentosPorUsuario();
            model.Nuevo_Estados_Asignar = new SelectList(_Estados.OrderBy(p => p.Doc_Orden), "Doc_Estado", "Doc_Estado", _FiltroActivo);
            model.Nuevo_Estado_Seleccionado = _FiltroActivo;
            model.Nuevo_Estado_Comentario = "";


            // filtro
            if (!string.IsNullOrEmpty(_FiltroActivo))
            {
                model.L_Documentos = model.L_Documentos.Where(p => p.Doc_Estado == _FiltroActivo).ToList();

            }

            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Oficina")]
        public ActionResult CambioEstado(Guid CambioEstado_Documento_Id,string Nuevo_Estado_Seleccionado, string Nuevo_Estado_Comentario)
        {
            var _Documento = _Db.PPROV_Documento.Find(CambioEstado_Documento_Id);
            if ( _Documento.Doc_Estado != Nuevo_Estado_Seleccionado)
            {
                _Documento.Doc_Estado = Nuevo_Estado_Seleccionado;
                _Db.SaveChanges();
            }

            // No crea Nota si no tiene comentario
            if ( Nuevo_Estado_Comentario != "")
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
            }
            


            return RedirectToAction("index");
        }

        private List<V_PPROV_Documentos_Oficina> ObtenerDocumentosPorUsuario()
        {
            var _UserName = User.Identity.Name;
            var _Datos = _Db.V_PPROV_Documentos_Oficina.AsNoTracking().Where(p=>p.UserName == _UserName).ToList();

            return _Datos;
        }

        public ActionResult TestRechazo(Guid id)
        {
            MandarCorreo_Rechazo(id);

            return RedirectToAction("index");
        }

        private void MandarCorreo_Rechazo(Guid id)
        {
            Infraestructura.FlexManDbEntities _Db_Flex = new FlexManDbEntities();
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

            _Db_Flex.NotificacionesCola.Add(new NotificacionesCola
            {
                Para = "jherrera@wareways.com",
                Copia = "julioherreraguate@gmail.com",
                Asunto = "Documento Rezadado",
                Cuerpo = body,
                EnviadoFecha = null,
                EnviarHasta = DateTime.Now,
                ProximoEnvio = null,
                IntervaloMinutos = 0,
                Sistema = "SS_Proveedores",
                Parametro1 = "RechazoDoc",
                Parametro2 = id.ToString(),
                Id = Guid.NewGuid()

            });
            _Db_Flex.SaveChanges();


        }
    }


}