using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using Microsoft.AspNet.Identity;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Configuration;
using System.Net.Http;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using System.Xml;
using Wareways.PortalProv.Infraestructura;
using Wareways.PortalProv.Infraestructura.SAP;
using Wareways.PortalProv.Models.OC;
using Wareways.PortalProv.Models.PP;
using Wareways.PortalProv.Servicios;

namespace Wareways.PortalProv.Controllers
{
    public class OrdenesCompraController : Controller
    {

        Infraestructura.PortalProvEntities _Db = new Infraestructura.PortalProvEntities();
        VServicio vServicio = new VServicio();

        public ActionResult EntregaParcial(Guid Fel_Unique, string EntregaSaldo, string EntregaMonto, string Observaciones)
        {
            try
            {
                if (decimal.Parse(EntregaMonto) > decimal.Parse(EntregaSaldo))
                {
                    TempData["MensajeDanger"] = "El monto de la Entrega no peude ser mayor que el saldo de la Orden de Compra";
                    return RedirectToAction("Emitir", new { id = Fel_Unique });
                }
                return RedirectToAction("EntregaC", new { Fel_Unique = Fel_Unique, MontoParcial = EntregaMonto, Observaciones = Observaciones });
            }
            catch (Exception ex) {
                TempData["MensajeDanger"] = "Error "+ ex.Message;
            }
            return RedirectToAction("Emitir", new { id = Fel_Unique });
        }


        public ActionResult AgregaComentario(String comentario, String unique, String estado)
        {
            var _Unique = Guid.Parse(unique);
            AgregarHistorico(_Unique, estado, estado, "Comentario", comentario);

            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        [HttpPost]
        public JsonResult ImportPasteExcel(string datos, string unique, string estado)
        {
            var error = "";
            int row = 0;
            if (estado == "Borrador" && unique != null && datos != null)
            {
                string[] lines = datos.Split('\n');

                string[] fields;

                List<FEL_DocDetalle> lstAgregar = new List<FEL_DocDetalle>();

                Boolean formatoCorrecto = false;

                try
                {


                    foreach (string item in lines)
                    {
                        fields = item.Split('\t');
                        if (
                            (row == 0 && fields[0] == "Cantidad") &&
                            (row == 0 && fields[1] == "Descripcion") &&
                            (row == 0 && fields[3] == "Precio") &&
                            (row == 0 && fields[4] == "Impuesto") &&
                            (row == 0 && fields[5] == "Cuenta Contable") &&
                            (row == 0 && fields[6] == "Centro Costo")
                           ) { formatoCorrecto = true; }
                        if (formatoCorrecto == false)
                        {
                            if (row == 0) { throw new Exception("Formato Incorrecto / Encabezados"); }

                        }



                        if (formatoCorrecto && row > 0 && fields.Count() == 7)
                        {


                            var nuevo = new FEL_DocDetalle();


                            nuevo.FEL_Unique = Guid.Parse(unique);
                            nuevo.Cantidad = Int32.Parse(fields[0]);
                            nuevo.CentroCosto = fields[6];
                            nuevo.CuentaContable = fields[5];
                            nuevo.Descripcion = fields[1];
                            nuevo.Linea = row + 1;
                            nuevo.PrecioUnitario = decimal.Parse(fields[3]);
                            nuevo.TipoImpuesto = fields[4];
                            nuevo.TotalLinea = decimal.Parse(fields[3]) * Int32.Parse(fields[0]);
                            nuevo.UnidadMedida = fields[2];
                            nuevo.TipoDet = "S";
                            nuevo.UserNameAudit = User.Identity.Name;
                            nuevo.DateAudit = DateTime.Now;


                            lstAgregar.Add(nuevo);
                        }

                        row++;

                    }
                    _Db.FEL_DocDetalle.AddRange(lstAgregar);
                    _Db.SaveChanges();

                }
                catch (Exception ex)
                {
                    error = string.Format("Error Linea {0} --> {1}", row, ex.Message);
                }

            }


            return Json(new { error = error }, JsonRequestBehavior.AllowGet);

        }

        [HttpGet]
        public JsonResult restodastiendas()
        {

            return Json(vServicio.ObtenerCCTiendasTodas(), JsonRequestBehavior.AllowGet);

        }
        [HttpGet]
        public JsonResult restodasregiones()
        {

            return Json(vServicio.ObtenerCCRegionesTodas(), JsonRequestBehavior.AllowGet);

        }
        [HttpGet]
        public JsonResult resregiontiendas(String region)
        {

            return Json(vServicio.ObtenerCCTiendasRegion(region), JsonRequestBehavior.AllowGet);

        }

        public ActionResult Eliminar(Guid id, Guid docid)
        {
            var _datos = _Db.FEL_DocAdjunto.Where(p => p.AdjuntoId == id && p.FEL_Unique == docid).ToList();

            if (_datos.Count > 0) { _Db.FEL_DocAdjunto.RemoveRange(_datos); }
            _Db.SaveChanges();

            return RedirectToAction("Emitir", new { id = docid });
        }


        [Authorize(Roles = "Ordenes Compra")]
        public ActionResult Index(String id)
        {

            string FiltroEstado = id;
            //if (id == null) id = "Borrador";
            var modelo = new OrdenesCompraIndexModel();


            var _EmpresaId = Int32.Parse(ConfigurationManager.AppSettings["WWPortal_EmpresaId"]);

            // Filtro Activo

            String _FiltroActivo = "Borrador";
            try
            {
                _FiltroActivo = Request.Cookies["FiltroEstadoOC"].Value;
            }
            catch { }

            if (FiltroEstado != null || FiltroEstado == "")
            {
                if (FiltroEstado == "") FiltroEstado = "Borrador";
                var _Cookie = new HttpCookie("FiltroEstadoOC", FiltroEstado);
                _Cookie.Expires = DateTime.MaxValue;
                Response.Cookies.Add(_Cookie);
                _FiltroActivo = FiltroEstado;
            }
            ViewBag.FiltroEstado = _FiltroActivo;





            modelo.Datos = vServicio.GetUserOC(User.Identity.Name, _EmpresaId).Where(p => p.Estado == _FiltroActivo).ToList();
            if (_FiltroActivo == "Cancelado")
            {
                modelo.Datos = modelo.Datos.Where(p => p.ActualizadoEl > DateAndTime.Now.AddDays(-30)).ToList();
            }


            if (_FiltroActivo == "AKIOL") modelo.DatosAKIOL = vServicio.GetUserOC_AKIOL(User.Identity.Name, _EmpresaId);

            modelo.EstadoSel = _FiltroActivo;
            // Resumenes
            modelo.NoBorrador = vServicio.GetUserOCEstadoNo(User.Identity.Name, _EmpresaId, "Borrador");
            modelo.NoAutorizar = vServicio.GetUserOCEstadoNo(User.Identity.Name, _EmpresaId, "Autorizar");
            modelo.NoAprobado = vServicio.GetUserOCEstadoNo(User.Identity.Name, _EmpresaId, "Aprobado");
            modelo.NoGenerado = vServicio.GetUserOCEstadoNo(User.Identity.Name, _EmpresaId, "Generado");
            modelo.NoRevision = vServicio.GetUserOCEstadoNo(User.Identity.Name, _EmpresaId, "Revision");
            modelo.NoAprobadoEspecial = vServicio.GetUserOCEstadoNo(User.Identity.Name, _EmpresaId, "AutoEspe");
            modelo.NoCancelado = 0; // vServicio.GetUserOCEstadoNo(User.Identity.Name, _EmpresaId,"Cancelado");
            modelo.NoAKIol = vServicio.GetUserOCEstadoNo(User.Identity.Name, _EmpresaId, "AKIOL");

            // Validar Permiso Crear
            var Usuario = _Db.AspNetUsers.Where(p => p.UserName == User.Identity.Name).First();
            var Accesos = _Db.PPROV_UsuarioDepartamento.Where(p => p.Empresa_Id == _EmpresaId && p.Crear && p.UserId == Usuario.Id).ToList();
            ViewBag.MostarCrear = false;
            if (Accesos.Count > 0) ViewBag.MostarCrear = true;


            return View(modelo);
        }


        public ActionResult TestBPChange()
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
            var _SAP_OCSerie = ConfigurationManager.AppSettings["SapOCSerie"];

            var txtSessID = DiServer.Login(_SAP_Server, _SAP_companydb, _SAP_DataBaseType, _SAP_dbuser, _SAP_dbpassword,
                                               _SAP_user, _SAP_password, _SAP_language, _SAP_Lincese);
            if (!txtSessID.Contains("Error"))
            {

                var oInvoicesXML = DiServer.GetEmpySchema(txtSessID, "oBusinessPartners");

                var Orignal = DiServer.GetByKey(txtSessID, "<Object>oBusinessPartners</Object><CardCode>C20000</CardCode>");

                // definir NameSapce
                XmlNamespaceManager nsmgr = new XmlNamespaceManager(oInvoicesXML.NameTable);
                nsmgr.AddNamespace("a", oInvoicesXML.DocumentElement.NamespaceURI);

                XmlNamespaceManager nsmgr2 = new XmlNamespaceManager(Orignal.NameTable);
                nsmgr2.AddNamespace("a", Orignal.DocumentElement.NamespaceURI);

                // Tipo Documento
                //oInvoicesXML.SelectSingleNode("//a:Object", nsmgr).InnerText = "oPurchaseOrders";

                oInvoicesXML.SelectNodes("//a:CardCode", nsmgr)[0].InnerText = "C20000";
                oInvoicesXML.SelectNodes("//a:CardCode", nsmgr)[1].InnerText = "C20000";






                XmlNode oDocumentLines = oInvoicesXML.SelectSingleNode("//a:BPAddresses", nsmgr);


                var nodooriginal = Orignal.SelectSingleNode("//a:BPAddresses", nsmgr2).CloneNode(true);



                //oDocumentLines.ParentNode.AppendChild(oInvoicesXML.CreateElement("BPAddresses2"));

                var DefaultShip = Orignal.SelectSingleNode("//a:ShipToDefault", nsmgr2).InnerText;
                var EncontroShipTo = false;

                foreach (XmlNode itemnode in nodooriginal.ChildNodes)
                {
                    var tipo = itemnode.SelectSingleNode(".//a:AddressName", nsmgr2).InnerText;
                    var AddressType = itemnode.SelectSingleNode(".//a:AddressType", nsmgr2).InnerText;
                    if (tipo == DefaultShip && AddressType == "bo_ShipTo")
                    {
                        itemnode.SelectSingleNode(".//a:Street", nsmgr).InnerText = "4417 RustField Road";
                        EncontroShipTo = true;
                    }
                    // limpia innecesarias
                    itemnode.SelectSingleNode(".//a:CreateDate", nsmgr2).InnerText = "";
                    itemnode.SelectSingleNode(".//a:CreateTime", nsmgr2).InnerText = "";
                }
                oDocumentLines.InnerXml = nodooriginal.InnerXml;
                if (EncontroShipTo == false)
                {
                    // agregar Registro si no econtrro Ship to
                    oDocumentLines.InnerXml = nodooriginal.InnerXml +
                        @"<row xmlns=""http://www.sap.com/SBO/DIS""><AddressName>Ship to</AddressName><Street>" + "4417 RustField Road" + "</Street></row>";
                    oInvoicesXML.SelectSingleNode("//a:ShipToDefault", nsmgr2).InnerText = "Ship to";
                }



                // get the first row                 

                //oDocumentLines.SelectNodes("//a:Street", nsmgr)[1].InnerText = "4417 RustField Rd";

                //oInvoicesXML.SelectNodes("//a:CreateDate", nsmgr)[0].InnerText = "";
                //oInvoicesXML.SelectNodes("//a:CreateDate", nsmgr)[1].InnerText = "";
                //oInvoicesXML.SelectNodes("//a:CreateTime", nsmgr)[0].InnerText = "";
                //oInvoicesXML.SelectNodes("//a:CreateTime", nsmgr)[1].InnerText = "";


                // Limpiar XML Vacios                        
                XmlNode oCleanInvoicesXML = DiServer.RemoveEmptyNodes(oInvoicesXML);


                // Add Quotation
                XmlDocument oXmlReply;
                oXmlReply = DiServer.UpdateInvoice(txtSessID, oCleanInvoicesXML.OuterXml);




                // Logout
                var _resultLogout = DiServer.Logout(txtSessID);

            }

            return RedirectToAction("index");
        }

