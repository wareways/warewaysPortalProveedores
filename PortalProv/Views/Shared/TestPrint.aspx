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
                var _Catalogos = _Db.Catalogo.ToList();

                ReportViewer2.LocalReport.ReportPath = Server.MapPath("~/Infraestructura/Reportes/Solicitud_Datos.rdlc");
                ReportViewer2.LocalReport.DataSources.Clear();
                ReportDataSource rdc = new ReportDataSource("DS_Catalogo", _Catalogos);
                ReportViewer2.LocalReport.DataSources.Add(rdc);
                ReportViewer2.LocalReport.Refresh();

                var parametros = Model;                

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
