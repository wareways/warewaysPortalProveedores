using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Wareways.PortalProv.Servicios;

namespace Wareways.PortalProv.Reportes
{
    public partial class PrintOc2ByDocNum : System.Web.UI.Page
    {
        Infraestructura.PortalProvEntities _Db = new Infraestructura.PortalProvEntities();
        VServicio vService = new VServicio();

        protected void Page_Load(object sender, EventArgs e)
        {

            ReportDocument Report = new ReportDocument();
            var ArchivoReporte = HttpContext.Current.Server.MapPath("~/Reportes/OrdenCompaSTD.rpt");
            var _Id = "";
            try { _Id = Page.Request.QueryString["pid"].ToString(); } catch { Response.End(); }

            if (_Id != null)
            {
                // Create document object  
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
                    foreach (string idEM in _Id.Split(','))
                    {
                        var ReportData = vService.List_SapOPORByDocnum(_Id.ToString());
                        Report.Load(ArchivoReporte);
                        Report.SetParameterValue("DocKey@", ReportData[0].DocEntry);

                        string fileName = String.Format("OC{0}_{1}_{2}.pdf", ReportData[0].DocNum, _Db.GEN_Empresa.First().EmpresaNombre, DateTime.Now.ToShortDateString() + "_" + DateTime.Now.ToString("hh-mm-ss"));
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

                    PDFwriter.Close();
                    

                    Response.ClearHeaders();
                    //Response.AppendHeader("content-disposition", "attachment; filename=" + fileName);

                    Response.ClearContent();



                    PDFdoc.Close();
                }

                System.IO.Stream streamPDF = new System.IO.FileStream(Nombretemp, System.IO.FileMode.Open);
                Report.Close();
                Report.Dispose();
                GC.Collect();


                streamPDF.Seek(0, System.IO.SeekOrigin.Begin);

                byte[] buffer = new byte[streamPDF.Length];

                streamPDF.Read(buffer, 0, (int)streamPDF.Length);

                Response.ContentType = "application/pdf";

                Response.BinaryWrite(buffer);
                
                
                Response.Flush();
                Response.Close();
                streamPDF.Close();
                File.Delete(Nombretemp);
                
                Response.End();

                


            }





        }

    }


}

