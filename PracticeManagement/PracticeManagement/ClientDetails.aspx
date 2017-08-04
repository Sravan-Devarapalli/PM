<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ClientDetails.aspx.cs"
    Inherits="PraticeManagement.ClientDetails" Title="Account Details | Practice Management"
    MasterPageFile="~/PracticeManagementMain.Master" %>

<%@ Import Namespace="PraticeManagement.Utils" %>
<%@ Register TagPrefix="cc2" Assembly="PraticeManagement" Namespace="PraticeManagement.Controls" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajaxToolkit" %>
<%@ Register TagPrefix="cc" Namespace="PraticeManagement.Controls.Generic.Buttons"
    Assembly="PraticeManagement" %>
<%@ Register Src="~/Controls/Clients/ClientProjects.ascx" TagName="ClientProjects"
    TagPrefix="uc" %>
<%@ Register Src="~/Controls/Clients/ClientGroups.ascx" TagName="ClientGroups" TagPrefix="uc" %>
<%@ Register Src="~/Controls/Clients/ClientPricingList.ascx" TagName="ClientPricingList"
    TagPrefix="uc" %>
<%@ Register Src="~/Controls/Clients/ClientBusinessGroups.ascx" TagName="ClientBusinessGroups"
    TagPrefix="uc" %>
<%@ Register Src="~/Controls/MessageLabel.ascx" TagName="Label" TagPrefix="uc" %>
<%@ Register TagPrefix="uc" TagName="LoadingProgress" Src="~/Controls/Generic/LoadingProgress.ascx" %>
<%@ Register Src="~/Controls/Clients/ClientMarginGoals.ascx" TagName="ClientMarginGoals"
    TagPrefix="uc" %>
<asp:Content ID="cntTitle" ContentPlaceHolderID="title" runat="server">
</asp:Content>
<asp:Content ID="cntHeader" ContentPlaceHolderID="head" runat="server">
    <link href="<%# Generic.GetClientUrl("~/Css/TableSortStyle.min.css", this) %>" rel="stylesheet"
        type="text/css" />
