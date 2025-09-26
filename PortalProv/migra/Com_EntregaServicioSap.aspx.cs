using Sap_Service;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class App_Compras_Com_EntregaServicioSap : System.Web.UI.Page
{
    MODULOSAPOYOEntities _DB_Apoyo = new MODULOSAPOYOEntities();
    oAttacments SAP_Attachments = new oAttacments();
    oPurchaseDeliveryNotes SAP_PurchaseDeliveryNotes = new oPurchaseDeliveryNotes();
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            CargarDatosIniciales();
        }
    }

    private void CargarDatosIniciales()
    {
        var _OrdenNumero = 0;
        try { _OrdenNumero = Int32.Parse(Request.QueryString["no"].ToString()); } catch { }
        var _Datos = _DB_Apoyo.SP_EntregasCOM_SAP("", DateTime.Parse("2000-01-01"), DateTime.Now.Date, _OrdenNumero).ToList();

        // Esconde Todas las Secciones
        uiBox_InfoEntrega.Visible = false;
        uiBox_CrearEntrega.Visible = false;
        uiBox_ActualizaAdjunto.Visible = true;


        if (_Datos.Count == 1)
        {
            // Ya tiene Entrega
            if (_Datos[0].Entrega_DocDate.HasValue)
            {
                LlenaDatosEntrega(_Datos[0]);
                uiBox_InfoEntrega.Visible = true;
                // No tiene Informe
                if (_Datos[0].Entrega_Adjuntos == 0)
                {
                    uiBox_ActualizaAdjunto.Visible = true;
                }
            }
            else // No tiene Entrega
            {
                uiBox_CrearEntrega.Visible = true;
            }
        }

    }

    private void LlenaDatosEntrega(SP_EntregasCOM_SAP_Result datos)
    {
        uiTB_Entrega_Numero.Text = datos.Entrega_DocNum.ToString();
        uiTB_Entrega_Fecha.Text = datos.Entrega_DocDate.Value.ToString("yyyy-MM-dd");
        uiTB_Entrega_Monto.Text = datos.Entrega_DocTotal.Value.ToString("###,###,##0.00");
        uiTB_Entrega_Usuario.Text = datos.Entrega_Usuario;
        uiTB_Entrega_Oservaciones.Text = datos.Entrega_Observaciones;
        uiHL_AccesoInforme.NavigateUrl = string.Format("../Servicios/SAP_AdjuntoDescarga.aspx?FileName={0}", HttpUtility.UrlEncode( datos.Entrega_AdjuntosUrl) );

    }

    protected void uiLB_SubirArchivo_Click(object sender, EventArgs e)
    {
        var _OrdenNumero = 0;
        try { _OrdenNumero = Int32.Parse(Request.QueryString["no"].ToString()); } catch { }
        var _Datos = _DB_Apoyo.SP_EntregasCOM_SAP("", DateTime.Parse("2000-01-01"), DateTime.Now.Date, _OrdenNumero).ToList();
        try
        {
            if (uiFU_SubirArchivo.HasFile)
            {
                if (uiFU_SubirArchivo.PostedFile.ContentType == "application/pdf")
                {
                    string _NombreArchivo = string.Format("Inf_{0}_{2}{1}", Guid.NewGuid(), Path.GetExtension(uiFU_SubirArchivo.PostedFile.FileName), DateTime.Now.ToString("yyyyMMddHHmmss"));

                    string path = @"/temp/" + _NombreArchivo;
                    uiFU_SubirArchivo.SaveAs(Server.MapPath(path));

                    String _Validacion = SAP_Attachments.EntregaCompras_AgregarArchivos((Int32)_Datos[0].Entrega_DocEntry, Server.MapPath(path), (Int32)_Datos[0].BPLId);

                    if (_Validacion == "")
                    {
                        ScriptManager.RegisterStartupScript(this, GetType(), "Popup",
                        GT.System_GT.f_success_SweetAlert("Informe Subido  con Exito", "", string.Format("Com_EntregaServicioSap.aspx?no={0}", _Datos[0].Orden_DocNum), ""), true);
                    } else
                    {
                        ScriptManager.RegisterStartupScript(this, GetType(),
                            "Popup", GT.System_GT.f_error_SweetAlert(new Exception(_Validacion), "", "", ""), true);
                    }
                        
                } else {
                    ScriptManager.RegisterStartupScript(this, GetType(),
                            "Popup", GT.System_GT.f_error_SweetAlert(new Exception ("El archivo no es archivo PDF"), "", "", ""), true);
                }

                

            }
            else
            {
                ScriptManager.RegisterStartupScript(this, GetType(),
                           "Popup", GT.System_GT.f_error_SweetAlert(new Exception("No tiene Adjunto Seleccionado"), "", "", ""), true);
            }
           
        }
        catch (Exception ex)
        {
            ScriptManager.RegisterStartupScript(this, GetType(),
             "Popup", GT.System_GT.f_error_SweetAlert(new Exception(ex.Message), "", "", ""), true);
        }
        
    }



    protected void uiLB_CrearySubir_Click(object sender, EventArgs e)
    {
        try
        {
            if (uiTB_ObservacionesCrear.Text != "" || uiTB_Resp_Nombre_Prov.Text == "" || uiTB_Ubicaciones.Text == "" ||
                uiTB_Descip_Servicio.Text != "" || uiTB_Maquinas.Text == "")
            {
                var _OrdenNumero = 0;
                try { _OrdenNumero = Int32.Parse(Request.QueryString["no"].ToString()); } catch { }
                var _Datos = _DB_Apoyo.SP_EntregasCOM_SAP("", DateTime.Parse("2000-01-01"), DateTime.Now.Date, _OrdenNumero).ToList();
                var _Retorna = "";

                // Crea Entrega
                _Retorna = SAP_PurchaseDeliveryNotes.CrearEntregaCompras_DeOden((Int32)_Datos[0].BPLId , _Datos[0].Orden_DocEntry, uiTB_ObservacionesCrear.Text, Session["UserName"].ToString(), uiTB_Resp_Nombre_Prov.Text, uiTB_Ubicaciones.Text, uiTB_Maquinas.Text, uiTB_Descip_Servicio.Text);

                if (_Retorna == "")
                {
                    ScriptManager.RegisterStartupScript(this, GetType(), "Popup",
                    GT.System_GT.f_success_SweetAlert("Entrega de Servicio Creada con Exito", "", string.Format("Com_EntregaServicioSap.aspx?no={0}", _Datos[0].Orden_DocNum), ""), true);

                }
                else
                {
                    ScriptManager.RegisterStartupScript(this, GetType(),
                 "Popup", GT.System_GT.f_error_SweetAlert(new Exception(_Retorna), "", "", ""), true);

                }
            }
            else
            {
                ScriptManager.RegisterStartupScript(this, GetType(),
             "Popup", GT.System_GT.f_error_SweetAlert(new Exception("Las Observaciones no puede estar Vacia"), "", "", ""), true);
            }

           
        }
        catch (Exception ex)
        {
            ScriptManager.RegisterStartupScript(this, GetType(),
             "Popup", GT.System_GT.f_error_SweetAlert(new Exception(ex.Message), "", "", ""), true);
            
        }


    }




    protected void uiLB_ChkDBSAP_Click(object sender, EventArgs e)
    {
        uiLbl_error.Text = GT.System_GT.f_success(SAP_PurchaseDeliveryNotes.EmpresaConectada(101));
    }
}