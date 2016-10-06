﻿<%@ Page Language="C#" MasterPageFile="~/PracticeManagementMain.Master" AutoEventWireup="true" CodeBehind="AccessDenied.aspx.cs" Inherits="PraticeManagement.AccessDenied" Title="Access was denied" %>
<asp:Content ID="cntTitle" ContentPlaceHolderID="title" runat="server">
	<title>Access was denied | Practice Management</title>
</asp:Content>
<asp:Content ID="cntHeader" ContentPlaceHolderID="header" runat="server">
	Access was denied
</asp:Content>
<asp:Content ID="cntBody" ContentPlaceHolderID="body" runat="server">
	<h2>An access to the requested resource was denied.</h2>
	<p>Please try to click <asp:HyperLink ID="lnkAppRoot" runat="server" Text="here"/> to navigate to your Dashboard page.
	If you believe you should have permission to this page, Contact your system administrator.</p>
</asp:Content>

