using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.Reporting.WebForms;
using Wareways.PortalProv.Servicios;

namespace Wareways.PortalProv.Reportes
{
    public partial class IntegradoLog : System.Web.UI.Page
    {
        Infraestructura.PortalProvEntities _Db = new Infraestructura.PortalProvEntities();
        VServicio vService = new VServicio();

        protected void Page_Load(object sender, EventArgs e)
        {

            ReportDocument Report = new ReportDocument();
            int? xTransId = 0;

            var _Id = "";
            try { _Id = Page.Request.QueryString["pid"].ToString(); } catch { Response.End(); }

            if (_Id != null)
            {
                var oDoc = _Db.PPROV_Documento.Where(p => p.Doc_Id.ToString() == _Id).First();

                iTextSharp.text.Document PDFdoc = new iTextSharp.text.Document();
                var Nombretemp = HttpContext.Current.Server.MapPath("~/Cargados/" + Guid.NewGuid().ToString() + ".pdf");

                using (System.IO.FileStream MyFileStream = new System.IO.FileStream(Nombretemp, System.IO.FileMode.Create))
                {
                    iTextSharp.text.pdf.PdfCopy PDFwriter = new iTextSharp.text.pdf.PdfCopy(PDFdoc, MyFileStream);
                    if (PDFwriter == null)
                    {
                        return;
                    }
                    PDFdoc.Open();

                    FacturaCliente(Report, oDoc, PDFwriter); 

                    // Orden de Compra Sistema
                     IntegrarOCS(Report, oDoc, PDFwriter);

                    // Entrega de Compra Sistema
                     IntegrarEMS(Report, oDoc, PDFwriter); 
                    // Factura SAP
                     IntegrarFactSAP(Report, oDoc, PDFwriter);
                     IntegrarFactSAPDiario(Report, oDoc, PDFwriter, ref xTransId); 


                     IntegrarContrasena(Report, oDoc, PDFwriter);

                     IntegrarRetenciones(Report, oDoc, PDFwriter); 

                    PDFwriter.Close();





                    PDFdoc.Close();
                }

                System.IO.Stream streamPDF = new System.IO.FileStream(Nombretemp, System.IO.FileMode.Open);
                Report.Close();
                Report.Dispose();
                GC.Collect();


                streamPDF.Seek(0, System.IO.SeekOrigin.Begin);

                byte[] buffer = new byte[streamPDF.Length];

                streamPDF.Read(buffer, 0, (int)streamPDF.Length);
                Response.Clear();

                Response.ContentType = "application/octet-stream";
                var filename = string.Format("OCS_{0}_{1}_FAC{2}.pdf", xTransId, oDoc.Doc_Fecha.Value.ToString("yyyyMMdd"), oDoc.Doc_Numero);
                Response.AddHeader("Content-Disposition", "attachment;filename=" + filename + ";");

                Response.BinaryWrite(buffer);

                streamPDF.Close();
                File.Delete(Nombretemp);

                Response.End();



            }





        }

        private void IntegrarContrasena(ReportDocument report, Infraestructura.PPROV_Documento oDoc, PdfCopy PDFwriter)
        {
            if (oDoc.Contrasena_Id != null)
            {

                string mimeType = "application/pdf";


                var _Encabezado = new List<Infraestructura.SP_REP_Contrasena_Encabezado_Result>();
                var _Detalle = new List<Infraestructura.SP_REP_Contrasena_Detalle_Result>();

                _Encabezado = _Db.SP_REP_Contrasena_Encabezado(oDoc.Contrasena_Id).ToList();
                _Detalle = _Db.SP_REP_Contrasena_Detalle(oDoc.Contrasena_Id).ToList();


                Microsoft.Reporting.WebForms.ReportViewer ReportViewer2 = new Microsoft.Reporting.WebForms.ReportViewer();

                ReportViewer2.LocalReport.ReportPath = Server.MapPath("~/Infraestructura/Reportes/Contrasena.rdlc");
                ReportViewer2.LocalReport.DataSources.Clear();
                ReportDataSource rdc_Det = new ReportDataSource("Detalle", _Detalle);
                ReportViewer2.LocalReport.DataSources.Add(rdc_Det);
                ReportDataSource rdc_Enc = new ReportDataSource("Encabezado", _Encabezado);
                ReportViewer2.LocalReport.DataSources.Add(rdc_Enc);
                ReportViewer2.LocalReport.Refresh();

                string encoding = string.Empty;
                string extension = string.Empty;
                Warning[] warnings;
                string[] streamIds;
                byte[] bytes = ReportViewer2.LocalReport.Render("PDF", null, out mimeType, out encoding, out extension, out streamIds, out warnings);

                System.IO.Stream s = new MemoryStream(bytes);
                PdfReader reader = new PdfReader(s);
                reader.ConsolidateNamedDestinations();



                for (int i = 1; i <= reader.NumberOfPages; i++)
                {
                    iTextSharp.text.pdf.PdfImportedPage page = PDFwriter.GetImportedPage(reader, i);
                    PDFwriter.AddPage(page);
                }
                //PRAcroForm form = reader.AcroForm;
                //if (form != null)
                //{
                //    writer.CopyAcroForm(reader);
                //}

                reader.Close();

            }

        }

