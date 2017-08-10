<%@ Page Language="C#" MasterPageFile="~/PracticeManagementMain.Master" AutoEventWireup="True"
    CodeBehind="ProjectDetail.aspx.cs" Inherits="PraticeManagement.ProjectDetail"
    Title="Project Details | Practice Management" EnableEventValidation="false" ValidateRequest="False" %>

<%@ Import Namespace="PraticeManagement.Utils" %>
<%@ Register Src="~/Controls/Projects/ProjectActivityLog.ascx" TagPrefix="uc" TagName="ActivityLogControl" %>
<%@ Register TagPrefix="extDisable" Namespace="PraticeManagement.Controls.Generic.ElementDisabler"
    Assembly="PraticeManagement" %>
<%@ Register TagPrefix="asp" Namespace="PraticeManagement.Controls.Generic.Buttons"
    Assembly="PraticeManagement" %>
<%@ Register TagPrefix="uc" Src="~/Controls/Projects/ProjectMilestonesFinancials.ascx"
    TagName="ProjectMilestonesFinancials" %>
<%@ Register TagPrefix="uc" Src="~/Controls/Projects/ProjectTimeTypes.ascx" TagName="ProjectTimeTypes" %>
<%@ Register TagPrefix="uc" Src="~/Controls/Projects/CommissionsAttribution.ascx"
    TagName="ProjectAttribution" %>
<%@ Register TagPrefix="uc" Src="~/Controls/Projects/ProjectFeedback.ascx" TagName="ProjectFeedback" %>
<%@ Register Src="Controls/ProjectExpenses/ProjectExpensesControl.ascx" TagName="ProjectExpenses"
    TagPrefix="uc2" %>
<%@ Register Src="Controls/DatePicker.ascx" TagName="DatePicker" TagPrefix="ucDate" %>
<%@ Register TagPrefix="uc" Src="~/Controls/Projects/ProjectFinancials.ascx" TagName="ProjectFinancials" %>
<%@ Register TagPrefix="uc" Src="~/Controls/Projects/ProjectPersons.ascx" TagName="ProjectPersons" %>
<%@ Register TagPrefix="uc" Src="~/Controls/Projects/ProjectCSAT.ascx" TagName="ProjectCSAT" %>
<%@ Register Src="~/Controls/MessageLabel.ascx" TagName="Label" TagPrefix="uc" %>
<%@ Register Src="~/Controls/Generic/LoadingProgress.ascx" TagName="LoadingProgress"
    TagPrefix="uc" %>
<%@ Register TagPrefix="uc" Src="~/Controls/Projects/BudgetManagementByProject.ascx" TagName="BudgetManagementByProject" %>
<%@ Register TagPrefix="ext" Assembly="PraticeManagement" Namespace="PraticeManagement.Controls.Generic.ScrollableDropdown" %>
<%@ Register TagPrefix="cc2" Assembly="PraticeManagement" Namespace="PraticeManagement.Controls" %>
<asp:Content ID="cntTitle" ContentPlaceHolderID="title" runat="server">
    <title>Project Details | Practice Management</title>
</asp:Content>
<asp:Content ID="cntHeader" ContentPlaceHolderID="header" runat="server">
    Project Details
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="head" runat="server">
    <script src="<%# Generic.GetClientUrl("~/Scripts/ScrollinDropDown.min.js", this) %>"
        type="text/javascript"></script>
