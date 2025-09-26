using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using Wareways.PortalProv.Infraestructura;
using Wareways.PortalProv.Models.OC;
using Wareways.PortalProv.Models.SAP;

namespace Wareways.PortalProv.Servicios
{
    public class VServicio
    {
        Wareways.PortalProv.Infraestructura.PortalProvEntities _Db = new Infraestructura.PortalProvEntities();


        public void  GrabarLoginTime(String Email)
        {
            try
            {                

                // Costruye Query Vista
                String query = string.Format(@"update AspNetUsers
                                               set LastLogin = getdate()
                                                where Email = '{0}'						    		
                                               "
                                               , Email);
                _Db.Database.ExecuteSqlCommand(query);



            }
            catch (Exception ex)
            {

            }
            GrabarLoginTimeLogs(Email);
        }

        public void GrabarLoginTimeLogs(String Email)
        {
            try
            {

                // Costruye Query Vista
                String query = string.Format(@"INSERT INTO [dbo].[AspNetUsers_Logins] 
                                                    ([Email]
                                                    ,[LastLogin])
                                                VALUES
                                                    ('{0}' ,getdate())                                               
                                               "
                                               , Email);
                _Db.Database.ExecuteSqlCommand(query);



            }
            catch (Exception ex)
            {

            }
        }

        public List<oOCRD_Inactivo> ObtenerCardCodeInactivo(String CardCode)
        {
            List<oOCRD_Inactivo> retorna = new List<oOCRD_Inactivo>();
            
            try
            {
                string sapDatabase = HttpContext.Current.Session["EmpresaSelDB"].ToString();

                // Costruye Query Vista
                String query = string.Format(@"select  
                                            	    case when frozenFor = 'Y' THEN  'Inactivo SAP: '+FrozenComm ELSE '' END AS InactivoMensaje,  
	                                                frozenFor Inactivo, CardCode
                                                from {0}..OCRD with(nolock)
                                                where CardCode = '{1}'						    		
                                               "
                                               , sapDatabase, CardCode);
                retorna = _Db.Database.SqlQuery<oOCRD_Inactivo>(query).ToList();

                

            }
            catch (Exception ex)
            {
                
            }

            return retorna;
        }

        public List<oRespDetEntregas> ObtenerDetalleOC_Parcial(String DocnumOC)
        {

            List<oRespDetEntregas> retorna = new List<oRespDetEntregas>();
            try
            {
                string sapDatabase = HttpContext.Current.Session["EmpresaSelDB"].ToString();

                // Costruye Query Vista
                String query = string.Format(@"select SUM(q1.monto) monto, DocNum, CANCELED from (
                                               select distinct  PDN1.LineTotal+pdn1.VatSum as Monto, 
                                                       Convert(varchar(16), OPDN.DocNum) DocNum, 
	                                                   OPDN.CANCELED	                                                   
                                               from {0}..opor
	                                                inner join {0}..POR1 on  OPOR.DocEntry = POR1.DocEntry	
	                                                inner join {0}..PDN1 on  PDN1.BaseEntry = POR1.DocEntry
                                                    inner join {0}..OPDN  on OPDN.DocEntry = PDN1.DocEntry
                                               where OPOR.DocNum = {1}
                                               ) as q1 group by DocNum, CANCELED								    		
                                               "
                                               , sapDatabase, DocnumOC);
                retorna = _Db.Database.SqlQuery<oRespDetEntregas>(query).ToList();

            }
            catch (Exception ex)
            {

            }
            return retorna;
        }

        internal List<oOPCHcheckSAP> List_SapInsumoFactByDocnum(string DocNum, string sapDatabase)
        {
            List<oOPCHcheckSAP> retorna = new List<oOPCHcheckSAP>();
            try
            {
                

                // Costruye Query Vista
                String query = string.Format("SELECT DocEntry, DocNum, U_WW_SyncId, U_WW_SyncDate FROM {0}..OINV WHERE DocNum = '{1}'", sapDatabase, DocNum);
                retorna = _Db.Database.SqlQuery<oOPCHcheckSAP>(query).ToList();


            }
            catch (Exception ex)
            {

            }
            return retorna;
        }