        public ActionResult GetInoviceXMl()
        {
            var _SAP_Server = ConfigurationManager.AppSettings["SapServer"];
            var _SAP_Lincese = ConfigurationManager.AppSettings["SapLicenseServer"];
            var _SAP_companydb = "Test2"; // ConfigurationManager.AppSettings["SapCompanyDb"];
            var _SAP_DataBaseType = ConfigurationManager.AppSettings["SapDataBaseType"];
            var _SAP_dbuser = ConfigurationManager.AppSettings["SapDbUser"];
            var _SAP_dbpassword = ConfigurationManager.AppSettings["SapDbPassword"];
            var _SAP_user = ConfigurationManager.AppSettings["SapUser"];
            var _SAP_password = ConfigurationManager.AppSettings["SapPassword"];
            var _SAP_language = ConfigurationManager.AppSettings["SapLanguage"];
            var _SAP_OCSerie = ConfigurationManager.AppSettings["SapOCSerie"];

            var txtSessID = DiServer.Login(_SAP_Server, _SAP_companydb, _SAP_DataBaseType, _SAP_dbuser, _SAP_dbpassword,
                                               _SAP_user, _SAP_password, _SAP_language, _SAP_Lincese);
            if (!txtSessID.Contains("Error"))
            {

                var oInvoicesXML = DiServer.GetEmpySchema(txtSessID, "oBusinessPartners");

                var Orignal = DiServer.GetByKey(txtSessID, "<Object>oPurchaseInvoices</Object><DocEntry>1</DocEntry>");

                // definir NameSapce
                XmlNamespaceManager nsmgr = new XmlNamespaceManager(oInvoicesXML.NameTable);
                nsmgr.AddNamespace("a", oInvoicesXML.DocumentElement.NamespaceURI);

                XmlNamespaceManager nsmgr2 = new XmlNamespaceManager(Orignal.NameTable);
                nsmgr2.AddNamespace("a", Orignal.DocumentElement.NamespaceURI);

                // Tipo Documento
                //oInvoicesXML.SelectSingleNode("//a:Object", nsmgr).InnerText = "oPurchaseOrders";

                oInvoicesXML.SelectNodes("//a:CardCode", nsmgr)[0].InnerText = "C20000";
                oInvoicesXML.SelectNodes("//a:CardCode", nsmgr)[1].InnerText = "C20000";






                XmlNode oDocumentLines = oInvoicesXML.SelectSingleNode("//a:BPAddresses", nsmgr);


                var nodooriginal = Orignal.SelectSingleNode("//a:BPAddresses", nsmgr2).CloneNode(true);



                //oDocumentLines.ParentNode.AppendChild(oInvoicesXML.CreateElement("BPAddresses2"));

                var DefaultShip = Orignal.SelectSingleNode("//a:ShipToDefault", nsmgr2).InnerText;
                var EncontroShipTo = false;

                foreach (XmlNode itemnode in nodooriginal.ChildNodes)
                {
                    var tipo = itemnode.SelectSingleNode(".//a:AddressName", nsmgr2).InnerText;
                    var AddressType = itemnode.SelectSingleNode(".//a:AddressType", nsmgr2).InnerText;
                    if (tipo == DefaultShip && AddressType == "bo_ShipTo")
                    {
                        itemnode.SelectSingleNode(".//a:Street", nsmgr).InnerText = "4417 RustField Road";
                        EncontroShipTo = true;
                    }
                    // limpia innecesarias
                    itemnode.SelectSingleNode(".//a:CreateDate", nsmgr2).InnerText = "";
                    itemnode.SelectSingleNode(".//a:CreateTime", nsmgr2).InnerText = "";
                }
                oDocumentLines.InnerXml = nodooriginal.InnerXml;
                if (EncontroShipTo == false)
                {
                    // agregar Registro si no econtrro Ship to
                    oDocumentLines.InnerXml = nodooriginal.InnerXml +
                        @"<row xmlns=""http://www.sap.com/SBO/DIS""><AddressName>Ship to</AddressName><Street>" + "4417 RustField Road" + "</Street></row>";
                    oInvoicesXML.SelectSingleNode("//a:ShipToDefault", nsmgr2).InnerText = "Ship to";
                }



                // get the first row                 

                //oDocumentLines.SelectNodes("//a:Street", nsmgr)[1].InnerText = "4417 RustField Rd";

                //oInvoicesXML.SelectNodes("//a:CreateDate", nsmgr)[0].InnerText = "";
                //oInvoicesXML.SelectNodes("//a:CreateDate", nsmgr)[1].InnerText = "";
                //oInvoicesXML.SelectNodes("//a:CreateTime", nsmgr)[0].InnerText = "";
                //oInvoicesXML.SelectNodes("//a:CreateTime", nsmgr)[1].InnerText = "";


                // Limpiar XML Vacios                        
                XmlNode oCleanInvoicesXML = DiServer.RemoveEmptyNodes(oInvoicesXML);


                // Add Quotation
                XmlDocument oXmlReply;
                oXmlReply = DiServer.UpdateInvoice(txtSessID, oCleanInvoicesXML.OuterXml);




                // Logout
                var _resultLogout = DiServer.Logout(txtSessID);

            }

            return RedirectToAction("index");
        }



