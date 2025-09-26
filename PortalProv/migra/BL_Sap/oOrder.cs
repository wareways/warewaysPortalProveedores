using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using Sap_Service;

/// <summary>
/// Summary description for PPrensa_OrderArtes
/// </summary>
/// 

namespace DbSapTableAdapters
{

    public partial class oOrderTableAdapter
    {

        [DataObjectMethodAttribute
     (DataObjectMethodType.Insert, true)]
        public void Insert_oOrder_OrderNumber(int OrderNumber, string Usuario)
        {
            //----------------------------------------------------------------

            string _InternalPartNbr = "";
            double _Price = 0;
            double _Quantity = 0;
            string _WarehouseCode = "";
            string _Comments = "Pedido No.: " + OrderNumber.ToString() + " Por: " + Usuario;
            string _ItemCode = "";
            int _CompanyNumber = 0;
            int _OrderNumber = 0;
            string _CardCode = "";
            int _Sucursal = 0; 

            //Datos Tarima incluye costo. 
            DbCustServTempTableAdapters.vw_Order_FullTableAdapter _Dbvw_Order_Full = new DbCustServTempTableAdapters.vw_Order_FullTableAdapter();
            DbCustServTemp.vw_Order_FullDataTable _vw_Order_Full = new DbCustServTemp.vw_Order_FullDataTable();
            _vw_Order_Full = _Dbvw_Order_Full.GetDataBy_OrderNumber(OrderNumber);

            if (_vw_Order_Full.Rows.Count > 0)
            {
                _CompanyNumber = _vw_Order_Full[0].Empresa;
            }
            else
            {
                throw new Exception("No se encontró información de Pedido no.:" + OrderNumber.ToString());
            }

            string _return = "";
            SAPbobsCOM.Company oCompany;
            oCompany = GlobalSAP.GetCompany(_CompanyNumber);
            _Sucursal = GlobalSAP.GetSucursal(_CompanyNumber);
            _WarehouseCode = GlobalSAP.GetWarehouseCode(_CompanyNumber);

            if (oCompany.Connected)
            {

                SAPbobsCOM.Documents oOrder;
                //SAPbobsCOM.SerialNumbers oSerialNumbers;
                //SAPbobsCOM.BatchNumbers oBatchNumbers;

                oOrder = (SAPbobsCOM.Documents)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oOrders);

                int lRetCode;
                string InvCodeStr = "";
                string _AccountCode = "";
                string _ItemDescription = "";

                string _ItmsGrpNam = "";
                int _ItmsGrpCod = 0;
                string _DocCurrency = "";
                string _WhsName = "";
                decimal _SapUnitPrice = 0;
                decimal _SapPrice = 0;
                string _AccountName = "";

                string _MovimientoId = "";
                string _MovimientoDetailId = "";
                int idTipoMovimiento = 4; //--Salidas De Bodega
                int idTipoDocumento = 9; //recepcion de producto terminado
                int idAlmacen = 5;
                int IdEstado = 0;

                global::System.Nullable<int> NumeroDocumento = 0;
                int CustomerNumber = 0;
                string CustomerName = "";
                string DocName = "";
                int DocType = 0;
                string DocNum = "";
                global::System.Nullable<global::System.DateTime> FechaDocumento = null;
                global::System.Nullable<global::System.DateTime> DocDate = null;
                int _SkidId = 0;

                string _CustomerUOM = "";
                int _NbrOfUnits = 0;
                string _MachineId = "";
                string _RevenuesAc = "";
                string _MarkInfo = "";
                DateTime _RequestedDeliveryDate = DateTime.Now; 

                for (int i = 0; i <= _vw_Order_Full.Rows.Count - 1; i++)
                {
                    _ItmsGrpNam = "";
                    _ItmsGrpCod = 0;
                    _SapUnitPrice = 0;
                    _SapPrice = 0;
                    _AccountName = "";
                    _SkidId = 0;
                    _MarkInfo = "";


                    if (i == 0)
                    {
                        _CardCode = _vw_Order_Full[i].CustomerNumber.ToString();
                        //valid _cardcode exists sap
                        DbSapTableAdapters.OCRDTableAdapter _tblOCRD = new DbSapTableAdapters.OCRDTableAdapter();
                        _tblOCRD.Connection.ConnectionString = GT.System_GT.Get_DbTori(_CompanyNumber);
                        if (_tblOCRD.GetDataBy_CardCode(_CardCode).Rows.Count == 0)
                        {
                            DbCustServTableAdapters.tblCustomerTableAdapter _oCustomer = new DbCustServTableAdapters.tblCustomerTableAdapter();
                            _oCustomer.Connection.ConnectionString = GT.System_GT.Get_DbTori(_CompanyNumber);
                            _oCustomer.Update_SAP(Convert.ToInt32(_CardCode));
                        }

                        CustomerNumber = _vw_Order_Full[i].CustomerNumber;
                        CustomerName = _vw_Order_Full[i].CustomerName;
                        _CustomerUOM = _vw_Order_Full[i].CustomerUOM;
                        _DocCurrency = _vw_Order_Full[i].CurrencyId;
                        try {_MarkInfo = _vw_Order_Full[i].MarkInfo; }catch {}

                        DbSapTableAdapters.OCRNTableAdapter _DbOCRN = new OCRNTableAdapter();
                        _DbOCRN.Connection.ConnectionString = GT.System_GT.Get_DbTori(_CompanyNumber);
                        DbSap.OCRNDataTable _tblOCRN = new DbSap.OCRNDataTable();

                        _tblOCRN = _DbOCRN.GetDataBy_DocCurrCod(_DocCurrency);

                        if (_tblOCRN.Rows.Count > 0)
                        {
                            _DocCurrency = _tblOCRN[0].CurrCode;
                        }

                        DbSapTableAdapters.OWHSTableAdapter _dbOHS = new OWHSTableAdapter();
                        DbSap.OWHSDataTable _tblOWHS = new DbSap.OWHSDataTable();

                        _tblOWHS = _dbOHS.GetDataBy_WhsCode(_WarehouseCode);

                        if (_tblOWHS.Rows.Count > 0)
                        {
                            _WhsName = _tblOWHS[0].WhsName;
                            _RevenuesAc = _tblOWHS[0].RevenuesAc;
                        }

                        oOrder.CardCode = _CardCode;
                        oOrder.BPL_IDAssignedToInvoice = _Sucursal;
                        oOrder.DocDate = DateTime.Now.Date;
                        oOrder.TaxDate = DateTime.Now.Date;
                        oOrder.DocDueDate = _RequestedDeliveryDate;
                        oOrder.Comments = GT.System_GT.Left(_Comments + ' ' +  _MarkInfo , 254);
                        oOrder.DocType = SAPbobsCOM.BoDocumentTypes.dDocument_Items;
                        oOrder.ManualNumber = "N";
                        oOrder.JournalMemo = GT.System_GT.Left(_Comments, 50);
                
                        try { oOrder.NumAtCard = _vw_Order_Full[i].CustPONbr; }
                        catch { }

                        oOrder.UserFields.Fields.Item("U_Comision").Value = _vw_Order_Full[i].CommissionPerc.ToString();
                        oOrder.UserFields.Fields.Item("U_Margen").Value = _vw_Order_Full[i].MargenNeto.ToString();

                        //kardex header 
                        try
                        {
                            DB_INVTableAdapters.Tbl_INV_Mov_HeaderTableAdapter _DbInv_Header = new DB_INVTableAdapters.Tbl_INV_Mov_HeaderTableAdapter();

                            _MovimientoId = _DbInv_Header.Insert_Header(idTipoMovimiento, idTipoDocumento, IdEstado, _CompanyNumber, NumeroDocumento, CustomerNumber, CustomerName, DocName, DocType, DocNum, FechaDocumento, DocDate);

                        }
                        catch (Exception ex)
                        {
                            GT.System_GT.f_error(ex, "oOrder.cs", "");
                        }


                    } //if (i == 0)

                    _InternalPartNbr = _vw_Order_Full[i].InternalPartNbr;
                    _Quantity = Convert.ToDouble(_vw_Order_Full[i].Quantity);
                    _Price = Convert.ToDouble( _vw_Order_Full[i].PricePerUnit);

                    try
                    {
                        _OrderNumber = _vw_Order_Full[i].OrderNumber;
                    }
                    catch { }

                    if (_vw_Order_Full[i].CustomerUOM == "Millar")
                    {
                        _Quantity = _Quantity / 1000;

                    }

                    //----InternalPart
                    DbSapTableAdapters.OITMTableAdapter _DbOITM = new OITMTableAdapter();
                    _DbOITM.Connection.ConnectionString = GT.System_GT.Get_DbTori(_CompanyNumber);
                    DbSap.OITMDataTable _OITM = new DbSap.OITMDataTable();

                    if (_InternalPartNbr != "")
                    {
                        _OITM = _DbOITM.GetDataBy_ItemCode(_InternalPartNbr.Substring(0, 8));
                    }
                    else
                    {
                        _OITM = _DbOITM.GetDataBy_ItemCode(_ItemCode);
                    }

                    if (_OITM.Rows.Count > 0)
                    {
                        _ItemCode = _OITM[0].ItemCode;
                        _ItemDescription = _OITM[0].ItemName;
                        _ItmsGrpCod = _OITM[0].ItmsGrpCod;

                        DbSapTableAdapters.OITBTableAdapter _dbOITB = new OITBTableAdapter();
                        _ItmsGrpNam = _dbOITB.GetDataBy_ItmsGrpCod(Convert.ToInt16(_ItmsGrpCod))[0].ItmsGrpNam;

                    }
                    else
                    {
                        DbPPrensaTableAdapters.v_ProdDefinitionsTableAdapter _dbProdDefinitions = new DbPPrensaTableAdapters.v_ProdDefinitionsTableAdapter();
                        _dbProdDefinitions.Update_SAP(_InternalPartNbr);

                        _ItemCode = GT.System_GT.Left(_InternalPartNbr, 8);
                        _ItemDescription = _InternalPartNbr;
                    }

                    //oOrder.Lines.BaseEntry = Convert.ToInt32(InvCodeStr);
                    if (i != 0)
                    {
                        oOrder.Lines.Add();
                    }
                    oOrder.Lines.ItemCode = _ItemCode;
                    oOrder.Lines.AccountCode = _AccountCode;
                    oOrder.Lines.ItemDescription = _ItemDescription;
                    oOrder.Lines.WarehouseCode = _WarehouseCode;
                    oOrder.Lines.PriceAfterVAT = _Price;
                    oOrder.Lines.Quantity = _Quantity;
                    oOrder.Lines.AccountCode = _RevenuesAc;

                    if (i == _vw_Order_Full.Rows.Count - 1)
                    {
                        lRetCode = oOrder.Add();
                        if (lRetCode != 0)
                        {
                            int lErrCode;
                            string sErrMsg;
                            oCompany.GetLastError(out lErrCode, out sErrMsg);
                            sErrMsg += "[" + _Comments + "]";
                            throw new Exception(sErrMsg);
                        }

                        oCompany.GetNewObjectCode(out InvCodeStr);
                        _return = InvCodeStr;

                    }
                } //for (int i = 0; i <= _vw_Order_Full.Rows.Count - 1; i++)
            }
            else
            {
                throw new Exception("Compania no conectada");
            }
        }
       
    }

}