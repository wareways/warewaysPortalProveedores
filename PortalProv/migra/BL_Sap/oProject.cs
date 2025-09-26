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

    public partial class oProjectTableAdapter
    {

        [DataObjectMethodAttribute
        (DataObjectMethodType.Select, false)]
        public DbSap.OCRDDataTable GetDataBy_CardCode(int _CompanyNumber, string _CardCode)
        {
            DbSapTableAdapters.OCRDTableAdapter _db = new OCRDTableAdapter();
            _db.Connection.ConnectionString = GT.System_GT.Get_DbTori(_CompanyNumber);
            return _db.GetDataBy_CardCode(_CardCode);
        }

        [DataObjectMethodAttribute
        (DataObjectMethodType.Insert, true)]
        public string Sync_oProject(int _OrderNumber)
        {
            string xReturn = "";

          
            string _Code = "";
            string _Name = "";
            int _Empresa = 0;

            DbCustServTableAdapters.vw_OrderTableAdapter _DbOrder = new DbCustServTableAdapters.vw_OrderTableAdapter();
            DbCustServ.vw_OrderDataTable _tblOrder = new DbCustServ.vw_OrderDataTable();

            _tblOrder = _DbOrder.GetDataBy_OrderNumber(_OrderNumber); 

            if (_tblOrder.Rows.Count > 0 )
            {
                _Code = _tblOrder[0].OrderNumber.ToString();
                _Name =  _tblOrder[0].CustomerName + " " + _tblOrder[0].InternalPartNbr;
                _Empresa = _tblOrder[0].Empresa; 
            }
            
            Insert_oProject(_Code, _Name, _Empresa);

            return xReturn;
        }

        [DataObjectMethodAttribute
        (DataObjectMethodType.Insert, true)]
        public string Insert_oProject( string _Code, string _Name, int _Empresa)
        {
            SAPbobsCOM.Company oCompany;
            oCompany = GlobalSAP.GetCompany(_Empresa);

            try
            {
                SAPbobsCOM.IProjectsService ProjectsService;
                SAPbobsCOM.ICompanyService CompanyService;
                SAPbobsCOM.Project Project;
                SAPbobsCOM.ProjectParams ProjectParams = null;

                if (oCompany.Connected)
                {

                    CompanyService = (SAPbobsCOM.ICompanyService)oCompany.GetCompanyService();
                    ProjectsService = (SAPbobsCOM.IProjectsService)CompanyService.GetBusinessService(SAPbobsCOM.ServiceTypes.ProjectsService);

                    Project = (SAPbobsCOM.Project)ProjectsService.GetDataInterface(SAPbobsCOM.ProjectsServiceDataInterfaces.psProject);

                    ProjectParams = (SAPbobsCOM.ProjectParams)ProjectsService.GetDataInterface(SAPbobsCOM.ProjectsServiceDataInterfaces.psProjectParams);

                    Project.Code = _Code;
                    Project.Name = _Name;

                    ProjectsService.AddProject(Project);

                    //actualiza toriflex. 
                    DbToriTableAdapters.tblOrderProjectTableAdapter _OrderProject = new DbToriTableAdapters.tblOrderProjectTableAdapter();
                    _OrderProject.InsertQuery(Convert.ToInt32(_Code), _Name, DateTime.Now, _Empresa);

                }
                else
                {
                    throw new Exception("Compania no conectada");
                }
            }
            catch (Exception ex)
            {
                GT.System_GT.f_error(ex, "_SistemaFlex.oProject.cs", "");
            }

            return _Name;
        }

     
    }
}