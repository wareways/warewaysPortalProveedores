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

    public partial class oInvetoryTableAdapter
    {

        [DataObjectMethodAttribute
        (DataObjectMethodType.Select, false)]
        public string GetDataBy_OrderNumber(int OrderNumber)
        {
            //DbSAPTableAdapters.vw_OrderTableAdapter _db = new vw_OrderTableAdapter();

            //try
            //{
            //    return _db.GetDataBy_OrderNumber(OrderNumber);
            //}
            //catch (Exception ex)
            //{

            //    throw ex;
            //}
            return "";
        }


        //Entradas PRODUCTO TERMINADO Por Tarima **
        [DataObjectMethodAttribute
        (DataObjectMethodType.Insert, true)]
        public string Procesa_PT_oInvetoryGenEntry( String _MovimientoId)
        {         

            DB_INVTableAdapters.Tbl_INV_Mov_HeaderTableAdapter _Db_Inv_Header = new DB_INVTableAdapters.Tbl_INV_Mov_HeaderTableAdapter();
            DB_INV.vw_INV_Mov_HeaderDataTable _Tbl_Inv_Header = new DB_INV.vw_INV_Mov_HeaderDataTable();

            _Tbl_Inv_Header = _Db_Inv_Header.GetDataBy_IdMovimiento(new Guid(_MovimientoId));

            DB_INVTableAdapters.Tbl_INV_Mov_DetailTableAdapter _db_Inv_Detail = new DB_INVTableAdapters.Tbl_INV_Mov_DetailTableAdapter();
            DB_INV.vw_INV_Mov_DetailDataTable _Tbl_Inv_Detail = new DB_INV.vw_INV_Mov_DetailDataTable();
            _Tbl_Inv_Detail = _db_Inv_Detail.GetDataBy_MovimientoId(new Guid(_MovimientoId));

            String _MovimientoDetailId = _Tbl_Inv_Detail[0].MovimientoDetailId.ToString();
            Int32 _Company = _Tbl_Inv_Header[0].IdEmpresa;
            String _InternalPartNbr = _Tbl_Inv_Detail[0].InternalPartNbr;
            Double _Price = Double.Parse( _Tbl_Inv_Detail[0].Price.ToString());
            Double _Quantity = Double.Parse(_Tbl_Inv_Detail[0].Qty.ToString());
            String _WarehouseCode = GlobalSAP.GetWarehouseCode(_Company);
            String _BatchNumber = _Tbl_Inv_Detail[0].SkidId.ToString();
            Decimal _Weight = _Tbl_Inv_Detail[0].Weight;
            String _Comments = _Tbl_Inv_Detail[0].Comments;
            Int32 _OrderNumber = _Tbl_Inv_Detail[0].OrderNumber;
            String _Account = GlobalSAP.GetAccount(_Company);

            return Insert_oInvetoryGenEntry(_Company, _InternalPartNbr, _Price, _Quantity, _WarehouseCode, _BatchNumber, _Weight,
                                _Comments, _InternalPartNbr, _Account, _OrderNumber, new Guid(_MovimientoDetailId));
        }


        //Entradas PRODUCTO TERMINADO Por Tarima **
        [DataObjectMethodAttribute
        (DataObjectMethodType.Insert, true)]
        public string Insert_oInvetoryGenEntry(int _CompanyNumber, string _InternalPartNbr, double _Price, double _Quantity, string _WarehouseCode, string _BatchNumber, decimal _Weight, string _Comments, string _ItemCode, string _Account, int _OrderNumber, Guid Original_MovimientoDetailId)
        {

            string _return = "";
            int _Sucursal = 0;

            SAPbobsCOM.Company oCompany;
            oCompany = GlobalSAP.GetCompany(_CompanyNumber);
            _Sucursal = GlobalSAP.GetSucursal(_CompanyNumber);

            try
            {
                SAPbobsCOM.Documents oInventory;
                //SAPbobsCOM.SerialNumbers oSerialNumbers;
                SAPbobsCOM.BatchNumbers oBatchNumbers;

                oInventory = (SAPbobsCOM.Documents)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oInventoryGenEntry);


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
                string _DocName = "";
                string _WhsName = "";
                decimal _SapUnitPrice = 0;
                decimal _SapPrice = 0;

                if (oCompany.Connected)
                {
                    //----InternalPart
                    DbSapTableAdapters.OITMTableAdapter _DbOITM = new OITMTableAdapter();
                    _DbOITM.Connection.ConnectionString = GT.System_GT.Get_DbTori(_CompanyNumber);
                    DbSap.OITMDataTable _OITM = new DbSap.OITMDataTable();


                    if (_InternalPartNbr != "")
                    {
                        _OITM = _DbOITM.GetDataBy_ItemCode(_InternalPartNbr.Substring(0, 8));

                        if (_OITM.Rows.Count == 0)
                        {
                            DbPPrensaTableAdapters.v_ProdDefinitionsTableAdapter _dbProdDefinitions = new DbPPrensaTableAdapters.v_ProdDefinitionsTableAdapter();
                            _dbProdDefinitions.Update_SAP(_InternalPartNbr);

                            //ACTUALIZA item code si no es muestra. 
                            if (!_InternalPartNbr.Substring(0, 8).Contains("M"))
                            {
                                _OITM = _DbOITM.GetDataBy_ItemCode(_InternalPartNbr.Substring(0, 8));
                            }
                            //throw new Exception("No existe el item code." + _InternalPartNbr.Substring(0, 8));

                            //string ErrRef = "";
                            //string _Mensaje = "No existe el item code." + _InternalPartNbr.Substring(0, 8) + " Internal Part Nbr:  " + _InternalPartNbr + " Para la tarima No." + _BatchNumber.ToString();

                            //GT.System_GT.Mandar_Correo(ref ErrRef, " juliodonis@polytec.com.gt", "No existe el item code." + _InternalPartNbr.Substring(0, 8), _Mensaje, "bodprodterm@polytec.com.gt", "1");

                            //if (ErrRef != "")
                            //{
                            //    GT.System_GT.f_error(new Exception(ErrRef), "oDeliveryNote.cs", "");
                            //}

                        }
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

                    //---- Warehouses 
                    //DbSapTableAdapters.OWHSTableAdapter _DbOWHS = new OWHSTableAdapter(); 
                    //DbSap.OWHSDataTable _OWHS = new DbSap.OWHSDataTable();

                    //_OWHS = _DbOWHS.GetDataBy_WhsCode(_WarehouseCode);
                    //if (_OWHS.Rows.Count > 0)
                    //{
                    //    _AccountCode = _OWHS[0].SaleCostAc; 
                    //}

                    //-- Account
                    DbSapTableAdapters.OACTTableAdapter _DBOACT = new OACTTableAdapter();
                    DbSap.OACTDataTable _OACT = new DbSap.OACTDataTable();

                    _OACT = _DBOACT.GetDataBy_FormatCode(_Account);
                    if (_OACT.Rows.Count > 0)
                    {
                        _AccountCode = _OACT[0].AcctCode;
                    }

                    oInventory.DocDate = DateTime.Now.Date;
                    oInventory.TaxDate = DateTime.Now.Date;
                    oInventory.Comments = _Comments;
                    oInventory.DocType = SAPbobsCOM.BoDocumentTypes.dDocument_Items;
                    oInventory.ManualNumber = "N";
                    oInventory.JournalMemo = GT.System_GT.Left(_Comments, 50);
                    oInventory.BPL_IDAssignedToInvoice = _Sucursal;


                    //oInventory.Lines.BaseEntry = Convert.ToInt32(InvCodeStr);
                    //oInventory.Lines.BaseLine = 

                    oInventory.Lines.ItemCode = _ItemCode;
                    oInventory.Lines.AccountCode = _AccountCode;
                    oInventory.Lines.ItemDescription = _ItemDescription;
                    oInventory.Lines.WarehouseCode = _WarehouseCode;
                    oInventory.Lines.Price = _Price;
                    oInventory.Lines.Quantity = Math.Round(_Quantity, 3);

                    ////Proyectos
                    //DbSapTableAdapters.OPRJTableAdapter _OPRJ = new OPRJTableAdapter();
                    //_OPRJ.Connection.ConnectionString = GT.System_GT.Get_DbTori(_CompanyNumber);

                    //if (_OPRJ.GetDataBy_PrjCode(_OrderNumber.ToString()).Rows.Count == 0)
                    //{
                    //    DbSapTableAdapters.oProjectTableAdapter _oProject = new oProjectTableAdapter();

                    //    _oProject.Sync_oProject(_OrderNumber);
                    //}

                    //oInventory.Lines.ProjectCode = _OrderNumber.ToString();


                    //===============================================
                    //'bacthes
                    oBatchNumbers = oInventory.Lines.BatchNumbers;
                    //oBatchNumbers.SetCurrentLine(1);
                    //oBatchNumbers.BaseLineNumber = 0;
                    oBatchNumbers.BatchNumber = _BatchNumber;
                    oBatchNumbers.Quantity = Math.Round(_Quantity, 3);
                    oBatchNumbers.Location = _WarehouseCode;

                    oBatchNumbers.UserFields.Fields.Item("U_Peso").Value = _Weight.ToString();
                    try
                    {
                        oBatchNumbers.UserFields.Fields.Item("U_CantIngreso").Value = Math.Round(_Quantity, 3).ToString();
                    }
                    catch { }
                    
                    oBatchNumbers.UserFields.Fields.Item("U_Pedido").Value = _OrderNumber.ToString();
                    oBatchNumbers.Add();



                    // Verificación de Transacción para Evitar Transacción Duplicada Julio Herrera
                    // ------------------------------------------------------------
                    oInventory.UserFields.Fields.Item("U_IdMovDet").Value = Original_MovimientoDetailId.ToString();
                    DbSapTableAdapters.OIGNTableAdapter _DB_OIGN_Check = new OIGNTableAdapter();
                    var _OIGN_Data = _DB_OIGN_Check.GetDataBy_IdMovDet(Original_MovimientoDetailId.ToString());

                    if (_OIGN_Data.Count() == 0)
                    {


                        lRetCode = oInventory.Add();

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
                        InvCodeStr = _OIGN_Data[0].DocEntry.ToString();
                    }

                    // Finaliza Modificaciones

                    DbSapTableAdapters.vw_OITLTableAdapter _Db_vw_OITL = new vw_OITLTableAdapter();
                    DbSap.vw_OITLDataTable _tbl_vw_OITL = new DbSap.vw_OITLDataTable();

                    _tbl_vw_OITL = _Db_vw_OITL.GetDataBy_DocEntry(_CompanyNumber, 59, Convert.ToInt32(InvCodeStr));

                    if (_tbl_vw_OITL.Rows.Count > 0)
                    {
                        _DocDate = _tbl_vw_OITL[0].DocDate;
                        _DocCurrency = _tbl_vw_OITL[0].Currency;
                        _DocType = _tbl_vw_OITL[0].DocType;
                        _DocName = _tbl_vw_OITL[0].Code;
                        _DocNum = _tbl_vw_OITL[0].DocNum;
                        _ItmsGrpCod = _tbl_vw_OITL[0].ItmsGrpCod;
                        _ItmsGrpNam = _tbl_vw_OITL[0].ItmsGrpNam;
                        _WhsName = _tbl_vw_OITL[0].WhsName;
                        _ItemCode = _tbl_vw_OITL[0].ItemCode;

                        _SapUnitPrice = _tbl_vw_OITL[0].Price;
                        _SapPrice = _tbl_vw_OITL[0].OpenValue;

                        DB_INVTableAdapters.Tbl_INV_Mov_DetailTableAdapter _dbinv = new DB_INVTableAdapters.Tbl_INV_Mov_DetailTableAdapter();

                        _dbinv.Update_Detail(Convert.ToInt32(InvCodeStr), _ItemDescription, _ItmsGrpCod, _ItmsGrpNam, _BatchNumber, _WarehouseCode, _WhsName, _Comments, _DocCurrency, _SapUnitPrice, _SapPrice, _Account, _DocName, _DocType, _DocNum.ToString(), _DocDate, _ItemCode, Original_MovimientoDetailId);

                    }
                    else
                    {
                       
                            GT.System_GT.f_error(new Exception("no se encontraron datos en sap. " + InvCodeStr), "vw_OITL51", "");
                       
                        
                        
                    }

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

        //Entradas a Producto Terminado Por Documento **
        [DataObjectMethodAttribute
        (DataObjectMethodType.Insert, true)]
        public string Insert_oInvetoryGenEntryDocumento(string _MovimientoId)
        {

            int _CompanyNumber = 0;
            int idTipodocumento = 0;
            int NumeroDocumento = 0;

            string _InternalPartNbr = "";
            double _Price = 0;
            double _Quantity = 0;
            string _WarehouseCode = "";
            string _BatchNumber = "";
            decimal _Weight = 0;
            string _Comments = "Saldos Al: " + DateTime.Now.ToString() + " Por: " + HttpContext.Current.Session["UserName"].ToString();

            string _ItemCode = "";
            string _Account = "";
            int _OrderNumber = 0;

            Guid _MovimientoDetailId;
            int idTipoMovimiento = 2; //--ingreso as bodega
            int idTipoDocumento = 8; //recepcion de producto terminado
            int idAlmacen = 5;
            int idEstado = 0;


            int CustomerNumber = 0;
            string CustomerName = "";
            string DocName = "";
            int DocType = 0;
            string DocNum = "";
            System.DateTime FechaDocumento;
            global::System.Nullable<global::System.DateTime> DocDate = null;

            string _CustomerUOM = "";
            int _NbrOfUnits = 0;
            string _DocCurrency = "";

            string _return = "";
            SAPbobsCOM.Company oCompany;

            DB_INVTableAdapters.vw_INV_Mov_DetailTableAdapter _DbVwTarimasRollTracking = new DB_INVTableAdapters.vw_INV_Mov_DetailTableAdapter();

            DB_INV.vw_INV_Mov_DetailDataTable _VwTarimasRollTracking = new DB_INV.vw_INV_Mov_DetailDataTable();

            _VwTarimasRollTracking = _DbVwTarimasRollTracking.GetDataBy_MovimientoId(new Guid(_MovimientoId));

            if (_VwTarimasRollTracking.Rows.Count > 0)
            {
                _CompanyNumber = _VwTarimasRollTracking[0].IdEmpresa;
                idTipodocumento = _VwTarimasRollTracking[0].idTipoDocumento;
                NumeroDocumento = _VwTarimasRollTracking[0].NumeroDocumento;
            }

            int _Sucursal = 0;

            oCompany = null;
            oCompany = GlobalSAP.GetCompany(_CompanyNumber);
            _Sucursal = GlobalSAP.GetSucursal(_CompanyNumber);
            _WarehouseCode = GlobalSAP.GetWarehouseCode(_CompanyNumber);

            _Account = GlobalSAP.GetAccount(_CompanyNumber);

            SAPbobsCOM.Documents oInventory;
            //SAPbobsCOM.SerialNumbers oSerialNumbers;
            SAPbobsCOM.BatchNumbers oBatchNumbers;

            oInventory = (SAPbobsCOM.Documents)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oInventoryGenEntry);



            for (int i = 0; i <= _VwTarimasRollTracking.Rows.Count - 1; i++)
            {

                _InternalPartNbr = _VwTarimasRollTracking[i].InternalPartNbr;

                //refresca costo

                _Quantity = 0;
                _BatchNumber = "";
                _Weight = 0;
                _Comments = "";
                _ItemCode = "";

                _OrderNumber = 0;

                CustomerNumber = 0;
                CustomerName = "";


                _InternalPartNbr = _VwTarimasRollTracking[i].InternalPartNbr;
                FechaDocumento = _VwTarimasRollTracking[i].FechaDocumento;

                try { _OrderNumber = _VwTarimasRollTracking[i].OrderNumber; }
                catch { }
                //refresca costo
                _Price = Convert.ToDouble(_VwTarimasRollTracking[i].Cost);

                // Detección de Costo Fuera de Rango
                Decimal _CostoMinimoRango = 8;  //Costo Minimo
                Decimal _CostoMaximoRango = 90;  // Costo Maximo

                TORIEntities1 _DB_Tori = new TORIEntities1();

                var _ItemCodeBuscar = _InternalPartNbr.Substring(0, 8);
                var _ListaMaquilaDeteccion = (from l in _DB_Tori.vw_OITM_CostosMaquilaPT where l.ItemCode == _ItemCodeBuscar select l).ToList();

                // Corección de Minimo debido a Codigos de Maquila
                if (_InternalPartNbr.StartsWith("0003-145") || _InternalPartNbr.StartsWith("0003-146") || _InternalPartNbr.StartsWith("0003-147") || _ListaMaquilaDeteccion.Count > 0)
                {
                    _CostoMinimoRango = 1;
                }

                // Agregar Exepcion Deteccion Regla Rango solo para esta Tarima


                var _CostoKilo = (_VwTarimasRollTracking[i].Cost * _VwTarimasRollTracking[i].Qty) / _VwTarimasRollTracking[i].Weight;
                if ( (_CostoKilo >= _CostoMinimoRango && _CostoKilo <= _CostoMaximoRango ) || (_ListaMaquilaDeteccion.Count > 0) || _VwTarimasRollTracking[i].SkidId == 1435018)
                {
                    
                }
                else
                {
                    String _Asunto = "No se Reproceso tarima No. " + _VwTarimasRollTracking[i].SkidId + " Producto: " + _VwTarimasRollTracking[i].InternalPartNbr;
                    String _Cuerpo = "No se Reproceso tarima No. " + _VwTarimasRollTracking[i].SkidId + " Producto: "+ _VwTarimasRollTracking[i].InternalPartNbr +  " debido a incerteza en el precio Cost x Kilo: " + _CostoKilo.ToString("##,###,###.00") + " , Costo Maximo: " + _CostoMaximoRango.ToString() + " Costo Minimo: "  + _CostoMinimoRango.ToString();

                    GT.System_GT.f_error(new Exception(_Cuerpo), "Tbl_Trn_Bod_Recepcion_det.cs", "");

                    string _ErrRef = "";
                    

                    GT.System_GT.Mandar_CorreoPorAlertas(ref _ErrRef, 17, _Asunto, _Cuerpo, "1");

                    if (_ErrRef != "")
                    {
                        GT.System_GT.f_error(new Exception(_ErrRef), "OInventory.cs", "");
                    }

                    throw new Exception( _Cuerpo);
                }

                // Fin de la Deteccion de Costo Fuera de Rango


                _Quantity = Math.Round(Convert.ToDouble(_VwTarimasRollTracking[i].Qty), 4);

                if (_VwTarimasRollTracking[i].BatchNumber != "")
                {
                    _BatchNumber = _VwTarimasRollTracking[i].BatchNumber;
                }
                else
                {
                    _BatchNumber = _VwTarimasRollTracking[i].SkidId.ToString();
                }

                _Weight = _VwTarimasRollTracking[i].Weight;
                _Comments = _VwTarimasRollTracking[i].Comments;
                _ItemCode = _VwTarimasRollTracking[i].ItemCode;

                _WarehouseCode = _VwTarimasRollTracking[i].WarehouseLocation;

                _CustomerUOM = _VwTarimasRollTracking[i].UOM;
                _NbrOfUnits = _VwTarimasRollTracking[i].Count_RollBoxNbr;
                _MovimientoDetailId = _VwTarimasRollTracking[i].MovimientoDetailId;

                //CustomerNumber = _VwTarimasRollTracking[i].CustomerNumber;
                //CustomerName = _VwTarimasRollTracking[i].CustomerName;



                try
                {


                    int lRetCode;
                    string InvCodeStr = "";
                    string _AccountCode = "";
                    string _ItemDescription = "";
                    string _ItmsGrpNam = "";
                    int _ItmsGrpCod = 0;
                    DateTime _DocDate;

                    int _DocType = 0;
                    int _DocNum = 0;
                    string _DocName = "";
                    string _WhsName = "";
                    decimal _SapUnitPrice = 0;
                    decimal _SapPrice = 0;

                    if (oCompany.Connected)
                    {
                        //----InternalPart
                        DbSapTableAdapters.OITMTableAdapter _DbOITM = new OITMTableAdapter();
                        _DbOITM.Connection.ConnectionString = GT.System_GT.Get_DbTori(_CompanyNumber);
                        DbSap.OITMDataTable _OITM = new DbSap.OITMDataTable();


                        //if (_InternalPartNbr != "")
                        //{
                        //    _OITM = _DbOITM.GetDataBy_ItemCode(_InternalPartNbr.Substring(0, 8));

                        //    if (_OITM.Rows.Count == 0)
                        //    {
                        //        //no actualiza a sap. 
                        //        //DbPPrensaTableAdapters.v_ProdDefinitionsTableAdapter _dbProdDefinitions = new DbPPrensaTableAdapters.v_ProdDefinitionsTableAdapter();
                        //        //_dbProdDefinitions.Update_SAP(_InternalPartNbr);
                        //        //_OITM = _DbOITM.GetDataBy_ItemCode(_InternalPartNbr.Substring(0, 8));

                        //        throw new Exception("No existe el item code." + _InternalPartNbr.Substring(0, 8));


                        //    }
                        //}

                        if (_ItemCode != "")
                        {
                            _OITM = _DbOITM.GetDataBy_ItemCode(_ItemCode);
                        }
                        else
                        {
                            _OITM = _DbOITM.GetDataBy_ItemCode(_InternalPartNbr.Substring(0, 8));
                        }

                        if (_OITM.Rows.Count > 0)
                        {
                            _ItemCode = _OITM[0].ItemCode;
                            _ItemDescription = _OITM[0].ItemName;
                            _ItmsGrpCod = _OITM[0].ItmsGrpCod;
                        }
                        // ITMS GROUP

                        DbSapTableAdapters.OITBTableAdapter _DbOITB = new OITBTableAdapter();
                        DbSap.OITBDataTable _OITB = new DbSap.OITBDataTable();
                        _OITB = _DbOITB.GetDataBy_ItmsGrpCod(Convert.ToInt16(_ItmsGrpCod));
                        if (_OITB.Rows.Count > 0)
                        {
                            _ItmsGrpNam = _OITB[0].ItmsGrpNam;
                        }

                        //---- Warehouses 
                        DbSapTableAdapters.OWHSTableAdapter _DbOWHS = new OWHSTableAdapter();
                        DbSap.OWHSDataTable _OWHS = new DbSap.OWHSDataTable();

                        _OWHS = _DbOWHS.GetDataBy_WhsCode(_WarehouseCode);
                        if (_OWHS.Rows.Count > 0)
                        {
                            _WhsName = _OWHS[0].WhsName;
                        }

                        //-- Account
                        DbSapTableAdapters.OACTTableAdapter _DBOACT = new OACTTableAdapter();
                        DbSap.OACTDataTable _OACT = new DbSap.OACTDataTable();

                        _OACT = _DBOACT.GetDataBy_FormatCode(_Account);
                        if (_OACT.Rows.Count > 0)
                        {
                            _AccountCode = _OACT[0].AcctCode;
                        }

                        //documento
                        if (i == 0)
                        {
                            oInventory.DocDate = DateTime.Now;  //FechaDocumento;
                            oInventory.TaxDate = DateTime.Now; //FechaDocumento;
                            oInventory.Comments = _Comments;
                            oInventory.DocType = SAPbobsCOM.BoDocumentTypes.dDocument_Items;
                            oInventory.ManualNumber = "N";
                            oInventory.JournalMemo = GT.System_GT.Left(_Comments, 50);
                            oInventory.BPL_IDAssignedToInvoice = _Sucursal;

                        }

                        //oInventory.Lines.BaseEntry = Convert.ToInt32(InvCodeStr);
                        //oInventory.Lines.BaseLine = 

                        if (i != 0)
                        {
                            oInventory.Lines.Add();
                        }

                        oInventory.Lines.ItemCode = _ItemCode;
                        oInventory.Lines.AccountCode = _AccountCode;
                        oInventory.Lines.ItemDescription = _ItemDescription;
                        oInventory.Lines.WarehouseCode = _WarehouseCode;
                        oInventory.Lines.Price = _Price;
                        oInventory.Lines.Quantity = _Quantity;

                        try
                        {
                            String _xref = "";
                            HttpContext httpContext = HttpContext.Current;
                            String _Mensaje = String.Format("Ejecucion Manual Recepcion del usuario {0} con la tarima {1} con costo {2} itemcode {3} ", httpContext.Session["UserName"].ToString(), _BatchNumber, _Price.ToString(), _ItemCode);
                            flexmanDBEntities _dbflex = new flexmanDBEntities();
                            var _transaccion = (from l in _dbflex.Tbl_INV_Mov_Header where l.MovimientoId == new Guid(_MovimientoId) select l).ToList();
                            if (_transaccion.Any()) { _transaccion[0].UserName = httpContext.Session["UserName"].ToString(); _dbflex.SaveChanges(); }
                            GT.System_GT.Mandar_CorreoPorAlertas(ref _xref, 15, "Recepción Manual sin Revision Costo", _Mensaje, "Alta");
                        }
                        catch { }
                        
                        //Proyectos
                        //DbSapTableAdapters.OPRJTableAdapter _OPRJ = new OPRJTableAdapter();
                        //_OPRJ.Connection.ConnectionString = GT.System_GT.Get_DbTori(_CompanyNumber);

                        //if (_OPRJ.GetDataBy_PrjCode(_OrderNumber.ToString()).Rows.Count == 0)
                        //{
                        //    DbSapTableAdapters.oProjectTableAdapter _oProject = new oProjectTableAdapter();

                        //    _oProject.Sync_oProject(_OrderNumber);
                        //}

                        //oInventory.Lines.ProjectCode = _OrderNumber.ToString();


                        //===============================================
                        //'bacthes

                        oBatchNumbers = oInventory.Lines.BatchNumbers;
                        if (i != 0)
                        {
                            oBatchNumbers.Add();
                        }

                        //oBatchNumbers.SetCurrentLine(1);
                        //oBatchNumbers.BaseLineNumber = 0;
                        oBatchNumbers.BatchNumber = _BatchNumber;
                        oBatchNumbers.Quantity = _Quantity;
                        oBatchNumbers.Location = _WarehouseCode;

                        oBatchNumbers.UserFields.Fields.Item("U_Peso").Value = _Weight.ToString();
                        try
                        {
                            oBatchNumbers.UserFields.Fields.Item("U_CantIngreso").Value = Math.Round(_Quantity, 3).ToString();
                        }
                        catch { }
                        oBatchNumbers.UserFields.Fields.Item("U_Pedido").Value = _OrderNumber.ToString();


                        //update kardex detail 
                        try
                        {
                            DB_INVTableAdapters.Tbl_INV_Mov_DetailTableAdapter _DbInv_Detail = new DB_INVTableAdapters.Tbl_INV_Mov_DetailTableAdapter();
                            _DbInv_Detail.Update_Detail_Unicamente(0, _ItemDescription, Convert.ToInt32(_ItmsGrpCod), _ItmsGrpNam, _BatchNumber.ToString(), _WarehouseCode, _WhsName, _Comments, _DocCurrency, Convert.ToDecimal(_Price), Convert.ToDecimal(_Price * _Quantity), _Account, _ItemCode, _MovimientoDetailId);
                        }
                        catch (Exception ex)
                        {
                            GT.System_GT.f_error(ex, "oInventory.cs", "");
                        }

                        // actualiza bod_recepcion toriflex
                        if (idAlmacen == 5 && idTipodocumento == 8)
                        {
                            DbCustServTableAdapters.Tbl_Trn_Bod_Recepcion_detTableAdapter _db = new DbCustServTableAdapters.Tbl_Trn_Bod_Recepcion_detTableAdapter();
                            try
                            {
                                if (_db.GetDataBy_Tarima(Convert.ToInt32(_BatchNumber)).Rows.Count == 0)
                                {
                                    //--**************************************************
                                    _db.InsertQuery_TarimaDocto(Convert.ToInt32(_BatchNumber));
                                    //---*************************************************
                                    // actualiza rolltracking 
                                    //--**************************************************
                                    _db.UpdateQuery_RollTracking_Ubicacion("11", Convert.ToInt32(_BatchNumber));
                                    //--**************************************************
                                }
                            }
                            catch (Exception ex)
                            {
                                GT.System_GT.f_error(ex, "oInventory.cs", "");
                            }
                        }

                        if (i == _VwTarimasRollTracking.Rows.Count - 1)
                        {

                            // Verificación de Transacción para Evitar Transacción Duplicada Julio Herrera
                            // ------------------------------------------------------------
                            oInventory.UserFields.Fields.Item("U_IdMovDet").Value = _MovimientoDetailId.ToString();
                            DbSapTableAdapters.OIGNTableAdapter _DB_OIGN_Check = new OIGNTableAdapter();
                            var _OIGN_Data = _DB_OIGN_Check.GetDataBy_IdMovDet(_MovimientoDetailId.ToString());

                            if (_OIGN_Data.Count() == 0)
                            {


                                lRetCode = oInventory.Add();

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
                                InvCodeStr = _OIGN_Data[0].DocEntry.ToString();
                            }

                            // Finaliza Modificaciones


                            DbSapTableAdapters.vw_OITLTableAdapter _Db_vw_OITL = new vw_OITLTableAdapter();
                            DbSap.vw_OITLDataTable _tbl_vw_OITL = new DbSap.vw_OITLDataTable();
                            _tbl_vw_OITL = _Db_vw_OITL.GetDataBy_DocEntry(_CompanyNumber, 59, Convert.ToInt32(InvCodeStr));
                            if (_tbl_vw_OITL.Rows.Count > 0)
                            {
                                _DocDate = _tbl_vw_OITL[0].DocDate;
                                _DocCurrency = _tbl_vw_OITL[0].Currency;
                                _DocType = _tbl_vw_OITL[0].DocType;
                                _DocName = _tbl_vw_OITL[0].Code;
                                _DocNum = _tbl_vw_OITL[0].DocNum;


                                DB_INVTableAdapters.Tbl_INV_Mov_HeaderTableAdapter _dbinvHead = new DB_INVTableAdapters.Tbl_INV_Mov_HeaderTableAdapter();
                                _dbinvHead.UpdateQuery_DocName(_DocName, _DocType, _DocNum.ToString(), _DocDate, new Guid(_MovimientoId));

                                _dbinvHead.UpdateQuery_Estado(5, new Guid(_MovimientoId));

                            }
                            else
                            {
                                GT.System_GT.f_error(new Exception("no se encontraron datos en sap."), "vw_OITL52", "");
                            }
                        }

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

            } //for
            return _return;
        }

        //Salidas a Producto Terminado Por Documento **
        [DataObjectMethodAttribute
        (DataObjectMethodType.Insert, true)]
        public string Insert_oInvetoryGenExitDocumento(string _MovimientoId)
        {

            int _CompanyNumber = 0;
            int idTipodocumento = 0;
            int NumeroDocumento = 0;

            string _InternalPartNbr = "";
            double _Price = 0;
            double _Quantity = 0;
            string _WarehouseCode = "";
            string _BatchNumber = "";
            decimal _Weight = 0;
            string _Comments = "Saldos Al: " + DateTime.Now.ToString() + " Por: " + HttpContext.Current.Session["UserName"].ToString();

            string _ItemCode = "";
            string _Account = "";
            int _OrderNumber = 0;

            Guid _MovimientoDetailId;
            int idTipoMovimiento = 2; //--ingreso as bodega
            int idTipoDocumento = 8; //recepcion de producto terminado
            int idAlmacen = 5;
            int idEstado = 0;


            int CustomerNumber = 0;
            string CustomerName = "";
            string DocName = "";
            int DocType = 0;
            string DocNum = "";
            System.DateTime FechaDocumento;
            global::System.Nullable<global::System.DateTime> DocDate = null;

            string _CustomerUOM = "";
            int _NbrOfUnits = 0;
            string _DocCurrency = "";

            string _return = "";
            SAPbobsCOM.Company oCompany;

            DB_INVTableAdapters.vw_INV_Mov_DetailTableAdapter _DbVwTarimasRollTracking = new DB_INVTableAdapters.vw_INV_Mov_DetailTableAdapter();

            DB_INV.vw_INV_Mov_DetailDataTable _VwTarimasRollTracking = new DB_INV.vw_INV_Mov_DetailDataTable();

            _VwTarimasRollTracking = _DbVwTarimasRollTracking.GetDataBy_MovimientoId(new Guid(_MovimientoId));

            if (_VwTarimasRollTracking.Rows.Count > 0)
            {
                _CompanyNumber = _VwTarimasRollTracking[0].IdEmpresa;
                idTipodocumento = _VwTarimasRollTracking[0].idTipoDocumento;
                NumeroDocumento = _VwTarimasRollTracking[0].NumeroDocumento;
            }

            //-- Datos kardex
            DB_INVTableAdapters.Tbl_INV_Mov_HeaderTableAdapter dbMOvHeader = new DB_INVTableAdapters.Tbl_INV_Mov_HeaderTableAdapter();
            DB_INV.vw_INV_Mov_HeaderDataTable _tblHeader = new DB_INV.vw_INV_Mov_HeaderDataTable();

            _tblHeader = dbMOvHeader.GetDataBy_IdMovimiento(new Guid(_MovimientoId));

            if (_tblHeader.Rows.Count > 0)
            {
                idTipoMovimiento = _tblHeader[0].idTipoMovimiento;
            }

            int _Sucursal = 0;

            oCompany = null;
            oCompany = GlobalSAP.GetCompany(_CompanyNumber);
            _Sucursal = GlobalSAP.GetSucursal(_CompanyNumber);
            _WarehouseCode = GlobalSAP.GetWarehouseCode(_CompanyNumber);

            _Account = GlobalSAP.GetAccount(_CompanyNumber);

            SAPbobsCOM.Documents oInventory;
            //SAPbobsCOM.SerialNumbers oSerialNumbers;
            SAPbobsCOM.BatchNumbers oBatchNumbers;

            oInventory = (SAPbobsCOM.Documents)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oInventoryGenExit);



            for (int i = 0; i <= _VwTarimasRollTracking.Rows.Count - 1; i++)
            {

                _InternalPartNbr = _VwTarimasRollTracking[i].InternalPartNbr;

                //refresca costo

                _Quantity = 0;
                _BatchNumber = "";
                _Weight = 0;
                _Comments = "";
                _ItemCode = "";

                _OrderNumber = 0;

                CustomerNumber = 0;
                CustomerName = "";

                _InternalPartNbr = _VwTarimasRollTracking[i].InternalPartNbr;
                FechaDocumento = _VwTarimasRollTracking[i].FechaDocumento;

                try { _OrderNumber = _VwTarimasRollTracking[i].OrderNumber; }
                catch { }
                //refresca costo

                _Price = Convert.ToDouble(_VwTarimasRollTracking[i].Cost);
                _Quantity = Math.Abs(Math.Round(Convert.ToDouble(_VwTarimasRollTracking[i].Qty), 4));
                try { _BatchNumber = _VwTarimasRollTracking[i].BatchNumber; } catch { }

                if (_BatchNumber == "")
                {
                    _BatchNumber = _VwTarimasRollTracking[i].SkidId.ToString();
                }

                if (_BatchNumber == "" || _BatchNumber == "0")
                {
                    throw new Exception("debe ingresar el número de Lote.");
                }
                //default
                _WarehouseCode = _VwTarimasRollTracking[i].WarehouseLocation;

                //--- wharehouse por medio del batchnumber 
                //-- movimientos polipropileno
                if (idTipoMovimiento == 9 || idTipoMovimiento == 10)
                {
                    DbCustServTableAdapters.vw_OITLTableAdapter _dbOITL = new DbCustServTableAdapters.vw_OITLTableAdapter();
                    DbCustServ.vw_OITLDataTable _tblOITL = new DbCustServ.vw_OITLDataTable();
                    _tblOITL = _dbOITL.GetData_BatchNumber(_BatchNumber);

                    if (_tblOITL.Rows.Count > 0)
                    {
                        _WarehouseCode = _tblOITL[0].Warehouse.ToString();
                        _Sucursal = GlobalSAP.GetSucursal(_tblOITL[0].CompanyNumber);
                        _Account = GlobalSAP.GetAccount(_tblOITL[0].CompanyNumber);
                        _CompanyNumber = _tblOITL[0].CompanyNumber;
                    }
                }
                //------------------------------------------------------
                _Weight = _VwTarimasRollTracking[i].Weight;
                _Comments = _VwTarimasRollTracking[i].Comments;
                _ItemCode = _VwTarimasRollTracking[i].ItemCode;

                _CustomerUOM = _VwTarimasRollTracking[i].UOM;
                _NbrOfUnits = _VwTarimasRollTracking[i].Count_RollBoxNbr;
                _MovimientoDetailId = _VwTarimasRollTracking[i].MovimientoDetailId;

                //CustomerNumber = _VwTarimasRollTracking[i].CustomerNumber;
                //CustomerName = _VwTarimasRollTracking[i].CustomerName;



                try
                {


                    int lRetCode;
                    string InvCodeStr = "";
                    string _AccountCode = "";
                    string _ItemDescription = "";
                    string _ItmsGrpNam = "";
                    int _ItmsGrpCod = 0;
                    DateTime _DocDate;

                    int _DocType = 0;
                    int _DocNum = 0;
                    string _DocName = "";
                    string _WhsName = "";
                    decimal _SapUnitPrice = 0;
                    decimal _SapPrice = 0;

                    if (oCompany.Connected)
                    {
                        //----InternalPart
                        DbSapTableAdapters.OITMTableAdapter _DbOITM = new OITMTableAdapter();
                        _DbOITM.Connection.ConnectionString = GT.System_GT.Get_DbTori(_CompanyNumber);
                        DbSap.OITMDataTable _OITM = new DbSap.OITMDataTable();


                        //if (_InternalPartNbr != "")
                        //{
                        //    _OITM = _DbOITM.GetDataBy_ItemCode(_InternalPartNbr.Substring(0, 8));

                        //    if (_OITM.Rows.Count == 0)
                        //    {
                        //        //no actualiza a sap. 
                        //        //DbPPrensaTableAdapters.v_ProdDefinitionsTableAdapter _dbProdDefinitions = new DbPPrensaTableAdapters.v_ProdDefinitionsTableAdapter();
                        //        //_dbProdDefinitions.Update_SAP(_InternalPartNbr);
                        //        //_OITM = _DbOITM.GetDataBy_ItemCode(_InternalPartNbr.Substring(0, 8));

                        //        throw new Exception("No existe el item code." + _InternalPartNbr.Substring(0, 8));
                        //    }
                        //}

                        if (_ItemCode != "")
                        {
                            _OITM = _DbOITM.GetDataBy_ItemCode(_ItemCode);
                        }
                        else
                        {
                            _OITM = _DbOITM.GetDataBy_ItemCode(_InternalPartNbr.Substring(0, 8));
                        }

                        if (_OITM.Rows.Count > 0)
                        {
                            _ItemCode = _OITM[0].ItemCode;
                            _ItemDescription = _OITM[0].ItemName;
                            _ItmsGrpCod = _OITM[0].ItmsGrpCod;
                        }
                        // ITMS GROUP

                        DbSapTableAdapters.OITBTableAdapter _DbOITB = new OITBTableAdapter();
                        DbSap.OITBDataTable _OITB = new DbSap.OITBDataTable();
                        _OITB = _DbOITB.GetDataBy_ItmsGrpCod(Convert.ToInt16(_ItmsGrpCod));
                        if (_OITB.Rows.Count > 0)
                        {
                            _ItmsGrpNam = _OITB[0].ItmsGrpNam;
                        }

                        //---- Warehouses 
                        DbSapTableAdapters.OWHSTableAdapter _DbOWHS = new OWHSTableAdapter();
                        DbSap.OWHSDataTable _OWHS = new DbSap.OWHSDataTable();

                        _OWHS = _DbOWHS.GetDataBy_WhsCode(_WarehouseCode);
                        if (_OWHS.Rows.Count > 0)
                        {
                            _WhsName = _OWHS[0].WhsName;
                        }

                        //-- Account
                        DbSapTableAdapters.OACTTableAdapter _DBOACT = new OACTTableAdapter();
                        DbSap.OACTDataTable _OACT = new DbSap.OACTDataTable();

                        _OACT = _DBOACT.GetDataBy_FormatCode(_Account);
                        if (_OACT.Rows.Count > 0)
                        {
                            _AccountCode = _OACT[0].AcctCode;
                        }

                        //documento
                        if (i == 0)
                        {
                            oInventory.DocDate = DateTime.Now;  //FechaDocumento;
                            oInventory.TaxDate = DateTime.Now; //FechaDocumento;
                            oInventory.Comments = _Comments;
                            oInventory.DocType = SAPbobsCOM.BoDocumentTypes.dDocument_Items;
                            oInventory.ManualNumber = "N";
                            oInventory.JournalMemo = GT.System_GT.Left(_Comments, 50);
                            oInventory.BPL_IDAssignedToInvoice = _Sucursal;
                            oInventory.Series = 183;
                            if (_Sucursal == 4) oInventory.Series = 185;
                            

                        }

                        //oInventory.Lines.BaseEntry = Convert.ToInt32(InvCodeStr);
                        //oInventory.Lines.BaseLine = 

                        if (i != 0)
                        {
                            oInventory.Lines.Add();
                        }

                        oInventory.Lines.ItemCode = _ItemCode;
                        oInventory.Lines.AccountCode = _AccountCode;
                        oInventory.Lines.ItemDescription = _ItemDescription;
                        oInventory.Lines.WarehouseCode = _WarehouseCode;
                        oInventory.Lines.Price = _Price;
                        oInventory.Lines.Quantity = _Quantity;

                        //Verifica existencias. 

                        VerifySapInventoryQuantity(oCompany, _WarehouseCode, _BatchNumber, Decimal.Parse( _Quantity.ToString()), _ItemCode,"===>","");

                        //DbSapTableAdapters.vw_OITWTableAdapter _OITW = new DbSapTableAdapters.vw_OITWTableAdapter();
                        //decimal xSaldo = _OITW.GetData_SaldoBy_ItemCode(_ItemCode, _BatchNumber, _WarehouseCode);

                        //if (xSaldo < Convert.ToDecimal(_Quantity))
                        //{
                        //    var _ShipmentStr = ObtenerShipmentdeTarima(_BatchNumber);
                        //    if (_ShipmentStr != "") _ShipmentStr = " Shipment: " + _ShipmentStr + " ";
                        //    DbSapTableAdapters.OIGETableAdapter _DB_OIGE_Check2 = new OIGETableAdapter();
                        //    var _OIGE_Data2 = _DB_OIGE_Check2.GetDataBy_IdMovDet(_MovimientoDetailId.ToString());

                        //    if (_OIGE_Data2.Count() == 0) throw new Exception("===> No hay cantidad suficiente en el inventario de SAP. " + _ShipmentStr + " Item: " + _ItemCode + " Lote: " + _BatchNumber + " Bodega: " + _WarehouseCode + " Requerido: " + _Quantity.ToString() + " Saldo: " + xSaldo.ToString());
                        //}


                        //Proyectos
                        //DbSapTableAdapters.OPRJTableAdapter _OPRJ = new OPRJTableAdapter();
                        //_OPRJ.Connection.ConnectionString = GT.System_GT.Get_DbTori(_CompanyNumber);

                        //if (_OPRJ.GetDataBy_PrjCode(_OrderNumber.ToString()).Rows.Count == 0)
                        //{
                        //    DbSapTableAdapters.oProjectTableAdapter _oProject = new oProjectTableAdapter();

                        //    _oProject.Sync_oProject(_OrderNumber);
                        //}

                        //oInventory.Lines.ProjectCode = _OrderNumber.ToString();


                        //===============================================
                        //'bacthes

                        oBatchNumbers = oInventory.Lines.BatchNumbers;
                        if (i != 0)
                        {
                            oBatchNumbers.Add();
                        }

                        //oBatchNumbers.SetCurrentLine(1);
                        //oBatchNumbers.BaseLineNumber = 0;
                        oBatchNumbers.BatchNumber = _BatchNumber;
                        oBatchNumbers.Quantity = Math.Abs(_Quantity);
                        oBatchNumbers.Location = _WarehouseCode;

                        oBatchNumbers.UserFields.Fields.Item("U_Peso").Value = _Weight.ToString();
                        oBatchNumbers.UserFields.Fields.Item("U_Pedido").Value = _OrderNumber.ToString();


                        //update kardex detail 
                        try
                        {

                            DB_INVTableAdapters.Tbl_INV_Mov_DetailTableAdapter _DbInv_Detail = new DB_INVTableAdapters.Tbl_INV_Mov_DetailTableAdapter();

                            _DbInv_Detail.Update_Detail_Unicamente(0, _ItemDescription, Convert.ToInt32(_ItmsGrpCod), _ItmsGrpNam, _BatchNumber.ToString(), _WarehouseCode, _WhsName, _Comments, _DocCurrency, Convert.ToDecimal(_Price), Convert.ToDecimal(_Price * _Quantity), _Account, _ItemCode, _MovimientoDetailId);

                        }
                        catch (Exception ex)
                        {
                            GT.System_GT.f_error(ex, "oInventory.cs", "");
                        }

                        if (i == _VwTarimasRollTracking.Rows.Count - 1)
                        {

                            oInventory.UserFields.Fields.Item("U_IdMovDet").Value = _MovimientoDetailId.ToString();
                            DbSapTableAdapters.OIGETableAdapter _DB_OIGE_Check = new OIGETableAdapter();
                            var _OIGE_Data = _DB_OIGE_Check.GetDataBy_IdMovDet(_MovimientoDetailId.ToString());

                            if (_OIGE_Data.Count() == 0)
                            {


                                lRetCode = oInventory.Add();

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
                                InvCodeStr = _OIGE_Data[0].DocEntry.ToString();
                            }

                            // Finalizacion de cAMBIOS



                            DbSapTableAdapters.vw_OITLTableAdapter _Db_vw_OITL = new vw_OITLTableAdapter();
                            DbSap.vw_OITLDataTable _tbl_vw_OITL = new DbSap.vw_OITLDataTable();

                            _tbl_vw_OITL = _Db_vw_OITL.GetDataBy_DocEntry(_CompanyNumber, 60, Convert.ToInt32(InvCodeStr));

                            if (_tbl_vw_OITL.Rows.Count > 0)
                            {
                                _DocDate = _tbl_vw_OITL[0].DocDate;
                                _DocCurrency = _tbl_vw_OITL[0].Currency;
                                _DocType = _tbl_vw_OITL[0].DocType;
                                _DocName = _tbl_vw_OITL[0].Code;
                                _DocNum = _tbl_vw_OITL[0].DocNum;


                                DB_INVTableAdapters.Tbl_INV_Mov_HeaderTableAdapter _dbinvHead = new DB_INVTableAdapters.Tbl_INV_Mov_HeaderTableAdapter();
                                _dbinvHead.UpdateQuery_DocName(_DocName, _DocType, _DocNum.ToString(), _DocDate, new Guid(_MovimientoId));

                                _dbinvHead.UpdateQuery_Estado(5, new Guid(_MovimientoId));

                                _dbinvHead.UpdateQuery_RollTrackingLocation("", _MovimientoDetailId);

                            }
                            else
                            {
                                GT.System_GT.f_error(new Exception("no se encontraron datos en sap."), "vw_OITL53", "");
                            }
                        }

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

            } //for
            return _return;
        }

        //Entradas a Producto Terminado Por Recibo Importaciones **
        [DataObjectMethodAttribute
        (DataObjectMethodType.Insert, true)]
        public string Insert_oInvetoryGenEntryReceiptId(int _ReceiptId)
        {

            int _CompanyNumber = 0;
            int idTipodocumento = 0;
            int NumeroDocumento = 0;

            string _InternalPartNbr = "";
            double _Price = 0;
            double _Quantity = 0;
            string _WarehouseCode = "";
            string _BatchNumber = "";
            decimal _Weight = 0;
            string _Comments = "Recepción Por Recibo: " + DateTime.Now.ToString() + " Por: " + HttpContext.Current.Session["UserName"].ToString();

            string _ItemCode = "";
            string _Account = "";
            int _OrderNumber = 0;

            string _MovimientoId = "";
            string _MovimientoDetailId = "";
            int idTipoMovimiento = 2; //--ingreso as bodega
            int idTipoDocumento = 8; //recepcion de producto terminado
            int idAlmacen = 5;
            int idEstado = 0;


            int CustomerNumber = 0;
            string CustomerName = "";
            string DocName = "";
            int DocType = 0;
            string DocNum = "";
            System.DateTime FechaDocumento = DateTime.Now;
            global::System.Nullable<global::System.DateTime> DocDate = null;

            string _CustomerUOM = "";
            int _NbrOfUnits = 0;
            string _DocCurrency = "";
            int SkidId = 0;
            string _return = "";
            SAPbobsCOM.Company oCompany;

            DbCustServTableAdapters.vwTarimasRollTrackingTableAdapter _DbVwTarimasRollTracking = new DbCustServTableAdapters.vwTarimasRollTrackingTableAdapter();
            DbCustServ.vwTarimasRollTrackingDataTable _VwTarimasRollTracking = new DbCustServ.vwTarimasRollTrackingDataTable();

            _VwTarimasRollTracking = _DbVwTarimasRollTracking.GetDataBy_ReceiptId(_ReceiptId);

            if (_VwTarimasRollTracking.Rows.Count > 0)
            {
                _CompanyNumber = _VwTarimasRollTracking[0].Empresa;
            }

            int _Sucursal = 0;

            oCompany = null;
            oCompany = GlobalSAP.GetCompany(_CompanyNumber);
            _Sucursal = GlobalSAP.GetSucursal(_CompanyNumber);
            _WarehouseCode = GlobalSAP.GetWarehouseCode(_CompanyNumber);

            _Account = GlobalSAP.GetAccount(_CompanyNumber);

            SAPbobsCOM.Documents oInventory;
            //SAPbobsCOM.SerialNumbers oSerialNumbers;
            SAPbobsCOM.BatchNumbers oBatchNumbers;

            oInventory = (SAPbobsCOM.Documents)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oInventoryGenEntry);

            for (int i = 0; i <= _VwTarimasRollTracking.Rows.Count - 1; i++)
            {

                _InternalPartNbr = _VwTarimasRollTracking[i].InternalPartNbr;

                //refresca costo

                _Quantity = 0;
                _BatchNumber = "";
                _Weight = 0;
                _ItemCode = "";
                _OrderNumber = 0;
                CustomerNumber = 0;
                CustomerName = "";

                _InternalPartNbr = _VwTarimasRollTracking[i].InternalPartNbr;
                try { _OrderNumber = _VwTarimasRollTracking[i].OrderNumber; }
                catch { }
                //refresca costo


                _Price = Convert.ToDouble(_VwTarimasRollTracking[i].CostoProduccion);
                _Quantity = Math.Round(Convert.ToDouble(_VwTarimasRollTracking[i].libras), 3);
                _BatchNumber = _VwTarimasRollTracking[i].SkidId.ToString();
                _Weight = _VwTarimasRollTracking[i].RemainingWeight;
                _ItemCode = _VwTarimasRollTracking[i].InternalPartNbr.Substring(10, _VwTarimasRollTracking[i].InternalPartNbr.Length - 10); ;
                SkidId = _VwTarimasRollTracking[i].SkidId;
                _CustomerUOM = _VwTarimasRollTracking[i].CustomerUOM;
                _NbrOfUnits = _VwTarimasRollTracking[i].NbrOfUnits;

                //CustomerNumber = _VwTarimasRollTracking[i].CustomerNumber;
                //CustomerName = _VwTarimasRollTracking[i].CustomerName;

                try
                {
                    if (_VwTarimasRollTracking[i].CustomerUOM != "MSM")
                    {
                        _Price = Math.Round(_VwTarimasRollTracking[i].CostoProduccion, 2);
                    }
                    else
                    {
                        _Price = Math.Round(_VwTarimasRollTracking[i].CostoProduccion * 1000, 2);
                    }
                }
                catch (Exception ex)
                {
                    GT.System_GT.f_error(new Exception(ex.Message + " Error en Costo Producción, Tarima." + _VwTarimasRollTracking[i].SkidId.ToString()), "oInventory.cs", "");
                }

                if (_VwTarimasRollTracking[i].CustomerUOM == "Millar" || _VwTarimasRollTracking[i].CustomerUOM == "MSM")
                {
                    _Quantity = Math.Round(_Quantity / 1000, 3);
                }

                if (_VwTarimasRollTracking[i].CustomerUOM == "Libra" || _VwTarimasRollTracking[i].CustomerUOM == "Kg" || _VwTarimasRollTracking[i].CustomerUOM == "Libra Espanola")
                {
                    _Quantity = Math.Round(_Quantity, 2);

                }

                try
                {


                    int lRetCode;
                    string InvCodeStr = "";
                    string _AccountCode = "";
                    string _ItemDescription = "";
                    string _ItmsGrpNam = "";
                    int _ItmsGrpCod = 0;
                    DateTime _DocDate;

                    int _DocType = 0;
                    int _DocNum = 0;
                    string _DocName = "";
                    string _WhsName = "";
                    decimal _SapUnitPrice = 0;
                    decimal _SapPrice = 0;


                    if (oCompany.Connected)
                    {
                        //----InternalPart
                        DbSapTableAdapters.OITMTableAdapter _DbOITM = new OITMTableAdapter();
                        _DbOITM.Connection.ConnectionString = GT.System_GT.Get_DbTori(_CompanyNumber);
                        DbSap.OITMDataTable _OITM = new DbSap.OITMDataTable();


                        if (_InternalPartNbr != "")
                        {
                            _OITM = _DbOITM.GetDataBy_ItemCode(_InternalPartNbr.Substring(0, 8));

                            if (_OITM.Rows.Count == 0)
                            {
                                //no actualiza a sap. 
                                //DbPPrensaTableAdapters.v_ProdDefinitionsTableAdapter _dbProdDefinitions = new DbPPrensaTableAdapters.v_ProdDefinitionsTableAdapter();
                                //_dbProdDefinitions.Update_SAP(_InternalPartNbr);
                                //_OITM = _DbOITM.GetDataBy_ItemCode(_InternalPartNbr.Substring(0, 8));

                                throw new Exception("No existe el item code." + _InternalPartNbr.Substring(0, 8));


                            }
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
                        }
                        // ITMS GROUP

                        DbSapTableAdapters.OITBTableAdapter _DbOITB = new OITBTableAdapter();
                        DbSap.OITBDataTable _OITB = new DbSap.OITBDataTable();
                        _OITB = _DbOITB.GetDataBy_ItmsGrpCod(Convert.ToInt16(_ItmsGrpCod));
                        if (_OITB.Rows.Count > 0)
                        {
                            _ItmsGrpNam = _OITB[0].ItmsGrpNam;
                        }

                        //---- Warehouses 
                        DbSapTableAdapters.OWHSTableAdapter _DbOWHS = new OWHSTableAdapter();
                        DbSap.OWHSDataTable _OWHS = new DbSap.OWHSDataTable();

                        _OWHS = _DbOWHS.GetDataBy_WhsCode(_WarehouseCode);
                        if (_OWHS.Rows.Count > 0)
                        {
                            _WhsName = _OWHS[0].WhsName;
                        }

                        //-- Account
                        DbSapTableAdapters.OACTTableAdapter _DBOACT = new OACTTableAdapter();
                        DbSap.OACTDataTable _OACT = new DbSap.OACTDataTable();

                        _OACT = _DBOACT.GetDataBy_FormatCode(_Account);
                        if (_OACT.Rows.Count > 0)
                        {
                            _AccountCode = _OACT[0].AcctCode;
                        }

                        //documento
                        if (i == 0)
                        {
                            oInventory.DocDate = DateTime.Now;  //FechaDocumento;
                            oInventory.TaxDate = DateTime.Now; //FechaDocumento;
                            oInventory.Comments = _Comments;
                            oInventory.DocType = SAPbobsCOM.BoDocumentTypes.dDocument_Items;
                            oInventory.ManualNumber = "N";
                            oInventory.JournalMemo = GT.System_GT.Left(_Comments, 50);
                            oInventory.BPL_IDAssignedToInvoice = _Sucursal;

                            //kardex
                            try
                            {
                                DB_INVTableAdapters.Tbl_INV_Mov_HeaderTableAdapter _DbInv_Header = new DB_INVTableAdapters.Tbl_INV_Mov_HeaderTableAdapter();
                                _MovimientoId = _DbInv_Header.Insert_Header(idTipoMovimiento, idTipoDocumento, idEstado, _CompanyNumber, NumeroDocumento, CustomerNumber, CustomerName, DocName, DocType, DocNum, FechaDocumento, DocDate);

                            }
                            catch (Exception ex)
                            {
                                GT.System_GT.f_error(ex, "kardex oInventory.cs", "");
                            }


                        }

                        //oInventory.Lines.BaseEntry = Convert.ToInt32(InvCodeStr);
                        //oInventory.Lines.BaseLine = 

                        if (i != 0)
                        {
                            oInventory.Lines.Add();
                        }

                        oInventory.Lines.ItemCode = _ItemCode;
                        oInventory.Lines.AccountCode = _AccountCode;
                        oInventory.Lines.ItemDescription = _ItemDescription;
                        oInventory.Lines.WarehouseCode = _WarehouseCode;
                        oInventory.Lines.Price = _Price;
                        oInventory.Lines.Quantity = _Quantity;



                        //===============================================
                        //'bacthes

                        oBatchNumbers = oInventory.Lines.BatchNumbers;
                        if (i != 0)
                        {
                            oBatchNumbers.Add();
                        }

                        //oBatchNumbers.SetCurrentLine(1);
                        //oBatchNumbers.BaseLineNumber = 0;
                        oBatchNumbers.BatchNumber = _BatchNumber;
                        oBatchNumbers.Quantity = Math.Round(_Quantity, 3);
                        oBatchNumbers.Location = _WarehouseCode;

                        oBatchNumbers.UserFields.Fields.Item("U_Peso").Value = _Weight.ToString();
                        oBatchNumbers.UserFields.Fields.Item("U_Pedido").Value = _OrderNumber.ToString();


                        //insert kardex detail 
                        try
                        {

                            DB_INVTableAdapters.Tbl_INV_Mov_DetailTableAdapter _DbInv_Detail = new DB_INVTableAdapters.Tbl_INV_Mov_DetailTableAdapter();

                            _MovimientoDetailId = _DbInv_Detail.Insert_Detail(new Guid(_MovimientoId), _WarehouseCode, _OrderNumber, SkidId, _ItemCode, _InternalPartNbr, Convert.ToDecimal(_Quantity), _CustomerUOM, _NbrOfUnits, _Weight, Convert.ToDecimal(_Price), Convert.ToDecimal(_Price * _Quantity), 0, "", 0, "", "", "", "", _Comments, "", Convert.ToDecimal(_Price), Convert.ToDecimal(_Price * _Quantity), "", 0, "", 0, 0, idAlmacen, i);

                        }
                        catch (Exception ex)
                        {
                            GT.System_GT.f_error(ex, "oInventory.cs", "");
                        }

                        if (i == _VwTarimasRollTracking.Rows.Count - 1)
                        {


                            // Verificación de Transacción para Evitar Transacción Duplicada Julio Herrera
                            // ------------------------------------------------------------
                            oInventory.UserFields.Fields.Item("U_IdMovDet").Value = _MovimientoDetailId.ToString();
                            DbSapTableAdapters.OIGNTableAdapter _DB_OIGN_Check = new OIGNTableAdapter();
                            var _OIGN_Data = _DB_OIGN_Check.GetDataBy_IdMovDet(_MovimientoDetailId.ToString());

                            if (_OIGN_Data.Count() == 0)
                            {


                                lRetCode = oInventory.Add();

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
                                InvCodeStr = _OIGN_Data[0].DocEntry.ToString();
                            }

                            // Finalizacion de cAMBIOS


                            DbSapTableAdapters.vw_OITLTableAdapter _Db_vw_OITL = new vw_OITLTableAdapter();
                            DbSap.vw_OITLDataTable _tbl_vw_OITL = new DbSap.vw_OITLDataTable();

                            _tbl_vw_OITL = _Db_vw_OITL.GetDataBy_DocEntry(_CompanyNumber, 59, Convert.ToInt32(InvCodeStr));

                            if (_tbl_vw_OITL.Rows.Count > 0)
                            {
                                _DocDate = _tbl_vw_OITL[0].DocDate;
                                _DocCurrency = _tbl_vw_OITL[0].Currency;
                                _DocType = _tbl_vw_OITL[0].DocType;
                                _DocName = _tbl_vw_OITL[0].Code;
                                _DocNum = _tbl_vw_OITL[0].DocNum;


                                DB_INVTableAdapters.Tbl_INV_Mov_HeaderTableAdapter _dbinvHead = new DB_INVTableAdapters.Tbl_INV_Mov_HeaderTableAdapter();
                                _dbinvHead.UpdateQuery_DocName(_DocName, _DocType, _DocNum.ToString(), _DocDate, new Guid(_MovimientoId));

                                _dbinvHead.UpdateQuery_Estado(5, new Guid(_MovimientoId));

                            }
                            else
                            {
                                GT.System_GT.f_error(new Exception("no se encontraron datos en sap."), "vw_OITL54", "");
                            }
                        }

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

            } //for
            return _return;
        }




        //Salidas de Producto Terminado Por Ajuste. **
        [DataObjectMethodAttribute
        (DataObjectMethodType.Insert, true)]
        public string Insert_oInventoryGenExit(int _CompanyNumber, string _InternalPartNbr, double _Price, double _Quantity, string _WarehouseCode, string _BatchNumber, decimal _Weight, string _Comments, string _ItemCode, string _Account, int _OrderNumber, Guid Original_MovimientoDetailId)
        {

            string _return = "";
            SAPbobsCOM.Company oCompany;
            oCompany = GlobalSAP.GetCompany(_CompanyNumber);
            int _Sucursal = 0;

            _Sucursal = GlobalSAP.GetSucursal(_CompanyNumber);

            try
            {
                SAPbobsCOM.Documents oInventory;
                //SAPbobsCOM.SerialNumbers oSerialNumbers;
                SAPbobsCOM.BatchNumbers oBatchNumbers;

                oInventory = (SAPbobsCOM.Documents)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oInventoryGenExit);

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
                string _DocName = "";
                string _WhsName = "";
                decimal _SapUnitPrice = 0;
                decimal _SapPrice = 0;

                if (oCompany.Connected)
                {
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
                    }
                    else
                    {
                        //DbPPrensaTableAdapters.v_ProdDefinitionsTableAdapter _dbProdDefinitions = new DbPPrensaTableAdapters.v_ProdDefinitionsTableAdapter();
                        //_dbProdDefinitions.Update_SAP(_InternalPartNbr);
                        if (_InternalPartNbr != "")
                        {
                            throw new Exception("No existe el item code." + _InternalPartNbr.Substring(0, 8));

                            string ErrRef = "";
                            string _Mensaje = "No existe el item code." + _InternalPartNbr.Substring(0, 8) + " Internal Part Nbr:  " + _InternalPartNbr + " Para la tarima No." + _BatchNumber.ToString();

                            // Quitar Comentario
                            GT.System_GT.Mandar_CorreoPorAlertas(ref ErrRef, 4, "No existe el item code." + _InternalPartNbr.Substring(0, 8), _Mensaje, "1");

                            if (ErrRef != "")
                            {
                                GT.System_GT.f_error(new Exception(ErrRef), "oDeliveryNote.cs", "");
                            }


                            _ItemCode = _InternalPartNbr.Substring(0, 8);
                            _ItemDescription = _InternalPartNbr;

                        }
                        else
                        {
                            throw new Exception("No existe el item code." + _ItemCode);
                        }

                    }

                    //-- Account
                    DbSapTableAdapters.OACTTableAdapter _DBOACT = new OACTTableAdapter();
                    DbSap.OACTDataTable _OACT = new DbSap.OACTDataTable();

                    _OACT = _DBOACT.GetDataBy_FormatCode(_Account);
                    if (_OACT.Rows.Count > 0)
                    {
                        _AccountCode = _OACT[0].AcctCode;
                    }

                    oInventory.DocDate = DateTime.Now.Date;
                    oInventory.TaxDate = DateTime.Now.Date;
                    oInventory.Comments = _Comments;
                    oInventory.DocType = SAPbobsCOM.BoDocumentTypes.dDocument_Items;
                    oInventory.ManualNumber = "N";
                    oInventory.JournalMemo = GT.System_GT.Left(_Comments, 50);
                    oInventory.BPL_IDAssignedToInvoice = _Sucursal;

                    //oInventory.Lines.BaseEntry = Convert.ToInt32(InvCodeStr);
                    oInventory.Lines.ItemCode = _ItemCode;
                    oInventory.Lines.AccountCode = _AccountCode;
                    oInventory.Lines.ItemDescription = _ItemDescription;
                    oInventory.Lines.WarehouseCode = _WarehouseCode;
                    oInventory.Lines.Price = _Price;
                    oInventory.Lines.Quantity = _Quantity;


                    // Validar Existencia Nuevo Julio Herrera
                    VerifySapInventoryQuantity(oCompany, _WarehouseCode, _BatchNumber, Convert.ToDecimal(_Quantity), _ItemCode, "(=> ", "");


                    ////Verifica existencias. 
                    //DbSapTableAdapters.vw_OITWTableAdapter _OITW = new DbSapTableAdapters.vw_OITWTableAdapter();
                    //decimal xSaldo = _OITW.GetData_SaldoBy_ItemCode(_ItemCode, _BatchNumber, _WarehouseCode);


                    //if (xSaldo < Convert.ToDecimal(_Quantity))
                    //{
                    //    var _ShipmentStr = ObtenerShipmentdeTarima(_BatchNumber);
                    //    if (_ShipmentStr != "") _ShipmentStr = " Shipment: " + _ShipmentStr + " ";
                    //    throw new Exception("=> No hay cantidad suficiente en el inventario de SAP. " + _ShipmentStr + " Item: " + _ItemCode + " Lote: " + _BatchNumber + " Bodega: " + _WarehouseCode + " Requerido: " + _Quantity.ToString() + " Saldo: " + xSaldo.ToString());
                    //}


                    ////Proyectos
                    //DbSapTableAdapters.OPRJTableAdapter _OPRJ = new OPRJTableAdapter();
                    //_OPRJ.Connection.ConnectionString = GT.System_GT.Get_DbTori(_CompanyNumber);

                    //if (_OPRJ.GetDataBy_PrjCode(_OrderNumber.ToString()).Rows.Count == 0)
                    //{
                    //    DbSapTableAdapters.oProjectTableAdapter _oProject = new oProjectTableAdapter();

                    //    _oProject.Sync_oProject(_OrderNumber);
                    //}

                    //oInventory.Lines.ProjectCode = _OrderNumber.ToString();


                    //===============================================
                    //'bacthes
                    oBatchNumbers = oInventory.Lines.BatchNumbers;
                    //oBatchNumbers.SetCurrentLine(1);
                    //oBatchNumbers.BaseLineNumber = 0;
                    oBatchNumbers.BatchNumber = _BatchNumber;
                    oBatchNumbers.Quantity = _Quantity;
                    oBatchNumbers.Location = _WarehouseCode;

                    _Weight = _Weight * -1;

                    oBatchNumbers.UserFields.Fields.Item("U_Peso").Value = (_Weight).ToString();
                    oBatchNumbers.UserFields.Fields.Item("U_Pedido").Value = _OrderNumber.ToString();
                    oBatchNumbers.Add();

                    // Verificación de Transacción para Evitar Transacción Duplicada Julio Herrera
                    // ------------------------------------------------------------
                    oInventory.UserFields.Fields.Item("U_IdMovDet").Value = Original_MovimientoDetailId.ToString();
                    DbSapTableAdapters.OIGETableAdapter _DB_OIGE_Check = new OIGETableAdapter();
                    var _OIGE_Data = _DB_OIGE_Check.GetDataBy_IdMovDet(Original_MovimientoDetailId.ToString());

                    if (_OIGE_Data.Count() == 0)
                    {


                        lRetCode = oInventory.Add();

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
                        InvCodeStr = _OIGE_Data[0].DocEntry.ToString();
                    }



                    //------------------------------actualiza kardex
                    DbSapTableAdapters.vw_OITLTableAdapter _Db_vw_OITL = new vw_OITLTableAdapter();                    

                    DbSap.vw_OITLDataTable _tbl_vw_OITL = new DbSap.vw_OITLDataTable();                    
                    _tbl_vw_OITL = _Db_vw_OITL.GetDataBy_DocEntry(_CompanyNumber, 60, Convert.ToInt32(InvCodeStr));
                    



                    if (_tbl_vw_OITL.Rows.Count > 0)
                    {
                        _DocDate = _tbl_vw_OITL[0].DocDate;
                        _DocCurrency = _tbl_vw_OITL[0].Currency;
                        _DocType = _tbl_vw_OITL[0].DocType;
                        _DocName = _tbl_vw_OITL[0].Code;
                        _DocNum = _tbl_vw_OITL[0].DocNum;
                        _ItmsGrpCod = _tbl_vw_OITL[0].ItmsGrpCod;
                        _ItmsGrpNam = _tbl_vw_OITL[0].ItmsGrpNam;
                        _WhsName = _tbl_vw_OITL[0].WhsName;
                        _ItemCode = _tbl_vw_OITL[0].ItemCode;

                        _SapUnitPrice = _tbl_vw_OITL[0].CalcPrice;
                        _SapPrice = _tbl_vw_OITL[0].OpenValue * -1;

                        DB_INVTableAdapters.Tbl_INV_Mov_DetailTableAdapter _dbinv = new DB_INVTableAdapters.Tbl_INV_Mov_DetailTableAdapter();

                        _dbinv.Update_Detail(Convert.ToInt32(InvCodeStr), _ItemDescription, _ItmsGrpCod, _ItmsGrpNam, _BatchNumber, _WarehouseCode, _WhsName, _Comments, _DocCurrency, _SapUnitPrice, _SapPrice, _Account, _DocName, _DocType, _DocNum.ToString(), _DocDate, _ItemCode, Original_MovimientoDetailId);

                    }
                    else
                    {
                        
                        GT.System_GT.f_error(new Exception("no se encontraron datos en sap. "+ InvCodeStr), "vw_OITL55", "");
                        

                    }


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



        //Salidas de Producto Terminado Por Ajuste Materia Prima **
        [DataObjectMethodAttribute
        (DataObjectMethodType.Insert, true)]
        public string Insert_oInventoryGenExit_MP(int _CompanyNumber, string _InternalPartNbr, double _Price, double _Quantity, string _WarehouseCode, string _BatchNumber, decimal _Weight, string _Comments, string _ItemCode, string _Account, int _OrderNumber, Guid Original_MovimientoDetailId)
        {

            string _return = "";
            SAPbobsCOM.Company oCompany;
            oCompany = GlobalSAP.GetCompany(_CompanyNumber);

            int _Sucursal = 0;

            _Sucursal = GlobalSAP.GetSucursal(_CompanyNumber);

            try
            {
                SAPbobsCOM.SBObob _oBridge = (SAPbobsCOM.SBObob)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoBridge);

                SAPbobsCOM.Documents oInventory = (SAPbobsCOM.Documents)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oInventoryGenExit);
                SAPbobsCOM.BatchNumbers oBatchNumbers;

                
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
                string _DocName = "";
                string _WhsName = "";
                decimal _SapUnitPrice = 0;
                decimal _SapPrice = 0;

                if (oCompany.Connected)
                {
                    //----InternalPart


                    SAPbobsCOM.Items _oItems = (SAPbobsCOM.Items)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oItems);

                    var _oItemsEncontrado = false;
                    var _ItemCodeBuscar = (_InternalPartNbr != "") ? _InternalPartNbr.Substring(0, 8) : _ItemCode;
                    _oItemsEncontrado = _oItems.GetByKey(_ItemCodeBuscar);



                    if (_oItemsEncontrado == true)
                    {
                        _ItemCode = _oItems.ItemCode;
                        _ItemDescription = _oItems.ItemName;
                    }
                    else
                    {

                        if (_InternalPartNbr != "")
                        {
                            throw new Exception("No existe el item code." + _InternalPartNbr.Substring(0, 8));
                        }
                        else
                        {
                            throw new Exception("No existe el item code." + _ItemCode);
                        }

                    }

                    //-- Account

                    SAPbobsCOM.Recordset _orsAccount = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                    _orsAccount = _oBridge.GetObjectKeyBySingleValue(SAPbobsCOM.BoObjectTypes.oChartOfAccounts, "FormatCode", _Account, SAPbobsCOM.BoQueryConditions.bqc_Equal);

                    if (_orsAccount.RecordCount != 0)
                    {
                        while (!_orsAccount.EoF)
                        {
                            _AccountCode = (String)_orsAccount.Fields.Item("AcctCode").Value;
                            _orsAccount.MoveNext();
                        }
                    }
                    else
                    {
                        throw new Exception("No existe la cuenta contable." + _ItemCode);
                    }



                    //-- Datos Transacción
                    //-----------------------
                    oInventory.DocDate = DateTime.Now.Date;
                    oInventory.TaxDate = DateTime.Now.Date;
                    oInventory.Comments = _Comments;
                    oInventory.DocType = SAPbobsCOM.BoDocumentTypes.dDocument_Items;
                    oInventory.ManualNumber = "N";
                    oInventory.JournalMemo = GT.System_GT.Left(_Comments, 50);
                    oInventory.BPL_IDAssignedToInvoice = _Sucursal;


                    oInventory.Lines.ItemCode = _ItemCode;
                    oInventory.Lines.AccountCode = _AccountCode;
                    oInventory.Lines.ItemDescription = _ItemDescription;
                    oInventory.Lines.WarehouseCode = _WarehouseCode;
                    oInventory.Lines.Price = _Price;
                    oInventory.Lines.Quantity = _Quantity;

                    //Verifica existencias. 

                    SAPbobsCOM.Recordset _orsVerificaExistencias = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                    _orsVerificaExistencias.DoQuery(String.Format("select I.ItemCode,BatchNum,Quantity,WhsCode,i.InDate,p.LstEvlPric,p.U_CodigoToriflex from OIBT AS I inner join OITM as P on I.ItemCode = P.ItemCode  inner join OITB as G on P.ItmsGrpCod = G.ItmsGrpCod  where  Quantity > 0 AND I.ItemCode = '{0}' AND I.BatchNum = '{1}' AND I.WhsCode = '{2}'"
                                               , _ItemCode, _BatchNumber, _WarehouseCode));


                    if (_orsVerificaExistencias.RecordCount == 0)
                    {
                        throw new Exception("==> No hay cantidad suficiente en el inventario de SAP. " + " Item: " + _ItemCode + " Lote: " + _BatchNumber + " Bodega: " + _WarehouseCode + " Requerido: " + _Quantity.ToString() + " Saldo: 0");
                    }
                    else
                    {
                        while (!_orsVerificaExistencias.EoF)
                        {
                            Decimal _ExistenciaArt = Decimal.Parse( ((Double)_orsVerificaExistencias.Fields.Item("Quantity").Value).ToString());
                            if (_ExistenciaArt < Convert.ToDecimal(_Quantity))
                            {
                                throw new Exception("==> No hay cantidad suficiente en el inventario de SAP. " + " Item: " + _ItemCode + " Lote: " + _BatchNumber + " Bodega: " + _WarehouseCode + " Requerido: " + _Quantity.ToString() + " Saldo: " + _ExistenciaArt.ToString());
                            }
                            _orsVerificaExistencias.MoveNext();
                        }
                    }



                    //===============================================
                    //'bacthes
                    oBatchNumbers = oInventory.Lines.BatchNumbers;
                    oBatchNumbers.BatchNumber = _BatchNumber;
                    oBatchNumbers.Quantity = _Quantity;
                    oBatchNumbers.Location = _WarehouseCode;
                    _Weight = _Weight * -1;
                    oBatchNumbers.UserFields.Fields.Item("U_Peso").Value = (_Weight).ToString();
                    oBatchNumbers.UserFields.Fields.Item("U_Pedido").Value = _OrderNumber.ToString();
                    oBatchNumbers.Add();
                    oInventory.UserFields.Fields.Item("U_IdMovDet").Value = Original_MovimientoDetailId.ToString();


                    // Verificación de Transacción para Evitar Transacción Duplicada Julio Herrera
                    // ------------------------------------------------------------
                    SAPbobsCOM.Recordset _orsCheckTran = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                    _orsCheckTran.DoQuery(string.Format("SELECT DocEntry FROM OIGE WHERE U_IdMovDet = '{0}'", Original_MovimientoDetailId.ToString()));




                    if (_orsCheckTran.RecordCount == 0)
                    {
                        lRetCode = oInventory.Add();
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
                        while (!_orsCheckTran.EoF)
                        {
                            InvCodeStr = ((Int32)_orsCheckTran.Fields.Item("DocEntry").Value).ToString();
                            _orsAccount.MoveNext();
                        }
                    }

                    // Finalizacion de cAMBIOS




                    //------------------------------actualiza kardex
                    SAPbobsCOM.Recordset _orsRegistroGrabado = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);

                    String _queryRegistroGrabado = "SELECT T0.[DocDate], T0.[ObjType], T0.[DocNum], T1.[ItemCode], T1.[Dscription], T1.[WhsCode], T2.[ItmsGrpCod], T3.[ItmsGrpNam], T4.[WhsName], T5.[CalcPrice], T5.[Currency], T5.[OpenValue], T1.[Quantity] FROM [dbo].[OIGE]  T0 INNER JOIN [dbo].[IGN1]  T1 ON T0.[DocEntry] = T1.[DocEntry] INNER JOIN OITM T2 ON T1.[ItemCode] = T2.[ItemCode] INNER JOIN OITB T3 ON T2.[ItmsGrpCod] = T3.[ItmsGrpCod] INNER JOIN OWHS T4 ON T1.[WhsCode] = T4.[WhsCode] INNER JOIN OINM T5 ON T5.TransType = t0.ObjType AND T5.BASE_REF = T0.DocNum AND T5.DocLineNum = T1.LineNum WHERE T0.[DocEntry] = {0}";
                    _orsRegistroGrabado.DoQuery(string.Format(_queryRegistroGrabado, InvCodeStr));

                    if (_orsRegistroGrabado.RecordCount == 1)
                    {


                        while (!_orsRegistroGrabado.EoF)
                        {

                            _DocDate = (DateTime)_orsRegistroGrabado.Fields.Item("DocDate").Value;
                            _DocCurrency = (String)_orsRegistroGrabado.Fields.Item("Currency").Value;
                            _DocType = Int32.Parse((String)_orsRegistroGrabado.Fields.Item("ObjType").Value);
                            _DocName = "Goods Issue";
                            _DocNum = (Int32)_orsRegistroGrabado.Fields.Item("DocNum").Value;
                            _ItmsGrpCod = (Int32)_orsRegistroGrabado.Fields.Item("ItmsGrpCod").Value;
                            _ItmsGrpNam = (String)_orsRegistroGrabado.Fields.Item("ItmsGrpNam").Value;
                            _WhsName = (String)_orsRegistroGrabado.Fields.Item("ItmsGrpNam").Value;
                            _ItemCode = (String)_orsRegistroGrabado.Fields.Item("ItemCode").Value;

                            _SapUnitPrice = Decimal.Parse(((Double)_orsRegistroGrabado.Fields.Item("CalcPrice").Value).ToString());
                            _SapPrice = Decimal.Parse(((Double)_orsRegistroGrabado.Fields.Item("OpenValue").Value).ToString());

                            DB_INVTableAdapters.Tbl_INV_Mov_DetailTableAdapter _dbinv = new DB_INVTableAdapters.Tbl_INV_Mov_DetailTableAdapter();

                            _dbinv.Update_Detail(Convert.ToInt32(InvCodeStr), _ItemDescription, _ItmsGrpCod, _ItmsGrpNam, _BatchNumber, _WarehouseCode, _WhsName, _Comments, _DocCurrency, _SapUnitPrice, _SapPrice, _Account, _DocName, _DocType, _DocNum.ToString(), _DocDate, _ItemCode, Original_MovimientoDetailId);

                            _orsRegistroGrabado.MoveNext();
                        }

                     
                    }
                    else
                    {
                        GT.System_GT.f_error(new Exception("no se encontraron datos en sap."), "vw_OITL56", "");
                    }


                }
                else
                {
                    throw new Exception("Compania no conectada");
                }
            }
            catch (Exception ex)
            {
                oCompany.Disconnect();
                throw (ex);
            }
            oCompany.Disconnect();
            return _return;
        }


        //Salidas de Producto Terminado Por Ajuste Materia Prima **
        [DataObjectMethodAttribute
        (DataObjectMethodType.Insert, true)]
        public string Insert_oInventoryGenExit_MP_PorDocto(int _CompanyNumber, List<Tbl_INV_Mov_Detail> _p_Detalle ) 
            
            //string _InternalPartNbr, double _Price, double _Quantity, string _WarehouseCode, 
            //string _BatchNumber, decimal _Weight, string _Comments, string _ItemCode, string _Account, int _OrderNumber, Guid Original_MovimientoDetailId)
        {

            string _return = "";
            SAPbobsCOM.Company oCompany = GlobalSAP.GetCompany(_CompanyNumber);
            int _Sucursal = GlobalSAP.GetSucursal(_CompanyNumber);

            try
            {
                SAPbobsCOM.SBObob _oBridge = (SAPbobsCOM.SBObob)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoBridge);
                SAPbobsCOM.Documents oInventory = (SAPbobsCOM.Documents)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oInventoryGenExit);
                

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
                string _DocName = "";
                string _WhsName = "";
                decimal _SapUnitPrice = 0;
                decimal _SapPrice = 0;

                if (oCompany.Connected)
                {
                    

                    //-- Account

                    SAPbobsCOM.Recordset _orsAccount = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                    _orsAccount = _oBridge.GetObjectKeyBySingleValue(SAPbobsCOM.BoObjectTypes.oChartOfAccounts, "FormatCode", _p_Detalle[0].Account, SAPbobsCOM.BoQueryConditions.bqc_Equal);

                    if (_orsAccount.RecordCount != 0)
                    {
                        while (!_orsAccount.EoF)
                        {
                            _AccountCode = (String)_orsAccount.Fields.Item("AcctCode").Value;
                            _orsAccount.MoveNext();
                        }
                    }
                    else
                    {
                        throw new Exception("No existe la cuenta contable." + _p_Detalle[0].Account);
                    }



                    //-- Datos Transacción
                    //-----------------------
                    oInventory.DocDate = DateTime.Now.Date;
                    oInventory.TaxDate = DateTime.Now.Date;
                    oInventory.Comments = _p_Detalle[0].Comments;
                    oInventory.DocType = SAPbobsCOM.BoDocumentTypes.dDocument_Items;
                    oInventory.ManualNumber = "N";
                    oInventory.JournalMemo = GT.System_GT.Left(oInventory.Comments, 50);
                    oInventory.BPL_IDAssignedToInvoice = _Sucursal;


                    var _Conteo = 1;
                    foreach ( var _Detalle_Linea in _p_Detalle)
                    {
                        SAPbobsCOM.BatchNumbers oBatchNumbers;

                        string _InternalPartNbr = "";
                        double _Price = Double.Parse(_Detalle_Linea.Price.ToString());
                        double _Quantity = Double.Parse(_Detalle_Linea.Qty.ToString());
                        string _WarehouseCode = _Detalle_Linea.WareHouse;
                        string _BatchNumber = _Detalle_Linea.BatchNumber;
                        decimal _Weight = (decimal)_Detalle_Linea.Weight;
                        string _ItemCode = _Detalle_Linea.InternalPartNbr;
                        string _Account = _Detalle_Linea.Account;
                        int _OrderNumber = 0;
                        try
                        {
                            _OrderNumber = (int)_Detalle_Linea.OrderNumber;
                        }
                        catch { }
                        

                        //----InternalPart


                        SAPbobsCOM.Items _oItems = (SAPbobsCOM.Items)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oItems);

                        var _oItemsEncontrado = false;
                        var _ItemCodeBuscar = (_InternalPartNbr != "") ? _InternalPartNbr.Substring(0, 8) : _ItemCode;
                        _oItemsEncontrado = _oItems.GetByKey(_ItemCodeBuscar);



                        if (_oItemsEncontrado == true)
                        {
                            _ItemCode = _oItems.ItemCode;
                            _ItemDescription = _oItems.ItemName;
                        }
                        else
                        {

                            if (_InternalPartNbr != "")
                            {
                                throw new Exception("No existe el item code." + _InternalPartNbr.Substring(0, 8));
                            }
                            else
                            {
                                throw new Exception("No existe el item code." + _ItemCode);
                            }

                        }

                        oInventory.Lines.ItemCode = _ItemCode;
                        oInventory.Lines.AccountCode = _AccountCode;
                        oInventory.Lines.ItemDescription = _ItemDescription;
                        oInventory.Lines.WarehouseCode = _WarehouseCode;
                        oInventory.Lines.Price = _Price;
                        oInventory.Lines.Quantity = _Quantity;

                        oInventory.UserFields.Fields.Item("U_IdMovDet").Value = _p_Detalle[0].MovimientoId.ToString();

                        //oInventory.UserFields.Fields.Item("U_OV").Value = _p_Detalle[0].OrderNumber;

                        //oInventory.Reference2 = _p_Detalle[0].ProcessName.ToString();

                        //Verifica existencias. 

                        SAPbobsCOM.Recordset _orsVerificaExistencias = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                        _orsVerificaExistencias.DoQuery(String.Format("select I.ItemCode,BatchNum,Quantity,WhsCode,i.InDate,p.LstEvlPric,p.U_CodigoToriflex from OIBT AS I inner join OITM as P on I.ItemCode = P.ItemCode  inner join OITB as G on P.ItmsGrpCod = G.ItmsGrpCod  where  Quantity > 0 AND I.ItemCode = '{0}' AND I.BatchNum = '{1}' AND I.WhsCode = '{2}'"
                                                   , _ItemCode, _BatchNumber, _WarehouseCode));

                        if (_orsVerificaExistencias.RecordCount == 0)
                        {
                            throw new Exception("==> No hay cantidad suficiente en el inventario de SAP. " + " Item: " + _ItemCode + " Lote: " + _BatchNumber + " Bodega: " + _WarehouseCode + " Requerido: " + _Quantity.ToString() + " Saldo: 0");
                        }
                        else
                        {
                            while (!_orsVerificaExistencias.EoF)
                            {
                                Decimal _ExistenciaArt = Decimal.Parse(((Double)_orsVerificaExistencias.Fields.Item("Quantity").Value).ToString());
                                if (_ExistenciaArt < Convert.ToDecimal(_Quantity))
                                {
                                    throw new Exception("==> No hay cantidad suficiente en el inventario de SAP. " + " Item: " + _ItemCode + " Lote: " + _BatchNumber + " Bodega: " + _WarehouseCode + " Requerido: " + _Quantity.ToString() + " Saldo: " + _ExistenciaArt.ToString());
                                }
                                _orsVerificaExistencias.MoveNext();
                            }
                        }

                        //===============================================
                        //'bacthes
                        oBatchNumbers = oInventory.Lines.BatchNumbers;
                        oBatchNumbers.BatchNumber = _BatchNumber;
                        oBatchNumbers.Quantity = _Quantity;
                        oBatchNumbers.Location = _WarehouseCode;
                        _Weight = _Weight * -1;
                        oBatchNumbers.UserFields.Fields.Item("U_Peso").Value = (_Weight).ToString();
                        oBatchNumbers.UserFields.Fields.Item("U_Pedido").Value = _OrderNumber.ToString();
                        //oBatchNumbers.Add();

                        if (_Conteo < _p_Detalle.Count)
                        {
                            oInventory.Lines.Add();
                        }
                        
                        var cantidad = oInventory.Lines.Count;
                        _Conteo += 1;

                    }

                    










                    // Verificación de Transacción para Evitar Transacción Duplicada Julio Herrera
                    // ------------------------------------------------------------
                    SAPbobsCOM.Recordset _orsCheckTran = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                    _orsCheckTran.DoQuery(string.Format("SELECT DocEntry FROM OIGE WHERE U_IdMovDet = '{0}'", _p_Detalle[0].MovimientoId.ToString()));




                    if (_orsCheckTran.RecordCount == 0)
                    {
                        lRetCode = oInventory.Add();
                        if (lRetCode != 0)
                        {
                            int lErrCode;
                            string sErrMsg;
                            oCompany.GetLastError(out lErrCode, out sErrMsg);
                            sErrMsg += "[" + _p_Detalle[0].Comments + "]";
                            throw new Exception(sErrMsg);
                        }

                        oCompany.GetNewObjectCode(out InvCodeStr);
                        _return = InvCodeStr;

                    }
                    else
                    {
                        while (!_orsCheckTran.EoF)
                        {
                            InvCodeStr = ((Int32)_orsCheckTran.Fields.Item("DocEntry").Value).ToString();
                            _orsCheckTran.MoveNext();
                        }
                    }

                    // Finalizacion de cAMBIOS




                    //------------------------------actualiza kardex
                    SAPbobsCOM.Recordset _orsRegistroGrabado = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);

                    String _queryRegistroGrabado = "SELECT T0.[DocDate], T0.[ObjType], T0.[DocNum], T1.[ItemCode], T1.[Dscription], T1.[WhsCode], T2.[ItmsGrpCod], T3.[ItmsGrpNam], T4.[WhsName], T5.[CalcPrice], T5.[Currency], T5.[OpenValue], T1.[Quantity] FROM [dbo].[OIGE]  T0 INNER JOIN [dbo].[IGE1]  T1 ON T0.[DocEntry] = T1.[DocEntry] INNER JOIN OITM T2 ON T1.[ItemCode] = T2.[ItemCode] INNER JOIN OITB T3 ON T2.[ItmsGrpCod] = T3.[ItmsGrpCod] INNER JOIN OWHS T4 ON T1.[WhsCode] = T4.[WhsCode] INNER JOIN OINM T5 ON T5.TransType = t0.ObjType AND T5.BASE_REF = T0.DocNum AND T5.DocLineNum = T1.LineNum WHERE T0.[DocEntry] = {0}";
                    _orsRegistroGrabado.DoQuery(string.Format(_queryRegistroGrabado, InvCodeStr));

                    if (_orsRegistroGrabado.RecordCount > 0)
                    {


                        while (!_orsRegistroGrabado.EoF)
                        {
                            String _ItemCode = "";
                            _DocDate = (DateTime)_orsRegistroGrabado.Fields.Item("DocDate").Value;
                            _DocCurrency = (String)_orsRegistroGrabado.Fields.Item("Currency").Value;
                            _DocType = Int32.Parse((String)_orsRegistroGrabado.Fields.Item("ObjType").Value);
                            _DocName = "Goods Issue";
                            _DocNum = (Int32)_orsRegistroGrabado.Fields.Item("DocNum").Value;
                            _ItmsGrpCod = (Int32)_orsRegistroGrabado.Fields.Item("ItmsGrpCod").Value;
                            _ItmsGrpNam = (String)_orsRegistroGrabado.Fields.Item("ItmsGrpNam").Value;
                            _WhsName = (String)_orsRegistroGrabado.Fields.Item("ItmsGrpNam").Value;
                            _ItemCode = (String)_orsRegistroGrabado.Fields.Item("ItemCode").Value;
                            Decimal _Quantity = Decimal.Parse( ((Double)_orsRegistroGrabado.Fields.Item("Quantity").Value).ToString());

                            _SapUnitPrice = Decimal.Parse(((Double)_orsRegistroGrabado.Fields.Item("CalcPrice").Value).ToString());
                            _SapPrice = Decimal.Parse(((Double)_orsRegistroGrabado.Fields.Item("OpenValue").Value).ToString());

                            List<Tbl_INV_Mov_Detail> _DetalleCor = (from l in _p_Detalle where l.ItemCode == _ItemCode && l.Qty == _Quantity select l).ToList();

                            if ( _DetalleCor.Count > 0)
                            {
                                DB_INVTableAdapters.Tbl_INV_Mov_DetailTableAdapter _dbinv = new DB_INVTableAdapters.Tbl_INV_Mov_DetailTableAdapter();
                                _dbinv.Update_Detail(Convert.ToInt32(InvCodeStr), _DetalleCor[0].ItemName, _ItmsGrpCod, _ItmsGrpNam, _DetalleCor[0].BatchNumber, _DetalleCor[0].WareHouse, _WhsName, _p_Detalle[0].Comments, _DocCurrency, _SapUnitPrice, _SapPrice, _AccountCode, _DocName, _DocType, _DocNum.ToString(), _DocDate, _ItemCode, _DetalleCor[0].MovimientoDetailId);

                            }

                            _orsRegistroGrabado.MoveNext();
                        }


                    }
                    else
                    {
                        GT.System_GT.f_error(new Exception("no se encontraron datos en sap."), "vw_OITL57 x", "");
                   
                        
                    }


                }
                else
                {
                    throw new Exception("Compania no conectada");
                }
            }
            catch (Exception ex)
            {
                //oCompany.Disconnect();
                throw (ex);
            }
            //oCompany.Disconnect();
            return _return;
        }


        [DataObjectMethodAttribute
     (DataObjectMethodType.Insert, true)]
        public string Insert_oInventoryGenExit_MPDoc( Tbl_INV_Mov_Header _Encabezado, List<Tbl_INV_Mov_Detail> _Detalle )
            
        {
            

            string _return = "";
            // Obtiene La Compañia
            SAPbobsCOM.Company oCompany = GlobalSAP.GetCompany((Int32)_Encabezado.IdEmpresa);
            // Obtiene La Sucursal
            int oSucursal = GlobalSAP.GetSucursal((Int32)_Encabezado.IdEmpresa);


            try
            {
                SAPbobsCOM.SBObob _oBridge = (SAPbobsCOM.SBObob)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoBridge);
                SAPbobsCOM.Documents oInventory = (SAPbobsCOM.Documents)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oInventoryGenExit);
              
               
                string _AccountCode = "";
                string _ItemDescription = "";                                    
                if (oCompany.Connected)
                {
                    String _Comments = _Detalle.Max(d => d.Comments);
                    //-- Datos Transacción
                    //-----------------------
                    oInventory.DocDate = DateTime.Now.Date;
                    oInventory.TaxDate = DateTime.Now.Date;
                    oInventory.Comments = _Comments;
                    oInventory.DocType = SAPbobsCOM.BoDocumentTypes.dDocument_Items;
                    oInventory.ManualNumber = "N";
                    oInventory.JournalMemo = GT.System_GT.Left(_Comments, 50);
                    oInventory.BPL_IDAssignedToInvoice = oSucursal;
                    oInventory.UserFields.Fields.Item("U_IdMovDet").Value = _Encabezado.MovimientoId.ToString();

                    foreach (var _ItemDetalle in _Detalle)
                    {
                        String _WarehouseCode = _ItemDetalle.WareHouse;
                        Int32 _OrderNumber = (int)_ItemDetalle.OrderNumber;
                        String _BatchNumber = _ItemDetalle.BatchNumber;
                        Decimal _Quantity = (Decimal)_ItemDetalle.Qty;
                        Decimal _Weight = (Decimal)_ItemDetalle.Weight;
                        Decimal _Price = (Decimal)_ItemDetalle.Price;

                        String _ItemCode = _ItemDetalle.InternalPartNbr;
                        GetSAPItemCodeInformation(oCompany, "", ref _ItemCode, ref _ItemDescription);

                        String _Account = _ItemDetalle.Account;
                        GetSAPAccountInformation(oCompany, _oBridge, _Account, ref _AccountCode);

                        oInventory.Lines.ItemCode = _ItemCode;
                        oInventory.Lines.AccountCode = _AccountCode;
                        oInventory.Lines.ItemDescription = _ItemDescription;
                        oInventory.Lines.WarehouseCode = _WarehouseCode;
                        oInventory.Lines.Price = Double.Parse(_Price.ToString());
                        oInventory.Lines.Quantity = Double.Parse(_Quantity.ToString());


                        // Revision de Existencia en SAP
                        VerifySapInventoryQuantity(oCompany, _WarehouseCode, _BatchNumber, _Quantity, _ItemCode,"=.=.>", "");

                        // Agregar Bach Numbers
                        AddSapBachNumberInfo(ref oInventory, _Quantity, _Weight, _OrderNumber, _WarehouseCode, _BatchNumber);

                    }

                    String _NumeroDocumentoSAP = "";
                    VerificaAplicaDocumentoSAP(_Encabezado, ref _return, oCompany, oInventory, _Comments, ref _NumeroDocumentoSAP);


                    bool _TransaccionEcontrada = oInventory.GetByKey(Int32.Parse( _NumeroDocumentoSAP));

                    var Lines = oInventory.Lines;
                    var Baches = Lines.BatchNumbers;

                    ////------------------------------actualiza kardex
                    //SAPbobsCOM.Recordset _orsRegistroGrabado = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);

                    //String _queryRegistroGrabado = "SELECT T0.[DocDate], T0.[ObjType], T0.[DocNum], T1.[ItemCode], T1.[Dscription], T1.[WhsCode], T2.[ItmsGrpCod], T3.[ItmsGrpNam], T4.[WhsName], T5.[CalcPrice], T5.[Currency], T5.[OpenValue], T1.[Quantity] FROM [dbo].[OIGE]  T0 INNER JOIN [dbo].[IGN1]  T1 ON T0.[DocEntry] = T1.[DocEntry] INNER JOIN OITM T2 ON T1.[ItemCode] = T2.[ItemCode] INNER JOIN OITB T3 ON T2.[ItmsGrpCod] = T3.[ItmsGrpCod] INNER JOIN OWHS T4 ON T1.[WhsCode] = T4.[WhsCode] INNER JOIN OINM T5 ON T5.TransType = t0.ObjType AND T5.BASE_REF = T0.DocNum AND T5.DocLineNum = T1.LineNum WHERE T0.[DocNum] = {0}";
                    //_orsRegistroGrabado.DoQuery(string.Format(_queryRegistroGrabado, _NumeroDocumentoSAP));

                    //if (_orsRegistroGrabado.RecordCount > 0)
                    //{
                    //    string _DocName = "Goods Issue";
                    //    int _DocType = 0;
                    //    int _DocNum = 0;
                    //    DateTime _DocDate;

                    //    while (!_orsRegistroGrabado.EoF)
                    //    {                                                 
                    //        _DocDate = (DateTime)_orsRegistroGrabado.Fields.Item("DocDate").Value;                         
                    //        _DocType = Int32.Parse((String)_orsRegistroGrabado.Fields.Item("ObjType").Value);                            
                    //        _DocNum = (Int32)_orsRegistroGrabado.Fields.Item("DocNum").Value;

                    //        string _DocCurrency = (String)_orsRegistroGrabado.Fields.Item("Currency").Value;
                    //        int _ItmsGrpCod = (Int32)_orsRegistroGrabado.Fields.Item("ItmsGrpCod").Value;
                    //        string _ItmsGrpNam = (String)_orsRegistroGrabado.Fields.Item("ItmsGrpNam").Value;
                    //        string _WhsName = (String)_orsRegistroGrabado.Fields.Item("WhsName").Value;
                    //        string _ItemCode = (String)_orsRegistroGrabado.Fields.Item("ItemCode").Value;
                    //        decimal _SapUnitPrice = Decimal.Parse(((Double)_orsRegistroGrabado.Fields.Item("CalcPrice").Value).ToString());
                    //        decimal _SapPrice = Decimal.Parse(((Double)_orsRegistroGrabado.Fields.Item("OpenValue").Value).ToString());

                    //        DB_INVTableAdapters.Tbl_INV_Mov_DetailTableAdapter _dbinv = new DB_INVTableAdapters.Tbl_INV_Mov_DetailTableAdapter();

                    //        _dbinv.Update_Detail(Convert.ToInt32( _NumeroDocumentoSAP), _ItemDescription, _ItmsGrpCod, _ItmsGrpNam, _BatchNumber, 
                    //                                              _WarehouseCode, _WhsName, _Comments, _DocCurrency, _SapUnitPrice, _SapPrice, 
                    //                                              _Account, _DocName, _DocType, _DocNum.ToString(), _DocDate, _ItemCode, 
                    //                                              Original_MovimientoDetailId);

                    //        _orsRegistroGrabado.MoveNext();
                    //    }

                        
                        

                    //}
                    //else
                    //{
                    //    GT.System_GT.f_error(new Exception("no se encontraron datos en sap."), "vw_OITL", "");
                    //}


                }
                else
                {
                    throw new Exception("Compania no conectada");
                }
            }
            catch (Exception ex)
            {
                oCompany.Disconnect();
                throw (ex);
            }
            oCompany.Disconnect();
            return _return;
        }

        private static void VerificaAplicaDocumentoSAP(Tbl_INV_Mov_Header _Encabezado, ref string _return, SAPbobsCOM.Company oCompany, SAPbobsCOM.Documents oInventory, string _Comments, ref string _NumeroDocumentoSAP)
        {
            int lRetCode;            
            // Verificación de Transacción para Evitar Transacción Duplicada Julio Herrera
            // ------------------------------------------------------------
            SAPbobsCOM.Recordset _orsCheckTran = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
            _orsCheckTran.DoQuery(string.Format("SELECT DocEntry FROM OIGE WHERE U_IdMovDet = '{0}'", _Encabezado.MovimientoId.ToString()));

            if (_orsCheckTran.RecordCount == 0)
            {
                lRetCode = oInventory.Add();
                if (lRetCode != 0)
                {
                    int lErrCode;
                    string sErrMsg;
                    oCompany.GetLastError(out lErrCode, out sErrMsg);
                    sErrMsg += "[" + _Comments + "]";
                    throw new Exception(sErrMsg);
                }

                oCompany.GetNewObjectCode(out _NumeroDocumentoSAP);
                _return = _NumeroDocumentoSAP;

            }
            else
            {
                while (!_orsCheckTran.EoF)
                {
                    _NumeroDocumentoSAP = ((Int32)_orsCheckTran.Fields.Item("DocEntry").Value).ToString();
                    _orsCheckTran.MoveNext();
                }
            }

            // Finalizacion de cAMBIOS            
        }

        private static void VerifySapInventoryQuantity(SAPbobsCOM.Company oCompany, string _WarehouseCode, string _BatchNumber, decimal _Quantity, string _ItemCode, String _Prefijo, String _Shipping)
        {
            //Verifica existencias. 

            SAPbobsCOM.Recordset _orsVerificaExistencias = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
            _orsVerificaExistencias.DoQuery(String.Format("select I.ItemCode,BatchNum,Quantity,WhsCode,i.InDate,p.LstEvlPric,p.U_CodigoToriflex from OIBT AS I inner join OITM as P on I.ItemCode = P.ItemCode  inner join OITB as G on P.ItmsGrpCod = G.ItmsGrpCod  where  Quantity > 0 AND I.ItemCode = '{0}' AND I.BatchNum = '{1}' AND I.WhsCode = '{2}'"
                                       , _ItemCode, _BatchNumber, _WarehouseCode));


            if (_orsVerificaExistencias.RecordCount == 0)
            {
                throw new Exception( _Prefijo + " No hay cantidad suficiente en el inventario de SAP. " + " Item: " + _ItemCode + " Lote: " + _BatchNumber + " Bodega: " + _WarehouseCode + " Requerido: " + _Quantity.ToString() + " Saldo: 0");
            }
            else
            {
                while (!_orsVerificaExistencias.EoF)
                {
                    Decimal _ExistenciaArt = Decimal.Parse(((Double)_orsVerificaExistencias.Fields.Item("Quantity").Value).ToString());
                    if (_ExistenciaArt < Convert.ToDecimal(_Quantity))
                    {
                        if (_Shipping != "") _Shipping = " Shipping: " + _Shipping + " ";
                        throw new Exception(_Prefijo + " No hay cantidad suficiente en el inventario de SAP. " + " Item: " + _ItemCode + " Lote: " + _BatchNumber + " Bodega: " + _WarehouseCode + " Requerido: " + _Quantity.ToString() + " Saldo: " + _ExistenciaArt.ToString());
                    }
                    _orsVerificaExistencias.MoveNext();
                }
            }
        }

        private static void GetSAPItemCodeInformation(SAPbobsCOM.Company oCompany, string _InternalPartNbr, ref string _ItemCode, ref string _ItemDescription)
        {

            SAPbobsCOM.Items _oItems = (SAPbobsCOM.Items)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oItems);

            var _ItemCodeBuscar = (_InternalPartNbr != "") ? _InternalPartNbr.Substring(0, 8) : _ItemCode;
            var _oItemsEncontrado = _oItems.GetByKey(_ItemCodeBuscar);

            if (_oItemsEncontrado == true)
            {
                _ItemCode = _oItems.ItemCode;
                _ItemDescription = _oItems.ItemName;
            }
            else
            {

                if (_InternalPartNbr != "")
                {
                    throw new Exception("No existe el item code." + _InternalPartNbr.Substring(0, 8));
                }
                else
                {
                    throw new Exception("No existe el item code." + _ItemCode);
                }

            }
            
        }

        private static void GetSAPAccountInformation(SAPbobsCOM.Company oCompany, SAPbobsCOM.SBObob _oBridge, string _Account, ref string _AccountCode)
        {
            SAPbobsCOM.Recordset _orsAccount = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
            _orsAccount = _oBridge.GetObjectKeyBySingleValue(SAPbobsCOM.BoObjectTypes.oChartOfAccounts, "FormatCode", _Account, SAPbobsCOM.BoQueryConditions.bqc_Equal);

            if (_orsAccount.RecordCount != 0)
            {
                while (!_orsAccount.EoF)
                {
                    _AccountCode = (String)_orsAccount.Fields.Item("AcctCode").Value;
                    _orsAccount.MoveNext();
                }
            }
            else
            {
                throw new Exception("No existe la cuenta contable." + _Account);
            }
        }
        
        private static void AddSapBachNumberInfo(ref SAPbobsCOM.Documents oInventory,Decimal  _Quantity, Decimal _Weight, Int32 _OrderNumber, String _WarehouseCode, String _BatchNumber)
        {
            SAPbobsCOM.BatchNumbers oBatchNumbers;

            oBatchNumbers = oInventory.Lines.BatchNumbers;
            oBatchNumbers.BatchNumber = _BatchNumber;
            oBatchNumbers.Quantity = double.Parse( _Quantity.ToString());
            oBatchNumbers.Location = _WarehouseCode;
            _Weight = _Weight * -1;
            oBatchNumbers.UserFields.Fields.Item("U_Peso").Value = (_Weight).ToString();
            oBatchNumbers.UserFields.Fields.Item("U_Pedido").Value = _OrderNumber.ToString();
            oBatchNumbers.Add();
        }

        //Entradas de Producto Terminado Por Ajuste Materia Prima **
        [DataObjectMethodAttribute
        (DataObjectMethodType.Insert, true)]
        public string Insert_oInventoryGenEntry_MP(int _CompanyNumber, string _InternalPartNbr, double _Price, double _Quantity, string _WarehouseCode, 
                                                    string _BatchNumber, decimal _Weight, string _Comments, string _ItemCode, string _Account, int _OrderNumber, 
                                                    Guid Original_MovimientoDetailId)
        {

            string _return = "";
            SAPbobsCOM.Company oCompany;
            
            oCompany = GlobalSAP.GetCompany(_CompanyNumber);
       
            int _Sucursal = 0;

            _Sucursal = GlobalSAP.GetSucursal(_CompanyNumber);

            try
            {
                SAPbobsCOM.Documents oInventory = (SAPbobsCOM.Documents)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oInventoryGenEntry); 
                SAPbobsCOM.BatchNumbers oBatchNumbers;

                SAPbobsCOM.SBObob _oBridge = (SAPbobsCOM.SBObob)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoBridge);


                int lRetCode;
                string InvCodeStr = "";


                string _ItmsGrpNam = "";
                int _ItmsGrpCod = 0;
                DateTime _DocDate = DateTime.Now;
                string _DocCurrency = "";
                int _DocType = 0;
                int _DocNum = 0;
                string _DocName = "";
                string _WhsName = "";
                decimal _SapUnitPrice = 0;
                decimal _SapPrice = 0;

                // Seccion InternalPart
                string _ItemDescription = "";
                // Seccion Account
                string _AccountCode = "";

                if (oCompany.Connected)
                {

                    // InternalPart
                    //-------------------

                    SAPbobsCOM.Items _oItems = (SAPbobsCOM.Items)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oItems);

                    var _oItemsEncontrado = false;
                    var _ItemCodeBuscar = (_InternalPartNbr != "") ? _InternalPartNbr.Substring(0, 8) : _ItemCode;                   
                    _oItemsEncontrado = _oItems.GetByKey( _ItemCodeBuscar);
                   
                    if (_oItemsEncontrado == true)
                    {
                        _ItemCode = _oItems.ItemCode;
                        _ItemDescription = _oItems.ItemName;
                    }
                    else
                    {
                        throw new Exception("No existe el item code." + _ItemCode);
                    }


                    //-- Account
                    //----------------------

                    
                    
                    SAPbobsCOM.Recordset _orsAccount = (SAPbobsCOM.Recordset) oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);

                    //var l = new SAPbobsCOM.IAccountSegmentations();
                   

                    //_orsAccount.DoQuery(string.Format("SELECT AcctCode FROM OACT WHERE FormatCode = '{0}'", _Account));

                    _orsAccount = _oBridge.GetObjectKeyBySingleValue(SAPbobsCOM.BoObjectTypes.oChartOfAccounts, "FormatCode", _Account, SAPbobsCOM.BoQueryConditions.bqc_Equal);

                    if (_orsAccount.RecordCount != 0)
                    {
                        while (!_orsAccount.EoF)
                        {
                            _AccountCode = (String)_orsAccount.Fields.Item("AcctCode").Value;
                            _orsAccount.MoveNext();
                        }
                    } else
                    {
                        throw new Exception("No existe la cuenta contable." + _ItemCode);
                    }
                    
                    
             

                    //-- Datos Transacción
                    //-----------------------
                    oInventory.DocDate = DateTime.Now.Date;
                    oInventory.TaxDate = DateTime.Now.Date;
                    oInventory.Comments = _Comments;
                    oInventory.DocType = SAPbobsCOM.BoDocumentTypes.dDocument_Items;
                    oInventory.ManualNumber = "N";
                    oInventory.JournalMemo = GT.System_GT.Left(_Comments, 50);
                    oInventory.BPL_IDAssignedToInvoice = _Sucursal;
                    
                    oInventory.Lines.ItemCode = _ItemCode;
                    oInventory.Lines.AccountCode = _AccountCode;
                    oInventory.Lines.ItemDescription = _ItemDescription;
                    oInventory.Lines.WarehouseCode = _WarehouseCode;
                    oInventory.Lines.Price = _Price;
                    oInventory.Lines.Quantity = _Quantity;

                    oBatchNumbers = oInventory.Lines.BatchNumbers;
                    oBatchNumbers.BatchNumber = _BatchNumber;
                    oBatchNumbers.Quantity = _Quantity;
                    oBatchNumbers.Location = _WarehouseCode;
                    oBatchNumbers.UserFields.Fields.Item("U_Peso").Value = (_Weight).ToString();
                    oBatchNumbers.UserFields.Fields.Item("U_Pedido").Value = _OrderNumber.ToString();                    
                    oBatchNumbers.Add();

                    oInventory.UserFields.Fields.Item("U_IdMovDet").Value = Original_MovimientoDetailId.ToString();

                    // Verificación de Transacción para Evitar Transacción Duplicada Julio Herrera
                    // ------------------------------------------------------------
                    SAPbobsCOM.Recordset _orsCheckTran = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);


                    _orsCheckTran.DoQuery(string.Format("SELECT DocEntry FROM OIGN WHERE U_IdMovDet = '{0}'", Original_MovimientoDetailId.ToString()));



                    
                    

                    if (_orsCheckTran.RecordCount == 0)
                    { 
                        lRetCode = oInventory.Add();
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
                        while (!_orsCheckTran.EoF)
                        {
                            InvCodeStr = ((Int32)_orsCheckTran.Fields.Item("DocEntry").Value).ToString();
                            _orsAccount.MoveNext();
                        }                        
                    }

                    // Finalizacion de cAMBIOS



                    

                    //------------------------------actualiza kardex



                    SAPbobsCOM.Recordset _orsRegistroGrabado = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);

                    String _queryRegistroGrabado = "SELECT T0.[DocDate], T0.[ObjType], T0.[DocNum], T1.[ItemCode], T1.[Dscription], T1.[WhsCode], T2.[ItmsGrpCod], T3.[ItmsGrpNam], T4.[WhsName], T5.[CalcPrice], T5.[Currency], T5.[OpenValue], T1.[Quantity] FROM [dbo].[OIGN]  T0 INNER JOIN [dbo].[IGN1]  T1 ON T0.[DocEntry] = T1.[DocEntry] INNER JOIN OITM T2 ON T1.[ItemCode] = T2.[ItemCode] INNER JOIN OITB T3 ON T2.[ItmsGrpCod] = T3.[ItmsGrpCod] INNER JOIN OWHS T4 ON T1.[WhsCode] = T4.[WhsCode] INNER JOIN OINM T5 ON T5.TransType = t0.ObjType AND T5.BASE_REF = T0.DocNum AND T5.DocLineNum = T1.LineNum WHERE T0.[DocEntry] = {0}";
                    _orsRegistroGrabado.DoQuery(string.Format(_queryRegistroGrabado  , InvCodeStr)  );

                    if (_orsRegistroGrabado.RecordCount == 0)
                    {
                        lRetCode = oInventory.Add();
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
                    } else
                    {
                        while (!_orsRegistroGrabado.EoF)
                        {
                            
                            _DocDate = (DateTime)_orsRegistroGrabado.Fields.Item("DocDate").Value; 
                            _DocCurrency = (String)_orsRegistroGrabado.Fields.Item("Currency").Value;
                            _DocType = Int32.Parse( (String)_orsRegistroGrabado.Fields.Item("ObjType").Value);
                            _DocName = "Goods Receipt";
                            _DocNum = (Int32)_orsRegistroGrabado.Fields.Item("DocNum").Value; 
                            _ItmsGrpCod = (Int32)_orsRegistroGrabado.Fields.Item("ItmsGrpCod").Value;
                            _ItmsGrpNam = (String)_orsRegistroGrabado.Fields.Item("ItmsGrpNam").Value; 
                            _WhsName = (String)_orsRegistroGrabado.Fields.Item("ItmsGrpNam").Value; 
                            _ItemCode = (String)_orsRegistroGrabado.Fields.Item("ItemCode").Value; 

                            _SapUnitPrice = Decimal.Parse(  ((Double)_orsRegistroGrabado.Fields.Item("CalcPrice").Value).ToString());
                            _SapPrice =Decimal.Parse( ( (Double)_orsRegistroGrabado.Fields.Item("OpenValue").Value).ToString());


                            _orsRegistroGrabado.MoveNext();
                        }

                        DB_INVTableAdapters.Tbl_INV_Mov_DetailTableAdapter _dbinv = new DB_INVTableAdapters.Tbl_INV_Mov_DetailTableAdapter();

                        _dbinv.Update_Detail(Convert.ToInt32(InvCodeStr), _ItemDescription, _ItmsGrpCod, _ItmsGrpNam, _BatchNumber, _WarehouseCode, _WhsName, _Comments, _DocCurrency, _SapUnitPrice, _SapPrice, _Account, _DocName, _DocType, _DocNum.ToString(), _DocDate, _ItemCode, Original_MovimientoDetailId);
                    }
                                   


                }
                else
                {
                    throw new Exception("Compania no conectada");
                }
            }
            catch (Exception ex)
            {
                oCompany.Disconnect();
                throw (ex);               
            }
            oCompany.Disconnect();
            return _return;
        }

        [DataObjectMethodAttribute
        (DataObjectMethodType.Insert, true)]
        public string Insert_oInventoryGenEntry_MP_PorDocto(int _CompanyNumber, List<Tbl_INV_Mov_Detail> _p_Detalle)
      
        {
            string _return = "";
            SAPbobsCOM.Company oCompany = GlobalSAP.GetCompany(_CompanyNumber);
            int _Sucursal =  GlobalSAP.GetSucursal(_CompanyNumber);

            try
            {
                SAPbobsCOM.Documents oInventory = (SAPbobsCOM.Documents)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oInventoryGenEntry);                
                SAPbobsCOM.SBObob _oBridge = (SAPbobsCOM.SBObob)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoBridge);


                int lRetCode;
                string InvCodeStr = "";


                string _ItmsGrpNam = "";
                int _ItmsGrpCod = 0;
                DateTime _DocDate = DateTime.Now;
                string _DocCurrency = "";
                int _DocType = 0;
                int _DocNum = 0;
                string _DocName = "";
                string _WhsName = "";
                decimal _SapUnitPrice = 0;
                decimal _SapPrice = 0;

                // Seccion InternalPart
                string _ItemDescription = "";
                // Seccion Account
                string _AccountCode = "";

                if (oCompany.Connected)
                {

                    


                    //-- Account
                    //----------------------



                    SAPbobsCOM.Recordset _orsAccount = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);               
                    _orsAccount = _oBridge.GetObjectKeyBySingleValue(SAPbobsCOM.BoObjectTypes.oChartOfAccounts, "FormatCode", _p_Detalle[0].Account, SAPbobsCOM.BoQueryConditions.bqc_Equal);

                    if (_orsAccount.RecordCount != 0)
                    {
                        while (!_orsAccount.EoF)
                        {
                            _AccountCode = (String)_orsAccount.Fields.Item("AcctCode").Value;
                            _orsAccount.MoveNext();
                        }
                    }
                    else
                    {
                        throw new Exception("No existe la cuenta contable." + _p_Detalle[0].Account);
                    }




                    //-- Datos Transacción
                    //-----------------------
                    oInventory.DocDate = DateTime.Now.Date;
                    oInventory.TaxDate = DateTime.Now.Date;
                    oInventory.Comments = _p_Detalle[0].Comments;
                    oInventory.DocType = SAPbobsCOM.BoDocumentTypes.dDocument_Items;
                    oInventory.ManualNumber = "N";
                    oInventory.JournalMemo = GT.System_GT.Left(_p_Detalle[0].Comments, 50);
                    oInventory.BPL_IDAssignedToInvoice = _Sucursal;

                    var _Conteo = 1;
                    foreach (var _Detalle_Linea in _p_Detalle)
                    {
                        SAPbobsCOM.BatchNumbers oBatchNumbers;

                        string _InternalPartNbr = "";
                        double _Price = Double.Parse(_Detalle_Linea.Price.ToString());
                        double _Quantity = Double.Parse(_Detalle_Linea.Qty.ToString());
                        string _WarehouseCode = _Detalle_Linea.WareHouse;
                        string _BatchNumber = _Detalle_Linea.BatchNumber;
                        decimal _Weight = (decimal)_Detalle_Linea.Weight;
                        string _ItemCode = _Detalle_Linea.InternalPartNbr;
                        string _Account = _Detalle_Linea.Account;
                        int _OrderNumber = 0;
                        try { _OrderNumber = (int)_Detalle_Linea.OrderNumber; } catch { }
                            

                        // InternalPart
                        //-------------------

                        SAPbobsCOM.Items _oItems = (SAPbobsCOM.Items)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oItems);

                        var _oItemsEncontrado = false;
                        var _ItemCodeBuscar = (_InternalPartNbr != "") ? _InternalPartNbr.Substring(0, 8) : _ItemCode;
                        _oItemsEncontrado = _oItems.GetByKey(_ItemCodeBuscar);

                        if (_oItemsEncontrado == true)
                        {
                            _ItemCode = _oItems.ItemCode;
                            _ItemDescription = _oItems.ItemName;
                        }
                        else
                        {
                            throw new Exception("No existe el item code." + _ItemCode);
                        }

                        oInventory.Lines.ItemCode = _ItemCode;
                        oInventory.Lines.AccountCode = _AccountCode;
                        oInventory.Lines.ItemDescription = _ItemDescription;
                        oInventory.Lines.WarehouseCode = _WarehouseCode;
                        oInventory.Lines.Price = _Price;
                        oInventory.Lines.Quantity = _Quantity;
                        oInventory.UserFields.Fields.Item("U_IdMovDet").Value = _p_Detalle[0].MovimientoId.ToString();


                        oBatchNumbers = oInventory.Lines.BatchNumbers;
                        oBatchNumbers.BatchNumber = _BatchNumber;
                        oBatchNumbers.Quantity = _Quantity;
                        oBatchNumbers.Location = _WarehouseCode;
                        oBatchNumbers.UserFields.Fields.Item("U_Peso").Value = (_Weight).ToString();
                        oBatchNumbers.UserFields.Fields.Item("U_Pedido").Value = _OrderNumber.ToString();
                        //oBatchNumbers.Add();

                        if (_Conteo < _p_Detalle.Count)
                        {
                            oInventory.Lines.Add();
                        }

                        var cantidad = oInventory.Lines.Count;
                        _Conteo += 1;
                    }

                    
                    // Verificación de Transacción para Evitar Transacción Duplicada Julio Herrera
                    // ------------------------------------------------------------
                    SAPbobsCOM.Recordset _orsCheckTran = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                    _orsCheckTran.DoQuery(string.Format("SELECT DocEntry FROM OIGN WHERE U_IdMovDet = '{0}'", _p_Detalle[0].MovimientoId.ToString()));
                    
                    if (_orsCheckTran.RecordCount == 0)
                    {
                        lRetCode = oInventory.Add();
                        if (lRetCode != 0)
                        {
                            int lErrCode;
                            string sErrMsg;
                            oCompany.GetLastError(out lErrCode, out sErrMsg);
                            sErrMsg += "[" + _p_Detalle[0].Comments + "]";
                            throw new Exception(sErrMsg);
                        }

                        oCompany.GetNewObjectCode(out InvCodeStr);
                        _return = InvCodeStr;

                    }
                    else
                    {
                        while (!_orsCheckTran.EoF)
                        {
                            InvCodeStr = ((Int32)_orsCheckTran.Fields.Item("DocEntry").Value).ToString();
                            _orsCheckTran.MoveNext();
                        }
                    }

                    // Finalizacion de cAMBIOS

                    //------------------------------actualiza kardex

                    

                    SAPbobsCOM.Recordset _orsRegistroGrabado = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);

                    String _queryRegistroGrabado = "SELECT T0.[DocDate], T0.[ObjType], T0.[DocNum], T1.[ItemCode], T1.[Dscription], T1.[WhsCode], T2.[ItmsGrpCod], T3.[ItmsGrpNam], T4.[WhsName], T5.[CalcPrice], T5.[Currency], T5.[OpenValue], T1.[Quantity] FROM [dbo].[OIGN]  T0 INNER JOIN [dbo].[IGN1]  T1 ON T0.[DocEntry] = T1.[DocEntry] INNER JOIN OITM T2 ON T1.[ItemCode] = T2.[ItemCode] INNER JOIN OITB T3 ON T2.[ItmsGrpCod] = T3.[ItmsGrpCod] INNER JOIN OWHS T4 ON T1.[WhsCode] = T4.[WhsCode] INNER JOIN OINM T5 ON T5.TransType = t0.ObjType AND T5.BASE_REF = T0.DocNum AND T5.DocLineNum = T1.LineNum WHERE T0.[DocEntry] = {0}";
                    _orsRegistroGrabado.DoQuery(string.Format(_queryRegistroGrabado, InvCodeStr));

                    if (_orsRegistroGrabado.RecordCount == 0)
                    {
                        lRetCode = oInventory.Add();
                        if (lRetCode != 0)
                        {
                            int lErrCode;
                            string sErrMsg;
                            oCompany.GetLastError(out lErrCode, out sErrMsg);
                            sErrMsg += "[" + _p_Detalle[0].Comments + "]";
                            throw new Exception(sErrMsg);
                        }

                        oCompany.GetNewObjectCode(out InvCodeStr);
                        _return = InvCodeStr;
                    }
                    else
                    {
                        while (!_orsRegistroGrabado.EoF)
                        {
                            String _ItemCode = "";
                            _DocDate = (DateTime)_orsRegistroGrabado.Fields.Item("DocDate").Value;
                            _DocCurrency = (String)_orsRegistroGrabado.Fields.Item("Currency").Value;
                            _DocType = Int32.Parse((String)_orsRegistroGrabado.Fields.Item("ObjType").Value);
                            _DocName = "Goods Receipt";
                            _DocNum = (Int32)_orsRegistroGrabado.Fields.Item("DocNum").Value;
                            _ItmsGrpCod = (Int32)_orsRegistroGrabado.Fields.Item("ItmsGrpCod").Value;
                            _ItmsGrpNam = (String)_orsRegistroGrabado.Fields.Item("ItmsGrpNam").Value;
                            _WhsName = (String)_orsRegistroGrabado.Fields.Item("ItmsGrpNam").Value;
                            _ItemCode = (String)_orsRegistroGrabado.Fields.Item("ItemCode").Value;
                            Decimal _Quantity = Decimal.Parse(((Double)_orsRegistroGrabado.Fields.Item("Quantity").Value).ToString());

                            _SapUnitPrice = Decimal.Parse(((Double)_orsRegistroGrabado.Fields.Item("CalcPrice").Value).ToString());
                            _SapPrice = Decimal.Parse(((Double)_orsRegistroGrabado.Fields.Item("OpenValue").Value).ToString());

                            List<Tbl_INV_Mov_Detail> _DetalleCor = (from l in _p_Detalle where l.ItemCode == _ItemCode && l.Qty == _Quantity select l).ToList();

                            if (_DetalleCor.Count > 0)
                            {
                                DB_INVTableAdapters.Tbl_INV_Mov_DetailTableAdapter _dbinv = new DB_INVTableAdapters.Tbl_INV_Mov_DetailTableAdapter();
                                _dbinv.Update_Detail(Convert.ToInt32(InvCodeStr), _ItemDescription, _ItmsGrpCod, _ItmsGrpNam, _DetalleCor[0].BatchNumber, _DetalleCor[0].WareHouse, _WhsName, _p_Detalle[0].Comments, _DocCurrency, _SapUnitPrice, _SapPrice, _AccountCode, _DocName, _DocType, _DocNum.ToString(), _DocDate, _ItemCode, _DetalleCor[0].MovimientoDetailId);
                            }

                            _orsRegistroGrabado.MoveNext();
                        }
                        
                    }



                }
                else
                {
                    throw new Exception("Compania no conectada");
                }
            }
            catch (Exception ex)
            {
                //oCompany.Disconnect();
                throw (ex);
            }
            //oCompany.Disconnect();
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


