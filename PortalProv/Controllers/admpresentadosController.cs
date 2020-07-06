using iText.Forms.Xfdf;
using System;
using System.Collections.Generic;
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
            _Estados.Add(new PPROV_Estado { Doc_Estado = "Rechazado", Doc_Orden = 3 });
            _Estados.Add(new PPROV_Estado { Doc_Estado = "Revision", Doc_Orden = 2 });

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
            PPROV_Nota _NuevaNota = new PPROV_Nota();
            _NuevaNota.Doc_Estado = Nuevo_Estado_Seleccionado;
            _NuevaNota.Doc_Id = _Documento.Doc_Id;
            _NuevaNota.Nota_Descripción = Nuevo_Estado_Comentario;
            _NuevaNota.Nota_Fecha = DateTime.Now;
            _NuevaNota.Nota_Id = Guid.NewGuid();
            _NuevaNota.Nota_Usuario = User.Identity.Name;
            _Db.PPROV_Nota.Add(_NuevaNota);
            _Db.SaveChanges();

            return RedirectToAction("index");
        }

        private List<V_PPROV_Documentos_Oficina> ObtenerDocumentosPorUsuario()
        {
            var _UserName = User.Identity.Name;
            var _Datos = _Db.V_PPROV_Documentos_Oficina.AsNoTracking().ToList();

            return _Datos;
        }
    }
}