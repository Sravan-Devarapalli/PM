<%@ Page Title="" Language="C#" MasterPageFile="~/PracticeManagementMain.Master" AutoEventWireup="true" CodeBehind="MarginThreshold.aspx.cs" Inherits="PraticeManagement.Config.MarginThreshold" %>

<%@ Register Src="~/Controls/DatePicker.ascx" TagName="DatePicker" TagPrefix="uc2" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajaxToolkit" %>
<asp:Content ID="Content1" ContentPlaceHolderID="title" runat="server">
    <title>Margin Thresholds | Practice Management</title>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="header" runat="server">
    Margin Thresholds
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="body" runat="server">


    <script type="text/javascript" src="../Scripts/jquery-1.4.1.min.js"></script>

    <asp:UpdatePanel ID="upnlBody" runat="server">
        <ContentTemplate>
            <AjaxControlToolkit:TabContainer ID="TabContainer1" runat="server" ActiveTabIndex="0"
                CssClass="CustomTabStyle">
                <ajaxToolkit:TabPanel runat="server" ID="tpMarginThresholds">
                    <headertemplate>
                        <span class="bg"><a href="#"><span>Contribution Margin Thresholds</span></a> </span>
                    </headertemplate>
                    <contenttemplate>
                        <div class="Width50Percent">
                            <table id="tblMarginThreshold" class="CompPerfTable WholeWidth BackGroundColorWhite">
                                <tr>
                                    <th class="Width3Percent ie-bg">&nbsp;</th>
                                    <th class="Width20PerImp">
                                        <div class='ie-bg'>
                                            Start Date
                                        </div>
                                    </th>
                                    <th class="Width20PerImp">
                                        <div class='ie-bg'>
                                            End Date
                                        </div>
                                    </th>
                                    <th class="Width20PerImp">
                                        <div class='ie-bg' style="border-right: none;">
                                            Threshold Variance
                                        </div>
                                    </th>
                                    <th class="Width3Percent ie-bg">&nbsp;</th>
                                </tr>
                                <tr class="no-wrap">
                                    <td class="Width3Percent">&nbsp;
                                    </td>
                                    <td class="textCenter">
                                        <uc2:DatePicker ID="dtpPeriodFrom" runat="server" ValidationGroup="MarginThreshold" />
                                        <asp:RequiredFieldValidator ID="reqPeriodFrom" runat="server" ControlToValidate="dtpPeriodFrom"
                                            ErrorMessage="The Start Date is required." ToolTip="The Start Date is required."
                                            Text="*" EnableClientScript="false" SetFocusOnError="true" ValidationGroup="MarginThreshold"></asp:RequiredFieldValidator>
                                        <asp:CompareValidator ID="compPeriodFrom" runat="server" ControlToValidate="dtpPeriodFrom"
                                            ErrorMessage="The StartDate has an incorrect format. It must be 'MM/dd/yyyy'."
                                            ToolTip="The StartDate has an incorrect format. It must be 'MM/dd/yyyy'."
                                            Text="*" EnableClientScript="false" SetFocusOnError="true" Display="Dynamic"
                                            Operator="DataTypeCheck" Type="Date" ValidationGroup="MarginThreshold"></asp:CompareValidator>
                                        <asp:CustomValidator ID="custThresholdPeriod" runat="server" ControlToValidate="dtpPeriodFrom"
                                            ErrorMessage="A Margin Threshold already exists for this period. Margin Threshold dates cannot overlap."
                                            ToolTip="A Margin Threshold already exists for this period. Margin Threshold dates cannot overlap."
                                            ValidationGroup="MarginThreshold" Text="*" EnableClientScript="false" SetFocusOnError="true"
                                            Display="Dynamic" OnServerValidate="custThresholdPeriod_ServerValidate"></asp:CustomValidator>
                                    </td>
                                    <td class="textCenter">
                                        <uc2:DatePicker ID="dtpPeriodTo" runat="server" />
                                        <asp:RequiredFieldValidator ID="reqPeriodTo" runat="server" ControlToValidate="dtpPeriodTo"
                                            ErrorMessage="The End Date is required." ToolTip="The End Date is required."
                                            Text="*" EnableClientScript="false" SetFocusOnError="true" ValidationGroup="MarginThreshold"></asp:RequiredFieldValidator>
                                        <asp:CompareValidator ID="compPeriodTo" runat="server" ControlToValidate="dtpPeriodTo"
                                            ErrorMessage="The End Date has an incorrect format. It must be 'MM/dd/yyyy'."
                                            ToolTip="The End Date has an incorrect format. It must be 'MM/dd/yyyy'."
                                            Text="*" EnableClientScript="false" SetFocusOnError="true" Display="Dynamic"
                                            Operator="DataTypeCheck" Type="Date" ValidationGroup="MarginThreshold"></asp:CompareValidator>
                                        <asp:CompareValidator ID="compPeriod" runat="server" ControlToCompare="dtpPeriodTo"
                                            ControlToValidate="dtpPeriodFrom" Display="Dynamic" EnableClientScript="False"
                                            ErrorMessage="The End date must be greater than the Start date."
                                            Operator="LessThanEqual" SetFocusOnError="True" Type="Date" ToolTip="The End date must be greater than the Start date."
                                            ValidationGroup="MarginThreshold">*</asp:CompareValidator>
                                    </td>
                                    <td class="textCenter">
                                        <asp:TextBox ID="tbVariance" runat="server"></asp:TextBox>%
                                        <asp:RequiredFieldValidator ID="rfvMarginThresholdVariance" runat="server" ControlToValidate="tbVariance"
                                            EnableClientScript="false" Text="*" ErrorMessage="Margin threshold Variance is Required" ToolTip="Margin threshold Variance is Required" ValidationGroup="MarginThreshold"></asp:RequiredFieldValidator>
                                        <asp:CompareValidator ID="compVariance" runat="server" Operator="DataTypeCheck" Type="Integer"
                                            ControlToValidate="tbVariance" ErrorMessage="The Margin theshold Variance must be a whole number" EnableClientScript="false" Text="*" ValidationGroup="MarginThreshold" />
                                        <AjaxControlToolkit:FilteredTextBoxExtender ID="ftetxtAmount" runat="server" TargetControlID="tbVariance"
                                            FilterMode="ValidChars" FilterType="Custom,Numbers" ValidChars=".">
                                        </AjaxControlToolkit:FilteredTextBoxExtender>
                                    </td>
                                </tr>
                        </div>

                        <div class="Width50Percent">
                            <table style="text-align: right; width: 100%">
                                <tr>
                                    <td>&nbsp;
                                    </td>
                                </tr>
                                <tr>
                                    <td>
                                        <asp:Button ID="btnSaveNewThreshold" runat="server" Text="Save" ToolTip="Save" OnClick="imgbtnUpdate_OnClick" />
                                    </td>
                                </tr>
                                <tr>
                                    <td>&nbsp;
                                    </td>
                                </tr>
                            </table>
                        </div>

                        <div class="Width50Percent">
                            <asp:Repeater ID="repMarginThresholdHistory" runat="server" OnItemDataBound="repMarginThresholdHistory_ItemDataBound" OnItemCommand="repMarginThresholdHistory_ItemCommand">
                                <HeaderTemplate>
                                    <div>
                                        <table id="tblMarginThresholdHistory" class="CompPerfTable WholeWidth BackGroundColorWhite">
                                            <thead>
                                                <tr class="no-wrap">
                                                    <th class="Width3Percent ie-bg">&nbsp;</th>
                                                    <th class="Width20PerImp">
                                                        <div class='ie-bg'>
                                                            Start Date
                                                        </div>
                                                    </th>
                                                    <th class="Width20PerImp">
                                                        <div class='ie-bg'>
                                                            End Date
                                                        </div>
                                                    </th>
                                                    <th class="Width20PerImp">
                                                        <div class='ie-bg' style="border-right: none;">
                                                            Threshold Variance
                                                        </div>
                                                    </th>
                                                    <th class="Width3Percent ie-bg"></th>
                                                </tr>
                                            </thead>
                                            <tbody>
                                </HeaderTemplate>
                                <ItemTemplate>
                                    <tr class="no-wrap">
                                        <td class="Width3Percent">
                                            <asp:ImageButton ID="imgEditThreshold" ToolTip="Edit Compensation" runat="server" CommandName="edit"
                                                ImageUrl="~/Images/icon-edit.png" />
                                            <asp:ImageButton ID="imgUpdateThreshold" Visible="false" CommandName="edit"
                                                ToolTip="Save" runat="server" ImageUrl="~/Images/icon-check.png" OnClick="imgUpdateThreshold_Click" />
                                            <asp:ImageButton ID="imgCancel" ToolTip="Cancel" runat="server" ImageUrl="~/Images/no.png" Visible="false"
                                                OnClick="imgCancel_Click" /></td>
                                        <td class="textCenter">
                                            <asp:Label ID="lblStartDate" runat="server" Text='<%#((DateTime)Eval("StartDate")).ToString("MM/dd/yyyy") %>'></asp:Label>

                                            <uc2:DatePicker ID="dtpPeriodFrom" runat="server" ValidationGroup="MarginThreshold" Visible="false" DateValue='<%#((DateTime)Eval("StartDate")).Date %>'
                                                ThresholdId='<%#Eval("Id")%>' />
                                            <asp:RequiredFieldValidator ID="reqPeriodFrom" runat="server" ControlToValidate="dtpPeriodFrom"
                                                ErrorMessage="The Start Date is required." ToolTip="The Start Date is required."
                                                Text="*" EnableClientScript="false" SetFocusOnError="true" ValidationGroup="MarginThresholdEdit"></asp:RequiredFieldValidator>
                                            <asp:CompareValidator ID="compPeriodFrom" runat="server" ControlToValidate="dtpPeriodFrom"
                                                ErrorMessage="The StartDate has an incorrect format. It must be 'MM/dd/yyyy'."
                                                ToolTip="The StartDate has an incorrect format. It must be 'MM/dd/yyyy'."
                                                Text="*" EnableClientScript="false" SetFocusOnError="true" Display="Dynamic"
                                                Operator="DataTypeCheck" Type="Date" ValidationGroup="MarginThresholdEdit"></asp:CompareValidator>

                                            <asp:CustomValidator ID="custThresholdPeriodUpdate" runat="server" ControlToValidate="dtpPeriodFrom"
                                                ErrorMessage="A Margin Threshold already exists for this period. Margin Threshold dates cannot overlap."
                                                ToolTip="A Margin Threshold already exists for this period. Margin Threshold dates cannot overlap."
                                                ValidationGroup="MarginThresholdEdit" Text="*" EnableClientScript="false" SetFocusOnError="true"
                                                Display="Dynamic"></asp:CustomValidator>
                                        </td>
                                        <td class="textCenter">
                                            <asp:Label ID="lblEndDate" runat="server" Text='<%#((DateTime)Eval("EndDate")).ToString("MM/dd/yyyy") %>'></asp:Label>

                                            <uc2:DatePicker ID="dtpPeriodTo" runat="server" ValidationGroup="MarginThreshold" Visible="false" DateValue='<%#((DateTime)Eval("EndDate")).Date%>' />
                                            <asp:RequiredFieldValidator ID="reqPeriodTo" runat="server" ControlToValidate="dtpPeriodTo"
                                                ErrorMessage="The End Date is required." ToolTip="The End Date is required."
                                                Text="*" EnableClientScript="false" SetFocusOnError="true" ValidationGroup="MarginThresholdEdit"></asp:RequiredFieldValidator>
                                            <asp:CompareValidator ID="compPeriodTo" runat="server" ControlToValidate="dtpPeriodTo"
                                                ErrorMessage="The End Date has an incorrect format. It must be 'MM/dd/yyyy'."
                                                ToolTip="The End Date has an incorrect format. It must be 'MM/dd/yyyy'."
                                                Text="*" EnableClientScript="false" SetFocusOnError="true" Display="Dynamic"
                                                Operator="DataTypeCheck" Type="Date" ValidationGroup="MarginThresholdEdit"></asp:CompareValidator>
                                            <asp:CompareValidator ID="compPeriod" runat="server" ControlToCompare="dtpPeriodTo"
                                                ControlToValidate="dtpPeriodFrom" Display="Dynamic" EnableClientScript="False"
                                                ErrorMessage="The End date must be greater than the Start date."
                                                Operator="LessThanEqual" SetFocusOnError="True" Type="Date" ToolTip="The End date must be greater than the Start date."
                                                ValidationGroup="MarginThresholdEdit">*</asp:CompareValidator>
                                        </td>
                                        <td class="textCenter">
                                            <asp:Label ID="lblThreshold" runat="server" Text='<%#Eval("ThresholdVariance")+"%"  %>'></asp:Label>

                                            <asp:TextBox ID="tbVariance" runat="server" Text='<%#Eval("ThresholdVariance") %>' Visible="false"></asp:TextBox>
                                            <asp:Label ID="lblPer" runat="server" Visible="false">%</asp:Label>
                                            <asp:RequiredFieldValidator ID="rfvMarginThresholdVariance" runat="server" ControlToValidate="tbVariance"
                                                EnableClientScript="false" Text="*" ErrorMessage="Margin threshold Variance is Required" ToolTip="Margin threshold Variance is Required"
                                                ValidationGroup="MarginThresholdEdit"></asp:RequiredFieldValidator>
                                            <asp:CompareValidator ID="compVariance" runat="server" Operator="DataTypeCheck" Type="Integer"
                                                ControlToValidate="tbVariance" ErrorMessage="The Margin theshold Variance must be a whole number"
                                                EnableClientScript="false" Text="*" ValidationGroup="MarginThresholdEdit" />
                                            <AjaxControlToolkit:FilteredTextBoxExtender ID="ftetxtAmount" runat="server" TargetControlID="tbVariance"
                                                FilterMode="ValidChars" FilterType="Custom,Numbers" ValidChars=".">
                                            </AjaxControlToolkit:FilteredTextBoxExtender>
                                        </td>
                                        <td class="textCenter">
                                            <asp:HiddenField ID="hidKey" runat="server" Value='<%# Eval("Id") %>' />
                                            <asp:ImageButton ID="imgDeleteThreshold" runat="server" ImageUrl="~/Images/icon-delete.png" Visible="false"
                                                OnClick="imgDeleteThreshold_Click" ToolTip="Delete Margin Threshold" />
                                        </td>
                                    </tr>
                                </ItemTemplate>

                                <FooterTemplate>
                                    </tbody></table></div>
                                </FooterTemplate>
                            </asp:Repeater>

                        </div>
                        <asp:ValidationSummary ID="valsMarginTheshold" runat="server" EnableClientScript="false"
                            CssClass="ApplyStyleForDashBoardLists" ValidationGroup="MarginThreshold" />
                        <asp:ValidationSummary ID="valsMarginThresholdEdit" runat="server" EnableClientScript="false"
                            CssClass="ApplyStyleForDashBoardLists" ValidationGroup="MarginThresholdEdit" />
                    </contenttemplate>
                </ajaxToolkit:TabPanel>
                <ajaxToolkit:TabPanel runat="server" ID="tpMarginException">
                    <headertemplate>
                        <span class="bg"><a href="#"><span></span>Margin Exception Thresholds</a> </span>
                    </headertemplate>
                    <contenttemplate>
                        <div class="Width50Percent">
                            <table id="tblMarginExceptionThreshold" class="CompPerfTable WholeWidth BackGroundColorWhite">
                                <tr class="no-wrap">
                                    <th class="Width5Percent ie-bg"></th>
                                    <th class="Width20PerImp">
                                        <div class='ie-bg'>
                                            Start Date
                                        </div>
                                    </th>
                                    <th class="Width20PerImp">
                                        <div class='ie-bg'>
                                            End Date
                                        </div>
                                    </th>
                                    <th class="Width15PercentImp">
                                        <div class='ie-bg'>
                                            Approval Level
                                        </div>
                                    </th>
                                    <th class="Width15PercentImp">
                                        <div class='ie-bg'>
                                            Below Margin Goal
                                        </div>
                                    </th>
                                    <th class=" Width20PerImp ">
                                        <div class='ie-bg' style="border-right: none;">
                                            Projected Revenue
                                            Greater than 
                                        </div>
                                    </th>
                                    <th class="Width3Percent ie-bg"></th>
                                </tr>
                                <tr class="no-wrap">
                                    <td class="Width3Percent">&nbsp;
                                    
                                    </td>
                                    <td class="textCenter">
                                        <uc2:DatePicker ID="dtpAddExcFrom" runat="server" ValidationGroup="MarginExceptionThreshold" />
                                        <asp:RequiredFieldValidator ID="rfvExcFrom" runat="server" ControlToValidate="dtpAddExcFrom"
                                            ErrorMessage="The Start Date is required." ToolTip="The Start Date is required."
                                            Text="*" EnableClientScript="false" SetFocusOnError="true" ValidationGroup="MarginExceptionThreshold"></asp:RequiredFieldValidator>
                                        <asp:CompareValidator ID="compFrom" runat="server" ControlToValidate="dtpAddExcFrom"
                                            ErrorMessage="The StartDate has an incorrect format. It must be 'MM/dd/yyyy'."
                                            ToolTip="The StartDate has an incorrect format. It must be 'MM/dd/yyyy'."
                                            Text="*" EnableClientScript="false" SetFocusOnError="true" Display="Dynamic"
                                            Operator="DataTypeCheck" Type="Date" ValidationGroup="MarginExceptionThreshold"></asp:CompareValidator>
                                        <asp:CustomValidator ID="custExceptionPeriod" runat="server" ControlToValidate="dtpAddExcFrom"
                                            ErrorMessage="A Margin Exception Threshold already exists for this period. Margin Exception Threshold dates cannot overlap."
                                            ToolTip="A Margin Exception Threshold already exists for this period. Margin Exception Threshold dates cannot overlap."
                                            ValidationGroup="MarginExceptionThreshold" Text="*" EnableClientScript="false" SetFocusOnError="true"
                                            Display="Dynamic" OnServerValidate="custExceptionPeriod_ServerValidate"></asp:CustomValidator>
                                    </td>
                                    <td class="textCenter">
                                        <uc2:DatePicker ID="dtpAddExcTo" runat="server" />
                                        <asp:RequiredFieldValidator ID="rqvExcTo" runat="server" ControlToValidate="dtpAddExcTo"
                                            ErrorMessage="The End Date is required." ToolTip="The End Date is required."
                                            Text="*" EnableClientScript="false" SetFocusOnError="true" ValidationGroup="MarginExceptionThreshold"></asp:RequiredFieldValidator>
                                        <asp:CompareValidator ID="compTo" runat="server" ControlToValidate="dtpAddExcTo"
                                            ErrorMessage="The End Date has an incorrect format. It must be 'MM/dd/yyyy'."
                                            ToolTip="The End Date has an incorrect format. It must be 'MM/dd/yyyy'."
                                            Text="*" EnableClientScript="false" SetFocusOnError="true" Display="Dynamic"
                                            Operator="DataTypeCheck" Type="Date" ValidationGroup="MarginExceptionThreshold"></asp:CompareValidator>
                                        <asp:CompareValidator ID="compDate" runat="server" ControlToCompare="dtpAddExcTo"
                                            ControlToValidate="dtpAddExcFrom" Display="Dynamic" EnableClientScript="False"
                                            ErrorMessage="The End date must be greater than the Start date."
                                            Operator="LessThanEqual" SetFocusOnError="True" Type="Date" ToolTip="The End date must be greater than the Start date."
                                            ValidationGroup="MarginExceptionThreshold">*</asp:CompareValidator>
                                    </td>
                                    <td class="textCenter">
                                        <asp:DropDownList ID="ddlAddApprovalLevel" runat="server">
                                            <asp:ListItem Value="-1" Text="Select Level"></asp:ListItem>
                                            <asp:ListItem Value="1" Text="Tier 1"></asp:ListItem>
                                            <asp:ListItem Value="2" Text="Tier 2"></asp:ListItem>
                                        </asp:DropDownList>
                                        <asp:RequiredFieldValidator ID="RequiredFieldValidator1" InitialValue="-1" runat="server" ControlToValidate="ddlAddApprovalLevel"
                                            ErrorMessage="The Approval Level is required." ToolTip="The Approval Level is required."
                                            Text="*" EnableClientScript="false" SetFocusOnError="true" ValidationGroup="MarginExceptionThreshold"></asp:RequiredFieldValidator>
                                    </td>
                                    <td class="textCenter">
                                        <asp:TextBox ID="txtAddMarginGoal" runat="server" Width="70%"></asp:TextBox>%
                                        <asp:RequiredFieldValidator ID="rfvMarginGoal" runat="server" ControlToValidate="txtAddMarginGoal"
                                            EnableClientScript="false" Text="*" ErrorMessage="Margin threshold Goal is Required" ToolTip="Margin threshold Goal is Required."
                                            ValidationGroup="MarginExceptionThreshold"></asp:RequiredFieldValidator>
                                        <asp:CompareValidator ID="compMarginGoal" runat="server" Operator="DataTypeCheck" Type="Integer" ToolTip="The Margin Goal Variance must be a whole number."
                                            ControlToValidate="txtAddMarginGoal" ErrorMessage="The Margin Goal Variance must be a whole number." EnableClientScript="false" Text="*"
                                            ValidationGroup="MarginExceptionThreshold" />
                                        <AjaxControlToolkit:FilteredTextBoxExtender ID="FilteredTextBoxExtender1" runat="server" TargetControlID="txtAddMarginGoal"
                                            FilterMode="ValidChars" FilterType="Custom,Numbers" ValidChars=".-">
                                        </AjaxControlToolkit:FilteredTextBoxExtender>
                                    </td>
                                    <td class="textCenter">$<asp:TextBox ID="txtAddRevenue" runat="server" Width="85%"></asp:TextBox>
                                        <asp:RequiredFieldValidator ID="rfvReveneu" runat="server" ControlToValidate="txtAddRevenue"
                                            EnableClientScript="false" Text="*" ErrorMessage="Projected Revenue is Required" ToolTip="Projected Revenue is Required."
                                            ValidationGroup="MarginExceptionThreshold"></asp:RequiredFieldValidator>
                                        <asp:CompareValidator ID="compRevenue" runat="server" ControlToValidate="txtAddRevenue"
                                            Display="Dynamic" EnableClientScript="false" ErrorMessage="A number with 2 decimal digits is allowed for the Revenue."
                                            Operator="DataTypeCheck" SetFocusOnError="true" Text="*" ToolTip="A number with 2 decimal digits is allowed for the Revenue."
                                            Type="Currency" ValidationGroup="MarginExceptionThreshold"></asp:CompareValidator>
                                        <AjaxControlToolkit:FilteredTextBoxExtender ID="FilteredTextBoxExtender2" runat="server" TargetControlID="txtAddRevenue"
                                            FilterMode="ValidChars" FilterType="Custom,Numbers" ValidChars=".">
                                        </AjaxControlToolkit:FilteredTextBoxExtender>

                                    </td>

                                </tr>
                            </table>
                        </div>
                        <div class="Width50Percent">
                            <table style="text-align: right; width: 100%">
                                <tr>
                                    <td>&nbsp;
                                    </td>
                                </tr>
                                <tr>
                                    <td>
                                        <asp:Button ID="btnSaveExceptionThreshold" runat="server" Text="Save" ToolTip="Save" OnClick="imgbtnAddExceptionThreshold_Click" />
                                    </td>
                                </tr>
                                <tr>
                                    <td>&nbsp;
                                    </td>
                                </tr>
                            </table>
                        </div>
                        <div class="Width50Percent">
                            <asp:Repeater ID="repMarginExceptionHistory" runat="server" OnItemDataBound="repMarginExceptionHistory_ItemDataBound" OnItemCommand="repMarginExceptionHistory_ItemCommand">
                                <HeaderTemplate>
                                    <div>
                                        <table id="tblMarginExceptionHistory" class="CompPerfTable WholeWidth BackGroundColorWhite">
                                            <thead>
                                                <tr class="no-wrap">
                                                    <th class="Width5Percent ie-bg"></th>
                                                    <th class="Width20PerImp">
                                                        <div class='ie-bg'>
                                                            Start Date
                                                        </div>
                                                    </th>
                                                    <th class="Width20PerImp">
                                                        <div class='ie-bg'>
                                                            End Date
                                                        </div>
                                                    </th>
                                                    <th class="Width15PercentImp">
                                                        <div class='ie-bg'>
                                                            Approval Level
                                                        </div>
                                                    </th>
                                                    <th class="Width15PercentImp">
                                                        <div class='ie-bg'>
                                                            Below Margin Goal
                                                        </div>
                                                    </th>
                                                    <th class=" Width20PerImp ">
                                                        <div class='ie-bg' style="border-right: none;">
                                                            Projected Revenue
                                                            Greater than 
                                                        </div>
                                                    </th>
                                                    <th class="Width2Percent ie-bg"></th>
                                                </tr>
                                            </thead>
                                            <tbody>
                                </HeaderTemplate>

                                <ItemTemplate>
                                    <tr class="no-wrap">
                                        <td class="Width3Percent">
                                            <asp:ImageButton ID="imgEditExceptionThreshold" ToolTip="Edit Compensation" runat="server" CommandName="edit"
                                                ImageUrl="~/Images/icon-edit.png" />
                                            <asp:ImageButton ID="imgUpdateExceptionThreshold" Visible="false" CommandName="edit"
                                                ToolTip="Save" runat="server" ImageUrl="~/Images/icon-check.png" OnClick="imgUpdateExceptionThreshold_Click" />
                                            <asp:ImageButton ID="imgExceptionCancel" ToolTip="Cancel" runat="server" ImageUrl="~/Images/no.png" Visible="false"
                                                OnClick="imgExceptionCancel_Click" />
                                        </td>
                                        <td class="textCenter">
                                            <asp:Label ID="lblStartDate" runat="server" Text='<%#((DateTime)Eval("StartDate")).ToString("MM/dd/yyyy") %>'></asp:Label>
                                            <uc2:DatePicker ID="dtpExcFrom" runat="server" ValidationGroup="MarginExceptionThresholdEdit" DateValue='<%#((DateTime)Eval("StartDate")).Date %>'
                                                ExcThresholdId='<%#Eval("Id")%>' Visible="false" />
                                            <asp:RequiredFieldValidator ID="rfvExcFrom" runat="server" ControlToValidate="dtpExcFrom"
                                                ErrorMessage="The Start Date is required." ToolTip="The Start Date is required."
                                                Text="*" EnableClientScript="false" SetFocusOnError="true" ValidationGroup="MarginExceptionThresholdEdit"></asp:RequiredFieldValidator>
                                            <asp:CompareValidator ID="compFrom" runat="server" ControlToValidate="dtpExcFrom"
                                                ErrorMessage="The StartDate has an incorrect format. It must be 'MM/dd/yyyy'."
                                                ToolTip="The StartDate has an incorrect format. It must be 'MM/dd/yyyy'."
                                                Text="*" EnableClientScript="false" SetFocusOnError="true" Display="Dynamic"
                                                Operator="DataTypeCheck" Type="Date" ValidationGroup="MarginExceptionThresholdEdit"></asp:CompareValidator>
                                            <asp:CustomValidator ID="custExceptionPeriod" runat="server" ControlToValidate="dtpExcFrom"
                                                ErrorMessage="A Margin Exception Threshold already exists for this period. Margin Exception Threshold dates cannot overlap."
                                                ToolTip="A Margin Exception Threshold already exists for this period. Margin Exception Threshold dates cannot overlap."
                                                ValidationGroup="MarginExceptionThresholdEdit" Text="*" EnableClientScript="false" SetFocusOnError="true"
                                                Display="Dynamic"></asp:CustomValidator>
                                        </td>
                                        <td class="textCenter">
                                            <asp:Label ID="lblEndDate" runat="server" Text='<%#((DateTime)Eval("EndDate")).ToString("MM/dd/yyyy") %>'></asp:Label>
                                            <uc2:DatePicker ID="dtpExcTo" runat="server" DateValue='<%#((DateTime)Eval("EndDate")).Date %>' Visible="false" />
                                            <asp:RequiredFieldValidator ID="rqvExcTo" runat="server" ControlToValidate="dtpExcTo"
                                                ErrorMessage="The End Date is required." ToolTip="The End Date is required."
                                                Text="*" EnableClientScript="false" SetFocusOnError="true" ValidationGroup="MarginExceptionThresholdEdit"></asp:RequiredFieldValidator>
                                            <asp:CompareValidator ID="compTo" runat="server" ControlToValidate="dtpExcTo"
                                                ErrorMessage="The End Date has an incorrect format. It must be 'MM/dd/yyyy'."
                                                ToolTip="The End Date has an incorrect format. It must be 'MM/dd/yyyy'."
                                                Text="*" EnableClientScript="false" SetFocusOnError="true" Display="Dynamic"
                                                Operator="DataTypeCheck" Type="Date" ValidationGroup="MarginExceptionThresholdEdit"></asp:CompareValidator>
                                            <asp:CompareValidator ID="compDate" runat="server" ControlToCompare="dtpExcTo"
                                                ControlToValidate="dtpExcFrom" Display="Dynamic" EnableClientScript="False"
                                                ErrorMessage="The End date must be greater than the Start date."
                                                Operator="LessThanEqual" SetFocusOnError="True" Type="Date" ToolTip="The End date must be greater than the Start date."
                                                ValidationGroup="MarginExceptionThresholdEdit">*</asp:CompareValidator>
                                        </td>
                                        <td class="textCenter">
                                            <asp:Label ID="lblLevel" runat="server" Text='<%#Eval("Level.Name") %>'></asp:Label>
                                            <asp:DropDownList ID="ddlApprovalLevel" runat="server" Visible="false">
                                                <asp:ListItem Value="-1" Text="--Select Level--"></asp:ListItem>
                                                <asp:ListItem Value="1" Text="Tier 1"></asp:ListItem>
                                                <asp:ListItem Value="2" Text="Tier 2"></asp:ListItem>
                                            </asp:DropDownList>
                                            <asp:RequiredFieldValidator ID="RequiredFieldValidator1" InitialValue="-1" runat="server" ControlToValidate="ddlApprovalLevel"
                                                ErrorMessage="The Approval Level is required." ToolTip="The Approval Level is required."
                                                Text="*" EnableClientScript="false" SetFocusOnError="true" ValidationGroup="MarginExceptionThresholdEdit"></asp:RequiredFieldValidator>

                                        </td>
                                        <td class="textCenter">
                                            <asp:Label ID="lblMarginGoal" runat="server" Text='<%#Eval("MarginThreshold")+"%" %>'></asp:Label>
                                            <asp:TextBox ID="txtMarginGoal" runat="server" Text='<%#Eval("MarginThreshold") %>' Visible="false" Width="70%"></asp:TextBox>
                                            <asp:Label ID="lblPer" runat="server" Text="%" Visible="false"></asp:Label>
                                            <asp:RequiredFieldValidator ID="rfvMarginGoal" runat="server" ControlToValidate="txtMarginGoal"
                                                EnableClientScript="false" Text="*" ErrorMessage="Margin threshold Goal is Required" ToolTip="Margin threshold Goal is Required."
                                                ValidationGroup="MarginExceptionThresholdEdit"></asp:RequiredFieldValidator>
                                            <asp:CompareValidator ID="compMarginGoal" runat="server" Operator="DataTypeCheck" Type="Integer" ToolTip="The Margin Goal Variance must be a whole number."
                                                ControlToValidate="txtMarginGoal" ErrorMessage="The Margin Goal Variance must be a whole number." EnableClientScript="false" Text="*"
                                                ValidationGroup="MarginExceptionThresholdEdit" />
                                            <AjaxControlToolkit:FilteredTextBoxExtender ID="FilteredTextBoxExtender2" runat="server" TargetControlID="txtMarginGoal"
                                                FilterMode="ValidChars" FilterType="Custom,Numbers" ValidChars=".-">
                                            </AjaxControlToolkit:FilteredTextBoxExtender>
                                        </td>
                                        <td class="textCenter">
                                            <asp:Label ID="lblRevenue" runat="server" Text='<%#((decimal)Eval("Revenue")).ToString("$#,###,##0.##") %>'></asp:Label>
                                            <asp:Label ID="lblDoller" runat="server" Text="$" Visible="false"></asp:Label>
                                            <asp:TextBox ID="txtRevenue" runat="server" Text='<%#((decimal)Eval("Revenue")).ToString("#,###,##0.##") %>' Visible="false" Width="85%"></asp:TextBox>
                                            <asp:RequiredFieldValidator ID="rfvRevenue" runat="server" ControlToValidate="txtRevenue"
                                                EnableClientScript="false" Text="*" ErrorMessage="Projected Revenue is Required" ToolTip="Projected Revenue is Required."
                                                ValidationGroup="MarginExceptionThresholdEdit"></asp:RequiredFieldValidator>
                                            <asp:CompareValidator ID="compEditRevenue" runat="server" ControlToValidate="txtRevenue"
                                                Display="Dynamic" EnableClientScript="false" ErrorMessage="A number with 2 decimal digits is allowed for the Revenue."
                                                Operator="DataTypeCheck" SetFocusOnError="true" Text="*" ToolTip="A number with 2 decimal digits is allowed for the Revenue."
                                                Type="Currency" ValidationGroup="MarginExceptionThresholdEdit"></asp:CompareValidator>
                                            <AjaxControlToolkit:FilteredTextBoxExtender ID="FilteredTextBoxExtender3" runat="server" TargetControlID="txtRevenue"
                                                FilterMode="ValidChars" FilterType="Custom,Numbers" ValidChars=".">
                                            </AjaxControlToolkit:FilteredTextBoxExtender>
                                        </td>
                                        <td class="textCenter">
                                            <asp:HiddenField ID="hidKey" runat="server" Value='<%# Eval("Id") %>' />
                                            <asp:ImageButton ID="imgDeleteExceptionThreshold" runat="server" ImageUrl="~/Images/icon-delete.png" Visible="false"
                                                OnClick="imgDeleteExceptionThreshold_Click" ToolTip="Delete Margin Threshold" />
                                        </td>
                                    </tr>
                                </ItemTemplate>

                                <FooterTemplate>
                                    </tbody></table></div>
                                </FooterTemplate>
                            </asp:Repeater>
                        </div>
                        <asp:ValidationSummary ID="valsMarginExceptionThreshold" runat="server" EnableClientScript="false"
                            CssClass="ApplyStyleForDashBoardLists" ValidationGroup="MarginExceptionThreshold" />
                        <asp:ValidationSummary ID="valsMarginExceptionThresholdEdit" runat="server" EnableClientScript="false"
                            CssClass="ApplyStyleForDashBoardLists" ValidationGroup="MarginExceptionThresholdEdit" />
                    </contenttemplate>
                </ajaxToolkit:TabPanel>
            </AjaxControlToolkit:TabContainer>
        </ContentTemplate>

    </asp:UpdatePanel>
</asp:Content>


