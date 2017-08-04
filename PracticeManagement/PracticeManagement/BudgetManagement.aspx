<%@ Page Language="C#" MasterPageFile="~/PracticeManagementMain.Master" AutoEventWireup="true" CodeBehind="BudgetManagement.aspx.cs"
    Inherits="PraticeManagement.BudgetManagement" EnableEventValidation="false" EnableViewState="true" Title="Budget Management" %>

<%@ Register Src="~/Controls/Projects/ProjectSummary.ascx" TagName="ProjectSummary"
    TagPrefix="PS" %>
<%@ Import Namespace="PraticeManagement.Utils" %>

<asp:Content ID="title" ContentPlaceHolderID="title" runat="server">
    <title>Budget Management Summary | Practice Management</title>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="head" runat="server">
    <script src="<%# Generic.GetClientUrl("~/Scripts/ScrollinDropDown.min.js", this) %>"
        type="text/javascript"></script>
</asp:Content>
<asp:Content ID="cntHeader" ContentPlaceHolderID="header" runat="server">
    Budget Management
</asp:Content>
<asp:Content ID="cntBody" ContentPlaceHolderID="body" runat="server">
    <PS:ProjectSummary ID="projectSummary" runat="server" />
</asp:Content>

