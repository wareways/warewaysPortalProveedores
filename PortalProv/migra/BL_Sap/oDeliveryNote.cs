using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using Sap_Service;
using System.Data;

/// <summary>
/// Summary description for PPrensa_OrderArtes
/// </summary>
/// 

namespace DbSapTableAdapters
{
    


    public partial class oDeliveryNoteTableAdapter
    {
        TORIEntities1 _Db_Tori = new TORIEntities1();

        [DataObjectMethod
        (DataObjectMethodType.Insert, true)]
        public void Insert_oDeliveryNote_Shipping(int ShipNumber, string Usuario)
        {
            // Obtener TipoProcesofactura 
            var _TipoProcesofactura = _Db_Tori.Database.SqlQuery<String>(string.Format("select isnull(InvoiceInstructions,'Entrega') FROM tblShippingRequest sr, tblShippingReqDetail srd WHERE sr.ShipRequest = srd.ShipRequest AND srd.ShipNumber = {0} ",
                                  ShipNumber)).FirstOrDefault();

            if (_TipoProcesofactura == "Traslado")
            {
                oStockTransfer _StockTransfer = new oStockTransfer();
                _StockTransfer.Insert_StockTransfer_Shipping(ShipNumber, Usuario);
                return;
            }

            //----------------------------------------------------------------

            string _InternalPartNbr = "";
            double _Price = 0;
            double _Quantity = 0;
            double _QuantitySubTotal = 0;
            string _WarehouseCode = "";
            string _BatchNumber = "";
            decimal _Weight = 0;
            decimal _WeightTotal = 0;

            string _Comments = "Embarque No.: " + ShipNumber.ToString() + " Por: " + Usuario;
            string _ItemCode = "";
            //string _Account = "511010011100";
            string _Account = "";
            //string _UbicacinDefault = "11";
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
                _CompanyNumber = _VwTarimasRollTracking[0].Empresa ;
                
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

                SAPbobsCOM.Documents oDeliveryNote;
                //SAPbobsCOM.SerialNumbers oSerialNumbers;
                SAPbobsCOM.BatchNumbers oBatchNumbers;

                oDeliveryNote = (SAPbobsCOM.Documents)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oDeliveryNotes);

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
                        //_tblOCRD.Connection.ConnectionString = GT.System_GT.Get_DbTori(_CompanyNumber);

