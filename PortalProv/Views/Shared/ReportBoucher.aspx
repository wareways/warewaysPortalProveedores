<%@ Page Language="C#" AutoEventWireup="true"  Inherits="System.Web.Mvc.ViewPage"  %>

<%@ Register Assembly="Microsoft.ReportViewer.WebForms" Namespace="Microsoft.Reporting.WebForms" TagPrefix="rsweb" %>




<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <script runat="server">
        void Page_load(object sender, EventArgs e)
        {
            Wareways.PortalProv.Infraestructura.corpordEntities _Db = new Wareways.PortalProv.Infraestructura.corpordEntities();

            if (!IsPostBack)
            {
                var parametros = (string)Model;
                Wareways.PortalProv.Infraestructura.Reportes.DsReportsTableAdapters.BoucherTableAdapter _Ta_Boucher = new Wareways.PortalProv.Infraestructura.Reportes.DsReportsTableAdapters.BoucherTableAdapter();
                var _Datos = _Ta_Boucher.GetDataBySolicitudId(Guid.Parse(parametros));

                foreach(Wareways.PortalProv.Infraestructura.Reportes.DsReports.BoucherRow _Item in _Datos.Rows)
                {


                    Wareways.PortalProv.Infraestructura.Reportes.DsReportsTableAdapters.BoucherDataTableAdapter _Ta_BoucherData = new Wareways.PortalProv.Infraestructura.Reportes.DsReportsTableAdapters.BoucherDataTableAdapter();
                    var  _DatosDetalle = _Ta_BoucherData.GetDataBySolicitudDetalleId(_Item.SolicitudDetalle_Id);

                    ReportViewer2.LocalReport.ReportPath = Server.MapPath("~/Infraestructura/Reportes/BoucherDetalle.rdlc");
                    ReportViewer2.LocalReport.DataSources.Clear();
                    ReportDataSource rdc = new ReportDataSource("Datos", _DatosDetalle.Rows);
                    ReportViewer2.LocalReport.DataSources.Add(rdc);
                    ReportViewer2.LocalReport.Refresh();

                    string mimeType = string.Empty;
                    string encoding = string.Empty;
                    string extension = string.Empty;
                    Warning[] warnings;
                    string[] streamIds;
                    byte[] bytes = ReportViewer2.LocalReport.Render("PDF");
            
                    Response.Buffer = true;
                    Response.Clear();
                    Response.ContentType = mimeType;
                    Response.AddHeader("content-disposition", "attachment; filename= filename" + "." + extension);
                    Response.OutputStream.Write(bytes, 0, bytes.Length); // create the file  
                    Response.Flush(); // send it to the client to download  
                    Response.End();
                }








            }
        }

    </script>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>            
            <rsweb:ReportViewer ID="ReportViewer2" runat="server" AsyncRendering="false" SizeToReportContent="true"></rsweb:ReportViewer>            
        </div>
    </form>
</body>
</html>
