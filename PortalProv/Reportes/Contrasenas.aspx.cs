using iText.Kernel.Pdf;
using iText.Kernel.Utils;
using Microsoft.Reporting.WebForms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Wareways.PortalProv.Reports
{
    public partial class Contrasenas : System.Web.UI.Page
    {
        Infraestructura.PortalProvEntities _Db = new Infraestructura.PortalProvEntities();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                string mimeType = "application/pdf";

                var parametros = Request.QueryString["Id"].ToString();

                var _Encabezado = _Db.SP_REP_Contrasena_Encabezado(Guid.Parse(parametros));
                var _Detalle = _Db.SP_REP_Contrasena_Detalle(Guid.Parse(parametros));

                Microsoft.Reporting.WebForms.ReportViewer ReportViewer2 = new Microsoft.Reporting.WebForms.ReportViewer();
                 
                ReportViewer2.LocalReport.ReportPath = Server.MapPath("~/Infraestructura/Reportes/Contrasena.rdlc");
                ReportViewer2.LocalReport.DataSources.Clear();
                ReportDataSource rdc_Det = new ReportDataSource("Detalle",_Detalle);
                ReportViewer2.LocalReport.DataSources.Add(rdc_Det);
                ReportDataSource rdc_Enc = new ReportDataSource("Encabezado", _Encabezado);
                ReportViewer2.LocalReport.DataSources.Add(rdc_Enc);                
                ReportViewer2.LocalReport.Refresh();

                string encoding = string.Empty;
                string extension = string.Empty;
                Warning[] warnings;
                string[] streamIds;
                byte[] bytes = ReportViewer2.LocalReport.Render("PDF", null, out mimeType, out encoding, out extension, out streamIds, out warnings);
                         
                Response.Buffer = true;
                Response.Clear();
                Response.ContentType = mimeType;
                //Response.AddHeader("content-disposition", "attachment; filename= filename" + "." + extension);
                Response.OutputStream.Write(bytes, 0, bytes.Length); // create the file  
                Response.Flush(); // send it to the client to download  
                Response.End();
            }
        }
    }
}