                        if (_tblOCRD.GetDataBy_CardCode(_CardCode).Rows.Count == 0)
                        {
                            DbCustServTableAdapters.tblCustomerTableAdapter _oCustomer = new DbCustServTableAdapters.tblCustomerTableAdapter();
                            _oCustomer.Connection.ConnectionString = GT.System_GT.Get_DbTori(_CompanyNumber);

                            _oCustomer.Update_SAP(Convert.ToInt32(_CardCode));

                            //throw new Exception("Cliente " + _VwTarimasRollTracking[i].CustomerNumber + " - " + _VwTarimasRollTracking[i].CustomerName + " no encontrado ");

                            //string ErrRef = "";
                            //string _Mensaje = "Cliente " + _VwTarimasRollTracking[i].CustomerNumber + " - " + _VwTarimasRollTracking[i].CustomerName + " no encontrado para el Embarque No. " + ShipNumber.ToString();

                            //GT.System_GT.Mandar_Correo(ref ErrRef, "vpolytec@polytec.com.gt, dportillo@polytec.com.gt, ehigueros@polytec.com.gt", "Cliente " + _VwTarimasRollTracking[i].CustomerNumber + " - " + _VwTarimasRollTracking[i].CustomerName + " no encontrado ", _Mensaje, "dtrejo@geoplast.com.gt, facturacion@polytec.com.gt", "1");

                            //if (ErrRef != "")
                            //{
                            //    GT.System_GT.f_error(new Exception(ErrRef), "oDeliveryNote.cs", "");
                            //}

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

                        oDeliveryNote.CardCode = _CardCode;
                        oDeliveryNote.DocDate = DateTime.Now.Date;
                        oDeliveryNote.TaxDate = DateTime.Now.Date;
                        oDeliveryNote.Comments = GT.System_GT.Left(_Comments + ' ' + _MarkInfo, 254);
                        oDeliveryNote.DocType = SAPbobsCOM.BoDocumentTypes.dDocument_Items;
                        oDeliveryNote.ManualNumber = "N";
                        oDeliveryNote.JournalMemo = GT.System_GT.Left(_Comments, 50);
                        oDeliveryNote.BPL_IDAssignedToInvoice = _Sucursal;
                        oDeliveryNote.DocCurrency = _DocCurrency;

                        //Vendedor
                        DbSapTableAdapters.OSLPTableAdapter _DbOSLP = new OSLPTableAdapter();
                        _DbOSLP.Connection.ConnectionString = GT.System_GT.Get_DbTori(_CompanyNumber);
                        DbSap.OSLPDataTable _tblOSLP = new DbSap.OSLPDataTable();

                        _tblOSLP = _DbOSLP.GetData_CardCode(_CardCode);

                        if (_tblOSLP.Rows.Count > 0)
                        {
                            oDeliveryNote.SalesPersonCode = _tblOSLP[0].SlpCode;
                        }
                        else
                        {
                            //oBP.SalesPersonCode = Convert.ToInt32(Insert_Vendedores(oCompanyNumber, _SalesRepName.Replace("Geo ", ""), _SalesAbr));
                            if (oDeliveryNote.SalesPersonCode <= 0)
                            {
                                //GT.System_GT.f_error(new Exception("Cliente no tiene vendedor" + _SalesRepName), "oBusinessPartners.cs", "");
                                throw new Exception("Cliente no tiene vendedor: " + _CardCode);
                            }
                        }

                        try { oDeliveryNote.NumAtCard = _VwTarimasRollTracking[i].CustPONbr; }
                        catch { }


                        oDeliveryNote.UserFields.Fields.Item("U_Embarque").Value = _VwTarimasRollTracking[i].ShipmentId.ToString();

                        try
                        {
                            oDeliveryNote.UserFields.Fields.Item("U_Comision").Value = _VwTarimasRollTracking[i].CommissionPerc.ToString();
                        }
                        catch { }

                        try
                        {
                            oDeliveryNote.UserFields.Fields.Item("U_Margen").Value = _VwTarimasRollTracking[i].MargenNeto.ToString();
                        }
                        catch { }

                        try
                        {
                            oDeliveryNote.UserFields.Fields.Item("U_FacNom").Value = _VwTarimasRollTracking[i].CustomerName.ToString();
                        }
                        catch { }

                        try
                        {
                            oDeliveryNote.UserFields.Fields.Item("U_FacNit").Value = _VwTarimasRollTracking[i].NIT.ToString();
                        }
                        catch { }

                        try
                        {
                            oDeliveryNote.UserFields.Fields.Item("U_OV").Value = _VwTarimasRollTracking[i].ShipOrderNumber.ToString();
                        }
                        catch { }

                        try
                        {
                            Int32 _p_OrderNumber = Int32.Parse( _VwTarimasRollTracking[i].ShipOrderNumber.ToString() );

                            var _Lista = (from l in _Db_Tori.vw_OrderDetail_WallmartData where l.OrderNumber == _p_OrderNumber select l).ToList();
                            if (_Lista.Any())
                            {
                                if (_Lista[0].UbicacionIso != "")
                                {
                                    oDeliveryNote.UserFields.Fields.Item("U_W_IdUbica").Value = _Lista[0].UbicacionIso;
                                    try
                                    {
                                        oDeliveryNote.UserFields.Fields.Item("U_W_OtrImp").Value = 0;
                                        oDeliveryNote.UserFields.Fields.Item("U_W_OtrPor").Value = 0;
                                    }
                                    catch { }

                                    try { oDeliveryNote.UserFields.Fields.Item("U_W_Albaran").Value = "0000"; } catch { }
                                    try { oDeliveryNote.UserFields.Fields.Item("U_W_IdProv").Value = _Lista[0].ProveedorIso; } catch { }
                                    try { oDeliveryNote.UserFields.Fields.Item("U_W_Transporte").Value = _Lista[0].TransporteIso; } catch { }                                
                                    try { oDeliveryNote.UserFields.Fields.Item("U_W_OC").Value = _Lista[0].OrdenCompra; } catch { }                                    
                                                                                                                                                                                  
                                } else
                                {
                                    oDeliveryNote.UserFields.Fields.Item("U_W_Albaran").Value = "Sin Datos";
                                }
                              
                            }

                        }
                        catch { }



                        try
                        {
                            oDeliveryNote.UserFields.Fields.Item("U_OC").Value = _VwTarimasRollTracking[i].CustPONbr.ToString();
                        }
                        catch { }

                        try
                        {
                            oDeliveryNote.UserFields.Fields.Item("U_PM").Value = _VwTarimasRollTracking[i].PackgingMethod.ToString();
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


                    } //if (i == 0)

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



                        //separar el iva. 
                        //try
                        //{
                        //    _TaxGroupId = _VwTarimasRollTracking[i].TaxGroupId;
                        //}
                        //catch {}

                        //if (_TaxGroupId != "EXPORT" && _TaxGroupId != "EXENTAS")
                        //{
                        //    _Price = _Price / 1.12; 
                        //}

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

                    if (_VwTarimasRollTracking[i].CustomerUOM == "Libra" || _VwTarimasRollTracking[i].CustomerUOM == "Kg" || _VwTarimasRollTracking[i].CustomerUOM == "Libra Espanola")
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
                            oDeliveryNote.Lines.Add();
                            iBatch = 0;
                            iLine += 1;
                        }

