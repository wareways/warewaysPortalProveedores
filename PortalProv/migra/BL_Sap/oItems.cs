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

    public partial class oItemsTableAdapter
    {

        [DataObjectMethodAttribute
        (DataObjectMethodType.Insert, true)]
        public string Add_Items(int _oCompanyNumber, string _ItemCode, string _ItemName, string _ItmsGrpNam, string _InventoryItem, string _SalesItem, string _PurchaseItem, string _ManageBatchNumbers, string _ProductStyle, string _InternalPartNbr, string _InventoryUOM)
        {

            SAPbobsCOM.Company oCompany;
            oCompany = GlobalSAP.GetCompany(_oCompanyNumber);

            try
            {
                SAPbobsCOM.Items oItems;
                int lRetCode;
                bool isGetByKey = false; 

                if (oCompany.Connected)
                {
                    oItems = (SAPbobsCOM.Items)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oItems);

                    isGetByKey = oItems.GetByKey(_ItemCode);


                    oItems.ItemCode = _ItemCode;
                    oItems.UserFields.Fields.Item("U_CodigoToriflex").Value = _InternalPartNbr;
                    oItems.ItemName = _ItemName;
                    oItems.ForeignName = _ItemName;
                    oItems.ItemType = SAPbobsCOM.ItemTypeEnum.itItems;

                    // obtiene ItemsGroupCode
                    DbSapTableAdapters.OITBTableAdapter _DbOITB = new OITBTableAdapter();
                    _DbOITB.Connection.ConnectionString = GT.System_GT.Get_DbTori(_oCompanyNumber);
                    DbSap.OITBDataTable _tblOITB = new DbSap.OITBDataTable();

                    _tblOITB = _DbOITB.GetDataBy_ItmsGrpNam(_ItmsGrpNam);

                    if (_tblOITB.Rows.Count > 0)
                    {
                        oItems.ItemsGroupCode = _tblOITB[0].ItmsGrpCod;
                    }

                    if (_InventoryItem == "tYES")
                    {
                        oItems.InventoryItem = SAPbobsCOM.BoYesNoEnum.tYES;
                    }
                    else
                    {
                        oItems.InventoryItem = SAPbobsCOM.BoYesNoEnum.tNO;
                    }

                    if (_SalesItem == "tYES")
                    {
                        oItems.SalesItem = SAPbobsCOM.BoYesNoEnum.tYES;
                    }
                    else
                    {
                        oItems.SalesItem = SAPbobsCOM.BoYesNoEnum.tNO;
                    }

                    if (_PurchaseItem == "tYES")
                    {
                        oItems.PurchaseItem = SAPbobsCOM.BoYesNoEnum.tYES;
                    }
                    else
                    {
                        oItems.PurchaseItem = SAPbobsCOM.BoYesNoEnum.tNO;
                    }
                    if (_ManageBatchNumbers == "tYES")
                    {
                        oItems.ManageBatchNumbers = SAPbobsCOM.BoYesNoEnum.tYES;
                    }
                    else
                    {
                        oItems.ManageBatchNumbers = SAPbobsCOM.BoYesNoEnum.tNO;
                    }

                         
                  


                    oItems.UserFields.Fields.Item("U_TipoA2").Value = "BB";

                    if (_ItemCode != "0122-089" && _ItemCode != "0122-088" && _ItemCode != "0122-119")
                    {
                        oItems.InventoryUOM = _InventoryUOM;
                        oItems.SalesUnit = _InventoryUOM;
                    }
                    
                    //-- product style 
                    DbSapTableAdapters.PRODUCTSTYLETableAdapter _dbProductStyle = new PRODUCTSTYLETableAdapter();
                    _dbProductStyle.Connection.ConnectionString = GT.System_GT.Get_DbTori(_oCompanyNumber);
                    DbSap.PRODUCTSTYLEDataTable _tblProductStyle = new DbSap.PRODUCTSTYLEDataTable();

                    _tblProductStyle = _dbProductStyle.GetDataBy_Name(_ProductStyle);

                    if (_tblProductStyle.Rows.Count > 0)
                    {
                        oItems.UserFields.Fields.Item("U_ProducStyle").Value = _tblProductStyle[0].Code;
                    }

                    if (isGetByKey)
                    {
                        lRetCode = oItems.Update();
                    }
                    else
                    {
                        lRetCode = oItems.Add();
                    }

                    if (lRetCode != 0)
                    {
                        int lErrCode;
                        string sErrMsg;
                        oCompany.GetLastError(out lErrCode, out sErrMsg);
                        throw new Exception(sErrMsg);
                    }

                    isGetByKey = oItems.GetByKey(_ItemCode);

                    Boolean _Poly31 = false;
                    Boolean _Polint31 = false;
                    // Validar que Bodegas Existen
                    for (int i = 0; i < oItems.WhsInfo.Count ; i++)
                    {

                        oItems.WhsInfo.SetCurrentLine(i);
                        if (oItems.WhsInfo.WarehouseCode == "POLY31") _Poly31 = true;
                        if (oItems.WhsInfo.WarehouseCode == "POLINT31") _Polint31 = true;                        
                    }
                    
                    if (! _Poly31) // Agregar Bodega Porque no esta
                    {
                        if (oItems.WhsInfo.WarehouseCode != "") oItems.WhsInfo.Add();

                        oItems.WhsInfo.WarehouseCode = "POLY31";
                        lRetCode = oItems.Update();
                        if (lRetCode != 0)
                        {
                            int lErrCode;
                            string sErrMsg;
                            oCompany.GetLastError(out lErrCode, out sErrMsg);
                            throw new Exception(sErrMsg);
                        }

                    }


                    isGetByKey = oItems.GetByKey(_ItemCode);

                    if (!_Polint31) // Agregar Bodega Porque no esta
                    {
                        if (oItems.WhsInfo.WarehouseCode != "") oItems.WhsInfo.Add();

                        oItems.WhsInfo.WarehouseCode = "POLINT31";
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
                else
                {
                    throw new Exception("Compania no conectada");
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }

            return _ItemCode; 
        }


        [DataObjectMethodAttribute
        (DataObjectMethodType.Insert, true)]
        public string Add_ItemsNuevo(int _oCompanyNumber, string _ItemCode, string _ItemName, string _ItmsGrpNam, string _InventoryItem, string _SalesItem, string _PurchaseItem, string _ManageBatchNumbers, string _ProductStyle, string _InternalPartNbr, string _InventoryUOM, String _CustomerPartNbr, String _PartidaArancelaria)
        {
            if (_InventoryUOM == "Fardo") _InventoryUOM = "FARDOS";

            SAPbobsCOM.Company oCompany;
            oCompany = GlobalSAP.GetCompany(_oCompanyNumber);

            try
            {
                SAPbobsCOM.Items oItems;
                int lRetCode;
                bool isGetByKey = false;

                if (oCompany.Connected)
                {
                    oItems = (SAPbobsCOM.Items)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oItems);

                    isGetByKey = oItems.GetByKey(_ItemCode);


                    oItems.ItemCode = _ItemCode;
                    oItems.UserFields.Fields.Item("U_CodigoToriflex").Value = _InternalPartNbr;
                    oItems.UserFields.Fields.Item("U_CodigoCliente").Value = _CustomerPartNbr;
                    oItems.UserFields.Fields.Item("U_DescCliente").Value = _PartidaArancelaria;
                    oItems.ItemName = _ItemName;
                    oItems.ForeignName = _ItemName;
                    oItems.ItemType = SAPbobsCOM.ItemTypeEnum.itItems;

                    // obtiene ItemsGroupCode
                    DbSapTableAdapters.OITBTableAdapter _DbOITB = new OITBTableAdapter();
                    _DbOITB.Connection.ConnectionString = GT.System_GT.Get_DbTori(_oCompanyNumber);
                    DbSap.OITBDataTable _tblOITB = new DbSap.OITBDataTable();

                    _tblOITB = _DbOITB.GetDataBy_ItmsGrpNam(_ItmsGrpNam);

                    if (_tblOITB.Rows.Count > 0)
                    {
                        oItems.ItemsGroupCode = _tblOITB[0].ItmsGrpCod;
                    }

                    if (_InventoryItem == "tYES")
                    {
                        oItems.InventoryItem = SAPbobsCOM.BoYesNoEnum.tYES;
                    }
                    else
                    {
                        oItems.InventoryItem = SAPbobsCOM.BoYesNoEnum.tNO;
                    }

                    if (_SalesItem == "tYES")
                    {
                        oItems.SalesItem = SAPbobsCOM.BoYesNoEnum.tYES;
                    }
                    else
                    {
                        oItems.SalesItem = SAPbobsCOM.BoYesNoEnum.tNO;
                    }

                    if (_PurchaseItem == "tYES")
                    {
                        oItems.PurchaseItem = SAPbobsCOM.BoYesNoEnum.tYES;
                    }
                    else
                    {
                        oItems.PurchaseItem = SAPbobsCOM.BoYesNoEnum.tNO;
                    }
                    if (_ManageBatchNumbers == "tYES")
                    {
                        oItems.ManageBatchNumbers = SAPbobsCOM.BoYesNoEnum.tYES;
                    }
                    else
                    {
                        oItems.ManageBatchNumbers = SAPbobsCOM.BoYesNoEnum.tNO;
                    }





                    oItems.UserFields.Fields.Item("U_TipoA2").Value = "BB";

                    if (_ItemCode != "0122-089" && _ItemCode != "0122-088" && _ItemCode != "0122-119")
                    {
                        oItems.InventoryUOM = _InventoryUOM;
                        oItems.SalesUnit = _InventoryUOM;
                    }
                    

                    //-- product style 
                    DbSapTableAdapters.PRODUCTSTYLETableAdapter _dbProductStyle = new PRODUCTSTYLETableAdapter();
                    _dbProductStyle.Connection.ConnectionString = GT.System_GT.Get_DbTori(_oCompanyNumber);
                    DbSap.PRODUCTSTYLEDataTable _tblProductStyle = new DbSap.PRODUCTSTYLEDataTable();

                    _tblProductStyle = _dbProductStyle.GetDataBy_Name(_ProductStyle);

                    if (_tblProductStyle.Rows.Count > 0)
                    {
                        oItems.UserFields.Fields.Item("U_ProducStyle").Value = _tblProductStyle[0].Code;
                    }

                    if (isGetByKey)
                    {
                        lRetCode = oItems.Update();
                    }
                    else
                    {
                        lRetCode = oItems.Add();
                    }

                    if (lRetCode != 0)
                    {
                        int lErrCode;
                        string sErrMsg;
                        oCompany.GetLastError(out lErrCode, out sErrMsg);
                        throw new Exception(sErrMsg);
                    }

                    isGetByKey = oItems.GetByKey(_ItemCode);

                    Boolean _Poly31 = false;
                    Boolean _Polint31 = false;
                    // Validar que Bodegas Existen
                    for (int i = 0; i < oItems.WhsInfo.Count; i++)
                    {

                        oItems.WhsInfo.SetCurrentLine(i);
                        if (oItems.WhsInfo.WarehouseCode == "POLY31") _Poly31 = true;
                        if (oItems.WhsInfo.WarehouseCode == "POLINT31") _Polint31 = true;
                    }

                    if (!_Poly31) // Agregar Bodega Porque no esta
                    {
                        if (oItems.WhsInfo.WarehouseCode != "") oItems.WhsInfo.Add();

                        oItems.WhsInfo.WarehouseCode = "POLY31";
                        lRetCode = oItems.Update();
                        if (lRetCode != 0)
                        {
                            int lErrCode;
                            string sErrMsg;
                            oCompany.GetLastError(out lErrCode, out sErrMsg);
                            throw new Exception(sErrMsg);
                        }

                    }


                    isGetByKey = oItems.GetByKey(_ItemCode);

                    if (!_Polint31) // Agregar Bodega Porque no esta
                    {
                        if (oItems.WhsInfo.WarehouseCode != "") oItems.WhsInfo.Add();

                        oItems.WhsInfo.WarehouseCode = "POLINT31";
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
                else
                {
                    throw new Exception("Compania no conectada");
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }

            return _ItemCode;
        }

    }
}