        [Authorize(Roles = "Ordenes Compra")]
        public ActionResult EmailOc(Guid id)
        {
            var EmpresaAsuntoCorreo = _Db.GEN_Empresa.First().EmpresaAsuntoCorreo;
            try
            {
                string body = string.Empty;
                using (StreamReader reader = new StreamReader(Server.MapPath("~/Infraestructura/Correos/EnvioOc.html")))
                {
                    body = reader.ReadToEnd();
                }
                var oWFactura2 = _Db.FEL_Doc.Where(p => p.FEL_Unique == id).First();

                if (oWFactura2.Correlativo != 0)
                {
                    //var _Documento = _oc _Db.PPROV_Documento.Find(id);
                    var _Proveedor = _Db.V_PPROV_Proveedor.Where(p => p.CardCode == oWFactura2.CardCode).ToList();
                    var _Empresa = _Db.V_PPROV_Empresas.Where(p => p.Empresa_Id == 1).ToList();

                    body = body.Replace("***EmpresaNombre***", _Empresa[0].AliasName);
                    body = body.Replace("***EmpresaLogo***", _Empresa[0].Logo);

                    body = body.Replace("**NombreProveedor**", _Proveedor[0].CardCode + " " + _Proveedor[0].CardName);
                    body = body.Replace("**NumeroOrdenCompra**", oWFactura2.Correlativo.ToString());

                    var _CorreosDestino = _Db.SP_ObtenerCorreos_Por_CardCode(_Proveedor[0].CardCode).ToList();
                    if (_CorreosDestino.Count == 0) _CorreosDestino.Add(new SP_ObtenerCorreos_Por_CardCode_Result { CardCode = _Proveedor[0].CardCode, Email = User.Identity.Name });

                    foreach (var _Correo in _CorreosDestino)
                    {
                        using (var message = new MailMessage())
                        {
                            SmtpSection smtpSection = (SmtpSection)ConfigurationManager.GetSection("system.net/mailSettings/smtp");
                            message.To.Add(new MailAddress(_Correo.Email));
                            //message.CC.Add(new MailAddress(oWFactura2.CreadoPor));
                            
                            try { message.CC.Add(new MailAddress(User.Identity.Name)); } catch { }

                            message.From = new MailAddress(smtpSection.From);
                            message.Subject = EmpresaAsuntoCorreo+" Portal Proveedores – OC Emitida No. " + oWFactura2.Correlativo.ToString() + ", Mensaje enviado el " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                            message.Body = body;
                            message.IsBodyHtml = true; // change to true if body msg is in html

                            try
                            {

                                ReportDocument Report = new ReportDocument();
                                var ArchivoReporte = Server.MapPath("~/Reportes/OrdenCompaSTD.rpt");

                                if (id != Guid.Empty)
                                {
                                    VServicio vService = new VServicio();
                                    var ReportData = vService.List_SapOPORSync(id.ToString());
                                    Report.Load(ArchivoReporte);
                                    Report.SetParameterValue("DocKey@", ReportData[0].DocEntry);

                                    string fileName = String.Format("OC{0}_{1}_{2}.pdf", ReportData[0].DocNum, Session["EmpresaSelName"].ToString(), DateTime.Now.ToShortDateString() + "_" + DateTime.Now.ToString("hh-mm-ss"));
                                    Report.SummaryInfo.ReportTitle = "ImpresionOC";

                                    var _SAP_Server = ConfigurationManager.AppSettings["SapServer"];
                                    var _SAP_companydb = ConfigurationManager.AppSettings["SapCompanyDb"];
                                    var _SAP_dbuser = ConfigurationManager.AppSettings["SapDbUser"];
                                    var _SAP_dbpassword = ConfigurationManager.AppSettings["SapDbPassword"];

                                    ConnectionInfo crConnectionInfo = new ConnectionInfo();
                                    crConnectionInfo.ServerName = _SAP_Server;
                                    crConnectionInfo.DatabaseName = _SAP_companydb;
                                    crConnectionInfo.UserID = _SAP_dbuser;
                                    crConnectionInfo.Password = _SAP_dbpassword;
                                    crConnectionInfo.IntegratedSecurity = false;

                                    TableLogOnInfo crTableLogoninfo = new TableLogOnInfo();

                                    foreach (CrystalDecisions.CrystalReports.Engine.Table CrTable in Report.Database.Tables)
                                    {
                                        crTableLogoninfo = CrTable.LogOnInfo;
                                        crTableLogoninfo.ConnectionInfo = crConnectionInfo;
                                        CrTable.ApplyLogOnInfo(crTableLogoninfo);
                                    }
                                    foreach (ReportDocument subreport in Report.Subreports)
                                    {
                                        foreach (CrystalDecisions.CrystalReports.Engine.Table CrTable in subreport.Database.Tables)
                                        {
                                            crTableLogoninfo = CrTable.LogOnInfo;
                                            crTableLogoninfo.ConnectionInfo = crConnectionInfo;
                                            CrTable.ApplyLogOnInfo(crTableLogoninfo);
                                        }
                                    }


                                    //ExportOptions options = new ExportOptions();
                                    //options.ExportFormatType = ExportFormatType.PortableDocFormat;
                                    //options.FormatOptions = new ExcelFormatOptions();
                                    System.IO.Stream s = Report.ExportToStream(ExportFormatType.PortableDocFormat);
                                    message.Attachments.Add(new Attachment(s, fileName));

                                }



                            }
                            catch { }

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


                    TempData["MensajeSuccess"] = "Correo Enviado con Extito";
                }


            }
            catch (Exception ex)
            {
                TempData["MensajeDanger"] = "Error " + ex.Message + "-->" + ex.StackTrace;
            }

            return RedirectToAction("Emitir", new { id = id });
        }




        [Authorize(Roles = "Ordenes Compra")]
        public ActionResult GenerarSap(Guid Fel_Unique)
        {
            var Subsite = ConfigurationManager.AppSettings["SubSite"];
            if (string.IsNullOrEmpty(Subsite)) Subsite = "";


            var _SAP_Server = ConfigurationManager.AppSettings["SapServer"];
            var _SAP_Lincese = ConfigurationManager.AppSettings["SapLicenseServer"];
            var _SAP_companydb = ConfigurationManager.AppSettings["SapCompanyDb"];
            var _SAP_DataBaseType = ConfigurationManager.AppSettings["SapDataBaseType"];
            var _SAP_dbuser = ConfigurationManager.AppSettings["SapDbUser"];
            var _SAP_dbpassword = ConfigurationManager.AppSettings["SapDbPassword"];
            var _SAP_user = ConfigurationManager.AppSettings["SapUser"];
            var _SAP_password = ConfigurationManager.AppSettings["SapPassword"];
            var _SAP_language = ConfigurationManager.AppSettings["SapLanguage"];
            var _SAP_OCSerie = ConfigurationManager.AppSettings["SapOCSerie"];
            var _DimensionCC = ConfigurationManager.AppSettings["DimensionCC"];
            var _DimensionFieldXml = "CostingCode";
            if (_DimensionCC != "1") _DimensionFieldXml += _DimensionCC;


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

                    var oWFactura = _Db.FEL_Doc.Where(p => p.FEL_Unique == Fel_Unique).First();
                    var lstDepartamento = _Db.PPROV_Departamento.Where(p => p.DepartmentId == oWFactura.DepartamentoId).ToList();

                    txtSessID = DiServer.Login(_SAP_Server, _SAP_companydb, _SAP_DataBaseType, _SAP_dbuser, _SAP_dbpassword,
                                                _SAP_user, _SAP_password, _SAP_language, _SAP_Lincese);
                    if (!txtSessID.Contains("Error"))
                    {
                        var serverPath = Server.MapPath("~");
                        var lstAdjunto = _Db.FEL_DocAdjunto.Where(p => p.FEL_Unique == oWFactura.FEL_Unique).OrderByDescending(p => p.AdjuntoFecha).ToList();
                        String noAttach = "";



                        if (lstAdjunto.Count > 0)
                        {
                            var oAttachments = DiServer.GetEmpySchema(txtSessID, "oAttachments2");
                            XmlNamespaceManager nsmgrs = new XmlNamespaceManager(oAttachments.NameTable);
                            nsmgrs.AddNamespace("a", oAttachments.DocumentElement.NamespaceURI);

                            var _contadorLineaSapAdjunto = 1;
                            // get ref to the Document_Lines
                            XmlNode oDocumentLinesAdj = oAttachments.SelectSingleNode("//a:Attachments2_Lines", nsmgrs);
                            //
                            XmlNode oFirstRowAdj = oDocumentLinesAdj.FirstChild;

                            foreach (var _ItemAdjunto in lstAdjunto)
                            {
                                if (_contadorLineaSapAdjunto == 1)
                                {
                                    string UbicacionArchivo = Server.MapPath(Subsite+_ItemAdjunto.AdjuntoRuta);
                                    oFirstRowAdj.SelectSingleNode("//a:SourcePath", nsmgrs).InnerText = Path.GetDirectoryName(UbicacionArchivo);
                                    oFirstRowAdj.SelectSingleNode("//a:FileName", nsmgrs).InnerText = Path.GetFileName(UbicacionArchivo).Split('.')[0];
                                    oFirstRowAdj.SelectSingleNode("//a:FileExtension", nsmgrs).InnerText = Path.GetExtension(UbicacionArchivo).Replace(".", "");
                                    oFirstRowAdj.SelectSingleNode("//a:Override", nsmgrs).InnerText = "tYES";

                                }
                                else
                                {
                                    // copy the first row the th new one -> for getting the same structure
                                    XmlNode oNewRow = oFirstRowAdj.CloneNode(true);
                                    string UbicacionArchivo = Server.MapPath(_ItemAdjunto.AdjuntoRuta);
                                    oNewRow.SelectSingleNode("//a:SourcePath", nsmgrs).InnerText = Path.GetDirectoryName(UbicacionArchivo);
                                    oNewRow.SelectSingleNode("//a:FileName", nsmgrs).InnerText = Path.GetFileName(UbicacionArchivo).Split('.')[0];
                                    oNewRow.SelectSingleNode("//a:FileExtension", nsmgrs).InnerText = Path.GetExtension(UbicacionArchivo).Replace(".", "");
                                    oNewRow.SelectSingleNode("//a:Override", nsmgrs).InnerText = "tYES";
                                    oDocumentLinesAdj.AppendChild(oNewRow);
                                }
                                _contadorLineaSapAdjunto += 1;
                            }

                            // Limpiar XML Vacios                        
                            XmlNode oCleanAttachmentXMLAtt = DiServer.RemoveEmptyNodes(oAttachments);
                            XmlDocument oXmlReplyAtt;

                            oXmlReplyAtt = DiServer.AddInvoice(txtSessID, oCleanAttachmentXMLAtt.OuterXml);
                            if (Strings.InStr(oXmlReplyAtt.InnerXml, "<env:Fault>", (Microsoft.VisualBasic.CompareMethod)(0)) > 0)
                            { // And (Not (sret.StartsWith("Error"))) Then
                                String sRet = "Error: " + oXmlReplyAtt.InnerXml;
                                TempData["MensajeDanger"] = "Error SAP: " + sRet;
                            }
                            {
                                try
                                {
                                    noAttach = oXmlReplyAtt.SelectNodes("//a:RetKey", nsmgrs).Item(0).InnerText;
                                }
                                catch (Exception ex)
                                {
                                    String sRet = "Error: " + oXmlReplyAtt.InnerXml;
                                    TempData["MensajeDanger"] = "Error SAP: " + sRet;
                                }
                                
                            }



                        }
                        var oInvoicesXML = DiServer.GetEmpySchema(txtSessID, "oPurchaseOrders");
                        // definir NameSapce
                        XmlNamespaceManager nsmgr = new XmlNamespaceManager(oInvoicesXML.NameTable);
                        nsmgr.AddNamespace("a", oInvoicesXML.DocumentElement.NamespaceURI);

                        // Tipo Documento
                        oInvoicesXML.SelectSingleNode("//a:Object", nsmgr).InnerText = "oPurchaseOrders";

                        // Encabezado
                        Console.WriteLine("Encabezado");
                        var oBpCode = oInvoicesXML.SelectNodes("//a:Series", nsmgr);
                        oBpCode.Item(0).InnerText = _SAP_OCSerie;
                        //oInvoice.BPL_IDAssignedToInvoice = 1;  Requerido Solo multi-empresa
                        oInvoicesXML.SelectNodes("//a:NumAtCard", nsmgr).Item(0).InnerText = oWFactura.Referencia;
                        oInvoicesXML.SelectNodes("//a:CardCode", nsmgr).Item(0).InnerText = oWFactura.CardCode;
                        //oInvoicesXML.SelectNodes("//a:CardName", nsmgr).Item(0).InnerText = oWFactura.Rece_Nombre;
                        oInvoicesXML.SelectNodes("//a:DocDate", nsmgr).Item(0).InnerText = oWFactura.FechaEmision.ToString("yyyyMMdd");
                        //oInvoicesXML.SelectNodes("//a:SalesPersonCode", nsmgr).Item(0).InnerText = oWFactura.SlpCode.Value.ToString();
                        oInvoicesXML.SelectNodes("//a:DocDueDate", nsmgr).Item(0).InnerText = oWFactura.FechaEmision.ToString("yyyyMMdd");
                        if (oWFactura.Tipo_Detalle != "Servicio")
                        {
                            oInvoicesXML.SelectNodes("//a:DocType", nsmgr).Item(0).InnerText = SAPbobsCOM.BoDocumentTypes.dDocument_Items.ToString();
                        }
                        else
                        {
                            oInvoicesXML.SelectNodes("//a:DocType", nsmgr).Item(0).InnerText = SAPbobsCOM.BoDocumentTypes.dDocument_Service.ToString();
                        }
                        if (!string.IsNullOrEmpty(noAttach))
                        {
                            oInvoicesXML.SelectNodes("//a:AttachmentEntry", nsmgr).Item(0).InnerText = noAttach;
                        }

                        oInvoicesXML.SelectNodes("//a:DocCurrency", nsmgr).Item(0).InnerText = (oWFactura.Moneda == "GTQ") ? "QTZ" : oWFactura.Moneda;

                        if (oWFactura.Comentario == null) oWFactura.Comentario = "";

                        if (oWFactura.Comentario.Length > 250)
                        {
                            oInvoicesXML.SelectNodes("//a:Comments", nsmgr).Item(0).InnerText = oWFactura.Comentario.Substring(0, 250);
                        }
                        else
                        {
                            oInvoicesXML.SelectNodes("//a:Comments", nsmgr).Item(0).InnerText = oWFactura.Comentario;
                        }

                        //oWFactura.Rece_Direccion = oWFactura.Rece_Direccion.PadLeft(254, ' ');
                        //oInvoicesXML.SelectNodes("//a:Address", nsmgr).Item(0).InnerText = oWFactura.Rece_Direccion.Substring(0, 254);

                        // User Fields
                        Console.WriteLine("UserFields");
                        oInvoicesXML.SelectNodes("//a:U_FacNit", nsmgr).Item(0).InnerText = oWFactura.Nit.ToString();

                        try { oInvoicesXML.SelectNodes("//a:U_Llsdi", nsmgr).Item(0).InnerText = "N"; } catch { }
                        try { oInvoicesXML.SelectNodes("//a:U_TIPO_DOCUMENTO", nsmgr).Item(0).InnerText = "ZZ"; } catch { }

                        oInvoicesXML.SelectNodes("//a:U_WW_SyncId", nsmgr).Item(0).InnerText = oWFactura.FEL_Unique.ToString();
                        oInvoicesXML.SelectNodes("//a:U_WW_SyncDate", nsmgr).Item(0).InnerText = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        oInvoicesXML.SelectNodes("//a:U_WW_SyncNumero", nsmgr).Item(0).InnerText = oWFactura.Correlativo.ToString();

                        oInvoicesXML.SelectNodes("//a:U_WW_Propietario", nsmgr).Item(0).InnerText = TraeNombreUsuario(oWFactura.CreadoPor);
                        oInvoicesXML.SelectNodes("//a:U_WW_EnviaAuto", nsmgr).Item(0).InnerText = oWFactura.EnvioAuto.Value.ToString("yyyy-MM-dd HH:mm:ss");
                        oInvoicesXML.SelectNodes("//a:U_WW_Autoriza", nsmgr).Item(0).InnerText = TraeNombreUsuario(oWFactura.AutorizadoPor);
                        oInvoicesXML.SelectNodes("//a:U_WW_AutorizaEl", nsmgr).Item(0).InnerText = oWFactura.ActualizadoEl.Value.ToString("yyyy-MM-dd HH:mm:ss");
                        oInvoicesXML.SelectNodes("//a:U_WW_GeneradoEl", nsmgr).Item(0).InnerText = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        if (lstDepartamento.Count == 1) oInvoicesXML.SelectNodes("//a:U_WW_Departamento", nsmgr).Item(0).InnerText = lstDepartamento[0].DepartmentName;
                        Console.WriteLine("Grupo 1");

                        // detalle
                        Console.WriteLine("Mandando Detalle");
                        var _contadorLineaSap = 1;
                        // get ref to the Document_Lines
                        XmlNode oDocumentLines = oInvoicesXML.SelectSingleNode("//a:Document_Lines", nsmgr);
                        // get the first row 
                        XmlNode oFirstRow = oDocumentLines.FirstChild;


                        foreach (var _ItemFactura in oWFactura.FEL_DocDetalle)
                        {
                            if (_contadorLineaSap == 1)
                            {
                                if (oWFactura.Tipo_Detalle == "Servicio")
                                {
                                    var DescripcionOld = _ItemFactura.Descripcion;
                                    _ItemFactura.Descripcion = _ItemFactura.Descripcion.Replace("&", "");
                                    if (!_ItemFactura.Descripcion.Contains(" Cant. "))
                                    {
                                        _ItemFactura.Descripcion = _ItemFactura.Descripcion + " Cant. " + _ItemFactura.Cantidad.ToString();
                                    }
                                    if (_ItemFactura.Descripcion.Count() > 100) _ItemFactura.Descripcion = _ItemFactura.Descripcion.Substring(0, 100);
                                    oFirstRow.SelectSingleNode("//a:ItemDescription", nsmgr).InnerXml = _ItemFactura.Descripcion;
                                    _ItemFactura.Descripcion = DescripcionOld;
                                    oFirstRow.SelectSingleNode("//a:AccountCode", nsmgr).InnerXml = lstCuentasSAP.Where(p => p.ActId == _ItemFactura.CuentaContable).First().AcctCode;
                                    oFirstRow.SelectSingleNode("//a:PriceAfterVAT", nsmgr).InnerXml = (_ItemFactura.PrecioUnitario * _ItemFactura.Cantidad).ToString();
                                }
                                else
                                {
                                    oFirstRow.SelectSingleNode("//a:ItemCode", nsmgr).InnerXml = _ItemFactura.Descripcion.Split(' ')[0].TrimEnd();
                                    oFirstRow.SelectSingleNode("//a:PriceAfterVAT", nsmgr).InnerXml = _ItemFactura.PrecioUnitario.ToString();
                                }
                                if (!String.IsNullOrEmpty(_ItemFactura.CentroCosto)) oFirstRow.SelectSingleNode("//a:" + _DimensionFieldXml, nsmgr).InnerXml = _ItemFactura.CentroCosto;
                                oFirstRow.SelectSingleNode("//a:Quantity", nsmgr).InnerXml = _ItemFactura.Cantidad.ToString();
                                oFirstRow.SelectSingleNode("//a:WarehouseCode", nsmgr).InnerXml = oWFactura.Bodega;

                                oFirstRow.SelectSingleNode("//a:TaxCode", nsmgr).InnerXml = _ItemFactura.TipoImpuesto;
                            }
                            else
                            {

                                // copy the first row the th new one -> for getting the same structure
                                XmlNode oNewRow = oFirstRow.CloneNode(true);
                                // update the new row
                                if (oWFactura.Tipo_Detalle == "Servicio")
                                {
                                    var DescripcionOld = _ItemFactura.Descripcion;
                                    _ItemFactura.Descripcion = _ItemFactura.Descripcion.Replace("&", "");

                                    if (!_ItemFactura.Descripcion.Contains(" Cant. "))
                                    {
                                        _ItemFactura.Descripcion = _ItemFactura.Descripcion + " Cant. " + _ItemFactura.Cantidad.ToString();
                                    }
                                    if (_ItemFactura.Descripcion.Count() > 100) _ItemFactura.Descripcion = _ItemFactura.Descripcion.Substring(0, 100);
                                    oNewRow.SelectSingleNode("//a:ItemDescription", nsmgr).InnerXml = _ItemFactura.Descripcion;
                                    _ItemFactura.Descripcion = DescripcionOld;

                                    oNewRow.SelectSingleNode("//a:AccountCode", nsmgr).InnerXml = lstCuentasSAP.Where(p => p.ActId == _ItemFactura.CuentaContable).First().AcctCode;
                                    oNewRow.SelectSingleNode("//a:PriceAfterVAT", nsmgr).InnerXml = (_ItemFactura.PrecioUnitario * _ItemFactura.Cantidad).ToString();
                                }
                                else
                                {
                                    oNewRow.SelectSingleNode("//a:ItemCode", nsmgr).InnerXml = _ItemFactura.Descripcion.Split(' ')[0].TrimEnd();
                                    oNewRow.SelectSingleNode("//a:PriceAfterVAT", nsmgr).InnerXml = _ItemFactura.PrecioUnitario.ToString();
                                }
                                if (!String.IsNullOrEmpty(_ItemFactura.CentroCosto)) oNewRow.SelectSingleNode("//a:" + _DimensionFieldXml, nsmgr).InnerXml = _ItemFactura.CentroCosto;

                                oNewRow.SelectSingleNode("//a:Quantity", nsmgr).InnerXml = _ItemFactura.Cantidad.ToString();
                                oNewRow.SelectSingleNode("//a:WarehouseCode", nsmgr).InnerXml = oWFactura.Bodega;

                                oNewRow.SelectSingleNode("//a:TaxCode", nsmgr).InnerXml = _ItemFactura.TipoImpuesto;
                                // add the new row to the DocumentLines object
                                oDocumentLines.AppendChild(oNewRow);
                            }


                            _contadorLineaSap += 1;
                        }


                        // Limpiar XML Vacios                        
                        XmlNode oCleanInvoicesXML = DiServer.RemoveEmptyNodes(oInvoicesXML);



                        // Add Quotation
                        XmlDocument oXmlReply;
                        if (vServicio.Lst_SapOPORSync(oWFactura.FEL_Unique.ToString()).Count == 0)
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
                                oWFactura.SyncSapDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                var FacturasEncontrada = vServicio.Lst_SapOPORSync(oWFactura.FEL_Unique.ToString());
                                oWFactura.SyncSapId = FacturasEncontrada[0].DocNum.ToString();
                                oWFactura.Estado = "Generado";
                                oWFactura.SincronizadoEl = DateTime.Now;
                                oWFactura.SincronizadoPor = User.Identity.Name;
                                _Db.SaveChanges();
                                oWFactura.Correlativo = Int32.Parse(oWFactura.SyncSapId);
                                _Db.SaveChanges();
                                TempData["MensajeSuccess"] = "Documento Generado en Sap, ya puede imprimir su OC";
                            }
                        }
                        else
                        {
                            // Actualizacion de Web y Busqueda Factura
                            oWFactura.SyncSapDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                            var FacturasEncontrada = vServicio.Lst_SapOPORSync(oWFactura.FEL_Unique.ToString());
                            oWFactura.SyncSapId = FacturasEncontrada[0].DocNum.ToString();
                            oWFactura.Estado = "Generado";
                            oWFactura.SincronizadoEl = DateTime.Now;
                            oWFactura.SincronizadoPor = User.Identity.Name;
                            _Db.SaveChanges();
                            oWFactura.Correlativo = Int32.Parse(oWFactura.SyncSapId);
                            _Db.SaveChanges();
                        }






                        // Logout
                        var _resultLogout = DiServer.Logout(txtSessID);

                    }
                    else
                    {
                        TempData["MensajeDanger"] = "Error " + txtSessID;
                        return RedirectToAction("Index");
                    }
                }
                catch (Exception ex)
                {
                    TempData["MensajeDanger"] = "Error " + ex.Message + "-->" + ex.StackTrace;
                    var _resultLogout = DiServer.Logout(txtSessID);
                    return RedirectToAction("Index");
                }


            }
            catch (Exception ex)
            {
                Console.WriteLine("Error " + ex.Message);

                return RedirectToAction("Index");
            }