        public List<oProductoSap> List_SapProductos(String WhsCode)
        {

            List<oProductoSap> retorna = new List<oProductoSap>();
            try
            {
                string sapDatabase = HttpContext.Current.Session["EmpresaSelDB"].ToString();

                // Costruye Query Vista
                String query = string.Format(@"SELECT OITW.WhsCode, OITM.ItemCode, OITM.ItemName, ISNULL(OITM.InvntryUom,'') UOM,  OITW.AvgPrice
                                               FROM {0}..OITM OITM 	INNER JOIN	 {0}..OITW OITW ON OITW.ItemCode = oitm.ItemCode 
                                               WHERE PrchseItem = 'Y' AND OITW.Locked <> 'Y' AND OITW.WhsCode = '{1}'"
                                               , sapDatabase, WhsCode);
                retorna = _Db.Database.SqlQuery<oProductoSap>(query).ToList();


            }
            catch (Exception ex)
            {

            }
            return retorna;
        }

        internal void UpdateOPDN_Insumos(int? eM_DocNum, string SyncId)
        {
            
            try
            {
                string sapDatabase = _Db.GEN_Empresa.First().SAP_Database;

                // Costruye Query Vista
                String query = string.Format(@"UPDATE {0}..OPDN SET U_WW_SyncId = '{1}', U_WW_SyncDate = convert(varchar(64),getdate(),120) WHERE DocNum = {2}"
                                               , sapDatabase, SyncId, eM_DocNum);
                _Db.Database.ExecuteSqlCommand(query);


            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            
        }

        public List<oProductoSap> List_SapProductos(String WhsCode, String Prefix)
        {

            List<oProductoSap> retorna = new List<oProductoSap>();
            try
            {
                string sapDatabase = HttpContext.Current.Session["EmpresaSelDB"].ToString();

                // Costruye Query Vista
                String query = string.Format(@"SELECT OITW.WhsCode, OITM.ItemCode, OITM.ItemName, ISNULL(OITM.InvntryUom,'') UOM, isnull( OITW.AvgPrice,0)  AvgPrice
                                               FROM {0}..OITM OITM 	INNER JOIN	 {0}..OITW OITW ON OITW.ItemCode = oitm.ItemCode 
                                               WHERE PrchseItem = 'Y' AND OITW.Locked <> 'Y' AND OITW.WhsCode = '{1}' 
                                                     AND ( OITM.ItemCode LIKE '{2}' OR OITM.ItemName like '{2}')"
                                               , sapDatabase, WhsCode, Prefix);
                retorna = _Db.Database.SqlQuery<oProductoSap>(query).ToList();


            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return retorna;
        }

        public List<oProductoSap> List_SapProductosCardCode(String WhsCode, String Prefix, String CardCode)
        {

            List<oProductoSap> retorna = new List<oProductoSap>();
            try
            {
                string sapDatabase = HttpContext.Current.Session["EmpresaSelDB"].ToString();

                // Costruye Query Vista
                String query = string.Format(@"SELECT OITW.WhsCode, OITM.ItemCode, OITM.ItemName, ISNULL(OITM.InvntryUom,'') UOM, 
                                               isnull( 
                                                   ( SELECT itm1.Price FROM {0}..ocrd, {0}..ITM1 
                                                     WHERE ocrd.CardCode = '{3}' AND 
                                                           itm1.PriceList = ocrd.ListNum and 
                                                           ItemCode = OITM.ItemCode),
                                               OITW.AvgPrice)  AvgPrice
                                               FROM {0}..OITM OITM 	INNER JOIN	 {0}..OITW OITW ON OITW.ItemCode = oitm.ItemCode 
                                               WHERE PrchseItem = 'Y' AND OITW.Locked <> 'Y' AND OITW.WhsCode = '{1}' 
                                                     AND ( OITM.ItemCode LIKE '{2}' OR OITM.ItemName like '{2}')"
                                               , sapDatabase, WhsCode, Prefix, CardCode);
                retorna = _Db.Database.SqlQuery<oProductoSap>(query).ToList();


            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return retorna;
        }

        public List<oMonedaSap> List_SapMonedas()
        {

            List<oMonedaSap> retorna = new List<oMonedaSap>();
            try
            {
                string sapDatabase = HttpContext.Current.Session["EmpresaSelDB"].ToString();

                // Costruye Query Vista
                String query = string.Format("SELECT CurrCode, CurrName FROM {0}..ocrn ", sapDatabase);
                retorna = _Db.Database.SqlQuery<oMonedaSap>(query).ToList();


            }
            catch (Exception ex)
            {

            }
            return retorna;
        }

        public List<oCuentaContableSap> List_SapCuentasServicio()
        {

            List<oCuentaContableSap> retorna = new List<oCuentaContableSap>();
            try
            {
                string sapDatabase = HttpContext.Current.Session["EmpresaSelDB"].ToString();

                // Costruye Query Vista
                String query = string.Format("SELECT ActId,AcctName, AcctCode FROM {0}..WW_PPROV_CuentasServico ", sapDatabase);
                retorna = _Db.Database.SqlQuery<oCuentaContableSap>(query).ToList();


            }
            catch (Exception ex)
            {

            }
            return retorna;
        }

        public List<oCuentaContableSap> List_SapCuentasServicioTodas()
        {

            List<oCuentaContableSap> retorna = new List<oCuentaContableSap>();
            try
            {
                string sapDatabase = HttpContext.Current.Session["EmpresaSelDB"].ToString();

                // Costruye Query Vista
                String query = string.Format("SELECT ActId,AcctName, AcctCode FROM {0}..OACT ", sapDatabase);
                retorna = _Db.Database.SqlQuery<oCuentaContableSap>(query).ToList();


            }
            catch (Exception ex)
            {

            }
            return retorna;
        }

        public List<oImpuestoSap> List_SapImpuesto()
        {

            List<oImpuestoSap> retorna = new List<oImpuestoSap>();
            try
            {
                retorna.Add(new oImpuestoSap { ImpCode = "IVA", ImpName = "IVA" });
                retorna.Add(new oImpuestoSap { ImpCode = "EXE", ImpName = "EXE" });

            }
            catch (Exception ex)
            {

            }
            return retorna;
        }

        public List<oUnidadSap> List_SaUnidades()
        {

            List<oUnidadSap> retorna = new List<oUnidadSap>();
            try
            {
                string sapDatabase = HttpContext.Current.Session["EmpresaSelDB"].ToString();

                // Costruye Query Vista
                String query = string.Format("SELECT UomCode, UomName FROM {0}..OUOM ", sapDatabase);
                retorna = _Db.Database.SqlQuery<oUnidadSap>(query).ToList();


            }
            catch (Exception ex)
            {

            }
            return retorna;
        }


        public List<String> ObtenerCCTiendasTodas()
        {
            List<String> retorna = new List<String>();
            try
            {
                string sapDatabase = HttpContext.Current.Session["EmpresaSelDB"].ToString();

                // Costruye Query Vista
                String query = string.Format("EXEC {0}..WW_CC_TiendasTodas", sapDatabase);
                retorna = _Db.Database.SqlQuery<String>(query).ToList();


            }
            catch (Exception ex)
            {

            }
            return retorna;
        }
        public List<String> ObtenerCCRegionesTodas()
        {
            List<String> retorna = new List<String>();
            try
            {
                string sapDatabase = HttpContext.Current.Session["EmpresaSelDB"].ToString();

                // Costruye Query Vista
                String query = string.Format("EXEC {0}..WW_CC_RegionesAsignadas", sapDatabase);
                retorna = _Db.Database.SqlQuery<String>(query).ToList();


            }
            catch (Exception ex)
            {

            }
            return retorna;
        }
        public List<String> ObtenerCCTiendasRegion(String Region)
        {
            List<String> retorna = new List<String>();
            try
            {
                string sapDatabase = HttpContext.Current.Session["EmpresaSelDB"].ToString();

                // Costruye Query Vista
                String query = string.Format("EXEC {0}..WW_CC_TiendasRegion @Region = '{1}'", sapDatabase, Region);
                retorna = _Db.Database.SqlQuery<String>(query).ToList();


            }
            catch (Exception ex)
            {

            }
            return retorna;
        }



        public List<oProveedorSap> List_SapProveedor()
        {

            List<oProveedorSap> retorna = new List<oProveedorSap>();
            try
            {
                string sapDatabase = HttpContext.Current.Session["EmpresaSelDB"].ToString();

                // Costruye Query Vista
                String query = string.Format("SELECT CardCode, CardName, ISNULL(U_NIT,'') AS U_NIT,ISNULL(U_NIT,'') + ' | '+CardName+' | '+CardCode AS   CardNameEsp  FROM {0}..OCRD WHERE CardType = 'S'", sapDatabase);
                retorna = _Db.Database.SqlQuery<oProveedorSap>(query).ToList();


            }
            catch (Exception ex)
            {

            }
            return retorna;
        }

        

        public List<oProveedorSap> List_SapProveedor(String Prefix)
        {

            List<oProveedorSap> retorna = new List<oProveedorSap>();
            try
            {
                string sapDatabase = HttpContext.Current.Session["EmpresaSelDB"].ToString();

                // Costruye Query Vista
                String query = string.Format("SELECT CardCode, CardName, ISNULL(U_NIT,'') AS U_NIT FROM {0}..OCRD where CardName like '%{1}%' OR CardCode like '%{1}%' OR U_NIT like '%{1}%'", sapDatabase, Prefix);
                retorna = _Db.Database.SqlQuery<oProveedorSap>(query).ToList();


            }
            catch (Exception ex)
            {

            }
            return retorna;
        }

        public List<OSapDoc> List_EntregaProveedorOPDN(String DocNum)
        {

            List<OSapDoc> retorna = new List<OSapDoc>();
            try
            {
                string sapDatabase = HttpContext.Current.Session["EmpresaSelDB"].ToString();

                // Costruye Query Vista
                String query = string.Format("SELECT DocEntry,Docnum,DocStatus,Canceled,CardCode,CardName,NumAtCard,DocType,Comments,DocTotal,VatSum, DocCur, ObjType, U_FacNit FROM {0}..OPDN where docnum = {1}", sapDatabase, DocNum);
                retorna = _Db.Database.SqlQuery<OSapDoc>(query).ToList();
            }
            catch (Exception ex)
            {

            }
            return retorna;
        }
        public List<OSapDetalle> List_EntregaProveedorDetPDN1(String DocEntry)
        {

            List<OSapDetalle> retorna = new List<OSapDetalle>();
            try
            {
                string sapDatabase = HttpContext.Current.Session["EmpresaSelDB"].ToString();

                // Costruye Query Vista
                String query = string.Format("SELECT LineNum, ItemCode,Quantity,Dscription,PriceAfVAT, CASE when Currency = 'QTZ' then LineTotal else TotalFrgn end LineTotal, AcctCode,WhsCode, unitMsr, CASE when Currency = 'QTZ' then VatSum else VatSumFrgn end VatSum, isnull(OcrCode,OcrCode2) OcrCode,TaxCode  FROM {0}..PDN1 where DocEntry = {1}", sapDatabase, DocEntry);
                retorna = _Db.Database.SqlQuery<OSapDetalle>(query).ToList();
            }
            catch (Exception ex)
            {

            }
            return retorna;
        }

        public List<oCentroCosto> List_SapCentroCosto()
        {

            List<oCentroCosto> retorna = new List<oCentroCosto>();
            try
            {
                
                string DimensionCC = ConfigurationManager.AppSettings["DimensionCC"].ToString();

                // Costruye Query Vista
                String query = string.Format("SELECT * FROM V_PPROV_Proveedor_CC WHERE dimcode = {0}", DimensionCC);
                retorna = _Db.Database.SqlQuery<oCentroCosto>(query).ToList();


            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return retorna;
        }

        public List<oOPCHcheckSAP> Lst_SapOPORSync(string SyncId)
        {

            List<oOPCHcheckSAP> retorna = new List<oOPCHcheckSAP>();
            try
            {
                string sapDatabase = HttpContext.Current.Session["EmpresaSelDB"].ToString();

                // Costruye Query Vista
                String query = string.Format("SELECT DocEntry, DocNum, U_WW_SyncId, U_WW_SyncDate FROM {0}..OPOR WHERE U_WW_SyncId = '{1}'", sapDatabase, SyncId);
                retorna = _Db.Database.SqlQuery<oOPCHcheckSAP>(query).ToList();


            }
            catch (Exception ex)
            {

            }
            return retorna;
        }

        public List<oOPCHcheckSAP> List_SapOPDNByDocnum(string doNum)
        {

            List<oOPCHcheckSAP> retorna = new List<oOPCHcheckSAP>();
            try
            {

                string sapDatabase = _Db.GEN_Empresa.First().SAP_Database;

                // Costruye Query Vista
                String query = string.Format("SELECT DocEntry, DocNum, U_WW_SyncId, U_WW_SyncDate FROM {0}..OPDN WHERE DocNum = '{1}'", sapDatabase, doNum);
                retorna = _Db.Database.SqlQuery<oOPCHcheckSAP>(query).ToList();


            }
            catch (Exception ex)
            {

            }
            return retorna;
        }

        public List<oOPCHcheckSAP> List_SapOPCHByDocnum(string SyncId)
        {

            List<oOPCHcheckSAP> retorna = new List<oOPCHcheckSAP>();
            try
            {

                string sapDatabase = HttpContext.Current.Session["EmpresaSelDB"].ToString();

                // Costruye Query Vista
                String query = string.Format("SELECT DocEntry, DocNum, U_WW_SyncId, U_WW_SyncDate, TransId FROM {0}..OPCH WHERE DocNum = '{1}'", sapDatabase, SyncId);
                retorna = _Db.Database.SqlQuery<oOPCHcheckSAP>(query).ToList();


            }
            catch (Exception ex)
            {

            }
            return retorna;
        }

        public List<oOPCHcheckSAP> List_SapOPORByDocnum(string SyncId)
        {

            List<oOPCHcheckSAP> retorna = new List<oOPCHcheckSAP>();
            try
            {

                string sapDatabase = _Db.GEN_Empresa.First().SAP_Database;

                // Costruye Query Vista
                String query = string.Format("SELECT DocEntry, DocNum, U_WW_SyncId, U_WW_SyncDate  FROM {0}..OPOR WHERE DocNum = '{1}'", sapDatabase, SyncId);
                retorna = _Db.Database.SqlQuery<oOPCHcheckSAP>(query).ToList();


            }
            catch (Exception ex)
            {

            }
            return retorna;
        }

        public List<oOPCHcheckSAP> List_SapOPDNSync(string SyncId)
        {

            List<oOPCHcheckSAP> retorna = new List<oOPCHcheckSAP>();
            try
            {
                string sapDatabase = HttpContext.Current.Session["EmpresaSelDB"].ToString();

                // Costruye Query Vista
                String query = string.Format("SELECT DocEntry, DocNum, U_WW_SyncId, U_WW_SyncDate FROM {0}..OPDN WHERE U_WW_SyncId = '{1}'", sapDatabase, SyncId);
                retorna = _Db.Database.SqlQuery<oOPCHcheckSAP>(query).ToList();


            }
            catch (Exception ex)
            {

            }
            return retorna;
        }

        public List<OdetalleRetencionesC> ObtenerDetalleRetencion(Guid Doc_Id)
        {
            List<OdetalleRetencionesC> retorna = new List<OdetalleRetencionesC>();
            try
            {
                


                // Costruye Query Vista
                String query = string.Format(@"select SUM(Retencion_Monto) as Retencion_Monto, t.Retencion_Nombre
                                               from  PPROV_RetencionFactura rf  inner join PPROV_Retencion r on r.Retencion_Id = rf.Retencion_Id inner join PPROV_RetencionTipo t on t.Retencion_Tipo = r.Retencion_Tipo
                                               where  rf.Doc_Id = '{0}' and t.Retencion_Activo = 1 group by  t.Retencion_Nombre", Doc_Id);
                retorna = _Db.Database.SqlQuery<OdetalleRetencionesC>(query).ToList();


            }
            catch (Exception ex)
            {

            }
            return retorna;
        }

        public List<oOPCHcheckSAP> List_SapOPCHSync(string SyncId)
        {

            List<oOPCHcheckSAP> retorna = new List<oOPCHcheckSAP>();
            try
            {
                string sapDatabase = HttpContext.Current.Session["EmpresaSelDB"].ToString();

                // Costruye Query Vista
                String query = string.Format("SELECT DocEntry, DocNum, U_WW_SyncId, U_WW_SyncDate, TransId FROM {0}..OPCH WHERE U_WW_SyncId = '{1}'", sapDatabase, SyncId);
                retorna = _Db.Database.SqlQuery<oOPCHcheckSAP>(query).ToList();


            }
            catch (Exception ex)
            {

            }
            return retorna;
        }

        public List<oOPCHcheckSAP> List_SapOPORSync(string SyncId)
        {

            List<oOPCHcheckSAP> retorna = new List<oOPCHcheckSAP>();
            try
            {
                string sapDatabase = _Db.GEN_Empresa.First().SAP_Database;

                // Costruye Query Vista
                String query = string.Format("SELECT DocEntry, DocNum, U_WW_SyncId, U_WW_SyncDate FROM {0}..OPOR WHERE U_WW_SyncId = '{1}'", sapDatabase, SyncId);
                retorna = _Db.Database.SqlQuery<oOPCHcheckSAP>(query).ToList();


            }
            catch (Exception ex)
            {

            }
            return retorna;
        }

        public List<oOPCHcheckSAP> List_SapOPOR_ByDocnum(string SyncId)
        {

            List<oOPCHcheckSAP> retorna = new List<oOPCHcheckSAP>();
            try
            {
                string sapDatabase = _Db.GEN_Empresa.First().SAP_Database;

                // Costruye Query Vista
                String query = string.Format("SELECT DocEntry, DocNum, U_WW_SyncId, U_WW_SyncDate FROM {0}..OPOR WHERE DocNum = '{1}'", sapDatabase, SyncId);
                retorna = _Db.Database.SqlQuery<oOPCHcheckSAP>(query).ToList();


            }
            catch (Exception ex)
            {

            }
            return retorna;
        }

        public List<oPOR1Detalle> List_SapOPORDetalleSync(string Docnum)
        {

            List<oPOR1Detalle> retorna = new List<oPOR1Detalle>();
            try
            {
                string sapDatabase = HttpContext.Current.Session["EmpresaSelDB"].ToString();

                // Costruye Query Vista
                String query = string.Format("select DocNum, por1.DocEntry, LineNum, Dscription, PriceAfVAT, AcctCode, TaxCode, OcrCode, Currency, Quantity,ItemCode  FROM {0}..OPOR OPOR inner join {0}..POR1 POR1 on OPOR.DocEntry = POR1.DocEntry  WHERE DocNum = '{1}'", sapDatabase, Docnum);
                retorna = _Db.Database.SqlQuery<oPOR1Detalle>(query).ToList();


            }
            catch (Exception ex)
            {

            }
            return retorna;
        }

        public List<oAlamacenSap> List_Almacenes()
        {

            List<oAlamacenSap> retorna = new List<oAlamacenSap>();
            try
            {
                string sapDatabase = HttpContext.Current.Session["EmpresaSelDB"].ToString();

                // Costruye Query Vista
                String query = string.Format("SELECT  WhsCode, WhsCode+'|'+WhsName WhsName FROM {0}..owhs where  locked = 'N' ", sapDatabase);
                retorna = _Db.Database.SqlQuery<oAlamacenSap>(query).ToList();


            }
            catch (Exception ex)
            {

            }
            return retorna;
        }

        public List<oDatosOCEmail> ObtenerOCAKI_SAP(int DocEntry)
        {

            List<oDatosOCEmail> retorna = new List<oDatosOCEmail>();
            try
            {
                string sapDatabase = _Db.GEN_Empresa.Where(p=>p.EmpresaId == 1).First().SAP_Database;

                // Costruye Query Vista
                String query = string.Format(@"SELECT OPOR.DocEntry, DocNum, NumAtCard, TaxDate,
                                                    CASE WHEN OPOR.NumAtCard LIKE '%-%'
                                                        AND OPOR.Comments LIKE '%/%' AND  OPOR.Comments LIKE '%-%'
                                                    THEN 'S' ELSE 'N' END AS EsAKIol,
                                                     OCRD.E_Mail
                                                FROM {0}..OPOR,{0}..OCRD
                                                WHERE OPOR.DocEntry = {1} AND OPOR.CardCode = OCRD.CardCode", sapDatabase, DocEntry);
                retorna = _Db.Database.SqlQuery<oDatosOCEmail>(query).ToList();

                

            }
            catch (Exception ex)
            {

            }
            return retorna;
        }


        public void UpdateOCAKI_SAP(int DocEntry, String EnviaFactura, String UltimoEstadoCuenta)
        {

            
            try
            {
                string sapDatabase = _Db.GEN_Empresa.Where(p => p.EmpresaId == 1).First().SAP_Database;

                // Costruye Query Vista
                String query = string.Format(@"update {0}..OPOR
                                                SET U_EnviaFactura = '{2}', 
                                                    U_UltimoEnvioEstCue = '{3}'
                                                WHERE OPOR.DocEntry = {1}", sapDatabase, DocEntry, EnviaFactura, UltimoEstadoCuenta);
                if (!EnviaFactura.Contains(";") && UltimoEstadoCuenta.Contains(";"))
                {
                    _Db.Database.ExecuteSqlCommand( query, new SqlParameter());
                }              
            }
            catch (Exception ex)
            {

            }
            
        }


        private string ObtenerDbConn(int EmpresaId)
        {
            var sapDatabase = "";
            try { sapDatabase = _Db.V_PPROV_Empresas.Where(p => p.Empresa_Id == EmpresaId).First().SAP_Database; }
            catch (Exception ex) { }

            return sapDatabase;
        }

        public List<SP_GetUserDepartment_Result> GetUserDepartment (String UserName )
        {
            var _retorna = new List<SP_GetUserDepartment_Result>();
            var oUsuario = _Db.AspNetUsers.Where(p => p.UserName == UserName).First();
            _retorna = _Db.SP_GetUserDepartment(oUsuario.Id).ToList();

            return _retorna;
        }

        public List<SP_GetUserOC_Result> GetUserOC(String UserName, Int32 Empresa)
        {
            var _retorna = new List<SP_GetUserOC_Result>();
            var oUsuario = _Db.AspNetUsers.Where(p => p.UserName == UserName).First();
            _retorna = _Db.SP_GetUserOC(oUsuario.Id,Empresa).ToList();
            
            

            return _retorna;
        }
        public DataSet GetUserOC_AKIOL(String UserName, Int32 Empresa)
        {
            var _retorna = Get_DatosOrdenCompraAKIOL();                      
            return _retorna;
        }


        public Int32 GetUserOCEstadoNo(String UserName, Int32 Empresa, String Estado)
        {
            Int32 _retorna =0;
            var oUsuario = _Db.AspNetUsers.Where(p => p.UserName == UserName).First();
            _retorna = _Db.SP_GetUserOCEstado(oUsuario.Id, Empresa, Estado).Count();

            return _retorna;
        }

        public List<SP_PPROV_DatosOrdenCompra_Result> Get_DatosOrdenCompra( String numeroOC)
        {


            var retorna = new List<SP_PPROV_DatosOrdenCompra_Result>();
            try
            {
                string sapDatabase = _Db.GEN_Empresa.First().SAP_Database;

                // Costruye Query Vista
                String query = string.Format("EXEC {0}.dbo.[WWSP_PPROV_DatosOrdenCompra] @Docnum = {1}", sapDatabase, numeroOC);
                // Busqueda AkiOL
                if( numeroOC.Contains("-")) query = string.Format("EXEC {0}.dbo.[WWSP_PPROV_DatosOrdenCompraAKIOL] @Docnum = '{1}'", sapDatabase, numeroOC);

                retorna = _Db.Database.SqlQuery<SP_PPROV_DatosOrdenCompra_Result>(query).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return retorna;

        }

        public DataSet Get_DatosOrdenCompraAKIOL()
        {
            var retorna = new DataSet();
            try
            {
                string sapDatabase = HttpContext.Current.Session["EmpresaSelDB"].ToString();
                DataSet dataSet = new DataSet();
                // Costruye Query Vista
                String query = string.Format("EXEC {0}.dbo.WWSP_PPROV_AKIOL_Abiertos", sapDatabase);
                using (SqlDataAdapter dataAdapter
                        = new SqlDataAdapter(query, ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString))
                {                    
                   
                    // fill the DataSet using our DataAdapter 
                    dataAdapter.Fill(dataSet);
                }


                retorna = dataSet;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return retorna;

        }
        public List<SP_EntregasCOM_SAP_Result> Get_EntregasCOM_SAP(String numeroOC, String fechaInicial, String fechaFinal)
        {
            var retorna = new List<SP_EntregasCOM_SAP_Result>();
            try
            {
                string sapDatabase = _Db.GEN_Empresa.First().SAP_Database;

                // Costruye Query Vista
                String query = string.Format("EXEC {0}.dbo.[WWSP_PPROV_EntregasCOM_SAP] @Departamento = '', @FechaInicio = '{2}', @FechaFin = '{3}',	@Orden_Numero = {1}", sapDatabase, numeroOC, fechaInicial, fechaFinal);
                retorna = _Db.Database.SqlQuery<SP_EntregasCOM_SAP_Result>(query).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return retorna;

        }
        public List<WWSP_PPROV_EntregasCOM_SAP_ByCardCode_Result> Get_EntregasCOM_SAP_ByCardCode(String CardCode, String fechaInicial, String fechaFinal)
        {
            var retorna = new List<WWSP_PPROV_EntregasCOM_SAP_ByCardCode_Result>();
            try
            {
                string sapDatabase = _Db.GEN_Empresa.First().SAP_Database;

                // Costruye Query Vista
                String query = string.Format("EXEC {0}.dbo.[WWSP_PPROV_EntregasCOM_SAP_ByCardCode] @CardCode = '{1}', @FechaInicio = '{2}', @FechaFin = '{3}'", sapDatabase, CardCode, fechaInicial, fechaFinal);
                retorna = _Db.Database.SqlQuery<WWSP_PPROV_EntregasCOM_SAP_ByCardCode_Result>(query).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return retorna;

        }


        //public List<VW_PE_DatosBaseGenerados> List_PE_DatosBaseGenerados(int EmpresaId, int PagoId)
        //{

        //    List<VW_PE_DatosBaseGenerados> retorna = new List<VW_PE_DatosBaseGenerados>();
        //    try
        //    {
        //        // Paremetros
        //        String vistaNombre = "VW_PE_DatosBaseGenerados";
        //        String implementacion = _DB_Apoyo.PP_Empresa.Where(p => p.EmpresaId == EmpresaId).Select(p => p.Implementacion).First();
        //        String companyDB = _DB_Apoyo.PP_Empresa.Where(p => p.EmpresaId == EmpresaId).Select(p => p.SAP_CompanyDB).First();

        //        // Costruye Query Vista
        //        String tableSql = string.Format("SELECT [Codigo] ,[Implementacion] ,[Nombre] , dbo.TransQry({1}, '{2}', Contenido, Implementacion,{4}) AS [Contenido] FROM [PP_Querys] WHERE Codigo = '{3}' AND  Implementacion = '{0}' ",
        //                                     implementacion, EmpresaId, companyDB, vistaNombre, PagoId);
        //        String queryVista = _DB_Apoyo.Database.SqlQuery<PP_Querys>(tableSql).Select(p => p.Contenido).First();
        //        // Tare Datos Query Vista
        //        retorna = _DB_Apoyo.Database.SqlQuery<VW_PE_DatosBaseGenerados>(queryVista).ToList();


        //    }
        //    catch (Exception ex)
        //    {

        //    }
        //    return retorna;
        //}
    }
}