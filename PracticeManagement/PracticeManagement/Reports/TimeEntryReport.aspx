﻿<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="TimeEntryReport.aspx.cs" Inherits="PraticeManagement.TimeEntryReport" MasterPageFile="~/PracticeManagementMain.Master"
    Title="Generic Time Entry Report | Practice Management" %>

<%@ Register Src="~/Controls/TimeEntry/TimeEntryManagement.ascx" TagPrefix="uc" TagName="TeManage" %>
<asp:Content ID="cntTitle" ContentPlaceHolderID="title" runat="server">
    <title>Generic Time Entry Report | Practice Management</title>
</asp:Content>
<asp:Content ID="cntHeader" ContentPlaceHolderID="header" runat="server">
    Time Entry Report
</asp:Content>
<asp:Content ID="cntBody" ContentPlaceHolderID="body" runat="server">
<script language="javascript" type="text/javascript" src="../Scripts/ScrollinDropDown.js"></script>
    <uc:TeManage ID="teManage" runat="server" />
</asp:Content>
