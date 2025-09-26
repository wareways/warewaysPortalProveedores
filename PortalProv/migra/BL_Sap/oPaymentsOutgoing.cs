using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using Sap_Service;

namespace DbSapTableAdapters
{
    public  class oPaymentsOutgoing
    {
        public oPaymentsOutgoing()
        {

        }
        
        public String Add_VendorPayments(Int32 _CodigoEmpresa, String _CuentaContableBanco, String _CodigoProveedor, DateTime _DocumentoFecha,
                                DateTime _DocumentoVencimiento, String _RefBoletaBanco, DateTime _TransferenciaFecha, Double _TransferenciaMonto,
                                String _TransferenciaReferencia, Double _TransferenciaGastosBancarios, String _SyncId,
                                List<VW_PP_PagoDetalle> _Lista_VW_PP_PagoDetalle)
        {
            // Variables Locales
            String _VendorPaymentsNewCode = ""; // Codigo de Nuevo Documento
            int _Sap_CodigoRetorno; // Codigo de Retorno de SAP despues de Ajecutar Commando Add
            int _Sap_Sucursal = GlobalSAP.GetSucursal(_CodigoEmpresa); // Obtiene Sucursal de SAP
            String _SAP_TransferAccount = "";
            String _SAP_DocCurrency = "";


            // Create Sap Objects
            SAPbobsCOM.Company oCompany;
            SAPbobsCOM.Payments oPayments;

            // Inicializar Sap Objects
            oCompany = GlobalSAP.GetCompany(_Sap_Sucursal);
            oPayments = (SAPbobsCOM.Payments)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oVendorPayments);

            try
            {

                if (oCompany.Connected)  // Revisa si se logro la Conexión
                {

                    if (!Existe_VendorPayments(_Sap_Sucursal, _SyncId, oCompany))
                    {
                        // Si no existe el SyncID en SAP hacer lo siguiente

                        oPayments.BPLID = _Sap_Sucursal; // Codigo Sucursal
                        oPayments.CardCode = _CodigoProveedor; // Codigo de Proveedor
                        oPayments.DocDate = _DocumentoFecha;
                        oPayments.DueDate = _DocumentoVencimiento;
                        oPayments.CounterReference = _RefBoletaBanco; // Numero de Referencia de la Boleta del Banco
                        oPayments.DocType = SAPbobsCOM.BoRcptTypes.rSupplier; //Tipo de Documento (FIJO)

                        // Asigna la Cta Interna y Moneda de la Cta.
                        AsignaCuentaContableBancoInternayMoneda(_CuentaContableBanco, ref _SAP_TransferAccount, ref _SAP_DocCurrency, oCompany);
                        oPayments.TransferAccount = _SAP_TransferAccount;
                        oPayments.DocCurrency = _SAP_DocCurrency;

                        // Seccion de Transferencias
                        oPayments.TransferDate = _TransferenciaFecha;  //Fecha Documento Dia Operado
                        oPayments.TransferSum = _TransferenciaMonto;  // Total de Pagos
                        oPayments.TransferReference = _TransferenciaReferencia;  //Ingreso Ultimos Diguitos de la Transferencia
                        oPayments.BankChargeAmount = _TransferenciaGastosBancarios; //Monto GastosBancarios

                        // Datos de Sincronizacion
                        oPayments.UserFields.Fields.Item("U_Sync_Id").Value = _SyncId; // Id de Sincronizacion
                        oPayments.UserFields.Fields.Item("U_Sync_Date").Value = DateTime.Now;  // Fecha de Sincronizacion


                        // Inicia Grabacion de Detalle
                        Int32 _LineasDetalle = 0;
                        String _Comentarios = "PAGO ";
                        String _NombreProveedor = "";


                        foreach (VW_PP_PagoDetalle _Item_Detalle in _Lista_VW_PP_PagoDetalle)
                        {
                            if (_LineasDetalle != 0)
                            {
                                oPayments.Invoices.Add();  // Agrega Nueva Linea de Detalle
                            }

                            oPayments.Invoices.DocEntry = (Int32)_Item_Detalle.CreatedBy;  // Equivalente TransID
                            oPayments.Invoices.InvoiceType = SAPbobsCOM.BoRcptInvTypes.it_PurchaseInvoice;
                            oPayments.Invoices.TotalDiscount = 0; // No hay Equivalente
                            oPayments.Invoices.SumApplied = double.Parse(_Item_Detalle.SaldoPagoSAPAplicar.ToString());
                            _Comentarios += string.Format("F/{0},", _Item_Detalle.NumAtCard);
                            _NombreProveedor = _Item_Detalle.CardName;

                            _LineasDetalle += 1;
                        }

                        _Comentarios = _Comentarios.TrimEnd(',') + " " + _NombreProveedor;
                        // Asigna Comentarios Finales
                        oPayments.Remarks = (_Comentarios.Length > 254) ? _Comentarios.Substring(0, 254) : _Comentarios;                        
                        oPayments.JournalRemarks = (_Comentarios.Length > 50) ? _Comentarios.Substring(0, 50) : _Comentarios;


                        _Sap_CodigoRetorno = oPayments.Add(); // Agrega Pago


                        

                        if (_Sap_CodigoRetorno != 0) // Valida Si hay Error en SAP
                        {
                            int lErrCode;
                            string sErrMsg;
                            oCompany.GetLastError(out lErrCode, out sErrMsg);
                            throw new Exception("Error SAP: " + sErrMsg);  // Excepcion Error SAP
                        }
                        else
                        {
                            oCompany.GetNewObjectCode(out _VendorPaymentsNewCode); // Obtiene en Nuevo Numero Documento
                            var _NewObjectKey = oCompany.GetNewObjectKey();
                            _VendorPaymentsNewCode = string.Format(@"{0},{1}", _VendorPaymentsNewCode, _NewObjectKey);
                        }
                        

                    }
                    else  // Si existe Company Payments
                    {
                        _VendorPaymentsNewCode = NumeroExistente_VendorPayments(_Sap_Sucursal, _SyncId, oCompany);
                    }

                }
                else // No se logro conectar a SAP
                {
                    throw new Exception("Compañia no conectada de SAP, Error de Conexión");
                }

            }
            catch (Exception ex)
            {
                throw (ex); // Excepcion Error General de Datos
            }

