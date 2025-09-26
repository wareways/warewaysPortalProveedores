using Sap_Service;
using SAPbobsCOM;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;


namespace DbSapTableAdapters
{
    public partial class oStockTransfer
    {
        TORIEntities1 _Db_Tori = new TORIEntities1();

        public oStockTransfer()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        [DataObjectMethod
        (DataObjectMethodType.Insert, true)]
        public void Insert_StockTransfer_Shipping(int ShipNumber, string Usuario)
        {
            string _InternalPartNbr = "";
            double _Price = 0;
            double _Quantity = 0;
            double _QuantitySubTotal = 0;            
            string _BatchNumber = "";
            decimal _Weight = 0;
            decimal _WeightTotal = 0;
            string _Comments = "Embarque No.: " + ShipNumber.ToString() + " Por: " + Usuario;
            string _ItemCode = "";            
            string _Account = "";            
            int _CompanyNumber = 0;
            int _OrderNumber = 0;
            string _CardCode = "";
            string _TaxGroupId = "";
            int _Sucursal = 0;

            //Datos Tarima incluye costo. 
            DbCustServTempTableAdapters.vwTarimasRollTrackingTableAdapter _DbVwTarimasRollTracking = new DbCustServTempTableAdapters.vwTarimasRollTrackingTableAdapter();
            DbCustServTemp.vwTarimasRollTrackingDataTable _VwTarimasRollTracking = new DbCustServTemp.vwTarimasRollTrackingDataTable();
            _DbVwTarimasRollTracking.SelectCommandTimeout = 300;
            _VwTarimasRollTracking = _DbVwTarimasRollTracking.GetDataBy_ShipNumberRound(ShipNumber, 6);

            var CustomerNumberCheck = _VwTarimasRollTracking[0].CustomerNumber;
            // Trae Excepciones 
            var _ListaExepciones = (from l in _Db_Tori.tblCustomer_UserData
                                    where l.DecimalesCliente != null && l.CustomerNumer == CustomerNumberCheck
                                    select l).ToList();
            // Trae Datos Corregidos si Fuere Necesario
            if (_ListaExepciones.Count == 1)
            {
                _VwTarimasRollTracking = _DbVwTarimasRollTracking.GetDataBy_ShipNumberRound(ShipNumber, Int32.Parse(_ListaExepciones[0].DecimalesCliente.ToString()));
            }

            if (_VwTarimasRollTracking.Rows.Count > 0)
            {
                _CompanyNumber = _VwTarimasRollTracking[0].Empresa;

            }
            else
            {
                throw new Exception("No se encontró información de tarima de embarque no.:" + ShipNumber.ToString());
            }

            if (_CompanyNumber == 2) { throw new Exception(" No se puede realizar traslado de Empresa Polinter, solo polytec "); }


            string _return = "";
            SAPbobsCOM.Company oCompany;
            oCompany = GlobalSAP.GetCompany(_CompanyNumber);
            _Sucursal = GlobalSAP.GetSucursal(_CompanyNumber);
            String _WarehouseCodeSource = GlobalSAP.GetWarehouseCode(_CompanyNumber);
            String _WarehouseCodeTarget = GlobalSAP.GetWarehouseCodeEnvio(_CompanyNumber);

            if (oCompany.Connected)
            {
                SAPbobsCOM.StockTransfer oStockTransfer;                
                SAPbobsCOM.BatchNumbers oBatchNumbers;                
                oStockTransfer = (SAPbobsCOM.StockTransfer)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oStockTransfer);

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

                string _DocName = "";
                string _WhsName = "";
                decimal _SapUnitPrice = 0;
                decimal _SapPrice = 0;
                string _AccountName = "";

                string _MovimientoId = "";
                string _MovimientoDetailId = "";
                int idTipoMovimiento = 4; //--Salidas De Bodega
                int idTipoDocumento = 10; //recepcion de producto terminado
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

                        try { _TaxGroupId = _VwTarimasRollTracking[i].TaxGroupId; } catch { }

                        try { _MachineId = _VwTarimasRollTracking[i].MachineID; } catch { }
                        _DocCurrency = _VwTarimasRollTracking[i].CurrencyId;
                        try { _MarkInfo = _VwTarimasRollTracking[i].MarkInfo; } catch { }


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

                        _tblOWHS = _dbOHS.GetDataBy_WhsCode(_WarehouseCodeSource);

                        if (_tblOWHS.Rows.Count > 0)
                        {
                            _WhsName = _tblOWHS[0].WhsName;
                            _RevenuesAc = _tblOWHS[0].RevenuesAc;
                        }

                        oStockTransfer.FromWarehouse = _WarehouseCodeSource;
                        oStockTransfer.ToWarehouse = _WarehouseCodeTarget;   

                        oStockTransfer.CardCode = _CardCode;
                        oStockTransfer.DocDate = DateTime.Now.Date;
                        oStockTransfer.TaxDate = DateTime.Now.Date;
                        oStockTransfer.Comments = GT.System_GT.Left(_Comments + ' ' + _MarkInfo, 254);
                        // OJO
                        //oStockTransfer.DocType = SAPbobsCOM.BoDocumentTypes.dDocument_Items;
                        //oStockTransfer.ManualNumber = "N";
                        oStockTransfer.JournalMemo = GT.System_GT.Left(_Comments, 50);

                        // OJO
                        //oStockTransfer.BPL_IDAssignedToInvoice = _Sucursal;
                        //oStockTransfer.DocCurrency = _DocCurrency;

                        //Vendedor
                        DbSapTableAdapters.OSLPTableAdapter _DbOSLP = new OSLPTableAdapter();
                        _DbOSLP.Connection.ConnectionString = GT.System_GT.Get_DbTori(_CompanyNumber);
                        DbSap.OSLPDataTable _tblOSLP = new DbSap.OSLPDataTable();

                        _tblOSLP = _DbOSLP.GetData_CardCode(_CardCode);

                        if (_tblOSLP.Rows.Count > 0)
                        {
                            oStockTransfer.SalesPersonCode = _tblOSLP[0].SlpCode;
                        }
                        else
                        {
                            
                            if (oStockTransfer.SalesPersonCode <= 0)
                            {                            
                                throw new Exception("Cliente no tiene vendedor: " + _CardCode);
                            }
                        }

                        // OJO
                        //try { oStockTransfer.NumAtCard = _VwTarimasRollTracking[i].CustPONbr; } catch { }


                        oStockTransfer.UserFields.Fields.Item("U_Embarque").Value = _VwTarimasRollTracking[i].ShipmentId.ToString();

                        try
                        {
                            oStockTransfer.UserFields.Fields.Item("U_Comision").Value = _VwTarimasRollTracking[i].CommissionPerc.ToString();
                        }
                        catch { }

                        try
                        {
                            oStockTransfer.UserFields.Fields.Item("U_Margen").Value = _VwTarimasRollTracking[i].MargenNeto.ToString();
                        }
                        catch { }

                        try
                        {
                            oStockTransfer.UserFields.Fields.Item("U_FacNom").Value = _VwTarimasRollTracking[i].CustomerName.ToString();
                        }
                        catch { }

                        try
                        {
                            oStockTransfer.UserFields.Fields.Item("U_FacNit").Value = _VwTarimasRollTracking[i].NIT.ToString();
                        }
                        catch { }

                        try
                        {
                            oStockTransfer.UserFields.Fields.Item("U_OV").Value = _VwTarimasRollTracking[i].ShipOrderNumber.ToString();
                        }
                        catch { }

                        try
                        {
                            Int32 _p_OrderNumber = Int32.Parse(_VwTarimasRollTracking[i].ShipOrderNumber.ToString());

                            var _Lista = (from l in _Db_Tori.vw_OrderDetail_WallmartData where l.OrderNumber == _p_OrderNumber select l).ToList();
                            if (_Lista.Any())
                            {
                                if (_Lista[0].UbicacionIso != "")
                                {
                                    oStockTransfer.UserFields.Fields.Item("U_W_IdUbica").Value = _Lista[0].UbicacionIso;
                                    try
                                    {
                                        oStockTransfer.UserFields.Fields.Item("U_W_OtrImp").Value = 0;
                                        oStockTransfer.UserFields.Fields.Item("U_W_OtrPor").Value = 0;
                                    }
                                    catch { }

                                    try { oStockTransfer.UserFields.Fields.Item("U_W_Albaran").Value = "0000"; } catch { }
                                    try { oStockTransfer.UserFields.Fields.Item("U_W_IdProv").Value = _Lista[0].ProveedorIso; } catch { }
                                    try { oStockTransfer.UserFields.Fields.Item("U_W_Transporte").Value = _Lista[0].TransporteIso; } catch { }
                                    try { oStockTransfer.UserFields.Fields.Item("U_W_OC").Value = _Lista[0].OrdenCompra; } catch { }

                                }
                                else
                                {
                                    oStockTransfer.UserFields.Fields.Item("U_W_Albaran").Value = "Sin Datos";
                                }

                            }

                        }
                        catch { }



                        try
                        {
                            oStockTransfer.UserFields.Fields.Item("U_OC").Value = _VwTarimasRollTracking[i].CustPONbr.ToString();
                        }
                        catch { }

                        try
                        {
                            oStockTransfer.UserFields.Fields.Item("U_PM").Value = _VwTarimasRollTracking[i].PackgingMethod.ToString();
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
                                GT.System_GT.f_error(ex, "oDeliveryNote.cs", "");
                            }

                            _MovimientoId = _DbInv_Header.Insert_Header(idTipoMovimiento, idTipoDocumento, IdEstado, _CompanyNumber, NumeroDocumento, CustomerNumber, CustomerName, DocName, DocType, DocNum, FechaDocumento, DocDate);

                        }
                        catch (Exception ex)
                        {
                            GT.System_GT.f_error(ex, "oDeliveryNote.cs", "");
                        }


                    } 

