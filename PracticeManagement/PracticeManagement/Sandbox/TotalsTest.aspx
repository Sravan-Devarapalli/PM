<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="TotalsTest.aspx.cs" Inherits="PraticeManagement.Sandbox.TotalsTest" MasterPageFile="~/PracticeManagementMain.Master"%>

<%@Register TagPrefix="ext" Assembly="PraticeManagement" Namespace="PraticeManagement.Controls.Generic.TotalCalculator" %>

<asp:Content ID="cntTitle" ContentPlaceHolderID="title" runat="server">
    <title>Opportunity Details | Practice Management</title>
</asp:Content>
<asp:Content ID="cntHeader" ContentPlaceHolderID="header" runat="server">
    Opportunity Details
</asp:Content>
<asp:Content ID="cntBody" ContentPlaceHolderID="body" runat="server">
    <div id="div" runat="server">        
        <table id="testTable" style="width: 100%;" runat="server">            
        </table>        
    </div>
</asp:Content>