            return _VendorPaymentsNewCode;
        }

        public String Add_VendorPayments(Int32 _CodigoEmpresa, String _CuentaContableBanco, String _CodigoProveedor, DateTime _DocumentoFecha,
                              DateTime _DocumentoVencimiento, String _RefBoletaBanco, DateTime _TransferenciaFecha, Double _TransferenciaMonto,
                              String _TransferenciaReferencia, Double _TransferenciaGastosBancarios, String _SyncId,
                              List<VW_PP_PagoDetalle> _Lista_VW_PP_PagoDetalle,String CBC_Clasif, String CBC_Cuenta, String CBC_Empresa, String CBC_Banco )
        {
            // Variables Locales
            String _VendorPaymentsNewCode = ""; // Codigo de Nuevo Documento
            int _Sap_CodigoRetorno; // Codigo de Retorno de SAP despues de Ajecutar Commando Add
            int _Sap_Sucursal = GlobalSAP.GetSucursal(_CodigoEmpresa); // Obtiene Sucursal de SAP
            String _SAP_TransferAccount = "";
            String _SAP_DocCurrency = "";


            // Create Sap Objects
            SAPbobsCOM.Company oCompany;
            SAPbobsCOM.Payments oPayments;

            // Inicializar Sap Objects
            oCompany = GlobalSAP.GetCompany(_Sap_Sucursal);
            oPayments = (SAPbobsCOM.Payments)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oVendorPayments);

