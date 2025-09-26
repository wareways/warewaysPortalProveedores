using Microsoft.Reporting.WebForms;
using System;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Web;

namespace DbSapTableAdapters
{
    //Busqueda por Almacen
    public partial class vw_Entregas_SinFacturaTableAdapter
    {

        [DataObjectMethodAttribute
        (DataObjectMethodType.Select, false)]
        public DbSap.vw_Entregas_SinFacturaDataTable GetData_ByFiltro(string ShipNumber)
        {

            SqlCommand _db = new SqlCommand();
            SqlConnection conex = new SqlConnection();
            SqlDataAdapter da = new SqlDataAdapter(_db);

            DbSap.vw_Entregas_SinFacturaDataTable _tbl = new DbSap.vw_Entregas_SinFacturaDataTable();

            string _CommandText = "";

            if (ShipNumber == null)
            {
                conex.ConnectionString = GT.System_GT.Get_ConnectionString("DbSap");

                _CommandText += "  SELECT ObjType, DocNum, DocDate, DocStatus, CardCode, CardName, Address, NumAtCard, DocCur, DocRate, DocTotal, Comments, U_Embarque, U_OV, U_OC, U_TotalRollos, U_TotalPesoKG, DocEntry, U_NoEnvio ";
                _CommandText += "  FROM vw_Entregas ";
                _CommandText += "  GROUP BY ObjType, DocNum, DocDate, DocStatus, CardCode, CardName, Address, NumAtCard, DocCur, DocRate, DocTotal, Comments, U_Embarque, U_OV, U_OC, U_TotalRollos, U_TotalPesoKG,TargetType, DocEntry, U_NoEnvio ";
                _CommandText += "  HAVING        (TargetType <> '16') AND(DocStatus <> 'C') AND(max(U_facNum) IS NULL) ";
                _CommandText += "  Order by DocDate ";


            }
            else
            {
                conex.ConnectionString = GT.System_GT.Get_ConnectionString("DbTori");

                _CommandText += "  SELECT ObjType, DocNum, max(DocDate)DocDate, DocStatus, CardCode, CardName, Address, NumAtCard, DocCur, max(DocRate) DocRate, sum(DocTotal) DocTotal, Comments, U_Embarque, U_OV, U_OC, sum(U_TotalRollos) as U_TotalRollos, sum(U_TotalPesoKG) as U_TotalPesoKG, DocEntry, U_NoEnvio ";
                _CommandText += "  FROM            vw_EntregasTori ";
                _CommandText += "  GROUP BY ObjType, DocNum, DocDate, DocStatus, CardCode, CardName, Address, NumAtCard, DocCur, Comments, U_Embarque, U_OV, U_OC, TargetType, DocEntry, U_NoEnvio ";
                _CommandText += "  HAVING   U_Embarque = @ShipNumber ";
                _CommandText += "  Order by DocDate ";

                _db.Parameters.AddWithValue("@ShipNumber", ShipNumber);

            }

            try
            {
                _db.CommandText = _CommandText;
                _db.Connection = conex;
                conex.Open();
                _db.CommandTimeout = 240;
                da.Fill(_tbl);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return _tbl;
        }


        public void Imprimir_Entregas(string _DocEntry, string _NoEnvio)
        {
            if (_NoEnvio != null)
            {
                if (_NoEnvio != "" && _NoEnvio != "null")
                {
                    oDeliveryNoteTableAdapter _oDelivery = new oDeliveryNoteTableAdapter();
                    _oDelivery.Update_oDeliveryNote_Envio(Convert.ToInt32(_DocEntry), _NoEnvio);
                }
            }

            Warning[] warnings;
            string[] streamIds;
            string mimeType = string.Empty, encoding = string.Empty, extension = string.Empty;
            string deviceInfo =
            "<DeviceInfo>" +
            "  <OutputFormat>EMF</OutputFormat>" +
            "  <PageWidth>8.5in</PageWidth>" +
            "  <PageHeight>11in</PageHeight>" +
            "  <MarginTop>0in</MarginTop>" +
            "  <MarginLeft>0in</MarginLeft>" +
            "  <MarginRight>0in</MarginRight>" +
            "  <MarginBottom>0in</MarginBottom>" +
            "</DeviceInfo>";
            ReportViewer viewer = new ReportViewer();

            DbSapTableAdapters.vw_EntregasTableAdapter _dbEntregas = new vw_EntregasTableAdapter();
            DbSap.vw_EntregasDataTable _tblEntregas = new DbSap.vw_EntregasDataTable();
            _tblEntregas = _dbEntregas.GetDataBy_DocEntry(Convert.ToInt32(_DocEntry));
            ReportDataSource _rdsData = new ReportDataSource("vw_Entregas", (DataTable)_tblEntregas);

            if (_tblEntregas.Rows.Count == 0)
            {
                HttpContext.Current.Response.Write("no Se encontraron datos");
                return;
            }

            viewer.LocalReport.Refresh();
            viewer.LocalReport.ReportPath = HttpContext.Current.Server.MapPath("~/App_SAp/vw_EntregasRPT.rdlc");
            viewer.LocalReport.DataSources.Add(_rdsData);

            byte[] bytes = viewer.LocalReport.Render("PDF", deviceInfo, out mimeType, out encoding, out extension, out streamIds, out warnings);
            HttpContext.Current.Response.Buffer = true;
            HttpContext.Current.Response.ClearContent();
            HttpContext.Current.Response.ClearHeaders();
            HttpContext.Current.Response.ContentType = "application/pdf";
            HttpContext.Current.Response.AddHeader("content-disposition", "inline; filename=" + "Entrega_" + _DocEntry.ToString() + "." + extension);
            HttpContext.Current.Response.OutputStream.Write(bytes, 0, bytes.Length);
            HttpContext.Current.Response.Flush();
            HttpContext.Current.Response.End();
        }

    }


}