                        oDeliveryNote.Lines.SetCurrentLine(iLine);
                        //oDeliveryNote.Lines.BaseLine = iLine; 
                        //oDeliveryNote.Lines.AccountCode = _AccountCode;

                        oDeliveryNote.Lines.WarehouseCode = _WarehouseCode;
                        oDeliveryNote.Lines.ItemCode = _ItemCode;
                        oDeliveryNote.Lines.ItemDescription = _ItemDescription;
                        oDeliveryNote.Lines.PriceAfterVAT = _Price;
                        oDeliveryNote.Lines.AccountCode = _RevenuesAc;
                        oDeliveryNote.Lines.UserFields.Fields.Item("U_TipoA").Value = _U_TipoA2;

                        try
                        {
                            oDeliveryNote.Lines.UserFields.Fields.Item("U_W_GTIN").Value = Obtener_GTIN(_ItemCode, _VwTarimasRollTracking[i].ShipOrderNumber.ToString()); 
                        }
                        catch { }

                        var _ListaShipmentIvaExe = (from l in _Db_Tori.tblCustomerShipmentTax
                                                    where l.ShipmentNumber == ShipNumber && l.TipoFacturacion != ""
                                                    select l).ToList();
                        if (_ListaShipmentIvaExe.Count() == 1)
                        {
                            oDeliveryNote.Lines.TaxCode = _ListaShipmentIvaExe[0].TipoFacturacion;
                        }


                        
                        ////valida existencia. Antiguo
                        //DbSapTableAdapters.vw_OITWTableAdapter _OITW = new DbSapTableAdapters.vw_OITWTableAdapter();
                        //decimal xSaldo = _OITW.GetData_SaldoBy_ItemCode(_ItemCode, _BatchNumber, _WarehouseCode);

                        //DB_SAP_ProduccionEntities _DB_Sap = new DB_SAP_ProduccionEntities();
                        //var _ShipDetailIdString = _ShipDetailId.ToString();
                        //var _Busqueda = (from l in _DB_Sap.ODLNs.AsNoTracking() where l.U_IdMovDet == _ShipDetailIdString select l).ToList();
                        //if (ShipNumber == 147862) xSaldo = Convert.ToDecimal(_Quantity);
                        //if (_Busqueda.Count == 0)
                        //{
                        //    if (xSaldo < Convert.ToDecimal(_Quantity))
                        //    {
                        //        var _ShipmentStr = ShipNumber.ToString();
                        //        //var _ShipmentStr =  ObtenerShipmentdeTarima(_BatchNumber);
                        //        if (_ShipmentStr != "") _ShipmentStr = " Shipment: " + _ShipmentStr + " ";
                        //        throw new Exception("=o=> No hay cantidad suficiente en el inventario de SAP. " + _ShipmentStr + " Item: " + _ItemCode + " Lote: " + _BatchNumber + " Bodega: " + _WarehouseCode + " Requerido: " + _Quantity.ToString() + " Saldo: " + xSaldo.ToString());
                        //    }
                        //}
                        ////fin valida existencia. Antiguo



                        ////Proyectos
                        //DbSapTableAdapters.OPRJTableAdapter _OPRJ = new OPRJTableAdapter();
                        //_OPRJ.Connection.ConnectionString = GT.System_GT.Get_DbTori(_CompanyNumber);

                        //if (_OPRJ.GetDataBy_PrjCode(_OrderNumber.ToString()).Rows.Count == 0)
                        //{
                        //    DbSapTableAdapters.oProjectTableAdapter _oProject = new oProjectTableAdapter();

                        //    _oProject.Sync_oProject(_OrderNumber);
                        //}

                        //oDeliveryNote.Lines.ProjectCode = _OrderNumber.ToString();
                    }

                    _Quantity = Math.Round(_Quantity,3);
                    // Validar Existencia Nuevo Julio Herrera
                    VerifySapInventoryQuantity(oCompany, _WarehouseCode, _BatchNumber,  Convert.ToDecimal(_Quantity), _ItemCode, ShipNumber.ToString(), Usuario);

                    //===============================================
                    //'bacthes
                    oBatchNumbers = oDeliveryNote.Lines.BatchNumbers;
                    if (iBatch != 0)
                    {
                        oBatchNumbers.Add();
                    }
                    //oBatchNumbers.BaseLineNumber = oDeliveryNote.Lines.BaseLine;
                    oBatchNumbers.SetCurrentLine(iBatch);
                    oBatchNumbers.BatchNumber = _BatchNumber;
                    if (_VwTarimasRollTracking[i].CustomerUOM == "Millar" || _VwTarimasRollTracking[i].CustomerUOM == "MSM" || _VwTarimasRollTracking[i].CustomerUOM == "KImp")
                    {
                        oBatchNumbers.Quantity = Math.Round(_Quantity, 3);
                        _QuantitySubTotal += Math.Round(_Quantity, 3);
                    } else
                    {
                        oBatchNumbers.Quantity = Math.Round(_Quantity, 2);
                        _QuantitySubTotal += Math.Round(_Quantity, 2);
                    }
                        