</asp:Content>
<asp:Content ID="cntBody" ContentPlaceHolderID="body" runat="server">
    <script src="Scripts/jquery.tablesorter.min.js" type="text/javascript"></script>
    <script src="Scripts/FilterTable.min.js" type="text/javascript"></script>
    <script src="Scripts/jquery.uploadify.min.js?Id=21" type="text/javascript"></script>
    <script src="Scripts/jquery.tablesorter.staticrow.min.js" type="text/javascript"></script>
    <uc:LoadingProgress ID="loadingProgress" runat="server" />
    <asp:UpdatePanel ID="upnlBody" runat="server">
        <ContentTemplate>
            <script type="text/javascript">

                var currentSort = [[1, 0]];
                var rbnBudgetResetTypeId = "<%=rbnBudgetResetType.ClientID%>";
                var hdnProjectNumberId = "<%= hdnProjectNumber.ClientID%>";
                var fuAttachmentsUploadId = "<%=fuAttachmentsUpload.ClientID%>";
                var uploadedFilesId = "<%= uploadedFiles.ClientID%>";
                var lblUplodedFilesMsgId = "<%= lblUplodedFilesMsg.ClientID%>";
                var btnUploadId = '<%= btnUpload.ClientID %>';
                var loadingProgressId = "<%= loadingProgress.ClientID %>";
                var btnCancelId = '<%= btnCancel.ClientID %>';
                var ddlAttachmentCategoryId = '<%= ddlAttachmentCategory.ClientID %>';
                var hdnProjectIdId = '<%= hdnProjectId.ClientID %>';
                var UserIdentity = "<%= User.Identity.Name %>";
                var btnDownloadButtonId = "<%= btnDownloadButton.ClientID %>";
                var hdnDownloadAttachmentIdId = "<%= hdnDownloadAttachmentId.ClientID %>";
                var hdnIsMarginExceptionId = "<%= hdnIsMarginException.ClientID %>";
                var ddlProjectStatusId = "<%= ddlProjectStatus.ClientID %>";
                var hdnProjectDeleteId = "<%= hdnProjectDelete.ClientID %>";
                var activityLogId = "<%= activityLog.ClientID %>";
                var tblSetProjectNumberId = "<%= tblSetProjectNumber.ClientID%>";
                var hdnVisibiltyId = "<%= hdnVisibilty.ClientID%>";
                var btnDeleteWorkTypeId = "<%= btnDeleteWorkType.ClientID%>";
                var hdnWorkTypeIdId = "<%= hdnWorkTypeId.ClientID%>";
                var imgNavigateToProjectId = "<%= imgNavigateToProject.ClientID %>";
            </script>
            <script src="Scripts/ProjectDetail.js" type="text/javascript"></script>
            <asp:HiddenField ID="hdnProjectNumber" runat="server" />
            <asp:Table ID="tblProjectTypeViewSwitch" runat="server" CssClass="CustomTabStyle CustomTabStyleProjectDetail">
                <asp:TableRow ID="ProjectTypeSitch" runat="server">
                    <asp:TableCell ID="CellExternal" runat="server" CssClass="SelectedSwitch">
                        <span class="bg"><span>
                            <asp:LinkButton ID="btnExternalPRoject" runat="server" Text="External Project" CausesValidation="false"
                                OnCommand="btnProjectView_Command" CommandArgument="0"></asp:LinkButton></span>
                        </span>
                    </asp:TableCell>
                    <asp:TableCell ID="CellInternal" runat="server">
                        <span class="bg"><span>
                            <asp:LinkButton ID="btnInternalProject" runat="server" Text="Internal Project" CausesValidation="false"
                                OnCommand="btnProjectView_Command" CommandArgument="1"></asp:LinkButton></span>
                        </span>
                    </asp:TableCell>
                </asp:TableRow>
            </asp:Table>
            <asp:MultiView ID="mvProjectType" runat="server" ActiveViewIndex="0">
                <asp:View ID="vwExternal" runat="server">
                    <br />
                    <table class="WholeWidth">
                        <tr>
                            <td class="Width2Percent">
                                <a id="mailtoHiddenLink" class="displayNone"></a>
                            </td>
                            <td>
                                <table class="WholeWidth">
                                    <tr>
                                        <td class="width30P no-wrap">
                                            <asp:TextBox ID="txtProjectNameFirstTime" CssClass="Width500PxImp" runat="server"
                                                Visible="false"></asp:TextBox>
                                            <AjaxControlToolkit:TextBoxWatermarkExtender ID="txtweProjectNameFirstTime" runat="server"
                                                TargetControlID="txtProjectNameFirstTime" WatermarkText="Enter a Project Name here..."
                                                EnableViewState="false" WatermarkCssClass="watermarkedtext Width500PxImp" />
                                            <asp:Label ID="lblProjectNumber" runat="server" CssClass="LabelProject"></asp:Label>
                                            <asp:Label ID="lblProjectName" runat="server" CssClass="LabelProject"></asp:Label>
                                            <asp:Label ID="lblProjectRange" runat="server" CssClass="LabelProject"></asp:Label>
                                            <asp:Image ID="imgEditProjectName" ToolTip="Edit Project Name" ImageUrl="~/Images/icon-edit.png"
                                                runat="server" />
                                            <a href="ProjectHelp.aspx" class="ProjectHelp" target="_blank" title="This field contains the name of the project.&#13;&#10;The name of the project should follow standard naming convention and match the opportunity name in CRM when possible.">*</a>
                                            <asp:CustomValidator ID="cvProjectName" runat="server" ErrorMessage="The Project Name is required."
                                                ToolTip="The Project Name is required." ValidationGroup="Project" Text="*" EnableClientScript="false"
                                                SetFocusOnError="true" Display="Dynamic" OnServerValidate="cvProjectName_ServerValidate"></asp:CustomValidator>
                                            <AjaxControlToolkit:ModalPopupExtender ID="mpeEditProjectName" runat="server" TargetControlID="imgEditProjectName"
                                                CancelControlID="btncloseEditProjectName" BehaviorID="mpeEditProjectName" BackgroundCssClass="modalBackground"
                                                PopupControlID="pnlProjectName" DropShadow="false" />
                                        </td>
                                        <td class="Width35Percent padLeft4Per">
                                            <table id="tblSetProjectNumber" runat="server">
                                                <tr>
                                                    <td>
                                                        <asp:RadioButton ID="rbAutoGenerate" runat="server" GroupName="ProjectNumber" Text="Auto generate project number"
                                                            Checked="true" onchange="showProjectNumberTextBox(2)" />
                                                        <asp:RadioButton ID="rbOwnNumber" runat="server" GroupName="ProjectNumber" Text="Set own project number"
                                                            onchange="showProjectNumberTextBox(1)" />
                                                    </td>
                                                    <td class="padLeft5">
                                                        <asp:TextBox ID="txtProjectNumber" runat="server" Style="display: none"></asp:TextBox>
                                                        &nbsp;
                                                    </td>
                                                    <td>
                                                        <asp:CustomValidator ID="custProjectNumberRequired" runat="server" ErrorMessage="The project number is required."
                                                            ToolTip="The project number is required." ValidationGroup="Project" Text="*"
                                                            EnableClientScript="false" SetFocusOnError="true" Display="Dynamic" OnServerValidate="custProjectNumberRequired_ServerValidate"></asp:CustomValidator>
                                                        <asp:CustomValidator ID="custNumberExistsInSystem" runat="server" ErrorMessage="The project number already exists in the system."
                                                            ToolTip="The project number already exists in the system." ValidationGroup="Project"
                                                            Text="*" EnableClientScript="false" SetFocusOnError="true" Display="Dynamic"
                                                            OnServerValidate="custNumberExistsInSystem_ServerValidate"></asp:CustomValidator>
                                                        <asp:CustomValidator ID="custFormat" runat="server" ErrorMessage="Project number should be in the format P + 6 Digits or P + 6 Digits + 1 Alphabet."
                                                            ToolTip="Project number should be in the format P + 6 Digits or P + 6 Digits + 1 Alphabet."
                                                            ValidationGroup="Project" Text="*" EnableClientScript="false" SetFocusOnError="true"
                                                            Display="Dynamic" OnServerValidate="custFormat_ServerValidate"></asp:CustomValidator>
                                                    </td>
                                                </tr>
                                            </table>
                                        </td>
                                        <td class="Width35Percent">
                                            <table class="WholeWidth">
                                                <tr>
                                                    <td class="LabelProject TdProjectStatus">
                                                        <table>
                                                            <tr>
                                                                <td>Project Status
                                                                </td>
                                                                <td>
                                                                    <div id="divStatus" class="PaddingLeft3Px" runat="server">
                                                                    </div>
                                                                </td>
                                                            </tr>
                                                        </table>
                                                    </td>
                                                    <td class="DdlProjectStatus">
                                                        <div id="divStatusYellow" runat="server">
                                                            <asp:DropDownList ID="ddlProjectStatus" runat="server" onchange=" setDirty();" AutoPostBack="True"
                                                                CssClass="WholeWidth" OnSelectedIndexChanged="DropDown_SelectedIndexChanged">
                                                            </asp:DropDownList>
                                                        </div>
                                                    </td>
                                                    <td class="TdValidators">
                                                        <asp:HiddenField ID="hdnIsMarginException" runat="server" />
                                                        <asp:HiddenField ID="hdnProjectHasRevenue" runat="server" />
                                                        <asp:HiddenField ID="hdnIsRevenueException" runat="server" />
                                                        <asp:RequiredFieldValidator ID="reqProjectStatus" runat="server" ControlToValidate="ddlProjectStatus"
                                                            ErrorMessage="The Project Status is required." ToolTip="The Project Status is required."
                                                            ValidationGroup="Project" Text="*" EnableClientScript="false" SetFocusOnError="true"
                                                            Display="Dynamic"></asp:RequiredFieldValidator>
                                                        <asp:CustomValidator ID="custProjectStatus" runat="server" ControlToValidate="ddlProjectStatus"
                                                            ErrorMessage="Only system administrators can make projects Active or Completed."
                                                            ToolTip="Only system administrators can make projects Active or Completed." ValidationGroup="Project"
                                                            Text="*" EnableClientScript="false" SetFocusOnError="true" Display="Dynamic"
                                                            OnServerValidate="custProjectStatus_ServerValidate"></asp:CustomValidator>
                                                        <asp:CustomValidator ID="custAtRiskStatus" runat="server" ControlToValidate="ddlProjectStatus"
                                                            ErrorMessage="Only system administrators, Operations can make projects At Risk."
                                                            ToolTip="Only system administrators, Operations can make projects At Risk." ValidationGroup="Project"
                                                            Text="*" EnableClientScript="false" SetFocusOnError="true" Display="Dynamic"
                                                            OnServerValidate="custAtRiskStatus_ServerValidate"></asp:CustomValidator>
                                                        <asp:CustomValidator ID="cvIsInternal" runat="server" EnableClientScript="false"
                                                            ErrorMessage="Can not change project status as some work types are already in use."
                                                            ValidateEmptyText="true" Text="*" ToolTip="Can not change project status as some timetypes are already in use."></asp:CustomValidator>
                                                        <asp:CustomValidator ID="custActiveStatus" runat="server" ControlToValidate="ddlProjectStatus"
                                                            ErrorMessage="Project should have milestone added to set status to Active." ToolTip="Project should have milestone added to set status to Active."
                                                            ValidationGroup="Project" Text="*" EnableClientScript="false" SetFocusOnError="true"
                                                            Display="Dynamic" OnServerValidate="custActiveStatus_ServerValidate"></asp:CustomValidator>
                                                        <asp:CustomValidator ID="cvActivestatusWithoutFinance" runat="server" ControlToValidate="ddlProjectStatus"
                                                            ErrorMessage="Project should have revenue to set status to Active." ToolTip="Project should have revenue to set status to Active."
                                                            ValidationGroup="Project" Text="*" EnableClientScript="false" SetFocusOnError="true"
                                                            Display="Dynamic" OnServerValidate="cvActivestatusWithoutFinance_ServerValidate"></asp:CustomValidator>
                                                        <asp:CustomValidator ID="cvMarginException" runat="server" ControlToValidate="ddlProjectStatus"
                                                            ErrorMessage="Project cannot be moved to active without a Margin Exception." ToolTip="Project cannot be moved to active without a Margin Exception."
                                                            ValidationGroup="Project" Text="*" EnableClientScript="false" SetFocusOnError="true"
                                                            Display="Dynamic" OnServerValidate="cvMarginException_ServerValidate"></asp:CustomValidator>
                                                    </td>

                                                </tr>
                                            </table>
                                        </td>
                                    </tr>
                                </table>
                            </td>
                            <td class="Width2Percent"></td>
                        </tr>
                        <tr>
                            <td colspan="3">&nbsp;
                            </td>
                        </tr>
                        <tr>
                            <td class="Width2Percent"></td>
                            <td>
                                <table class="WholeWidth">
                                    <tr>
                                        <td class="TdProjectDetailFeild">
                                            <table class="WholeWidth">
                                                <tr>
                                                    <td class="width30P">Account
                                                    </td>
                                                    <td class="width60P">
                                                        <asp:DropDownList ID="ddlClientName" runat="server" OnSelectedIndexChanged="ddlClientName_SelectedIndexChanged"
                                                            CssClass="Width94Per" AutoPostBack="True" onchange="setDirty();">
                                                        </asp:DropDownList>
                                                    </td>
                                                    <td class="Width10Percent">
                                                        <a href="ProjectHelp.aspx" class="ProjectHelp" target="_blank" title="This field contains the name of company we are completing the project for.">*</a>
                                                        <asp:RequiredFieldValidator ID="reqClientName" runat="server" ControlToValidate="ddlClientName"
                                                            ErrorMessage="The Account Name is required." ToolTip="The Account Name is required."
                                                            ValidationGroup="Project" Text="*" EnableClientScript="false" SetFocusOnError="true"></asp:RequiredFieldValidator>
                                                        <asp:CustomValidator ID="cvClient" runat="server" ErrorMessage="The project's account designation cannot be changed as time has been entered towards the project."
                                                            ToolTip="The project's account designation cannot be changed as time has been entered towards the project."
                                                            ValidationGroup="Project" Text="*" EnableClientScript="false" SetFocusOnError="true"
                                                            Display="Dynamic" OnServerValidate="cvClient_ServerValidate"></asp:CustomValidator>
                                                    </td>
                                                </tr>
                                            </table>
                                        </td>
                                        <td class="Width2Percent"></td>
                                        <td class="TdProjectDetailFeild">
                                            <table class="WholeWidth">
                                                <tr>
                                                    <td class="width30P">Revenue Type
                                                    </td>
                                                    <td class="width60P WhiteSpaceNoWrap">&nbsp;
                                                        <asp:DropDownList ID="ddlRevenueType" runat="server" onchange="setDirty();" CssClass="Width94Per"
                                                            AutoPostBack="True">
                                                        </asp:DropDownList>
                                                    </td>
                                                    <td class="TdValidators">
                                                        <a href="ProjectHelp.aspx" class="ProjectHelp" target="_blank" title="The Revenue Type field is to be used for tracking revenue which has a separate P&L associated with it.&#13;&#10;  When multiple Revenue Types can apply, the Revenue Type should be set to General.">*</a>
                                                        <asp:RequiredFieldValidator ID="reqRevenueType" runat="server" ControlToValidate="ddlRevenueType"
                                                            ErrorMessage="The Revenue Type is required." ToolTip="The Revenue Type is required."
                                                            ValidationGroup="Project" Text="*" EnableClientScript="false" SetFocusOnError="true"
                                                            Display="Dynamic"></asp:RequiredFieldValidator>
                                                    </td>
                                                </tr>
                                            </table>
                                        </td>
                                        <td class="Width2Percent"></td>
                                        <td class="TdProjectDetailFeild">
                                            <table class="WholeWidth">
                                                <tr>
                                                    <td class="width30P">Salesperson
                                                    </td>
                                                    <td class="width60P">
                                                        <asp:DropDownList ID="ddlSalesperson" runat="server" CssClass="Width95Per" onchange="setDirty();">
                                                        </asp:DropDownList>
                                                    </td>
                                                    <td class="Width2Percent WhiteSpaceNoWrap">
                                                        <a href="ProjectHelp.aspx" class="ProjectHelp" target="_blank" title="This field contains the name of the Logic20/20 sales person responsible for the opportunity/project.">*</a>
                                                        <asp:RequiredFieldValidator ID="reqSalesperson" runat="server" ControlToValidate="ddlSalesperson"
                                                            EnableClientScript="false" ValidationGroup="Project" ErrorMessage="The Salesperson is required."
                                                            SetFocusOnError="true" Text="*" ToolTip="The Salesperson is required."></asp:RequiredFieldValidator>
                                                    </td>
                                                    <td class="Width8Percent">
                                                        <asp:ImageButton ID="imgMailToSalesperson" runat="server" OnClick="imgMailToSalesperson_OnClick"
                                                            ToolTip="Mail To" ImageUrl="Images/email_envelope.png" />
                                                    </td>
                                                </tr>
                                            </table>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td class="TdProjectDetailFeild">
                                            <table class="WholeWidth">
                                                <tr>
                                                    <td class="width30P">Business Unit
                                                    </td>
                                                    <td class="width60P">
                                                        <asp:DropDownList ID="ddlProjectGroup" runat="server" CssClass="Width94Per" OnSelectedIndexChanged="ddlProjectGroup_SelectedIndexChanged"
                                                            AutoPostBack="true" onchange="setDirty();">
                                                            <asp:ListItem Text="-- Select Business Unit --" Value="" Selected="True"></asp:ListItem>
                                                        </asp:DropDownList>
                                                    </td>
                                                    <td class="Width10Percent">
                                                        <a href="ProjectHelp.aspx" class="ProjectHelp" target="_blank" title="For most clients this field will be set to Default Group.&#13;&#10; For Microsoft projects this field should be set according the group we are working with within Microsoft.">*</a>
                                                        <asp:RequiredFieldValidator ID="reqBusinessUnit" runat="server" ControlToValidate="ddlProjectGroup"
                                                            EnableClientScript="false" ValidationGroup="Project" ErrorMessage="The Business Unit is required."
                                                            SetFocusOnError="true" Text="*" ToolTip="The Business Unit is required."></asp:RequiredFieldValidator>
                                                        <asp:CustomValidator ID="cvGroup" runat="server" ErrorMessage="The project's Business Unit designation cannot be changed as time has been entered towards the project."
                                                            ToolTip="The project's Business Unit designation cannot be changed as time has been entered towards the project."
                                                            ValidationGroup="Project" Text="*" EnableClientScript="false" SetFocusOnError="true"
                                                            Display="Dynamic" OnServerValidate="cvGroup_ServerValidate"></asp:CustomValidator>
                                                    </td>
                                                </tr>
                                            </table>
                                        </td>
                                        <td></td>
                                        <td class="TdProjectDetailFeild">
                                            <table class="WholeWidth">
                                                <tr>
                                                    <td class="width30P">SOW Amount
                                                        <span class="floatright">$</span>
                                                    </td>
                                                    <td class="width60P WhiteSpaceNoWrap">&nbsp
                                                        <asp:TextBox ID="txtSowBudget" CssClass="Width92Per" runat="server" onchange="setDirty();"
                                                            MaxLength="100"></asp:TextBox>
                                                        <AjaxControlToolkit:TextBoxWatermarkExtender ID="watermarkSowBudget" runat="server"
                                                            TargetControlID="txtSowBudget" WatermarkText="Ex: 15000" EnableViewState="false"
                                                            WatermarkCssClass="watermarkedtext Width92Per" />
                                                        <AjaxControlToolkit:FilteredTextBoxExtender ID="fteSowBudget" TargetControlID="txtSowBudget"
                                                            FilterType="Numbers,Custom" FilterMode="ValidChars" runat="server" ValidChars=".," />
                                                    </td>
                                                    <td class="TdValidators">
                                                        <asp:CustomValidator ID="custSowBudget" runat="server" ControlToValidate="txtSowBudget"
                                                            ToolTip="A number with 2 decimal digits is allowed for the Est. Revenue." Text="*"
                                                            ErrorMessage="A number with 2 decimal digits is allowed for the SOW Budget."
                                                            EnableClientScript="false" SetFocusOnError="true" OnServerValidate="custSowBudget_ServerValidate"
                                                            Display="Dynamic" ValidationGroup="Project"></asp:CustomValidator>
                                                    </td>
                                                </tr>
                                            </table>
                                        </td>
                                        <td class="Width2Percent"></td>
                                        <td class="TdProjectDetailFeild">
                                            <table class="WholeWidth">
                                                <tr>
                                                    <td class="width30P">Executive in Charge
                                                    </td>
                                                    <td class="width60P">
                                                        <asp:DropDownList ID="ddlDirector" runat="server" CssClass="Width95Per" onchange="setDirty();">
                                                            <%--OnSelectedIndexChanged="ddlDirector_SelectedIndexChanged" AutoPostBack="true">--%>
                                                        </asp:DropDownList>
                                                    </td>
                                                    <td class="Width2Percent">&nbsp;
                                                    </td>
                                                    <td class="Width8Percent">
                                                        <asp:ImageButton ID="imgMailToClientDirector" runat="server" OnClick="imgMailToClientDirector_OnClick"
                                                            ToolTip="Mail To" ImageUrl="Images/email_envelope.png" />
                                                    </td>
                                                </tr>
                                            </table>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td class="TdProjectDetailFeild">
                                            <table class="WholeWidth">
                                                <tr>
                                                    <td class="width30P">Business Group
                                                    </td>
                                                    <td class="width60P">
                                                        <asp:Label ID="lblBusinessGroup" runat="server" Text=""></asp:Label>
                                                    </td>
                                                    <td class="Width10Percent"></td>
                                                </tr>
                                            </table>
                                        </td>
                                        <td class="Width2Percent"></td>
                                        <td class="TdProjectDetailFeild">
                                            <table class="WholeWidth">
                                                <tr>
                                                    <td class="width30P">Pricing List
                                                    </td>
                                                    <td class="width60P WhiteSpaceNoWrap">&nbsp;
                                                        <asp:DropDownList ID="ddlPricingList" runat="server" CssClass="Width94Per" onchange="setDirty();">
                                                            <asp:ListItem Text="-- Select Pricing List --" Value="" Selected="True"></asp:ListItem>
                                                        </asp:DropDownList>
                                                    </td>
                                                    <td class="TdValidators">
                                                        <a href="ProjectHelp.aspx" class="ProjectHelp" target="_blank" title="This should be set to the Logic20/20 associate who has been assigned the Project Manager role for the project.">*</a>
                                                        <asp:RequiredFieldValidator ID="reqPricingList" runat="server" ControlToValidate="ddlPricingList"
                                                            Display="Dynamic" EnableClientScript="false" ValidationGroup="Project" ErrorMessage="The Pricing List is required."
                                                            SetFocusOnError="true" Text="*" ToolTip="The Pricing List is required."></asp:RequiredFieldValidator>
                                                    </td>
                                                </tr>
                                            </table>
                                        </td>
                                        <td class="Width2Percent"></td>
                                        <td class="TdProjectDetailFeild">
                                            <table class="WholeWidth">
                                                <tr>
                                                    <td class="width30P">Project Manager
                                                    </td>
                                                    <td class="width60P">
                                                        <asp:DropDownList ID="ddlProjectOwner" runat="server" onchange="setDirty();" CssClass="Width95Per">
                                                        </asp:DropDownList>
                                                    </td>
                                                    <td class="Width2Percent WhiteSpaceNoWrap">
                                                        <a href="ProjectHelp.aspx" class="ProjectHelp" target="_blank" title="This should be set to the Logic20/20 associate who has been assigned the Project Manager role for the project.">*</a>
                                                        <asp:RequiredFieldValidator ID="reqProjectOwner" runat="server" ControlToValidate="ddlProjectOwner"
                                                            Display="Dynamic" EnableClientScript="false" ValidationGroup="Project" ErrorMessage="The Project Manager is required."
                                                            SetFocusOnError="true" Text="*" ToolTip="The Project Manager is required."></asp:RequiredFieldValidator>
                                                        <asp:CustomValidator ID="cvProjectOwner" runat="server" EnableClientScript="false"
                                                            ValidationGroup="Project" ErrorMessage="The selected Project Manager has been terminated or made inactive.  Please select another Manager."
                                                            ValidateEmptyText="true" OnServerValidate="cvProjectOwner_OnServerValidate" SetFocusOnError="true"
                                                            Display="Dynamic" Text="*" ToolTip="The selected Project Manager has been terminated or made inactive.  Please select another Manager."></asp:CustomValidator>
                                                    </td>
                                                    <td class="Width8Percent">
                                                        <asp:ImageButton ID="imgMailToProjectOwner" runat="server" OnClick="imgMailToProjectOwner_OnClick"
                                                            ToolTip="Mail To" ImageUrl="Images/email_envelope.png" />
                                                    </td>
                                                </tr>
                                            </table>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td class="TdProjectDetailFeild">
                                            <table class="WholeWidth">
                                                <tr>
                                                    <td class="width30P">Division
                                                    </td>
                                                    <td class="width60P">
                                                        <asp:DropDownList ID="ddlDivision" runat="server" onchange="setDirty();" CssClass="Width94Per"
                                                            AutoPostBack="True" OnSelectedIndexChanged="ddlDivision_SelectedIndexChanged">
                                                        </asp:DropDownList>
                                                    </td>
                                                    <td class="Width10Percent">
                                                        <a href="ProjectHelp.aspx" class="ProjectHelp" target="_blank" title="This field should be set based on the Division the project falls within.">*</a>
                                                        <asp:RequiredFieldValidator ID="reqDivision" runat="server" ControlToValidate="ddlDivision"
                                                            EnableClientScript="false" ValidationGroup="Project" ErrorMessage="The Division is required."
                                                            SetFocusOnError="true" Text="*" ToolTip="The Division is required."></asp:RequiredFieldValidator>
                                                    </td>
                                                </tr>
                                            </table>
                                        </td>
                                        <td class="Width2Percent"></td>
                                        <td class="TdProjectDetailFeild">
                                            <table class="WholeWidth">
                                                <tr>
                                                    <td class="width30P">PO Number
                                                    </td>
                                                    <td class="width60P WhiteSpaceNoWrap">&nbsp
                                                        <asp:TextBox ID="txtPONumber" runat="server" CssClass="Width92Per" onchange="setDirty();">
                                                        </asp:TextBox>
                                                    </td>
                                                    <td class="TdValidators"></td>
                                                </tr>
                                            </table>
                                        </td>
                                        <td class="Width2Percent"></td>
                                        <td class="TdProjectDetailFeild">
                                            <table class="WholeWidth">
                                                <tr>
                                                    <td class="width30P">Engagement Manager
                                                    </td>
                                                    <td class="width60P WhiteSpaceNoWrap">
                                                        <asp:DropDownList ID="ddlSeniorManager" runat="server" CssClass="Width945Per" onchange="setDirty();">
                                                        </asp:DropDownList>
                                                    </td>
                                                    <td class="Width2Percent WhiteSpaceNoWrap">
                                                        <a href="ProjectHelp.aspx" class="ProjectHelp" target="_blank" title="This field contains the name of the person who is responsible for maintaining the overall account/client relationship.">*</a>
                                                        <asp:RequiredFieldValidator ID="req" runat="server" ControlToValidate="ddlSeniorManager"
                                                            Display="Dynamic" EnableClientScript="false" ValidationGroup="Project" ErrorMessage="The Engagement Manager is required."
                                                            SetFocusOnError="true" Text="*" ToolTip="The Engagement Manager is required."></asp:RequiredFieldValidator>
                                                    </td>
                                                    <td class="Width8Percent">
                                                        <asp:ImageButton ID="imgMailToSeniorManager" runat="server" OnClick="imgMailToSeniorManager_OnClick"
                                                            ToolTip="Mail To" ImageUrl="Images/email_envelope.png" />
                                                    </td>
                                                </tr>
                                            </table>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td class="TdProjectDetailFeild">
                                            <table class="WholeWidth">
                                                <tr>
                                                    <td class="width30P">Practice Area
                                                    </td>
                                                    <td class="width60P">
                                                        <asp:DropDownList ID="ddlPractice" runat="server" onchange="setDirty();" CssClass="Width94Per">
                                                            <%-- AutoPostBack="True" OnSelectedIndexChanged="DropDown_SelectedIndexChanged"--%>
                                                        </asp:DropDownList>
                                                    </td>
                                                    <td class="Width10Percent">
                                                        <a href="ProjectHelp.aspx" class="ProjectHelp" target="_blank" title="This field should be set based on the Practice Area the project falls within.">*</a>
                                                        <asp:RequiredFieldValidator ID="reqPractice" runat="server" ControlToValidate="ddlPractice"
                                                            EnableClientScript="false" ValidationGroup="Project" ErrorMessage="The Practice Area is required."
                                                            SetFocusOnError="true" Text="*" ToolTip="The Practice Area is required."></asp:RequiredFieldValidator>
                                                    </td>
                                                </tr>
                                            </table>
                                        </td>
                                        <td class="Width2Percent"></td>
                                        <td class="TdProjectDetailFeild">
                                            <table class="WholeWidth">
                                                <tr>
                                                    <td class="width30P">PO Amount
                                                        <span class="floatright">$</span>
                                                    </td>
                                                    <td class="width60P WhiteSpaceNoWrap">&nbsp
                                                        <asp:TextBox ID="txtPOAmount" CssClass="Width92Per" runat="server" onchange="setDirty();"
                                                            MaxLength="100"></asp:TextBox>
                                                        <AjaxControlToolkit:TextBoxWatermarkExtender ID="tbwePOAmount" runat="server" TargetControlID="txtPOAmount"
                                                            WatermarkText="Ex: 15000" EnableViewState="false" WatermarkCssClass="watermarkedtext Width92Per" />
                                                        <AjaxControlToolkit:FilteredTextBoxExtender ID="ftbePOAmount" TargetControlID="txtPOAmount"
                                                            FilterType="Numbers,Custom" FilterMode="ValidChars" runat="server" ValidChars=".," />
                                                    </td>
                                                    <td class="TdValidators">
                                                        <asp:CustomValidator ID="custPOAmount" runat="server" ControlToValidate="txtPOAmount"
                                                            ToolTip="A number with 2 decimal digits is allowed for the PO Amount." Text="*"
                                                            ErrorMessage="A number with 2 decimal digits is allowed for the PO Amount." EnableClientScript="false"
                                                            SetFocusOnError="true" OnServerValidate="custPOAmount_ServerValidate" Display="Dynamic"
                                                            ValidationGroup="Project"></asp:CustomValidator>
                                                    </td>
                                                </tr>
                                            </table>
                                        </td>
                                        <td class="Width2Percent"></td>
                                        <td class="TdProjectDetailFeild">
                                            <table class="WholeWidth">
                                                <tr>
                                                    <td class="LableProjectManager">Project Access
                                                    </td>
                                                    <td class="width60P">
                                                        <cc2:ScrollingDropDown ID="cblProjectManagers" runat="server" SetDirty="true" CssClass="ProjectDetailScrollingDropDown Width16point5PercentImp"
                                                            AllSelectedReturnType="AllItems" onclick="scrollingDropdown_onclick('cblProjectManagers','Project Access','es','Project Accesses',33);"
                                                            DropDownListType="Project Access" DropDownListTypePluralForm="Project Accesses"
                                                            PluralForm="es" />
                                                        <ext:ScrollableDropdownExtender ID="sdeProjectManagers" runat="server" TargetControlID="cblProjectManagers"
                                                            Width="94%" UseAdvanceFeature="true" EditImageUrl="Images/Dropdown_Arrow.png">
                                                        </ext:ScrollableDropdownExtender>
                                                    </td>
                                                    <td class="Width10Percent">
                                                        <asp:CustomValidator ID="cvProjectManager" runat="server" EnableClientScript="false"
                                                            ValidationGroup="Project" ErrorMessage="The Project Access is required." ValidateEmptyText="true"
                                                            OnServerValidate="cvProjectManager_OnServerValidate" SetFocusOnError="true" Text="*"
                                                            ToolTip="The Project Access is required."></asp:CustomValidator>
                                                        <asp:CustomValidator ID="cvProjectManagerStatus" runat="server" EnableClientScript="false"
                                                            ValidationGroup="Project" ErrorMessage="The selected Project Access has been terminated or made inactive.  Please select another Project Access."
                                                            ValidateEmptyText="true" OnServerValidate="cvProjectManagerStatus_OnServerValidate"
                                                            SetFocusOnError="true" Text="*" ToolTip="The selected Project Access has been terminated or made inactive.  Please select another Project Access."></asp:CustomValidator>
                                                    </td>
                                                </tr>
                                            </table>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td class="TdProjectDetailFeild">
                                            <table class="WholeWidth">
                                                <tr>
                                                    <td class="width30P vTopImp PaddingTop4">Capabilities
                                                    </td>
                                                    <td class="width60P">
                                                        <cc2:ScrollingDropDown ID="cblPracticeCapabilities" runat="server" SetDirty="true"
                                                            CssClass="ProjectDetailScrollingDropDown Width16point5PercentImp" AllSelectedReturnType="AllItems"
                                                            onclick="scrollingDropdown_onclick('cblPracticeCapabilities','Capability','ies','Capabilities',29);"
                                                            DropDownListType="Capability" DropDownListTypePluralForm="Capabilities" PluralForm="ies" />
                                                        <ext:ScrollableDropdownExtender ID="sdePracticeCapabilities" runat="server" TargetControlID="cblPracticeCapabilities"
                                                            BehaviorID="sdePracticeCapabilities" MaxNoOfCharacters="29" Width="93.5%" UseAdvanceFeature="true"
                                                            EditImageUrl="Images/Dropdown_Arrow.png">
                                                        </ext:ScrollableDropdownExtender>
                                                    </td>
                                                    <td class="Width10Percent">
                                                        <asp:CustomValidator ID="cvCapabilities" runat="server" EnableClientScript="false"
                                                            ValidationGroup="Project" ErrorMessage="The Capability(ies) is required." ValidateEmptyText="true"
                                                            OnServerValidate="cvCapabilities_OnServerValidate" SetFocusOnError="true" Text="*"
                                                            ToolTip="The Capability(ies) is required."></asp:CustomValidator>
                                                    </td>
                                                </tr>
                                            </table>
                                        </td>
                                        <td class="Width2Percent"></td>
                                        <td class="TdProjectDetailFeild">
                                            <table class="WholeWidth">
                                                <tr>
                                                    <td class="width30P">Budget Amount
                                                    </td>
                                                    <td class="WhiteSpaceNoWrap">
                                                        <asp:Label ID="txtBudget" runat="server"></asp:Label>
                                                        <asp:Image ID="imgReset" runat="server" ImageUrl="~/Images/budget_change_icon.jpg" CssClass="Width20Px" ToolTip="Budget has been reset." Style="display: none" />
                                                        <asp:Button ID="btnBudgetResetReq" runat="server" Text="Request Revision" OnClick="btnBudgetResetReq_Click" Style="display: none" />
                                                        <span id="spanBudgetAsterisk" runat="server" style="color: red; display: none" title="A Margin Exception is required prior to resetting the budget.">*</span>
                                                        <asp:Button ID="btnResetBudget" runat="server" Text="Revise Budget" OnClick="btnResetBudget_Click" Style="display: none" />
                                                        <asp:Button ID="btnBudgetResetDecline" runat="server" Text="Reject Budget" OnClick="btnBudgetResetDecline_Click" Style="display: none" />
                                                    </td>

                                                </tr>
                                            </table>
                                        </td>
                                        <td class="Width2Percent"></td>
                                        <td class="TdProjectDetailFeild">
                                            <table class="WholeWidth">
                                                <tr>
                                                    <td class="width30P">CSAT Owner
                                                    </td>
                                                    <td class="width60P WhiteSpaceNoWrap">
                                                        <asp:DropDownList ID="ddlCSATOwner" runat="server" onchange="setDirty();" CssClass="Width945Per"
                                                            OnSelectedIndexChanged="ddlCSATOwner_SelectedIndexChanged" AutoPostBack="true">
                                                        </asp:DropDownList>
                                                    </td>
                                                    <td class="Width2Percent WhiteSpaceNoWrap"></td>
                                                    <td class="Width8Percent">
                                                        <asp:ImageButton ID="imgMailToCSATOwner" runat="server" OnClick="imgMailToCSATOwner_OnClick"
                                                            ToolTip="Mail To" ImageUrl="Images/email_envelope.png" />
                                                    </td>
                                                </tr>
                                            </table>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td class="TdProjectDetailFeild">
                                            <table class="WholeWidth">
                                                <tr>
                                                    <td class="width30P">Channel
                                                    </td>
                                                    <td class="width60P">
                                                        <asp:DropDownList ID="ddlChannel" runat="server" onchange="setDirty();" CssClass="Width94Per"
                                                            AutoPostBack="True" OnSelectedIndexChanged="ddlChannel_SelectedIndexChanged">
                                                        </asp:DropDownList>
                                                    </td>
                                                    <td class="Width10Percent">
                                                        <a href="ProjectHelp.aspx" class="ProjectHelp" target="_blank" title="The Channel field is to be used for identifying and tracking the source of a project.">*</a>
                                                        <asp:RequiredFieldValidator ID="reqChannel" runat="server" ControlToValidate="ddlChannel"
                                                            EnableClientScript="false" ValidationGroup="Project" ErrorMessage="The Channel is required."
                                                            SetFocusOnError="true" Text="*" ToolTip="The Channel is required."></asp:RequiredFieldValidator>
                                                    </td>
                                                </tr>
                                            </table>
                                        </td>
                                        <td class="Width2Percent"></td>
                                        <td class="TdProjectDetailFeild">
                                            <table class="WholeWidth">
                                                <tr>
                                                    <td class="width30P">Margin Exception
                                                    </td>
                                                    <td class="width60P WhiteSpaceNoWrap">&nbsp
                                                        <asp:Label ID="txtMarginExc" runat="server" Text="N/A"></asp:Label>
                                                    </td>
                                                    <td class="TdValidators"></td>
                                                </tr>
                                            </table>
                                        </td>
                                        <td class="Width2Percent"></td>
                                        <td class="TdProjectDetailFeild">
                                            <table class="WholeWidth">
                                                <tr>
                                                    <td class="width30P">Offering
                                                    </td>
                                                    <td class="width60P WhiteSpaceNoWrap">
                                                        <asp:DropDownList ID="ddlOffering" runat="server" onchange="setDirty();" CssClass="Width945Per">
                                                        </asp:DropDownList>
                                                    </td>

                                                    <td class="TdValidators Width8Percent">
                                                        <a href="ProjectHelp.aspx" class="ProjectHelp" target="_blank" title="The Offering field is to be used to associate a project with a particular service we have positioned in the market.&#13;&#10; This will aid the business in identifying/confirming which of our services are profitable or not.">*</a>
                                                        <asp:RequiredFieldValidator ID="reqOffering" runat="server" ControlToValidate="ddlOffering"
                                                            ErrorMessage="The Offering is required." ToolTip="The Offering is required."
                                                            ValidationGroup="Project" Text="*" EnableClientScript="false" SetFocusOnError="true"
                                                            Display="Dynamic"></asp:RequiredFieldValidator>
                                                    </td>
                                                </tr>
                                            </table>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td class="TdProjectDetailFeild">
                                            <table class="WholeWidth">
                                                <tr>
                                                    <td class="width30P vTopImp PaddingTop4">Channel-Sub
                                                    </td>
                                                    <td class="width60P">
                                                        <asp:DropDownList ID="ddlSubChannel" runat="server" CssClass="Width94Per" onchange="setDirty();"
                                                            Visible="false">
                                                        </asp:DropDownList>
                                                        <asp:TextBox ID="txtChannelSub" CssClass="Width925Per" runat="server" onchange="setDirty();"
                                                            Visible="false" MaxLength="50"></asp:TextBox>
                                                        <AjaxControlToolkit:TextBoxWatermarkExtender ID="tbwChannelSub" runat="server" TargetControlID="txtChannelSub"
                                                            WatermarkText="Enter Channel-sub" EnableViewState="false" WatermarkCssClass="watermarkedtext Width925Per" />
                                                    </td>
                                                    <td class="Width10Percent">
                                                        <a href="ProjectHelp.aspx" class="ProjectHelp" target="_blank" title="The Channel-sub field is to be used to provide detail regarding the source of a project. &#13;&#10;For example; if the Channel is Salesperson, the Channel-sub would the name of the Salesperson.">*</a>
                                                        <asp:RegularExpressionValidator ID="regTxtChannelSub" runat="server" ErrorMessage="Channel-Sub should be limited to 2-50 characters in length."
                                                            ToolTip="Channel-Sub should be limited to 2-50 characters in length." ControlToValidate="txtChannelSub"
                                                            ValidationGroup="Project" Text="*" EnableClientScript="false" SetFocusOnError="true"
                                                            Enabled="false" Display="Dynamic" ValidationExpression="^[ A-Za-z0-9_@.,/&+-]{2,50}$"></asp:RegularExpressionValidator>
                                                        <asp:RequiredFieldValidator ID="reqddlChannelSub" runat="server" ControlToValidate="ddlSubChannel"
                                                            Display="Dynamic" EnableClientScript="false" ValidationGroup="Project" ErrorMessage="The Channel-Sub is required."
                                                            SetFocusOnError="true" Text="*" ToolTip="The Channel-Sub is required." Enabled="false"></asp:RequiredFieldValidator>
                                                        <asp:RequiredFieldValidator ID="reqTxtChannelSub" runat="server" ControlToValidate="txtChannelSub"
                                                            Display="Dynamic" EnableClientScript="false" ValidationGroup="Project" ErrorMessage="The Channel-Sub is required."
                                                            SetFocusOnError="true" Text="*" ToolTip="The Channel-Sub is required." Enabled="false"></asp:RequiredFieldValidator>
                                                    </td>
                                                </tr>
                                            </table>
                                        </td>
                                        <td class="Width2Percent"></td>
                                        <td class="TdProjectDetailFeild" colspan="2">
                                            <table class="WholeWidth">
                                                <tr>
                                                    <td class="width30P"></td>
                                                    <td class="width60P ">
                                                        <asp:Button ID="btnMrgSubmit" CssClass="btn-MarginException" runat="server" Text="Request Margin Exception" OnClick="btnMrgSubmit_Click" Visible="false"></asp:Button>
                                                        <asp:Button ID="btnMrgAccept" Style="width: 100px; white-space: normal" runat="server" Text="Accept Margin Exception" OnClick="btnMrgAccept_Click" Visible="false" />
                                                        <asp:Button ID="btnMrgDecline" CssClass="btn-MarginException" runat="server" Text="Decline Margin Exception" OnClick="btnMrgDecline_Click" Visible="false" />
                                                    </td>
                                                    <td class="TdValidators"></td>
                                                </tr>
                                            </table>
                                        </td>
                                        <td class="TdProjectDetailFeild">
                                            <table class="WholeWidth">
                                                <tr>
                                                    <td class="width30P">Buyer Name
                                                    </td>
                                                    <td class="width60P WhiteSpaceNoWrap">
                                                        <asp:TextBox ID="txtBuyerName" runat="server" onchange="setDirty();" CssClass="Width93Per"
                                                            MaxLength="100"></asp:TextBox>
                                                    </td>

                                                    <td class="TdValidators Width8Percent">
                                                        <a href="ProjectHelp.aspx" class="ProjectHelp" target="_blank" title="This field contains the name of the main client point of contact.">*</a>
                                                        <asp:RequiredFieldValidator ID="reqBuyerName" runat="server" ControlToValidate="txtBuyerName"
                                                            ErrorMessage="The Buyer Name is required." ToolTip="The Buyer Name is required."
                                                            ValidationGroup="Project" Text="*" EnableClientScript="false" SetFocusOnError="true"
                                                            Display="Dynamic"></asp:RequiredFieldValidator>
                                                        <asp:RegularExpressionValidator ID="valregBuyerName" runat="server" ControlToValidate="txtBuyerName"
                                                            ErrorMessage="Buyer Name should be limited to 2-30 characters in length containing only letters and/or an apostrophe, hyphen, or a single space."
                                                            ToolTip="Buyer Name should be limited to 2-30 characters in length containing only letters and/or an apostrophe, hyphen, or a single space."
                                                            ValidationGroup="Project" Text="*" EnableClientScript="false" SetFocusOnError="true"
                                                            Display="Dynamic" ValidationExpression="^[a-zA-Z'\-\ ]{2,30}$"></asp:RegularExpressionValidator>
                                                        <asp:CustomValidator ID="cvBNAllowSpace" runat="server" ControlToValidate="txtBuyerName"
                                                            ErrorMessage="Buyer Name should be limited to 2-30 characters in length containing only letters and/or an apostrophe, hyphen, or a single space."
                                                            ToolTip="Buyer Name should be limited to 2-30 characters in length containing only letters and/or an apostrophe, hyphen, or a single space."
                                                            ValidationGroup="Project" Text="*" EnableClientScript="false" SetFocusOnError="true"
                                                            Display="Dynamic" OnServerValidate="cvBNAllowSpace_ServerValidate"></asp:CustomValidator>
                                                    </td>
                                                </tr>
                                            </table>
                                        </td>
                                    </tr>


                                </table>
                            </td>
                            <td class="Width2Percent"></td>
                        </tr>
                        <tr>
                            <td colspan="2">&nbsp;
                            </td>
                        </tr>
                        <tr>
                        </tr>
                        <tr>
                            <td class="Width2Percent"></td>
                            <td>
                                <table class="WholeWidth">
                                    <tr>
                                        <td class="TdProjectNotes">
                                            <table class="WholeWidth Height100Percent">
                                                <tr>
                                                    <td class="InnerTdProjectNotes">
                                                        <u>Project Notes</u>
                                                        <asp:CustomValidator ID="custProjectDesciption" runat="server" ControlToValidate="txtDescription"
                                                            Display="Dynamic" OnServerValidate="custProjectDesciption_ServerValidation" SetFocusOnError="True"
                                                            ErrorMessage="The project description cannot be more than 2000 symbols" ToolTip="The project description cannot be more than 2000 symbols"
                                                            ValidationGroup="Project">*</asp:CustomValidator>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td>
                                                        <asp:TextBox ID="txtDescription" runat="server" TextMode="MultiLine" Rows="5" onchange="setDirty();"
                                                            CssClass="TextBoxProjectNotes"></asp:TextBox>
                                                    </td>
                                                </tr>
                                            </table>
                                        </td>
                                        <td class="TdOpportunityLink">
                                            <table class="WholeWidth Height100Percent">
                                                <tr>
                                                    <td>
                                                        <table>
                                                            <tr>
                                                                <td>

                                                                    <div id="divOutsource" runat="server" style="visibility: hidden">
                                                                        Outsourced Service ID
                                                                        <asp:RadioButtonList runat="server" ID="rbOutsourcedId" RepeatDirection="Horizontal"
                                                                            RepeatLayout="Flow">
                                                                            <asp:ListItem Value="1" Text="Yes" Selected="False"></asp:ListItem>
                                                                            <asp:ListItem Value="2" Text="No" Selected="False"></asp:ListItem>
                                                                        </asp:RadioButtonList>
                                                                        <asp:RequiredFieldValidator ID="reqOutsourceId" runat="server" Text="*" ErrorMessage="The Outsource Service Id is Required."
                                                                            ToolTip="The Outsource Service Id is Required." EnableClientScript="false" Enabled="false"
                                                                            ControlToValidate="rbOutsourcedId" ValidationGroup="Project"></asp:RequiredFieldValidator>
                                                                    </div>
                                                                </td>
                                                            </tr>
                                                        </table>
                                                    </td>
                                                    <td>
                                                        <table class="WholeWidth">
                                                            <tr>
                                                                <td>New/Extension
                                                                </td>
                                                                <td class="WhiteSpaceNoWrap">&nbsp;
                                                                    <asp:DropDownList ID="ddlBusinessOptions" runat="server" onchange="setDirty(); EnableOrDisablePreProject(this);" Width="85%">
                                                                    </asp:DropDownList>
                                                                </td>
                                                                <td class="TdValidators" style="width: 14%">
                                                                    <a href="ProjectHelp.aspx" class="ProjectHelp" target="_blank" title="This field should be set according to whether this is a new project or an extension of an existing project.">*</a>
                                                                    <asp:RequiredFieldValidator ID="reqBusinessTypes" runat="server" ControlToValidate="ddlBusinessOptions" InitialValue="0"
                                                                        Display="Dynamic" EnableClientScript="false" ValidationGroup="Project" ErrorMessage="The New/Extension is required."
                                                                        SetFocusOnError="true" Text="*" ToolTip="The New/Extension is required."></asp:RequiredFieldValidator>
                                                                </td>
                                                            </tr>
                                                        </table>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td class="TdUTimeEntry">
                                                        <u>Time Entry</u>
                                                    </td>
                                                    <td>
                                                        <table class="WholeWidth">
                                                            <tr>
                                                                <td>Previous Project Linking
                                                                </td>
                                                                <td>
                                                                    <asp:TextBox ID="txtPrevProjectLink" runat="server" Width="80%" onchange="setDirty();"
                                                                        Enabled="false" MaxLength="50"></asp:TextBox>
                                                                    <asp:ImageButton ID="imgNavigateToProject" runat="server" AlternateText="Navigate to Project"
                                                                        OnClientClick="RedirectToProject(); return false;"
                                                                        ToolTip="Navigate to Project" Visible="false" ImageUrl="~/Images/arrow_16x16.png" />
                                                                </td>
                                                                <td style="width: 14%">
                                                                    <asp:RequiredFieldValidator ID="rfvPrevProjectLink" runat="server" Text="*" ErrorMessage="The Previous Project Number is Required."
                                                                        ToolTip="The Previous Project Number is Required" EnableClientScript="false"
                                                                        Enabled="false" ControlToValidate="txtPrevProjectLink" ValidationGroup="Project"></asp:RequiredFieldValidator>
                                                                    <asp:CustomValidator ID="cvPreviousProjectNumber" runat="server" ControlToValidate="txtPrevProjectLink"
                                                                        ValidationGroup="Project" OnServerValidate="cvPreviousProjectNumber_ServerValidate"
                                                                        Enabled="false" EnableClientScript="false" ErrorMessage="Enter a valid Project Number"
                                                                        Display="Dynamic" Text="*" ToolTip="Enter a valid Project Number"></asp:CustomValidator>
                                                                </td>
                                                            </tr>
                                                        </table>

                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td class="TdProjectNotesDescription">Notes for this project are&nbsp;
                                                        <asp:DropDownList ID="ddlNotes" runat="server" CssClass="Width200Px" onchange="setDirty();">
                                                            <asp:ListItem Selected="True" Text="Required" Value="1"></asp:ListItem>
                                                            <asp:ListItem Text="Optional" Value="0"></asp:ListItem>
                                                        </asp:DropDownList>
                                                    </td>
                                                    <td></td>
                                                </tr>
                                                <tr>
                                                    <td class="TdProjectNotesDescription">Client System Time Entry
                                                        <asp:DropDownList ID="ddlClientTimeEntry" runat="server" CssClass="Width200Px" onchange="setDirty();">
                                                            <asp:ListItem Selected="True" Text="Not Required" Value="0"></asp:ListItem>
                                                            <asp:ListItem Text="Required" Value="1"></asp:ListItem>
                                                        </asp:DropDownList>
                                                    </td>

                                                </tr>
                                            </table>
                                        </td>
                                        <td></td>
                                    </tr>
                                </table>
                            </td>
                            <td class="Width2Percent"></td>
                        </tr>
                        <tr>
                            <td colspan="3">&nbsp;
                            </td>
                        </tr>
                        <tr>
                            <td class="Width2Percent"></td>
                            <td>
                                <div id="divMarginExceptions" runat="server">
                                    <div id="divExceptionOne" runat="server" class="textCenter" style="background-color: red; color: white" visible="false">Projected Margin Below Threshold, Exception Required</div>
                                    <div id="divExceptionTwo" runat="server" class="textCenter" style="background-color: yellow" visible="false">Margin Below Threshold, Review Needed</div>
                                    <div id="divExceptionTierOne" runat="server" class="textCenter" style="background-color: red; color: white" visible="false">Project locked for Margin Approval; Tier 1</div>
                                    <div id="divExceptionTierTwo" runat="server" class="textCenter" style="background-color: red; color: white" visible="false">Project locked for Margin Approval; Tier 2</div>
                                </div>
                            </td>
                            <td class="Width2Percent"></td>
                        </tr>
                        <tr>
                            <td colspan="3">&nbsp;
                            </td>
                        </tr>
                        <tr>
                            <td colspan="3">
                                <asp:Table ID="tblProjectDetailTabViewSwitch" runat="server" CssClass="CustomTabStyle CustomTabStyleProjectDetail">
                                    <asp:TableRow ID="rowSwitcher" runat="server">
                                        <asp:TableCell ID="cellMilestones" runat="server" CssClass="SelectedSwitch">
                                            <span class="bg"><span>
                                                <asp:LinkButton ID="btnMilestones" runat="server" Text="Milestones" CausesValidation="false"
                                                    OnCommand="btnView_Command" CommandArgument="0"></asp:LinkButton></span>
                                            </span>
                                        </asp:TableCell>
                                        <asp:TableCell ID="cellProjectTimeTypes" runat="server">
                                            <span class="bg"><span>
                                                <asp:LinkButton ID="btnProjectTimeTypes" runat="server" Text="Project Time Types"
                                                    CausesValidation="false" OnCommand="btnView_Command" CommandArgument="1"></asp:LinkButton></span>
                                            </span>
                                        </asp:TableCell>
                                        <asp:TableCell ID="CellAttachments" runat="server">
                                            <span class="bg"><span>
                                                <asp:LinkButton ID="btnAttachments" runat="server" Text="Attachments" CausesValidation="false"
                                                    OnCommand="btnView_Command" CommandArgument="2"></asp:LinkButton></span>
                                            </span>
                                        </asp:TableCell>
                                        <asp:TableCell ID="cellFinancials" runat="server">
                                            <span class="bg"><span>
                                                <asp:LinkButton ID="btnFinancials" runat="server" Text="Financial Summary" CausesValidation="false"
                                                    OnCommand="btnView_Command" CommandArgument="3"></asp:LinkButton></span>
                                            </span>
                                        </asp:TableCell>
                                        <asp:TableCell ID="cellCommissions" runat="server">
                                            <span class="bg"><span>
                                                <asp:LinkButton ID="btnCommissions" runat="server" Text="Commissions" CausesValidation="false"
                                                    OnCommand="btnView_Command" CommandArgument="4"></asp:LinkButton></span>
                                            </span>
                                        </asp:TableCell>
                                        <asp:TableCell ID="cellExpenses" runat="server">
                                            <span class="bg"><span>
                                                <asp:LinkButton ID="btnExpenses" runat="server" Text="Expenses" CausesValidation="false"
                                                    OnCommand="btnView_Command" CommandArgument="5"></asp:LinkButton></span>
                                            </span>
                                        </asp:TableCell>
                                        <asp:TableCell ID="cellPersons" runat="server">
                                            <span class="bg"><span>
                                                <asp:LinkButton ID="btnPersons" runat="server" Text="Persons" CausesValidation="false"
                                                    OnCommand="btnView_Command" CommandArgument="6"></asp:LinkButton></span>

                                            </span>
                                        </asp:TableCell>
                                        <asp:TableCell ID="TableCellHistoryg" runat="server" Visible="false">
                                            <span class="bg"><span>
                                                <asp:LinkButton ID="btnHstory" runat="server" Text="History" CausesValidation="false"
                                                    OnCommand="btnView_Command" CommandArgument="7"></asp:LinkButton></span>
                                            </span>
                                        </asp:TableCell>
                                        <asp:TableCell ID="cellProjectTools" runat="server" Visible="false">
                                            <span class="bg"><span>
                                                <asp:LinkButton ID="btnProjectTools" runat="server" Text="Clone Project" CausesValidation="false"
                                                    OnCommand="btnView_Command" CommandArgument="8"></asp:LinkButton></span>
                                            </span>
                                        </asp:TableCell>
                                        <asp:TableCell ID="cellProjectCSAT" runat="server" Visible="false">
                                            <span class="bg"><span>
                                                <asp:LinkButton ID="btnProjectCSAT" runat="server" Text="CSAT" CausesValidation="false"
                                                    OnCommand="btnView_Command" CommandArgument="9"></asp:LinkButton></span>
                                            </span>
                                        </asp:TableCell>
                                        <asp:TableCell ID="cellBudgetMgmt" runat="server" Visible="false">
                                            <span class="bg"><span>
                                                <asp:LinkButton ID="btnBudgetMgmt" runat="server" Text="Budget Management" CausesValidation="false"
                                                    OnCommand="btnView_Command" CommandArgument="10"></asp:LinkButton></span>
                                            </span>
                                        </asp:TableCell>
                                    </asp:TableRow>
                                </asp:Table>
                                <asp:MultiView ID="mvProjectDetailTab" runat="server" ActiveViewIndex="0">
                                    <asp:View ID="vwMilestones" runat="server">
                                        <asp:Panel ID="pnlRevenueMilestones" runat="server" CssClass="tab-pane">
                                            <div class="PaddingBottom35Px">
                                                <asp:ShadowedTextButton ID="btnAddMilistone" runat="server" CausesValidation="false"
                                                    OnClick="btnAddMilistone_Click" CssClass="add-btn" OnClientClick="if (!confirmSaveDirty()) return false;"
                                                    Text="Add Milestone" />
                                            </div>
                                            <uc:ProjectMilestonesFinancials ID="milestones" runat="server" ValidationGroup="MilestoneTab" />
                                        </asp:Panel>
                                    </asp:View>
                                    <asp:View ID="View1" runat="server">
                                        <asp:Panel ID="pnlProjectTimeTypes" runat="server" CssClass="tab-pane">
                                            <div class="PaddingBottom6">
                                                <uc:ProjectTimeTypes ID="ucProjectTimeTypes" runat="server" />
                                                <asp:Button ID="btnDeleteWorkType" OnClick="btnDeleteWorkType_OnClick" runat="server"
                                                    Style='display: none;' Text="" /><asp:HiddenField ID="hdnWorkTypeId" runat="server" />
                                                <asp:CustomValidator ID="cvWorkTypesAssigned" runat="server" EnableClientScript="false"
                                                    ErrorMessage="" ValidateEmptyText="true" Text=""></asp:CustomValidator>
                                            </div>
                                        </asp:Panel>
                                    </asp:View>
                                    <asp:View ID="vmAttachments" runat="server">
                                        <asp:Panel ID="pnlAttachments" runat="server" CssClass="tab-pane">
                                            <div class="PaddingBottom35Px">
                                                <asp:ShadowedTextButton ID="stbAttachSOW" runat="server" CausesValidation="false"
                                                    OnClick="stbAttachSOW_Click" OnClientClick="if(!ConfirmSaveOrExit()) return false;"
                                                    CssClass="add-btn" Text="Add Attachment" />
                                                <asp:HiddenField ID="hdnOpenAttachmentPopUp" runat="server" />
                                                <AjaxControlToolkit:ModalPopupExtender ID="mpeAttachSOW" runat="server" TargetControlID="hdnOpenAttachmentPopUp"
                                                    BackgroundCssClass="modalBackground" PopupControlID="pnlAttachSOW" DropShadow="false" />
                                            </div>
                                            <asp:Repeater ID="repProjectAttachments" runat="server">
                                                <HeaderTemplate>
                                                    <table class="CompPerfTable tablesorter" width="100%" align="center" id="tblProjectAttachments">
                                                        <thead>
                                                            <tr class="CompPerfHeader">
                                                                <th class="Width43Percent">
                                                                    <div class="ie-bg NoBorder">
                                                                        Attachment Name
                                                                    </div>
                                                                </th>
                                                                <th class="Width13Percent">
                                                                    <div class="ie-bg NoBorder">
                                                                        Category
                                                                    </div>
                                                                </th>
                                                                <th class="Width11Percent">
                                                                    <div class="ie-bg NoBorder">
                                                                        Size
                                                                    </div>
                                                                </th>
                                                                <th class="Width13Percent">
                                                                    <div class="ie-bg NoBorder">
                                                                        Uploaded Date
                                                                    </div>
                                                                </th>
                                                                <th class="Width15Percent">
                                                                    <div class="ie-bg NoBorder">
                                                                        Uploader
                                                                    </div>
                                                                </th>
                                                                <th class="Width5Percent">
                                                                    <div class="ie-bg NoBorder">
                                                                        &nbsp;
                                                                    </div>
                                                                </th>
                                                            </tr>
                                                        </thead>
                                                        <tbody>
                                                </HeaderTemplate>
                                                <ItemTemplate>
                                                    <tr>
                                                        <td class="textLeft padLeft20">
                                                            <% if (Project != null && Project.Id.HasValue)
                                                                { %>
                                                            <asp:HyperLink ID="hlnkProjectAttachment1" CssClass="ProjectAttachmentNameWrap" runat="server"
                                                                Text='<%# GetWrappedText( (string)Eval("AttachmentFileName")) %>' NavigateUrl='<%# GetNavigateUrl((string)Eval("AttachmentFileName"), (int)Eval("AttachmentId")) %>'></asp:HyperLink>
                                                            <% }
                                                                else
                                                                { %>
                                                            <asp:LinkButton ID="lnkProjectAttachment1" runat="server" CssClass="ProjectAttachmentNameWrap"
                                                                Visible="<%# IsProjectCreated() %>" attachmentid='<%# Eval("AttachmentId") %>'
                                                                Text='<%# GetWrappedText((string)Eval("AttachmentFileName")) %>' OnClientClick="DownloadUnsavedFile(this); return false;"
                                                                OnClick="lnkProjectAttachment_OnClick" />
                                                            <% } %>
                                                        </td>
                                                        <td class="textCenter">
                                                            <asp:Label ID="lblAttachmentCategory" runat="server" Text='<%# Eval("Category")%>'></asp:Label>
                                                        </td>
                                                        <td class="textCenter" sorttable_customkey='<%# ((int)Eval("AttachmentSize")/1024) %>'>
                                                            <asp:Label ID="lblAttachmentSize" runat="server" Text='<%# string.Format("{0}Kb", (int)Eval("AttachmentSize")/1024)  %>'></asp:Label>
                                                        </td>
                                                        <td class="textCenter">
                                                            <asp:Label ID="lblUploadedDate" runat="server" Text='<%# ((DateTime?)Eval("UploadedDate")).HasValue ? string.Format("{0}", ((DateTime?)Eval("UploadedDate")).Value.ToString("MM/dd/yyyy")) : string.Empty %>'></asp:Label>
                                                        </td>
                                                        <td class="textCenter">
                                                            <asp:Label ID="lblUploader" runat="server" Text='<%# Eval("Uploader") %>'></asp:Label>
                                                        </td>
                                                        <td class="textCenter">
                                                            <asp:ImageButton ID="imgbtnDeleteAttachment1" OnClick="imgbtnDeleteAttachment_Click"
                                                                AttachmentId='<%# Eval("AttachmentId") %>' OnClientClick="if(confirm('Do you really want to delete the project attachment?')){ return true;}return false;"
                                                                Visible="true" runat="server" ImageUrl="~/Images/trash-icon-Large.png" ToolTip="Delete Attachment" />
                                                        </td>
                                                    </tr>
                                                </ItemTemplate>
                                                <FooterTemplate>
                                                    </tbody> </table>
                                                </FooterTemplate>
                                            </asp:Repeater>
                                            <div id="divEmptyMessage" style="display: none;" class="BackGroundColorWhite" runat="server">
                                                No attachments have been uploaded for this project.
                                            </div>
                                        </asp:Panel>
                                    </asp:View>
                                    <asp:View ID="vwFinancials" runat="server">
                                        <uc:ProjectFinancials ID="financials" runat="server" />
                                    </asp:View>
                                    <asp:View ID="vwCommissions" runat="server">
                                        <asp:Panel ID="pnlCommissions" runat="server" CssClass="tab-pane">
                                            <uc:ProjectAttribution ID="projectAttribution" runat="server" ValidationGroup="AttributionGroup" />
                                        </asp:Panel>
                                    </asp:View>
                                    <asp:View ID="vwExpenses" runat="server">
                                        <div class="tab-pane">
                                            <uc2:ProjectExpenses runat="server" ID="projectExpenses" />
                                        </div>
                                    </asp:View>
                                    <asp:View ID="vwPersons" runat="server">
                                        <uc:ProjectPersons ID="persons" runat="server" />
                                    </asp:View>
                                    <asp:View ID="vwHistory" runat="server">
                                        <asp:Panel ID="plnTabHistory" runat="server" CssClass="tab-pane">
                                            <uc:ActivityLogControl runat="server" ID="activityLog" DisplayDropDownValue="Project"
                                                LabelTextBeforeDropDown="changes over " DateFilterValue="Year" />
                                            <%--   ShowDisplayDropDown="false" />--%>
                                        </asp:Panel>
                                    </asp:View>
                                    <asp:View ID="vwProjectTools" runat="server">
                                        <asp:Panel ID="pnlTools" runat="server" CssClass="tab-pane">
                                            <table class="alterrow">
                                                <tr>
                                                    <td>
                                                        <ul class="ListStyleNone">
                                                            <li>
                                                                <asp:CheckBox ID="chbCloneMilestones" OnCheckedChanged="chbCloneMilestones_CheckedChanged"
                                                                    runat="server" Checked="true" AutoPostBack="True" Text="Clone milestones and milestone person details" /></li>
                                                            <li>
                                                                <asp:CheckBox ID="chbCloneCommissions" runat="server" Checked="true" Text="Clone commissions" /></li>
                                                            <li>Clone status
                                                                <asp:DropDownList ID="ddlCloneProjectStatus" runat="server" />
                                                                <asp:CustomValidator ID="cvCloneStatus" runat="server" ControlToValidate="ddlCloneProjectStatus"
                                                                    ValidationGroup="CloneStatus" OnServerValidate="cvCloneStatus_ServerValidate"
                                                                    ErrorMessage="Project without milestone cannot be cloned with Active status."
                                                                    Display="Dynamic" ToolTip="Project without milestone cannot be cloned with Active status.">*</asp:CustomValidator>

                                                            </li>
                                                            <li style="padding-top: 2px">Clone Revenue
                                                                <asp:DropDownList ID="ddlCloneRevenueType" runat="server">
                                                                    <asp:ListItem Text="Projected" Value="1"></asp:ListItem>
                                                                    <asp:ListItem Text="Budget" Value="2"></asp:ListItem>
                                                                    <asp:ListItem Text="ETC" Value="3" Selected="True"></asp:ListItem>
                                                                </asp:DropDownList>
                                                            </li>
                                                        </ul>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td>
                                                        <asp:Button ID="lnkClone" runat="server" Text="Clone *" ToolTip="Clone *" OnClick="lnkClone_OnClick" />
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td>
                                                        <span class="colorGray">* You will be redirected to the cloned project after you click
                                                            the button.</span>
                                                        <extDisable:ElementDisablerExtender ID="edeCloneButton" runat="server" TargetControlID="lnkClone"
                                                            ControlToDisableID="lnkClone" />
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td>
                                                        <asp:ValidationSummary ID="vsCloneStatus" runat="server" EnableClientScript="false"
                                                            ValidationGroup="CloneStatus" />
                                                    </td>
                                                </tr>
                                            </table>
                                        </asp:Panel>
                                    </asp:View>
                                    <asp:View ID="vwCSAT" runat="server">
                                        <uc:ProjectCSAT ID="ucCSAT" runat="server" />
                                    </asp:View>
                                    <asp:View ID="vwBudgetMgmt" runat="server">
                                        <asp:Panel ID="pnlBudgetManagement" runat="server" CssClass="xScrollAuto minheight250Px">
                                            <uc:BudgetManagementByProject ID="budgetManagementByProject" runat="server"></uc:BudgetManagementByProject>
                                        </asp:Panel>
                                    </asp:View>
                                </asp:MultiView>
                            </td>
                        </tr>
                        <tr>
                            <td align="center" colspan="3" class="PaddingTop5">
                                <asp:HiddenField ID="hdnProjectId" runat="server" />
                                <asp:HiddenField ID="hdnProjectDelete" runat="server" />
                                <asp:Button ID="btnDelete" runat="server" Text="Delete Project" OnClick="btnDelete_Click"
                                    ToolTip="Delete Project" OnClientClick="ConfirmToDeleteProject();" Enabled="false"
                                    Visible="false" />&nbsp;
                                <asp:Button ID="btnSave" runat="server" Text="Save" ToolTip="Save" OnClick="btnSave_Click"
                                    CssClass="Width115PxImp" ValidationGroup="Project" />
                                <label id="lblStart" runat="server" title="Margin Exception" class="colorRed vTop" visible="false">*</label>&nbsp;
                                <asp:CancelAndReturnButton ID="btnCancelAndReturn" runat="server" CssClass="Width115PxImp" />
                            </td>
                        </tr>
                    </table>
                </asp:View>
                <asp:View ID="vwInternal" runat="server">
                    <br />
                    <table class="width30P">
                        <tr>
                            <td class="Width5Percent"></td>
                            <td colspan="2">
                                <asp:TextBox ID="txtInternalProjectName" CssClass="Width500PxImp" runat="server"></asp:TextBox>
                                <AjaxControlToolkit:TextBoxWatermarkExtender ID="txtWeInternalProject" runat="server"
                                    TargetControlID="txtInternalProjectName" WatermarkText="Enter a Project Name here..."
                                    EnableViewState="false" WatermarkCssClass="watermarkedtext Width500PxImp" />
                                <asp:RequiredFieldValidator ID="rvIntlProjectName" runat="server" Text="*" ErrorMessage="The Project Name is Required."
                                    ToolTip="The Project Name is Required." ControlToValidate="txtInternalProjectName"
                                    ValidationGroup="InternalProject"></asp:RequiredFieldValidator>
                            </td>
                        </tr>
                        <tr>
                            <td class="Width5Percent"></td>
                            <td>Project Number
                            </td>
                            <td class="TdProjectDetailFeild">
                                <asp:DropDownList ID="ddlProjectNumberSeries" runat="server">
                                    <asp:ListItem Text="--Select a series--" Value="0" Selected="True"></asp:ListItem>
                                    <asp:ListItem Text="P91" Value="91"></asp:ListItem>
                                    <asp:ListItem Text="P92" Value="92"></asp:ListItem>
                                    <asp:ListItem Text="P93" Value="93"></asp:ListItem>
                                    <asp:ListItem Text="P94" Value="94"></asp:ListItem>
                                    <asp:ListItem Text="P95" Value="95"></asp:ListItem>
                                    <asp:ListItem Text="P96" Value="96"></asp:ListItem>
                                    <asp:ListItem Text="P97" Value="97"></asp:ListItem>
                                </asp:DropDownList>
                                <asp:RequiredFieldValidator ID="rfvPrjectNumberSeries" runat="server" ControlToValidate="ddlProjectNumberSeries"
                                    Display="Dynamic" EnableClientScript="false" ErrorMessage="The Project Number Series is Required."
                                    InitialValue="0" SetFocusOnError="true" Text="*" ValidationGroup="InternalProject"
                                    ToolTip="The Project Number Series is Required."></asp:RequiredFieldValidator>
                            </td>
                        </tr>
                        <tr>
                            <td class="Width5Percent"></td>
                            <td class="Width15Per">Project Status
                            </td>
                            <td class="TdProjectDetailFeild">Active
                            </td>
                        </tr>
                        <tr>
                            <td class="Width5Percent"></td>
                            <td class="Width15Per">Account
                            </td>
                            <td class="TdProjectDetailFeild">Logic20/20
                            </td>
                        </tr>
                        <tr>
                            <td class="Width5Percent"></td>
                            <td class="Width15Per">Business Unit
                            </td>
                            <td class="TdProjectDetailFeild">Operations
                            </td>
                        </tr>
                        <tr>
                            <td class="Width5Percent"></td>
                            <td class="Width15Per">Division
                            </td>
                            <td class="width60PImp">
                                <asp:DropDownList ID="ddlIntDivision" runat="server" onchange="setDirty();" CssClass="Width94Per"
                                    AutoPostBack="True" OnSelectedIndexChanged="ddlDivision_SelectedIndexChanged">
                                </asp:DropDownList>
                                <asp:RequiredFieldValidator ID="reqddlIntDivision" runat="server" ControlToValidate="ddlIntDivision"
                                    Display="Dynamic" EnableClientScript="false" ErrorMessage="The Project Division is Required."
                                    SetFocusOnError="true" Text="*" ValidationGroup="InternalProject" ToolTip="The Project Division is Required."></asp:RequiredFieldValidator>
                            </td>
                        </tr>
                        <tr>
                            <td class="Width5Percent"></td>
                            <td class="Width15Per">Practice Area
                            </td>
                            <td class="TdProjectDetailFeild  width60PImp">
                                <asp:DropDownList ID="ddlIntPractice" runat="server" onchange="setDirty();" CssClass="Width94Per"
                                    Enabled="false">
                                </asp:DropDownList>
                                <asp:RequiredFieldValidator ID="reqddlIntPractice" runat="server" ControlToValidate="ddlIntPractice"
                                    Display="Dynamic" EnableClientScript="false" ErrorMessage="The Project Practice Area is Required."
                                    SetFocusOnError="true" Text="*" ValidationGroup="InternalProject" ToolTip="The Project Practice Area is Required."></asp:RequiredFieldValidator>
                            </td>
                        </tr>
                        <tr align="center">
                            <td colspan="3" class="PaddingTop15Imp">
                                <asp:Button ID="btnSaveInternalProject" runat="server" Text="Save" ToolTip="Save"
                                    CssClass="Width115PxImp" ValidationGroup="Project" OnClick="btnSaveInternalProject_Click" />&nbsp;
                                <asp:CancelAndReturnButton ID="btnInternalCancelAndReturnButton" runat="server" CssClass="Width115PxImp" />
                            </td>
                        </tr>
                    </table>
                </asp:View>
            </asp:MultiView>
            <asp:HiddenField ID="hdnVisibilty" runat="server" />
            <asp:HiddenField ID="hdnTargetErrorPanel" runat="server" />
            <asp:HiddenField ID="hdnLinkPopup" runat="server" Value="" />
            <asp:HiddenField ID="hdnCanShowPopup" runat="server" />
            <asp:HiddenField ID="hdnCompletedStatusPopup" runat="server" />
            <asp:Panel ID="pnlProjectName" runat="server" Style="display: none" CssClass="PanelPerson Width480Px">
                <table class="WholeWidth">
                    <tr>
                        <th class="ThEditProjectNameText vMiddle" colspan="3">Edit Project Name
                        </th>
                    </tr>
                    <tr>
                        <td colspan="3">&nbsp;
                        </td>
                    </tr>
                    <tr>
                        <td class="TdProjectNameText">Project Name
                        </td>
                        <td class="paddingBottom5px Width70Percent">
                            <asp:TextBox ID="txtProjectName" runat="server" CssClass="Width95Percent"></asp:TextBox>
                        </td>
                        <td class="paddingBottom5px Width70Percent">
                            <asp:RequiredFieldValidator ID="reqProjectName" runat="server" ControlToValidate="txtProjectName"
                                ErrorMessage="The Project Name is required." ToolTip="The Project Name is required."
                                ValidationGroup="ProjectName" Text="*" EnableClientScript="false" SetFocusOnError="true"></asp:RequiredFieldValidator>
                        </td>
                    </tr>
                    <tr>
                        <td colspan="3" class="TextAlignCenter Width70Percent paddingBottom5pxImp">
                            <asp:Button ID="btnUpdateProjectName" runat="server" Text="Update" ToolTip="Update"
                                OnClick="btnUpdateProjectName_OnClick" OnClientClick="setDirty();" />&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
                            <asp:Button ID="btncloseEditProjectName" runat="server" ToolTip="Cancel" Text="Cancel"></asp:Button>
                        </td>
                    </tr>
                    <tr>
                        <td colspan="3" class="paddingBottom5pxImp paddingLeft5pxImp">
                            <asp:ValidationSummary ID="VsumProjectName" runat="server" EnableClientScript="false"
                                ValidationGroup="ProjectName" />
                        </td>
                    </tr>
                </table>
            </asp:Panel>
            <asp:Panel ID="pnlAttachSOW" runat="server" Style="display: none" CssClass="PanelPerson Width465Px">
                <table class="WholeWidth Padding5">
                    <tr class="BackGroundColorGray Height27Px">
                        <td align="center" class="font14Px LineHeight25Px WS-Normal" colspan="2">Add Attachment
                            <asp:Button ID="btnAttachmentPopupClose" runat="server" CssClass="mini-report-close floatright"
                                ToolTip="Close" Text="X" OnClientClick="ClearVariables();" OnClick="btnCancel_OnClick"></asp:Button>
                        </td>
                    </tr>
                    <tr>
                        <td colspan="2">&nbsp;
                        </td>
                    </tr>
                    <tr>
                        <td class="FileUploadAttachment PaddingBottom10" colspan="2">
                            <asp:FileUpload ID="fuAttachmentsUpload" runat="server" />
                        </td>
                    </tr>
                    <tr>
                        <td class="FileUploadAttachment" colspan="2">
                            <asp:Label ID="lblAttachmentMessage" ForeColor="Gray" runat="server" CssClass="WordWrap"></asp:Label>
                        </td>
                    </tr>
                    <tr>
                        <td class="FileUploadAttachment PaddingTop10Px">
                            <asp:DropDownList ID="ddlAttachmentCategory" runat="server" onchange="EnableUploadButton();">
                            </asp:DropDownList>
                            <asp:CustomValidator ID="cvAttachmentCategory" runat="server" ControlToValidate="ddlAttachmentCategory"
                                EnableClientScript="true" SetFocusOnError="true" Display="Dynamic" OnServerValidate="cvAttachmentCategory_OnServerValidate"
                                ValidationGroup="ProjectAttachment" Text="*" ToolTip="Category is required."
                                ErrorMessage="Category is required."></asp:CustomValidator>
                        </td>
                        <td align="right" class="FileUploadAttachment PaddingTop10Px no-wrap">
                            <asp:Button ID="btnUpload" ValidationGroup="ProjectAttachment" runat="server" Text="Upload"
                                ToolTip="Upload" Enabled="false"></asp:Button>
                            <asp:Button ID="btnCancel" runat="server" ToolTip="Cancel" Text="Cancel" OnClientClick="ClearVariables();"
                                OnClick="btnCancel_OnClick"></asp:Button>
                        </td>
                    </tr>
                    <tr>
                        <td colspan="2">&nbsp;
                        </td>
                    </tr>
                    <tr>
                        <td class="FileUploadAttachment paddingBottom10px" colspan="2">
                            <label id="lblUplodedFilesMsg" runat="server" class="displayNone">
                                Following files uploaded successfully:</label>
                            <div id="uploadedFiles" runat="server" class="padLeft2 uploadedFilesDiv">
                            </div>
                        </td>
                    </tr>
                </table>
            </asp:Panel>
            <AjaxControlToolkit:ModalPopupExtender ID="mpeErrorPanel" runat="server" BehaviorID="mpeErrorPanelBehaviourId"
                TargetControlID="hdnTargetErrorPanel" BackgroundCssClass="modalBackground" PopupControlID="pnlErrorPanel"
                OkControlID="btnOKErrorPanel" CancelControlID="btnOKErrorPanel" DropShadow="false" />
            <asp:Panel ID="pnlErrorPanel" runat="server" Style="display: none;" CssClass="ProjectDetailErrorPanel PanelPerson">
                <table class="Width100Per">
                    <tr>
                        <th align="center" class="TextAlignCenter BackGroundColorGray vBottom">
                            <b class="BtnClose">Attention!</b>
                        </th>
                    </tr>
                    <tr>
                        <td class="Padding10Px">
                            <uc:Label ID="mlConfirmation" runat="server" ErrorColor="Red" InfoColor="Green" WarningColor="Orange" />
                            <asp:ValidationSummary ID="vsumProject" runat="server" DisplayMode="BulletList" CssClass="ApplyStyleForDashBoardLists"
                                ShowMessageBox="false" ShowSummary="true" EnableClientScript="false" HeaderText="Following errors occurred while saving a project."
                                ValidationGroup="Project" />
                            <asp:ValidationSummary ID="vsInternalProject" runat="server" DisplayMode="BulletList"
                                CssClass="ApplyStyleForDashBoardLists" ShowMessageBox="false" ShowSummary="true"
                                EnableClientScript="false" HeaderText="Following errors occurred while saving an Internal project."
                                ValidationGroup="InternalProject" />
                            <asp:ValidationSummary ID="vsumProjectAttachment" runat="server" EnableClientScript="false"
                                DisplayMode="BulletList" CssClass="ApplyStyleForDashBoardLists" ShowMessageBox="false"
                                ShowSummary="true" ValidationGroup="ProjectAttachment" />
                            <asp:ValidationSummary ID="vsumCSAT" runat="server" DisplayMode="BulletList" CssClass="ApplyStyleForDashBoardLists"
                                ShowMessageBox="false" ShowSummary="true" EnableClientScript="false" HeaderText="Following errors occurred while saving a CSAT details."
                                ValidationGroup="CSATUpdate" />
                            <asp:ValidationSummary ID="vsumAttribution" runat="server" DisplayMode="BulletList"
                                CssClass="ApplyStyleForDashBoardLists" ValidationGroup="AttributionGroup" ShowMessageBox="false"
                                ShowSummary="true" EnableClientScript="false" HeaderText="Following errors occurred while saving the attribution details." />
                            <asp:ValidationSummary ID="vsumMilestone" runat="server" DisplayMode="BulletList"
                                CssClass="ApplyStyleForDashBoardLists" ValidationGroup="MilestoneTab" ShowMessageBox="false"
                                ShowSummary="true" EnableClientScript="false" HeaderText="Following errors occurred while saving the milestone details." />
                            <%-- <asp:ValidationSummary ID="valsMarginExceptionResponse" runat="server" EnableClientScript="false"
                                CssClass="ApplyStyleForDashBoardLists" ValidationGroup="MarginExceptionResponse" />--%>
                            <asp:ValidationSummary ID="vsMarginExceptionSubmit" runat="server" EnableClientScript="false"
                                CssClass="ApplyStyleForDashBoardLists" ValidationGroup="MarginExceptionSubmit" />
                        </td>
                    </tr>
                    <tr>
                        <td class="Padding10Px TextAlignCenter">
                            <asp:Button ID="btnOKErrorPanel" runat="server" ToolTip="OK" Text="OK" CssClass="Width100PxImp"
                                OnClientClick="$find('mpeErrorPanelBehaviourId').hide();return false;" />

                        </td>
                    </tr>
                </table>
            </asp:Panel>
            <AjaxControlToolkit:ModalPopupExtender ID="mpeTimeEntriesRelatedToitPopup" runat="server"
                TargetControlID="hdnCanShowPopup" CancelControlID="btnClose" BehaviorID="mpeTEsRelatedToItPopup"
                BackgroundCssClass="modalBackground" PopupControlID="pnlPopup" DropShadow="false"
                OkControlID="btnOk" />
            <asp:Panel ID="pnlPopup" runat="server" CssClass="PanelPerson ConfirmBoxClassError"
                Style="display: none">
                <table class="WholeWidth">
                    <tr>
                        <th align="center" class="TextAlignCenter BackGroundColorGray vBottom" colspan="2">
                            <b class="BtnClose">Attention!</b>
                            <asp:Button ID="btnClose" runat="server" CssClass="mini-report-close floatright"
                                ToolTip="Cancel" Text="X"></asp:Button>
                        </th>
                    </tr>
                    <tr>
                        <td class="Padding10Px" colspan="2">
                            <table>
                                <tr>
                                    <td>
                                        <p>
                                            You cannot delete this Work type.Because, there are some time entries related to
                                            it.
                                        </p>
                                        <br />
                                    </td>
                                </tr>
                                <tr>
                                    <td class="TextAlignCenter">
                                        <asp:Button ID="btnOk" runat="server" Text="OK" ToolTip="OK" OnClientClick="return false;" />
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                </table>
            </asp:Panel>
            <AjaxControlToolkit:ModalPopupExtender ID="mpeCompletedStatusPopUp" runat="server"
                TargetControlID="hdnCompletedStatusPopup" BehaviorID="mpeCompletedStatusPopUp"
                BackgroundCssClass="modalBackground" PopupControlID="pnlCompletedStatusPopUp"
                DropShadow="false" />
            <asp:Panel ID="pnlCompletedStatusPopUp" runat="server" CssClass="PanelPerson ConfirmBoxClassError"
                Style="display: none">
                <table class="WholeWidth">
                    <tr>
                        <th align="center" class="TextAlignCenter BackGroundColorGray vBottom" colspan="2">
                            <b class="BtnClose">Attention!</b>
                        </th>
                    </tr>
                    <tr>
                        <td class="Padding10Px" colspan="2">
                            <table>
                                <tr>
                                    <td>
                                        <p>
                                            <asp:CustomValidator ID="cvCompletedStatus" runat="server" ErrorMessage="" ValidationGroup="cvCompletedStatus"></asp:CustomValidator>
                                            <asp:Label ID="lblCompletedStatus" runat="server"></asp:Label>
                                        </p>
                                        <br />
                                    </td>
                                </tr>
                                <tr>
                                    <td class="TextAlignCenter">
                                        <asp:Button ID="btnOkCompletedStatusPopup" runat="server" Text="OK" ToolTip="OK"
                                            OnClick="btnOkCompletedStatusPopup_Click" />
                                        <asp:Button ID="btnCancelCompletedStatusPopup" runat="server" Text="Cancel" ToolTip="Cancel"
                                            OnClick="btnCancelCompletedStatusPopup_Click" />
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                </table>
            </asp:Panel>

            <asp:HiddenField ID="hdnMarginExcSubmit" runat="server" />
            <AjaxControlToolkit:ModalPopupExtender ID="mpeExceptionSubmit" runat="server"
                TargetControlID="hdnMarginExcSubmit" CancelControlID="btnCloseException" BehaviorID="mpeTEsRelatedToItPopup"
                BackgroundCssClass="modalBackground" PopupControlID="pnlSubmitException" DropShadow="false" />
            <asp:Panel ID="pnlSubmitException" runat="server" CssClass="PanelPerson ConfirmBoxClassError"
                Style="display: none">
                <table class="WholeWidth">
                    <tr>
                        <th align="center" class="TextAlignCenter BackGroundColorGray vBottom" colspan="2">
                            <b class="BtnClose">Submit Margin Exception</b>
                        </th>
                    </tr>
                    <tr>
                        <td class="Padding10Px" colspan="2">
                            <table>
                                <tr>
                                    <td>Project:
                                    </td>
                                    <td class="LeftPadding10px">
                                        <asp:Label ID="lblProject" runat="server"></asp:Label>
                                    </td>
                                </tr>
                                <tr>
                                    <td>Requester: </td>
                                    <td class="LeftPadding10px">
                                        <asp:Label ID="lblExceptionRequestor" runat="server"></asp:Label></td>
                                </tr>
                                <tr>
                                    <td>Approval Required: </td>
                                    <td class="LeftPadding10px">
                                        <asp:Label ID="lblExceptionLevel" runat="server"></asp:Label></td>
                                </tr>
                                <tr>
                                    <td>Reason For Exception:</td>
                                    <td class="LeftPadding10px">
                                        <cc2:ScrollingDropDown ID="cblExceptionReason" runat="server" SetDirty="true" CssClass="ProjectDetailScrollingDropDown Width200PxImp"
                                            AllSelectedReturnType="AllItems" onclick="scrollingDropdown_onclick('cblExceptionReason','Reason');"
                                            DropDownListType="Reason" DropDownListTypePluralForm="Reason" />
                                        <ext:ScrollableDropdownExtender ID="ScrollableDropdownExtender2" runat="server" TargetControlID="cblExceptionReason"
                                            Width="200px" UseAdvanceFeature="true" EditImageUrl="Images/Dropdown_Arrow.png">
                                        </ext:ScrollableDropdownExtender>
                                        <asp:CustomValidator ID="cvMarginExceptionSubmit" runat="server" EnableClientScript="false"
                                            ValidationGroup="MarginExceptionSubmit" ErrorMessage="Reason(s) for Exception is required." ValidateEmptyText="true"
                                            OnServerValidate="cvMarginExceptionSubmit_ServerValidate" SetFocusOnError="true" Text="*"
                                            ToolTip="Reason(s) for Exception is required."></asp:CustomValidator>
                                    </td>
                                </tr>
                            </table>
                            <div>
                                Comments<br />
                                <asp:TextBox ID="txtExceptionSubmitComments" runat="server" TextMode="MultiLine" Rows="3" onchange="setDirty();"
                                    CssClass="TextBoxProjectNotes"></asp:TextBox>
                                <asp:RequiredFieldValidator ID="rfv" runat="server" ControlToValidate="txtExceptionSubmitComments"
                                    ErrorMessage="Comments are required." ToolTip="Comments are required."
                                    Text="*" EnableClientScript="false" SetFocusOnError="true" ValidationGroup="MarginExceptionSubmit"></asp:RequiredFieldValidator>
                                <asp:RegularExpressionValidator Display="Dynamic" ControlToValidate="txtExceptionSubmitComments" ID="RegularExpressionValidator3" ValidationExpression="^[\s\S]{5,1000}$" runat="server"
                                    Text="*" ErrorMessage="Minimum characters required is 5, maximum characters allowed is 1000 for comments." EnableClientScript="false" SetFocusOnError="true" ValidationGroup="MarginExceptionSubmit">
                                </asp:RegularExpressionValidator>
                            </div>
                        </td>
                    </tr>
                    <tr>
                        <td class="TextAlignCenter">
                            <asp:Button ID="btnCloseException" runat="server" Text="Cancel" ToolTip="Cancel" OnClientClick="return false;" />
                            <asp:Button ID="btnSubmitException" runat="server" Text="Submit" ToolTip="Submit" OnClick="btnSubmitException_Click" />
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <asp:ValidationSummary ID="ValidationSummary1" runat="server" EnableClientScript="false"
                                CssClass="ApplyStyleForDashBoardLists" ValidationGroup="MarginExceptionSubmit" />
                        </td>
                    </tr>
                </table>
            </asp:Panel>

            <asp:HiddenField ID="hdnMarginExceptionResponse" runat="server" />
            <AjaxControlToolkit:ModalPopupExtender ID="mpeMarginExceptionResponse" runat="server"
                TargetControlID="hdnMarginExceptionResponse" CancelControlID="btnCloseExceptionResponse" BehaviorID="mpeTEsRelatedToItPopup"
                BackgroundCssClass="modalBackground" PopupControlID="pnlSubmitExceptionResponse" DropShadow="false" />
            <asp:Panel ID="pnlSubmitExceptionResponse" runat="server" CssClass="PanelPerson ConfirmBoxClassError"
                Style="display: none">
                <table class="WholeWidth">
                    <tr>
                        <th align="center" class="TextAlignCenter BackGroundColorGray vBottom" colspan="2">
                            <b class="BtnClose">Margin Exception Response</b>
                        </th>
                    </tr>
                    <tr>
                        <td class="Padding10Px" colspan="2">
                            <table>
                                <tr>
                                    <td>Project:
                                    </td>
                                    <td class="LeftPadding10px">
                                        <asp:Label ID="lblProjectResponse" runat="server"></asp:Label>
                                    </td>
                                </tr>
                                <tr>
                                    <td>Requester: </td>
                                    <td class="LeftPadding10px">
                                        <asp:Label ID="lblRequestor" runat="server"></asp:Label></td>
                                </tr>
                                <tr>
                                    <td>Approval Level: </td>
                                    <td class="LeftPadding10px">
                                        <asp:Label ID="lblResponseApprovalLevel" runat="server"></asp:Label></td>
                                </tr>
                                <tr>
                                    <td>Response: </td>
                                    <td class="LeftPadding10px">
                                        <asp:Label ID="lblResponse" runat="server"></asp:Label></td>
                                </tr>
                                <tr>
                                    <td>Reason For Exception:</td>
                                    <td class="LeftPadding10px">
                                        <cc2:ScrollingDropDown ID="cblExceptionResponseReasons" runat="server" SetDirty="true" CssClass="ProjectDetailScrollingDropDown Width200PxImp"
                                            AllSelectedReturnType="AllItems" onclick="scrollingDropdown_onclick('cblExceptionResponseReasons','Reason');" Width="200px"
                                            DropDownListType="Reason" DropDownListTypePluralForm="Reason" />
                                        <ext:ScrollableDropdownExtender ID="ScrollableDropdownExtender1" runat="server" TargetControlID="cblExceptionResponseReasons"
                                            Width="200px" UseAdvanceFeature="true" EditImageUrl="Images/Dropdown_Arrow.png">
                                        </ext:ScrollableDropdownExtender>
                                        <asp:CustomValidator ID="cvExpResonseReason" runat="server" EnableClientScript="false"
                                            ValidationGroup="MarginExceptionResponse" ErrorMessage="Reason(s) for Exception is required." ValidateEmptyText="true"
                                            OnServerValidate="cvExpResonseReason_ServerValidate" SetFocusOnError="true" Text="*"
                                            ToolTip="Reason(s) for Exception is required."></asp:CustomValidator>
                                    </td>
                                </tr>
                            </table>
                            <div>
                                Comments<br />
                                <asp:TextBox ID="txtResponseComments" runat="server" TextMode="MultiLine" Rows="3" onchange="setDirty();"
                                    CssClass="TextBoxProjectNotes"></asp:TextBox>
                                <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ControlToValidate="txtResponseComments"
                                    ErrorMessage="Comments are required." ToolTip="Comments are required."
                                    Text="*" EnableClientScript="false" SetFocusOnError="true" ValidationGroup="MarginExceptionResponse"></asp:RequiredFieldValidator>
                                <asp:RegularExpressionValidator Display="Dynamic" ControlToValidate="txtResponseComments" ID="RegularExpressionValidator1" ValidationExpression="^[\s\S]{5,1000}$" runat="server"
                                    Text="*" ErrorMessage="Minimum characters required is 5, maximum characters allowed is 1000 for comments." EnableClientScript="false" SetFocusOnError="true" ValidationGroup="MarginExceptionResponse">
                                </asp:RegularExpressionValidator>
                            </div>
                        </td>
                    </tr>

                    <tr>
                        <td class="TextAlignCenter">
                            <asp:Button ID="btnCloseExceptionResponse" runat="server" Text="Cancel" ToolTip="Cancel" OnClientClick="return false;" />
                            <asp:Button ID="btnSubmitExceptionResponse" runat="server" Text="Submit" ToolTip="Submit" OnClick="btnSubmitExceptionResponse_Click" />
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <asp:ValidationSummary ID="valsMarginExceptionResponse" runat="server" EnableClientScript="false"
                                CssClass="ApplyStyleForDashBoardLists" ValidationGroup="MarginExceptionResponse" />
                        </td>
                    </tr>
                </table>
            </asp:Panel>
            <asp:HiddenField ID="HiddenField1" runat="server" />
            <asp:HiddenField ID="hdnBudgetStatus" runat="server" />
            <AjaxControlToolkit:ModalPopupExtender ID="mpeBudgetReset" runat="server"
                TargetControlID="HiddenField1" CancelControlID="btnBudgetResetCancel" BehaviorID="mpeTEsRelatedToItPopup"
                BackgroundCssClass="modalBackground" PopupControlID="pnlBudgetReset" DropShadow="false" />
            <asp:Panel ID="pnlBudgetReset" runat="server" CssClass="PanelPerson ConfirmBoxClassError"
                Style="display: none" Width="30%">
                <table class="WholeWidth">
                    <tr>
                        <th align="center" class="TextAlignCenter BackGroundColorGray vBottom" colspan="2">
                            <b class="BtnClose">
                                <asp:Label ID="lblBudgetPopUpHeader" runat="server"></asp:Label>

                            </b>
                        </th>
                    </tr>
                    <tr>
                        <td class="Padding10Px" colspan="2">
                            <table>
                                <tr>
                                    <td><b>Project:</b>
                                    </td>
                                    <td class="LeftPadding10px">
                                        <asp:Label ID="lblPrjName" runat="server"></asp:Label>
                                    </td>
                                </tr>
                                <tr>
                                    <td><b>Requester:</b> </td>
                                    <td class="LeftPadding10px">
                                        <asp:Label ID="lblBudgetResetRequestor" runat="server"></asp:Label></td>
                                </tr>
                                <tr id="tblRevisionReason" runat="server">
                                    <td><b>Reason For Revision:</b></td>
                                    <td class="LeftPadding10px">
                                        <cc2:ScrollingDropDown ID="cblBudgetRevisionReason" runat="server" SetDirty="true" CssClass="ProjectDetailScrollingDropDown Width200PxImp"
                                            AllSelectedReturnType="AllItems" onclick="scrollingDropdown_onclick('cblBudgetRevisionReason','Reason');" Width="200px"
                                            DropDownListType="Reason" DropDownListTypePluralForm="Reason" />
                                        <ext:ScrollableDropdownExtender ID="ScrollableDropdownExtender3" runat="server" TargetControlID="cblBudgetRevisionReason"
                                            Width="200px" UseAdvanceFeature="true" EditImageUrl="Images/Dropdown_Arrow.png">
                                        </ext:ScrollableDropdownExtender>

                                        <asp:CustomValidator ID="cvBudgetRevisionReason" runat="server" EnableClientScript="false"
                                            ValidationGroup="BudgetRejection" ErrorMessage="Reason(s) for Budget Revision is required." ValidateEmptyText="true"
                                            OnServerValidate="cvBudgetRevisionReason_ServerValidate" SetFocusOnError="true" Text="*"
                                            ToolTip="Reason(s) for Budget Revision is required."></asp:CustomValidator>
                                    </td>
                                </tr>
                            </table>
                            <div>
                                <b>Comments:</b>
                                <br />
                                <asp:TextBox ID="txtBudgetRejectionComments" runat="server" TextMode="MultiLine" Rows="3"
                                    CssClass="TextBoxProjectNotes"></asp:TextBox>
                                <asp:RequiredFieldValidator ID="rfvBudgetRejectionComments" runat="server" ControlToValidate="txtBudgetRejectionComments"
                                    ErrorMessage="Comments are required." ToolTip="Comments are required."
                                    Text="*" EnableClientScript="false" SetFocusOnError="true" ValidationGroup="BudgetRejection"></asp:RequiredFieldValidator>
                                <asp:RegularExpressionValidator Display="Dynamic" ControlToValidate="txtBudgetRejectionComments" ID="RegularExpressionValidator2" ValidationExpression="^[\s\S]{5,1000}$" runat="server"
                                    Text="*" ErrorMessage="Minimum characters required is 5, maximum characters allowed is 1000 for comments." EnableClientScript="false" SetFocusOnError="true" ValidationGroup="BudgetRejection">
                                </asp:RegularExpressionValidator>
                            </div>
                            <div id="divResetParameters" runat="server">
                                <b>Budget Revision Includes: (<span style="font-style: italic">select one</span>)</b>
                                <asp:RequiredFieldValidator ID="rfvBudgetResetType" runat="server" ControlToValidate="rbnBudgetResetType"
                                    ErrorMessage="The Budget Revision type is required." ToolTip="The Budget Revision type is required."
                                    Text="*" EnableClientScript="false" SetFocusOnError="true" ValidationGroup="BudgetRejection"></asp:RequiredFieldValidator>
                                <asp:RadioButtonList ID="rbnBudgetResetType" runat="server" onchange="OnResetType();">
                                    <asp:ListItem Text="Change Order (Original Budget + CO Milestone)" Value="1"></asp:ListItem>
                                    <asp:ListItem Text="Change Order and General Changes (Budget to Date + Projected Remaining + CO Milestone)" Value="2"></asp:ListItem>
                                    <asp:ListItem Text="General Changes (Budget to Date + Projected Remaining)" Value="3"></asp:ListItem>
                                </asp:RadioButtonList>
                                <br />
                                <table id="spBudgetToDate" runat="server">
                                    <tr>
                                        <td>&nbsp;&nbsp; Budget to Date:&nbsp;
                                        </td>
                                        <td>
                                            <ucDate:DatePicker ID="dtpBudgetToDate" runat="server" ValidationGroup="BudgetRejection" class="BudgetTodate" />
                                        </td>
                                        <td>
                                            <asp:RequiredFieldValidator ID="reqBudgetToDate" runat="server" ControlToValidate="dtpBudgetToDate"
                                                ErrorMessage="The Budget to date is required." ToolTip="The Budget to date is required."
                                                Text="*" EnableClientScript="false" SetFocusOnError="true" ValidationGroup="BudgetRejection"></asp:RequiredFieldValidator>
                                            <asp:CompareValidator ID="compBudgetToDate" runat="server" ControlToValidate="dtpBudgetToDate"
                                                ErrorMessage="The Budget to Date has an incorrect format. It must be 'MM/dd/yyyy'."
                                                ToolTip="The Budget to Date has an incorrect format. It must be 'MM/dd/yyyy'." Text="*"
                                                EnableClientScript="false" SetFocusOnError="true" Display="Dynamic" Operator="DataTypeCheck"
                                                Type="Date" ValidationGroup="BudgetRejection"></asp:CompareValidator>
                                            <asp:CustomValidator ID="cvBudgetToDate" runat="server" EnableClientScript="false"
                                                ValidationGroup="BudgetRejection" ErrorMessage="Selected Budget to date should be with in the project period." ValidateEmptyText="true"
                                                OnServerValidate="cvBudgetToDate_ServerValidate" SetFocusOnError="true" Text="*"
                                                ToolTip="Selected Budget to date should be with in the project period."></asp:CustomValidator>
                                            <asp:CustomValidator ID="cvFFBudgetResetDate" runat="server" EnableClientScript="false"
                                                ValidationGroup="BudgetRejection" ErrorMessage="Date must be the first of the month." ValidateEmptyText="true"
                                                OnServerValidate="cvFFBudgetResetDate_ServerValidate" SetFocusOnError="true" Text="*"
                                                ToolTip="Date must be the first of the month."></asp:CustomValidator>
                                        </td>
                                    </tr>
                                </table>


                            </div>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <asp:ValidationSummary ID="valSumBudgetRejection" runat="server" EnableClientScript="false"
                                CssClass="ApplyStyleForDashBoardLists" ValidationGroup="BudgetRejection" />
                        </td>
                    </tr>
                    <tr>
                        <td class="TextAlignCenter">
                            <asp:Button ID="btnBudgetResetCancel" runat="server" Text="Cancel" ToolTip="Cancel" OnClientClick="return false;" />
                            <asp:Button ID="btnBudgetResetSubmit" runat="server" Text="Submit" ToolTip="Submit" OnClick="btnBudgetResetSubmit_Click" />
                        </td>
                    </tr>

                </table>
            </asp:Panel>

            <asp:Button ID="btnDownloadButton" runat="server" OnClick="lnkProjectAttachment_OnClick"
                Style="display: none;" />
            <asp:HiddenField ID="hdnDownloadAttachmentId" runat="server" Value="" />
        </ContentTemplate>
        <Triggers>
            <asp:PostBackTrigger ControlID="btnDownloadButton" />
            <asp:PostBackTrigger ControlID="budgetManagementByProject$btnExport" />

        </Triggers>
    </asp:UpdatePanel>
</asp:Content>

