using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using Sap_Service;


namespace DbSapTableAdapters
{
    /// <summary>
    /// Summary description for PPrensa_OrderArtes
    /// </summary>
    /// 

    public partial class oBusinessPartnersTableAdapter
    {

        [DataObjectMethod
        (DataObjectMethodType.Select, false)]
        public DbSap.OCRDDataTable GetDataBy_CardCode(int _CompanyNumber, string _CardCode)
        {
            DbSapTableAdapters.OCRDTableAdapter _db = new OCRDTableAdapter();
            _db.Connection.ConnectionString = GT.System_GT.Get_DbTori(_CompanyNumber);
            return _db.GetDataBy_CardCode(_CardCode);
        }

        [DataObjectMethod
        (DataObjectMethodType.Insert, true)]
        public string Insert_BusinessPartner(int oCompanyNumber, string _CardCode, string _CardName, string _NIT, string _CardType, string _SalesRepName, string _SalesAbr, string _Currency, string _Phone1, string _Phone2,
                string Street, string City, string State, string Country, string ZipCode, string _MensajeExenta)
        {
            SAPbobsCOM.Company oCompany;
            oCompany = GlobalSAP.GetCompany(oCompanyNumber);

            DbSapTableAdapters.OSLPTableAdapter _DbOSLP = new OSLPTableAdapter();
            _DbOSLP.Connection.ConnectionString = GT.System_GT.Get_DbTori(oCompanyNumber);
            DbSap.OSLPDataTable _tblOSLP = new DbSap.OSLPDataTable();

            string StateCode = "";
            string CountryCode = "";

            if(_MensajeExenta == null)
            {
                _MensajeExenta = "";
            }

            try
            {
                SAPbobsCOM.BusinessPartners oBP;
                int lRetCode;
                bool isGetByKey = false;

                if (oCompany.Connected)
                {
                    oBP = (SAPbobsCOM.BusinessPartners)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oBusinessPartners);

                    isGetByKey = oBP.GetByKey(_CardCode);

                   // if (!isGetByKey)
                   // {
                        oBP.CardCode = _CardCode;
                    //}
                    oBP.CardName = _CardName;
                    
                    oBP.CardForeignName = _CardName;
                    try { oBP.Phone1 = GT.System_GT.Left(_Phone1, 20); }
                    catch { }
                    try { oBP.Phone2 = GT.System_GT.Left(_Phone2, 20); }
                    catch { }

                    if (_CardType == "cCustomer")
                    {
                        oBP.CardType = SAPbobsCOM.BoCardTypes.cCustomer;
                        //Vendedor
                        _tblOSLP = _DbOSLP.GetDataBy_SlpName(_SalesRepName.Replace("Geo ", ""));

                        if (_tblOSLP.Rows.Count > 0)
                        {
                            oBP.SalesPersonCode = _tblOSLP[0].SlpCode;
                        }
                        else
                        {
                            //oBP.SalesPersonCode = Convert.ToInt32(Insert_Vendedores(oCompanyNumber, _SalesRepName.Replace("Geo ", ""), _SalesAbr));
                            if (oBP.SalesPersonCode == 0)
                            {
                                //GT.System_GT.f_error(new Exception("Cliente no tiene vendedor" + _SalesRepName), "oBusinessPartners.cs", "");
                                throw new Exception("Cliente no tiene vendedor" + _SalesRepName);
                            }
                        }
                    }
                    //currency
                    DbSapTableAdapters.OCRNTableAdapter _DbOCRN = new OCRNTableAdapter();
                    _DbOCRN.Connection.ConnectionString = GT.System_GT.Get_DbTori(oCompanyNumber);
                    DbSap.OCRNDataTable _tblOCRN = new DbSap.OCRNDataTable();

                    string xCurrency = oBP.Currency.ToString(); 
 
                    _tblOCRN = _DbOCRN.GetDataBy_DocCurrCod(_Currency);

                    if (_tblOCRN.Rows.Count > 0)
                    {
                        if (xCurrency != "##")
                        {
                            oBP.Currency = _tblOCRN[0].CurrCode;
                        }
                    }

                    if (_CardType == "cSupplier")
                    {
                        oBP.CardType = SAPbobsCOM.BoCardTypes.cSupplier;
                    }

                    oBP.FederalTaxID = "000000000000";
                    oBP.UserFields.Fields.Item("U_NIT").Value = _NIT;
                    oBP.UserFields.Fields.Item("U_Resolucion").Value = _MensajeExenta;

                    //ADDRESS 
                    //for (int i = 0; i <= oBP.Addresses.Count - 1; i++)
                    //{
                    //0			B

                    //COUNTRY
                    DbSapTableAdapters.OCRYTableAdapter _dbOCRY = new OCRYTableAdapter();
                    DbSap.OCRYDataTable _tblOCRY = new DbSap.OCRYDataTable();

                    _tblOCRY = _dbOCRY.GetDataBy_Name(Country); 
                    if(_tblOCRY.Rows.Count >0 )
                    {
                        CountryCode = _tblOCRY[0].Code; 
                    }

                    //state
                    DbSapTableAdapters.OCSTTableAdapter _dbOCST = new OCSTTableAdapter();
                    DbSap.OCSTDataTable _tblOCST = new DbSap.OCSTDataTable();

                    _tblOCST = _dbOCST.GetDataBy_Country_Name(CountryCode, State);
                    if(_tblOCST.Rows.Count >  0)
                    {
                        StateCode = _tblOCST[0].Code;
                    }

                    if (oBP.Addresses.Count == 0)
                    {
                        oBP.Addresses.Add();
                    }

                    oBP.Addresses.SetCurrentLine(0);
                    oBP.Addresses.TypeOfAddress = "B";
                    oBP.Addresses.AddressType = SAPbobsCOM.BoAddressType.bo_BillTo;
                    oBP.Addresses.AddressName = "Fiscal";
                    oBP.Addresses.Street = Street;
                    oBP.Addresses.City = City; //City

                    //oBP.Addresses.State = ""; //ProvState
                    //oBP.Addresses.Country = "DE"; //country


                    DbSysTableAdapters.tblSmCountry_ISOTableAdapter _TA_CountryISO = new DbSysTableAdapters.tblSmCountry_ISOTableAdapter();
                    var _CountryISO_result = _TA_CountryISO.GetDataBy_Country(Country);
                    if (_CountryISO_result.Count == 1)
                    {
                        oBP.Country = _CountryISO_result[0].Country_ISO;
                    }

                    oBP.Addresses.ZipCode = ZipCode; // postalzipcode 

                    if (oBP.Addresses.Count <= 1)
                    {
                        oBP.Addresses.Add();
                    }
                    //1			S
                    oBP.Addresses.SetCurrentLine(1);
                    oBP.Addresses.TypeOfAddress = "S";
                    oBP.Addresses.AddressType = SAPbobsCOM.BoAddressType.bo_ShipTo;
                    oBP.Addresses.AddressName = "Destino";

                    oBP.Addresses.Street = Street;
                    oBP.Addresses.City = City; //City

                  

                    //oBP.Addresses.State = ""; //ProvState
                    //oBP.Addresses.Country = "DE"; //country
                    oBP.Addresses.ZipCode = ZipCode; // postalzipcode 

                    //}

                    if (isGetByKey)
                    {
                        //lRetCode = oBP.Update();

			lRetCode = 0;
                    }
                    else
                    {
                        oBP.Frozen = SAPbobsCOM.BoYesNoEnum.tYES;

                        //lRetCode = oBP.Add();

			lRetCode = 0;
                    }


                    if (lRetCode != 0)
                    {
                        int lErrCode;
                        string sErrMsg;
                        oCompany.GetLastError(out lErrCode, out sErrMsg);
                        throw new Exception(sErrMsg);
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

            return _CardCode;
        }

        [DataObjectMethod
       (DataObjectMethodType.Insert, true)]
        public string Insert_Vendedores(int oCompanyNumber, string _SalesEmployeeName, string _U_Siglas)
        {
            SAPbobsCOM.Company oCompany;
            oCompany = GlobalSAP.GetCompany(oCompanyNumber);
            string _InvCodeStr = "";

            try
            {
                SAPbobsCOM.SalesPersons oSLP;
                int lRetCode;

                if (oCompany.Connected)
                {
                    oSLP = (SAPbobsCOM.SalesPersons)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oSalesPersons);
                    oSLP.SalesEmployeeName = _SalesEmployeeName;
                    oSLP.UserFields.Fields.Item("U_Siglas").Value = _U_Siglas;

                    lRetCode = oSLP.Add();

                    if (lRetCode != 0)
                    {
                        int lErrCode;
                        string sErrMsg;
                        oCompany.GetLastError(out lErrCode, out sErrMsg);
                        throw new Exception(sErrMsg);
                    }

                    oCompany.GetNewObjectCode(out _InvCodeStr);

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

            if (_InvCodeStr == "")
            {
                _InvCodeStr = "0";
            }

            return _InvCodeStr;
        }

        [DataObjectMethod
        (DataObjectMethodType.Insert, true)]
        public string Insert_BusinessPartner(int oCompanyNumber, string _CardCode, string _CardName, string _NIT, string _CardType, string _SalesRepName, string _SalesAbr, string _Currency, string _Phone1, string _Phone2,
                string Street, string City, string State, string Country, string ZipCode, string _MensajeExenta, string taxGroupId, string FraseExento)
        {
            SAPbobsCOM.Company oCompany;
            oCompany = GlobalSAP.GetCompany(oCompanyNumber);

            DbSapTableAdapters.OSLPTableAdapter _DbOSLP = new OSLPTableAdapter();
            _DbOSLP.Connection.ConnectionString = GT.System_GT.Get_DbTori(oCompanyNumber);
            DbSap.OSLPDataTable _tblOSLP = new DbSap.OSLPDataTable();

            string StateCode = "";
            string CountryCode = "";

            if (_MensajeExenta == null)
            {
                _MensajeExenta = "";
            }

            try
            {
                SAPbobsCOM.BusinessPartners oBP;
                int lRetCode;
                bool isGetByKey = false;

                if (oCompany.Connected)
                {
                    oBP = (SAPbobsCOM.BusinessPartners)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oBusinessPartners);

                    isGetByKey = oBP.GetByKey(_CardCode);

                    // if (!isGetByKey)
                    // {
                    oBP.CardCode = _CardCode;
                    //}
                    oBP.CardName = _CardName;

                    oBP.CardForeignName = _CardName;
                    try { oBP.Phone1 = GT.System_GT.Left(_Phone1, 20); }
                    catch { }
                    try { oBP.Phone2 = GT.System_GT.Left(_Phone2, 20); }
                    catch { }

                    if (_CardType == "cCustomer")
                    {
                        oBP.CardType = SAPbobsCOM.BoCardTypes.cCustomer;
                        //Vendedor
                        _tblOSLP = _DbOSLP.GetDataBy_SlpName(_SalesRepName.Replace("Geo ", ""));

                        if (_tblOSLP.Rows.Count > 0)
                        {
                            oBP.SalesPersonCode = _tblOSLP[0].SlpCode;
                        }
                        else
                        {
                            //oBP.SalesPersonCode = Convert.ToInt32(Insert_Vendedores(oCompanyNumber, _SalesRepName.Replace("Geo ", ""), _SalesAbr));
                            if (oBP.SalesPersonCode == 0)
                            {
                                //GT.System_GT.f_error(new Exception("Cliente no tiene vendedor" + _SalesRepName), "oBusinessPartners.cs", "");
                                throw new Exception("Cliente no tiene vendedor" + _SalesRepName);
                            }
                        }
                    }
                    //currency
                    DbSapTableAdapters.OCRNTableAdapter _DbOCRN = new OCRNTableAdapter();
                    _DbOCRN.Connection.ConnectionString = GT.System_GT.Get_DbTori(oCompanyNumber);
                    DbSap.OCRNDataTable _tblOCRN = new DbSap.OCRNDataTable();

                    string xCurrency = oBP.Currency.ToString();

                    _tblOCRN = _DbOCRN.GetDataBy_DocCurrCod(_Currency);

                    if (_tblOCRN.Rows.Count > 0)
                    {
                        if (xCurrency != "##")
                        {
                            oBP.Currency = _tblOCRN[0].CurrCode;
                        }
                    }

                    if (_CardType == "cSupplier")
                    {
                        oBP.CardType = SAPbobsCOM.BoCardTypes.cSupplier;
                    }

                    oBP.FederalTaxID = "000000000000";
                    oBP.UserFields.Fields.Item("U_NIT").Value = _NIT;
                    oBP.UserFields.Fields.Item("U_Resolucion").Value = _MensajeExenta;
                    oBP.UserFields.Fields.Item("U_FEL_FRASE_COD").Value = FraseExento;

                    // Modificación Solicitada de Contabilidad 2016/11/23 Julio Herrera
                    oBP.VatLiable = SAPbobsCOM.BoVatStatus.vExempted;
                    if (taxGroupId == "GTM") oBP.VatLiable = SAPbobsCOM.BoVatStatus.vLiable;
                    // ------------------------------------------------------

                    //ADDRESS 
                    //for (int i = 0; i <= oBP.Addresses.Count - 1; i++)
                    //{
                    //0			B

                    //COUNTRY
                    DbSapTableAdapters.OCRYTableAdapter _dbOCRY = new OCRYTableAdapter();
                    DbSap.OCRYDataTable _tblOCRY = new DbSap.OCRYDataTable();

                    _tblOCRY = _dbOCRY.GetDataBy_Name(Country);
                    if (_tblOCRY.Rows.Count > 0)
                    {
                        CountryCode = _tblOCRY[0].Code;
                    }

                    //state
                    DbSapTableAdapters.OCSTTableAdapter _dbOCST = new OCSTTableAdapter();
                    DbSap.OCSTDataTable _tblOCST = new DbSap.OCSTDataTable();

                    _tblOCST = _dbOCST.GetDataBy_Country_Name(CountryCode, State);
                    if (_tblOCST.Rows.Count > 0)
                    {
                        StateCode = _tblOCST[0].Code;
                    }

                    if (oBP.Addresses.Count == 0)
                    {
                        oBP.Addresses.Add();
                    }

                    oBP.Addresses.SetCurrentLine(0);
                    oBP.Addresses.TypeOfAddress = "B";
                    oBP.Addresses.AddressType = SAPbobsCOM.BoAddressType.bo_BillTo;
                    oBP.Addresses.AddressName = "Fiscal";
                    oBP.Addresses.Street = Street;
                    oBP.Addresses.City = City; //City
                    oBP.Addresses.StreetNo = "";

                    //oBP.Addresses.State = ""; //ProvState
                    //oBP.Addresses.Country = "DE"; //country

                  

                    oBP.Addresses.Country = CountryCode;
                    


                    oBP.Addresses.ZipCode = ZipCode; // postalzipcode 

                    if (oBP.Addresses.Count <= 1)
                    {
                        oBP.Addresses.Add();
                    }
                    //1			S
                    oBP.Addresses.SetCurrentLine(1);
                    oBP.Addresses.TypeOfAddress = "S";
                    oBP.Addresses.AddressType = SAPbobsCOM.BoAddressType.bo_ShipTo;
                    oBP.Addresses.AddressName = "Destino";
                    oBP.Addresses.StreetNo = "";

                    oBP.Addresses.Street = Street;
                    oBP.Addresses.City = City; //City
                    //oBP.Addresses.State = ""; //ProvState
                    oBP.Addresses.Country = CountryCode;
                    oBP.Addresses.ZipCode = ZipCode; // postalzipcode 

                    //}

                    if (isGetByKey)
                    {
                       // lRetCode = oBP.Update();

			lRetCode = 0;
                    }
                    else
                    {
                        oBP.Frozen = SAPbobsCOM.BoYesNoEnum.tYES;

                       // lRetCode = oBP.Add();

			lRetCode = 0;
                    }

                    var l = oBP.ContactPerson;


                    if (lRetCode != 0)
                    {
                        int lErrCode;
                        string sErrMsg;
                        oCompany.GetLastError(out lErrCode, out sErrMsg);
                        throw new Exception(sErrMsg);
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

            return _CardCode;
        }
    }
}