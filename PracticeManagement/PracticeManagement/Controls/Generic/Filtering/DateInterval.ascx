﻿<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="DateInterval.ascx.cs"
    Inherits="PraticeManagement.Controls.Generic.Filtering.DateInterval1" %>
<table class="no-wrap fontsize100P">
    <tr>
        <td class="no-wrap">
            &nbsp;<asp:Label ID="lblFrom" runat="server"></asp:Label>&nbsp;
        </td>
        <td>
            <asp:TextBox ID="tbFrom" runat="server" CssClass="MarginClass width50Px" />&nbsp;&nbsp;
        </td>
        <td class="vMiddle">
            <asp:HyperLink ID="lnkCalendarFrom" runat="server" ImageUrl="~/Images/calendar.gif"
                NavigateUrl="#"></asp:HyperLink>
        </td>
        <td>
            <asp:RequiredFieldValidator ID="reqValFrom" runat="server" ControlToValidate="tbFrom"
                Enabled="false" ErrorMessage="From Date is Required." ToolTip="From Date Is Required."
                ValidationGroup='<%# ClientID %>'>*</asp:RequiredFieldValidator>
            <asp:RegularExpressionValidator ID="valFrom" runat="server" ControlToValidate="tbFrom"
                ToolTip="The Date has an incorrect format. It must be 'MM/dd/yyyy'." ErrorMessage="The Date has an incorrect format. It must be 'MM/dd/yyyy'."
                ValidationExpression="^([1-9]|0[1-9]|1[012])[/]([1-9]|0[1-9]|[12][0-9]|3[01])[/][0-9]{4}$"
                ValidationGroup='<%# ClientID %>'>*</asp:RegularExpressionValidator>
            <asp:RangeValidator ID="rangeValFrom" runat="server" ControlToValidate="tbFrom" ErrorMessage="From date should be between 1/1/1985 and 12/31/2100"
                MaximumValue="12/31/2100" MinimumValue="1/1/1985" ValidationGroup='<%# ClientID %>'
                Display="Dynamic" ToolTip="From date should be between 1/1/1985 and 12/31/2100"
                Type="Date">*</asp:RangeValidator>
        </td>
        <td class="no-wrap">
            &nbsp;<asp:Label ID="lblTo" runat="server"></asp:Label>&nbsp;
        </td>
        <td>
            <asp:TextBox ID="tbTo" runat="server" CssClass="MarginClass width50Px" />&nbsp;&nbsp;
        </td>
        <td class="vMiddle">
            <asp:HyperLink ID="lnkCalendarTo" runat="server" ImageUrl="~/Images/calendar.gif"
                NavigateUrl="#"></asp:HyperLink>
        </td>
        <td>
            <asp:RequiredFieldValidator ID="reqValTo" runat="server" ControlToValidate="tbTo"
                Display="Dynamic" Enabled="false" ErrorMessage="To Date is Required." ToolTip="To Date Is Required."
                ValidationGroup='<%# ClientID %>'>*</asp:RequiredFieldValidator>
            <asp:RegularExpressionValidator ID="valTo" runat="server" ControlToValidate="tbTo"
                Display="Dynamic" ToolTip="The Date has an incorrect format. It must be 'MM/dd/yyyy'."
                ErrorMessage="The Date has an incorrect format. It must be 'MM/dd/yyyy'." ValidationExpression="^([1-9]|0[1-9]|1[012])[/]([1-9]|0[1-9]|[12][0-9]|3[01])[/][0-9]{4}$"
                ValidationGroup='<%# ClientID %>'>*</asp:RegularExpressionValidator>
            <asp:RangeValidator ID="rangeValTo" runat="server" ControlToValidate="tbTo" ErrorMessage="To date should be between 1/1/1985 and 12/31/2100"
                ToolTip="To date should be between 1/1/1985 and 12/31/2100" MaximumValue="12/31/2100"
                Display="Dynamic" MinimumValue="1/1/1985" ValidationGroup='<%# ClientID %>' Type="Date">*</asp:RangeValidator>
            <asp:CompareValidator ID="compToDate" runat="server" ControlToValidate="tbTo" ControlToCompare="tbFrom"
                ErrorMessage="To date must be greater or equal to the from date." ToolTip="To date must be greater or equal to the from date."
                Operator="GreaterThanEqual" Type="Date" ValidationGroup='<%# ClientID %>'>*</asp:CompareValidator>
        </td>
    </tr>
</table>
<AjaxControlToolkit:CalendarExtender ID="clFromDate" runat="server" BehaviorID="bhclFromDate"
    Format='<%# PraticeManagement.Constants.Formatting.EntryDateFormat %>' PopupPosition="BottomLeft"
    PopupButtonID="lnkCalendarFrom" TargetControlID="tbFrom" />
<AjaxControlToolkit:CalendarExtender ID="clToDate" runat="server" BehaviorID="bhclToDate"
    Format='<%# PraticeManagement.Constants.Formatting.EntryDateFormat %>' PopupPosition="BottomLeft"
    PopupButtonID="lnkCalendarTo" TargetControlID="tbTo" />

