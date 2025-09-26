using Microsoft.Reporting.WebForms;
using System;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Web;

namespace DbSapTableAdapters
{
    //Busqueda por Almacen
    public partial class vw_EntregasTableAdapter
    {

        [DataObjectMethodAttribute
        (DataObjectMethodType.Select, false)]
        public DbSap.vw_EntregasDataTable GetData_ByFiltro(string _ItemCode, int _ItmsGrpCod)
        {

            SqlCommand _db = new SqlCommand();
            SqlConnection conex = new SqlConnection(GT.System_GT.Get_ConnectionString("DbSap"));

            SqlDataAdapter da = new SqlDataAdapter(_db);

            DbSap.vw_EntregasDataTable _tbl = new DbSap.vw_EntregasDataTable();

            string _CommandText = "";

            _CommandText += " SELECT        ItmsGrpCod, ItmsGrpNam, ItemCode, ItemName, U_CodigoToriflex, InvntryUOm, MinStock, MaxStock, QtyPOLYTEC, QtyPOLINTER, QtyGEOPLAST, QtyTotal ";
            _CommandText += " FROM vw_Entregas where 1 = 1  ";

            if (_ItmsGrpCod != -1)
            {
                _CommandText += " And ItmsGrpCod = @ItmsGrpCod ";
                _db.Parameters.AddWithValue("@ItmsGrpCod", _ItmsGrpCod);
            }

            _CommandText += " And (upper(ItemCode) like upper(@Descripcion) or upper(isnull(U_CodigoToriflex,'')) like upper(@Descripcion)  )  ";
            _db.Parameters.AddWithValue("@Descripcion", "%" + _ItemCode + "%");

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

        [DataObjectMethodAttribute
        (DataObjectMethodType.Select, false)]
        public DbSap.vw_EntregasDataTable GetData_ByShipNumberTori(int ShipNumber)
        {
            SqlCommand _db = new SqlCommand();
            SqlConnection conex = new SqlConnection(GT.System_GT.Get_ConnectionString("DbTori"));

            SqlDataAdapter da = new SqlDataAdapter(_db);

            DbSap.vw_EntregasDataTable _tbl = new DbSap.vw_EntregasDataTable();

            string _CommandText = "";

            _CommandText += " SELECT Address, MAX(BPLId)AS BPLId, MAX(BPLName) AS BPLName, MAX(BaseEntry) AS BaseEntry, MAX(BaseLine) AS BaseLine, MAX(BaseType) AS BaseType, MAX(CANCELED) AS CANCELED, CardCode, ";
            _CommandText += "                         MAX(CardFName) AS CardFName, MAX(CardName) AS CardName, MAX(Comments) AS Comments, MAX(Currency) AS Currency, DocCur, DocDate, MAX(DocDateFacturacion) AS DocDateFacturacion, ";
            _CommandText += "                         MAX(DocDueDate) AS DocDueDate, DocNum, MAX(DocNumFacturacion) AS DocNumFacturacion, MAX(DocRate) AS DocRate, DocStatus, MAX(DocTotal) AS DocTotal, MAX(DocType) AS DocType, MAX(Dscription) ";
            _CommandText += "                         AS Dscription, MAX(GroupNum) AS GroupNum, MAX(ItemCode) AS ItemCode, MAX(ItemCodeFacturacion) AS ItemCodeFacturacion, MAX(ItemDescripction) AS ItemDescripction, MAX(JrnlMemo) AS JrnlMemo, ";
            _CommandText += "                         MAX(LineTotal) AS LineTotal, NumAtCard, ObjType, MAX(Price) AS Price, MAX(Printed) AS Printed, SUM(Quantity) AS Quantity, MAX(QuantityFacturacion) AS QuantityFacturacion, MAX(Rate) AS Rate, MAX(Ref1) ";
            _CommandText += "                         AS Ref1, MAX(Ref2) AS Ref2, MAX(SeriesName) AS SeriesName, MAX(SlpCode) AS SlpCode, MAX(SlpName) AS SlpName, TargetType, MAX(TransId) AS TransId, U_Embarque, MAX(U_FOB) AS U_FOB, ";
            _CommandText += "                         MAX(U_FacNit) AS U_FacNit, MAX(U_FacNom) AS U_FacNom, MAX(U_FacSerie) AS U_FacSerie, U_NoEnvio, U_OC, U_OV, MAX(U_PM) AS U_PM, MAX(U_Siglas) AS U_Siglas, SUM(U_TotalPesoKG) ";
            _CommandText += "                         AS U_TotalPesoKG, SUM(U_TotalRollos) AS U_TotalRollos, MAX(U_facNum) AS U_facNum, MAX(U_usuario) AS U_usuario, MAX(VatSum) AS VatSum, MAX(WhsCode) AS WhsCode, MAX(unitMsr) AS unitMsr ";
            _CommandText += " FROM            vw_EntregasTori ";
            _CommandText += " GROUP BY ObjType, DocNum, DocDate, DocStatus, CardCode, CardName, Address, NumAtCard, DocCur, Comments, U_Embarque, U_OV, U_OC, TargetType, DocEntry, U_NoEnvio, ItemCode ";
            _CommandText += " HAVING   U_Embarque = @ShipNumber ";
            _CommandText += " Order by DocDate ";

            _db.Parameters.AddWithValue("@ShipNumber", ShipNumber);

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


        public void Imprimir_Entregas(string _DocEntry, string _NoEnvio, string _ShipNumber, string OrderNumber)
        {

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

            if (_DocEntry != null && _DocEntry != "")
            {
                _tblEntregas = _dbEntregas.GetDataBy_DocEntry(Convert.ToInt32(_DocEntry));
            }
            else
            {
                try { _tblEntregas = GetData_ByShipNumberTori(Convert.ToInt32(_ShipNumber.ToString())); } catch { }
            }

            ReportDataSource _rdsData = new ReportDataSource("vw_Entregas", (DataTable)_tblEntregas);

            if (_tblEntregas.Rows.Count == 0)
            {
                HttpContext.Current.Response.Write("no Se encontraron datos");
                return;
            } 

            viewer.LocalReport.Refresh();
            viewer.LocalReport.ReportPath = HttpContext.Current.Server.MapPath("~/App_SAp/vw_EntregasRPT_L4.rdlc");
            viewer.LocalReport.DataSources.Add(_rdsData);
            if (_tblEntregas.Rows.Count > 0)
            {
                if (_tblEntregas.Rows.Count == 1)
                {
                    viewer.LocalReport.ReportPath = HttpContext.Current.Server.MapPath("~/App_SAp/vw_EntregasRPT_L1.rdlc");
                }
                else if (_tblEntregas.Rows.Count == 2)
                {
                    viewer.LocalReport.ReportPath = HttpContext.Current.Server.MapPath("~/App_SAp/vw_EntregasRPT_L2.rdlc");
                }
                else if (_tblEntregas.Rows.Count == 3)
                {
                    viewer.LocalReport.ReportPath = HttpContext.Current.Server.MapPath("~/App_SAp/vw_EntregasRPT_L3.rdlc");
                }
            }

            byte[] bytes = viewer.LocalReport.Render("PDF", deviceInfo, out mimeType, out encoding, out extension, out streamIds, out warnings);
            HttpContext.Current.Response.Buffer = true;
            //HttpContext.Current.Response.ClearContent();
            //HttpContext.Current.Response.ClearHeaders();
            HttpContext.Current.Response.ContentType = "application/pdf";
            HttpContext.Current.Response.AddHeader("content-disposition", "inline; filename=" + "Entrega_" + _DocEntry.ToString() + "." + extension);
            HttpContext.Current.Response.OutputStream.Write(bytes, 0, bytes.Length);
            
            HttpContext.Current.Response.Flush(); // Sends all currently buffered output to the client.
            HttpContext.Current.Response.SuppressContent = true;  // Gets or sets a value indicating whether to send HTTP content to the client.
            HttpContext.Current.ApplicationInstance.CompleteRequest(); // Ca
        }
    }
}