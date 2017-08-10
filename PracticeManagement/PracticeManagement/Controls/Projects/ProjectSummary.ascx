<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ProjectSummary.ascx.cs"
    Inherits="PraticeManagement.Controls.Projects.ProjectSummary" %>
<%@ Import Namespace="PraticeManagement.Utils" %>
<%@ Register TagPrefix="asp" Namespace="PraticeManagement.Controls.Generic.Buttons"
    Assembly="PraticeManagement" %>
<%@ Register TagPrefix="ext" Assembly="PraticeManagement" Namespace="PraticeManagement.Controls.Generic.ScrollableDropdown" %>
<%@ Register Src="~/Controls/Generic/LoadingProgress.ascx" TagName="LoadingProgress"
    TagPrefix="uc3" %>
<%@ Register Src="~/Controls/MessageLabel.ascx" TagName="Label" TagPrefix="uc" %>
<%@ Register Src="~/Controls/Generic/Filtering/DateInterval.ascx" TagPrefix="uc"
    TagName="DateInterval" %>
<script type="text/javascript">

    var isBudgetManagement = false;

    $(document).ready(function () {
        if (window.location.href.indexOf("BudgetManagement.aspx") > -1) {
            isBudgetManagement = true;
        }
    });

    function pageLoad() {
        SetActualDropDown();
    }

    function SetActualDropDown() {
        if (isBudgetManagement) {
            $(".tdActual").css("display", "none");
        }
        else {
            var selectedCalculationtype = $("[id$='ddlCalculationsType']").val();
            if (selectedCalculationtype == "2" || selectedCalculationtype == "4") {
                $(".tdActual").css("display", "");
            }
            else {
                $(".tdActual").css("display", "none");
            }
        }
    }

    function setPosition(item, ytop, xleft) {
        item.offset({ top: ytop, left: xleft });
    }

    function SetTooltipText(descriptionText, hlinkObj) {
        var hlinkObjct = $(hlinkObj);
        var displayPanel = $('#<%= pnlProjectToolTipHolder.ClientID %>');
        iptop = hlinkObjct.offset().top;
        ipleft = hlinkObjct.offset().left + hlinkObjct[0].offsetWidth + 5;
        setPosition(displayPanel, iptop - 20, ipleft);
        displayPanel.show();
        setPosition(displayPanel, iptop - 20, ipleft);
        displayPanel.show();

        var lblProjectTooltip = document.getElementById('<%= lblProjectTooltip.ClientID %>');
        lblProjectTooltip.innerHTML = descriptionText.toString();
    }

    function HidePanel() {
        var displayPanel = $('#<%= pnlProjectToolTipHolder.ClientID %>');
        displayPanel.hide();
    }

    function HideColumns() {
        $(".hideCol").css("display", "none");
    }

    function ShowDetailedColumns() {
        $(".hideCol").css("display", "");
        $(".hideMonthCol").css("display", "none");
    }

    function HideDetailedColumns() {
        $(".hideCol").css("display", "none");
        $(".hideMonthCol").css("display", "none");
    }



    var IsExportAllDisplayed = false;

    function imgArrow_click() {
        IsExportAllDisplayed = true;
        var menu = document.getElementById('popupmenu');
        menu.style.display = 'block';
    }

    function imgArrow_mouseOver() {
        IsExportAllDisplayed = true;
    }

    function imgArrow_mouseOut() {
        IsExportAllDisplayed = false;
        setTimeout(function () {
            if (!IsExportAllDisplayed) {
                var menu = document.getElementById('popupmenu');
                menu.style.display = 'none';
            }

        }, 500);
    }

    function Exportall_click_mouseOver() {
        IsExportAllDisplayed = true;
        var menu = document.getElementById('popupmenu');
        menu.style.display = 'block';
    }

    function excludeDualSelection(target) {
        var targetElement = $get(target);

        if (targetElement)
            if (targetElement.checked)
                targetElement.checked = false;
    }

    function RemoveExtraCharAtEnd(url) {
        if (url.lastIndexOf('#') == url.length - 1) {
            return url.substring(0, url.length - 1);
        }
        else {
            return url;
        }
    }

    function correctMonthMiniReportPosition(reportPanelId, headerId, scrollPanelId) {
        var reportPanel = $get(reportPanelId);
        var header = $get(headerId);
        var scrollPanel = $get(scrollPanelId);

        var reportPanelPos = getPosition(reportPanel);
        var headerPosition = getPosition(header);

        reportPanel.style.left = headerPosition.left - scrollPanel.scrollLeft - 25;
    }

    function getPosition(obj) {
        var topValue = 0, leftValue = 0;
        while (obj) {
            leftValue += obj.offsetLeft;
            topValue += obj.offsetTop;
            obj = obj.offsetParent;
        }

        function point() {
            this.left = 0;
            this.top = 0;
        }

        var result = new point();
        result.left = leftValue;
        result.top = topValue;

        return result;
    }

    function EnableOrDisableGroup() {
        var cbl = document.getElementById("<%= cblClient.ClientID %>");
        var arrayOfCheckBoxes = cbl.getElementsByTagName("input");
        var cblList = $("div[id^='sdeCblProjectGroup']");
        var anySingleItemChecked = "false";
        for (var i = 0; i < arrayOfCheckBoxes.length; i++) {
            if (arrayOfCheckBoxes[i].checked) {
                anySingleItemChecked = "true";
            }
        }

        if (anySingleItemChecked == "true") {
            cblList[0].disabled = "";
        }
        else {
            cblList[0].disabled = "disabled";
        }
        custom_ScrollingDropdown_onclick('cblProjectGroup', 'Business Unit');
    }

    function resetFiltersTab() {
        GetDefaultcblList();
        GetDefault(document.getElementById("<%= cblClient.ClientID %>"));
        GetDefault(document.getElementById("<%= cblSalesperson.ClientID %>"));
        GetDefault(document.getElementById("<%= cblPractice.ClientID %>"));
        GetDefault(document.getElementById("<%= cblProjectOwner.ClientID %>"));

        var scrollingDropdownList = document.getElementById('cblProjectGroup');
        var arrayOfTRs = scrollingDropdownList.getElementsByTagName("tr");
        for (var i = 0; i < arrayOfTRs.length; i++) {
            arrayOfTRs[i].removeAttribute('class');
        }

        custom_ScrollingDropdown_onclick('cblProjectGroup', 'Business Unit');

    }

    function GetDefaultcblList() {
        var div = document.getElementById("<%= divProjectFilter.ClientID %>");
        var arrayOfCheckBoxes = div.getElementsByTagName('input');
        for (var i = 0; i < arrayOfCheckBoxes.length; i++) {
            arrayOfCheckBoxes[i].checked = true;
        }
    }

    function GetDefault(control) {
        control.click();
        //control.fireEvent('onclick');
    }

    function resetCalculationsTab() {
        document.getElementById("<%= chbActive.ClientID %>").checked = true;
        document.getElementById("<%= chbInternal.ClientID %>").checked = false;
        document.getElementById("<%= chbProjected.ClientID %>").checked = true;
        document.getElementById("<%= chbCompleted.ClientID %>").checked = true;
        document.getElementById("<%= chbExperimental.ClientID %>").checked = false;
        document.getElementById("<%= chbProposed.ClientID %>").checked = false;
        document.getElementById("<%= chbInactive.ClientID %>").checked = false;
        document.getElementById("<%= chbAtRisk.ClientID %>").checked = true;
        document.getElementById("<%= ddlSupressZeroBalance.ClientID %>").selectedIndex = 0;

        var div = document.getElementById("<%= divDataPoints.ClientID %>");
        var arrayOfCheckBoxes = div.getElementsByTagName('input');
        for (var i = 0; i < arrayOfCheckBoxes.length; i++) {
            arrayOfCheckBoxes[i].checked = i == 1;
        }

        var calculationType = document.getElementById("<%= ddlCalculationsType.ClientID %>");
        var level = document.getElementById("<%= ddlLevel.ClientID %>");
        if (level === null) {
            calculationType.selectedIndex = 3;
        }
        else {
            level.selectedIndex = 0;
            calculationType.selectedIndex = 0;

            var divFee = document.getElementById("<%= divFeeType.ClientID %>");
            var arrayOfCheckBoxes = divFee.getElementsByTagName('input');
            for (var i = 0; i < arrayOfCheckBoxes.length; i++) {
                arrayOfCheckBoxes[i].checked = true;
            }
            GetDefault(document.getElementById("<%= ddlFeeType.ClientID %>"));
        }

        GetDefault(document.getElementById("<%= ddldataPoints.ClientID %>"));
        SetActualDropDown();
    }

    function custom_ScrollingDropdown_onclick(control, type) {
        var temp = 0;
        var text = "";
        var scrollingDropdownList = document.getElementById(control.toString());
        var arrayOfCheckBoxes = scrollingDropdownList.getElementsByTagName("input");
        if (arrayOfCheckBoxes.length == 1 && arrayOfCheckBoxes[0].disabled) {
            text = "No " + type.toString() + "s to select.";
        }
        else {
            for (var i = 0; i < arrayOfCheckBoxes.length; i++) {

                if (arrayOfCheckBoxes[i].checked) {
                    if (arrayOfCheckBoxes[i].parentNode.parentNode.style.display != "none") {
                        temp++;
                        text = arrayOfCheckBoxes[i].parentNode.childNodes[1].innerHTML;
                    }
                }
                if (temp > 1) {
                    text = "Multiple " + type.toString() + "s selected";

                }
                if (arrayOfCheckBoxes[0].checked) {
                    text = arrayOfCheckBoxes[0].parentNode.childNodes[1].innerHTML;
                }
                if (temp === 0) {
                    text = "Select " + type.toString() + "(s)";
                }
            }
            if (text.length > 32) {
                text = text.substr(0, 30) + "..";
            }
            scrollingDropdownList.parentNode.children[1].children[0].firstChild.nodeValue = text;
        }
    }

    Sys.WebForms.PageRequestManager.getInstance().add_endRequest(endRequestHandle);

    function endRequestHandle(sender, Args) {
        addListenersToParent('<%= cblProjectGroup.ClientID %>', '<%= cblClient.ClientID %>');
    }

    function ReAssignStartDateEndDates() {
        hdnStartDate = document.getElementById('<%= hdnStartDate.ClientID %>');
        hdnEndDate = document.getElementById('<%= hdnEndDate.ClientID %>');
        hdnStartDateTxtBoxId = document.getElementById('<%= hdnStartDateTxtBoxId.ClientID %>');
        hdnEndDateTxtBoxId = document.getElementById('<%= hdnEndDateTxtBoxId.ClientID %>');
        txtStartDate = document.getElementById(hdnStartDateTxtBoxId.value);
        txtEndDate = document.getElementById(hdnEndDateTxtBoxId.value);
        txtStartDate.value = hdnStartDate.value;
        txtEndDate.value = hdnEndDate.value;
    }

    function CheckIfDatesValid() {
        hdnStartDateTxtBoxId = document.getElementById('<%= hdnStartDateTxtBoxId.ClientID %>');
        hdnEndDateTxtBoxId = document.getElementById('<%= hdnEndDateTxtBoxId.ClientID %>');
        txtStartDate = document.getElementById(hdnStartDateTxtBoxId.value);
        txtEndDate = document.getElementById(hdnEndDateTxtBoxId.value);
        var startDate = new Date(txtStartDate.value);
        var EtartDate = new Date(txtEndDate.value);
        if (txtStartDate.value != '' && txtEndDate.value != ''
            && startDate <= EtartDate) {
            var btnCustDatesClose = document.getElementById('<%= btnCustDatesClose.ClientID %>');
            btnCustDatesClose.click();
        }
    }

    function CheckAndShowCustomDatesPoup(ddlPeriod) {
        if (ddlPeriod.value == '0') {
            imgCalender = document.getElementById('<%= imgCalender.ClientID %>');
            lblCustomDateRange = document.getElementById('<%= lblCustomDateRange.ClientID %>');
            if (imgCalender.fireEvent) {
                imgCalender.attributes["class"].value = "";
                lblCustomDateRange.attributes["class"].value = "fontBold";
                imgCalender.click();
            }
            if (document.createEvent) {
                imgCalender.attributes["class"].value = "";
                lblCustomDateRange.attributes["class"].value = "fontBold";
                var event = document.createEvent('HTMLEvents');
                event.initEvent('click', true, true);
                imgCalender.dispatchEvent(event);
            }
        }
        else {
            imgCalender.attributes["class"].value = "displayNone";
            lblCustomDateRange.attributes["class"].value = "displayNone";
        }
    }