                    _InternalPartNbr = _VwTarimasRollTracking[i].InternalPartNbr;
                    _Quantity = Convert.ToDouble(_VwTarimasRollTracking[i].RemainingQty);
                    _Weight = _VwTarimasRollTracking[i].RemainingWeight;
                    _WeightTotal += _VwTarimasRollTracking[i].RemainingWeight;
                    _BatchNumber = _VwTarimasRollTracking[i].SkidId.ToString();
                    _SkidId = _VwTarimasRollTracking[i].SkidId;
                    _ShipDetailId = _VwTarimasRollTracking[i].ShipDetailId;
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
                    }

                    if (_VwTarimasRollTracking[i].CustomerUOM == "Libra" || _VwTarimasRollTracking[i].CustomerUOM == "Kg" || _VwTarimasRollTracking[i].CustomerUOM == "Libra Espanola")
                    {
                        _Quantity = Math.Round(_Quantity, 2);

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
                        try { _U_TipoA2 = _OITM[0].U_TipoA2; }
                        catch { _U_TipoA2 = "BB"; }

                        if (_TaxGroupId == "EXPORT")
                        {
                            _U_TipoA2 = "I";
                        }


                        DbSapTableAdapters.OITBTableAdapter _dbOITB = new OITBTableAdapter();
                        _ItmsGrpNam = _dbOITB.GetDataBy_ItmsGrpCod(Convert.ToInt16(_ItmsGrpCod))[0].ItmsGrpNam;

                    }
                    
                    //Lines 
                    if (_LastInternalPartNbr != _ItemCode)
                    {
                        if (i != 0) // Lines 
                        {
                            oStockTransfer.Lines.Add();
                            iBatch = 0;
                            iLine += 1;
                        }

                        oStockTransfer.Lines.SetCurrentLine(iLine);
                        
                        //oStockTransfer.Lines.WarehouseCode = _WarehouseCodeSource;
                        oStockTransfer.Lines.ItemCode = _ItemCode;
                        oStockTransfer.Lines.ItemDescription = _ItemDescription;
                        // OJO
                        //oStockTransfer.Lines.PriceAfterVAT = _Price;
                        //oStockTransfer.Lines.AccountCode = _RevenuesAc;
                        oStockTransfer.Lines.UserFields.Fields.Item("U_TipoA").Value = _U_TipoA2;

                        try
                        {
                            oStockTransfer.Lines.UserFields.Fields.Item("U_W_GTIN").Value = Obtener_GTIN(_ItemCode, _VwTarimasRollTracking[i].ShipOrderNumber.ToString());
                        }
                        catch { }

                        var _ListaShipmentIvaExe = (from l in _Db_Tori.tblCustomerShipmentTax
                                                    where l.ShipmentNumber == ShipNumber && l.TipoFacturacion != ""
                                                    select l).ToList();
                        if (_ListaShipmentIvaExe.Count() == 1)
                        {
                            // OJO
                            //oStockTransfer.Lines.TaxCode = _ListaShipmentIvaExe[0].TipoFacturacion;
                        }
                    }

                    _Quantity = Math.Round(_Quantity, 3);
                    // Validar Existencia Nuevo Julio Herrera
                    VerifySapInventoryQuantity(oCompany, _WarehouseCodeSource, _BatchNumber, Convert.ToDecimal(_Quantity), _ItemCode, ShipNumber.ToString(), Usuario);
                    // Revisar si Tiene Acceso Bodega POLY34
                    VerifySapWarehouse34(oCompany, _ItemCode);

                    //===============================================
                    //'bacthes
                    oBatchNumbers = oStockTransfer.Lines.BatchNumbers;
                    if (iBatch != 0)
                    {
                        oBatchNumbers.Add();
                    }                    
                    oBatchNumbers.SetCurrentLine(iBatch);
                    oBatchNumbers.BatchNumber = _BatchNumber;
                    if (_VwTarimasRollTracking[i].CustomerUOM == "Millar" || _VwTarimasRollTracking[i].CustomerUOM == "MSM" || _VwTarimasRollTracking[i].CustomerUOM == "KImp")
                    {
                        oBatchNumbers.Quantity = Math.Round(_Quantity, 3);
                        _QuantitySubTotal += Math.Round(_Quantity, 3);
                    }
                    else
                    {
                        oBatchNumbers.Quantity = Math.Round(_Quantity, 2);
                        _QuantitySubTotal += Math.Round(_Quantity, 2);
                    }

                    oBatchNumbers.Location = _WarehouseCodeSource;
                    oBatchNumbers.UserFields.Fields.Item("U_Peso").Value = _Weight.ToString();
                    oBatchNumbers.UserFields.Fields.Item("U_Pedido").Value = _OrderNumber.ToString();
                    _NbrOfUnits = _VwTarimasRollTracking[i].NbrOfUnits;
                    _NbrOfUnitsTotal += _VwTarimasRollTracking[i].NbrOfUnits;

                    iBatch += 1;                   

                    try { _LastInternalPartNbr = GT.System_GT.Left(_VwTarimasRollTracking[i + 1].InternalPartNbr, 8); }
                    catch { _LastInternalPartNbr = ""; }

                    if (_LastInternalPartNbr != _ItemCode)
                    {
                        oStockTransfer.Lines.Quantity = _QuantitySubTotal;                        
                        _QuantitySubTotal = 0;
                    }

                    try { _LastInternalPartNbr = GT.System_GT.Left(_VwTarimasRollTracking[i].InternalPartNbr, 8); }
                    catch { _LastInternalPartNbr = ""; }

                    _Quantity = Math.Round(_Quantity, 3);

                    // kardex detail 
                    try
                    {
                        DB_INVTableAdapters.Tbl_INV_Mov_DetailTableAdapter _DbInv_Detail = new DB_INVTableAdapters.Tbl_INV_Mov_DetailTableAdapter();
                        _MovimientoDetailId = _DbInv_Detail.Insert_Detail(new Guid(_MovimientoId), _WarehouseCodeSource, _OrderNumber, _SkidId, _ItemCode, _InternalPartNbr, Convert.ToDecimal(_Quantity), _CustomerUOM, _NbrOfUnits, _Weight, Convert.ToDecimal(_Price), Convert.ToDecimal(_Price * _Quantity), 0, _ItemDescription, _ItmsGrpCod, _ItmsGrpNam, _BatchNumber, _WarehouseCodeSource, _WhsName, _Comments, _DocCurrency, Convert.ToDecimal(_Price), Convert.ToDecimal(_Price * _Quantity), _AccountCode, 0, _MachineId, ShipNumber, _ShipDetailId, idAlmacen, iLine);
                    }
                    catch (Exception ex)
                    {
                        GT.System_GT.f_error(ex, "oDeliveryNote.cs", "");
                    }

                    // EOF
                    if (i == _VwTarimasRollTracking.Rows.Count - 1)
                    {

                        try
                        {
                            oStockTransfer.UserFields.Fields.Item("U_TotalRollos").Value = _NbrOfUnitsTotal;
                        }
                        catch { }
                        try
                        {
                            oStockTransfer.UserFields.Fields.Item("U_TotalPesoKG").Value = _WeightTotal.ToString();
                        }
                        catch { }


                        // Verificación de Transacción para Evitar Transacción Duplicada Julio Herrera
                        // ------------------------------------------------------------
                        oStockTransfer.UserFields.Fields.Item("U_IdMovDet").Value = _ShipDetailId.ToString();
                        DB_SAP_ProduccionEntities _DB_Sap = new DB_SAP_ProduccionEntities();
                        var _ShipDetailIdString = _ShipDetailId.ToString();
                        var _Busqueda = (from l in _DB_Sap.OWTRs.AsNoTracking() where l.U_IdMovDet == _ShipDetailIdString select l).ToList();


                        if (_Busqueda.Count == 0 && _ShipDetailId != 210950)
                        {
                            lRetCode = oStockTransfer.Add();
                            if (lRetCode != 0)
                            {
                                int lErrCode;
                                string sErrMsg;
                                oCompany.GetLastError(out lErrCode, out sErrMsg);
                                sErrMsg += "[" + _Comments + "]";

                                if (sErrMsg.Contains("Item number is missing")) sErrMsg += " ,Código Enviado: " + GT.System_GT.Left(_VwTarimasRollTracking[i].InternalPartNbr, 8) + " , InternalPartNbr : " + _VwTarimasRollTracking[i].InternalPartNbr + ", Problemas Código Toriflex Revise el InternalPartNbr";

                                throw new Exception(sErrMsg);

                            }
                            oCompany.GetNewObjectCode(out InvCodeStr);


                        }
                        else
                        {
                            String _ref = "";
                            
                            if (_ShipDetailId == 210950)
                            {
                                InvCodeStr = "18153";
                            }
                            else
                            {
                                InvCodeStr = _Busqueda[0].DocNum.ToString();
                            }

                        }


                        _return = InvCodeStr;

                        //------------------------------actualiza kardex
                        //DbSapTableAdapters.vw_OITLTableAdapter _Db_vw_OITL = new vw_OITLTableAdapter();
                        //DbSap.vw_OITLDataTable _tbl_vw_OITL = new DbSap.vw_OITLDataTable();

                        //_tbl_vw_OITL = _Db_vw_OITL.GetDataBy_DocEntry(_CompanyNumber, 15, Convert.ToInt32(InvCodeStr));

                        //if (_tbl_vw_OITL.Rows.Count > 0)
                        //{
                        //    _DocDate = _tbl_vw_OITL[0].DocDate;
                        //    _DocCurrency = _tbl_vw_OITL[0].Currency;
                        //    _DocType = _tbl_vw_OITL[0].DocType;
                        //    _DocName = _tbl_vw_OITL[0].Code;
                        //    _DocNum = _tbl_vw_OITL[0].DocNum;

                        //    _WhsName = _tbl_vw_OITL[0].WhsName;
                        //    _AccountCode = _tbl_vw_OITL[0].InvntAct;

                        //    //account

                        //    DbSapTableAdapters.OACTTableAdapter _dbAccount = new OACTTableAdapter();

                        //    _AccountName = _dbAccount.GetDataBy_AcctCode(_AccountCode)[0].AcctName;

                        //    DB_INVTableAdapters.Tbl_INV_Mov_DetailTableAdapter _dbinv = new DB_INVTableAdapters.Tbl_INV_Mov_DetailTableAdapter();

                        //    _dbinv.Update_Detail_Delivery(Convert.ToInt32(InvCodeStr), _WarehouseCodeSource, _WhsName, _Comments, _DocCurrency, _AccountCode, _DocName, _DocType, _DocNum.ToString(), _DocDate, new Guid(_MovimientoId));

                        //}
                        //else
                        //{
                        //    GT.System_GT.f_error(new Exception("no se encontraron datos en Sap. ODeliveryNote_01, Codigo = " + InvCodeStr), "vw_OITL", "");
                        //}


                    }
                } 
            }
            else
            {
                throw new Exception("Compañía no conectada");
            }
        }

        private void VerifySapWarehouse34(Company oCompany, string _ItemCode)
        {
            bool isGetByKey = false;
            int lRetCode;

            SAPbobsCOM.Items oItems;
            oItems = (SAPbobsCOM.Items)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oItems);

            isGetByKey = oItems.GetByKey(_ItemCode);

            if ( isGetByKey)
            {
                Boolean _Poly34 = false;
                // Validar que Bodegas Existen
                for (int i = 0; i < oItems.WhsInfo.Count; i++)
                {
                    oItems.WhsInfo.SetCurrentLine(i);
                    if (oItems.WhsInfo.WarehouseCode == "POLY34") _Poly34 = true;
                }

                if (!_Poly34) // Agregar Bodega Porque no esta
                {
                    if (oItems.WhsInfo.WarehouseCode != "") oItems.WhsInfo.Add();

                    oItems.WhsInfo.WarehouseCode = "POLY34";
                    lRetCode = oItems.Update();
                    if (lRetCode != 0)
                    {
                        int lErrCode;
                        string sErrMsg;
                        oCompany.GetLastError(out lErrCode, out sErrMsg);
                        throw new Exception(sErrMsg);
                    }

                }
            }
            


        }

        private static void VerifySapInventoryQuantity(SAPbobsCOM.Company oCompany, string _WarehouseCode, string _BatchNumber, decimal _Quantity, string _ItemCode, String _Shipment, String _Usuario)
        {
            //Verifica existencias. 

            SAPbobsCOM.Recordset _orsVerificaExistencias = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
            _orsVerificaExistencias.DoQuery(String.Format("select I.ItemCode,BatchNum,Quantity,WhsCode,i.InDate,p.LstEvlPric,p.U_CodigoToriflex from OIBT AS I inner join OITM as P on I.ItemCode = P.ItemCode  inner join OITB as G on P.ItmsGrpCod = G.ItmsGrpCod  where  Quantity > 0 AND I.ItemCode = '{0}' AND I.BatchNum = '{1}' AND I.WhsCode = '{2}'"
                                       , _ItemCode, _BatchNumber, _WarehouseCode));


            if (_orsVerificaExistencias.RecordCount == 0)
            {
                throw new Exception("dn=> No hay cantidad suficiente en el inventario de SAP. ShipmentId: " + _Shipment + " Item: " + _ItemCode + " Lote: " + _BatchNumber + " Bodega: " + _WarehouseCode + " Requerido: " + _Quantity.ToString() + " Saldo: 0");
            }
            else
            {
                while (!_orsVerificaExistencias.EoF)
                {
                    Decimal _ExistenciaArt = Decimal.Parse(((Double)_orsVerificaExistencias.Fields.Item("Quantity").Value).ToString());
                    if ((Double.Parse(_Quantity.ToString()) - Double.Parse(_ExistenciaArt.ToString())) < 0.1
                            && (Double.Parse(_Quantity.ToString()) - Double.Parse(_ExistenciaArt.ToString())) > 0)
                    {
                        Double Ajuste = Double.Parse(_Quantity.ToString()) - Double.Parse(_ExistenciaArt.ToString());
                        var _return_value = new SqlParameter("@return_value", SqlDbType.Int);
                        _return_value.Direction = ParameterDirection.Output;
                        flexmanDBEntities _Db_Flex = new flexmanDBEntities();



                        var _Resultado = _Db_Flex.Database.SqlQuery<String>("EXEC	@return_value = [dbo].[SP_GenerarAjustePTPorDespacho] @ItemCode = '" + _ItemCode + "', @UserName = '" + _Usuario + "' , @Qty = " + Ajuste.ToString() + ", @SkidId =  " + _BatchNumber, _return_value).Single();
                        if (_Resultado.ToString().Contains("ERROR"))
                        { throw new Exception(_Resultado.ToString()); }
                        else
                        {
                            DbSapTableAdapters.oInvetoryTableAdapter oInventory = new oInvetoryTableAdapter();
                            oInventory.Procesa_PT_oInvetoryGenEntry(_Resultado.ToString());
                            _ExistenciaArt = Decimal.Parse(((Double)_orsVerificaExistencias.Fields.Item("Quantity").Value).ToString());
                        }


                    }

                    if (_ExistenciaArt < Convert.ToDecimal(_Quantity))
                    {
                        throw new Exception("dn=> No hay cantidad suficiente en el inventario de SAP. ShipmentId: " + _Shipment + " Item: " + _ItemCode + " Lote: " + _BatchNumber + " Bodega: " + _WarehouseCode + " Requerido: " + _Quantity.ToString() + " Saldo: " + _ExistenciaArt.ToString());
                    }
                    _orsVerificaExistencias.MoveNext();
                }
            }
        }

        private object Obtener_GTIN(string _ItemCode, string _OrdenNumberString)
        {
            String _Retorna = "na";


            Int32 _OrderNumber = Int32.Parse(_OrdenNumberString);
            var _Lista = (from l in _Db_Tori.vw_OrderDetail_WallmartData where l.OrderNumber == _OrderNumber && l.ItemCode == _ItemCode select l).ToList();
            if (_Lista.Any())
            {
                if (_Lista[0].GTIN != "")
                {
                    _Retorna = _Lista[0].GTIN;
                }
                else
                {
                    if (_Lista[0].UbicacionIso != "")
                    {
                        throw new Exception("El codigo de Producto de Walmart no esta Mapeado, contacte al vendedor, no se podra despachar hasta qu esto esto solventado ==> " + _Lista[0].ItemCode);
                    }
                }
            }


            return _Retorna;
        }
    }
}