            AgregarHistorico(Fel_Unique, "Aprobado", "Generado", "Envio", "");


            return RedirectToAction("EmailOc", new { id = Fel_Unique });
        }

        [Authorize(Roles = "Ordenes Compra")]
        public ActionResult Test()
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
            var _SAP_OCSerie = ConfigurationManager.AppSettings["SapOCSerie"];

            if (Session["EmpresaSelId"] == null)
            {
                Wareways.PortalProv.Servicios.ServicioSeguridad.CheckSession(User.Identity.Name);
            }

            try
            {

                var txtSessID = "";
                try
                {

                    txtSessID = DiServer.Login(_SAP_Server, _SAP_companydb, _SAP_DataBaseType, _SAP_dbuser, _SAP_dbpassword,
                                                _SAP_user, _SAP_password, _SAP_language, _SAP_Lincese);

                    if (!txtSessID.Contains("Error"))
                    {
                        var oInvoicesXML = DiServer.GetEmpySchema(txtSessID, "oUserFields");



                        XmlNamespaceManager nsmgrs = new XmlNamespaceManager(oInvoicesXML.NameTable);
                        nsmgrs.AddNamespace("a", oInvoicesXML.DocumentElement.NamespaceURI);




                        XmlNode new2 = oInvoicesXML.SelectSingleNode("//a:UserFieldsMD", nsmgrs);
                        new2.SelectSingleNode("//a:Object", nsmgrs).InnerText = "oUserFields";
                        new2.SelectSingleNode(".//a:TableName", nsmgrs).InnerText = "OINV";
                        new2.SelectSingleNode(".//a:Type", nsmgrs).InnerText = "db_Alpha"; // SAPbobsCOM.BoFieldTypes.db_Alpha;
                        new2.SelectSingleNode(".//a:Name", nsmgrs).InnerText = "Test2"; // SAPbobsCOM.BoFieldTypes.db_Alpha;
                        new2.SelectSingleNode(".//a:Description", nsmgrs).InnerText = "Test 2"; // SAPbobsCOM.BoFieldTypes.db_Alpha;
                        new2.SelectSingleNode(".//a:Size", nsmgrs).InnerText = "12"; // SAPbobsCOM.BoFieldTypes.db_Alpha;


                        // Limpiar XML Vacios                        
                        XmlNode oCleanInvoicesXML = DiServer.RemoveEmptyNodes(oInvoicesXML);
                        var oXmlReply = DiServer.AddInvoice(txtSessID, oCleanInvoicesXML.OuterXml);
                        string sRet = null;
                        if (Strings.InStr(oXmlReply.InnerXml, "<env:Fault>", (Microsoft.VisualBasic.CompareMethod)(0)) > 0)
                        {
                            sRet = "Error: " + oXmlReply.InnerXml;
                            TempData["MensajeDanger"] = "Error SAP: " + sRet;
                        }
                        else
                        {

                            TempData["MensajeSuccess"] = "Documento Generado en Sap, ya puede imprimir su OC";
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



            return RedirectToAction("index");
        }

        [Authorize(Roles = "Ordenes Compra")]
        public ActionResult EntregaC(Guid Fel_Unique, String MontoParcial, string Observaciones)
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
            var _DimensionCC = ConfigurationManager.AppSettings["DimensionCC"];
            var _DimensionFieldXml = "CostingCode";
            if (_DimensionCC != "1") _DimensionFieldXml += _DimensionCC;

            var _OCoriginal = Fel_Unique;

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

                    var oWFactura = _Db.FEL_Doc.Where(p => p.FEL_Unique == Fel_Unique).First();
                    var lstDepartamento = _Db.PPROV_Departamento.Where(p => p.DepartmentId == oWFactura.DepartamentoId).ToList();



                    txtSessID = DiServer.Login(_SAP_Server, _SAP_companydb, _SAP_DataBaseType, _SAP_dbuser, _SAP_dbpassword,
                                                _SAP_user, _SAP_password, _SAP_language, _SAP_Lincese);
                    if (!txtSessID.Contains("Error"))
                    {
                        var serverPath = Server.MapPath("~");
                        var lstAdjunto = _Db.FEL_DocAdjunto.Where(p => p.FEL_Unique == oWFactura.FEL_Unique).OrderByDescending(p => p.AdjuntoFecha).ToList();
                        String noAttach = "";

                        var oInvoicesXML = DiServer.GetEmpySchema(txtSessID, "oPurchaseDeliveryNotes");
                        // definir NameSapce
                        XmlNamespaceManager nsmgr = new XmlNamespaceManager(oInvoicesXML.NameTable);
                        nsmgr.AddNamespace("a", oInvoicesXML.DocumentElement.NamespaceURI);

                        // Tipo Documento
                        oInvoicesXML.SelectSingleNode("//a:Object", nsmgr).InnerText = "oPurchaseDeliveryNotes";

                        // Encabezado
                        Console.WriteLine("Encabezado");
                        var oBpCode = oInvoicesXML.SelectNodes("//a:Series", nsmgr);
                        oBpCode.Item(0).InnerText = _SAP_OCSerie;
                        //oInvoice.BPL_IDAssignedToInvoice = 1;  Requerido Solo multi-empresa
                        oInvoicesXML.SelectNodes("//a:NumAtCard", nsmgr).Item(0).InnerText = oWFactura.Referencia;
                        oInvoicesXML.SelectNodes("//a:CardCode", nsmgr).Item(0).InnerText = oWFactura.CardCode;
                        //oInvoicesXML.SelectNodes("//a:CardName", nsmgr).Item(0).InnerText = oWFactura.Rece_Nombre;
                        //oInvoicesXML.SelectNodes("//a:DocDate", nsmgr).Item(0).InnerText = oWFactura.FechaEmision.ToString("yyyyMMdd");
                        oInvoicesXML.SelectNodes("//a:DocDate", nsmgr).Item(0).InnerText = DateTime.Now.ToString("yyyyMMdd");
                        //oInvoicesXML.SelectNodes("//a:SalesPersonCode", nsmgr).Item(0).InnerText = oWFactura.SlpCode.Value.ToString();
                        //oInvoicesXML.SelectNodes("//a:DocDueDate", nsmgr).Item(0).InnerText =   oWFactura.FechaEmision.ToString("yyyyMMdd");
                        oInvoicesXML.SelectNodes("//a:DocDueDate", nsmgr).Item(0).InnerText = DateTime.Now.ToString("yyyyMMdd");
                        if (oWFactura.Tipo_Detalle != "Servicio")
                        {
                            oInvoicesXML.SelectNodes("//a:DocType", nsmgr).Item(0).InnerText = SAPbobsCOM.BoDocumentTypes.dDocument_Items.ToString();
                        }
                        else
                        {
                            oInvoicesXML.SelectNodes("//a:DocType", nsmgr).Item(0).InnerText = SAPbobsCOM.BoDocumentTypes.dDocument_Service.ToString();
                        }
                        //if (!string.IsNullOrEmpty(noAttach))
                        //{
                        //    oInvoicesXML.SelectNodes("//a:AttachmentEntry", nsmgr).Item(0).InnerText = noAttach;
                        //}

                        oInvoicesXML.SelectNodes("//a:DocCurrency", nsmgr).Item(0).InnerText = (oWFactura.Moneda == "GTQ") ? "QTZ" : oWFactura.Moneda;                        

                        if (oWFactura.Comentario == null) oWFactura.Comentario = "";
                        if (MontoParcial != null) oWFactura.Comentario = Observaciones;

                            if (oWFactura.Comentario.Length > 250)
                        {
                            oInvoicesXML.SelectNodes("//a:Comments", nsmgr).Item(0).InnerText = oWFactura.Comentario.Substring(0, 250);
                        }
                        else
                        {
                            oInvoicesXML.SelectNodes("//a:Comments", nsmgr).Item(0).InnerText = oWFactura.Comentario;
                        }

                        //oWFactura.Rece_Direccion = oWFactura.Rece_Direccion.PadLeft(254, ' ');
                        //oInvoicesXML.SelectNodes("//a:Address", nsmgr).Item(0).InnerText = oWFactura.Rece_Direccion.Substring(0, 254);

                        // User Fields
                        Console.WriteLine("UserFields");
                        oInvoicesXML.SelectNodes("//a:U_FacNit", nsmgr).Item(0).InnerText = oWFactura.Nit.ToString();
                        try { oInvoicesXML.SelectNodes("//a:U_TIPO_DOCUMENTO", nsmgr).Item(0).InnerText = "ZZ"; } catch { }

                        oInvoicesXML.SelectNodes("//a:U_WW_SyncId", nsmgr).Item(0).InnerText = oWFactura.FEL_Unique.ToString();
                        oInvoicesXML.SelectNodes("//a:U_WW_SyncDate", nsmgr).Item(0).InnerText = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        oInvoicesXML.SelectNodes("//a:U_WW_SyncNumero", nsmgr).Item(0).InnerText = oWFactura.Correlativo.ToString();

                        oInvoicesXML.SelectNodes("//a:U_WW_Propietario", nsmgr).Item(0).InnerText = TraeNombreUsuario(oWFactura.CreadoPor);
                        oInvoicesXML.SelectNodes("//a:U_WW_EnviaAuto", nsmgr).Item(0).InnerText = oWFactura.EnvioAuto.Value.ToString("yyyy-MM-dd HH:mm:ss");
                        oInvoicesXML.SelectNodes("//a:U_WW_Autoriza", nsmgr).Item(0).InnerText = TraeNombreUsuario(oWFactura.AutorizadoPor);
                        oInvoicesXML.SelectNodes("//a:U_WW_AutorizaEl", nsmgr).Item(0).InnerText = oWFactura.ActualizadoEl.Value.ToString("yyyy-MM-dd HH:mm:ss");
                        oInvoicesXML.SelectNodes("//a:U_WW_GeneradoEl", nsmgr).Item(0).InnerText = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        if (lstDepartamento.Count == 1) oInvoicesXML.SelectNodes("//a:U_WW_Departamento", nsmgr).Item(0).InnerText = lstDepartamento[0].DepartmentName;
                        Console.WriteLine("Grupo 1");

                        // detalle
                        Console.WriteLine("Mandando Detalle");
                        var _contadorLineaSap = 1;
                        // get ref to the Document_Lines
                        XmlNode oDocumentLines = oInvoicesXML.SelectSingleNode("//a:Document_Lines", nsmgr);
                        // get the first row 
                        XmlNode oFirstRow = oDocumentLines.FirstChild;


                        var detalleOrdenSAP = vServicio.List_SapOPORDetalleSync(oWFactura.SyncSapId);

                        foreach (var _ItemFactura in detalleOrdenSAP)
                        {
                            if (_contadorLineaSap == 1)
                            {
                                if (oWFactura.Tipo_Detalle == "Servicio")
                                {
                                    oFirstRow.SelectSingleNode("//a:ItemDescription", nsmgr).InnerXml = _ItemFactura.Dscription;
                                    oFirstRow.SelectSingleNode("//a:AccountCode", nsmgr).InnerXml = _ItemFactura.AcctCode;
                                    oFirstRow.SelectSingleNode("//a:BaseEntry", nsmgr).InnerXml = _ItemFactura.DocEntry.ToString();
                                    oFirstRow.SelectSingleNode("//a:BaseType", nsmgr).InnerXml = "22";
                                    oFirstRow.SelectSingleNode("//a:BaseLine", nsmgr).InnerXml = _ItemFactura.LineNum.ToString();
                                    oFirstRow.SelectSingleNode("//a:PriceAfterVAT", nsmgr).InnerXml = _ItemFactura.PriceAfVAT.ToString();
                                    if (MontoParcial != null) { oFirstRow.SelectSingleNode("//a:PriceAfterVAT", nsmgr).InnerXml = MontoParcial; }
                                    
                                    
                                }
                                else
                                {
                                    oFirstRow.SelectSingleNode("//a:ItemCode", nsmgr).InnerXml = _ItemFactura.ItemCode;
                                    oFirstRow.SelectSingleNode("//a:PriceAfterVAT", nsmgr).InnerXml = _ItemFactura.PriceAfVAT.ToString();
                                }
                                if (!String.IsNullOrEmpty(_ItemFactura.OcrCode)) oFirstRow.SelectSingleNode("//a:" + _DimensionFieldXml, nsmgr).InnerXml = _ItemFactura.OcrCode;
                                oFirstRow.SelectSingleNode("//a:Quantity", nsmgr).InnerXml = _ItemFactura.Quantity.ToString();
                                oFirstRow.SelectSingleNode("//a:WarehouseCode", nsmgr).InnerXml = oWFactura.Bodega;

                                oFirstRow.SelectSingleNode("//a:TaxCode", nsmgr).InnerXml = _ItemFactura.TaxCode;
                            }
                            else
                            {

                                // copy the first row the th new one -> for getting the same structure
                                XmlNode oNewRow = oFirstRow.CloneNode(true);
                                // update the new row
                                if (oWFactura.Tipo_Detalle == "Servicio")
                                {

                                    oNewRow.SelectSingleNode("//a:ItemDescription", nsmgr).InnerXml = _ItemFactura.Dscription;
                                    oNewRow.SelectSingleNode("//a:AccountCode", nsmgr).InnerXml = _ItemFactura.AcctCode;
                                    oNewRow.SelectSingleNode("//a:BaseEntry", nsmgr).InnerXml = _ItemFactura.DocEntry.ToString();
                                    oNewRow.SelectSingleNode("//a:BaseType", nsmgr).InnerXml = "22";
                                    oNewRow.SelectSingleNode("//a:BaseLine", nsmgr).InnerXml = _ItemFactura.LineNum.ToString();
                                    oNewRow.SelectSingleNode("//a:PriceAfterVAT", nsmgr).InnerXml = _ItemFactura.PriceAfVAT.ToString();
                                    if (MontoParcial != null) { oNewRow.SelectSingleNode("//a:PriceAfterVAT", nsmgr).InnerXml = MontoParcial; }
                                }
                                else
                                {
                                    oNewRow.SelectSingleNode("//a:ItemCode", nsmgr).InnerXml = _ItemFactura.ItemCode;
                                    oNewRow.SelectSingleNode("//a:PriceAfterVAT", nsmgr).InnerXml = _ItemFactura.PriceAfVAT.ToString();
                                }
                                if (!String.IsNullOrEmpty(_ItemFactura.OcrCode)) oNewRow.SelectSingleNode("//a:" + _DimensionFieldXml, nsmgr).InnerXml = _ItemFactura.OcrCode;

                                oNewRow.SelectSingleNode("//a:Quantity", nsmgr).InnerXml = _ItemFactura.Quantity.ToString();
                                oNewRow.SelectSingleNode("//a:WarehouseCode", nsmgr).InnerXml = oWFactura.Bodega;
                                oNewRow.SelectSingleNode("//a:TaxCode", nsmgr).InnerXml = _ItemFactura.TaxCode;
                                // add the new row to the DocumentLines object
                                oDocumentLines.AppendChild(oNewRow);
                            }


                            _contadorLineaSap += 1;
                        }


                        // Limpiar XML Vacios                        
                        XmlNode oCleanInvoicesXML = DiServer.RemoveEmptyNodes(oInvoicesXML);



                        // Add Quotation
                        XmlDocument oXmlReply;
                        if (vServicio.List_SapOPDNSync(oWFactura.FEL_Unique.ToString()).Count == 0 || MontoParcial !=null)
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

                                TempData["XmlData"] = oCleanInvoicesXML.OuterXml;
                                TempData["XmlDataRet"] = oXmlReply.OuterXml;

                                TempData["MensajeDanger"] = "Error SAP: " + sRet;
                            }
                            else
                            {
                                // saves the Quotation key
                                String invoiceDocNum = oXmlReply.FirstChild.FirstChild.InnerText;
                                // Actualizacion de Web y Busqueda Factura
                                oWFactura.EntregaEl = DateTime.Now;
                                var FacturasEncontrada = vServicio.List_SapOPDNSync(oWFactura.FEL_Unique.ToString());
                                oWFactura.EntregaNo = FacturasEncontrada[0].DocNum;
                                oWFactura.EntregaPor = User.Identity.Name;
                                _Db.SaveChanges();
                                TempData["MensajeSuccess"] = "Entrega Generado en Sap";
                            }
                        }
                        else
                        {
                            // Actualizacion de Web y Busqueda Factura
                            oWFactura.EntregaEl = DateTime.Now;
                            var FacturasEncontrada = vServicio.List_SapOPDNSync(oWFactura.FEL_Unique.ToString());
                            oWFactura.EntregaNo = FacturasEncontrada[0].DocNum;
                            oWFactura.EntregaPor = User.Identity.Name;
                            _Db.SaveChanges();
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

            AgregarHistorico(Fel_Unique, "Aprobado", "Generado", "Envio", "");

            return RedirectToAction("Emitir", new { id = _OCoriginal });
        }

        public ActionResult CerrarEntrega(Guid Fel_Unique)
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

            var _OCoriginal = Fel_Unique;

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

                    var oWFactura = _Db.FEL_Doc.Where(p => p.FEL_Unique == Fel_Unique).First();
                    var lstDepartamento = _Db.PPROV_Departamento.Where(p => p.DepartmentId == oWFactura.DepartamentoId).ToList();



                    txtSessID = DiServer.Login(_SAP_Server, _SAP_companydb, _SAP_DataBaseType, _SAP_dbuser, _SAP_dbpassword,
                                                _SAP_user, _SAP_password, _SAP_language, _SAP_Lincese);
                    if (!txtSessID.Contains("Error"))
                    {
                        String xmlString = @"<BOM>
    <BO>
     <AdmInfo>
      <Object>oPurchaseDeliveryNotes</Object>
     </AdmInfo>
     <QueryParams>
      <DocEntry>1247</DocEntry>
     </QueryParams>    
    </BO>
   </BOM>";
                        XmlDocument oXmlReply;
                        oXmlReply = DiServer.CloseInvoice(txtSessID, xmlString);
                        string sRet = null;

                        // check for error
                        if (Strings.InStr(oXmlReply.InnerXml, "<env:Fault>", (Microsoft.VisualBasic.CompareMethod)(0)) > 0)
                        { // And (Not (sret.StartsWith("Error"))) Then
                            sRet = "Error: " + oXmlReply.InnerXml;
                            TempData["MensajeDanger"] = "Error SAP: " + sRet;
                        }
                        else
                        {
                            // saves the Quotation key
                            String invoiceDocNum = oXmlReply.FirstChild.FirstChild.InnerText;
                            // Actualizacion de Web y Busqueda Factura
                            //oWFactura.EntregaEl = DateTime.Now;
                            //var FacturasEncontrada = vServicio.List_SapOPDNSync(oWFactura.FEL_Unique.ToString());
                            //oWFactura.EntregaNo = FacturasEncontrada[0].DocNum;
                            //oWFactura.EntregaPor = User.Identity.Name;
                            //_Db.SaveChanges();
                            TempData["MensajeSuccess"] = "Entrega Cerrada en Sap";
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

            AgregarHistorico(Fel_Unique, "Aprobado", "Generado", "Envio", "");

            return RedirectToAction("Emitir", new { id = _OCoriginal });
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

        [Authorize(Roles = "Ordenes Compra")]
        public ActionResult EnviarAprobacion(String Fel_Unique)
        {
            if (Session["EmpresaSelId"] == null)
            {
                Wareways.PortalProv.Servicios.ServicioSeguridad.CheckSession(User.Identity.Name);
            }

            var _EmpresaId = Int32.Parse(ConfigurationManager.AppSettings["WWPortal_EmpresaId"]);
            var _Unique = Guid.Parse(Fel_Unique);
            var oDoc = _Db.FEL_Doc.Where(p => p.FEL_Unique == _Unique).ToList();
            if (oDoc.Count == 1)
            {
                if (oDoc[0].Estado == "Borrador" || oDoc[0].Estado == "Revision")
                {
                    String _Errores = ValidacionesEnvio(oDoc[0]);
                    if (_Errores.Length != 0)
                    {
                        TempData["MensajeDanger"] = _Errores;
                        return RedirectToAction("index");
                    }
                    else
                    {
                        oDoc[0].Estado = "Autorizar";
                        oDoc[0].ActualizadoPor = User.Identity.Name;
                        oDoc[0].ActualizadoEl = DateTime.Now;
                        oDoc[0].EnvioAuto = DateTime.Now;
                        _Db.SaveChanges();
                        TempData["MensajeSuccess"] = "Documento Enviado para Aprobación";
                        AgregarHistorico(_Unique, "Borrador", "Autorizar", "Envio", "");
                    }


                }
            }
            return RedirectToAction("index");
        }


        [Authorize(Roles = "Ordenes Compra")]
        public ActionResult EnviarRevision(String Fel_Unique)
        {
            if (Session["EmpresaSelId"] == null)
            {
                Wareways.PortalProv.Servicios.ServicioSeguridad.CheckSession(User.Identity.Name);
            }

            var _EmpresaId = Int32.Parse(ConfigurationManager.AppSettings["WWPortal_EmpresaId"]);
            var _Unique = Guid.Parse(Fel_Unique);
            var oDoc = _Db.FEL_Doc.Where(p => p.FEL_Unique == _Unique).ToList();
            if (oDoc.Count == 1)
            {
                if (oDoc[0].Estado == "Borrador")
                {
                    String _Errores = ValidacionesEnvio(oDoc[0]);
                    if (_Errores.Length != 0)
                    {
                        TempData["MensajeDanger"] = _Errores;
                        return RedirectToAction("index");
                    }
                    else
                    {
                        oDoc[0].Estado = "Revision";
                        oDoc[0].ActualizadoPor = User.Identity.Name;
                        oDoc[0].ActualizadoEl = DateTime.Now;
                        oDoc[0].EnvioAuto = DateTime.Now;
                        _Db.SaveChanges();
                        TempData["MensajeSuccess"] = "Documento Enviado para Revision";
                        AgregarHistorico(_Unique, "Borrador", "Autorizar", "Envio", "");
                    }


                }
            }
            return RedirectToAction("index");
        }

        [Authorize(Roles = "Ordenes Compra")]
        public ActionResult Cancelar(String Fel_Unique, String comentario)
        {
            if (Session["EmpresaSelId"] == null)
            {
                Wareways.PortalProv.Servicios.ServicioSeguridad.CheckSession(User.Identity.Name);
            }

            var _EmpresaId = Int32.Parse(ConfigurationManager.AppSettings["WWPortal_EmpresaId"]);
            var _Unique = Guid.Parse(Fel_Unique);
            var oDoc = _Db.FEL_Doc.Where(p => p.FEL_Unique == _Unique).ToList();
            if (oDoc.Count == 1)
            {
                if (oDoc[0].Estado == "Borrador")
                {
                    oDoc[0].Estado = "Cancelado";
                    oDoc[0].ActualizadoPor = User.Identity.Name;
                    oDoc[0].ActualizadoEl = DateTime.Now;
                    oDoc[0].EnvioAuto = DateTime.Now;
                    _Db.SaveChanges();
                    TempData["MensajeSuccess"] = "Documento Cancelado con Exito";
                    AgregarHistorico(_Unique, "Borrador", "Cancelado", "Envio", comentario);
                }
            }
            return RedirectToAction("index");
        }

        private string ValidacionesEnvio(FEL_Doc fEL_Doc)
        {
            String _Retona = "";
            if (fEL_Doc.Total == 0 || fEL_Doc.FEL_DocDetalle.Count == 0)
            {
                _Retona += "No se puede Enviar Documentos sin lineas o Total 0 ,";
            }
            if (fEL_Doc.Tipo_Detalle == "Servicio" && fEL_Doc.FEL_DocDetalle.Where(P => P.CuentaContable == null).Count() > 0)
            {
                _Retona += "No se puede Enviar Documentos de Serivio sin cuentas contables ,";
            }
            if (ConfigurationManager.AppSettings["ObligatorioCC"] == "S" && fEL_Doc.FEL_DocDetalle.Where(P => P.CentroCosto == null).Count() > 0)
            {
                _Retona += "No se puede Enviar Documentos sin Centros de Costo ,";
            }

            return _Retona;
        }

        [Authorize(Roles = "Ordenes Compra")]
        public ActionResult RechazarAprobacion(String Fel_Unique, String comentarioRech)
        {
            if (Session["EmpresaSelId"] == null)
            {
                Wareways.PortalProv.Servicios.ServicioSeguridad.CheckSession(User.Identity.Name);
            }

            var _EmpresaId = Int32.Parse(ConfigurationManager.AppSettings["WWPortal_EmpresaId"]);
            var _Unique = Guid.Parse(Fel_Unique);
            var oDoc = _Db.FEL_Doc.Where(p => p.FEL_Unique == _Unique).ToList();
            if (oDoc.Count == 1)
            {
                var _EstadoAnterio = oDoc[0].Estado;
                if (oDoc[0].Estado == "Autorizar" || oDoc[0].Estado == "Aprobado" || 
                    oDoc[0].Estado == "Revision" || oDoc[0].Estado == "Revision")
                {
                    oDoc[0].Estado = "Borrador";
                    oDoc[0].ActualizadoPor = User.Identity.Name;
                    oDoc[0].ActualizadoEl = DateTime.Now;
                    oDoc[0].AutorizadoPor = null;
                    oDoc[0].AutorizadoEl = null;
                    oDoc[0].EnvioAuto = null;

                    _Db.SaveChanges();
                    TempData["MensajeSuccess"] = "Documento Rechazado para Aprobación";
                    AgregarHistorico(_Unique, _EstadoAnterio, "Borrador", "Rechazo", comentarioRech);
                }
            }
            return RedirectToAction("index");
        }

        [Authorize(Roles = "Ordenes Compra")]
        public ActionResult AprobarDocto(String Fel_Unique)
        {
            if (Session["EmpresaSelId"] == null)
            {
                Wareways.PortalProv.Servicios.ServicioSeguridad.CheckSession(User.Identity.Name);
            }

            var _EmpresaId = Int32.Parse(ConfigurationManager.AppSettings["WWPortal_EmpresaId"]);
            var _Unique = Guid.Parse(Fel_Unique);
            var oDoc = _Db.FEL_Doc.Where(p => p.FEL_Unique == _Unique).ToList();
            if (oDoc.Count == 1)
            {

                int MONTOMAX = 999999999;
                try { MONTOMAX = Int32.Parse(ConfigurationManager.AppSettings["MontoAprobacionEspecialQTZ"]); } catch {  }

                if (oDoc[0].Total >= MONTOMAX)
                {
                    // lo mando a Segundo Proceso de Autorizacion
                    if (oDoc[0].Estado == "Autorizar")
                    {
                        oDoc[0].Estado = "AutoEspe";
                        oDoc[0].AutorizadoPor = User.Identity.Name;
                        oDoc[0].AutorizadoEl = DateTime.Now;
                        _Db.SaveChanges();
                        TempData["MensajeSuccess"] = "Documento a Auto Especial Con exito";
                        AgregarHistorico(_Unique, "AutoEspe", "Aprobado", "Envio", "");
                    }
                }
                else
                {
                    // solo requiere una autorizacion por el monto
                    if (oDoc[0].Estado == "Autorizar")
                    {
                        oDoc[0].Estado = "Aprobado";
                        oDoc[0].AutorizadoPor = User.Identity.Name;
                        oDoc[0].AutorizadoEl = DateTime.Now;
                        _Db.SaveChanges();
                        TempData["MensajeSuccess"] = "Documento Autorizado Con exito";
                        AgregarHistorico(_Unique, "Autorizar", "Aprobado", "Envio", "");
                    }
                }              
            }
            return RedirectToAction("index");
        }


        [Authorize(Roles = "Ordenes Compra")]
        public ActionResult AprEspeDocto(String Fel_Unique)
        {
            if (Session["EmpresaSelId"] == null)
            {
                Wareways.PortalProv.Servicios.ServicioSeguridad.CheckSession(User.Identity.Name);
            }

            var _EmpresaId = Int32.Parse(ConfigurationManager.AppSettings["WWPortal_EmpresaId"]);
            var _Unique = Guid.Parse(Fel_Unique);
            var oDoc = _Db.FEL_Doc.Where(p => p.FEL_Unique == _Unique).ToList();
            if (oDoc.Count == 1)
            {

                // solo requiere una autorizacion por el monto
                if (oDoc[0].Estado == "AutoEspe")
                {
                    oDoc[0].Estado = "Aprobado";
                    oDoc[0].AutorizadoPor = User.Identity.Name;
                    oDoc[0].AutorizadoEl = DateTime.Now;
                    _Db.SaveChanges();
                    TempData["MensajeSuccess"] = "Documento Autorizado Con exito";
                    AgregarHistorico(_Unique, "Autorizar", "Aprobado", "Envio", "");
                }

            }
            return RedirectToAction("index");
        }

        [Authorize(Roles = "Ordenes Compra")]
        public ActionResult Emitir(OC_EmitirModel _modelo, String uiBT_SubmitGrabar, String uiBT_SubmitAgregarLinea, String uiBT_EliminarFila, String uiBT_SubmitEliminarLineas, Guid? id, String uiBTEDITAR_CORREO, string uiBT_UploadFileOC, HttpPostedFileBase fileoc, string uiBT_SubmitAgregarLineaSubmit, string uiBT_SubmitAgregarExcel, string uiBT_SubmitAgregarExcelBien)
        {
            ViewBag.NombreBotonTodasTiendas = "Todas Tienda";
            try { ViewBag.NombreBotonTodasTiendas = ConfigurationManager.AppSettings["OC_NombreBotonCC_Todos"]; } catch { }
            

            if (Session["EmpresaSelId"] == null)
            {
                Wareways.PortalProv.Servicios.ServicioSeguridad.CheckSession(User.Identity.Name);
            }
            var _EmpresaId = Int32.Parse(ConfigurationManager.AppSettings["WWPortal_EmpresaId"]);


            try
            {
                // Elimiar Datos si Cambia de Tipo Documento
                if (_modelo.EliminarDetalle == true)
                {
                    _modelo.Detalle.Clear();
                    _modelo.EliminarDetalle = false;
                }

                // Inicializar
                var _listaEstablecimiento = (from d in vServicio.List_Almacenes() select d).ToList();
                ViewBag.VB_listaEstablecimiento = _listaEstablecimiento;
                var _listaMoneda = (from d in vServicio.List_SapMonedas() select d).ToList();
                ViewBag.VB_listaMoneda = _listaMoneda;
                var _listaCliente = (from d in vServicio.List_SapProveedor() select d).ToList();
                _listaCliente.Add(new oProveedorSap { CardCode = "", CardName = "" });
                ViewBag.VB_listaCliente = _listaCliente;

                var _listaUnidades = (from d in vServicio.List_SaUnidades() select d).ToList();
                ViewBag.VB_listaUnidades = _listaUnidades;
                var _ListaCentoCosto = (from d in vServicio.List_SapCentroCosto() select d).ToList();
                _ListaCentoCosto.Add(new oCentroCosto { ocrCode = "", ocrName = "" });
                ViewBag.VB_listaCentoCosto = _ListaCentoCosto.OrderBy(p => p.ocrCode).ToList();
             
                


                var _ListaDeptosUsario = (from l in vServicio.GetUserDepartment(User.Identity.Name) where l.Empresa_Id == _EmpresaId select l);
                ViewBag.VB_Deptos = _ListaDeptosUsario;



                var _ListaImpuesto = vServicio.List_SapImpuesto();
                ViewBag.VB_Impuesto = _ListaImpuesto;
                var _ListaCuentaServicio = vServicio.List_SapCuentasServicio();
                var _ListaCuentaServicioTodas = vServicio.List_SapCuentasServicioTodas();
                //var _ListaCuentaServicioTodas = vServicio.List_SapCuentasServicio();
                _ListaCuentaServicio.Add(new oCuentaContableSap { AcctCode = "", AcctName = "", ActId = "" });
                ViewBag.VB_CuentaServicio = _ListaCuentaServicio.OrderBy(p => p.AcctCode).ToList();
                ViewBag.VB_CuentaServicioTodas = _ListaCuentaServicioTodas.OrderBy(p => p.AcctCode).ToList();


                // Si tiene Id llena los Datos
                if (id != null && _modelo.Fel_Unique == Guid.Empty)
                {
                    _modelo.ModalEditar = new OC_DetalleModel();
                    _modelo.ModalNuevo = new OC_DetalleModel();
                    _modelo.ModalNuevo.Descuentos = 0;
                    _modelo.ModalNuevo.Cantidad = 0;
                    

                    _modelo = (from l in _Db.FEL_Doc.AsNoTracking().Where(p => p.FEL_Unique == id)
                               select new OC_EmitirModel
                               {
                                   Fel_Unique = l.FEL_Unique,
                                   Establecimiento_Codigo = l.Bodega,
                                   Fel_Correlativo = l.Correlativo,
                                   TipoDoc = l.TipoDoc,
                                   FechaEmision = l.FechaEmision,
                                   FechaEntrega = l.FechaEntrega,
                                   Nit = l.Nit,
                                   CardName = l.Cardname,
                                   CardCode = l.CardCode,
                                   Moneda = l.Moneda,
                                   TasaCambio = (decimal)l.TasaCambio,
                                   Total = (decimal)l.Total,
                                   Tipo_Detalle = l.Tipo_Detalle,
                                   Estado = l.Estado,
                                   Referencia = l.Referencia,
                                   EmpresaId = l.EmpresaId,
                                   Departamento = l.DepartamentoId,
                                   CreadoPor = l.CreadoPor,
                                   CreadoEl = l.CreadoEl,
                                   ActualizadoPor = l.ActualizadoPor,
                                   ActualizadoEl = l.ActualizadoEl,
                                   Comentario = l.Comentario,
                                   AutorizadoPor = l.AutorizadoPor,
                                   AutorizadoEl = l.AutorizadoEl,
                                   EnviadoAuto = l.EnvioAuto,
                                   EntregaEl = l.EntregaEl,
                                   EntregaNo = l.EntregaNo,
                                   EntregaPor = l.EntregaPor

                               }).First();

                    _modelo.Detalle = (from l in _Db.FEL_DocDetalle
                                       where l.FEL_Unique == id
                                       select new OC_DetalleModel
                                       {
                                           Cantidad = (Decimal)l.Cantidad,
                                           DateAudit = l.DateAudit,
                                           Descripcion = l.Descripcion,
                                           Descuentos = (Decimal)l.Descuentos,
                                           Fel_Unique = l.FEL_Unique,
                                           Impuestos = (Decimal)l.Impuestos,
                                           Linea = l.Linea,
                                           TipoImpuesto = l.TipoImpuesto,
                                           PrecioUnitario = (Decimal)l.PrecioUnitario,
                                           TipoDet = l.TipoDet,
                                           TotalLinea = (Decimal)l.TotalLinea,
                                           UnidadMedida = l.UnidadMedida,
                                           UniqueId = Guid.NewGuid(),
                                           UserNameAudit = l.UserNameAudit,
                                           CuentaContable = l.CuentaContable,
                                           CentoCosto = l.CentroCosto
                                       }).ToList();

                    if (_modelo.Detalle.Count > 0)
                    {
                        var Doc =  _Db.FEL_Doc.Where(p => p.FEL_Unique == id).ToList();

                        /// Entregas Parciales
                        try
                        {
                            var lstDetalleEN = vServicio.ObtenerDetalleOC_Parcial(Doc[0].SyncSapId).ToList();                            
                            var lstSaldo = Doc[0].Total - lstDetalleEN.Where(p => p.CANCELED == "N").Sum(p => p.Monto);
                            _modelo.EntregaSaldo = lstSaldo;
                            Doc[0].EntregaSaldo = _modelo.EntregaSaldo;

                            _modelo.EntregaMultiple = " ";
                            foreach (var item in lstDetalleEN.Where(p => p.CANCELED == "N").ToList())
                            {
                                _modelo.EntregaMultiple += item.DocNum + ",";
                            }
                            _modelo.EntregaMultiple = _modelo.EntregaMultiple.Remove(_modelo.EntregaMultiple.Length - 1, 1).TrimStart().TrimEnd();
                            Doc[0].EntregaMultiple = _modelo.EntregaMultiple;
                            _Db.SaveChanges();
                        }
                        catch { 
                        }
                        
                    }
                }

                if (_modelo.Estado == "Aprobado" || _modelo.Estado == "Generado" || _modelo.Estado == "Autorizar" || _modelo.Estado == "Cancelado" || _modelo.Estado == "AutoEspe" || _modelo.Estado == "Revision")
                {
                    _modelo.SoloLectura = true;
                }

                // Inactivo
                var _listaInactivo = vServicio.ObtenerCardCodeInactivo(_modelo.CardCode);
                if (_listaInactivo.Count == 1)
                {
                    _modelo.Inactivo = _listaInactivo[0].Inactivo;
                    _modelo.InactivoMensaje = _listaInactivo[0].InactivoMensaje;
                }


                //Primera Carga
                if (String.IsNullOrEmpty(_modelo.CardName) && _modelo.TipoDoc == null)
                {
                    //Valores por defecto
                    _modelo.FechaEmision = DateTime.Now;
                    _modelo.TipoDoc = "OC";
                    _modelo.Moneda = "QTZ";
                    _modelo.TasaCambio = 1;
                    _modelo.Estado = "Borrador";
                    _modelo.Nit = "";
                    _modelo.FechaEntrega = DateTime.Now;
                    if (_modelo.ModalNuevo == null)
                    {
                        _modelo.ModalNuevo = new OC_DetalleModel();
                        _modelo.ModalNuevo.UnidadMedida = "Unidad";
                        _modelo.ModalNuevo.TipoImpuesto = "IVA";
                    }
                    if (_modelo.ModalEditar == null)
                    {
                        _modelo.ModalEditar = new OC_DetalleModel();
                    }


                }

                // Si es AgregarLinea
                if (!string.IsNullOrEmpty(uiBT_SubmitAgregarLinea) || !string.IsNullOrEmpty(uiBT_SubmitAgregarLineaSubmit))
                {
                    if (_modelo.Tipo_Detalle == "Bien")
                    {
                        try
                        {
                            var elementos = _modelo.ModalNuevo.Descripcion.Split('|');
                            _modelo.ModalNuevo.Descripcion = elementos[0] + '|' + elementos[1];
                        }
                        catch { }
                    }
                    _modelo.ModalNuevo.UniqueId = Guid.NewGuid();
                    List<String> CCnuevos = new List<string>();
                    try { CCnuevos = _modelo.ModalNuevo.CentoCostoArray.Split(',').Where(p => p.Length > 0).ToList(); } catch { }
                    if (CCnuevos.Count < 2)
                    {
                        if (CCnuevos.Count == 1) _modelo.ModalNuevo.CentoCosto = CCnuevos[0];
                        _modelo.Detalle.Add(_modelo.ModalNuevo);
                    }
                    else
                    {
                        var TotalxLinea = Math.Round((_modelo.ModalNuevo.TotalLinea / CCnuevos.Count), 2);
                        var diferencia = _modelo.ModalNuevo.TotalLinea - (TotalxLinea * CCnuevos.Count);

                        var linea = 1;
                        foreach (var cc in CCnuevos)
                        {
                            Decimal ajuste = 0;
                            if (linea == CCnuevos.Count) ajuste = diferencia;
                            _modelo.Detalle.Add(new OC_DetalleModel
                            {
                                Cantidad = 1,
                                CentoCosto = cc,
                                CuentaContable = _modelo.ModalNuevo.CuentaContable,
                                Descripcion = _modelo.ModalNuevo.Descripcion,
                                UniqueId = Guid.NewGuid(),
                                PrecioUnitario = TotalxLinea + ajuste,
                                TipoImpuesto = _modelo.ModalNuevo.TipoImpuesto,
                                TotalLinea = TotalxLinea + ajuste,
                                UnidadMedida = _modelo.ModalNuevo.UnidadMedida
                            });
                            linea += 1;
                        }
                    }


                    _modelo.CollapseDatosGenerales = true;
                    ModelState.Clear();
                }

                //--------------------------------
                // Si es AgregarExcel Servicio
                //---------------------------
                if (!string.IsNullOrEmpty(uiBT_SubmitAgregarExcel))
                {
                    var error = "";
                    int row = 0;
                    if (_modelo.Estado == "Borrador" && _modelo.Fel_Unique != null && _modelo.importE != null)
                    {
                        string[] lines = _modelo.importE.Split('\n');

                        string[] fields;

                        List<OC_DetalleModel> lstAgregar = new List<OC_DetalleModel>();

                        Boolean formatoCorrecto = false;

                        try
                        {


                            foreach (string item in lines)
                            {
                                fields = item.Split('\t');
                                if (
                                    (row == 0 && fields[0] == "Cantidad") &&
                                    (row == 0 && fields[1] == "Descripcion") &&
                                    (row == 0 && fields[3] == "Precio") &&
                                    (row == 0 && fields[4] == "Impuesto") &&
                                    (row == 0 && fields[5] == "Cuenta Contable") &&
                                    (row == 0 && fields[6].Replace("\r", "") == "Centro Costo")
                                   ) { formatoCorrecto = true; }
                                if (formatoCorrecto == false)
                                {
                                    if (row == 0) { TempData["MensajeDanger"] = "Formato Incorrecto, Debe de incluir el encabezado"; }

                                }



                                if (formatoCorrecto && row > 0 && fields.Count() == 7)
                                {


                                    var nuevo = new OC_DetalleModel();


                                    nuevo.UniqueId = Guid.NewGuid();
                                    nuevo.Cantidad = Decimal.Parse(fields[0].Replace("\r", ""));
                                    nuevo.CentoCosto = fields[6].Replace("\r", "");
                                    nuevo.CuentaContable = fields[5].Replace("\r", "");
                                    nuevo.Descripcion = fields[1].Replace("\r", "");


                                    nuevo.PrecioUnitario = decimal.Parse(fields[3]);
                                    nuevo.TipoImpuesto = fields[4];
                                    nuevo.TotalLinea = decimal.Parse(fields[3]) * Decimal.Parse(fields[0]);
                                    nuevo.UnidadMedida = fields[2];
                                    nuevo.TipoDet = "S";
                                    nuevo.UserNameAudit = User.Identity.Name;
                                    nuevo.DateAudit = DateTime.Now;
                                    if (_ListaCentoCosto.Where(p => p.ocrCode == nuevo.CentoCosto).Count() == 0)
                                    {
                                        throw new Exception(string.Format("Centro de Costo {0} no valido", nuevo.CentoCosto));
                                    }
                                    if (_ListaCuentaServicio.Where(p => p.ActId == nuevo.CuentaContable).Count() == 0)
                                    {
                                        throw new Exception(string.Format("Cuenta Contable {0} no valido", nuevo.CuentaContable));
                                    }
                                    if (nuevo.TipoImpuesto == "EXE" || nuevo.TipoImpuesto == "IVA")
                                    {                                       
                                    }
                                    else {
                                        throw new Exception(string.Format("Tipo de Impuesto {0} no valido", nuevo.TipoImpuesto));
                                    }


                                    lstAgregar.Add(nuevo);
                                }

                                row++;

                            }
                            _modelo.Detalle.AddRange(lstAgregar);
                            _modelo.importE = "";


                        }
                        catch (Exception ex)
                        {
                            TempData["MensajeDanger"] = string.Format("Error Linea {0} --> {1}", row, ex.Message);
                        }

                    }



                }
                //--------------------------------
                // Si es AgregarExcel Bien
                //---------------------------

                if (!string.IsNullOrEmpty(uiBT_SubmitAgregarExcelBien))
                {
                    var error = "";
                    int row = 0;
                    if (_modelo.Estado == "Borrador" && _modelo.Fel_Unique != null && _modelo.importE != null)
                    {
                        string[] lines = _modelo.importE.Split('\n');

                        string[] fields;

                        List<OC_DetalleModel> lstAgregar = new List<OC_DetalleModel>();

                        Boolean formatoCorrecto = false;

                        try
                        {


                            foreach (string item in lines)
                            {
                                fields = item.Split('\t');
                                if (
                                    (row == 0 && fields[0] == "Cantidad") &&
                                    (row == 0 && fields[1] == "Descripcion") &&
                                    (row == 0 && fields[3] == "Precio") &&
                                    (row == 0 && fields[4] == "Impuesto") &&
                                    (row == 0 && fields[5] == "CodigoInventario") &&
                                    (row == 0 && fields[6].Replace("\r", "") == "Centro Costo") &&
                                    (row == 0 && fields[7].Replace("\r", "") == "Bodega")
                                   ) { formatoCorrecto = true; }
                                if (formatoCorrecto == false)
                                {
                                    if (row == 0) { TempData["MensajeDanger"] = "Formato Incorrecto, Debe de incluir el encabezado"; }

                                }



                                if (formatoCorrecto && row > 0 && fields.Count() == 8)
                                {


                                    var nuevo = new OC_DetalleModel();


                                    nuevo.UniqueId = Guid.NewGuid();
                                    nuevo.Cantidad = Decimal.Parse(fields[0].Replace("\r", ""));
                                    nuevo.CentoCosto = fields[6].Replace("\r", "");
                                    nuevo.Bodega = fields[7].Replace("\r", "");
                                    nuevo.Producto_Codigo = fields[5].Replace("\r", "");
                                    nuevo.Descripcion = fields[1].Replace("\r", "");


                                    nuevo.PrecioUnitario = decimal.Parse(fields[3]);
                                    nuevo.TipoImpuesto = fields[4];
                                    nuevo.TotalLinea = decimal.Parse(fields[3]) * Decimal.Parse(fields[0]);
                                    nuevo.UnidadMedida = fields[2];
                                    nuevo.TipoDet = "S";
                                    nuevo.UserNameAudit = User.Identity.Name;
                                    nuevo.DateAudit = DateTime.Now;
                                    if (_ListaCentoCosto.Where(p => p.ocrCode == nuevo.CentoCosto).Count() == 0)
                                    {
                                        throw new Exception(string.Format("Centro de Costo {0} no valido", nuevo.CentoCosto));
                                    }
                                    var lstProductos = vServicio.List_SapProductos(nuevo.Bodega);
                                    
                                    if (lstProductos.Where(p => p.ItemCode == nuevo.Producto_Codigo).Count() == 0)
                                    {
                                        throw new Exception(string.Format("Articulo {0} no valido", nuevo.CentoCosto));
                                    }
                                    var lstBodegas = vServicio.List_Almacenes();
                                    if (lstBodegas.Where(p => p.WhsCode== nuevo.Bodega).Count() == 0)
                                    {
                                        throw new Exception(string.Format("Bodega {0} no valido", nuevo.CentoCosto));
                                    }
                                    if (nuevo.TipoImpuesto == "EXE" || nuevo.TipoImpuesto == "IVA")
                                    {
                                    }
                                    else
                                    {
                                        throw new Exception(string.Format("Tipo de Impuesto {0} no valido", nuevo.TipoImpuesto));
                                    }



                                    lstAgregar.Add(nuevo);
                                }

                                row++;

                            }
                            _modelo.Detalle.AddRange(lstAgregar);
                            _modelo.importE = "";


                        }
                        catch (Exception ex)
                        {
                            TempData["MensajeDanger"] = string.Format("Error Linea {0} --> {1}", row, ex.Message);
                        }

                    }



                }


                // Si es EliminarLinea
                if (!string.IsNullOrEmpty(uiBT_EliminarFila))
                {
                    _modelo.Detalle = _modelo.Detalle.Where(p => p.UniqueId.ToString() != uiBT_EliminarFila).ToList();
                    _modelo.CollapseDatosGenerales = true;
                    ModelState.Clear();
                }

                try
                {
                    CalcularTotales_Impuestos_Varios(ref _modelo);
                }
                catch { }



                // Si es Grabar
                if (!string.IsNullOrEmpty(uiBT_SubmitGrabar))
                {
                    _modelo.CollapseDatosGenerales = false;
                    // Grabar Encabezado
                    var oTabla = new FEL_Doc();
                    if (_modelo.Fel_Unique != Guid.Empty)
                    {
                        oTabla = _Db.FEL_Doc.Where(p => p.FEL_Unique == _modelo.Fel_Unique).FirstOrDefault();
                    }
                    oTabla.Bodega = _modelo.Establecimiento_Codigo;
                    oTabla.TipoDoc = _modelo.TipoDoc;
                    oTabla.FechaEmision = _modelo.FechaEmision;
                    oTabla.FechaEntrega = _modelo.FechaEntrega;
                    oTabla.Nit = _modelo.Nit;
                    if (String.IsNullOrEmpty(oTabla.Nit)) oTabla.Nit = "";
                    oTabla.Cardname = _modelo.CardName;
                    oTabla.CardCode = _modelo.CardCode;
                    oTabla.Moneda = _modelo.Moneda;
                    if (_modelo.Moneda == "GTQ") { _modelo.TasaCambio = 1; };
                    oTabla.TasaCambio = _modelo.TasaCambio;
                    oTabla.Total = _modelo.Total;
                    oTabla.Tipo_Detalle = _modelo.Tipo_Detalle;
                    oTabla.Estado = _modelo.Estado;
                    oTabla.Referencia = _modelo.Referencia;
                    oTabla.EmpresaId = _modelo.EmpresaId;
                    oTabla.DepartamentoId = _modelo.Departamento;
                    oTabla.Comentario = _modelo.Comentario;

                    if (string.IsNullOrEmpty(_modelo.CreadoPor))
                    {
                        oTabla.CreadoPor = User.Identity.Name;
                        oTabla.CreadoEl = DateTime.Now;
                    }
                    oTabla.ActualizadoEl = DateTime.Now;
                    oTabla.ActualizadoPor = User.Identity.Name;
                    // Lo Agrega Si es Nuevo
                    if (String.IsNullOrEmpty(_modelo.CardCode)) throw new Exception("El codigo de Proveedor es Requerido");
                    if (_modelo.Fel_Unique == Guid.Empty)
                    {
                        oTabla.FEL_Unique = Guid.NewGuid();

                        _Db.FEL_Doc.Add(oTabla);
                        AgregarHistorico(oTabla.FEL_Unique, "", "Creado", "Inicial", "");
                    }
                    oTabla.ActivoFijo = false;

                    _Db.SaveChanges();




                    // Borrador Anterior                    
                    _Db.FEL_DocDetalle.RemoveRange(_Db.FEL_DocDetalle.Where(p => p.FEL_Unique == _modelo.Fel_Unique));
                    _Db.SaveChanges();



                    // Grabar Detalle
                    var lstprov = _Db.V_PPROV_Proveedor.Where(p => p.CardCode == _modelo.CardCode);


                    var _ContadorLinea = 1;
                    foreach (var _ItemDetalle in _modelo.Detalle)
                    {
                        //Agregar a DocDetalle
                        var _NuevoD = new FEL_DocDetalle();
                        _NuevoD.Linea = _ContadorLinea;
                        _NuevoD.FEL_Unique = oTabla.FEL_Unique;
                        _NuevoD.Cantidad = _ItemDetalle.Cantidad;
                        _NuevoD.TipoDet = oTabla.Tipo_Detalle.Substring(0, 1);
                        _NuevoD.Descripcion = _ItemDetalle.Descripcion;
                        _NuevoD.UnidadMedida = _ItemDetalle.UnidadMedida;
                        _NuevoD.PrecioUnitario = _ItemDetalle.PrecioUnitario;
                        _NuevoD.Descuentos = _ItemDetalle.Descuentos;
                        _NuevoD.TotalLinea = _ItemDetalle.TotalLinea;
                        _NuevoD.Impuestos = _ItemDetalle.Impuestos;
                        _NuevoD.DateAudit = DateTime.Now;
                        _NuevoD.UserNameAudit = User.Identity.Name;
                        _NuevoD.TipoImpuesto = _ItemDetalle.TipoImpuesto;
                        if (lstprov.First().VatGroup == "EXE") _NuevoD.TipoImpuesto = "EXE";
                        _NuevoD.CentroCosto = _ItemDetalle.CentoCosto;
                        _NuevoD.CuentaContable = _ItemDetalle.CuentaContable;
                        oTabla.FEL_DocDetalle.Add(_NuevoD);

                        var lstCC_ActivoFijoDetect = ConfigurationManager.AppSettings["CuentaContableActivoFijo"].Split(',');
                        if (lstCC_ActivoFijoDetect.Contains(_ItemDetalle.CuentaContable))
                        {
                            oTabla.ActivoFijo = true;
                            _Db.SaveChanges();
                        }

                        _ContadorLinea += 1;
                    }



                    _Db.SaveChanges();
                    // Corrector de Frases



                    TempData["MensajeSuccess"] = "Datos Grabados Con Exito";
                    return RedirectToAction("Emitir", "Ordenescompra", new { id = oTabla.FEL_Unique });

                }

                // Permisos de Autorizacion
                if (_modelo.Estado == "Autorizar")
                {
                    ViewBag.EsAutorizador = _ListaDeptosUsario.Where(p => p.DepartmentId == _modelo.Departamento).First().Autorizar;
                }

                if (_modelo.Estado == "AutoEspe")
                {
                    ViewBag.EsAutorizador = User.IsInRole("Autoriza Especial");
                }
                // Filttro de Departamentos Creacion
                ViewBag.MostarCrearGrabar = true;
                if (_modelo.Estado == "Borrador")
                {
                    var _ListaDeptosCrear = _ListaDeptosUsario.Where(p => p.Crear == true);
                    ViewBag.VB_Deptos = _ListaDeptosCrear;

                    // Validar Permiso Crear                                        
                    if (_ListaDeptosCrear.Count() == 0) ViewBag.MostarCrearGrabar = false;
                }

            }
            catch (Exception ex)
            {
                String Adicional = "";
                try { Adicional = String.Join(",", ((System.Data.Entity.Validation.DbEntityValidationException)ex).EntityValidationErrors.Select(P => P.ValidationErrors).First().Select(p => p.ErrorMessage)); } catch { }

                TempData["MensajeDanger"] = (ex.InnerException == null) ? ex.Message + " " + Adicional : ex.InnerException.Message + " " + Adicional;
            }

            if (string.IsNullOrEmpty(_modelo.Estado)) _modelo.Estado = "Borrador";
            if (string.IsNullOrEmpty(_modelo.EmpresaId.ToString())) _modelo.EmpresaId = _EmpresaId;

            if (_modelo.ModalNuevo == null)
            {
                _modelo.ModalEditar = new OC_DetalleModel();
                _modelo.ModalNuevo = new OC_DetalleModel();
                _modelo.ModalNuevo.Descuentos = 0;
                _modelo.ModalNuevo.Cantidad = 0;

            }
            _modelo.CCobligatorio = ConfigurationManager.AppSettings["ObligatorioCC"];


            _modelo.Histotico = _Db.FEL_DocHistorico.Where(p => p.Fel_Unique == _modelo.Fel_Unique).OrderByDescending(p => p.FechaHora).ToList();

            // Correccion de Nulo Boton Crear
            if (ViewBag.MostarCrearGrabar == null) ViewBag.MostarCrearGrabar = true;

            // Btn Cargar Documentos
            if (uiBT_UploadFileOC != null)
            {
                try
                {
                    // valida de Haya Seleccionado Adjunto
                    if (fileoc == null)
                    {
                        TempData["MensajeDanger"] = "Debe de Seleccionar un Documento para Poder Subirlo";
                        ModelState.Clear();
                    }
                    else
                    {
                        if (fileoc.ContentLength > 0)
                        {
                            if (!System.IO.Path.GetExtension(fileoc.FileName).Contains("pdf"))
                            {
                                TempData["MensajeDanger"] = "Solo se pueden cargar archivos PDF";
                                ModelState.Clear();

                            }
                            else
                            {
                                // Upload Files to Server
                                var _ServerPathRef = @"~/Cargados/" + _modelo.CardCode + "/AdjunotosOC";
                                var _ServerPath = Server.MapPath(_ServerPathRef);
                                if (!System.IO.Directory.Exists(_ServerPath)) Directory.CreateDirectory(_ServerPath);
                                var _filename = Guid.NewGuid().ToString() + ".pdf";

                                String _Fact_Path = System.IO.Path.Combine(_ServerPath, _filename);
                                fileoc.SaveAs(_Fact_Path);
                                FEL_DocAdjunto nuevoAdjunto = new FEL_DocAdjunto();
                                nuevoAdjunto.AdjuntoId = Guid.NewGuid();
                                nuevoAdjunto.AdjuntoFecha = DateTime.Now;
                                nuevoAdjunto.AdjuntoRuta = @"/Cargados/" + _modelo.CardCode + "/AdjunotosOC" + @"/" + _filename;
                                nuevoAdjunto.AdjuntoTipo = _modelo.TipoAdjuntoDoc;
                                nuevoAdjunto.FEL_Unique = _modelo.Fel_Unique;
                                nuevoAdjunto.NombreOriginal = Path.GetFileName(fileoc.FileName);
                                nuevoAdjunto.UserName = User.Identity.Name;
                                _Db.FEL_DocAdjunto.Add(nuevoAdjunto);
                                _Db.SaveChanges();
                            }
                        }
                        else
                        {
                            TempData["MensajeWarning"] = "Erro Archivo Corrupto";
                        }
                    }
                }
                catch (Exception ex)
                {
                    ViewBag.Message = ex.Message;

                    ModelState.Clear();

                    return View(_modelo);
                }
            }
            // Actualizar Adjuntos Documentos
            if (_modelo.Fel_Unique != Guid.Empty)
            {
                _modelo.Adjuntos = _Db.FEL_DocAdjunto.Where(p => p.FEL_Unique == _modelo.Fel_Unique).ToList();
            }
            // Llena Lista Adjuntos 
            List<String> opcionesTipoDocumento = new List<string>();
            opcionesTipoDocumento.Add("Cotizacion");
            opcionesTipoDocumento.Add("Otros");
            ViewBag.VB_listaTipoAdjuntoDoc = new SelectList(opcionesTipoDocumento);

            _modelo.Regiones = new List<string>();
            try { _modelo.Regiones = vServicio.ObtenerCCRegionesTodas(); } catch { }


            return View(_modelo);
        }

        private void AgregarHistorico(Guid Unique, string Origen, string Destino, string Tipo, string Comentario)
        {
            FEL_DocHistorico _nuevo = new FEL_DocHistorico();
            _nuevo.Estado_Original = Origen;
            _nuevo.Estado_Destino = Destino;
            _nuevo.FechaHora = DateTime.Now;
            _nuevo.Fel_Unique = Unique;
            _nuevo.Observaciones = Comentario;
            _nuevo.Tipo = Tipo;
            _nuevo.Usuario = User.Identity.Name;
            _Db.FEL_DocHistorico.Add(_nuevo);
            _Db.SaveChanges();
        }

        private void CalcularTotales_Impuestos_Varios(ref OC_EmitirModel _modelo)
        {
            foreach (var _item in _modelo.Detalle)
            {
                if (_item.TipoImpuesto == "IVA")
                {
                    _item.Impuestos = Math.Round(_item.TotalLinea - (_item.TotalLinea / 1.12m), 2);
                }
                else
                {
                    _item.Impuestos = 0;
                }
            }

            _modelo.Total = 0; try { _modelo.Total = _modelo.Detalle.Sum(p => p.TotalLinea); } catch { }

            // Calcular Impuesto  Si no es Exportacion

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
            if (( modelo.Nuevo.Doc_EM_MontoSum- modelo.Nuevo.Doc_MontoNeto) > modelo.ExcedenteMaximo)
            {
                _Retorna = String.Format("El excedente no debe ser mayor de {0}", modelo.ExcedenteMaximo);
            }
            if (modelo.Nuevo.Doc_EM_MontoSum < modelo.Nuevo.Doc_MontoNeto)
            {
                _Retorna = String.Format("La factura no puede ser mayor que la entrega", modelo.ExcedenteMaximo);
            }

            return _Retorna;
        }

        [Authorize]
        public ActionResult SubirFacturaEsp(string pid)
        {
            Models.PP.PresentadosModel modelo = new Models.PP.PresentadosModel();
            modelo.ModoActivo = "Nuevo_Paso1";
            modelo.Nuevo = new PPROV_Documento();
            modelo.Nuevo.Doc_TasaCambio = 1;
            modelo.MultiplesOrdenes = _Db.FEL_Doc.Where(p => p.FEL_Unique.ToString() == pid).ToList()[0].SyncSapId.ToString() + ",";


            return View(modelo);
        }
        [Authorize]
        [HttpPost]
        public ActionResult SubirFacturaEsp(Models.PP.PresentadosModel modelo, HttpPostedFileBase filefac, HttpPostedFileBase fileoc, FormCollection collection, string CargarCotizacion, HttpPostedFileBase filecotiza, string pid)
        {
            // Cargar Catalogos
            ViewBag.Usuario_Empresas = _Db.GEN_Empresa.Where(p => p.EmpresaId == 1).Select(p => new { Empresa_Id = p.EmpresaId, AliasName = p.EmpresaNombre }).Distinct().ToList();
            modelo.Usuario_Moneda = _Db.GEN_CatalogoDetalle.Where(p => p.Catalogo_Id == (int)Servicios.TipoCatalogo.Moneda).OrderBy(p => p.Orden).ToList();



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
                    }
                    catch { }



                    _Db.PPROV_Documento.Add(modelo.Nuevo);
                    _Db.SaveChanges();
                    TempData["MensajeSuccess"] = "Factura Especial Enviada y Subida en el sistema";

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
                    String _NumeroOc = modelo.MultiplesOrdenes.Split(',')[0];// ObtenerOCfromDPF(ParsePdf(fileoc), ref _EntregaRequerida);
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
                            foreach (var _Item in _DetalleEntrega)
                            {
                                _Item.Entrega_Usuario = "0";
                                if (_Item.Entrega_DocNum == _InfoOrden[0].Entrega_DocNum)
                                {
                                    _Item.Entrega_Usuario = "1";
                                    _multiplesEntregas = _multiplesEntregas + _Item.Entrega_DocNum + ",";
                                    _montoEntrega += (Decimal)_Item.Entrega_DocTotal;
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
                            modelo.DetalleEntregas = _DetalleEntrega;
                            modelo.Nuevo.Doc_EM_Multiple = _multiplesEntregas;
                            modelo.Nuevo.Doc_EM_MontoSum = _montoEntrega;


                            //var _CardCodeAutorizados = Obtener_CardCode_AutorizadasPorUsuario();
                            if (true)
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


            ModelState.Clear();
            return View(modelo);
        }

        private static async Task Download(string remoteUri, string local)
        {
            try
            {
                // path where download file to be saved, with filename, here I have taken file name from supplied remote url
                string FilePath = local;
                var httpClient = new HttpClient();


                var stream = await httpClient.GetStreamAsync(remoteUri);
                var fileStream = new FileStream(FilePath, FileMode.CreateNew);
                await stream.CopyToAsync(fileStream);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}