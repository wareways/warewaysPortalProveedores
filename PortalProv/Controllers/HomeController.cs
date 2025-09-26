using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Configuration;
using System.Text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using Microsoft.AspNet.Identity;
using System.Net;
using Wareways.PortalProv.Infraestructura;

namespace Wareways.PortalProv.Controllers
{
    public class HomeController : Controller
    {
        Infraestructura.PortalProvEntities _Db = new Infraestructura.PortalProvEntities();


        public ActionResult Index(string MMP, string xcode)
        {
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ViewBag.MPP = "0";
            ViewBag.MMP_List = new List<Wareways.PortalProv.Infraestructura.SP_PPROV_DeteccionFecha_PresentacionMax_Result>(); ;
            @ViewBag.MesLetras = "";

            if (User.Identity.IsAuthenticated)
            {
               

                if (Session["MenuList"] == null)
                {
                    var _MenusPermitidos = (from l in _Db.V_GEN_MenuDisplay.AsNoTracking() where l.UserName == User.Identity.Name orderby l.Menu_Orden select l).ToList();
                    Session["MenuList"] = _MenusPermitidos;
                }
                if (Session["UserName"] == null)
                {
                    Session["UserName"] = User.Identity.Name;
                }
                // Validacion Mensaje Proveedores
                if (MMP == "1")
                {
                    var _UserId = System.Web.HttpContext.Current.User.Identity.GetUserId();

                    var _FechasMax = _Db.SP_PPROV_DeteccionFecha_PresentacionMax(Guid.Parse(_UserId)).Where(p => p.Dia_Maximo > 0).ToList();
                    var _FechasMaxServ = _Db.SP_PPROV_DeteccionFecha_PresentacionMaxServ(Guid.Parse(_UserId)).Where(p => p.Dia_Maximo > 0).ToList();

                    if (xcode != null)
                    {
                        _FechasMax = new List<Infraestructura.SP_PPROV_DeteccionFecha_PresentacionMax_Result>();
                        var Serv = _Db.SP_PPROV_DeteccionFecha_PresentacionMaxServ_PorCardCode(xcode).Where(p => p.Dia_Maximo > 0).ToList();
                        foreach (var _item in _Db.SP_PPROV_DeteccionFecha_PresentacionMax_PorCardCode(xcode).Where(p => p.Dia_Maximo > 0).ToList())
                        {
                            _FechasMax.Add(new Infraestructura.SP_PPROV_DeteccionFecha_PresentacionMax_Result
                            {
                                CardCode = _item.CardCode,
                                CardName = _item.CardName,
                                DiaMaximoFechaCompleta = _item.DiaMaximoFechaCompleta,
                                DiaMaximoSemana = _item.DiaMaximoSemana,
                                Dia_Esp = _item.Dia_Esp,
                                Dia_Global = _item.Dia_Global,
                                Dia_Maximo = _item.Dia_Maximo,
                                Dia_Sap = _item.Dia_Sap,
                                Estado = _item.Estado,
                                Mensaje = _item.Mensaje,
                                MesActual = _item.MesActual,
                                USERID = _item.UserId
                            });
                        }
                    }

                    if (_FechasMax.Count() > 0)
                    {
                        ViewBag.MPP = "1";
                        ViewBag.MMP_List = _FechasMax;
                        ViewBag.MMP_ListServ = _FechasMaxServ;
                        @ViewBag.MesLetras = _FechasMax[0].MesActual;
                    }



                }
            }


            String _Servidor = System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString.Split(';')[0].Split('=')[1];
            Session.Add("ConexionDB", (_Servidor == ".") ? "Local" : "Productivo");
            
            ViewBag.VB_Servidor = (_Servidor == ".") ? "Local" : "Productivo";

            if (User.Identity.Name != "")
            {
                //var _Estadisticas = _Db.SP_PPROV_STATS_FacturacionUltimoAnio(User.Identity.Name).ToList();
                //ViewBag.DataTotal = string.Join(",", _Estadisticas.Select(p => p.Total).ToArray());
                //ViewBag.DataCantidad = string.Join(",", _Estadisticas.Select(p => p.Cantidad).ToArray());
                //ViewBag.DataTitulo = string.Join(",", _Estadisticas.Select(p => @"'" + p.Anio.ToString() + "-" + p.mes.ToString() + @"'").ToArray());

                var _Indicadores = new List<SP_PPROV_Indicadores_Usuario_Result>(); 
                    //_Db.SP_PPROV_Indicadores_Usuario(User.Identity.Name).ToList();
                ViewBag.IndicadoresTop = _Indicadores.Where(p => p.Grupo == "HomeTop").OrderBy(p => p.Orden).ToList();
                ViewBag.IndicadoresBottom = _Indicadores.Where(p => p.Grupo == "HomeBottom").OrderBy(p => p.Orden).ToList();

                var _IndicadoresOficina = _Db.SP_PPROV_Indicadores_Oficina(User.Identity.Name).ToList();
                ViewBag.IndicadoresTopOficina = _IndicadoresOficina.Where(p => p.Grupo == "HomeTop").OrderBy(p => p.Orden).ToList();
                ViewBag.IndicadoresBottomOficina = _IndicadoresOficina.Where(p => p.Grupo == "HomeBottom").OrderBy(p => p.Orden).ToList();
            }


            return View();
        }

        public ActionResult Permisos()
        {
            ViewBag.Message = "No tiene Persmisos para ver esta Pagina.";

            return View();
        }

        public ActionResult PermisosDatos()
        {
            ViewBag.Message = "No tiene Permisos para ver los datos Solicitados.";

            return View();
        }


        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        [Authorize]
        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }







    }
}