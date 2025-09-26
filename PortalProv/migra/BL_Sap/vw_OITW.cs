using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace DbSapTableAdapters
{
    //Busqueda por Almacen
    public partial class vw_OITWTableAdapter
    {

        [DataObjectMethodAttribute
        (DataObjectMethodType.Select, false)]
        public DbSap.vw_OITWDataTable GetData_ByFiltro(string _ItemCode, int _ItmsGrpCod)
        {

            SqlCommand _db = new SqlCommand();
            SqlConnection conex = new SqlConnection(GT.System_GT.Get_ConnectionString("DbSap"));

            SqlDataAdapter da = new SqlDataAdapter(_db);

            DbSap.vw_OITWDataTable _tbl = new DbSap.vw_OITWDataTable();

            string _CommandText = "";

            _CommandText += " SELECT        ItmsGrpCod, ItmsGrpNam, ItemCode, ItemName, U_CodigoToriflex, InvntryUOm, MinStock, MaxStock, QtyPOLYTEC, QtyPOLINTER, QtyGEOPLAST, QtyTotal ";
            _CommandText += " FROM vw_OITW where 1 = 1  ";

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
        public DbSap.vw_OITW_BatchDataTable GetData_GrouByBatchNumber(string _ItemCode, int _ItmsGrpCod)
        {

            SqlCommand _db = new SqlCommand();
            SqlConnection conex = new SqlConnection(GT.System_GT.Get_ConnectionString("DbToriNube"));

            SqlDataAdapter da = new SqlDataAdapter(_db);

            DbSap.vw_OITW_BatchDataTable _tbl = new DbSap.vw_OITW_BatchDataTable();

            string _CommandText = "";

            _CommandText += " Select ItmsGrpCod, ItmsGrpNam, ItemCode, ItemName, BatchNumber as U_CodigoToriflex, ";
			_CommandText += "	             sum(QtyPOLYTEC) as QtyPOLYTEC, ";
            _CommandText += "	             sum(QtyPOLINTER) as QtyPOLINTER, ";
            _CommandText += "	             sum(QtyGEOPLAST) QtyGEOPLAST, ";
            _CommandText += "	             sum(QtyTotal) as QtyTotal ";
            _CommandText += "    From( SELECT ";
            _CommandText += "                 ItmsGrpCod, ItmsGrpNam, ItemCode, ItemName,  BatchNumber, ";
            _CommandText += "	             case when CompanyNumber = 1 then Quantity else 0 end as QtyPOLYTEC, ";
            _CommandText += "	             case when CompanyNumber = 2 then Quantity else 0 end as QtyPOLINTER, ";
            _CommandText += "	             case when CompanyNumber = 3 then Quantity else 0 end as QtyGEOPLAST, ";
            _CommandText += "                 Quantity as QtyTotal ";
            _CommandText += "             FROM            vw_OITL Where 1 = 1 ";


            if (_ItmsGrpCod != -1)
            {
                _CommandText += " And ItmsGrpCod = @ItmsGrpCod ";
                _db.Parameters.AddWithValue("@ItmsGrpCod", _ItmsGrpCod);
            }

            if (_ItemCode != "")
            {
                _CommandText += " And (upper(ItemCode) = upper(@Descripcion)  )  ";
                _db.Parameters.AddWithValue("@Descripcion", "%" + _ItemCode + "%");
            }

            _CommandText += "                     ) as  vw_OITL ";
            _CommandText += "    GROUP BY   ItmsGrpCod, ItmsGrpNam, ItemCode, ItemName, BatchNumber ";


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
        public DbSap.vw_OITW_BatchDataTable GetData_GroupByBatchNumberBy_BatchNumber(string _BatchNumber)
        {

            SqlCommand _db = new SqlCommand();
            SqlConnection conex = new SqlConnection(GT.System_GT.Get_ConnectionString("DbToriNube"));

            SqlDataAdapter da = new SqlDataAdapter(_db);

            DbSap.vw_OITW_BatchDataTable _tbl = new DbSap.vw_OITW_BatchDataTable();

            string _CommandText = "";

            _CommandText += " Select ItmsGrpCod, ItmsGrpNam, ItemCode, ItemName, BatchNumber as U_CodigoToriflex, ";
            _CommandText += "	             sum(QtyPOLYTEC) as QtyPOLYTEC, ";
            _CommandText += "	             sum(QtyPOLINTER) as QtyPOLINTER, ";
            _CommandText += "	             sum(QtyGEOPLAST) QtyGEOPLAST, ";
            _CommandText += "	             sum(QtyTotal) as QtyTotal ";
            _CommandText += "    From( SELECT ";
            _CommandText += "                 ItmsGrpCod, ItmsGrpNam, ItemCode, ItemName,  BatchNumber, ";
            _CommandText += "	             case when CompanyNumber = 1 then Quantity else 0 end as QtyPOLYTEC, ";
            _CommandText += "	             case when CompanyNumber = 2 then Quantity else 0 end as QtyPOLINTER, ";
            _CommandText += "	             case when CompanyNumber = 3 then Quantity else 0 end as QtyGEOPLAST, ";
            _CommandText += "                 Quantity as QtyTotal ";
            _CommandText += "             FROM            vw_OITL Where 1 = 1 ";


            if (_BatchNumber != "")
            {
                _CommandText += " And (upper(BatchNumber) = upper(@BatchNumber)  )  ";
                _db.Parameters.AddWithValue("@BatchNumber", _BatchNumber);
            }

            _CommandText += "                     ) as  vw_OITL ";
            _CommandText += "    GROUP BY   ItmsGrpCod, ItmsGrpNam, ItemCode, ItemName, BatchNumber ";


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
        public decimal GetData_SaldoBy_ItemCode(string _ItemCode, string _BatchNumber, string _Warehouse )
        {

            SqlCommand _db = new SqlCommand();
            SqlConnection conex = new SqlConnection(GT.System_GT.Get_ConnectionString("DbToriNube"));

            SqlDataAdapter da = new SqlDataAdapter(_db);

            DbSap.vw_OITW_BatchDataTable _tbl = new DbSap.vw_OITW_BatchDataTable();

            decimal xreturn = 0;

            string _CommandText = "";

            _CommandText += " Select ItmsGrpCod, ItmsGrpNam, ItemCode, max(ItemName) as ItemName, BatchNumber as U_CodigoToriflex, ";
            _CommandText += "	             sum(QtyPOLYTEC) as QtyPOLYTEC, ";
            _CommandText += "	             sum(QtyPOLINTER) as QtyPOLINTER, ";
            _CommandText += "	             sum(QtyGEOPLAST) QtyGEOPLAST, ";
            _CommandText += "	             sum(QtyTotal) as QtyTotal ";
            _CommandText += "    From( SELECT ";
            _CommandText += "                 ItmsGrpCod, ItmsGrpNam, ItemCode, ItemName,  BatchNumber, ";
            _CommandText += "	             case when CompanyNumber = 1 then Quantity else 0 end as QtyPOLYTEC, ";
            _CommandText += "	             case when CompanyNumber = 2 then Quantity else 0 end as QtyPOLINTER, ";
            _CommandText += "	             case when CompanyNumber = 3 then Quantity else 0 end as QtyGEOPLAST, ";
            _CommandText += "                 Quantity as QtyTotal ";
            _CommandText += "             FROM            vw_OITL Where 1 = 1 ";


            if (_ItemCode != "")
            {
                _CommandText += " And (upper(ItemCode) = upper(@ItemCode)  )  ";
                _db.Parameters.AddWithValue("@ItemCode", _ItemCode);
            }


            if (_BatchNumber != "")
            {
                _CommandText += " And (upper(BatchNumber) = upper(@BatchNumber)  )  ";
                _db.Parameters.AddWithValue("@BatchNumber", _BatchNumber);
            }

            if (_Warehouse != "")
            {
                _CommandText += " And (upper(Warehouse) = upper(@Warehouse)  )  ";
                _db.Parameters.AddWithValue("@Warehouse", _Warehouse);
            }

            _CommandText += "                     ) as  vw_OITL ";
            _CommandText += "    GROUP BY ItmsGrpCod, ItmsGrpNam, ItemCode,  BatchNumber ";


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

            if (_tbl.Rows.Count > 0)
            {
                xreturn = _tbl[0].QtyTotal;
            }

            return xreturn;
        }

    }


}