</script>
<asp:UpdatePanel ID="flrPanel" runat="server">
    <ContentTemplate>
        <div class="filters">
            <div class="buttons-block">
                <table class="WholeWidth BorderNone LeftPadding10px">
                    <tr>
                        <td class="Width3Percent">
                            <AjaxControlToolkit:CollapsiblePanelExtender ID="cpe" runat="Server" TargetControlID="pnlFilters"
                                ImageControlID="btnExpandCollapseFilter" CollapsedImage="~/Images/expand.jpg"
                                ExpandedImage="~/Images/collapse.jpg" CollapseControlID="btnExpandCollapseFilter"
                                ExpandControlID="btnExpandCollapseFilter" Collapsed="True" TextLabelID="lblFilter" />
                            <asp:Label ID="lblFilter" runat="server"></asp:Label>&nbsp;
                            <asp:Image ID="btnExpandCollapseFilter" runat="server" ImageUrl="~/Images/collapse.jpg"
                                ToolTip="Search, Filter and Calculation Options" />
                        </td>
                        <td class="Width8Percent">&nbsp;Show&nbsp;Projects&nbsp;for&nbsp;
                        </td>
                        <td class="Width8Percent">
                            <asp:DropDownList ID="ddlPeriod" runat="server" Onchange=" return CheckAndShowCustomDatesPoup(this);">
                                <asp:ListItem Text="Next 3 months" Value="3"></asp:ListItem>
                                <asp:ListItem Text="Next 6 months" Value="6"></asp:ListItem>
                                <asp:ListItem Text="Next 12 months" Value="12"></asp:ListItem>
                                <asp:ListItem Text="Last 3 months" Value="-3"></asp:ListItem>
                                <asp:ListItem Text="Last 6 months" Value="-6"></asp:ListItem>
                                <asp:ListItem Text="Last 12 months" Value="-12"></asp:ListItem>
                                <asp:ListItem Text="Previous FY" Value="-13"></asp:ListItem>
                                <asp:ListItem Text="Current FY" Value="13"></asp:ListItem>
                                <asp:ListItem Text="Custom Dates" Value="0"></asp:ListItem>
                            </asp:DropDownList>
                            <AjaxControlToolkit:ModalPopupExtender ID="mpeCustomDates" runat="server" TargetControlID="imgCalender"
                                CancelControlID="btnCustDatesCancel" OkControlID="btnCustDatesClose" BackgroundCssClass="modalBackground"
                                PopupControlID="pnlCustomDates" BehaviorID="bhCustomDates" DropShadow="false"
                                OnCancelScript="ReAssignStartDateEndDates();" />
                            <asp:HiddenField ID="hdnStartDate" runat="server" Value="" />
                            <asp:HiddenField ID="hdnEndDate" runat="server" Value="" />
                            <asp:HiddenField ID="hdnStartDateTxtBoxId" runat="server" Value="" />
                            <asp:HiddenField ID="hdnEndDateTxtBoxId" runat="server" Value="" />
                        </td>
                        <td class="Width48Percent padLeft5">
                            <asp:Label ID="lblCustomDateRange" runat="server" Text=""></asp:Label>
                            <asp:Image ID="imgCalender" runat="server" ImageUrl="~/Images/calendar.gif" />
                        </td>
                        <td class="alignRight" colspan="2">
                            <asp:DropDownList ID="ddlView" runat="server">
                                <asp:ListItem Text="View 10" Value="10"></asp:ListItem>
                                <asp:ListItem Text="View 25" Value="25"></asp:ListItem>
                                <asp:ListItem Text="View 50" Value="50"></asp:ListItem>
                                <asp:ListItem Text="View ALL" Value="1"></asp:ListItem>
                            </asp:DropDownList>
                            &nbsp; &nbsp;
                            <asp:ShadowedHyperlink runat="server" Text="Add Project" ID="lnkAddProject" CssClass="add-btn"
                                NavigateUrl="~/ProjectDetail.aspx?from=sub_toolbar&returnTo=Projects.aspx" />
                        </td>
                    </tr>
                    <tr>
                        <td>&nbsp;
                        </td>
                        <td colspan="3" class="buttons-blockCheckBox PaddingTop4Px"></td>
                        <td class="PaddingTop5">
                            <asp:Button ID="btnUpdateFilters" runat="server" Text="Refresh Report" OnClick="btnUpdateView_Click"
                                ValidationGroup="Filter" EnableViewState="False" CssClass="Width100Px" />
                        </td>
                        <td align="right" style="width: 2px">
                            <table>
                                <tr class="PaddingTop5 padRight5">
                                    <td class="PaddingTop5">
                                        <asp:Button ID="btnExportToExcel" runat="server"
                                            Text="Export" CssClass="WholeWidth" Width="80px" Style="margin-right: 0px;" />
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                </table>
            </div>
            <asp:Panel ID="pnlFilters" runat="server">
                <AjaxControlToolkit:TabContainer ID="tcFilters" runat="server" ActiveTabIndex="0"
                    CssClass="CustomTabStyle">
                    <AjaxControlToolkit:TabPanel ID="tpSearch" runat="server">
                        <HeaderTemplate>
                            <span class="bg"><a href="#"><span>Search</span></a> </span>
                        </HeaderTemplate>
                        <ContentTemplate>
                            <asp:Panel ID="pnlSearch" runat="server" CssClass="Height80Px" DefaultButton="btnSearchAll">
                                <table class="WholeWidth">
                                    <tr>
                                        <td class="padRight8 padLeft4">
                                            <asp:TextBox ID="txtSearchText" runat="server" CssClass="WholeWidth" EnableViewState="False" />
                                        </td>
                                        <td class="Width10Px">
                                            <asp:RequiredFieldValidator ID="reqSearchText" runat="server" ControlToValidate="txtSearchText"
                                                ErrorMessage="Please type text to be searched." ToolTip="Please type text to be searched."
                                                Text="*" SetFocusOnError="true" ValidationGroup="Search" CssClass="searchError"
                                                Display="Static" />
                                        </td>
                                        <td>
                                            <asp:Button ID="btnSearchAll" runat="server" Text="Search All" ValidationGroup="Search"
                                                PostBackUrl="~/ProjectSearch.aspx" CssClass="Width100Px" EnableViewState="False" />
                                        </td>
                                        <td></td>
                                    </tr>
                                    <tr>
                                        <td>
                                            <asp:ValidationSummary ID="valsSearch" runat="server" ValidationGroup="Search" EnableClientScript="true"
                                                ShowMessageBox="false" CssClass="searchError" />
                                        </td>
                                    </tr>
                                </table>
                            </asp:Panel>
                        </ContentTemplate>
                    </AjaxControlToolkit:TabPanel>
                    <AjaxControlToolkit:TabPanel ID="tpAdvancedFilters" runat="server">
                        <HeaderTemplate>
                            <span class="bg"><a href="#"><span>Filters</span></a> </span>
                        </HeaderTemplate>
                        <ContentTemplate>
                            <div id="divProjectFilter" runat="server">
                                <table class="WholeWidth Height80Px">
                                    <tr class="tb-header ProjectSummaryAdvancedFiltersHeader">
                                        <th>Account / Business Unit
                                        </th>
                                        <td class="Padding5 Width10Px"></td>
                                        <th>Sales Team
                                        </th>
                                        <td class="Padding5 Width10Px"></td>
                                        <th>Division / Practice Area
                                        </th>
                                        <td class="Padding5 Width10Px"></td>
                                        <th>Revenue Type / Offering
                                        </th>
                                        <td class="Padding5 Width10Px"></td>
                                        <th>Channel
                                        </th>
                                        <td rowspan="3">
                                            <table class="textRight WholeWidth">
                                                <tr>
                                                    <td></td>
                                                </tr>
                                                <tr>
                                                    <td class="PaddingTop10">
                                                        <asp:Button ID="btnResetFilters" runat="server" Text="Reset" CausesValidation="False"
                                                            OnClientClick="resetFiltersTab(); return false;" EnableViewState="False" CssClass="Width100Px" />
                                                    </td>
                                                </tr>
                                            </table>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td class="Padding5">
                                            <pmc:CascadingMsdd ID="cblClient" runat="server" TargetControlId="cblProjectGroup"
                                                SetDirty="false" CssClass="ProjectSummaryScrollingDropDown" onclick="scrollingDropdown_onclick('cblClient','Account');EnableOrDisableGroup();"
                                                DropDownListType="Account" />
                                            <ext:ScrollableDropdownExtender ID="sdeCblClient" runat="server" TargetControlID="cblClient"
                                                UseAdvanceFeature="true" EditImageUrl="~/Images/Dropdown_Arrow.png" Width="240px">
                                            </ext:ScrollableDropdownExtender>
                                        </td>
                                        <td></td>
                                        <td>
                                            <pmc:ScrollingDropDown ID="cblSalesperson" runat="server" SetDirty="false" CssClass="ProjectSummaryScrollingDropDown"
                                                onclick="scrollingDropdown_onclick('cblSalesperson','Salesperson')" DropDownListType="Salesperson" />
                                            <ext:ScrollableDropdownExtender ID="sdeCblSalesperson" runat="server" TargetControlID="cblSalesperson"
                                                UseAdvanceFeature="true" EditImageUrl="~/Images/Dropdown_Arrow.png" Width="240px">
                                            </ext:ScrollableDropdownExtender>
                                        </td>
                                        <td></td>
                                        <td>
                                            <pmc:ScrollingDropDown ID="cblDivision" runat="server" SetDirty="false" CssClass="ProjectSummaryScrollingDropDown"
                                                onclick="scrollingDropdown_onclick('cblDivision','Division')" DropDownListType="Division" />
                                            <ext:ScrollableDropdownExtender ID="sdecblDivision" runat="server" TargetControlID="cblDivision"
                                                UseAdvanceFeature="true" EditImageUrl="~/Images/Dropdown_Arrow.png" Width="240px">
                                            </ext:ScrollableDropdownExtender>
                                        </td>
                                        <td></td>
                                        <td>
                                            <pmc:ScrollingDropDown ID="cblRevenueType" runat="server" SetDirty="false" CssClass="ProjectSummaryScrollingDropDown"
                                                onclick="scrollingDropdown_onclick('cblRevenueType','RevenueType')" DropDownListType="Revenue Type" />
                                            <ext:ScrollableDropdownExtender ID="sdecblRevenueType" runat="server" TargetControlID="cblRevenueType"
                                                UseAdvanceFeature="true" EditImageUrl="~/Images/Dropdown_Arrow.png" Width="240px">
                                            </ext:ScrollableDropdownExtender>
                                        </td>
                                        <td></td>
                                        <td>
                                            <pmc:ScrollingDropDown ID="cblChannel" runat="server" SetDirty="false" CssClass="ProjectSummaryScrollingDropDown"
                                                onclick="scrollingDropdown_onclick('cblChannel','Channel')" DropDownListType="Channel" />
                                            <ext:ScrollableDropdownExtender ID="sdecblChannel" runat="server" TargetControlID="cblChannel"
                                                UseAdvanceFeature="true" EditImageUrl="~/Images/Dropdown_Arrow.png" Width="240px">
                                            </ext:ScrollableDropdownExtender>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td class="Padding5">
                                            <pmc:ScrollingDropDown ID="cblProjectGroup" runat="server" SetDirty="false" CssClass="ProjectSummaryScrollingDropDown"
                                                onclick="custom_ScrollingDropdown_onclick('cblProjectGroup','Business Unit')"
                                                DropDownListType="Business Unit" />
                                            <ext:ScrollableDropdownExtender ID="sdeCblProjectGroup" runat="server" TargetControlID="cblProjectGroup"
                                                UseAdvanceFeature="true" EditImageUrl="~/Images/Dropdown_Arrow.png" Width="240px">
                                            </ext:ScrollableDropdownExtender>
                                        </td>
                                        <td></td>
                                        <td>
                                            <pmc:ScrollingDropDown ID="cblProjectOwner" runat="server" SetDirty="false" CssClass="ProjectSummaryScrollingDropDown"
                                                onclick="scrollingDropdown_onclick('cblProjectOwner','Project Access','es','Project Accesses',33);"
                                                DropDownListType="Project Access" DropDownListTypePluralForm="Project Accesses"
                                                PluralForm="es" />
                                            <ext:ScrollableDropdownExtender ID="sdeCblProjectOwner" runat="server" TargetControlID="cblProjectOwner"
                                                UseAdvanceFeature="true" EditImageUrl="~/Images/Dropdown_Arrow.png" Width="240px">
                                            </ext:ScrollableDropdownExtender>
                                        </td>
                                        <td></td>
                                        <td>
                                            <pmc:ScrollingDropDown ID="cblPractice" runat="server" SetDirty="false" CssClass="ProjectSummaryScrollingDropDown"
                                                onclick="scrollingDropdown_onclick('cblPractice','Practice Area')" DropDownListType="Practice Area" />
                                            <ext:ScrollableDropdownExtender ID="sdeCblPractice" runat="server" TargetControlID="cblPractice"
                                                UseAdvanceFeature="true" EditImageUrl="~/Images/Dropdown_Arrow.png" Width="240px">
                                            </ext:ScrollableDropdownExtender>
                                        </td>
                                        <td></td>
                                        <td>
                                            <pmc:ScrollingDropDown ID="cblOffering" runat="server" SetDirty="false" CssClass="ProjectSummaryScrollingDropDown"
                                                onclick="scrollingDropdown_onclick('cblOffering','Offering')" DropDownListType="Offering" />
                                            <ext:ScrollableDropdownExtender ID="sdecblOffering" runat="server" TargetControlID="cblOffering"
                                                UseAdvanceFeature="true" EditImageUrl="~/Images/Dropdown_Arrow.png" Width="240px">
                                            </ext:ScrollableDropdownExtender>
                                        </td>
                                        <td></td>
                                        <td></td>
                                    </tr>
                                </table>
                            </div>
                        </ContentTemplate>
                    </AjaxControlToolkit:TabPanel>
                    <AjaxControlToolkit:TabPanel runat="server" ID="tpMainFilters">
                        <HeaderTemplate>
                            <span class="bg"><a href="#"><span>View</span></a> </span>
                        </HeaderTemplate>
                        <ContentTemplate>
                            <div class="project-filter">
                                <table class="WholeWidth Height80Px" cellpadding="5">
                                    <tr class="tb-header">
                                        <td colspan="3" class="ProjectSummaryProjectTypeTd">Project Types Included
                                        </td>
                                        <td class="Width20Px" rowspan="4"></td>
                                        <td class="ProjectSummaryGrandTotalTd">Data Points
                                        </td>
                                        <td class="Width20Px" rowspan="4"></td>
                                        <td class="ProjectSummaryGrandTotalTd">View
                                        </td>
                                        <td class="Width20Px tdActual" rowspan="4"></td>
                                        <td class="ProjectSummaryGrandTotalTd tdActual" style="width: 150px !important">Actuals
                                        </td>
                                        <td class="Width20Px" rowspan="4"></td>
                                        <td id="tdFeeTypeLbl" runat="server" class="ProjectSummaryGrandTotalTd" visible="false" style="width: 150px !important">Fee Type</td>
                                        <td class="Width20Px" rowspan="4"></td>
                                        <td class="ProjectSummaryGrandTotalTd" style="width: 150px !important">Suppress Zero Balance Projects
                                        </td>
                                        <td class="Width20Px" rowspan="4"></td>
                                        <td id="tdLevelLbl" runat="server" class="ProjectSummaryGrandTotalTd" visible="false" width="150px">Level
                                        </td>
                                        <td rowspan="4">
                                            <table class="textRight WholeWidth">
                                                <tr>
                                                    <td></td>
                                                </tr>
                                                <tr>
                                                    <td class="PaddingTop10">
                                                        <asp:Button ID="btnResetCalculations" runat="server" Text="Reset" CausesValidation="False"
                                                            OnClientClick="resetCalculationsTab(); return false;" EnableViewState="False"
                                                            CssClass="Width100Px" />
                                                    </td>
                                                </tr>
                                            </table>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td class="ProjectSummaryCheckboxTd">
                                            <asp:CheckBox ID="chbActive" runat="server" Text="Active" Checked="True" EnableViewState="False" />
                                        </td>
                                        <td class="ProjectSummaryCheckboxTd">
                                            <asp:CheckBox ID="chbInternal" runat="server" Text="Internal" Checked="false" EnableViewState="False" />
                                        </td>
                                        <td class="ProjectSummaryCheckboxTd">
                                            <asp:CheckBox ID="chbProposed" runat="server" Text="Proposed" Checked="false" EnableViewState="False" />
                                        </td>
                                        <td rowspan="2" class="">
                                            <div id="divDataPoints" runat="server">
                                                <pmc:ScrollingDropDown ID="ddldataPoints" runat="server" SetDirty="false" CssClass="ProjectSummaryScrollingDropDown TextAlignLeftImp"
                                                    onclick="scrollingDropdown_onclick('ddldataPoints','Data point')" DropDownListType="DataPoint" />
                                                <ext:ScrollableDropdownExtender ID="ScrollableDropdownExtender1" runat="server" TargetControlID="ddldataPoints"
                                                    UseAdvanceFeature="true" EditImageUrl="~/Images/Dropdown_Arrow.png" Width="220px">
                                                </ext:ScrollableDropdownExtender>
                                            </div>
                                        </td>
                                        <td rowspan="2" class="">
                                            <asp:DropDownList ID="ddlCalculationsType" runat="server" AutoPostBack="false" CssClass="height20PImp Width170Px" Onchange="SetActualDropDown();">
                                                <asp:ListItem Text="Budget" Value="3"></asp:ListItem>
                                                <asp:ListItem Text="Projected" Value="1"></asp:ListItem>
                                                <asp:ListItem Text="Actual" Value="2"></asp:ListItem>
                                                <asp:ListItem Text="ETC" Value="4" Selected="True"></asp:ListItem>
                                            </asp:DropDownList>
                                        </td>
                                        <td rowspan="2" class="tdActual">
                                            <asp:DropDownList ID="ddlActualPeriod" runat="server" AutoPostBack="false" Width="150px">
                                                <asp:ListItem Value="1">Today</asp:ListItem>
                                                <asp:ListItem Value="15">Last Pay Period</asp:ListItem>
                                                <asp:ListItem Value="30" Selected="True">Prior Month End</asp:ListItem>
                                                <asp:ListItem Value="0">All</asp:ListItem>
                                            </asp:DropDownList>
                                        </td>
                                        <td id="tdFeeType" runat="server" rowspan="2" visible="false">
                                            <div id="divFeeType" runat="server">
                                                <pmc:ScrollingDropDown ID="ddlFeeType" runat="server" SetDirty="false" CssClass="ProjectSummaryScrollingDropDown TextAlignLeftImp"
                                                    onclick="scrollingDropdown_onclick('ddlFeeType','Fee Type')" DropDownListType="FeeType" />
                                                <ext:ScrollableDropdownExtender ID="ScrollableDropdownExtender2" runat="server" TargetControlID="ddlFeeType"
                                                    UseAdvanceFeature="true" EditImageUrl="~/Images/Dropdown_Arrow.png" Width="150px">
                                                </ext:ScrollableDropdownExtender>
                                            </div>
                                        </td>
                                        <td rowspan="2" class="">
                                            <asp:DropDownList ID="ddlSupressZeroBalance" runat="server" AutoPostBack="false" CssClass="height20PImp" Width="150px">
                                                <asp:ListItem Text="Yes" Value="1" Selected="True"></asp:ListItem>
                                                <asp:ListItem Text="No" Value="2"></asp:ListItem>
                                            </asp:DropDownList>
                                        </td>
                                        <td id="tdLevelValue" runat="server" rowspan="2" visible="false">
                                            <asp:DropDownList ID="ddlLevel" runat="server" AutoPostBack="false" CssClass="height20PImp" Width="150px">
                                                <asp:ListItem Text="Summary" Value="1" Selected="True"></asp:ListItem>
                                                <asp:ListItem Text="Detail" Value="2"></asp:ListItem>
                                            </asp:DropDownList>
                                        </td>
                                    </tr>
                                    <tr class="PadLeft10Td">
                                        <td>
                                            <asp:CheckBox ID="chbProjected" runat="server" Text="Projected" Checked="True" EnableViewState="False" />
                                        </td>
                                        <td>
                                            <asp:CheckBox ID="chbInactive" runat="server" Text="Inactive" EnableViewState="False" />
                                        </td>
                                        <td>
                                            <asp:CheckBox ID="chbAtRisk" runat="server" Text="At Risk" EnableViewState="False" Checked="true" />
                                        </td>
                                    </tr>
                                    <tr class="PadLeft10Td">
                                        <td>
                                            <asp:CheckBox ID="chbCompleted" runat="server" Text="Completed" Checked="True" EnableViewState="False" />
                                        </td>
                                        <td>
                                            <asp:CheckBox ID="chbExperimental" runat="server" Text="Experimental" EnableViewState="False" />
                                        </td>
                                        <td></td>
                                    </tr>
                                </table>
                            </div>
                        </ContentTemplate>
                    </AjaxControlToolkit:TabPanel>
                </AjaxControlToolkit:TabContainer>
            </asp:Panel>

            <AjaxControlToolkit:ModalPopupExtender ID="mpeExportPopUp" runat="server"
                TargetControlID="btnExportToExcel" BehaviorID="mpeExportPopUp"
                BackgroundCssClass="modalBackground" PopupControlID="pnlExportPopUp" CancelControlID="btnCancelExport"
                DropShadow="false" />
            <asp:Panel ID="pnlExportPopUp" runat="server" CssClass="PanelPerson ConfirmBoxClassError"
                Style="display: none">
                <table class="WholeWidth">
                    <tr>
                        <th align="center" class="TextAlignCenter BackGroundColorGray vBottom" colspan="2">
                            <b class="BtnClose">Export</b>
                        </th>
                    </tr>
                    <tr>
                        <td class="Padding10Px" colspan="2">
                            <table>
                                <tr>
                                    <td>
                                        <asp:DropDownList ID="ddlExportOptions" runat="server">
                                            <asp:ListItem Text="Screen information only" Value="1" Selected="True"></asp:ListItem>
                                            <asp:ListItem Text="All data fields - revenue only" Value="2"></asp:ListItem>
                                            <asp:ListItem Text="All data fields - margin only" Value="3"></asp:ListItem>
                                            <asp:ListItem Text="All data fields - revenue and margin" Value="4"></asp:ListItem>
                                        </asp:DropDownList>
                                        <br />
                                    </td>
                                </tr>
                                <tr class="bgColor_F5FAFF">
                                    <td class="textRight Padding3PX">
                                        <asp:Button ID="btnExport" Text="Export" ToolTip="Export" runat="server" OnClick="btnExportToExcel_Click" />
                                        <asp:Button ID="btnCancelExport" Text="Close" ToolTip="Close" runat="server" />
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                </table>
            </asp:Panel>

            <asp:ValidationSummary ID="valsPerformance" runat="server" ValidationGroup="Filter"
                CssClass="searchError WholeWidth" />


        </div>
    </ContentTemplate>
    <Triggers>
        <asp:PostBackTrigger ControlID="btnExport" />
    </Triggers>
