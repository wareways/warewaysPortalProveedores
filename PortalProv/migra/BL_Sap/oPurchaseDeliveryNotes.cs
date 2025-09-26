using Sap_Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for oPurchaseDeliveryNotes
/// </summary>
public class oPurchaseDeliveryNotes
{
    public oPurchaseDeliveryNotes()
    {
        //
        // TODO: Add constructor logic here
        //
    }

    public string EmpresaConectada(Int32 EmpresaSap)
    {
        SAPbobsCOM.Company oCompany = GlobalSAP.GetCompany(EmpresaSap);
        return oCompany.CompanyDB;
    }
    public string CrearEntregaCompras_DeOden(Int32 EmpresaSap, Int32 DocEntry, string ComentarioUsuario, string Usuario, string _RespProv, string _Ubicaciones, string _Maquinas, string _DescServicio)
    {
        String _retorna = "";
        SAPbobsCOM.Company oCompany = GlobalSAP.GetCompany(EmpresaSap);
        // Obtiene La Sucursal

        SAPbobsCOM.Documents oPurchaseOrders = (SAPbobsCOM.Documents)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oPurchaseOrders);
        Boolean _OrdenCompraEncontrada = oPurchaseOrders.GetByKey(DocEntry);

        SAPbobsCOM.Documents oPurchaseDeliveryNotes = (SAPbobsCOM.Documents)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oPurchaseDeliveryNotes);
        oPurchaseDeliveryNotes.CardCode = oPurchaseOrders.CardCode;
        oPurchaseDeliveryNotes.DocDate = DateTime.Now;
        oPurchaseDeliveryNotes.DocDueDate = oPurchaseDeliveryNotes.DocDate;
        oPurchaseDeliveryNotes.DocType = oPurchaseOrders.DocType;
        //oPurchaseDeliveryNotes.DocTotal = oPurchaseOrders.DocTotal;
        oPurchaseDeliveryNotes.DocCurrency = oPurchaseOrders.DocCurrency;
        oPurchaseDeliveryNotes.BPL_IDAssignedToInvoice = oPurchaseOrders.BPL_IDAssignedToInvoice;
        oPurchaseDeliveryNotes.UserFields.Fields.Item("U_UsersolicitaC").Value = oPurchaseOrders.UserFields.Fields.Item("U_UsersolicitaC").Value;

        oPurchaseDeliveryNotes.UserFields.Fields.Item("U_ES_RespProv").Value = _RespProv;
        oPurchaseDeliveryNotes.UserFields.Fields.Item("U_ES_Ubicaciones").Value = _Ubicaciones;
        oPurchaseDeliveryNotes.UserFields.Fields.Item("U_ES_Maquinas").Value = _Maquinas;
        oPurchaseDeliveryNotes.UserFields.Fields.Item("U_ES_DescServicio").Value = _DescServicio;
        // Detalle
        for (int i = 0; i < oPurchaseOrders.Lines.Count; i++)
        {
            oPurchaseOrders.Lines.SetCurrentLine(i);

            oPurchaseDeliveryNotes.Lines.ItemDescription = oPurchaseOrders.Lines.ItemDescription;
            oPurchaseDeliveryNotes.Lines.Quantity = oPurchaseOrders.Lines.Quantity;
            oPurchaseDeliveryNotes.Lines.PriceAfterVAT = oPurchaseOrders.Lines.PriceAfterVAT;
            oPurchaseDeliveryNotes.Lines.TaxCode = oPurchaseOrders.Lines.TaxCode;
            oPurchaseDeliveryNotes.Lines.BaseEntry = oPurchaseOrders.Lines.DocEntry;
            oPurchaseDeliveryNotes.Lines.BaseType = 22; // Purchase Order Number
            oPurchaseDeliveryNotes.Lines.BaseLine = oPurchaseOrders.Lines.LineNum;
            oPurchaseDeliveryNotes.Lines.UserFields.Fields.Item("U_TipoA").Value = oPurchaseOrders.Lines.UserFields.Fields.Item("U_TipoA").Value;
            oPurchaseDeliveryNotes.Lines.Add();
        }
        
        if ( (ComentarioUsuario + " , Autoriza " + Usuario + " " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ";" + oPurchaseOrders.Comments).Length > 250)
        {
            oPurchaseDeliveryNotes.Comments = (ComentarioUsuario + " , Autoriza " + Usuario + " " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ";" + oPurchaseOrders.Comments).Substring(0, 250);
        } else
        {
            oPurchaseDeliveryNotes.Comments = (ComentarioUsuario + " , Autoriza " + Usuario + " " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ";" + oPurchaseOrders.Comments);
        }
        

        int resp = oPurchaseDeliveryNotes.Add();

        if (resp != 0)

        {
            var _ErrorCode = oCompany.GetLastErrorCode();
            _retorna = oCompany.GetLastErrorDescription();
        }

        oCompany.Disconnect();

        return _retorna;
    }


}