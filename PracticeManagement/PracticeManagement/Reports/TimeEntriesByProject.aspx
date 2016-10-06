﻿<%@ Page Title="" Language="C#" MasterPageFile="~/PracticeManagementMain.Master" AutoEventWireup="true" CodeBehind="TimeEntriesByProject.aspx.cs" Inherits="PraticeManagement.Sandbox.TimeEntriesByProjectReport" %>
<%@ Register Src="~/Controls/TimeEntry/TimeEntriesByProject.ascx" TagName="TimeEntriesByProject" TagPrefix="uc" %>
<asp:Content ID="Content1" ContentPlaceHolderID="title" runat="server">
    <title>Time Entry by Project | Practice Management</title>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="header" runat="server">
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="body" runat="server">
    <uc:TimeEntriesByProject ID="tebp" runat="server" />
</asp:Content>
<asp:Content ID="Content5" ContentPlaceHolderID="footer" runat="server">
</asp:Content>

