using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using Wareways.PortalProv.Servicios;

namespace Wareways.PortalProv.Reportes
{
    public partial class PrintOC : System.Web.UI.Page
    {
        Infraestructura.PortalProvEntities _Db = new Infraestructura.PortalProvEntities();
        VServicio vService = new VServicio();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["EmpresaSelId"] == null)
            {
                Wareways.PortalProv.Servicios.ServicioSeguridad.CheckSession(User.Identity.Name);                
            }
            else
            {
                ReportDocument Report = new ReportDocument();
                var ArchivoReporte = HttpContext.Current.Server.MapPath("~/Reportes/OrdenCompaSTD.rpt");
                var _Id = Guid.Empty;
                try { _Id  = Guid.Parse(Page.Request.QueryString["pid"].ToString()); } catch { Response.End(); }

                if (_Id != Guid.Empty)
                {
                    var ReportData = vService.List_SapOPORSync(_Id.ToString());
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
                    Report.Close();
                    Report.Dispose();
                    GC.Collect();


                    Response.ClearHeaders();
                    Response.AppendHeader("content-disposition", "attachment; filename=" + fileName);

                    Response.ClearContent();


                    Response.ContentType = "application/pdf";


                    s.Seek(0, System.IO.SeekOrigin.Begin);

                    byte[] buffer = new byte[s.Length];

                    s.Read(buffer, 0, (int)s.Length);

                    Response.BinaryWrite(buffer);

                    Response.Flush();
                    Response.Close();
                    s.Close();
                    

                    Response.End();
                }
                
            }

            
        }
    }
}