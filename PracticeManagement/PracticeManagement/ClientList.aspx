﻿<%@ Page Language="C#" MasterPageFile="~/PracticeManagementMain.Master" AutoEventWireup="true"
    CodeBehind="ClientList.aspx.cs" Inherits="PraticeManagement.ClientList" Title="Account List | Practice Management" %>

<%@ Register Src="Controls/PracticeFilter.ascx" TagName="PracticeFilter" TagPrefix="uc1" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajaxToolkit" %>
<%@ Register TagPrefix="asp" Namespace="PraticeManagement.Controls.Generic.Buttons" Assembly="PraticeManagement" %>
<asp:Content ID="cntTitle" ContentPlaceHolderID="title" runat="server">
    <title>Account List | Practice Management</title>
</asp:Content>
<asp:Content ID="cntHeader" ContentPlaceHolderID="header" runat="server">
    Account List
</asp:Content>
<asp:Content ID="cntBody" ContentPlaceHolderID="body" runat="server">
    <asp:UpdatePanel ID="pnlBody" runat="server">
        <ContentTemplate>
            <div class="buttons-block">
                 <ajaxToolkit:CollapsiblePanelExtender ID="cpe" runat="Server"
                                TargetControlID="pnlFilters" ImageControlID="btnExpandCollapseFilter"
                                CollapsedImage="Images/expand.jpg" ExpandedImage="Images/collapse.jpg" CollapseControlID="btnExpandCollapseFilter"
                                ExpandControlID="btnExpandCollapseFilter" Collapsed="True" TextLabelID="lblFilter" />                        
                <asp:Label ID="lblFilter" runat="server"></asp:Label>&nbsp;
                <asp:Image ID="btnExpandCollapseFilter" runat="server" ImageUrl="~/Images/collapse.jpg" ToolTip="Expand Filters" />
                <asp:ShadowedHyperlink runat="server" Text="Add Account" ID="lnkAddClient" CssClass="add-btn" NavigateUrl="~/ClientDetails.aspx?returnTo=ClientList.aspx"/>
            </div>
            <asp:Panel CssClass="filters" ID="pnlFilters" runat="server">
                <AjaxControlToolkit:TabContainer ID="TabContainer1" runat="server" ActiveTabIndex="0"
                    CssClass="CustomTabStyle">
                    <ajaxToolkit:TabPanel runat="server" ID="tpMainFilters">
                        <HeaderTemplate>
                            <span class="bg  DefaultCursor"><span class="NoHyperlink" >Filters</span></span>
                        </HeaderTemplate>
                        <ContentTemplate>
                            <asp:CheckBox ID="chbShowActive" runat="server" AutoPostBack="true" Text="Show Active Accounts Only"
                                Checked="True" OnCheckedChanged="chbShowActive_CheckedChanged" />
                        </ContentTemplate>
                    </ajaxToolkit:TabPanel>
                </AjaxControlToolkit:TabContainer>
            </asp:Panel>
            <br />
            <asp:GridView ID="gvClients" runat="server" AutoGenerateColumns="False" EmptyDataText="There is nothind to be displayed here."
            DataKeyNames="Id" CssClass="CompPerfTable" GridLines="None">
            <AlternatingRowStyle BackColor="#F9FAFF" />
            <Columns>
                <asp:TemplateField>
                    <HeaderTemplate>
                        <div class="ie-bg">
                            Account Name</div>
                    </HeaderTemplate>
                    <ItemTemplate>
                        <asp:HyperLink ID="btnClientName" runat="server" Text='<%# HttpUtility.HtmlEncode((string)Eval("Name")) %>'
                            NavigateUrl='<%# GetClientDetailsUrlWithReturn(Eval("Id")) %>' />
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField>
                    <ItemStyle HorizontalAlign="Center" Width="50" />
                    <HeaderTemplate>
                        <div class="ie-bg">
                            Active</div>
                    </HeaderTemplate>
                    <ItemTemplate>
                        <asp:CheckBox ID="chbInactive" runat="server" Enabled="false" Checked='<%# !Convert.ToBoolean(Eval("Inactive")) %>' />
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField>
                    <ItemStyle HorizontalAlign="Center" Width="120" />
                    <HeaderTemplate>
                        <div class="ie-bg">
                            Billable by default</div>
                    </HeaderTemplate>
                    <ItemTemplate>
                        <asp:CheckBox ID="chbIsChargeable" runat="server" Checked='<%# Convert.ToBoolean(Eval("IsChargeable")) %>'
                            Enabled="false" />
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
        </asp:GridView>
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>