                    oBatchNumbers.Location = _WarehouseCode;

                    oBatchNumbers.UserFields.Fields.Item("U_Peso").Value = _Weight.ToString();
                    oBatchNumbers.UserFields.Fields.Item("U_Pedido").Value = _OrderNumber.ToString();
                    

                    _NbrOfUnits = _VwTarimasRollTracking[i].NbrOfUnits;
                    _NbrOfUnitsTotal += _VwTarimasRollTracking[i].NbrOfUnits;


                    iBatch += 1;
                    // oBatchNumbers.Add();

                    try { _LastInternalPartNbr = GT.System_GT.Left(_VwTarimasRollTracking[i + 1].InternalPartNbr, 8); }
                    catch { _LastInternalPartNbr = ""; }

                    if (_LastInternalPartNbr != _ItemCode)
                    {
                        oDeliveryNote.Lines.Quantity = _QuantitySubTotal;
                        //oDeliveryNote.Lines.Add(); 
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
                        GT.System_GT.f_error(ex, "oDeliveryNote.cs", "");
                    }

                    // EOF
                    if (i == _VwTarimasRollTracking.Rows.Count - 1)
                    {

                        try
                        {
                            oDeliveryNote.UserFields.Fields.Item("U_TotalRollos").Value = _NbrOfUnitsTotal;
                        }
                        catch { }
                        try
                        {
                            oDeliveryNote.UserFields.Fields.Item("U_TotalPesoKG").Value = _WeightTotal.ToString();
                        }
                        catch { }


                        // Verificación de Transacción para Evitar Transacción Duplicada Julio Herrera
                        // ------------------------------------------------------------
                        oDeliveryNote.UserFields.Fields.Item("U_IdMovDet").Value = _ShipDetailId.ToString();
                        DB_SAP_ProduccionEntities _DB_Sap = new DB_SAP_ProduccionEntities();
                        var _ShipDetailIdString = _ShipDetailId.ToString();
                        var _Busqueda = (from l in _DB_Sap.ODLNs.AsNoTracking() where l.U_IdMovDet == _ShipDetailIdString select l).ToList();


                        if (_Busqueda.Count == 0 && _ShipDetailId != 210950)
                        {
                            lRetCode = oDeliveryNote.Add();
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
                            //var _MesajeCorreo = "Documento Recuperado Entregas No. " + _Busqueda[0].DocNum.ToString();
                            //GT.System_GT.Mandar_CorreoPorAlertas(ref _ref, 10, _MesajeCorreo,_MesajeCorreo , "Baja");
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
                        DbSapTableAdapters.vw_OITLTableAdapter _Db_vw_OITL = new vw_OITLTableAdapter();
                        DbSap.vw_OITLDataTable _tbl_vw_OITL = new DbSap.vw_OITLDataTable();

                        _tbl_vw_OITL = _Db_vw_OITL.GetDataBy_DocEntry(_CompanyNumber, 15, Convert.ToInt32(InvCodeStr));

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
                            GT.System_GT.f_error(new Exception("no se encontraron datos en Sap. ODeliveryNote_01, Codigo = "+ InvCodeStr), "vw_OITL", "");
                        }


                    }
                } //for (int i = 0; i <= _VwTarimasRollTracking.Rows.Count - 1; i++)
            }
            else
            {
                throw new Exception("Compañía no conectada");
            }
        }


        [DataObjectMethod
        (DataObjectMethodType.Insert, true)]
        public void Insert_oDeliveryNote_FromTransfer(int ShipNumber, string Usuario)
        {
            // Obtener TipoProcesofactura 
            var _TipoProcesofactura = _Db_Tori.Database.SqlQuery<String>(string.Format("select isnull(InvoiceInstructions,'Entrega') FROM tblShippingRequest sr, tblShippingReqDetail srd WHERE sr.ShipRequest = srd.ShipRequest AND srd.ShipNumber = {0} ",
                                  ShipNumber)).FirstOrDefault();

            if (_TipoProcesofactura != "Traslado")
            {
                throw new Exception("No se pueden Generar Entrega porque no provino del Translado");
            }

            //----------------------------------------------------------------

            string _InternalPartNbr = "";
            double _Price = 0;
            double _Quantity = 0;
            double _QuantitySubTotal = 0;
            string _WarehouseTrasladoCode = "";
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

            string _return = "";
            SAPbobsCOM.Company oCompany;
            oCompany = GlobalSAP.GetCompany(_CompanyNumber);
            _Sucursal = GlobalSAP.GetSucursal(_CompanyNumber);
            _WarehouseTrasladoCode = GlobalSAP.GetWarehouseCodeEnvio(_CompanyNumber);
            // _Account = GlobalSAP.GetAccountPT(_CompanyNumber);

            if (oCompany.Connected)
            {

                SAPbobsCOM.Documents oDeliveryNote;
                //SAPbobsCOM.SerialNumbers oSerialNumbers;
                SAPbobsCOM.BatchNumbers oBatchNumbers;

                oDeliveryNote = (SAPbobsCOM.Documents)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oDeliveryNotes);

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

                        _tblOWHS = _dbOHS.GetDataBy_WhsCode(_WarehouseTrasladoCode);

                        if (_tblOWHS.Rows.Count > 0)
                        {
                            _WhsName = _tblOWHS[0].WhsName;
                            _RevenuesAc = _tblOWHS[0].RevenuesAc;
                        }

                        oDeliveryNote.CardCode = _CardCode;
                        oDeliveryNote.DocDate = DateTime.Now.Date;
                        oDeliveryNote.TaxDate = DateTime.Now.Date;
                        oDeliveryNote.Comments = GT.System_GT.Left(_Comments + ' ' + _MarkInfo, 254);
                        oDeliveryNote.DocType = SAPbobsCOM.BoDocumentTypes.dDocument_Items;
                        oDeliveryNote.ManualNumber = "N";
                        oDeliveryNote.JournalMemo = GT.System_GT.Left(_Comments, 50);
                        oDeliveryNote.BPL_IDAssignedToInvoice = _Sucursal;
                        oDeliveryNote.DocCurrency = _DocCurrency;

                        //Vendedor
                        DbSapTableAdapters.OSLPTableAdapter _DbOSLP = new OSLPTableAdapter();
                        _DbOSLP.Connection.ConnectionString = GT.System_GT.Get_DbTori(_CompanyNumber);
                        DbSap.OSLPDataTable _tblOSLP = new DbSap.OSLPDataTable();

                        _tblOSLP = _DbOSLP.GetData_CardCode(_CardCode);

                        if (_tblOSLP.Rows.Count > 0)
                        {
                            oDeliveryNote.SalesPersonCode = _tblOSLP[0].SlpCode;
                        }
                        else
                        {                            
                            if (oDeliveryNote.SalesPersonCode <= 0)
                            {                                
                                throw new Exception("Cliente no tiene vendedor: " + _CardCode);
                            }
                        }

                        try { oDeliveryNote.NumAtCard = _VwTarimasRollTracking[i].CustPONbr; }
                        catch { }

                        oDeliveryNote.UserFields.Fields.Item("U_Embarque").Value = _VwTarimasRollTracking[i].ShipmentId.ToString();

                        try
                        {
                            oDeliveryNote.UserFields.Fields.Item("U_Comision").Value = _VwTarimasRollTracking[i].CommissionPerc.ToString();
                        }
                        catch { }

                        try
                        {
                            oDeliveryNote.UserFields.Fields.Item("U_Margen").Value = _VwTarimasRollTracking[i].MargenNeto.ToString();
                        }
                        catch { }

                        try
                        {
                            oDeliveryNote.UserFields.Fields.Item("U_FacNom").Value = _VwTarimasRollTracking[i].CustomerName.ToString();
                        }
                        catch { }

                        try
                        {
                            oDeliveryNote.UserFields.Fields.Item("U_FacNit").Value = _VwTarimasRollTracking[i].NIT.ToString();
                        }
                        catch { }

                        try
                        {
                            oDeliveryNote.UserFields.Fields.Item("U_OV").Value = _VwTarimasRollTracking[i].ShipOrderNumber.ToString();
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
                                    oDeliveryNote.UserFields.Fields.Item("U_W_IdUbica").Value = _Lista[0].UbicacionIso;
                                    try
                                    {
                                        oDeliveryNote.UserFields.Fields.Item("U_W_OtrImp").Value = 0;
                                        oDeliveryNote.UserFields.Fields.Item("U_W_OtrPor").Value = 0;
                                    }
                                    catch { }

                                    try { oDeliveryNote.UserFields.Fields.Item("U_W_Albaran").Value = "0000"; } catch { }
                                    try { oDeliveryNote.UserFields.Fields.Item("U_W_IdProv").Value = _Lista[0].ProveedorIso; } catch { }
                                    try { oDeliveryNote.UserFields.Fields.Item("U_W_Transporte").Value = _Lista[0].TransporteIso; } catch { }
                                    try { oDeliveryNote.UserFields.Fields.Item("U_W_OC").Value = _Lista[0].OrdenCompra; } catch { }

                                }
                                else
                                {
                                    oDeliveryNote.UserFields.Fields.Item("U_W_Albaran").Value = "Sin Datos";
                                }

                            }

                        }
                        catch { }



                        try
                        {
                            oDeliveryNote.UserFields.Fields.Item("U_OC").Value = _VwTarimasRollTracking[i].CustPONbr.ToString();
                        }
                        catch { }

                        try
                        {
                            oDeliveryNote.UserFields.Fields.Item("U_PM").Value = _VwTarimasRollTracking[i].PackgingMethod.ToString();
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


                    } //if (i == 0)

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
                    

                    //Lines 
                    if (_LastInternalPartNbr != _ItemCode)
                    {
                        if (i != 0) // Lines 
                        {
                            oDeliveryNote.Lines.Add();
                            iBatch = 0;
                            iLine += 1;
                        }

                        oDeliveryNote.Lines.SetCurrentLine(iLine);                      
                        oDeliveryNote.Lines.WarehouseCode = _WarehouseTrasladoCode;
                        oDeliveryNote.Lines.ItemCode = _ItemCode;
                        oDeliveryNote.Lines.ItemDescription = _ItemDescription;
                        oDeliveryNote.Lines.PriceAfterVAT = _Price;
                        oDeliveryNote.Lines.AccountCode = _RevenuesAc;
                        oDeliveryNote.Lines.UserFields.Fields.Item("U_TipoA").Value = _U_TipoA2;

                        try
                        {
                            oDeliveryNote.Lines.UserFields.Fields.Item("U_W_GTIN").Value = Obtener_GTIN(_ItemCode, _VwTarimasRollTracking[i].ShipOrderNumber.ToString());
                        }
                        catch { }

                        var _ListaShipmentIvaExe = (from l in _Db_Tori.tblCustomerShipmentTax
                                                    where l.ShipmentNumber == ShipNumber && l.TipoFacturacion != ""
                                                    select l).ToList();
                        if (_ListaShipmentIvaExe.Count() == 1)
                        {
                            oDeliveryNote.Lines.TaxCode = _ListaShipmentIvaExe[0].TipoFacturacion;
                        }
;
                    }

                    _Quantity = Math.Round(_Quantity, 3);
                    // Validar Existencia Nuevo Julio Herrera
                    VerifySapInventoryQuantity(oCompany, _WarehouseTrasladoCode, _BatchNumber, Convert.ToDecimal(_Quantity), _ItemCode, ShipNumber.ToString(), Usuario);

                    //===============================================
                    //'bacthes
                    oBatchNumbers = oDeliveryNote.Lines.BatchNumbers;
                    if (iBatch != 0)
                    {
                        oBatchNumbers.Add();
                    }
                    //oBatchNumbers.BaseLineNumber = oDeliveryNote.Lines.BaseLine;
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

                    oBatchNumbers.Location = _WarehouseTrasladoCode;
                    oBatchNumbers.UserFields.Fields.Item("U_Peso").Value = _Weight.ToString();
                    oBatchNumbers.UserFields.Fields.Item("U_Pedido").Value = _OrderNumber.ToString();

                    _NbrOfUnits = _VwTarimasRollTracking[i].NbrOfUnits;
                    _NbrOfUnitsTotal += _VwTarimasRollTracking[i].NbrOfUnits;

                    iBatch += 1;
                   
                    try { _LastInternalPartNbr = GT.System_GT.Left(_VwTarimasRollTracking[i + 1].InternalPartNbr, 8); }
                    catch { _LastInternalPartNbr = ""; }

                    if (_LastInternalPartNbr != _ItemCode)
                    {
                        oDeliveryNote.Lines.Quantity = _QuantitySubTotal;
                       
                        _QuantitySubTotal = 0;
                    }

                    try { _LastInternalPartNbr = GT.System_GT.Left(_VwTarimasRollTracking[i].InternalPartNbr, 8); }
                    catch { _LastInternalPartNbr = ""; }

                    _Quantity = Math.Round(_Quantity, 3);

                    // kardex detail 
                    try
                    {
                        // No se Toma porque se inserto anteriormente
                        //DB_INVTableAdapters.Tbl_INV_Mov_DetailTableAdapter _DbInv_Detail = new DB_INVTableAdapters.Tbl_INV_Mov_DetailTableAdapter();
                        //_MovimientoDetailId = _DbInv_Detail.Insert_Detail(new Guid(_MovimientoId), _WarehouseTrasladoCode, _OrderNumber, _SkidId, _ItemCode, _InternalPartNbr, Convert.ToDecimal(_Quantity), _CustomerUOM, _NbrOfUnits, _Weight, Convert.ToDecimal(_Price), Convert.ToDecimal(_Price * _Quantity), 0, _ItemDescription, _ItmsGrpCod, _ItmsGrpNam, _BatchNumber, _WarehouseTrasladoCode, _WhsName, _Comments, _DocCurrency, Convert.ToDecimal(_Price), Convert.ToDecimal(_Price * _Quantity), _AccountCode, 0, _MachineId, ShipNumber, _ShipDetailId, idAlmacen, iLine);
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
                            oDeliveryNote.UserFields.Fields.Item("U_TotalRollos").Value = _NbrOfUnitsTotal;
                        }
                        catch { }
                        try
                        {
                            oDeliveryNote.UserFields.Fields.Item("U_TotalPesoKG").Value = _WeightTotal.ToString();
                        }
                        catch { }


                        // Verificación de Transacción para Evitar Transacción Duplicada Julio Herrera
                        // ------------------------------------------------------------
                        oDeliveryNote.UserFields.Fields.Item("U_IdMovDet").Value = _ShipDetailId.ToString();
                        DB_SAP_ProduccionEntities _DB_Sap = new DB_SAP_ProduccionEntities();
                        var _ShipDetailIdString = _ShipDetailId.ToString();
                        var _Busqueda = (from l in _DB_Sap.ODLNs.AsNoTracking() where l.U_IdMovDet == _ShipDetailIdString select l).ToList();


                        if (_Busqueda.Count == 0 && _ShipDetailId != 210950)
                        {
                            lRetCode = oDeliveryNote.Add();                            
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
                            //var _MesajeCorreo = "Documento Recuperado Entregas No. " + _Busqueda[0].DocNum.ToString();
                            //GT.System_GT.Mandar_CorreoPorAlertas(ref _ref, 10, _MesajeCorreo,_MesajeCorreo , "Baja");
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
                        DbSapTableAdapters.vw_OITLTableAdapter _Db_vw_OITL = new vw_OITLTableAdapter();
                        DbSap.vw_OITLDataTable _tbl_vw_OITL = new DbSap.vw_OITLDataTable();

                        _tbl_vw_OITL = _Db_vw_OITL.GetDataBy_DocEntry(_CompanyNumber, 15, Convert.ToInt32(InvCodeStr));

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

                            _dbinv.Update_Detail_Delivery(Convert.ToInt32(InvCodeStr), _WarehouseTrasladoCode, _WhsName, _Comments, _DocCurrency, _AccountCode, _DocName, _DocType, _DocNum.ToString(), _DocDate, new Guid(_MovimientoId));

                        }
                        else
                        {
                            GT.System_GT.f_error(new Exception("no se encontraron datos en Sap. ODeliveryNote_01, Codigo = " + InvCodeStr), "vw_OITL", "");
                        }


                    }
                } //for (int i = 0; i <= _VwTarimasRollTracking.Rows.Count - 1; i++)
            }
            else
            {
                throw new Exception("Compañía no conectada");
            }
        }


        private object Obtener_GTIN(string _ItemCode, string _OrdenNumberString)
        {
            String _Retorna = "na";
            

            Int32 _OrderNumber = Int32.Parse(_OrdenNumberString);
            var _Lista = (from l in _Db_Tori.vw_OrderDetail_WallmartData where l.OrderNumber == _OrderNumber && l.ItemCode == _ItemCode select l).ToList();
            if ( _Lista.Any())
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

        private static void VerifySapInventoryQuantity(SAPbobsCOM.Company oCompany, string _WarehouseCode, string _BatchNumber, decimal _Quantity, string _ItemCode, String _Shipment, String  _Usuario)
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
                        
                        

                        var _Resultado = _Db_Flex.Database.SqlQuery<String>("EXEC	@return_value = [dbo].[SP_GenerarAjustePTPorDespacho] @ItemCode = '" + _ItemCode+ "', @UserName = '" + _Usuario + "' , @Qty = " + Ajuste.ToString() + ", @SkidId =  " + _BatchNumber, _return_value).Single();
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


        [DataObjectMethod
        (DataObjectMethodType.Insert, true)]
        public string Insert_oDeliveryNote(int _Company, string _InternalPartNbr, double _Price, double _Quantity, string _WarehouseCode, string _BatchNumber, decimal _Weight, string _Comments, string _ItemCode, string _Account, int _OrderNumber, string _CardCode)
        {

            string _return = "";
            int _Sucursal = 0;
            SAPbobsCOM.Company oCompany;
            oCompany = GlobalSAP.GetCompany(_Company);

            _Sucursal = GlobalSAP.GetSucursal(_Company);

            try
            {
                SAPbobsCOM.Documents oDeliveryNote;
                SAPbobsCOM.BatchNumbers oBatchNumbers;

                oDeliveryNote = (SAPbobsCOM.Documents)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oDeliveryNotes);

                int lRetCode;
                string InvCodeStr = "";
                string _AccountCode = "";
                string _ItemDescription = "";

                if (oCompany.Connected)
                {
                    //----InternalPart
                    DbSapTableAdapters.OITMTableAdapter _DbOITM = new OITMTableAdapter();
                    DbSap.OITMDataTable _OITM = new DbSap.OITMDataTable();

                    if (_InternalPartNbr != "")
                    {
                        _OITM = _DbOITM.GetDataBy_U_CodigoToriflex(_InternalPartNbr);
                    }
                    else
                    {
                        _OITM = _DbOITM.GetDataBy_ItemCode(_ItemCode);
                    }

                    if (_OITM.Rows.Count > 0)
                    {
                        _ItemCode = _OITM[0].ItemCode;
                        _ItemDescription = _OITM[0].ItemName;
                    }
                    else
                    {
                        DbPPrensaTableAdapters.v_ProdDefinitionsTableAdapter _dbProdDefinitions = new DbPPrensaTableAdapters.v_ProdDefinitionsTableAdapter();
                        _dbProdDefinitions.Update_SAP(_InternalPartNbr);

                        _ItemCode = GT.System_GT.Left(_InternalPartNbr, 8);
                        _ItemDescription = _InternalPartNbr;
                    }

                    //-- Account
                    DbSapTableAdapters.OACTTableAdapter _DBOACT = new OACTTableAdapter();
                    DbSap.OACTDataTable _OACT = new DbSap.OACTDataTable();

                    _OACT = _DBOACT.GetDataBy_FormatCode(_Account);
                    if (_OACT.Rows.Count > 0)
                    {
                        _AccountCode = _OACT[0].AcctCode;
                    }

                    oDeliveryNote.CardCode = _CardCode;
                    oDeliveryNote.DocDate = DateTime.Now.Date;
                    oDeliveryNote.TaxDate = DateTime.Now.Date;
                    oDeliveryNote.Comments = _Comments;
                    oDeliveryNote.DocType = SAPbobsCOM.BoDocumentTypes.dDocument_Items;
                    oDeliveryNote.ManualNumber = "N";
                    oDeliveryNote.JournalMemo = GT.System_GT.Left(_Comments, 50);
                    oDeliveryNote.BPL_IDAssignedToInvoice = _Sucursal;


                    //oDeliveryNote.Lines.BaseEntry = Convert.ToInt32(InvCodeStr);
                    oDeliveryNote.Lines.ItemCode = _ItemCode;
                    oDeliveryNote.Lines.AccountCode = _AccountCode;
                    oDeliveryNote.Lines.ItemDescription = _ItemDescription;
                    oDeliveryNote.Lines.WarehouseCode = _WarehouseCode;
                    oDeliveryNote.Lines.Price = _Price;
                    oDeliveryNote.Lines.Quantity = _Quantity;


                    //===============================================
                    //'bacthes
                    oBatchNumbers = oDeliveryNote.Lines.BatchNumbers;
                    oBatchNumbers.SetCurrentLine(0);
                    oBatchNumbers.BaseLineNumber = 0;
                    oBatchNumbers.BatchNumber = _BatchNumber;
                    oBatchNumbers.Quantity = _Quantity;
                    oBatchNumbers.Location = _WarehouseCode;

                    oBatchNumbers.UserFields.Fields.Item("U_Peso").Value = _Weight.ToString();
                    oBatchNumbers.UserFields.Fields.Item("U_Pedido").Value = _OrderNumber.ToString();

                    lRetCode = oDeliveryNote.Add();

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
                else
                {
                    throw new Exception("Compania no conectada");
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            return _return;
        }

        [DataObjectMethod
        (DataObjectMethodType.Insert, true)]
        public string Update_oDeliveryNote_Envio(int _AbsEntry, string _NoEnvio)
        {
            int _Company = 1;
            string _return = "";
            int _Sucursal = 0;
            SAPbobsCOM.Company oCompany;
            oCompany = GlobalSAP.GetCompany(_Company);

            _Sucursal = GlobalSAP.GetSucursal(_Company);

            try
            {
                SAPbobsCOM.Documents oDeliveryNote;
                SAPbobsCOM.BatchNumbers oBatchNumbers;

                oDeliveryNote = (SAPbobsCOM.Documents)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oDeliveryNotes);

                int lRetCode;
                string InvCodeStr = "";
                string _AccountCode = "";
                string _ItemDescription = "";
                string _Comments = "Actualiza Entrega No. Envío " + _NoEnvio;

                if (oCompany.Connected)
                {

                    oDeliveryNote.GetByKey(_AbsEntry);

                    oDeliveryNote.UserFields.Fields.Item("U_NoEnvio").Value = _NoEnvio;

                    lRetCode = oDeliveryNote.Update();

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
                else
                {
                    throw new Exception("Compania no conectada");
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            return _return;
        }


        public String ObtenerShipmentdeTarima(String _Tarima)
        {
            String _Retorna = "";
            try
            {
                Int32 _SkidId = Int32.Parse(_Tarima);
                TORIEntities1 _DB_Tori = new TORIEntities1();
                _Retorna = (from l in _DB_Tori.tblRollTracking where l.SkidId == _SkidId select l).First().ShipmentId.ToString();
            }
            catch { }

            return _Retorna;
        }

    }

}