        private void FacturaCliente(ReportDocument Report, Infraestructura.PPROV_Documento oDoc, PdfCopy PDFwriter)
        {

            //ExportOptions options = new ExportOptions();
            //options.ExportFormatType = ExportFormatType.PortableDocFormat;
            //options.FormatOptions = new ExcelFormatOptions();
            var _ServerPath = Server.MapPath(oDoc.Doc_PdfFactura);

            PdfReader.unethicalreading = true;            
            PdfReader reader = new PdfReader(_ServerPath);
            
            
            reader.ConsolidateNamedDestinations();
            
        


            for (int i = 1; i <= reader.NumberOfPages; i++)
            {
                iTextSharp.text.pdf.PdfImportedPage page = PDFwriter.GetImportedPage(reader, i);
                PDFwriter.AddPage(page);
            }
            //PRAcroForm form = reader.AcroForm;
            //if (form != null)
            //{
            //    writer.CopyAcroForm(reader);
            //}

            reader.Close();

        }

        private void IntegrarRetenciones(ReportDocument Report, Infraestructura.PPROV_Documento oDoc, PdfCopy PDFwriter)
        {
            foreach (var itemr in oDoc.PPROV_Retencion)
            {

                //ExportOptions options = new ExportOptions();
                //options.ExportFormatType = ExportFormatType.PortableDocFormat;
                //options.FormatOptions = new ExcelFormatOptions();
                var _ServerPath = Server.MapPath(itemr.Retencion_Pdf);
                PdfReader reader = new PdfReader(_ServerPath);
                reader.ConsolidateNamedDestinations();



                for (int i = 1; i <= reader.NumberOfPages; i++)
                {
                    iTextSharp.text.pdf.PdfImportedPage page = PDFwriter.GetImportedPage(reader, i);
                    PDFwriter.AddPage(page);
                }
                //PRAcroForm form = reader.AcroForm;
                //if (form != null)
                //{
                //    writer.CopyAcroForm(reader);
                //}

                reader.Close();

            }


            

        }

        private void IntegrarOCS(ReportDocument Report, Infraestructura.PPROV_Documento oDoc, PdfCopy PDFwriter)
        {
            foreach (var oitemOc in oDoc.Doc_OC_Multiple.Split(','))
            {
                var ArchivoReporte = HttpContext.Current.Server.MapPath("~/Reportes/OrdenCompaSTD.rpt");

                var ReportData = vService.List_SapOPORByDocnum(oitemOc);

                Report.Load(ArchivoReporte);
                Report.SetParameterValue("DocKey@", ReportData[0].DocEntry);

                string fileName = String.Format("OCx{0}_{1}_{2}.pdf", ReportData[0].DocNum, Session["EmpresaSelName"].ToString(), DateTime.Now.ToShortDateString() + "_" + DateTime.Now.ToString("hh-mm-ss"));
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
                PdfReader reader = new PdfReader(s);
                reader.ConsolidateNamedDestinations();



                for (int i = 1; i <= reader.NumberOfPages; i++)
                {
                    iTextSharp.text.pdf.PdfImportedPage page = PDFwriter.GetImportedPage(reader, i);
                    PDFwriter.AddPage(page);
                }
                //PRAcroForm form = reader.AcroForm;
                //if (form != null)
                //{
                //    writer.CopyAcroForm(reader);
                //}

                reader.Close();
            }
        }


