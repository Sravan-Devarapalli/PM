﻿<%@ Page Title="" Language="C#" MasterPageFile="~/PracticeManagementMain.Master"
    AutoEventWireup="true" CodeBehind="Branding.aspx.cs" Inherits="PraticeManagement.Config.Branding" %>

<%@ Register Src="~/Controls/BrandingSettingsControl.ascx" TagName="BrandingLogo" TagPrefix="uc" %>
<asp:Content ID="Content1" ContentPlaceHolderID="title" runat="server">
    <title>Branding/Logo | Practice Management</title>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="header" runat="server">
    Branding/Logo
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="body" runat="server">
    <uc:BrandingLogo ID="brandingLogo" runat="server" />
</asp:Content>

