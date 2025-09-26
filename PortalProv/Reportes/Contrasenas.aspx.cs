using iText.Kernel.Pdf;
using iText.Kernel.Utils;
using Microsoft.Reporting.WebForms;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
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

                if (Request.QueryString["Num"] != null)
                {
                    try
                    {
                        var contnum = int.Parse(Request.QueryString["Num"].ToString());
                        var find = _Db.PPROV_Contrasena.Where(p => p.Contrasena_Numero == contnum).ToList();
                        if (find.Count == 1) parametros = find[0].Contrasena_Id.ToString();
                    }
                    catch { }
                    
                }

                var _Encabezado = new List<Infraestructura.SP_REP_Contrasena_Encabezado_Result>();
                var _Detalle = new List<Infraestructura.SP_REP_Contrasena_Detalle_Result>();

                if (parametros == "-1")
                {
                    _Encabezado.Add(new Infraestructura.SP_REP_Contrasena_Encabezado_Result
                    {
                        CardCode = "V1010",
                        CardName = "Far East Imports",
                        AliasName = "OPERADORA DE CENTRO DE SERVICIOS, S.A",
                        DiasCredito = 30,
                        Empresa_Id = 1,
                        Nit_Empres = "5946343-0",
                        Nit_Proveedor = "1152215-K",
                        RowNbr = 1,
                        Empresa_Name = "OPERADORA DE CENTRO DE SERVICIOS, S.A",
                        Contrasena_Usuario = "Juan Perez",
                        Contrasena_Numero= 8452,
                        Contrasena_Moneda= "QTZ",
                        Contrasena_Fecha = new DateTime(2024, 1, 1),                        
                        Contrasena_Fecha_Estimada = new DateTime(2024, 1, 1),

                    });
                    _Detalle.Add(new Infraestructura.SP_REP_Contrasena_Detalle_Result
                    {
                        Doc_Serie = "AADFRG",
                        Doc_Numero = "45855554",
                        Doc_Fecha = new DateTime(2024, 1, 1),
                        Doc_NumeroOC = 1566568,
                        Doc_MontoNeto = 1235.56M,
                        Doc_Id = new Guid()

                    }) ;
                }
                else
                {
                    _Encabezado = _Db.SP_REP_Contrasena_Encabezado(Guid.Parse(parametros)).ToList();
                    _Detalle = _Db.SP_REP_Contrasena_Detalle(Guid.Parse(parametros)).ToList();

                }


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