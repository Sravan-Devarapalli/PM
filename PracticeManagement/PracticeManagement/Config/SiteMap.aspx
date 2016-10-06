﻿<%@ Page Title="" Language="C#" MasterPageFile="~/PracticeManagementMain.Master"
    AutoEventWireup="true" CodeBehind="SiteMap.aspx.cs" Inherits="PraticeManagement.Config.SiteMap" %>

<asp:Content ID="Content1" ContentPlaceHolderID="title" runat="server">
    <title>Site Map | Practice Management</title>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="header" runat="server">
    Site Map
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="body" runat="server">    
    <asp:Menu ID="menu" runat="server" DataSourceID="odsSiteMap" StaticDisplayLevels="4" OnMenuItemDataBound="menu_OnMenuItemDataBound" />
    <asp:SiteMapDataSource ID="odsSiteMap" runat="server" />
</asp:Content>

