<%@ Page Title="" Language="C#" MasterPageFile="~/App_Forms/LTEcompressTecleo.master" AutoEventWireup="true" CodeFile="Com_EntregaServicioSap.aspx.cs" Inherits="App_Compras_Com_EntregaServicioSap" %>

<asp:Content ID="Content1" ContentPlaceHolderID="Page_Title" runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="head" runat="Server">
    <script src="https://unpkg.com/sweetalert/dist/sweetalert.min.js"></script>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="Page_Header" runat="Server">
    <h4>Entrega de Compras SAP</h4>
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="Page_Description" runat="Server">
</asp:Content>
<asp:Content ID="Content5" ContentPlaceHolderID="Page_Rutas" runat="Server">
</asp:Content>
<asp:Content ID="Content6" ContentPlaceHolderID="ContentPlaceHolder" runat="Server">
    <asp:UpdatePanel ID="UpdatePanel1" runat="server">
        <Triggers>
            <asp:PostBackTrigger ControlID="uiLB_CrearySubir" />
            <asp:PostBackTrigger ControlID="uiLB_SubirArchivo" />
        </Triggers>
        <ContentTemplate>
             <asp:Label runat="server" ID="uiLbl_error"></asp:Label>
    <div class="box box-solid box-default" runat="server" id="uiBox_InfoEntrega">

        <div class="box-body">
            <div class="form-horizontal">
                <div class="form-group">
                    <label class="col-sm-2 control-label">Entrega No.</label>
                    <div class="col-sm-2">
                        <asp:TextBox runat="server" ID="uiTB_Entrega_Numero" CssClass=" form-control" ReadOnly="true"></asp:TextBox>
                    </div>
                    <div class="col-sm-8">
                        &nbsp;
                    </div>
                </div>
                <div class="form-group">
                    <label class="col-sm-2 control-label">Monto</label>
                    <div class="col-sm-2">
                        <asp:TextBox runat="server" ID="uiTB_Entrega_Monto" CssClass=" form-control" ReadOnly="true"></asp:TextBox>
                    </div>
                    <label class="col-sm-2 control-label">Fecha</label>
                    <div class="col-sm-2">
                        <asp:TextBox runat="server" ID="uiTB_Entrega_Fecha" type="date" CssClass=" form-control" ReadOnly="true"></asp:TextBox>
                    </div>
                    <label class="col-sm-2 control-label">Usuario</label>
                    <div class="col-sm-2">
                        <asp:TextBox runat="server" ID="uiTB_Entrega_Usuario" CssClass=" form-control" ReadOnly="true"></asp:TextBox>
                    </div>
                </div>
                <div class="form-group">
                    <label class="col-sm-2 control-label">Observaciones</label>
                    <div class="col-sm-8">
                        <asp:TextBox runat="server" ID="uiTB_Entrega_Oservaciones" CssClass=" form-control" ReadOnly="true"></asp:TextBox>
                    </div>
                    <div class="col-sm-2">
                        <asp:HyperLink runat="server" ID="uiHL_AccesoInforme" CssClass="btn btn-primary col-md-12 iframe"><span class="fa fa-file-pdf-o"> </span> Ver Informe...</asp:HyperLink>
                    </div>
                </div>


            </div>
        </div>
    </div>
    <div class="row">
        <div class="col-md-6">
            <div class="box box-success box-solid" runat="server" id="uiBox_CrearEntrega">
                <div class="box-header with-border">
                    <h3 class="box-title">Crear Entrega</h3>
                </div>
                <div class="box-body">
                    <div class="form-horizontal">
                        <div class="form-group">
                            <label class="col-sm-3 control-label">Observaciones</label><span style="color:red">*</span>
                            <div class="col-sm-9">
                                <asp:TextBox runat="server" ID="uiTB_ObservacionesCrear" CssClass=" form-control"></asp:TextBox>
                            </div>
                        </div>     
                        <div class="form-group">
                            <label class="col-sm-3 control-label">Responsable Nombre Prov.</label><span style="color:red">*</span>
                            <div class="col-sm-9">
                                <asp:TextBox runat="server" ID="uiTB_Resp_Nombre_Prov" CssClass=" form-control"></asp:TextBox>
                            </div>
                        </div>     
                        <div class="form-group">
                            <label class="col-sm-3 control-label">Ubicacion(es)</label><span style="color:red">*</span>
                            <div class="col-sm-9">
                                <asp:TextBox TextMode="MultiLine" Rows="2" runat="server" ID="uiTB_Ubicaciones" CssClass=" form-control"></asp:TextBox>
                            </div>
                        </div>                           
                        <div class="form-group">
                            <label class="col-sm-3 control-label">Maquina(s)</label><span style="color:red">*</span>
                            <div class="col-sm-9">
                                <asp:TextBox TextMode="MultiLine"  Rows="2" runat="server" ID="uiTB_Maquinas" CssClass=" form-control"></asp:TextBox>
                            </div>
                        </div>          
                         <div class="form-group">
                            <label class="col-sm-3 control-label">Descripción Servicio</label><span style="color:red">*</span>
                            <div class="col-sm-9">
                                <asp:TextBox TextMode="MultiLine"  runat="server" ID="uiTB_Descip_Servicio" CssClass=" form-control" Rows="3"></asp:TextBox>
                            </div>
                        </div>                           
                    </div>
                </div>
                <div class="box-footer">
                    <asp:LinkButton runat="server" ID="uiLB_CrearySubir" CssClass="btn btn-primary" OnClick="uiLB_CrearySubir_Click"><span class="fa fa-save"></span> Crear y Subir</asp:LinkButton>
                    <asp:LinkButton runat="server" ID="uiLB_ChkDBSAP" CssClass="btn btn-primary" OnClick="uiLB_ChkDBSAP_Click" style="display:none"><span class="fa fa-save"></span> Rev Conexion</asp:LinkButton>
                </div>
            </div>
        </div>
        <div class="col-md-6">
            <div class="box box-primary box-solid" runat="server" id="uiBox_ActualizaAdjunto">
                <div class="box-header with-border">
                    <h4 class="box-title">Agregar Informe</h4>
                </div>
                <div class="box-body">
                    <div class="form-horizontal">
                        <div class="form-group">
                            <label class="col-sm-2 control-label">Subir Informe</label>
                            <asp:FileUpload runat="server" ID="uiFU_SubirArchivo" />

                        </div>
                        <div class="form-group">
                            <div class="col-md-12">
                                <div class="callout callout-warning" style="">Solo subir archivos PDF</div>
                            </div>

                        </div>
                    </div>

                </div>
                <div class="box-footer" style="text-align: right">
                    <asp:LinkButton runat="server" ID="uiLB_SubirArchivo" CssClass="btn btn-primary" OnClick="uiLB_SubirArchivo_Click"><span class="fa fa-save"></span> Grabar</asp:LinkButton>
                </div>
            </div>
        </div>
    </div>
        </ContentTemplate>
    </asp:UpdatePanel>
     <asp:UpdateProgress ID="UpdateProgress_Main" runat="server" AssociatedUpdatePanelID="UpdatePanel1" DisplayAfter="50">
        <ProgressTemplate>
            <div class="update_background">
                <div class="update_background_inner">
                    <img src="../../Content/images/ajax_loader_red_48.gif" />
                </div>
            </div>
        </ProgressTemplate>
    </asp:UpdateProgress>

   
</asp:Content>
<asp:Content ID="Content7" ContentPlaceHolderID="Page_FooterScript" runat="Server">
</asp:Content>