            try
            {

                if (oCompany.Connected)  // Revisa si se logro la Conexión
                {

                    if (!Existe_VendorPayments(_Sap_Sucursal, _SyncId, oCompany))
                    {
                        // Si no existe el SyncID en SAP hacer lo siguiente

                        oPayments.BPLID = _Sap_Sucursal; // Codigo Sucursal
                        oPayments.CardCode = _CodigoProveedor; // Codigo de Proveedor
                        oPayments.DocDate = _DocumentoFecha;
                        oPayments.DueDate = _DocumentoVencimiento;
                        oPayments.CounterReference = _RefBoletaBanco; // Numero de Referencia de la Boleta del Banco
                        oPayments.DocType = SAPbobsCOM.BoRcptTypes.rSupplier; //Tipo de Documento (FIJO)

                        // Asigna la Cta Interna y Moneda de la Cta.
                        AsignaCuentaContableBancoInternayMoneda(_CuentaContableBanco, ref _SAP_TransferAccount, ref _SAP_DocCurrency, oCompany);
                        oPayments.TransferAccount = _SAP_TransferAccount;
                        oPayments.DocCurrency = _SAP_DocCurrency;

                        // Seccion de Transferencias
                        oPayments.TransferDate = _TransferenciaFecha;  //Fecha Documento Dia Operado
                        oPayments.TransferSum = _TransferenciaMonto;  // Total de Pagos
                        oPayments.TransferReference = _TransferenciaReferencia;  //Ingreso Ultimos Diguitos de la Transferencia
                        oPayments.BankChargeAmount = _TransferenciaGastosBancarios; //Monto GastosBancarios

                        // Datos de Sincronizacion
                        oPayments.UserFields.Fields.Item("U_Sync_Id").Value = _SyncId; // Id de Sincronizacion
                        oPayments.UserFields.Fields.Item("U_Sync_Date").Value = DateTime.Now;  // Fecha de Sincronizacion

                        oPayments.UserFields.Fields.Item("U_CBC_Clasif").Value = CBC_Clasif;
                        oPayments.UserFields.Fields.Item("U_CBC_Ctas").Value = CBC_Cuenta;
                        oPayments.UserFields.Fields.Item("U_CBC_Banco").Value = CBC_Banco;
                        oPayments.UserFields.Fields.Item("U_CBC_Empresa").Value = CBC_Empresa;


                        // Inicia Grabacion de Detalle
                        Int32 _LineasDetalle = 0;
                        String _Comentarios = "PAGO ";
                        String _NombreProveedor = "";


                        foreach (VW_PP_PagoDetalle _Item_Detalle in _Lista_VW_PP_PagoDetalle)
                        {
                            if (_LineasDetalle != 0)
                            {
                                oPayments.Invoices.Add();  // Agrega Nueva Linea de Detalle
                            }

                            oPayments.Invoices.DocEntry = (Int32)_Item_Detalle.CreatedBy;  // Equivalente TransID
                            oPayments.Invoices.InvoiceType = SAPbobsCOM.BoRcptInvTypes.it_PurchaseInvoice;
                            oPayments.Invoices.TotalDiscount = 0; // No hay Equivalente
                            oPayments.Invoices.SumApplied = double.Parse(_Item_Detalle.SaldoPagoSAPAplicar.ToString());
                            _Comentarios += string.Format("F/{0},", _Item_Detalle.NumAtCard);
                            _NombreProveedor = _Item_Detalle.CardName;

                            _LineasDetalle += 1;
                        }

                        _Comentarios = _Comentarios.TrimEnd(',') + " " + _NombreProveedor;
                        // Asigna Comentarios Finales
                        oPayments.Remarks = (_Comentarios.Length > 254) ? _Comentarios.Substring(0, 254) : _Comentarios;
                        oPayments.JournalRemarks = (_Comentarios.Length > 50) ? _Comentarios.Substring(0, 50) : _Comentarios;


                        _Sap_CodigoRetorno = oPayments.Add(); // Agrega Pago




                        if (_Sap_CodigoRetorno != 0) // Valida Si hay Error en SAP
                        {
                            int lErrCode;
                            string sErrMsg;
                            oCompany.GetLastError(out lErrCode, out sErrMsg);
                            throw new Exception("Error SAP: " + sErrMsg);  // Excepcion Error SAP
                        }
                        else
                        {
                            oCompany.GetNewObjectCode(out _VendorPaymentsNewCode); // Obtiene en Nuevo Numero Documento
                            var _NewObjectKey = oCompany.GetNewObjectKey();
                            _VendorPaymentsNewCode = string.Format(@"{0},{1}", _VendorPaymentsNewCode, _NewObjectKey);
                        }


                    }
                    else  // Si existe Company Payments
                    {
                        _VendorPaymentsNewCode = NumeroExistente_VendorPayments(_Sap_Sucursal, _SyncId, oCompany);
                    }

                }
                else // No se logro conectar a SAP
                {
                    throw new Exception("Compañia no conectada de SAP, Error de Conexión");
                }

            }
            catch (Exception ex)
            {
                throw (ex); // Excepcion Error General de Datos
            }

