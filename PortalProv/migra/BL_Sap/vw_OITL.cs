using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace DbSapTableAdapters
{
    //Busqueda por Almacen
    public partial class vw_OITLTableAdapter
    {

        [DataObjectMethodAttribute
      (DataObjectMethodType.Select, false)]
        public DbSap.vw_OITLDataTable GetDataBy_Filtro(int _CompanyNumber, string _Warehouse, int _ItmsGrpCod, string _ItemCode, DateTime _DocDate, string _DistNumber)
        {

            SqlCommand _db = new SqlCommand();
            SqlConnection conex = new SqlConnection(GT.System_GT.Get_ConnectionString("DbToriNube"));
            SqlDataAdapter da = new SqlDataAdapter(_db);
            DbSap.vw_OITLDataTable _tbl = new DbSap.vw_OITLDataTable();

            string _CommandText = "";
            _CommandText += " SELECT        AppDocNum, BASE_REF, BatchNumber, CalcPrice, Code, Comments, CompanyName, CompanyNumber, Currency, DistNumber, DocDate, DocNum, DocType, InQty, InvntAct, InvntryUom, ItemCode, ItemName, ";
            _CommandText += "                          ItmsGrpCod, ItmsGrpNam, LocCode, LogEntry, MdAbsEntry, OpenValue, OutQty, Price, PriceTotal, Quantity, TransValue, U_CodigoToriflex, U_Pedido, U_Peso, Warehouse, WhsName ";
            _CommandText += " FROM            vw_OITL ";
            _CommandText += " WHERE     1 = 1 ";

            //   (CompanyNumber = @CompanyNumber) AND (Warehouse = @WareHouse) AND (ItmsGrpCod = @ItmsGrpCod) AND (ItemCode = @ItemCode) AND (DocDate <= @DocDate) ";

            if (_CompanyNumber > 0)
            {
                _CommandText += " And (CompanyNumber = @CompanyNumber)  ";
                _db.Parameters.AddWithValue("@CompanyNumber", _CompanyNumber);
            }

            if (_Warehouse != null)
            {
                _CommandText += " And (Warehouse = @WareHouse)  ";
                _db.Parameters.AddWithValue("@WareHouse", _Warehouse);
            }

            if (_ItmsGrpCod != -1)
            {
                _CommandText += " And (ItmsGrpCod = @ItmsGrpCod)  ";
                _db.Parameters.AddWithValue("@ItmsGrpCod", _ItmsGrpCod);
            }

            if (_ItemCode != null)
            {
                _CommandText += " And (upper(ItemCode) like upper(@ItemCode))  ";
                _db.Parameters.AddWithValue("@ItemCode", "%" + _ItemCode + "%");

            }
            if (_DocDate != null)
            {
                _CommandText += " And (DocDate <= @DocDate)  ";
                _db.Parameters.AddWithValue("@DocDate", _DocDate);

            }

            if (_DistNumber != null)
            {
                _CommandText += " And (DistNumber = @DistNumber)  ";
                _db.Parameters.AddWithValue("@DistNumber", _DistNumber);

            }
            _CommandText += " Order by DocDate, DocNum, DocType ";

            try
            {
                _db.CommandText = _CommandText;
                _db.Connection = conex;
                _db.CommandTimeout = 240;
                conex.Close();
                da.Fill(_tbl);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return _tbl;
        }

       
    }


}