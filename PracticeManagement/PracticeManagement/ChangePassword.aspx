﻿<%@ Page Language="C#" MasterPageFile="~/PracticeManagementMain.Master" AutoEventWireup="true"
    CodeBehind="ChangePassword.aspx.cs" Inherits="PraticeManagement.ChangePassword"
    Title="Change User Password | Practice Management" %>

<%@ Register Src="~/Controls/MessageLabel.ascx" TagName="MessageLabel" TagPrefix="uc" %>
<asp:Content ID="cntTitle" ContentPlaceHolderID="title" runat="server">
    <title>Change User Password | Practice Management</title>
</asp:Content>
<asp:Content ID="cntHeader" ContentPlaceHolderID="header" runat="server">
    Change User Password
</asp:Content>
<asp:Content ID="cntBody" ContentPlaceHolderID="body" runat="server">
    <script src="Scripts/jquery.blockUI.min.js" type="text/javascript"></script>
    <script type="text/javascript">
        $(document).ready(function () {
            hdnAreCredentialssaved = document.getElementById('<%= hdnAreCredentialssaved.ClientID %>');
            if (hdnAreCredentialssaved.value == 'true') {
                $.blockUI({ message: '<div class="Padding5">New password successfully saved.  Redirecting to landing page...</div>', css: { width: '380px'} });
                setTimeout('__doPostBack("__Page", "")', 2000);
            }
        }
    );
    </script>
    <div class="DivLogin">
        <asp:ChangePassword ID="changePassword" runat="server" MembershipProvider="PracticeManagementMembershipProvider"
            ContinueDestinationPageUrl="~/" OnChangingPassword="changePassword_OnChangingPassword"
            ChangePasswordFailureText="New Password should be should be at least {0} characters and should contain at least {1} non-alphanumeric characters (e.g. !, @, #, $ etc).">
        </asp:ChangePassword>
    </div>
    <div class="DivLogin">
        <uc:MessageLabel ID="msglblchangePasswordDetails" runat="server" ErrorColor="Red"
            InfoColor="Green" WarningColor="Orange" EnableViewState="false" />
    </div>
    <asp:HiddenField ID="hdnAreCredentialssaved" runat="server" Value="false" />
</asp:Content>

