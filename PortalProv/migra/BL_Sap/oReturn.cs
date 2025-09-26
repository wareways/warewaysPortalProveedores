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

    public partial class oReturnTableAdapter
    {

        [DataObjectMethod
        (DataObjectMethodType.Insert, true)]
        public void Insert_oReturn_Shipping(int ShipNumber, string Usuario)
        {
            //----------------------------------------------------------------

            string _InternalPartNbr = "";
            double _Price = 0;
            double _Quantity = 0;
            double _QuantitySubTotal = 0;
            string _WarehouseCode = "";
            string _BatchNumber = "";
            decimal _Weight = 0;
            decimal _WeightTotal = 0;

            string _Comments = "Retorno Embarque No.: " + ShipNumber.ToString() + " Por: " + Usuario;
            string _ItemCode = "";
            //string _Account = "511010011100";
            string _Account = "";
            //string _UbicacinDefault = "11";
            int _CompanyNumber = 0;
            int _OrderNumber = 0;
            string _CardCode = "";
            string _TaxGroupId = "";
            int _Sucursal = 0;

            int BaseType = 0;
            string BaseEntry = ""; 

            //Datos Tarima incluye costo. 
            DbCustServTempTableAdapters.vwTarimasRollTrackingTableAdapter _DbVwTarimasRollTracking = new DbCustServTempTableAdapters.vwTarimasRollTrackingTableAdapter();
            DbCustServTemp.vwTarimasRollTrackingDataTable _VwTarimasRollTracking = new DbCustServTemp.vwTarimasRollTrackingDataTable();
            _VwTarimasRollTracking = _DbVwTarimasRollTracking.GetDataBy_Return_Shipping(ShipNumber);

            
            if (_VwTarimasRollTracking.Rows.Count > 0)
            {
                _CompanyNumber = _VwTarimasRollTracking[0].Empresa;
                BaseType = _VwTarimasRollTracking[0].DocType;
                BaseEntry = _VwTarimasRollTracking[0].DocNum.ToString();
            }
            else
            {
                throw new Exception("No se encontró información de tarima de embarque no.:" + ShipNumber.ToString());
            }

            string _return = "";
            SAPbobsCOM.Company oCompany;
            oCompany = GlobalSAP.GetCompany(_CompanyNumber);
            _Sucursal = GlobalSAP.GetSucursal(_CompanyNumber);
            _WarehouseCode = GlobalSAP.GetWarehouseCode(_CompanyNumber);
            // _Account = GlobalSAP.GetAccountPT(_CompanyNumber);

            if (oCompany.Connected)
            {
                SAPbobsCOM.Documents oReturn;
                //SAPbobsCOM.SerialNumbers oSerialNumbers;
                SAPbobsCOM.BatchNumbers oBatchNumbers;
                oReturn = (SAPbobsCOM.Documents)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oReturns);
                int lRetCode;
                string InvCodeStr = "";
                string _AccountCode = "";
                string _ItemDescription = "";

                string _ItmsGrpNam = "";
                int _ItmsGrpCod = 0;
                DateTime _DocDate;
                string _DocCurrency = "";
                int _DocType = 0;
                int _DocNum = 0;
                int _ShipDetailId = 0;

                int _LineNum = 0;
                string _DocName = "";
                string _WhsName = "";
                decimal _SapUnitPrice = 0;
                decimal _SapPrice = 0;
                string _AccountName = "";

                string _MovimientoId = "";
                string _MovimientoDetailId = "";
                int idTipoMovimiento = 2; //--Ingreso
                int idTipoDocumento = 6; //Recepción Por Devolución
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
                int _NbrOfUnitsTotal = 0;
                string _MachineId = "";
                string _RevenuesAc = "";
                string _MarkInfo = "";
                string _LastInternalPartNbr = "";
                int iBatch = 0;
                int iLine = 0;
                string _NIT = "";
                string _CustPONbr = "";

                string _U_TipoA2 = "";

                for (int i = 0; i <= _VwTarimasRollTracking.Rows.Count - 1; i++)
                {
                    _ItmsGrpNam = "";
                    _ItmsGrpCod = 0;
                    _DocType = 0;
                    _DocNum = 0;
                    _DocName = "";
                    _SapUnitPrice = 0;
                    _SapPrice = 0;
                    _AccountName = "";
                    _SkidId = 0;
                    _TaxGroupId = "";
                    _MarkInfo = "";
                    _ShipDetailId = 0;
                    _LineNum = 0; 

                    if (i == 0) // header 
                    {
                        _CardCode = _VwTarimasRollTracking[i].CustomerNumber.ToString();
                        //valid _cardcode exists sap
                        DbSapTableAdapters.OCRDTableAdapter _tblOCRD = new DbSapTableAdapters.OCRDTableAdapter();

                        if (_tblOCRD.GetDataBy_CardCode(_CardCode).Rows.Count == 0)
                        {
                            DbCustServTableAdapters.tblCustomerTableAdapter _oCustomer = new DbCustServTableAdapters.tblCustomerTableAdapter();
                            _oCustomer.Connection.ConnectionString = GT.System_GT.Get_DbTori(_CompanyNumber);

                            _oCustomer.Update_SAP(Convert.ToInt32(_CardCode));
                        }

                        CustomerNumber = _VwTarimasRollTracking[i].CustomerNumber;
                        CustomerName = _VwTarimasRollTracking[i].CustomerName;
                        _CustomerUOM = _VwTarimasRollTracking[i].CustomerUOM;

                        try
                        {
                            _TaxGroupId = _VwTarimasRollTracking[i].TaxGroupId;
                        }
                        catch { }

                        try { _MachineId = _VwTarimasRollTracking[i].MachineID; }
                        catch { }
                        _DocCurrency = _VwTarimasRollTracking[i].CurrencyId;
                        try { _MarkInfo = _VwTarimasRollTracking[i].MarkInfo; }
                        catch { }


                        DbSapTableAdapters.OCRNTableAdapter _DbOCRN = new OCRNTableAdapter();
                        _DbOCRN.Connection.ConnectionString = GT.System_GT.Get_DbTori(_CompanyNumber);
                        DbSap.OCRNDataTable _tblOCRN = new DbSap.OCRNDataTable();

                        _tblOCRN = _DbOCRN.GetDataBy_DocCurrCod(_DocCurrency);

                        if (_tblOCRN.Rows.Count > 0)
                        {
                            _DocCurrency = _tblOCRN[0].CurrCode;
                        }
                        else
                        {
                            _DocCurrency = "QTZ";
                        }

                        DbSapTableAdapters.OWHSTableAdapter _dbOHS = new OWHSTableAdapter();
                        DbSap.OWHSDataTable _tblOWHS = new DbSap.OWHSDataTable();

                        _tblOWHS = _dbOHS.GetDataBy_WhsCode(_WarehouseCode);

                        if (_tblOWHS.Rows.Count > 0)
                        {
                            _WhsName = _tblOWHS[0].WhsName;
                            _RevenuesAc = _tblOWHS[0].RevenuesAc;
                        }

                        oReturn.CardCode = _CardCode;
                        oReturn.DocDate = DateTime.Now.Date;
                        oReturn.TaxDate = DateTime.Now.Date;
                        oReturn.Comments = GT.System_GT.Left(_Comments + ' ' + _MarkInfo, 254);
                        oReturn.DocType = SAPbobsCOM.BoDocumentTypes.dDocument_Items;
                        oReturn.ManualNumber = "N";
                        oReturn.JournalMemo = GT.System_GT.Left(_Comments, 50);
                        oReturn.BPL_IDAssignedToInvoice = _Sucursal;
                        oReturn.DocCurrency = _DocCurrency;

                        //Vendedor
                        DbSapTableAdapters.OSLPTableAdapter _DbOSLP = new OSLPTableAdapter();
                        _DbOSLP.Connection.ConnectionString = GT.System_GT.Get_DbTori(_CompanyNumber);
                        DbSap.OSLPDataTable _tblOSLP = new DbSap.OSLPDataTable();

                        _tblOSLP = _DbOSLP.GetData_CardCode(_CardCode);

                        if (_tblOSLP.Rows.Count > 0)
                        {
                            oReturn.SalesPersonCode = _tblOSLP[0].SlpCode;
                        }
                        else
                        {
                            //oBP.SalesPersonCode = Convert.ToInt32(Insert_Vendedores(oCompanyNumber, _SalesRepName.Replace("Geo ", ""), _SalesAbr));
                            if (oReturn.SalesPersonCode <= 0)
                            {
                                //GT.System_GT.f_error(new Exception("Cliente no tiene vendedor" + _SalesRepName), "oBusinessPartners.cs", "");
                                throw new Exception("Cliente no tiene vendedor: " + _CardCode);
                            }
                        }

                        try { oReturn.NumAtCard = _VwTarimasRollTracking[i].CustPONbr; }
                        catch { }


                        oReturn.UserFields.Fields.Item("U_Embarque").Value = _VwTarimasRollTracking[i].ShipmentId.ToString();

                        try
                        {
                            oReturn.UserFields.Fields.Item("U_Comision").Value = _VwTarimasRollTracking[i].CommissionPerc.ToString();
                        }
                        catch { }

                        try
                        {
                            oReturn.UserFields.Fields.Item("U_Margen").Value = _VwTarimasRollTracking[i].MargenNeto.ToString();
                        }
                        catch { }

                        try
                        {
                            oReturn.UserFields.Fields.Item("U_FacNom").Value = _VwTarimasRollTracking[i].CustomerName.ToString();
                        }
                        catch { }

                        try
                        {
                            oReturn.UserFields.Fields.Item("U_FacNit").Value = _VwTarimasRollTracking[i].NIT.ToString();
                        }
                        catch { }

                        try
                        {
                            oReturn.UserFields.Fields.Item("U_OV").Value = _VwTarimasRollTracking[i].ShipOrderNumber.ToString();
                        }
                        catch { }

                        try
                        {
                            oReturn.UserFields.Fields.Item("U_OC").Value = _VwTarimasRollTracking[i].CustPONbr.ToString();
                        }
                        catch { }

                        try
                        {
                            oReturn.UserFields.Fields.Item("U_PM").Value = _VwTarimasRollTracking[i].PackgingMethod.ToString();
                        }
                        catch { }

                        //kardex header 
                        try
                        {
                            DB_INVTableAdapters.Tbl_INV_Mov_HeaderTableAdapter _DbInv_Header = new DB_INVTableAdapters.Tbl_INV_Mov_HeaderTableAdapter();

                            try
                            {
                                _DbInv_Header.UpdateQuery_CancelShipping(ShipNumber);
                            }
                            catch (Exception ex)
                            {
                                GT.System_GT.f_error(ex, "oReturn.cs", "");
                            }

                            _MovimientoId = _DbInv_Header.Insert_Header(idTipoMovimiento, idTipoDocumento, IdEstado, _CompanyNumber, NumeroDocumento, CustomerNumber, CustomerName, DocName, DocType, DocNum, FechaDocumento, DocDate);

                        }
                        catch (Exception ex)
                        {
                            GT.System_GT.f_error(ex, "oReturn.cs", "");
                        }


                    } //if (i == 0)

                    _InternalPartNbr = _VwTarimasRollTracking[i].InternalPartNbr;
                    _Quantity = Convert.ToDouble(_VwTarimasRollTracking[i].RemainingQty);
                    _Weight = _VwTarimasRollTracking[i].RemainingWeight;
                    _WeightTotal += _VwTarimasRollTracking[i].RemainingWeight;
                    _BatchNumber = _VwTarimasRollTracking[i].SkidId.ToString();
                    _SkidId = _VwTarimasRollTracking[i].SkidId;
                    _ShipDetailId = _VwTarimasRollTracking[i].ShipDetailId; 
                    _LineNum = _VwTarimasRollTracking[i].LineNum;
                    try
                    {
                        _Price = Convert.ToDouble(_VwTarimasRollTracking[i].PricePerUnit);

                        if (_VwTarimasRollTracking[i].CustomerUOM == "MSM")
                        {
                            _Price = Convert.ToDouble(Math.Round(_VwTarimasRollTracking[i].PricePerUnit * 1000, 3));
                        }
                    }
                    catch (Exception ex)
                    {
                        GT.System_GT.f_error(new Exception(ex.Message + " Error en Costo Producción, Shipping." + ShipNumber.ToString()), "tblShipping.cs", "");
                    }

                    try
                    {
                        _OrderNumber = _VwTarimasRollTracking[i].OrderNumber;
                    }
                    catch { }

                    if (_VwTarimasRollTracking[i].CustomerUOM == "Millar" || _VwTarimasRollTracking[i].CustomerUOM == "MSM" || _VwTarimasRollTracking[i].CustomerUOM == "KImp")
                    {
                        _Quantity = Math.Round(_Quantity / 1000, 3);
                        //_Quantity = Convert.ToDouble(Math.Round(Convert.ToDecimal(_Quantity), MidpointRounding.AwayFromZero));
                    }

                    if (_VwTarimasRollTracking[i].CustomerUOM == "Libra" || _VwTarimasRollTracking[i].CustomerUOM == "Kg")
                    {
                        _Quantity = Math.Round(_Quantity, 2);
                    }

                    //

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
                        try { _U_TipoA2 = _OITM[0].U_TipoA2; }
                        catch { _U_TipoA2 = "BB"; }

                        if (_TaxGroupId == "EXPORT")
                        {
                            _U_TipoA2 = "I";
                        }

                        DbSapTableAdapters.OITBTableAdapter _dbOITB = new OITBTableAdapter();
                        _ItmsGrpNam = _dbOITB.GetDataBy_ItmsGrpCod(Convert.ToInt16(_ItmsGrpCod))[0].ItmsGrpNam;
                    }
                    else
                    {
                        //DbPPrensaTableAdapters.v_ProdDefinitionsTableAdapter _dbProdDefinitions = new DbPPrensaTableAdapters.v_ProdDefinitionsTableAdapter();
                        //_dbProdDefinitions.Update_SAP(_InternalPartNbr);

                        //_ItemCode = GT.System_GT.Left(_InternalPartNbr, 8);
                        //_ItemDescription = _InternalPartNbr;
                    }

                    //Lines 
                    if (_LastInternalPartNbr != _ItemCode)
                    {
                        if (i != 0) // Lines 
                        {
                            oReturn.Lines.Add();
                            iBatch = 0;
                            iLine += 1;
                        }

                        oReturn.Lines.SetCurrentLine(iLine);

                        //oReturn.Lines.AccountCode = _AccountCode;

                        if (BaseType != 0) { 
                            oReturn.Lines.BaseType = BaseType ;
                            oReturn.Lines.BaseEntry = Convert.ToInt32(BaseEntry) ;
                            oReturn.Lines.BaseLine = _LineNum; 
                        }
                        oReturn.Lines.WarehouseCode = _WarehouseCode;
                        oReturn.Lines.ItemCode = _ItemCode;
                        oReturn.Lines.ItemDescription = _ItemDescription;
                        oReturn.Lines.PriceAfterVAT = _Price;
                        //oReturn.Lines.ReturnCost = _Price; 
                        oReturn.Lines.AccountCode = _RevenuesAc;
                        oReturn.Lines.UserFields.Fields.Item("U_TipoA").Value = _U_TipoA2;

                        
                    }
                    //===============================================
                    //'bacthes
                    oBatchNumbers = oReturn.Lines.BatchNumbers;
                    if (iBatch != 0)
                    {
                        oBatchNumbers.Add();
                    }
                    //oBatchNumbers.BaseLineNumber = oReturn.Lines.BaseLine;
                    oBatchNumbers.SetCurrentLine(iBatch);
                    oBatchNumbers.BatchNumber = _BatchNumber;
                    oBatchNumbers.Quantity = Math.Round(_Quantity, 3);
                    oBatchNumbers.Location = _WarehouseCode;

                    oBatchNumbers.UserFields.Fields.Item("U_Peso").Value = _Weight.ToString();
                    oBatchNumbers.UserFields.Fields.Item("U_Pedido").Value = _OrderNumber.ToString();
                    _QuantitySubTotal += Math.Round(_Quantity, 3);

                    _NbrOfUnits = _VwTarimasRollTracking[i].NbrOfUnits;
                    _NbrOfUnitsTotal += _VwTarimasRollTracking[i].NbrOfUnits;


                    iBatch += 1;
                    // oBatchNumbers.Add();

                    try { _LastInternalPartNbr = GT.System_GT.Left(_VwTarimasRollTracking[i + 1].InternalPartNbr, 8); }
                    catch { _LastInternalPartNbr = ""; }

                    if (_LastInternalPartNbr != _ItemCode)
                    {
                        oReturn.Lines.Quantity = _QuantitySubTotal;
                        //oReturn.Lines.Add(); 
                        _QuantitySubTotal = 0;
                    }

                    try { _LastInternalPartNbr = GT.System_GT.Left(_VwTarimasRollTracking[i].InternalPartNbr, 8); }
                    catch { _LastInternalPartNbr = ""; }

                    _Quantity = Math.Round(_Quantity, 3);

                    // kardex detail 
                    try
                    {
                        DB_INVTableAdapters.Tbl_INV_Mov_DetailTableAdapter _DbInv_Detail = new DB_INVTableAdapters.Tbl_INV_Mov_DetailTableAdapter();
                        _MovimientoDetailId = _DbInv_Detail.Insert_Detail(new Guid(_MovimientoId), _WarehouseCode, _OrderNumber, _SkidId, _ItemCode, _InternalPartNbr, Convert.ToDecimal(_Quantity), _CustomerUOM, _NbrOfUnits, _Weight, Convert.ToDecimal(_Price), Convert.ToDecimal(_Price * _Quantity), 0, _ItemDescription, _ItmsGrpCod, _ItmsGrpNam, _BatchNumber, _WarehouseCode, _WhsName, _Comments, _DocCurrency, Convert.ToDecimal(_Price), Convert.ToDecimal(_Price * _Quantity), _AccountCode, 0, _MachineId, ShipNumber, _ShipDetailId, idAlmacen, iLine);
                    }
                    catch (Exception ex)
                    {
                        GT.System_GT.f_error(ex, "oReturn.cs", "");
                    }

                    // EOF
                    if (i == _VwTarimasRollTracking.Rows.Count - 1)
                    {

                        try
                        {
                            oReturn.UserFields.Fields.Item("U_TotalRollos").Value = _NbrOfUnitsTotal;
                        }
                        catch { }
                        try
                        {
                            oReturn.UserFields.Fields.Item("U_TotalPesoKG").Value = _WeightTotal.ToString();
                        }
                        catch { }


                        lRetCode = oReturn.Add();
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

                        //------------------------------actualiza kardex
                        DbSapTableAdapters.vw_OITLTableAdapter _Db_vw_OITL = new vw_OITLTableAdapter();
                        DbSap.vw_OITLDataTable _tbl_vw_OITL = new DbSap.vw_OITLDataTable();

                        _tbl_vw_OITL = _Db_vw_OITL.GetDataBy_DocNum(_CompanyNumber, 16, Convert.ToInt32(InvCodeStr));

                        if (_tbl_vw_OITL.Rows.Count > 0)
                        {
                            _DocDate = _tbl_vw_OITL[0].DocDate;
                            _DocCurrency = _tbl_vw_OITL[0].Currency;
                            _DocType = _tbl_vw_OITL[0].DocType;
                            _DocName = _tbl_vw_OITL[0].Code;
                            _DocNum = _tbl_vw_OITL[0].DocNum;

                            _WhsName = _tbl_vw_OITL[0].WhsName;
                            _AccountCode = _tbl_vw_OITL[0].InvntAct;

                            //account

                            DbSapTableAdapters.OACTTableAdapter _dbAccount = new OACTTableAdapter();

                            _AccountName = _dbAccount.GetDataBy_AcctCode(_AccountCode)[0].AcctName;

                            DB_INVTableAdapters.Tbl_INV_Mov_DetailTableAdapter _dbinv = new DB_INVTableAdapters.Tbl_INV_Mov_DetailTableAdapter();

                            _dbinv.Update_Detail_Delivery(Convert.ToInt32(InvCodeStr), _WarehouseCode, _WhsName, _Comments, _DocCurrency, _AccountCode, _DocName, _DocType, _DocNum.ToString(), _DocDate, new Guid(_MovimientoId));

                        }
                        else
                        {
                            GT.System_GT.f_error(new Exception("no se encontraron datos en sap."), "vw_OITL", "");
                        }


                    }
                } //for (int i = 0; i <= _VwTarimasRollTracking.Rows.Count - 1; i++)
            }
            else
            {
                throw new Exception("Compania no conectada");
            }
        }

       
    }

}