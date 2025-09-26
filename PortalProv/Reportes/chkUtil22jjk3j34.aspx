<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="chkUtil22jjk3j34.aspx.cs" EnableEventValidation="false" Inherits="Wareways.PortalProv.Reportes.chkUtil" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <div>

            <asp:HiddenField ID="hdnConstring" runat="server" />
            <table width="100%">
                <tr>
                    <td>
                        <div class="span2">
                            <small>dB selection</small>
                            <div>
                                <asp:ListBox ID="lstConnections" runat="server" OnSelectedIndexChanged="lstDBConnections" AutoPostBack="true"></asp:ListBox>
                            </div>
                        </div>
                    </td>
                    <td>
                        <div class="span2">
                            <small>Tables</small>
                            <div>
                                <asp:ListBox ID="lstTables" runat="server" Height="200px" CssClass="small" AutoPostBack="true" OnSelectedIndexChanged="lstTableSelect"></asp:ListBox>
                            </div>
                        </div>
                    </td>
                    <td>
                        <div class="span2">
                            <small>Fields</small>
                            <div>
                                <asp:ListBox ID="lstFields" runat="server" Height="200px" CssClass="small-size" AutoPostBack="true"></asp:ListBox>
                            </div>
                        </div>
                    </td>
                    <td>
                        <div class="span2">
                            <small>StoredProcedures</small>
                            <div>
                                <asp:ListBox ID="lstSP" runat="server" Height="200px" CssClass="small-size" AutoPostBack="true" OnSelectedIndexChanged="lstSPSelect"></asp:ListBox>
                            </div>
                        </div>
                    </td>
                    <td>
                        <div class="span2">
                            <small>Views</small>
                            <div>
                                <asp:ListBox ID="lstViews" runat="server" Height="200px" CssClass="small-size" AutoPostBack="true" OnSelectedIndexChanged="lstViewsSelect"></asp:ListBox>
                            </div>
                        </div>
                    </td>
                </tr>
                <tr>
                    <td colspan="4">
                        <div>
                            <small>Query</small>
                        </div>
                        <div>
                            <asp:TextBox ID="txtQuery" TextMode="MultiLine"   Width="100%" Height="600px" runat="server"></asp:TextBox>
                        </div>
                    </td>
                    <td>
                        <div>
                            <asp:Button ID="btnCommit" runat="server" OnClick="btnCommitClick" Text="Query" />
                            <asp:Button ID="btnClear" runat="server" OnClick="btnClear_Click" Text="Clear" />
                            <asp:Button ID="btnExport" runat="server" OnClick="btnExport_Click" Text="Export" />
                        </div>
                    </td>
                </tr>
              

            </table>
             <asp:Label ID="lblInfo" runat="server" Text=""></asp:Label>
                        <div class="row">
                            <div>
                                <asp:GridView ID="grdResult" runat="server" AutoGenerateColumns="true" AllowSorting="true" HeaderStyle-BackColor="#3AC0F2" HeaderStyle-ForeColor="White"
                                    RowStyle-BackColor="#A1DCF2" AlternatingRowStyle-BackColor="White" AlternatingRowStyle-ForeColor="#000"
                                    AllowCustomPaging="false" EnableSortingAndPagingCallbacks="true" AllowPaging="false">
                                </asp:GridView>

                            </div>
                        </div>



        </div>
    </form>
</body>
</html>
