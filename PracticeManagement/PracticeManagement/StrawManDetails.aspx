﻿<%@ Page Title="Strawman Details | Practice Management" Language="C#" MasterPageFile="~/PracticeManagementMain.Master"
    AutoEventWireup="true" CodeBehind="StrawManDetails.aspx.cs" Inherits="PraticeManagement.StrawManDetails" %>

<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajaxToolkit" %>
<%@ Register Src="~/Controls/MessageLabel.ascx" TagName="MessageLabel" TagPrefix="uc" %>
<%@ Register TagPrefix="asp" Namespace="PraticeManagement.Controls.Generic.Buttons"
    Assembly="PraticeManagement" %>
<%@ Register Src="Controls/PersonnelCompensation.ascx" TagName="PersonnelCompensation"
    TagPrefix="uc1" %>
<%@ Register Src="~/Controls/Generic/LoadingProgress.ascx" TagName="LoadingProgress"
    TagPrefix="uc" %>
<asp:Content ID="Content1" ContentPlaceHolderID="title" runat="server">
    <title>Strawman Details | Practice Management</title>
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="body" runat="server">
    <uc:LoadingProgress ID="lpStrawMan" runat="server" />
    <asp:UpdatePanel ID="upStrawMan" runat="server">
        <ContentTemplate>
            <table class="WholeWidth">
                <tr>
                    <td style="padding-left: 4px; padding-top: 5px;">
                        Title
                    </td>
                    <td style="padding-top: 5px;">
                        <asp:TextBox ID="tbLastName" runat="server" onchange="setDirty();"></asp:TextBox>
                        <asp:RequiredFieldValidator ID="rqfvLastName" runat="server" Text="*" ErrorMessage="Title is required."
                            ControlToValidate="tbLastName" ToolTip="Title is required." SetFocusOnError="true" EnableClientScript="false"
                            ValidationGroup="StrawmanGroup"></asp:RequiredFieldValidator>
                        <asp:CustomValidator ID="cvLengthLastName" runat="server" Text="*" ErrorMessage="Title character length must be lessthan or equal to 50."
                            ToolTip="Title character length must be lessthan or equal to 50." ValidationGroup="StrawmanGroup" EnableClientScript="false"
                            SetFocusOnError="true" OnServerValidate="cvNameLength_ServerValidate"></asp:CustomValidator>
                    </td>
                    <td>
                    </td>
                </tr>
                <tr>
                    <td width="1%" style="padding-left: 4px;">
                        Skill
                    </td>
                    <td width="10%">
                        <asp:TextBox ID="tbFirstName" runat="server" onchange="setDirty();"></asp:TextBox>
                        <asp:RequiredFieldValidator ID="rqfvFirstName" runat="server" Text="*" ErrorMessage="Skill is required." 
                            ControlToValidate="tbFirstName" ToolTip="Skill is required." SetFocusOnError="true" EnableClientScript="false"
                            ValidationGroup="StrawmanGroup"></asp:RequiredFieldValidator>
                        <asp:CustomValidator ID="cvLengthFirstName" runat="server" Text="*" ErrorMessage="Skill characters length must be lessthan or equal to 50."
                            ToolTip="Skill characters length must be lessthan or equal to 50." ValidationGroup="StrawmanGroup" EnableClientScript="false"
                            SetFocusOnError="true" OnServerValidate="cvNameLength_ServerValidate"></asp:CustomValidator>
                    </td>
                    <td>
                    </td>
                </tr>
                <tr>
                    <td colspan="3" style="padding-top: 10px; font-weight: bold;">
                        Current Compensation :
                    </td>
                </tr>
                <tr>
                    <td colspan="3">
                        <asp:Panel ID="pnlCompensation" runat="server" CssClass="bg-light-frame">
                            <div class="filters" style="margin-top: 5px; margin-bottom: 10px;">
                                <uc1:PersonnelCompensation ID="personnelCompensation" runat="server" IsStrawmanMode="true" ValidationGroup="StrawmanGroup" />
                            </div>
                            <div style="padding-top: 10px; font-weight: bold; height: 50px">
                                Compensation History :<br />
                                <span style="color: Gray; font-family: Arial;">Click on Start Date column to edit an
                                    item<br />
                                </span>
                            </div>
                            <asp:GridView ID="gvCompensationHistory" runat="server" AutoGenerateColumns="False"
                                EmptyDataText="No compensation history for this person." CssClass="CompPerfTable WholeWidth"
                                GridLines="None" BackColor="White">
                                <AlternatingRowStyle BackColor="#F9FAFF" />
                                <Columns>
                                    <asp:TemplateField HeaderText="Start">
                                        <ItemTemplate>
                                            <asp:LinkButton ID="btnStartDate" runat="server" Text='<%# ((DateTime?)Eval("StartDate")).HasValue ? ((DateTime?)Eval("StartDate")).Value.ToString("MM/dd/yyyy") : string.Empty %>'
                                                CommandArgument='<%# Eval("StartDate") %>' OnCommand="btnStartDate_Command" OnClientClick="if (!confirmSaveDirty()) return false;"></asp:LinkButton>
                                        </ItemTemplate>
                                        <HeaderTemplate>
                                            <div class="ie-bg">
                                                Start</div>
                                        </HeaderTemplate>
                                        <ItemStyle HorizontalAlign="Center" />
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="End">
                                        <ItemTemplate>
                                            <asp:Label ID="lblEndDate" runat="server" Text='<%# ((DateTime?)Eval("EndDate")).HasValue ? ((DateTime?)Eval("EndDate")).Value.AddDays(-1).ToString("MM/dd/yyyy") : string.Empty %>'></asp:Label></ItemTemplate>
                                        <HeaderTemplate>
                                            <div class="ie-bg">
                                                End</div>
                                        </HeaderTemplate>
                                        <ItemStyle HorizontalAlign="Center" />
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Basis">
                                        <ItemTemplate>
                                            <asp:Label ID="lblBasis" runat="server" Text='<%# Eval("TimescaleName") %>'></asp:Label></ItemTemplate>
                                        <HeaderTemplate>
                                            <div class="ie-bg">
                                                Basis</div>
                                        </HeaderTemplate>
                                        <ItemStyle HorizontalAlign="Center" />
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Amount">
                                        <HeaderTemplate>
                                            <div class="ie-bg">
                                                Pay Rate</div>
                                        </HeaderTemplate>
                                        <ItemStyle HorizontalAlign="Center" />
                                        <EditItemTemplate>
                                            <asp:TextBox ID="TextBox1" runat="server" Text='<%# Bind("Amount") %>'></asp:TextBox>
                                        </EditItemTemplate>
                                        <ItemTemplate>
                                            <asp:Label ID="Label1" runat="server" Text='<%# Bind("Amount") %>'></asp:Label>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Vacation">
                                        <HeaderTemplate>
                                            <div class="ie-bg">
                                                PTO Accrual (In Hours)</div>
                                        </HeaderTemplate>
                                        <ItemStyle HorizontalAlign="Center" />
                                        <EditItemTemplate>
                                            <asp:TextBox ID="TextBox2" runat="server" Text='<%# ((int?)Eval("VacationDays")).HasValue ? (((int?)Eval("VacationDays")).Value * 8).ToString() : string.Empty %>'></asp:TextBox>
                                        </EditItemTemplate>
                                        <ItemTemplate>
                                            <asp:Label ID="Label2" runat="server" Text='<%# ((int?)Eval("VacationDays")).HasValue ? (((int?)Eval("VacationDays")).Value  * 8).ToString() : string.Empty %>'></asp:Label>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderStyle-HorizontalAlign="Center">
                                        <HeaderTemplate>
                                            <div class="ie-bg">
                                                &nbsp;
                                            </div>
                                        </HeaderTemplate>
                                        <ItemStyle Width="3%" HorizontalAlign="Center" Height="20px" Wrap="false" />
                                        <ItemTemplate>
                                            <asp:ImageButton ID="imgCompensationDelete" ToolTip="Delete" runat="server" OnClick="imgCompensationDelete_OnClick"
                                                ImageUrl="~/Images/cross_icon.png" />
                                            <ajaxToolkit:ConfirmButtonExtender ID="ConfirmButtonExtender1" ConfirmText="Are you sure you want to delete this compensation record?"
                                                runat="server" TargetControlID="imgCompensationDelete">
                                            </ajaxToolkit:ConfirmButtonExtender>
                                        </ItemTemplate>
                                        <EditItemTemplate>
                                        </EditItemTemplate>
                                    </asp:TemplateField>
                                </Columns>
                            </asp:GridView>
                        </asp:Panel>
                    </td>
                </tr>
                <tr>
                    <td colspan="3" style="padding-top: 15px;">
                    </td>
                </tr>
                <tr>
                    <td colspan="3" style="text-align: center;">
                        <asp:Button ID="btnSave" runat="server" Text="Save" ToolTip="Save" OnClick="btnSave_Click"/>
                        <asp:CancelAndReturnButton ID="btnCancelAndRetrun" runat="server" />
                    </td>
                </tr>
            </table>
            <asp:HiddenField ID="hdnTargetValidationPanel" runat="server" />
            <AjaxControlToolkit:ModalPopupExtender ID="mpeValidationPanel" runat="server" BehaviorID="mpeValidationPanelBehaviourId"
                TargetControlID="hdnTargetValidationPanel" BackgroundCssClass="modalBackground"
                PopupControlID="pnlValidationPanel" OkControlID="btnOKValidationPanel" CancelControlID="btnOKValidationPanel"
                DropShadow="false" />
            <asp:Panel ID="pnlValidationPanel" runat="server" BackColor="White" BorderColor="Black"
                Style="display: none; max-height: 400px; max-width: 550px; min-height: 100px;
                min-width: 400px" BorderWidth="2px">
                <table width="100%">
                    <tr>
                        <th align="center" style="text-align: center; background-color: Gray;" colspan="2"
                            valign="bottom">
                            <b style="font-size: 14px; padding-top: 2px;">Attention!</b>
                        </th>
                    </tr>
                    <tr>
                        <td style="padding: 10px;">
                            <asp:ValidationSummary ID="valSummary" runat="server" ValidationGroup="StrawmanGroup" />
                            <uc:MessageLabel ID="lblSave" runat="server" ErrorColor="Red" InfoColor="DarkGreen"
                                WarningColor="Orange" EnableViewState="false" />
                        </td>
                    </tr>
                    <tr>
                        <td style="padding: 10px; text-align: center;">
                            <asp:Button ID="btnOKValidationPanel" runat="server" ToolTip="OK" Text="OK" Width="100" />
                        </td>
                    </tr>
                </table>
            </asp:Panel>
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>