</asp:UpdatePanel>
<pmcg:StyledUpdatePanel ID="StyledUpdatePanel" runat="server" CssClass="container minheight250Px"
    UpdateMode="Conditional">
    <ContentTemplate>
        <asp:Panel runat="server" ID="horisontalScrollDiv" CssClass="xScrollAuto minheight250Px">
            <div id="test">
                <asp:ListView ID="lvProjects" runat="server" DataKeyNames="Id" OnItemDataBound="lvProjects_ItemDataBound"
                    OnSorted="lvProjects_Sorted" OnDataBound="lvProjects_OnDataBound" OnSorting="lvProjects_Sorting"
                    OnPagePropertiesChanging="lvProjects_PagePropertiesChanging">
                    <LayoutTemplate>
                        <table id="lvProjects_table" runat="server" class="CompPerfTable WholeWidth tablesorter">
                            <tr runat="server" id="lvHeader" class="CompPerfHeader">
                                <td class="CompPerfProjectState">
                                    <div class="ie-bg">
                                    </div>
                                </td>
                                <td class="CompPerfProjectNumber">
                                    <div class="ie-bg">
                                        <asp:LinkButton ID="btnSortProject" CommandArgument="1" CommandName="Sort" runat="server"
                                            CssClass="arrow">Project #</asp:LinkButton>
                                    </div>
                                </td>
                                <td class="CompPerfClient">
                                    <div class="ie-bg">
                                        <asp:LinkButton ID="btnSortClient" CommandArgument="2" CommandName="Sort" runat="server"
                                            CssClass="arrow">Account</asp:LinkButton>
                                    </div>
                                </td>
                                <%--------%>
                                <td class="MinWidth hideCol">
                                    <div class="ie-bg">
                                        <asp:LinkButton ID="LinkButton1" CommandArgument="3" CommandName="Sort" runat="server"
                                            CssClass="arrow">House Account</asp:LinkButton>
                                    </div>
                                </td>
                                <td class="MinWidth hideCol">
                                    <div class="ie-bg">
                                        <asp:LinkButton ID="LinkButton2" CommandArgument="4" CommandName="Sort" runat="server"
                                            CssClass="arrow">Business Group</asp:LinkButton>
                                    </div>
                                </td>
                                <td class="MinWidth hideCol">
                                    <div class="ie-bg">
                                        <asp:LinkButton ID="LinkButton3" CommandArgument="5" CommandName="Sort" runat="server"
                                            CssClass="arrow">Business Unit</asp:LinkButton>
                                    </div>
                                </td>
                                <td class="MinWidth hideCol">
                                    <div class="ie-bg">
                                        <asp:LinkButton ID="LinkButton4" CommandArgument="6" CommandName="Sort" runat="server"
                                            CssClass="arrow">Buyer</asp:LinkButton>
                                    </div>
                                </td>
                                <%--------%>
                                <td class="CompPerfProject">
                                    <div class="ie-bg">
                                        <asp:LinkButton ID="btnSortProjectName" CommandArgument="7" CommandName="Sort" runat="server"
                                            CssClass="arrow">Project</asp:LinkButton>
                                    </div>
                                </td>
                                <%--------%>
                                <td class="MinWidth hideCol" id="thNewOrExtension" runat="server">
                                    <div class="ie-bg alignCenter">
                                        <asp:LinkButton ID="btnNewOrExtension" CommandArgument="8" CommandName="Sort" runat="server"
                                            CssClass="arrow">New/Extension</asp:LinkButton>
                                    </div>
                                </td>

                                <td class="MinWidth hideCol" id="thStatus" runat="server">
                                    <div class="ie-bg alignCenter">
                                        <asp:LinkButton ID="btnStatus" CommandArgument="9" CommandName="Sort" runat="server"
                                            CssClass="arrow">Status</asp:LinkButton>
                                    </div>
                                </td>

                                <td class="CompPerfPeriod">
                                    <div class="ie-bg alignCenter">
                                        <asp:LinkButton ID="btnSortStartDate" CommandArgument="10" CommandName="Sort" runat="server"
                                            CssClass="arrow">Start Date</asp:LinkButton>
                                    </div>
                                </td>
                                <td class="CompPerfPeriod">
                                    <div class="ie-bg  alignCenter">
                                        <asp:LinkButton ID="btnSortEndDate" CommandArgument="11" CommandName="Sort" runat="server"
                                            CssClass="arrow">End Date</asp:LinkButton>
                                    </div>
                                </td>

                                <td class="MinWidth hideCol">
                                    <div class="ie-bg  alignCenter">
                                        <asp:LinkButton ID="btnDivision" CommandArgument="12" CommandName="Sort" runat="server"
                                            CssClass="arrow">Division</asp:LinkButton>
                                    </div>
                                </td>
                                <td class="MinWidth hideCol">
                                    <div class="ie-bg  alignCenter">
                                        <asp:LinkButton ID="btnPractice" CommandArgument="13" CommandName="Sort" runat="server"
                                            CssClass="arrow">Practice Area</asp:LinkButton>
                                    </div>
                                </td>
                                <td class="MinWidth hideCol">
                                    <div class="ie-bg  alignCenter">
                                        <asp:LinkButton ID="btnChannel" CommandArgument="14" CommandName="Sort" runat="server"
                                            CssClass="arrow">Channel</asp:LinkButton>
                                    </div>
                                </td>
                                <td class="MinWidth hideCol">
                                    <div class="ie-bg  alignCenter">
                                        <asp:LinkButton ID="btnSubChannel" CommandArgument="15" CommandName="Sort" runat="server"
                                            CssClass="arrow">Channel-Sub</asp:LinkButton>
                                    </div>
                                </td>
                                <td class="MinWidth hideCol">
                                    <div class="ie-bg  alignCenter">
                                        <asp:LinkButton ID="btnOffering" CommandArgument="16" CommandName="Sort" runat="server"
                                            CssClass="arrow">Offering</asp:LinkButton>
                                    </div>
                                </td>
                                <td class="MinWidth hideCol">
                                    <div class="ie-bg  alignCenter">
                                        <asp:LinkButton ID="btnRevenueType" CommandArgument="17" CommandName="Sort" runat="server"
                                            CssClass="arrow">Revenue Type</asp:LinkButton>
                                    </div>
                                </td>

                                <td class="MinWidth hideCol">
                                    <div class="ie-bg  alignCenter">
                                        <%--<asp:LinkButton ID="LinkButton5" CommandArgument="18" CommandName="Sort" runat="server"
                                            CssClass="arrow">--%>
                                            Capabilities

                                        <%--</asp:LinkButton>--%>
                                    </div>
                                </td>

                                <td class="MinWidth hideCol" id="thSalesPerson" runat="server">
                                    <div class="ie-bg alignCenter">
                                        <asp:LinkButton ID="btnSalesPerson" CommandArgument="19" CommandName="Sort" runat="server"
                                            CssClass="arrow">Sales Person</asp:LinkButton>
                                    </div>
                                </td>

                                <td class="MinWidth hideCol" id="Td1" runat="server">
                                    <div class="ie-bg alignCenter">
                                        <asp:LinkButton ID="LinkButton6" CommandArgument="20" CommandName="Sort" runat="server"
                                            CssClass="arrow">Project Manager</asp:LinkButton>
                                    </div>
                                </td>
                                <td class="MinWidth hideCol" id="Td2" runat="server">
                                    <div class="ie-bg alignCenter">
                                        <asp:LinkButton ID="LinkButton7" CommandArgument="21" CommandName="Sort" runat="server"
                                            CssClass="arrow">Engagement Manager</asp:LinkButton>
                                    </div>
                                </td>
                                <td class="MinWidth hideCol" id="Td3" runat="server">
                                    <div class="ie-bg alignCenter">
                                        <asp:LinkButton ID="LinkButton8" CommandArgument="22" CommandName="Sort" runat="server"
                                            CssClass="arrow">Executive in Charge</asp:LinkButton>
                                    </div>
                                </td>
                                <td class="MinWidth hideCol" id="Td4" runat="server">
                                    <div class="ie-bg alignCenter">
                                        <asp:LinkButton ID="LinkButton9" CommandArgument="23" CommandName="Sort" runat="server"
                                            CssClass="arrow">Pricing List</asp:LinkButton>
                                    </div>
                                </td>
                                <td class="MinWidth hideCol" id="Td5" runat="server">
                                    <div class="ie-bg alignCenter">
                                        <asp:LinkButton ID="LinkButton10" CommandArgument="24" CommandName="Sort" runat="server"
                                            CssClass="arrow">PO Number</asp:LinkButton>
                                    </div>
                                </td>
                                <td class="MinWidth hideCol" id="Td6" runat="server">
                                    <div class="ie-bg alignCenter">
                                        <asp:LinkButton ID="LinkButton11" CommandArgument="25" CommandName="Sort" runat="server"
                                            CssClass="arrow">Client Time Entry Required</asp:LinkButton>
                                    </div>
                                </td>
                                <td class="MinWidth hideCol" id="Td7" runat="server">
                                    <div class="ie-bg alignCenter">
                                        <asp:LinkButton ID="LinkButton12" CommandArgument="26" CommandName="Sort" runat="server"
                                            CssClass="arrow">Previous Project Number</asp:LinkButton>
                                    </div>
                                </td>
                                <td class="MinWidth hideCol" id="Td8" runat="server">
                                    <div class="ie-bg alignCenter">
                                        <asp:LinkButton ID="LinkButton13" CommandArgument="27" CommandName="Sort" runat="server"
                                            CssClass="arrow">Outsource Id Indicator</asp:LinkButton>
                                    </div>
                                </td>
                                <td class="CompPerfTotalSummary">
                                    <div class="ie-bg alignCenter">
                                        Total
                                    </div>
                                </td>
                            </tr>
                            <tr runat="server" id="lvSummary" class="summary">
                                <td id="tdSummary" runat="server">
                                    <div class="cell-pad">
                                        Financial Summary
                                    </div>
                                </td>
                            </tr>
                            <tbody>
                                <tr runat="server" id="itemPlaceholder" />
                            </tbody>
                        </table>
                    </LayoutTemplate>
                    <ItemTemplate>
                        <tr runat="server" id="boundingRow" class="bgcolorwhite">
                            <td class="CompPerfProjectState"></td>
                            <td class="CompPerfProjectNumber">
                                <asp:Label ID="lblProjectNumber" runat="server" />
                            </td>
                            <td class="CompPerfClient">
                                <asp:HyperLink ID="btnClientName" runat="server" />
                            </td>
                            <td class="MinWidth TextAlignCenter hideCol" id="tdHouseAccount" runat="server">
                                <asp:Label ID="lblIsHouseAccount" runat="server" />
                            </td>
                            <td class="MinWidth TextAlignCenter hideCol" id="tdBusinessGroup" runat="server">
                                <asp:Label ID="lblBusinessGroup" runat="server" />
                            </td>
                            <td class="MinWidth TextAlignCenter hideCol" id="tdBusinessUnit" runat="server">
                                <asp:Label ID="lblBusinessUnit" runat="server" />
                            </td>
                            <td class="MinWidth TextAlignCenter hideCol" id="tdBuyer" runat="server">
                                <asp:Label ID="lblBuyer" runat="server" />
                            </td>

                            <td id="tdProjectName" runat="server" class="CompPerfProject padLeft10Imp"></td>

                            <td class="MinWidth TextAlignCenter hideCol" id="tdNewOrExt" runat="server">
                                <asp:Label ID="lblNewOrExt" runat="server" />
                            </td>

                            <td class="MinWidth TextAlignCenter hideCol" id="tdStatus" runat="server">
                                <asp:Label ID="lblStatus" runat="server" />
                            </td>
                            <td class="CompPerfPeriod TextAlignCenter">
                                <asp:Label ID="lblStartDate" runat="server" />
                            </td>
                            <td class="CompPerfPeriod TextAlignCenter">
                                <asp:Label ID="lblEndDate" runat="server" />
                            </td>

                            <td class="MinWidth padLeft10Imp TextAlignCenter hideCol">
                                <asp:Label ID="lblDivision" runat="server" />
                            </td>
                            <td class="MinWidth padLeft10Imp TextAlignCenter hideCol">
                                <asp:Label ID="lblPractice" runat="server" />
                            </td>
                            <td class="MinWidth padLeft10Imp TextAlignCenter hideCol">
                                <asp:Label ID="lblChannel" runat="server" />
                            </td>
                            <td class="MinWidth padLeft10Imp TextAlignCenter hideCol">
                                <asp:Label ID="lblSubChannel" runat="server" />
                            </td>
                            <td class="MinWidth padLeft10Imp TextAlignCenter hideCol">
                                <asp:Label ID="lblOffering" runat="server" />
                            </td>
                            <td class="MinWidth padLeft10Imp TextAlignCenter hideCol">
                                <asp:Label ID="lblRevenue" runat="server" />
                            </td>
                            <td class="MinWidth padLeft10Imp TextAlignCenter hideCol Width20PerImp">
                                <asp:Label ID="lblCapabilities" runat="server" />
                            </td>

                            <td class="MinWidth hideCol TextAlignCenter" id="tdSalesPerson" runat="server">
                                <asp:Label ID="lblSalesPerson" runat="server" />
                            </td>

                            <td class="MinWidth hideCol TextAlignCenter" id="tdProjectManager" runat="server">
                                <asp:Label ID="lblProjectManager" runat="server" />
                            </td>
                            <td class="MinWidth hideCol TextAlignCenter" id="tdEngagementManager" runat="server">
                                <asp:Label ID="lblEngagementManager" runat="server" />
                            </td>
                            <td class="MinWidth hideCol TextAlignCenter" id="tdExecutiveIncharge" runat="server">
                                <asp:Label ID="lblExecutiveInCharge" runat="server" />
                            </td>

                            <td class="MinWidth hideCol TextAlignCenter" id="tdPricingList" runat="server">
                                <asp:Label ID="lblPricingList" runat="server" />
                            </td>
                            <td class="MinWidth hideCol TextAlignCenter" id="tdPONumber" runat="server">
                                <asp:Label ID="lblPONumber" runat="server" />
                            </td>
                            <td class="MinWidth hideCol TextAlignCenter" id="td9" runat="server">
                                <asp:Label ID="lblClientTimeEntry" runat="server" />
                            </td>
                            <td class="MinWidth hideCol TextAlignCenter" id="td10" runat="server">
                                <asp:Label ID="lblPreviousProjectNumber" runat="server" />
                            </td>
                            <td class="MinWidth hideCol TextAlignCenter" id="td11" runat="server">
                                <asp:Label ID="lblOutSourceId" runat="server" />
                            </td>

                        </tr>
                    </ItemTemplate>
                    <AlternatingItemTemplate>
                        <tr runat="server" id="boundingRow" class="rowEven">
                            <td class="CompPerfProjectState"></td>
                            <td class="CompPerfProjectNumber">
                                <asp:Label ID="lblProjectNumber" runat="server" />
                            </td>
                            <td class="CompPerfClient">
                                <asp:HyperLink ID="btnClientName" runat="server" />
                            </td>
                            <td class="MinWidth TextAlignCenter hideCol" id="tdHouseAccount" runat="server">
                                <asp:Label ID="lblIsHouseAccount" runat="server" />
                            </td>
                            <td class="MinWidth TextAlignCenter hideCol" id="tdBusinessGroup" runat="server">
                                <asp:Label ID="lblBusinessGroup" runat="server" />
                            </td>
                            <td class="MinWidth TextAlignCenter hideCol" id="tdBusinessUnit" runat="server">
                                <asp:Label ID="lblBusinessUnit" runat="server" />
                            </td>
                            <td class="MinWidth TextAlignCenter hideCol" id="tdBuyer" runat="server">
                                <asp:Label ID="lblBuyer" runat="server" />
                            </td>

                            <td id="tdProjectName" runat="server" class="CompPerfProject padLeft10Imp"></td>

                            <td class="MinWidth TextAlignCenter hideCol" id="tdNewOrExt" runat="server">
                                <asp:Label ID="lblNewOrExt" runat="server" />
                            </td>

                            <td class="MinWidth TextAlignCenter hideCol" id="tdStatus" runat="server">
                                <asp:Label ID="lblStatus" runat="server" />
                            </td>
                            <td class="CompPerfPeriod TextAlignCenter">
                                <asp:Label ID="lblStartDate" runat="server" />
                            </td>
                            <td class="CompPerfPeriod TextAlignCenter">
                                <asp:Label ID="lblEndDate" runat="server" />
                            </td>

                            <td class="MinWidth padLeft10Imp TextAlignCenter hideCol">
                                <asp:Label ID="lblDivision" runat="server" />
                            </td>
                            <td class="MinWidth padLeft10Imp TextAlignCenter hideCol">
                                <asp:Label ID="lblPractice" runat="server" />
                            </td>
                            <td class="MinWidth padLeft10Imp TextAlignCenter hideCol">
                                <asp:Label ID="lblChannel" runat="server" />
                            </td>
                            <td class="MinWidth padLeft10Imp TextAlignCenter hideCol">
                                <asp:Label ID="lblSubChannel" runat="server" />
                            </td>
                            <td class="MinWidth padLeft10Imp TextAlignCenter hideCol">
                                <asp:Label ID="lblOffering" runat="server" />
                            </td>
                            <td class="MinWidth padLeft10Imp TextAlignCenter hideCol">
                                <asp:Label ID="lblRevenue" runat="server" />
                            </td>
                            <td class="MinWidth padLeft10Imp TextAlignCenter hideCol Width20PerImp">
                                <asp:Label ID="lblCapabilities" runat="server" />
                            </td>

                            <td class="MinWidth hideCol TextAlignCenter" id="tdSalesPerson" runat="server">
                                <asp:Label ID="lblSalesPerson" runat="server" />
                            </td>

                            <td class="MinWidth hideCol TextAlignCenter" id="tdProjectManager" runat="server">
                                <asp:Label ID="lblProjectManager" runat="server" />
                            </td>
                            <td class="MinWidth hideCol TextAlignCenter" id="tdEngagementManager" runat="server">
                                <asp:Label ID="lblEngagementManager" runat="server" />
                            </td>
                            <td class="MinWidth hideCol TextAlignCenter" id="tdExecutiveIncharge" runat="server">
                                <asp:Label ID="lblExecutiveInCharge" runat="server" />
                            </td>

                            <td class="MinWidth hideCol TextAlignCenter" id="tdPricingList" runat="server">
                                <asp:Label ID="lblPricingList" runat="server" />
                            </td>
                            <td class="MinWidth hideCol TextAlignCenter" id="tdPONumber" runat="server">
                                <asp:Label ID="lblPONumber" runat="server" />
                            </td>
                            <td class="MinWidth hideCol TextAlignCenter" id="td9" runat="server">
                                <asp:Label ID="lblClientTimeEntry" runat="server" />
                            </td>
                            <td class="MinWidth hideCol TextAlignCenter" id="td10" runat="server">
                                <asp:Label ID="lblPreviousProjectNumber" runat="server" />
                            </td>
                            <td class="MinWidth hideCol TextAlignCenter" id="td11" runat="server">
                                <asp:Label ID="lblOutSourceId" runat="server" />
                            </td>

                        </tr>
                    </AlternatingItemTemplate>
                    <EmptyDataTemplate>
                        <tr runat="server" id="EmptyDataRow">
                            <td>There is nothing to be displayed here.
                            </td>
                        </tr>
                    </EmptyDataTemplate>
                </asp:ListView>
            </div>
            <asp:DataPager ID="dpProjects" runat="server" PagedControlID="lvProjects">
                <Fields>
                    <asp:NextPreviousPagerField ButtonType="Link" ShowFirstPageButton="false" ShowPreviousPageButton="true"
                        ShowNextPageButton="false" />
                    <asp:NumericPagerField ButtonCount="10" NextPageText="..." PreviousPageText="..." />
                    <asp:NextPreviousPagerField ButtonType="Link" ShowNextPageButton="true" ShowLastPageButton="false" ShowPreviousPageButton="false" />
                </Fields>
            </asp:DataPager>
            <uc3:LoadingProgress ID="LoadingProgress1" runat="server" DisplayText="Refreshing Data..." />
        </asp:Panel>
        <div class="padding5Right10">
            <table class="WholeWidth">
                <tr>
                    <td class="padLeft40Percent">
                        <div class="ProjectSummaryPageCountTd">
                            <asp:Label ID="lblPageCount" runat="server"></asp:Label>
                        </div>
                    </td>
                    <td align="right"></td>
                </tr>
            </table>
        </div>
        <asp:Panel ID="pnlCustomDates" runat="server" CssClass="ConfirmBoxClass CustomDatesPopUp"
            Style="display: none;">
            <table class="WholeWidth">
                <tr>
                    <td align="center">
                        <uc:DateInterval ID="diRange" runat="server" IsFromDateRequired="true" IsToDateRequired="true"
                            FromToDateFieldCssClass="Width70Px" />
                    </td>
                </tr>
                <tr>
                    <td class="custBtns">
                        <asp:Button ID="btnCustDatesOK" runat="server" OnClientClick="CheckIfDatesValid();"
                            Text="OK" OnClick="btnUpdateView_Click" />
                        <asp:Button ID="btnCustDatesClose" runat="server" Style="display: none;" CausesValidation="true" />
                        &nbsp; &nbsp;
                        <asp:Button ID="btnCustDatesCancel" runat="server" OnClientClick="ReAssignStartDateEndDates();"
                            Text="Cancel" />
                    </td>
                </tr>
                <tr>
                    <td class="textCenter">
                        <asp:ValidationSummary ID="valSum" runat="server" />
                    </td>
                </tr>
            </table>
        </asp:Panel>
        <asp:Panel ID="pnlProjectToolTipHolder" Style="display: none;" runat="server" CssClass="ToolTip WordWrap ProjectsToolTip">
            <table>
                <tr class="top">
                    <td class="lt">
                        <div class="tail">
                        </div>
                    </td>
                    <td class="tbor"></td>
                    <td class="rt"></td>
                </tr>
                <tr class="middle">
                    <td class="lbor"></td>
                    <td class="content WordWrap">
                        <asp:Label ID="lblProjectTooltip" CssClass="WordWrap" runat="server"></asp:Label>
                    </td>
                    <td class="rbor"></td>
                </tr>
                <tr class="bottom">
                    <td class="lb"></td>
                    <td class="bbor"></td>
                    <td class="rb"></td>
                </tr>
            </table>
        </asp:Panel>
    </ContentTemplate>
</pmcg:StyledUpdatePanel>