</asp:Content>
<asp:Content ID="cntBody" ContentPlaceHolderID="body" runat="server">
    <script src="Scripts/jquery.tablesorter.min.js" type="text/javascript"></script>
    <script type="text/javascript">
        $(document).ready(function () {

            $("#tblAccountSummaryByProject").tablesorter({
                sortList: [[0, 0]],
                sortForce: [[0, 0]]
            });
        });
        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(endRequestHandle);
        function endRequestHandle(sender, Args) {
            $("#tblAccountSummaryByProject").tablesorter({
                sortList: [[0, 0]],
                sortForce: [[0, 0]]
            });
        }
        function setClassForAddProject() {
            var button = document.getElementById("<%= btnAddProject.ClientID%>");
            var chbActive = document.getElementById("<%= chbActive.ClientID%>");
            if (!chbActive.checked) {
                button.disabled = "disabled";
                button.className = "darkadd-btn-project";
            }
            else {
                button.disabled = "";
                button.className = "add-btn-project";
            }
        }
        function checkhdnchbActive() {
            var hdnchbActive = document.getElementById("<%= hdnchbActive.ClientID%>");
            if (hdnchbActive.value == "true") {
                return true;
            }
            else {
                return false;
            }
        }
        function checkDirty(mpId) {
            if (showDialod()) {
                __doPostBack('__Page', mpId);
                return true;
            }
            return false;
        }

        function applyColor(ddlColor) {
            for (var i = 0; i < ddlColor.length; i++) {
                if (ddlColor[i].selected) {
                    if (ddlColor[i].attributes["colorvalue"] != null && ddlColor[i].attributes["colorvalue"] != "undefined") {
                        ddlColor.style.backgroundColor = ddlColor[i].attributes["colorvalue"].value;
                    }
                    break;
                }
            }
        }

        function SetBackGroundColorForDdls() {
            var list = document.getElementsByTagName('select');

            for (var j = 0; j < list.length; j++) {
                applyColor(list[j]);
            }
        }

        $(document).ready(function () {
            $(window).keydown(function (event) {
                if (event.keyCode == 13) {
                    event.preventDefault();
                    return false;
                }
            });
        });

        window.onload = SetBackGroundColorForDdls;

    </script>
    <uc:LoadingProgress ID="LoadingProgress1" runat="server" />
    <asp:UpdatePanel ID="upClientPage" runat="server">
        <ContentTemplate>
            <div class="filters tab-pane">
                <table>
                    <tr>
                        <td>Account Name
                        </td>
                        <td class="Width15Px"></td>
                        <td class="PaddingBottom5">
                            <asp:TextBox ID="txtClientName" runat="server" onchange="setDirty();" Width="189px"
                                ValidationGroup="Client" />
                        </td>
                        <td>
                            <asp:RequiredFieldValidator ID="reqClientName" runat="server" ControlToValidate="txtClientName"
                                ErrorMessage="The Account Name is required." ToolTip="The Account Name is required."
                                Text="*" SetFocusOnError="True" EnableClientScript="False" ValidationGroup="Client" />
                            <asp:CustomValidator ID="custClientName" runat="server" ControlToValidate="txtClientName"
                                ErrorMessage="There is another Account with the same Name." ToolTip="There is another Account with the same Name."
                                Text="*" EnableClientScript="false" SetFocusOnError="true" Display="Dynamic"
                                OnServerValidate="custClientName_ServerValidate" ValidationGroup="Client" />
                            <asp:CustomValidator ID="custClient" runat="server" ControlToValidate="txtClientName"
                                ErrorMessage="An error occurs during saving the data. Please contact your administrator."
                                ToolTip="An error occurs during saving the data. Please contact your administrator."
                                Text="*" EnableClientScript="false" SetFocusOnError="true" Display="Dynamic"
                                OnServerValidate="custClient_ServerValidate" ValidationGroup="Client" />
                        </td>
                        <td class="Width15Px"></td>
                        <td>Account Active?
                        </td>
                        <td>
                            <asp:CheckBox ID="chbActive" runat="server" Checked="true" onclick="setDirty();setClassForAddProject();" />
                            <asp:HiddenField ID="hdnchbActive" runat="server" />
                        </td>
                        <td></td>
                    </tr>
                    <tr>
                        <td>Default Salesperson
                        </td>
                        <td class="Width15Px"></td>
                        <td class="PaddingBottom5">
                            <asp:DropDownList ID="ddlDefaultSalesperson" runat="server" onchange="setDirty();"
                                CssClass="Width194px" ValidationGroup="Client" />
                        </td>
                        <td>
                            <asp:RequiredFieldValidator ID="reqDefaultSalesperson" runat="server" ControlToValidate="ddlDefaultSalesperson"
                                ErrorMessage="The Default Salesperson is required." ToolTip="The Default Salesperson is required."
                                Text="*" SetFocusOnError="True" EnableClientScript="False" ValidationGroup="Client" />
                        </td>
                        <td class="Width15Px"></td>
                        <td>House Account?
                        </td>
                        <td>
                            <asp:CheckBox ID="chbHouseAccount" runat="server" Checked="true" onclick="setDirty();setClassForAddProject();" />
                        </td>
                        <td></td>
                    </tr>
                    <tr>
                        <td>Default Client Director
                        </td>
                        <td class="Width15Px"></td>
                        <td colspan="2" class="PaddingBottom5">
                            <asp:DropDownList ID="ddlDefaultDirector" runat="server" onchange="setDirty();" CssClass="Width194px"
                                ValidationGroup="Client" />
                        </td>
                        <td class="Width15Px"></td>
                        <td>Are projects billable?
                        </td>
                        <td>
                            <asp:CheckBox ID="chbIsChar" runat="server" Checked="true" onclick="setDirty();"
                                ToolTip="Projects for this account are billable by default." />
                        </td>
                        <td></td>
                    </tr>
                    <tr>
                        <td>Default Discount
                        </td>
                        <td class="Width15Px"></td>
                        <td class="PaddingBottom5">
                            <asp:TextBox ID="txtDefaultDiscount" runat="server" onchange="setDirty();" Width="189px"
                                ValidationGroup="Client" />%
                        </td>
                        <td>
                            <asp:RequiredFieldValidator ID="reqDefaultDiscount" runat="server" ControlToValidate="txtDefaultDiscount"
                                ErrorMessage="The Default Discount is required." ToolTip="The Default Discount is required."
                                Text="*" SetFocusOnError="True" EnableClientScript="False" Display="Dynamic"
                                ValidationGroup="Client" />
                            <asp:CompareValidator ID="compDefaultDiscount" runat="server" ControlToValidate="txtDefaultDiscount"
                                ErrorMessage="A number with 2 decimal digits is allowed for the Default Discount."
                                ToolTip="A number with 2 decimal digits is allowed the Default Discount." Text="*"
                                SetFocusOnError="true" EnableClientScript="false" Display="Dynamic" Operator="DataTypeCheck"
                                Type="Currency" ValidationGroup="Client" />
                            <asp:RangeValidator ID="rvDefaultDiscount" runat="server" ControlToValidate="txtDefaultDiscount"
                                ErrorMessage="Default discount should be between 0 and 100" MinimumValue="0"
                                MaximumValue="100" ToolTip="Default discount should be between 0 and 100" EnableClientScript="false"
                                Display="Dynamic" Text="*" SetFocusOnError="true" Type="Currency" ValidationGroup="Client"></asp:RangeValidator>
                        </td>
                        <td class="Width15Px"></td>
                        <td>Is Account Internal?
                        </td>
                        <td>
                            <asp:CheckBox ID="chbIsInternal" runat="server" Checked="false" Enabled="false" />
                        </td>
                        <td></td>
                    </tr>
                    <tr>
                        <td>Terms
                        </td>
                        <td class="Width15Px"></td>
                        <td class="PaddingBottom5">
                            <asp:DropDownList ID="ddlDefaultTerms" runat="server" onchange="setDirty();" DataTextField="Name"
                                DataValueField="Frequency" AppendDataBoundItems="true" CssClass="Width194px">
                                <asp:ListItem Text=""></asp:ListItem>
                            </asp:DropDownList>
                        </td>
                        <td></td>
                        <td class="Width15Px"></td>
                        <td>Is Note Required?
                        </td>
                        <td>
                            <asp:CheckBox ID="chbIsNoteRequired" runat="server" Checked="true" />
                        </td>
                        <td></td>
                    </tr>
                </table>
                <div class="PaddingTop10Px">
                    <uc:Label ID="mlConfirmation" runat="server" ErrorColor="Red" InfoColor="Green" WarningColor="Orange" />
                </div>

            </div>
            <div class="buttons-block Margin-Bottom10Px">
                <div>
                    <asp:ValidationSummary ID="vsumClient" runat="server" ValidationGroup="Client" />
                </div>
                <asp:HiddenField ID="hdnClientId" runat="server" />
                <cc:CancelAndReturnButton ID="btnCancelAndReturn" runat="server" UseSubmitBehavior="false" />
                &nbsp;
                <asp:Button ID="btnSave" runat="server" Text="Save" OnClick="btnSave_Click" CssClass="pm-button"
                    ValidationGroup="Client" />
            </div>
            <asp:ImageButton ImageUrl="~/Images/add-project.png" runat="server" ID="btnAddProject"
                ValidationGroup="Client" CssClass="add-btn-project" OnClick="btnAddProject_Click" />
            <AjaxControlToolkit:TabContainer ID="tcFilters" runat="server" ActiveTabIndex="0"
                CssClass="CustomTabStyle">
                <ajaxToolkit:TabPanel runat="server" ID="tpProjects">
                    <HeaderTemplate>
                        <span class="bg">
                            <asp:LinkButton ID="lnkProjects" Text="Projects" runat="server" OnClick="lnkProjects_Click"></asp:LinkButton>
                        </span>
                    </HeaderTemplate>
                    <ContentTemplate>
                        <div class="project-filter">
                            <asp:UpdatePanel ID="UpdatePanel1" runat="server">
                                <ContentTemplate>
                                    <uc:ClientProjects ID="ucProjects" runat="server" />
                                </ContentTemplate>
                            </asp:UpdatePanel>
                        </div>
                    </ContentTemplate>
                </ajaxToolkit:TabPanel>
                <ajaxToolkit:TabPanel runat="server" ID="tpGroups">
                    <HeaderTemplate>
                        <span class="bg">
                            <asp:LinkButton ID="lnkBusinessUnit" Text="Business Units" runat="server" OnClick="lnkBusinessUnit_Click"></asp:LinkButton>
                        </span>
                    </HeaderTemplate>
                    <ContentTemplate>
                        <div class="project-filter">
                            <asp:UpdatePanel ID="updGroups" runat="server">
                                <ContentTemplate>
                                    <uc:ClientGroups ID="ucProjectGoups" runat="server" />
                                </ContentTemplate>
                            </asp:UpdatePanel>
                        </div>
                    </ContentTemplate>
                </ajaxToolkit:TabPanel>
                <ajaxToolkit:TabPanel runat="server" ID="tpBusinessGroup">
                    <HeaderTemplate>
                        <span class="bg">
                            <asp:LinkButton ID="lnkBusinessGroup" Text="Business Group" runat="server" OnClick="lnkBusinessGroup_Click"></asp:LinkButton>
                        </span>
                    </HeaderTemplate>
                    <ContentTemplate>
                        <div class="project-filter">
                            <asp:UpdatePanel ID="UpdatePanel2" runat="server">
                                <ContentTemplate>
                                    <uc:ClientBusinessGroups ID="ucBusinessGroups" runat="server" />
                                </ContentTemplate>
                            </asp:UpdatePanel>
                        </div>
                    </ContentTemplate>
                </ajaxToolkit:TabPanel>
                <ajaxToolkit:TabPanel runat="server" ID="tpPricingList">
                    <HeaderTemplate>
                        <span class="bg">
                            <asp:LinkButton ID="lnkPricingList" Text="Pricing List" runat="server" OnClick="lnkPricingList_Click"> </asp:LinkButton></span>
                    </HeaderTemplate>
                    <ContentTemplate>
                        <div class="project-filter">
                            <asp:UpdatePanel ID="UpdatePanel3" runat="server">
                                <ContentTemplate>
                                    <uc:ClientPricingList ID="ucPricingList" runat="server" />
                                </ContentTemplate>
                            </asp:UpdatePanel>
                        </div>
                    </ContentTemplate>
                </ajaxToolkit:TabPanel>
                <ajaxToolkit:TabPanel runat="server" ID="tpMarginGoal">
                    <HeaderTemplate>
                        <span class="bg">
                            <asp:LinkButton ID="lnkMarginGoal" Text="Margin Goals" runat="server" OnClick="lnkMarginGoal_Click"> </asp:LinkButton></span>
                    </HeaderTemplate>
                    <ContentTemplate>
                        <div class="project-filter">
                            <asp:UpdatePanel ID="UpdatePanel4" runat="server">
                                <ContentTemplate>
                                    <uc:ClientMarginGoals ID="ucMarginGoals" runat="server" />
                                </ContentTemplate>
                            </asp:UpdatePanel>

                        </div>
                    </ContentTemplate>
                </ajaxToolkit:TabPanel>
            </AjaxControlToolkit:TabContainer>
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>