        private void IntegrarEMS(ReportDocument Report, Infraestructura.PPROV_Documento oDoc, PdfCopy PDFwriter)
        {
            foreach (var oitemOc in oDoc.Doc_EM_Multiple.Split(','))
            {
                var ArchivoReporte = HttpContext.Current.Server.MapPath("~/Reportes/EntregaCompaSTD.rpt");

                var ReportData = vService.List_SapOPDNByDocnum(oitemOc);
                Report.Load(ArchivoReporte);
                Report.SetParameterValue("DocKey@", ReportData[0].DocEntry);

                string fileName = String.Format("OCx{0}_{1}_{2}.pdf", ReportData[0].DocNum, Session["EmpresaSelName"].ToString(), DateTime.Now.ToShortDateString() + "_" + DateTime.Now.ToString("hh-mm-ss"));
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
                PdfReader reader = new PdfReader(s);
                reader.ConsolidateNamedDestinations();



                for (int i = 1; i <= reader.NumberOfPages; i++)
                {
                    iTextSharp.text.pdf.PdfImportedPage page = PDFwriter.GetImportedPage(reader, i);
                    PDFwriter.AddPage(page);
                }
                //PRAcroForm form = reader.AcroForm;
                //if (form != null)
                //{
                //    writer.CopyAcroForm(reader);
                //}

                reader.Close();
            }
        }

        private void IntegrarFactSAP(ReportDocument Report, Infraestructura.PPROV_Documento oDoc, PdfCopy PDFwriter)
        {
            foreach (var oitemOc in oDoc.Doc_NumeroFactProv.ToString().Split(','))
            {
                var ArchivoReporte = HttpContext.Current.Server.MapPath("~/Reportes/FacturaCompaSTD.rpt");

                var ReportData = vService.List_SapOPCHByDocnum(oitemOc);
                Report.Load(ArchivoReporte);
                Report.SetParameterValue("DocKey@", ReportData[0].DocEntry);

                string fileName = String.Format("OCx{0}_{1}_{2}.pdf", ReportData[0].DocNum, Session["EmpresaSelName"].ToString(), DateTime.Now.ToShortDateString() + "_" + DateTime.Now.ToString("hh-mm-ss"));
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
                PdfReader reader = new PdfReader(s);
                reader.ConsolidateNamedDestinations();



                for (int i = 1; i <= reader.NumberOfPages; i++)
                {
                    iTextSharp.text.pdf.PdfImportedPage page = PDFwriter.GetImportedPage(reader, i);
                    PDFwriter.AddPage(page);
                }
                //PRAcroForm form = reader.AcroForm;
                //if (form != null)
                //{
                //    writer.CopyAcroForm(reader);
                //}

                reader.Close();
            }
        }

        private void IntegrarFactSAPDiario(ReportDocument Report, Infraestructura.PPROV_Documento oDoc, PdfCopy PDFwriter, ref int? xTransId)
        {
            foreach (var oitemOc in oDoc.Doc_NumeroFactProv.ToString().Split(','))
            {
                var ArchivoReporte = HttpContext.Current.Server.MapPath("~/Reportes/RegistroDiarioFactPSTD.rpt");

                var ReportData = vService.List_SapOPCHByDocnum(oitemOc);
                xTransId = ReportData[0].TransId;
                Report.Load(ArchivoReporte);
                Report.SetParameterValue("DocKey@", ReportData[0].DocEntry);

                string fileName = String.Format("OCx2{0}_{1}_{2}.pdf", ReportData[0].DocNum, Session["EmpresaSelName"].ToString(), DateTime.Now.ToShortDateString() + "_" + DateTime.Now.ToString("hh-mm-ss"));
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
                PdfReader reader = new PdfReader(s);
                reader.ConsolidateNamedDestinations();



                for (int i = 1; i <= reader.NumberOfPages; i++)
                {
                    iTextSharp.text.pdf.PdfImportedPage page = PDFwriter.GetImportedPage(reader, i);
                    PDFwriter.AddPage(page);
                }
                //PRAcroForm form = reader.AcroForm;
                //if (form != null)
                //{
                //    writer.CopyAcroForm(reader);
                //}

                reader.Close();
            }
        }
    }


}