            return _VendorPaymentsNewCode;
        }

        // Trae cuenta contale Interna del Banco
        private static void AsignaCuentaContableBancoInternayMoneda(string _CuentaContableBanco, ref string _SAP_TransferAccount, ref string _SAP_DocCurrency, SAPbobsCOM.Company oCompany)
        {
            SAPbobsCOM.Recordset oRecordset_BankAccount = null;
            oRecordset_BankAccount = ((SAPbobsCOM.Recordset)(oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)));
            oRecordset_BankAccount.DoQuery("SELECT * FROM [dbo].[oact] WHERE [AcctCode] = '" + _CuentaContableBanco + "' ");
            oRecordset_BankAccount.MoveFirst();
            if (oRecordset_BankAccount.EoF == true)
            {
                // No Records
            }
            else
            {
                // Encontroe Registros
                _SAP_TransferAccount = Convert.ToString(oRecordset_BankAccount.Fields.Item("AcctCode").Value);

                _SAP_DocCurrency = Convert.ToString(oRecordset_BankAccount.Fields.Item("ActCurr").Value);
            }
        }



        // Revisa si Existe la Transaccion en SAP
        private static Boolean Existe_VendorPayments(int _Sap_Sucursal, String _Sap_Sync_Id, SAPbobsCOM.Company oCompany)
        {
            Boolean _Retorna = false;

            SAPbobsCOM.Recordset oRecordset_BankAccount = null;
            oRecordset_BankAccount = ((SAPbobsCOM.Recordset)(oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)));
            oRecordset_BankAccount.DoQuery("SELECT * FROM [dbo].[OVPM] WHERE [BPLId] = " + _Sap_Sucursal.ToString() + " and [U_Sync_Id] = '" + _Sap_Sync_Id + "' ");
            oRecordset_BankAccount.MoveFirst();
            if (oRecordset_BankAccount.EoF == true) // No Records
            {
                _Retorna = false;
                // No Records
            }
            else
            {
                _Retorna = true;
                // Encontroe Registros
            }
            return _Retorna;
        }

        private static String NumeroExistente_VendorPayments(int _Sap_Sucursal, String _Sap_Sync_Id, SAPbobsCOM.Company oCompany)
        {
            String _Retorna = "";

            SAPbobsCOM.Recordset oRecordset_BankAccount = null;
            oRecordset_BankAccount = ((SAPbobsCOM.Recordset)(oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)));
            oRecordset_BankAccount.DoQuery("SELECT * FROM [dbo].[OVPM] WHERE [BPLId] = " + _Sap_Sucursal.ToString() + " and [U_Sync_Id] = '" + _Sap_Sync_Id + "' ");
            oRecordset_BankAccount.MoveFirst();
            if (oRecordset_BankAccount.EoF == true) // No Records
            {
                // No Records
            }
            else
            {
                _Retorna = Convert.ToString(oRecordset_BankAccount.Fields.Item("DocNum").Value);
                // Encontroe Registros
            }
            return _Retorna;
        }
    }

    //// ---------------------------
    //// Tipos de Pago Adicionales de Transacciones
    //// ---------------------------
    ////
    ////                 if (pago.TipoPago == TipoPagoPago.Efectivo)
    ////                {
    ////                    downPayment.CashSum = (double)total;
    ////                }
    ////                else if (pago.TipoPago == TipoPagoPago.Cheque)
    ////                {
    ////                    downPayment.Checks.CheckSum = (double)total;
    ////                    downPayment.Checks.AccounttNum = pago.CodigoCuenta;
    ////                    downPayment.Checks.BankCode = pago.CodigoBanco;
    ////                    downPayment.Checks.CheckNumber = (int)pago.NumeroCheque.Value;
    ////                    downPayment.Checks.DueDate = pago.Fecha;
    ////                }
    ////                else if (pago.TipoPago == TipoPagoPago.Deposito || pago.TipoPago == TipoPagoPago.Transferencia)
    ////                {
    ////                    downPayment.TransferAccount = pago.BancoCuenta.GLAccount;
    ////                    downPayment.TransferSum = (double)total;
    ////                }
}


    