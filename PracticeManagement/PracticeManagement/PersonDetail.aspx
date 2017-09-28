<%@ Page Language="C#" MasterPageFile="~/PracticeManagementMain.Master" AutoEventWireup="True"
    CodeBehind="PersonDetail.aspx.cs" Inherits="PraticeManagement.PersonDetail" Title="Person Details | Practice Management" %>

<%@ Register Src="~/Controls/Generic/Notes.ascx" TagName="Notes" TagPrefix="uc" %>
<%@ Register Src="Controls/DatePicker.ascx" TagName="DatePicker" TagPrefix="uc2" %>
<%@ Register Src="~/Controls/RestrictionPanel.ascx" TagPrefix="uc" TagName="RestrictionPanel" %>
<%@ Register Src="~/Controls/ActivityLogControl.ascx" TagPrefix="uc" TagName="ActivityLog" %>
<%@ Register Src="~/Controls/Persons/PersonProjects.ascx" TagPrefix="uc" TagName="PersonProjects" %>
<%@ Register Src="~/Controls/Configuration/DefaultUser.ascx" TagPrefix="uc" TagName="DefaultManager" %>
<%@ Register Src="~/Controls/MessageLabel.ascx" TagName="MessageLabel" TagPrefix="uc" %>
<%@ Register Src="Controls/PersonnelCompensation.ascx" TagName="PersonnelCompensation"
    TagPrefix="uc" %>
<%@ Register Src="~/Controls/Generic/OpportunityList.ascx" TagName="OpportunityList"
    TagPrefix="uc" %>
<%@ Register Src="~/Controls/Persons/MSBadge.ascx" TagName="MSBadge" TagPrefix="uc" %>
<%@ Register TagPrefix="ext" Assembly="PraticeManagement" Namespace="PraticeManagement.Controls.Generic.ElementDisabler" %>
<%@ Register TagPrefix="asp" Namespace="PraticeManagement.Controls.Generic.Buttons"
    Assembly="PraticeManagement" %>
<%@ Register TagPrefix="uc" TagName="LoadingProgress" Src="~/Controls/Generic/LoadingProgress.ascx" %>
<%@ Register TagPrefix="uc" Namespace="PraticeManagement.Controls" %>
<asp:Content ID="cntTitle" ContentPlaceHolderID="title" runat="server">
    <title>Person Details | Practice Management</title>
</asp:Content>
<asp:Content ID="cntHeader" ContentPlaceHolderID="header" runat="server">
    Person Details
</asp:Content>
<asp:Content ID="cntBody" ContentPlaceHolderID="body" runat="server">
    <script src="Scripts/jquery.tablesorter_2.min.js" type="text/javascript"></script>
    <script src="Scripts/date.yui.js" type="text/javascript"></script>
    <script type="text/javascript" language="javascript">
        var currentSort = [[1, 0]];
        $(document).ready(function () {
            showTarget();
            $("#tblPersonProjects").tablesorter({
                headers: {
                    0: {
                        sorter: false
                    }
                },
                sortList: currentSort,
                sortForce: [[1, 0]]
            }).bind("sortEnd", function (sorter) {
                currentSort = sorter.target.config.sortList;
                var spanName = $("#tblPersonProjects #name");
                if (currentSort != '1,0' && currentSort != '1,1') {
                    spanName[0].setAttribute('class', 'backGroundNone');
                }
                else {
                    spanName[0].setAttribute('class', '');
                }
            });
        });

        /*
        This script is needed to initialize select all/none behavior for checkbox lists
        This is done because tab content is loaded asynchronously and window.load event is not fired
        when the user goes to the Permissions tab. So this method is called each time post back
        goes to the server in any way.
        */

        Sys.WebForms.PageRequestManager.getInstance().add_pageLoaded(pageLoadedHandler);
        function pageLoadedHandler(sender, args) {
            addListenersToAllCheckBoxes('ctl00_body_rpPermissions_msddClients');
            addListenersToParent('ctl00_body_rpPermissions_msddGroups', 'ctl00_body_rpPermissions_msddClients');
            addListenersToAllCheckBoxes('ctl00_body_rpPermissions_msddGroups');
            addListenersToAllCheckBoxes('ctl00_body_rpPermissions_msddSalespersons');
            addListenersToAllCheckBoxes('ctl00_body_rpPermissions_msddPracticeManagers');
            addListenersToAllCheckBoxes('ctl00_body_rpPermissions_msddPractices');
        }

        function SetTooltipText(descriptionText, hlinkObj) {
      <%--      var hlinkObjct = $('#' + hlinkObj.id);
            var displayPanel = $('#<%= personOpportunities.ClientID %>_oppNameToolTipHolder');
            iptop = hlinkObjct.offset().top - hlinkObjct[0].offsetHeight;
            ipleft = hlinkObjct.offset().left + hlinkObjct[0].offsetWidth + 10;
            iptop = iptop;
            ipleft = ipleft;
            setPosition(displayPanel, iptop, ipleft);
            displayPanel.show();
            setPosition(displayPanel, iptop, ipleft);
            displayPanel.show();

            var lbloppNameTooltipContent = document.getElementById('<%= personOpportunities.ClientID %>_lbloppNameTooltipContent');
            lbloppNameTooltipContent.innerHTML = descriptionText.toString();--%>
        }

        function txtEmployeeNumber_ClientClick(source) {
            if (!source.readOnly && (source.getAttribute('accept') == 0) && !confirm('This value should not normally be changed once set. Please be cautious about changing this value. Press OK to continue or Cancel to return without changing it.')) source.blur();
            else { if (source.getAttribute('accept') == 0) source.setAttribute('accept', 1); }
        }

        function txtEmployeeNumber_OnBlur(source) {
            source.setAttribute('accept', 0);
        }

        function HidePanel() {

          <%--  var displayPanel = $('#<%= personOpportunities.ClientID %>_oppNameToolTipHolder');
            displayPanel.hide();--%>
        }

        function printform(popup) {
            var printContent;
            if (popup == 1) {
                printContent = document.getElementById('<%= dvTerminationDateErrors.ClientID %>');
            }
            else if (popup == 2) {
                printContent = document.getElementById('<%= dvCancelTerminationDateErrors.ClientID %>');
            }
            else {
                printContent = document.getElementById('<%= dvExtendingHireDate.ClientID %>');
            }
        var windowUrl = 'about:blank';
        var uniqueName = new Date();
        var windowName = 'Print' + uniqueName.getTime();
        var printWindow = window.open(windowUrl, windowName);

        printWindow.document.write(printContent.innerHTML);
        printWindow.document.close();
        printWindow.focus();
        printWindow.print();
        printWindow.close();
        return false;
    }

    function saveReport(popup) {
        var printContent;
        var hdnSaveReportText;

        if (popup == 1) {
            printContent = document.getElementById('<%= dvTerminationDateErrors.ClientID %>');
            hdnSaveReportText = document.getElementById('<%= hdnSaveReportText.ClientID %>');
        }
        else if (popup == 2) {
            printContent = document.getElementById('<%= dvCancelTerminationDateErrors.ClientID %>');
            hdnSaveReportText = document.getElementById('<%= hdnSaveReportTextCancelTermination.ClientID %>');
        }
        else {
            printContent = document.getElementById('<%= dvExtendingHireDate.ClientID %>');
            hdnSaveReportText = document.getElementById('<%= hdnSaveReportHireDateExtend.ClientID %>');
        }
    hdnSaveReportText.value = printContent.innerHTML;
}

function SetTooltipsForallDropDowns() {
    var optionList = document.getElementsByTagName('option');

    for (var i = 0; i < optionList.length; ++i) {

        optionList[i].title = optionList[i].innerHTML;
    }

}
Sys.WebForms.PageRequestManager.getInstance().add_endRequest(endRequestHandle);
function endRequestHandle(sender, Args) {
    ModifyInnerTextToWrapText();
    var activityLog = document.getElementById('<%= activityLog.ClientID%>');
    if (activityLog != null) {
        imgCalender = document.getElementById('<%= activityLog.ClientID %>_imgCalender');
        lblCustomDateRange = document.getElementById('<%= activityLog.ClientID %>_lblCustomDateRange');
        ddlPeriod = document.getElementById('<%=  activityLog.ClientID %>_ddlPeriod');
        if (imgCalender.fireEvent && ddlPeriod.value != '0') {
            imgCalender.style.display = "none";
            lblCustomDateRange.style.display = "none";
        }
    }
    SetTooltipsForallDropDowns();
    $("#tblPersonProjects").tablesorter({
        headers: {
            0: {
                sorter: false
            }
        },
        sortList: currentSort,
        sortForce: [[1, 0]]
    }).bind("sortEnd", function (sorter) {
        currentSort = sorter.target.config.sortList;
        var spanName = $("#tblPersonProjects #name")[0];
        if (currentSort != '1,0' && currentSort != '1,1') {
            spanName[0].setAttribute('class', 'backGroundNone');
        }
        else {
            spanName[0].setAttribute('class', '');
        }
    });
}

function CheckIfDatesValid() {

    txtStartDate = document.getElementById('<%= activityLog.ClientID %>_diRange_tbFrom');
    txtEndDate = document.getElementById('<%= activityLog.ClientID %>_diRange_tbTo');
    var startDate = new Date(txtStartDate.value);
    var endDate = new Date(txtEndDate.value);
    if (txtStartDate.value != '' && txtEndDate.value != ''
    && startDate <= endDate) {
        var btnCustDatesClose = document.getElementById('<%= activityLog.ClientID %>_btnCustDatesClose');
        hdnStartDate = document.getElementById('<%= activityLog.ClientID %>_hdnStartDate');
        hdnEndDate = document.getElementById('<%= activityLog.ClientID %>_hdnEndDate');
        lblCustomDateRange = document.getElementById('<%= activityLog.ClientID %>_lblCustomDateRange');
        var startDate = new Date(txtStartDate.value);
        var endDate = new Date(txtEndDate.value);
        var startDateStr = startDate.format("MM/dd/yyyy");
        var endDateStr = endDate.format("MM/dd/yyyy");
        hdnStartDate.value = startDateStr;
        hdnEndDate.value = endDateStr;
        lblCustomDateRange.innerHTML = '(' + startDateStr + '&nbsp;-&nbsp;' + endDateStr + ')';
        btnCustDatesClose.click();

    }
    return false;
}

function CheckAndShowCustomDatesPoup(ddlPeriod) {
    imgCalender = document.getElementById('<%= activityLog.ClientID %>_imgCalender');
    lblCustomDateRange = document.getElementById('<%= activityLog.ClientID %>_lblCustomDateRange');
    if (ddlPeriod.value == '0') {
        imgCalender.attributes["class"].value = "";
        lblCustomDateRange.attributes["class"].value = "fontBold";
        if (imgCalender.fireEvent) {
            imgCalender.style.display = "";
            lblCustomDateRange.style.display = "";
            imgCalender.click();
        }
        if (document.createEvent) {
            var event = document.createEvent('HTMLEvents');
            event.initEvent('click', true, true);
            imgCalender.dispatchEvent(event);
        }
    }
    else {
        imgCalender.attributes["class"].value = "displayNone";
        lblCustomDateRange.attributes["class"].value = "displayNone";
        if (imgCalender.fireEvent) {
            imgCalender.style.display = "none";
            lblCustomDateRange.style.display = "none";
        }
    }
}
function ReAssignStartDateEndDates() {
    hdnStartDate = document.getElementById('<%= activityLog.ClientID %>_hdnStartDate');
    hdnEndDate = document.getElementById('<%= activityLog.ClientID %>_hdnEndDate');
    txtStartDate = document.getElementById('<%= activityLog.ClientID %>_diRange_tbFrom');
    txtEndDate = document.getElementById('<%= activityLog.ClientID %>_diRange_tbTo');
    hdnStartDateCalExtenderBehaviourId = document.getElementById('<%= activityLog.ClientID %>_hdnStartDateCalExtenderBehaviourId');
    hdnEndDateCalExtenderBehaviourId = document.getElementById('<%= activityLog.ClientID %>_hdnEndDateCalExtenderBehaviourId');

    var endDateCalExtender = $find(hdnEndDateCalExtenderBehaviourId.value);
    var startDateCalExtender = $find(hdnStartDateCalExtenderBehaviourId.value);
    if (startDateCalExtender != null) {
        startDateCalExtender.set_selectedDate(hdnStartDate.value);
    }
    if (endDateCalExtender != null) {
        endDateCalExtender.set_selectedDate(hdnEndDate.value);
    }
    CheckIfDatesValid();
}

function SetWrapText(str) {
    for (var i = 30; i < str.length; i = i + 10) {
        str = str.slice(0, i) + "<wbr />" + str.slice(i, str.length);
    }
    return str;
}

function GetWrappedText(childObj) {
    if (childObj != null) {

        for (var i = 0; i < childObj.children.length; i++) {
            if (childObj.children[i] != null) {
                if (childObj.children[i].children.length == 0) {
                    if (childObj.children[i].innerHTML != null && childObj.children[i].innerHTML != "undefined" && childObj.children[i].innerHTML.length > 70) {
                        childObj.children[i].innerHTML = SetWrapText(childObj.children[i].innerHTML);
                    }
                }
            }

        }
    }
}

function ModifyInnerTextToWrapText() {
    if (navigator.userAgent.indexOf(" Firefox/") > -1) {
        var tbl = $("table[id*='gvActivities']");
        if (tbl != null && tbl.length > 0) {
            var gvActivitiesclientId = tbl[0].id;
            var lastTds = $('#' + gvActivitiesclientId + ' tr td:nth-child(3)');

            for (var i = 0; i < lastTds.length; i++) {
                GetWrappedText(lastTds[i]);
            }
        }
    }
}
    <%--
        The following script is needed to implement dirty checks on Projects tab
        Use Page.ClientScript.GetPostBackClientHyperlink(...) method to generate
        personProjects control postback url
    --%>
        function checkDirty(target, entityId) {
            if (showDialod()) {
                __doPostBack('ctl00$body$personProjects', target + ':' + entityId);
                return false;
            }
            return true;
        }

        function setHintPosition(img, displayPnl) {
            var image = $("#" + img);
            var displayPanel = $("#" + displayPnl);
            iptop = image.offset().top;
            ipleft = image.offset().left;
            iptop = iptop + 10;
            ipleft = ipleft - 10;
            setPosition(displayPanel, iptop, ipleft);
            displayPanel.show();
            setPosition(displayPanel, iptop, ipleft);
            displayPanel.show();
        }

        function setPosition(item, ytop, xleft) {
            item.offset({ top: ytop, left: xleft });
        }

        function ValidateCompensationAndTerminationDate(btnTerminatePersonOk, popupTerminationDate, ddlPopupTerminationReason) {
            var terminationDateExt = $find(popupTerminationDate);
            var ddlterminationReason = document.getElementById(ddlPopupTerminationReason);
            var terminationDate = terminationDateExt.get_selectedDate();
            var terminationDateTextValue = terminationDateExt._textbox.get_Value();
            var compensationEndDate = new Date(btnTerminatePersonOk.getAttribute('CompensationEndDate'));
            var hasNotClosedCompensation = btnTerminatePersonOk.getAttribute('HasNotClosedCompensation');

            if (terminationDate != '' && terminationDate != undefined && terminationDate != null && compensationEndDate != null && (compensationEndDate > terminationDate || (hasNotClosedCompensation == 'true')) && ddlterminationReason.value != '') {
                var message = '';
                if (hasNotClosedCompensation)
                    message = message + 'This person has Termination Date, but still has an open-ended compensation record. Click OK to close their compensation as of their termination date, or click Cancel to not to save changes.';
                else
                    message = message + 'This person has Termination Date, but still has an active compensation record. Click OK to close their compensation as of their termination date, or click Cancel to not to save changes.';

                if (!confirm(message)) {
                    Page_ClientValidate('PersonTerminate');
                    return false;
                }
            }
            return true;
        }

        function EnableTerminateEmployeeOkButton(btnPersonTerminateOkId, terminationDateTextBoxId, ddlTerminationReasonId) {
            var btnPersonTerminateOk = document.getElementById(btnPersonTerminateOkId);
            var terminationDate = document.getElementById(terminationDateTextBoxId);
            var ddlTerminationReason = document.getElementById(ddlTerminationReasonId);

            if (btnPersonTerminateOk != null && ddlTerminationReason != null && terminationDate != null) {
                Disable(btnPersonTerminateOk, (ddlTerminationReason.value == '' || terminationDate.value == ''));
            }
        }

        function Disable(control, disable) {
            if (disable) {
                control.disabled = 'disabled';
            }
            else {
                control.disabled = '';
            }
        }

        function showDivRighttoPresent() {
            var rbn = document.getElementById('<%= rbnRighttoPresent.ClientID %>');
            var div = document.getElementById('<%= divRighttoPresent.ClientID %>');
            var divCont = document.getElementById('<%= divContingent.ClientID %>');
            var divActv = document.getElementById('<%= divActive.ClientID %>');
            var divTerm = document.getElementById('<%= divTerminate.ClientID %>');
            var valSumTerm = document.getElementById('<%= valSummaryChangePersonStatusToTerminate.ClientID %>');
            var valSumActv = document.getElementById('<%= valSummaryChangePersonStatusToActive.ClientID %>');
            var valSumCont = document.getElementById('<%= valSummaryChangePersonStatusToContingent.ClientID %>');
            var valSumRtP = document.getElementById('<%= valSummaryChangeStatusToRightToPresent.ClientID %>');
            var displayNoneClass = "displayNone";

            if (rbn.checked) {
                div.className = "padLeft25 PaddingTop6";
                divActv.className = displayNoneClass;
                divTerm.className = displayNoneClass;
                divCont.className = displayNoneClass;
                if (valSumActv != null) {
                    valSumActv.className = displayNoneClass;
                }
                if (valSumCont != null) {
                    valSumCont.className = displayNoneClass;
                }
                if (valSumTerm != null) {
                    valSumTerm.className = displayNoneClass;
                }
                if (valSumRtP != null) {
                    valSumRtP.className = "";
                }
            }
        }


        function showDivContingent() {
            var rbn = document.getElementById('<%= rbnContingent.ClientID %>');
            var div = document.getElementById('<%= divContingent.ClientID %>');
            var divActv = document.getElementById('<%= divActive.ClientID %>');
            var divTerm = document.getElementById('<%= divTerminate.ClientID %>');
            var divRtP = document.getElementById('<%= divRighttoPresent.ClientID %>');
            var valSumTerm = document.getElementById('<%= valSummaryChangePersonStatusToTerminate.ClientID %>');
            var valSumActv = document.getElementById('<%= valSummaryChangePersonStatusToActive.ClientID %>');
            var valSumCont = document.getElementById('<%= valSummaryChangePersonStatusToContingent.ClientID %>');
            var valSumRtP = document.getElementById('<%= valSummaryChangeStatusToRightToPresent.ClientID %>');
            var displayNoneClass = "displayNone";

            if (rbn.checked) {
                div.className = "padLeft25 PaddingTop6";
                divActv.className = displayNoneClass;
                divTerm.className = displayNoneClass;
                divRtP.className = displayNoneClass;
                if (valSumActv != null) {
                    valSumActv.className = displayNoneClass;
                }
                if (valSumCont != null) {
                    valSumCont.className = "";
                }
                if (valSumTerm != null) {
                    valSumTerm.className = displayNoneClass;
                }
                if (valSumRtP != null) {
                    valSumRtP.className = displayNoneClass;
                }
            }
        }

        function showDivActive() {
            var rbnContgn = document.getElementById('<%= rbnContingent.ClientID %>');
            var rbnTermin = document.getElementById('<%= rbnTerminate.ClientID %>');
            var rbn = document.getElementById('<%= rbnActive.ClientID %>');
            var div = document.getElementById('<%= divActive.ClientID %>');
            var divContgn = document.getElementById('<%= divContingent.ClientID %>');
            var divTerm = document.getElementById('<%= divTerminate.ClientID %>');
            var divRtP = document.getElementById('<%= divRighttoPresent.ClientID %>');
            var valSumActv = document.getElementById('<%= valSummaryChangePersonStatusToActive.ClientID %>');
            var valSumCont = document.getElementById('<%= valSummaryChangePersonStatusToContingent.ClientID %>');
            var valSumTerm = document.getElementById('<%= valSummaryChangePersonStatusToTerminate.ClientID %>');
            var valSumRtP = document.getElementById('<%= valSummaryChangeStatusToRightToPresent.ClientID %>');

            var displayNoneClass = "displayNone";

            if (rbn.checked) {
                div.className = "padLeft25 PaddingTop6";
                divRtP.className = displayNoneClass;
                if (divContgn != null) {
                    divContgn.className = displayNoneClass;
                }
                if (divTerm != null) {
                    divTerm.className = displayNoneClass;
                }
                if (valSumActv != null) {
                    valSumActv.className = "";
                }
                if (valSumCont != null) {
                    valSumCont.className = displayNoneClass;
                }
                if (valSumTerm != null) {
                    valSumTerm.className = displayNoneClass;
                }
                if (valSumRtP != null) {
                    valSumRtP.className = displayNoneClass;
                }
            }
        }

        function showDivTerminate() {
            var rbn = document.getElementById('<%= rbnTerminate.ClientID %>');
            var div = document.getElementById('<%= divTerminate.ClientID %>');
            var rbnActve = document.getElementById('<%= rbnActive.ClientID %>');
            var rbnContgn = document.getElementById('<%= rbnContingent.ClientID %>');
            var divActv = document.getElementById('<%= divActive.ClientID %>');
            var divContgn = document.getElementById('<%= divContingent.ClientID %>');
            var divRtP = document.getElementById('<%= divRighttoPresent.ClientID %>');
            var valSumActv = document.getElementById('<%= valSummaryChangePersonStatusToActive.ClientID %>');
            var valSumTerm = document.getElementById('<%= valSummaryChangePersonStatusToTerminate.ClientID %>');
            var valSumCont = document.getElementById('<%= valSummaryChangePersonStatusToContingent.ClientID %>');
            var valSumRtP = document.getElementById('<%= valSummaryChangeStatusToRightToPresent.ClientID %>');
            var displayNoneClass = "displayNone";

            if (rbn.checked) {
                div.className = "padLeft25 PaddingTop6";

                if (valSumTerm != null) {
                    valSumTerm.className = "";
                }
                if (divActv != null) {
                    divActv.className = displayNoneClass;
                }
                if (valSumActv != null) {
                    valSumActv.className = displayNoneClass;
                }
                if (divContgn != null) {
                    divContgn.className = displayNoneClass;
                }
                if (valSumCont != null) {
                    valSumCont.className = displayNoneClass;
                }
                if (valSumRtP != null) {
                    valSumRtP.className = displayNoneClass;
                }
            }
        }

        function enterPressed(evn) {
            if (window.event && window.event.keyCode == 13) {
                return false;
            } else if (evn && evn.keyCode == 13) {
                return false;
            }
        }

        function pageLoad() {
            document.onkeypress = enterPressed;
            SetTooltipsForallDropDowns();
        }

        function SetTooltipsForallDropDowns() {
            var optionList = document.getElementsByTagName('option');

            for (var i = 0; i < optionList.length; ++i) {
                optionList[i].title = DecodeString(optionList[i].innerHTML);
            }
        }


        function showTarget() {
            var chbInvestmentResouce = document.getElementById('<%= chbInvestmentResouce.ClientID %>');
            var tblTargetUtil = document.getElementById('<%= tblTargetUtil.ClientID %>');
            if (!chbInvestmentResouce.checked) {
                tblTargetUtil.style.display = 'none';
            }
            else {
                tblTargetUtil.style.display = 'inline';
            }
        }
    </script>
    <uc:LoadingProgress ID="LoadingProgress1" runat="server" />
    <asp:UpdatePanel ID="upnlBody" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <table class="PersonForm">
                <tr>
                    <td>
                        <table class="PersonInfo">
                            <tr>
                                <td>Legal First Name
                                </td>
                                <td>
                                    <asp:TextBox ID="txtFirstName" runat="server" CssClass="Width252Px" onchange="setDirty();"></asp:TextBox>
                                </td>
                                <td>
                                    <asp:RequiredFieldValidator ID="reqFirstName" runat="server" ValidationGroup="Person"
                                        ControlToValidate="txtFirstName" ErrorMessage="The First Name is required." EnableClientScript="False"
                                        SetFocusOnError="True" ToolTip="The First Name is required.">*</asp:RequiredFieldValidator>
                                    <asp:CustomValidator ID="custPersonName" runat="server" ControlToValidate="txtFirstName"
                                        ErrorMessage="There is another Person with the same First Name and Last Name."
                                        ToolTip="There is another Person with the same First Name and Last Name." ValidationGroup="Person"
                                        Text="*" EnableClientScript="false" SetFocusOnError="true" Display="Dynamic"
                                        OnServerValidate="custPersonName_ServerValidate"></asp:CustomValidator>
                                    <asp:RegularExpressionValidator ControlToValidate="txtFirstName" ValidationGroup="Person"
                                        ID="valRegFirstName" runat="server" ErrorMessage="First Name should be limited to 2-35 characters in length containing only letters and/or an apostrophe, hyphen, or a single space."
                                        ToolTip="First Name should be limited to 2-35 characters in length containing only letters and/or an apostrophe, hyphen, or a single space."
                                        EnableClientScript="false" Text="*" ValidationExpression="^[a-zA-Z'\-\ ]{2,35}$" />
                                    <asp:CustomValidator ID="cvFNAllowSpace" runat="server" ControlToValidate="txtFirstName"
                                        ErrorMessage="First Name should be limited to 2-35 characters in length containing only letters and/or an apostrophe, hyphen, or a single space."
                                        ToolTip="First Name should be limited to 2-35 characters in length containing only letters and/or an apostrophe, hyphen, or a single space."
                                        ValidationGroup="Person" Text="*" EnableClientScript="false" SetFocusOnError="true"
                                        Display="Dynamic" OnServerValidate="cvFNAllowSpace_ServerValidate"></asp:CustomValidator>
                                </td>
                                <td>Status
                                </td>
                                <td>
                                    <asp:DropDownList ID="ddlPersonStatus" runat="server" CssClass="Width256Px" onchange="setDirty();" OnSelectedIndexChanged="ddlPersonStatus_SelectedIndexChanged"
                                        AutoPostBack="true" Visible="false">
                                    </asp:DropDownList>
                                    <asp:Label ID="lblPersonStatus" runat="server"></asp:Label>
                                    <asp:CustomValidator ID="cvIsOwnerOrAssignedToProject" runat="server" ValidationGroup="ChangePersonStatusToContingent"
                                        Display="Dynamic" Text="*" OnServerValidate="custIsOwnerOrAssignedToProject_OnServerValidate"
                                        Enabled="false"></asp:CustomValidator>
                                </td>
                                <td>
                                    <asp:RequiredFieldValidator ID="reqPersonStatus" runat="server" ControlToValidate="ddlPersonStatus"
                                        ErrorMessage="The Status is required." ToolTip="The Status is required." ValidationGroup="Person"
                                        Text="*" EnableClientScript="false" SetFocusOnError="true" Display="Dynamic"></asp:RequiredFieldValidator>
                                </td>

                                <td>
                                    <asp:Button ID="btnChangeEmployeeStatus" runat="server" Text="Change Employee Status"
                                        UseSubmitBehavior="false" OnClick="btnChangeEmployeeStatus_Click" />&nbsp;
                                    
                                </td>

                            </tr>
                            <tr>
                                <td>Preferred First Name
                                </td>
                                <td>
                                    <asp:TextBox ID="txtPrefferedFirstName" runat="server" CssClass="Width252Px" onchange="setDirty();"></asp:TextBox>
                                </td>
                                <td>
                                    <asp:RegularExpressionValidator ControlToValidate="txtPrefferedFirstName" ID="valRegPrefferedFirstName"
                                        runat="server" ValidationGroup="Person" ErrorMessage="Preferred First Name should be limited to 2-35 characters in length containing only letters and/or an apostrophe, hyphen, or a single space."
                                        ToolTip="Preferred First Name should be limited to 2-35 characters in length containing only letters and/or an apostrophe, hyphen, or a single space."
                                        EnableClientScript="false" Text="*" ValidationExpression="^[a-zA-Z'\-\ ]{2,35}$" />
                                    &nbsp;
                                    <asp:CustomValidator ID="cvPFNAllowSpace" runat="server" ControlToValidate="txtPrefferedFirstName"
                                        ErrorMessage="Preferred First Name should be limited to 2-35 characters in length containing only letters and/or an apostrophe, hyphen, or a single space."
                                        ToolTip="Preferred First Name should be limited to 2-35 characters in length containing only letters and/or an apostrophe, hyphen, or a single space."
                                        ValidationGroup="Person" Text="*" EnableClientScript="false" SetFocusOnError="true"
                                        Display="Dynamic" OnServerValidate="cvPFNAllowSpace_ServerValidate"></asp:CustomValidator>
                                </td>
                                <td>Career Manager
                                </td>
                                <td class="padRight2">
                                    <asp:UpdatePanel ID="pnlLineManager" runat="server">
                                        <ContentTemplate>
                                            <uc:DefaultManager ID="defaultManager" runat="server" InsertFirtItem="false" PersonsRole="Practice Area Manager"
                                                CssClass="Width260Px" />
                                        </ContentTemplate>
                                        <Triggers>
                                            <asp:AsyncPostBackTrigger ControlID="lbSetPracticeOwner" EventName="Click" />
                                        </Triggers>
                                    </asp:UpdatePanel>
                                </td>
                                <td>&nbsp;
                                </td>
                                <td>
                                    <asp:Label Text="Cohort Assignment" ID="lblCohort" runat="server" Style="display: none;"></asp:Label>
                                    <asp:DropDownList ID="ddlCohortAssignment" runat="server" Style="display: none;">
                                    </asp:DropDownList>
                                    <asp:LinkButton ID="lbSetPracticeOwner" runat="server" PostBackUrl="#" OnClick="lbSetPracticeOwner_Click"
                                        Visible="false">Set Career Manager to Practice Area Owner</asp:LinkButton>
                                    <asp:HiddenField ID="hdnIsSetPracticeOwnerClicked" Value="false" runat="server" />
                                </td>

                            </tr>
                            <tr>
                                <td>Last Name
                                </td>
                                <td>
                                    <asp:TextBox ID="txtLastName" runat="server" CssClass="Width252Px" onchange="setDirty();"></asp:TextBox>
                                </td>
                                <td>
                                    <asp:RequiredFieldValidator ID="reqLastName" runat="server" ValidationGroup="Person"
                                        ControlToValidate="txtLastName" ErrorMessage="The Last Name is required." EnableClientScript="False"
                                        SetFocusOnError="True" ToolTip="The Last Name is required.">*</asp:RequiredFieldValidator>
                                    <asp:RegularExpressionValidator ControlToValidate="txtLastName" ID="valRegLastName"
                                        runat="server" ValidationGroup="Person" ErrorMessage="Last Name should be limited to 2-35 characters in length containing only letters and/or an apostrophe, hyphen, or a single space."
                                        ToolTip="Last Name should be limited to 2-35 characters in length containing only letters and/or an apostrophe, hyphen, or a single space."
                                        EnableClientScript="false" Text="*" ValidationExpression="^[a-zA-Z'\-\ ]{2,35}$" />
                                    &nbsp;
                                    <asp:CustomValidator ID="cvLNAllowSpace" runat="server" ControlToValidate="txtLastName"
                                        ErrorMessage="Last Name should be limited to 2-35 characters in length containing only letters and/or an apostrophe, hyphen, or a single space."
                                        ToolTip="Last Name should be limited to 2-35 characters in length containing only letters and/or an apostrophe, hyphen, or a single space."
                                        ValidationGroup="Person" Text="*" EnableClientScript="false" SetFocusOnError="true"
                                        Display="Dynamic" OnServerValidate="cvLNAllowSpace_ServerValidate"></asp:CustomValidator>
                                </td>
                                <td>Hire&nbsp;Date
                                </td>
                                <td class="DatePickerOuterTd">
                                    <uc2:DatePicker ID="dtpHireDate" runat="server" OnSelectionChanged="dtpHireDate_SelectionChanged"
                                        AutoPostBack="true" />
                                </td>
                                <td>
                                    <asp:RequiredFieldValidator ID="reqHireDate" runat="server" ControlToValidate="dtpHireDate"
                                        Display="Dynamic" EnableClientScript="False" ErrorMessage="The Hire Date is required."
                                        SetFocusOnError="True" ValidationGroup="Person" ToolTip="The Hire Date is required.">*</asp:RequiredFieldValidator>
                                    <asp:CompareValidator ID="compHireDate" runat="server" ControlToValidate="dtpHireDate"
                                        EnableClientScript="False" ErrorMessage="The Hire Date must be in the format 'MM/dd/yyyy'"
                                        Operator="DataTypeCheck" SetFocusOnError="True" ValidationGroup="Person" ToolTip="The Hire Date must be in the format 'MM/dd/yyyy'"
                                        Type="Date">*</asp:CompareValidator>
                                    <asp:CustomValidator ID="custWithPreviousTermDate" runat="server" ControlToValidate="dtpHireDate"
                                        ErrorMessage="Hire Date should be greater than previous Termination date." ToolTip="Hire Date should be greater than previous Termination date."
                                        ValidationGroup="Person" Text="*" Display="Dynamic" OnServerValidate="custWithPreviousTermDate_ServerValidate"
                                        ValidateEmptyText="false" SetFocusOnError="true" EnableClientScript="false"></asp:CustomValidator>
                                    <asp:CustomValidator ID="cvWithRtpStartDate" runat="server" ControlToValidate="dtpHireDate"
                                        ErrorMessage="Hire Date should be greater than Right to Present Start Date." ToolTip="Hire Date should be greater than Right to Present Start Date."
                                        ValidationGroup="Person" Text="*" Display="Dynamic" OnServerValidate="cvWithRtpStartDate_ServerValidate"
                                        ValidateEmptyText="false" SetFocusOnError="true" EnableClientScript="false"></asp:CustomValidator>
                                </td>
                                <td>
                                    <table id="tblRtpStartDate" runat="server" visible="false">
                                        <tr>
                                            <td>Right to Present Start Date
                                            </td>
                                            <td>
                                                <uc2:DatePicker ID="dtpRtpStartDate" runat="server" OnSelectionChanged="dtpRtpStartDate_SelectionChanged"
                                                    AutoPostBack="true" />
                                            </td>
                                            <td>
                                                <asp:RequiredFieldValidator ID="reqRtpStartDate" runat="server" ControlToValidate="dtpRtpStartDate" Enabled="false"
                                                    Display="Dynamic" EnableClientScript="False" ErrorMessage="The Right to Present Start Date is required."
                                                    SetFocusOnError="True" ValidationGroup="Person" ToolTip="The Right to Present Start Date is required.">*</asp:RequiredFieldValidator>
                                                <asp:CompareValidator ID="compRtpStartDate" runat="server" ControlToValidate="dtpRtpStartDate" Enabled="false"
                                                    EnableClientScript="False" ErrorMessage="The Right to Present Start Date must be in the format 'MM/dd/yyyy'"
                                                    Operator="DataTypeCheck" SetFocusOnError="True" ValidationGroup="Person" ToolTip="The Right to Present Start Date must be in the format 'MM/dd/yyyy'"
                                                    Type="Date">*</asp:CompareValidator>
                                            </td>
                                        </tr>
                                    </table>
                                </td>

                            </tr>
                            <tr>
                                <td>Title
                                </td>
                                <td>
                                    <pmc:CustomDropDown ID="ddlPersonTitle" runat="server" onchange="setDirty();" CssClass="Width256Px"
                                        OnSelectedIndexChanged="ddlPersonTitle_OnSelectedIndexChanged" AutoPostBack="true">
                                    </pmc:CustomDropDown>
                                </td>
                                <td>
                                    <asp:RequiredFieldValidator ID="rvPersonTitle" runat="server" ErrorMessage="The Title is required."
                                        ToolTip="The Title is required." ValidationGroup="Person" ControlToValidate="ddlPersonTitle"
                                        Text="*" EnableClientScript="false" SetFocusOnError="true" Display="Dynamic"></asp:RequiredFieldValidator>&nbsp;
                                    <asp:CustomValidator ID="cvSLTApproval" runat="server" OnServerValidate="cvSLTApproval_OnServerValidate"
                                        ValidationGroup="SLTApprovalForTitleChange" Text="*" EnableClientScript="false"
                                        SetFocusOnError="true" Display="Dynamic"></asp:CustomValidator>&nbsp;
                                    <asp:CustomValidator ID="cvSLTPTOApproval" runat="server" OnServerValidate="cvSLTPTOApproval_OnServerValidate"
                                        ValidationGroup="SLTApprovalForTitleChange" Text="*" EnableClientScript="false"
                                        SetFocusOnError="true" Display="Dynamic"></asp:CustomValidator>&nbsp;
                                    <asp:HiddenField ID="hdTitleChanged" runat="server" Value="0" />
                                    <asp:HiddenField ID="hdcvSLTApproval" runat="server" Value="false" />
                                    <asp:HiddenField ID="hdcvSLTPTOApproval" runat="server" Value="false" />
                                </td>
                                <td nowrap="nowrap">Termination Date&nbsp;
                                </td>
                                <td class="DatePickerOuterTd padRight10Imp">
                                    <uc2:DatePicker ID="dtpTerminationDate" runat="server" AutoPostBack="true" OnSelectionChanged="dtpTerminationDate_OnSelectionChanged"
                                        EnabledTextBox="false" ReadOnly="true" />
                                </td>
                                <td>
                                    <asp:CompareValidator ID="compTerminationDate" runat="server" ControlToValidate="dtpTerminationDate"
                                        Display="Dynamic" EnableClientScript="False" Enabled="False" EnableTheming="True"
                                        ErrorMessage="The Termination Date must be in the format 'MM/dd/yyyy'" Operator="DataTypeCheck"
                                        SetFocusOnError="True" ValidationGroup="Person" ToolTip="The Termination Date must be in the format 'MM/dd/yyyy'"
                                        Type="Date">*</asp:CompareValidator>
                                    <asp:CustomValidator ID="custTerminationDate" runat="server" ErrorMessage="To terminate the person the Termination Date should be specified."
                                        ToolTip="To terminate the person the Termination Date should be specified." ValidationGroup="Person"
                                        Text="*" Display="Dynamic" EnableClientScript="false" OnServerValidate="custTerminationDate_ServerValidate"></asp:CustomValidator>
                                    <asp:CompareValidator ID="cmpTerminateDate" runat="server" ControlToValidate="dtpTerminationDate"
                                        ControlToCompare="dtpHireDate" Operator="GreaterThanEqual" Type="Date" ErrorMessage="Termination date should be greater than or equal to Hire date."
                                        Display="Dynamic" Text="*" ValidationGroup="Person" ToolTip="Termination date should be greater than or equal to Hire date."
                                        SetFocusOnError="true"></asp:CompareValidator>
                                    <asp:CustomValidator ID="custTerminateDateTE" runat="server" ErrorMessage="" ToolTip=""
                                        Display="Dynamic" ValidationGroup="Person" Text="*" EnableClientScript="false"
                                        OnServerValidate="custTerminationDateTE_ServerValidate"></asp:CustomValidator>
                                    <asp:CustomValidator ID="custIsDefautManager" runat="server" ErrorMessage="Unable to set the Termination Date for this person because this person is set as the Default Career Manager for the company. Please select another Default Career Manager (Configuration > Company Resources > Default Career Manager) before terminating the individual."
                                        Display="Dynamic" ValidationGroup="Person" Text="*" EnableClientScript="false"
                                        OnServerValidate="custIsDefautManager_ServerValidate"></asp:CustomValidator>

                                    <AjaxControlToolkit:TextBoxWatermarkExtender ID="waterMarkTxtTerminationReason" runat="server"
                                        TargetControlID="txtTerminationReason" BehaviorID="waterMarkTxtTerminationReason"
                                        WatermarkCssClass="watermarkedtext Width252Px" WatermarkText="No Reason Selected">
                                    </AjaxControlToolkit:TextBoxWatermarkExtender>
                                    <asp:CustomValidator ID="custTerminationReason" runat="server" ErrorMessage="To terminate the person the Termination Reason should be specified."
                                        ToolTip="To terminate the person the Termination Reason should be specified."
                                        ValidationGroup="Person" Text="*" Display="Static" EnableClientScript="false"
                                        OnServerValidate="custTerminationReason_ServerValidate"></asp:CustomValidator>&nbsp;
                                </td>
                                <td>
                                    <table id="tblRtpEndDate" runat="server" visible="false">
                                        <tr>
                                            <td>Right to Present End Date&nbsp;
                                            </td>
                                            <td>
                                                <uc2:DatePicker ID="dtpRtpEndDate" runat="server" ReadOnly="true" EnabledTextBox="false"
                                                    AutoPostBack="false" />
                                            </td>
                                            <td>&nbsp;
                                            </td>
                                        </tr>
                                    </table>
                                </td>

                            </tr>
                            <tr>
                                <td>Location
                                </td>
                                <td>
                                    <asp:DropDownList ID="ddlLocation" runat="server" CssClass="Width256Px">
                                    </asp:DropDownList>
                                    &nbsp;
                                    <asp:RequiredFieldValidator ID="rfvLocation" runat="server" ErrorMessage="The Location is required."
                                        ControlToValidate="ddlLocation" ToolTip="The Location is required." ValidationGroup="Person"
                                        Text="*" EnableClientScript="false" SetFocusOnError="true" Display="Dynamic"></asp:RequiredFieldValidator>&nbsp;&nbsp;
                                </td>
                                <td></td>
                                <td>&nbsp; 
                                </td>
                                <td>
                                    <asp:DropDownList ID="ddlTerminationReason" runat="server" Visible="false" CssClass="Width256Px">
                                    </asp:DropDownList>
                                    <asp:TextBox ID="txtTerminationReason" runat="server" Visible="true" Enabled="false" CssClass="Width252Px"></asp:TextBox>

                                </td>
                                <td>&nbsp;
                                </td>
                                <td>&nbsp;
                                </td>

                            </tr>
                            <tr>
                                <td>Email&nbsp;Address
                                </td>
                                <td>
                                    <asp:TextBox ID="txtEmailAddress" runat="server" CssClass="Width120Px" onchange="setDirty();"></asp:TextBox>&nbsp;@&nbsp;
                                    <asp:DropDownList ID="ddlDomain" runat="server" onchange="setDirty();" CssClass="Width109PxImp">
                                    </asp:DropDownList>
                                </td>
                                <td>
                                    <asp:RequiredFieldValidator ID="rfvEmailAddress" runat="server" ControlToValidate="txtEmailAddress"
                                        ErrorMessage="The Email Address is required." ToolTip="The Email Address is required."
                                        ValidationGroup="Person" Text="*" EnableClientScript="false" SetFocusOnError="true"
                                        Display="Dynamic" ValidateEmptyText="true"></asp:RequiredFieldValidator>
                                    <asp:RegularExpressionValidator ID="regEmailAddress" runat="server" ControlToValidate="txtEmailAddress"
                                        Display="Dynamic" ErrorMessage="The Email Address is not valid." ValidationGroup="Person"
                                        ToolTip="The Email Address is not valid." Text="*" EnableClientScript="False"
                                        ValidationExpression="\w+([-+.']\w+)*"></asp:RegularExpressionValidator>
                                    <asp:CustomValidator ID="custEmailAddress" runat="server" ControlToValidate="txtEmailAddress"
                                        ErrorMessage="A user with the same email address already exists in the system. Please enter another email address."
                                        ToolTip="A user with the same email address already exists in the system. Please enter another email address."
                                        ValidationGroup="Person" Text="*" EnableClientScript="false" SetFocusOnError="true"
                                        Display="Dynamic" OnServerValidate="custEmailAddress_ServerValidate"></asp:CustomValidator>
                                    <asp:CustomValidator ID="custUserName" runat="server" ControlToValidate="txtEmailAddress"
                                        ErrorMessage="Unknown error occured. Please contact your system administrator."
                                        ToolTip="Unknown error occured. Please contact your system administrator." ValidateEmptyText="true"
                                        Text="*" EnableClientScript="false" SetFocusOnError="true" Display="Dynamic"
                                        ValidationGroup="Person" OnServerValidate="custUserName_ServerValidate"></asp:CustomValidator>&nbsp;
                                </td>
                                <td>
                                    <asp:Label ID="lbPayChexID" runat="server" Text="ADP ID" Visible="false"></asp:Label>

                                </td>
                                <td>
                                    <asp:TextBox ID="txtPayCheckId" runat="server" CssClass="Width252Px" onchange="setDirty();"
                                        Visible="false"></asp:TextBox>

                                </td>
                                <td></td>
                                <td>&nbsp;
                                </td>

                            </tr>
                            <tr>
                                <td nowrap="nowrap">Offshore Resource
                                </td>
                                <td>
                                    <asp:DropDownList ID="ddlPersonType" runat="server" CssClass="Width256Px" onchange="setDirty();">
                                        <asp:ListItem Text="NO" Value="0" Selected="True"></asp:ListItem>
                                        <asp:ListItem Text="YES" Value="1"></asp:ListItem>
                                    </asp:DropDownList>
                                </td>
                                <td>&nbsp;
                                </td>
                                <td nowrap="nowrap" class="padRight10Imp">Division 
                                </td>
                                <td>
                                    <asp:DropDownList ID="ddlDivision" runat="server" CssClass="Width256Px" AutoPostBack="true"
                                        OnSelectedIndexChanged="ddlDivision_SelectIndexChanged">
                                    </asp:DropDownList>

                                </td>
                                <td>
                                    <asp:RequiredFieldValidator ID="rfvDivision" runat="server" ErrorMessage="The Division is required."
                                        ControlToValidate="ddlDivision" ToolTip="The Division is required." ValidationGroup="Person"
                                        Text="*" EnableClientScript="false" SetFocusOnError="true" Display="Dynamic"></asp:RequiredFieldValidator>&nbsp;
                                   
                                </td>
                                <td>&nbsp;
                                </td>

                            </tr>
                            <tr>
                                <td>Telephone number
                                </td>
                                <td>
                                    <asp:TextBox ID="txtTelephoneNumber" runat="server" onchange="setDirty();" CssClass="Width252Px"></asp:TextBox>
                                </td>
                                <td>
                                    <asp:RegularExpressionValidator ID="reqTelphoneNumber" runat="server" ControlToValidate="txtTelephoneNumber"
                                        Display="Dynamic" ErrorMessage="The Telephone number is not valid. " ValidationGroup="Person"
                                        ToolTip="The Telephone number is not valid." Text="*" EnableClientScript="False"
                                        ValidationExpression="^[01]?[- .]?(\([2-9]\d{2}\)|[2-9]\d{2})[- .]?\d{3}[- .]?\d{4}$"></asp:RegularExpressionValidator>&nbsp;
                                    <asp:RequiredFieldValidator ID="rfvTelephoneNumber" runat="server" ErrorMessage="The Telephone number is required."
                                        ControlToValidate="txtTelephoneNumber" ToolTip="The Telephone number is required."
                                        ValidationGroup="Person" Text="*" EnableClientScript="false" SetFocusOnError="true"
                                        Display="Dynamic"></asp:RequiredFieldValidator>&nbsp;
                                </td>
                                <td>Practice Area  
                                </td>
                                <td>
                                    <asp:DropDownList ID="ddlDefaultPractice" runat="server" CssClass="Width256Px" onchange="setDirty();"
                                        OnSelectedIndexChanged="ddlDefaultPractice_OnSelectedIndexChanged" AutoPostBack="true">
                                    </asp:DropDownList>

                                </td>
                                <td>
                                    <asp:RequiredFieldValidator ID="rfvPracticeArea" runat="server" ErrorMessage="The Practice Area is required."
                                        ToolTip="The Practice Area is required." ValidationGroup="Person" Text="*" EnableClientScript="false"
                                        SetFocusOnError="true" Display="Dynamic" ControlToValidate="ddlDefaultPractice"></asp:RequiredFieldValidator>&nbsp;
                                    
                                </td>
                                <td>&nbsp;
                                </td>

                            </tr>
                            <tr>
                                <td>
                                    <asp:Label ID="lblEmployeeNumber" runat="server" Text="PersonID"></asp:Label>
                                </td>
                                <td>
                                    <asp:TextBox ID="txtEmployeeNumber" runat="server" MaxLength="12" onchange="setDirty();"
                                        accept="0" onclick="txtEmployeeNumber_ClientClick(this)" onkeypress="txtEmployeeNumber_ClientClick(this)"
                                        onblur="txtEmployeeNumber_OnBlur(this)" CssClass="Width252Px"></asp:TextBox>
                                    <asp:HiddenField ID="hdnPersonId" runat="server" />
                                    <asp:HiddenField ID="hdnIsDefaultManager" runat="server" />
                                </td>
                                <td>
                                    <asp:RequiredFieldValidator ID="reqEmployeeNumber" runat="server" ValidationGroup="Person"
                                        ControlToValidate="txtEmployeeNumber" EnableClientScript="False" ErrorMessage="The Employee Number is required."
                                        SetFocusOnError="True" ToolTip="The Employee Number is required.">*</asp:RequiredFieldValidator>
                                    <asp:CustomValidator ID="custEmployeeNumber" runat="server" ControlToValidate="txtEmployeeNumber"
                                        Display="Dynamic" EnableClientScript="false" ValidationGroup="Person" ErrorMessage="There is another Person with the same Employee Number."
                                        OnServerValidate="custEmployeeNumber_ServerValidate" SetFocusOnError="true" Text="*"
                                        ToolTip="There is another Person with the same Employee Number."></asp:CustomValidator>&nbsp;
                                </td>
                                <td>Practice Leadership
                                </td>
                                <td>
                                    <asp:DropDownList ID="ddlPracticeLeadership" runat="server" CssClass="Width256Px">
                                    </asp:DropDownList>

                                </td>
                                <td>&nbsp;
                                </td>
                                <td>&nbsp;
                                </td>

                            </tr>
                            <tr>
                                <td colspan="3">&nbsp;
                                </td>
                                <td>&nbsp;
                                </td>
                                <td>
                                    <table>
                                        <tr>
                                            <td class="padRight0">
                                                <asp:CheckBox ID="chbMBO" runat="server" />
                                            </td>
                                            <td>
                                                <label for="ctl00_body_chbMBO">
                                                    MBO</label>
                                            </td>
                                            <td>&ensp; &ensp; &ensp; &ensp; &ensp; &ensp;
                                            </td>
                                            <td class="padRight0">
                                                <asp:CheckBox ID="chbInvestmentResouce" runat="server" Style="padding-right: 0px"
                                                    onclick="showTarget();" />
                                            </td>
                                            <td>
                                                <label for="ctl00_body_chbInvestmentResouce">
                                                    Investment Resource</label>
                                            </td>
                                        </tr>
                                    </table>
                                </td>
                                <td>&nbsp;
                                </td>
                                <td>&nbsp;
                                </td>

                            </tr>
                            <tr>
                                <td colspan="3">&nbsp;
                                </td>
                                <td>&nbsp;
                                </td>
                                <td>
                                    <table>
                                        <tr>
                                            <td class="padRight0">
                                                <asp:CheckBox ID="chbLockedOut" runat="server" Checked="false" onclick="setDirty();" />
                                            </td>
                                            <td>
                                                <label for="ctl00_body_chbLockedOut">
                                                    Locked-Out</label>
                                            </td>
                                            <td>&ensp; &ensp;
                                            </td>
                                            <td>
                                                <table id="tblTargetUtil" runat="server" enableviewstate="true">
                                                    <tr>
                                                        <td class="padRight0">Target Utilization&ensp;
                                                        </td>
                                                        <td>
                                                            <asp:TextBox ID="txtTargetUtilization" runat="server" CssClass="Width40Px" MaxLength="3"></asp:TextBox>%
                                                            <asp:RequiredFieldValidator ID="rfvTxtTargetUtil" runat="server" ErrorMessage="The Target Utilization for the person is required."
                                                                ControlToValidate="txtTargetUtilization" ToolTip="The Target Utilization for the person is required."
                                                                ValidationGroup="Person" Text="*" EnableClientScript="true" SetFocusOnError="true"
                                                                Display="Dynamic"></asp:RequiredFieldValidator>
                                                            <asp:RangeValidator ID="rvTxtTargetUtil" runat="server" Type="Integer" MinimumValue="1"
                                                                MaximumValue="100" ControlToValidate="txtTargetUtilization" ErrorMessage="Target Utilization must be between 1% - 100%"
                                                                ToolTip="Target Utilization must be between 1% - 100%" ValidationGroup="Person"
                                                                Text="*" EnableClientScript="true" SetFocusOnError="true"> 
                                                            </asp:RangeValidator>
                                                        </td>
                                                    </tr>
                                                </table>
                                            </td>
                                        </tr>
                                    </table>
                                </td>
                                <td>&nbsp;
                                </td>
                                <td>&nbsp;
                                </td>

                            </tr>
                        </table>
                    </td>
                </tr>
                <tr>
                    <td>
                        <asp:CustomValidator ID="custPersonData" runat="server" OnServerValidate="custPersonData_ServerValidate"></asp:CustomValidator>
                        &nbsp;
                    </td>
                </tr>
            </table>
            <table class="WholeWidth">
                <tr>
                    <td class="PersonMultiView">
                        <asp:Table ID="tblPersonViewSwitch" runat="server" CssClass="CommonCustomTabStyle PersonDetailPage_CustomTabStyle">
                            <asp:TableRow ID="rowSwitcher" runat="server">
                                <asp:TableCell ID="cellSecurity" runat="server" CssClass="SelectedSwitch">
                                    <span class="bg">
                                        <asp:LinkButton ID="btnSecurity" runat="server" Text="Application Security" CausesValidation="false"
                                            CssClass="Width80Px" OnCommand="btnView_Command" CommandArgument="0"></asp:LinkButton>
                                    </span>
                                </asp:TableCell>
                                <asp:TableCell ID="cellPermissions" runat="server">
                                    <span class="bg">
                                        <asp:LinkButton ID="btnPermissions" runat="server" Text="Project Permissions"
                                            CssClass="Width83Px" CausesValidation="false" OnCommand="btnView_Command" CommandArgument="1"></asp:LinkButton>
                                    </span>
                                </asp:TableCell>
                                <asp:TableCell ID="cellCompensation" runat="server">
                                    <span class="bg">
                                        <asp:LinkButton ID="btnCompensation" runat="server" Text="&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; Compensation"
                                            CausesValidation="false" CssClass="Width83Px" OnCommand="btnView_Command" CommandArgument="2"></asp:LinkButton>
                                    </span>
                                </asp:TableCell>
                                <%--Command Argument = 7 for Recruiting Metrics tab--%>
                                <asp:TableCell ID="cellRecruitingMetrics" runat="server">
                                    <span class="bg">
                                        <asp:LinkButton ID="btnRecruitingMetrics" runat="server" Text="Recruiting Metrics"
                                            CausesValidation="false" CssClass="Width83Px" OnCommand="btnView_Command" CommandArgument="3"></asp:LinkButton>
                                    </span>
                                </asp:TableCell>
                                <asp:TableCell ID="cellEmploymentHistory" runat="server">
                                    <span class="bg">
                                        <asp:LinkButton ID="btnEmploymentHistory" runat="server" Text="Employment History"
                                            CausesValidation="false" CssClass="Width83Px" OnCommand="btnView_Command" CommandArgument="4"></asp:LinkButton>
                                    </span>
                                </asp:TableCell>
                                <asp:TableCell ID="cellProjects" runat="server">
                                    <span class="bg">
                                        <asp:LinkButton ID="btnProjects" runat="server" Text="&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; Projects"
                                            CausesValidation="false" CssClass="width50Px" OnCommand="btnView_Command" CommandArgument="5"></asp:LinkButton>
                                    </span>
                                </asp:TableCell>
                                <%--<asp:TableCell ID="cellOpportunities" runat="server">
                                    <span class="bg">
                                        <asp:LinkButton ID="btnOppportunities" runat="server" Text="&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; Opportunities"
                                            CausesValidation="false" CssClass="Width80Px" OnCommand="btnView_Command" CommandArgument="6"></asp:LinkButton>
                                    </span>
                                </asp:TableCell>--%>
                                <asp:TableCell ID="cellActivityLog" runat="server" Visible="false">
                                    <span class="bg">
                                        <asp:LinkButton ID="btnActivityLog" runat="server" Text="&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; History"
                                            CausesValidation="false" CssClass="Width45Px" OnCommand="btnView_Command" CommandArgument="6"></asp:LinkButton>
                                    </span>
                                </asp:TableCell>
                                <asp:TableCell ID="cellMSBadge" runat="server">
                                    <span class="bg">
                                        <asp:LinkButton ID="btnMSBadge" runat="server" Text="MS Badge History" CausesValidation="false"
                                            CssClass="Width80Px" OnCommand="btnView_Command" CommandArgument="7"></asp:LinkButton>
                                    </span>
                                </asp:TableCell>
                            </asp:TableRow>
                        </asp:Table>
                        <asp:MultiView ID="mvPerson" runat="server" ActiveViewIndex="0">
                            <asp:View ID="vwSecurity" runat="server" OnPreRender="vwPermissions_PreRender">
                                <asp:Panel ID="Panel6" runat="server" CssClass="tab-pane WholeWidth">
                                    <table cellpadding="3">
                                        <tr>
                                            <td class="Width60Px">
                                                <strong>
                                                    <asp:Localize ID="locRolesLabel" runat="server" Text="Roles"></asp:Localize></strong>
                                                <asp:CustomValidator ID="cvRoles" runat="server" Display="Dynamic" EnableClientScript="false"
                                                    ValidationGroup="Person" ErrorMessage="Person should have at least one role checked."
                                                    OnServerValidate="cvRoles_ServerValidate" SetFocusOnError="true" Text="*" ToolTip="Person should have at least one role checked."></asp:CustomValidator>
                                            </td>
                                            <td>&nbsp;
                                            </td>
                                            <td>
                                                <b>Seniority</b>
                                            </td>
                                            <td>&nbsp;
                                            </td>
                                        </tr>
                                        <tr>
                                            <td class="Width60Px">
                                                <asp:CheckBoxList ID="chblRoles" runat="server" AutoPostBack="true" OnSelectedIndexChanged="chblRoles_SelectedIndexChanged"
                                                    CssClass="Width170Px">
                                                </asp:CheckBoxList>
                                            </td>
                                            <td>&nbsp;
                                            </td>
                                            <td valign="top">
                                                <asp:DropDownList ID="ddlSeniority" runat="server" onchange="setDirty();" />
                                            </td>
                                            <td valign="top">
                                                <asp:RequiredFieldValidator ID="rfvSeniority" runat="server" ValidationGroup="Person"
                                                    ErrorMessage="The Seniority is required." ToolTip="The Seniority is required."
                                                    Text="*" EnableClientScript="false" SetFocusOnError="true" Display="Dynamic"
                                                    ControlToValidate="ddlSeniority"></asp:RequiredFieldValidator>
                                                &nbsp;
                                            </td>
                                        </tr>
                                        <tr>
                                            <td class="Width60Px">
                                                <asp:Button ID="btnResetPassword" runat="server" OnClick="btnResetPassword_Click"
                                                    UseSubmitBehavior="false" OnClientClick="if( !confirm('Do you really want to reset user\'s password?')) return false;"
                                                    Text="Reset Password" />
                                            </td>
                                            <td>&nbsp;
                                            </td>
                                            <td>&nbsp;
                                            </td>
                                            <td>&nbsp;
                                            </td>
                                        </tr>
                                        <tr>
                                            <td colspan="4">
                                                <asp:Label ID="lblPaswordResetted" runat="server" Text="The password was reset, and the person has been notified by email."
                                                    Visible="false"></asp:Label>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td colspan="4">
                                                <div class="DivPermissionDiffenrece">
                                                    <asp:GridView ID="gvPermissionDiffenrece" runat="server" AutoGenerateColumns="false"
                                                        CssClass="CompPerfTable PermissionDiffenrece">
                                                        <AlternatingRowStyle CssClass="alterrow" />
                                                        <Columns>
                                                            <asp:TemplateField>
                                                                <HeaderTemplate>
                                                                    <div class="ie-bg no-wrap">
                                                                        Page
                                                                    </div>
                                                                </HeaderTemplate>
                                                                <ItemTemplate>
                                                                    <%# Eval("Title") %>
                                                                </ItemTemplate>
                                                            </asp:TemplateField>
                                                            <asp:TemplateField>
                                                                <HeaderTemplate>
                                                                    <div class="ie-bg no-wrap">
                                                                        Current
                                                                    </div>
                                                                </HeaderTemplate>
                                                                <ItemTemplate>
                                                                    <%# ((bool)Eval("Old")) ? "Yes" : "No" %>
                                                                </ItemTemplate>
                                                            </asp:TemplateField>
                                                            <asp:TemplateField>
                                                                <HeaderTemplate>
                                                                    <div class="ie-bg no-wrap">
                                                                        New
                                                                    </div>
                                                                </HeaderTemplate>
                                                                <ItemTemplate>
                                                                    <%# ((bool)Eval("New")) ? "Yes" : "No" %>
                                                                </ItemTemplate>
                                                            </asp:TemplateField>
                                                            <asp:TemplateField>
                                                                <HeaderTemplate>
                                                                    <div class="ie-bg no-wrap">
                                                                        Diff.
                                                                    </div>
                                                                </HeaderTemplate>
                                                                <ItemTemplate>
                                                                    <%# ((bool)Eval("IsDifferent")) ? "Yes" : "No"%>
                                                                </ItemTemplate>
                                                            </asp:TemplateField>
                                                        </Columns>
                                                    </asp:GridView>
                                                </div>
                                            </td>
                                        </tr>
                                    </table>
                                </asp:Panel>
                            </asp:View>
                            <asp:View ID="vwPermissions" runat="server" OnPreRender="vwPermissions_PreRender">
                                <asp:Panel ID="pnlPermissions" runat="server" CssClass="tab-pane">
                                    <asp:HiddenField ID="hfReloadPerms" runat="server" Value="False" />
                                    <uc:RestrictionPanel ID="rpPermissions" runat="server" />
                                </asp:Panel>
                            </asp:View>
                            <asp:View ID="vwCompensation" runat="server">
                                <asp:Panel ID="pnlCompensation" runat="server" CssClass="tab-pane">
                                    <div id="divCompensationHistory" runat="server">
                                        <div class="filters Margin-Top5Px Margin-Bottom10Px">
                                            <div class="buttons-block">
                                                <span class="colorGray">Click on Start Date column in the grid to edit compensation
                                                    history item.</span>
                                                <asp:ShadowedTextButton ID="btnAddCompensation" runat="server" Text="Add Compensation"
                                                    OnClick="btnAddCompensation_Click" CssClass="add-btn" OnClientClick="if (!confirmSaveDirty()) return false;" />
                                                &nbsp;<asp:CustomValidator ID="custCompensationCoversMilestone" runat="server" ValidationGroup="Person"
                                                    ErrorMessage="This person has a status of Active/Termination Pending, but does not have an active compensation record. &nbsp;Go back to their record so you can create a compensation record for them, or set their status as Contingent or Terminated."
                                                    ToolTip="This person has a status of Active/Termination Pending, but does not have an active compensation record. &nbsp;Go back to their record so you can create a compensation record for them, or set their status as Contingent or Terminated."
                                                    OnServerValidate="custCompensationCoversMilestone_ServerValidate" Text="*"> </asp:CustomValidator>
                                                <div class="clear0">
                                                </div>
                                            </div>
                                        </div>
                                        <asp:GridView ID="gvCompensationHistory" runat="server" AutoGenerateColumns="False"
                                            OnRowDataBound="gvCompensationHistory_OnRowDataBound" EmptyDataText="No compensation history for this person."
                                            CssClass="CompPerfTable CompensationHistory" ShowFooter="false">
                                            <AlternatingRowStyle CssClass="alterrow" />
                                            <Columns>
                                                <asp:TemplateField>
                                                    <HeaderTemplate>
                                                        <div class="ie-bg">
                                                            &nbsp;
                                                        </div>
                                                    </HeaderTemplate>
                                                    <HeaderStyle CssClass="Width3Percent" />
                                                    <ItemTemplate>
                                                        <asp:ImageButton ID="imgCopy" ToolTip="Copy" runat="server" OnClick="imgCopy_OnClick"
                                                            ImageUrl="~/Images/copy.png" />
                                                    </ItemTemplate>
                                                </asp:TemplateField>
                                                <asp:TemplateField>
                                                    <HeaderTemplate>
                                                        <div class="ie-bg">
                                                            &nbsp;
                                                        </div>
                                                    </HeaderTemplate>
                                                    <HeaderStyle CssClass="Width3Percent" />
                                                    <ItemTemplate>
                                                        <asp:ImageButton ID="imgEditCompensation" ToolTip="Edit Compensation" runat="server"
                                                            OnClick="imgEditCompensation_OnClick" ImageUrl="~/Images/icon-edit.png" />
                                                    </ItemTemplate>
                                                    <EditItemTemplate>
                                                        <asp:ImageButton ID="imgUpdateCompensation" StartDate='<%# ((DateTime)Eval("StartDate")).ToString("MM/dd/yyyy") %>'
                                                            ToolTip="Save" runat="server" ImageUrl="~/Images/icon-check.png" OnClick="imgUpdateCompensation_OnClick"
                                                            operation="Update" />
                                                        <asp:ImageButton ID="imgCancel" ToolTip="Cancel" runat="server" ImageUrl="~/Images/no.png"
                                                            OnClick="imgCancel_OnClick" />
                                                    </EditItemTemplate>
                                                    <FooterTemplate>
                                                        <asp:ImageButton ID="imgUpdateCompensation" operation="Insert" ToolTip="Save" runat="server"
                                                            ImageUrl="~/Images/icon-check.png" OnClick="imgUpdateCompensation_OnClick" />
                                                        <asp:ImageButton ID="imgCancel" ToolTip="Cancel" runat="server" ImageUrl="~/Images/no.png"
                                                            OnClick="imgCancelFooter_OnClick" />
                                                    </FooterTemplate>
                                                </asp:TemplateField>
                                                <asp:TemplateField>
                                                    <HeaderTemplate>
                                                        <div class="ie-bg">
                                                            Start
                                                        </div>
                                                    </HeaderTemplate>
                                                    <ItemTemplate>
                                                        <asp:LinkButton ID="btnStartDate" runat="server" Text='<%# ((DateTime)Eval("StartDate")).ToString("MM/dd/yyyy") %>'
                                                            CommandArgument='<%# Eval("StartDate") %>' OnCommand="btnStartDate_Command" OnClientClick="if (!confirmSaveDirty()) return false;"></asp:LinkButton>
                                                    </ItemTemplate>
                                                    <EditItemTemplate>
                                                        <span class="fl-left Width85Percent">
                                                            <uc2:DatePicker ID="dpStartDate" ValidationGroup="CompensationUpdate" runat="server"
                                                                TextBoxWidth="90%" AutoPostBack="false" />
                                                        </span><span class="Width15Percent vMiddle">
                                                            <asp:RequiredFieldValidator ID="reqStartDate" runat="server" ControlToValidate="dpStartDate"
                                                                ValidationGroup="CompensationUpdate" ErrorMessage="The Start Date is required."
                                                                ToolTip="The Start Date is required." Text="*" EnableClientScript="false" SetFocusOnError="true"
                                                                Display="Static"></asp:RequiredFieldValidator>
                                                            <asp:CompareValidator ID="compStartDate" runat="server" ControlToValidate="dpStartDate"
                                                                ValidationGroup="CompensationUpdate" ErrorMessage="The Start Date has an incorrect format. It must be 'MM/dd/yyyy'."
                                                                ToolTip="The Start Date has an incorrect format. It must be 'MM/dd/yyyy'." Text="*"
                                                                EnableClientScript="false" SetFocusOnError="true" Display="Dynamic" Operator="DataTypeCheck"
                                                                Type="Date"></asp:CompareValidator>
                                                        </span>
                                                    </EditItemTemplate>
                                                    <FooterTemplate>
                                                        <span class="fl-left Width85Percent">
                                                            <uc2:DatePicker ID="dpStartDate" ValidationGroup="CompensationUpdate" runat="server"
                                                                TextBoxWidth="90%" AutoPostBack="false" />
                                                        </span><span class="Width15Percent vMiddle">
                                                            <asp:RequiredFieldValidator ID="reqStartDate" runat="server" ControlToValidate="dpStartDate"
                                                                ValidationGroup="CompensationUpdate" ErrorMessage="The Start Date is required."
                                                                ToolTip="The Start Date is required." Text="*" EnableClientScript="false" SetFocusOnError="true"
                                                                Display="Static"></asp:RequiredFieldValidator>
                                                            <asp:CompareValidator ID="compStartDate" runat="server" ControlToValidate="dpStartDate"
                                                                ValidationGroup="CompensationUpdate" ErrorMessage="The Start Date has an incorrect format. It must be 'MM/dd/yyyy'."
                                                                ToolTip="The Start Date has an incorrect format. It must be 'MM/dd/yyyy'." Text="*"
                                                                EnableClientScript="false" SetFocusOnError="true" Display="Dynamic" Operator="DataTypeCheck"
                                                                Type="Date"></asp:CompareValidator>
                                                        </span>
                                                    </FooterTemplate>
                                                    <HeaderStyle CssClass="Width8Percent" />
                                                </asp:TemplateField>
                                                <asp:TemplateField>
                                                    <HeaderTemplate>
                                                        <div class="ie-bg">
                                                            End
                                                        </div>
                                                    </HeaderTemplate>
                                                    <ItemTemplate>
                                                        <asp:Label ID="lblEndDate" runat="server" Text='<%# ((DateTime?)Eval("EndDate")).HasValue ? ((DateTime?)Eval("EndDate")).Value.AddDays(-1).ToString("MM/dd/yyyy") : string.Empty %>'></asp:Label>
                                                    </ItemTemplate>
                                                    <EditItemTemplate>
                                                        <span class="fl-left Width85Percent">
                                                            <uc2:DatePicker ID="dpEndDate" ValidationGroup="CompensationUpdate" runat="server"
                                                                TextBoxWidth="90%" AutoPostBack="false" />
                                                        </span><span class="Width15Percent vMiddle">
                                                            <asp:CompareValidator ID="compDateRange" runat="server" ControlToValidate="dpEndDate"
                                                                ValidationGroup="CompensationUpdate" ControlToCompare="dpStartDate" ErrorMessage="The End Date must be greater than the or equal to Start Date."
                                                                ToolTip="The End Date must be greater than the or equal to Start Date." Text="*"
                                                                EnableClientScript="false" SetFocusOnError="true" Display="Static" Operator="GreaterThanEqual"
                                                                Type="Date"></asp:CompareValidator>
                                                            <asp:CompareValidator ID="compEndDate" runat="server" ControlToValidate="dpEndDate"
                                                                ValidationGroup="CompensationUpdate" ErrorMessage="The End Date has an incorrect format. It must be 'MM/dd/yyyy'."
                                                                ToolTip="The End Date has an incorrect format. It must be 'MM/dd/yyyy'." Text="*"
                                                                EnableClientScript="false" SetFocusOnError="true" Display="Dynamic" Operator="DataTypeCheck"
                                                                Type="Date"></asp:CompareValidator>
                                                            <asp:CustomValidator ID="custValLockoutDates" runat="server" ValidationGroup="CompensationUpdate"
                                                                Display="Dynamic" Text="*" OnServerValidate="custValLockoutDates_OnServerValidate"></asp:CustomValidator>
                                                        </span>
                                                    </EditItemTemplate>
                                                    <FooterTemplate>
                                                        <span class="fl-left Width85Percent">
                                                            <uc2:DatePicker ID="dpEndDate" ValidationGroup="CompensationUpdate" runat="server"
                                                                TextBoxWidth="90%" AutoPostBack="false" />
                                                        </span><span class="Width15Percent vMiddle">
                                                            <asp:CompareValidator ID="compDateRange" runat="server" ControlToValidate="dpEndDate"
                                                                ValidationGroup="CompensationUpdate" ControlToCompare="dpStartDate" ErrorMessage="The End Date must be greater than the or equal to Start Date."
                                                                ToolTip="The End Date must be greater than the or equal to Start Date." Text="*"
                                                                EnableClientScript="false" SetFocusOnError="true" Display="Static" Operator="GreaterThanEqual"
                                                                Type="Date"></asp:CompareValidator>
                                                            <asp:CompareValidator ID="compEndDate" runat="server" ControlToValidate="dpEndDate"
                                                                ValidationGroup="CompensationUpdate" ErrorMessage="The End Date has an incorrect format. It must be 'MM/dd/yyyy'."
                                                                ToolTip="The End Date has an incorrect format. It must be 'MM/dd/yyyy'." Text="*"
                                                                EnableClientScript="false" SetFocusOnError="true" Display="Dynamic" Operator="DataTypeCheck"
                                                                Type="Date"></asp:CompareValidator>
                                                            <asp:CustomValidator ID="custValLockoutDates" runat="server" ValidationGroup="CompensationUpdate"
                                                                Display="Dynamic" Text="*" OnServerValidate="custValLockoutDates_OnServerValidate"></asp:CustomValidator>
                                                        </span>
                                                    </FooterTemplate>
                                                    <HeaderStyle CssClass="Width8Percent" />
                                                </asp:TemplateField>
                                                <asp:TemplateField>
                                                    <HeaderTemplate>
                                                        <div class="ie-bg">
                                                            &nbsp;
                                                        </div>
                                                    </HeaderTemplate>
                                                    <HeaderStyle CssClass="Width3Percent" />
                                                    <ItemTemplate>
                                                        <asp:Image ID="imgRtP" CssClass="rtp-Compensation" ToolTip="Right to Present Compensation" runat="server" Visible='<%# (bool)Eval("IsRighttoPresent") %>'
                                                            ImageUrl="~/Images/RtP_Comp_Icon.png" />
                                                    </ItemTemplate>
                                                </asp:TemplateField>
                                                <asp:TemplateField>
                                                    <HeaderTemplate>
                                                        <div class="ie-bg">
                                                            Division
                                                        </div>
                                                    </HeaderTemplate>
                                                    <ItemTemplate>
                                                        <asp:Label ID="lblDivision" runat="server" Text='<%# Eval("HtmlEncodedDivisionName")%>'></asp:Label>
                                                    </ItemTemplate>
                                                    <EditItemTemplate>
                                                        <asp:DropDownList ValidationGroup="CompensationUpdate" ID="ddlCompDivision" runat="server"
                                                            AutoPostBack="true" OnSelectedIndexChanged="ddlCompDivision_SelectIndexChanged"
                                                            CssClass="Width85Percent">
                                                        </asp:DropDownList>
                                                        <asp:CustomValidator ID="custValCompDivision" runat="server" ToolTip="Please select Division"
                                                            ValidationGroup="CompensationUpdate" Display="Dynamic" Text="*" ErrorMessage="Please select Division"
                                                            OnServerValidate="custValCompDivision_OnServerValidate">
                                                        </asp:CustomValidator>
                                                        <asp:CustomValidator ID="custLockOutDivision" runat="server" ValidationGroup="CompensationUpdate"
                                                            Display="Dynamic" Text="*" OnServerValidate="custLockOutDivision_OnServerValidate"></asp:CustomValidator>
                                                    </EditItemTemplate>
                                                    <FooterTemplate>
                                                        <asp:DropDownList ValidationGroup="CompensationUpdate" ID="ddlCompDivision" runat="server"
                                                            AutoPostBack="true" OnSelectedIndexChanged="ddlCompDivision_SelectIndexChanged"
                                                            CssClass="Width85Percent">
                                                        </asp:DropDownList>
                                                        <asp:CustomValidator ID="custValCompDivision" runat="server" ToolTip="Please select Division"
                                                            ValidationGroup="CompensationUpdate" Display="Dynamic" Text="*" ErrorMessage="Please select Division"
                                                            OnServerValidate="custValCompDivision_OnServerValidate">
                                                        </asp:CustomValidator>
                                                        <asp:CustomValidator ID="custLockOutDivision" runat="server" ValidationGroup="CompensationUpdate"
                                                            Display="Dynamic" Text="*" OnServerValidate="custLockOutDivision_OnServerValidate"></asp:CustomValidator>
                                                    </FooterTemplate>
                                                    <HeaderStyle CssClass="Width14Percent" HorizontalAlign="Center" />
                                                </asp:TemplateField>
                                                <asp:TemplateField>
                                                    <HeaderTemplate>
                                                        <div class="ie-bg">
                                                            Practice Area
                                                        </div>
                                                    </HeaderTemplate>
                                                    <ItemTemplate>
                                                        <asp:Label ID="lblpractice" runat="server" Text='<%# Eval("HtmlEncodedPracticeName")%>'></asp:Label>
                                                    </ItemTemplate>
                                                    <EditItemTemplate>
                                                        <asp:DropDownList ValidationGroup="CompensationUpdate" ID="ddlPractice" runat="server"
                                                            CssClass="Width85Percent">
                                                        </asp:DropDownList>
                                                        <asp:CustomValidator ID="custValPractice" runat="server" ToolTip="Please select Practice Area"
                                                            ValidationGroup="CompensationUpdate" Display="Dynamic" Text="*" ErrorMessage="Please select Practice Area"
                                                            OnServerValidate="custValPractice_OnServerValidate">
                                                        </asp:CustomValidator>
                                                        <asp:CustomValidator ID="custLockOutPractice" runat="server" ValidationGroup="CompensationUpdate"
                                                            Display="Dynamic" Text="*" OnServerValidate="custLockOutPractice_OnServerValidate"></asp:CustomValidator>
                                                    </EditItemTemplate>
                                                    <FooterTemplate>
                                                        <asp:DropDownList ValidationGroup="CompensationUpdate" ID="ddlPractice" runat="server"
                                                            CssClass="Width85Percent">
                                                        </asp:DropDownList>
                                                        <asp:CustomValidator ID="custValPractice" runat="server" ToolTip="Please select Practice Area"
                                                            ValidationGroup="CompensationUpdate" Display="Dynamic" Text="*" ErrorMessage="Please select Practice Area"
                                                            OnServerValidate="custValPractice_OnServerValidate">
                                                        </asp:CustomValidator>
                                                        <asp:CustomValidator ID="custLockOutPractice" runat="server" ValidationGroup="CompensationUpdate"
                                                            Display="Dynamic" Text="*" OnServerValidate="custLockOutPractice_OnServerValidate"></asp:CustomValidator>
                                                    </FooterTemplate>
                                                    <HeaderStyle CssClass="Width19Percent" HorizontalAlign="Center" />
                                                </asp:TemplateField>
                                                <asp:TemplateField>
                                                    <HeaderTemplate>
                                                        <div class="ie-bg">
                                                            Title
                                                        </div>
                                                    </HeaderTemplate>
                                                    <ItemTemplate>
                                                        <asp:Label ID="lblTitle" runat="server" Text=' <%# Eval("HtmlEncodedTitleName") %>'></asp:Label>
                                                    </ItemTemplate>
                                                    <EditItemTemplate>
                                                        <pmc:CustomDropDown ValidationGroup="CompensationUpdate" ID="ddlTitle" runat="server"
                                                            AutoPostBack="true" OnSelectedIndexChanged="ddlTitle_OnSelectedIndexChanged"
                                                            CssClass="Width80Percent">
                                                        </pmc:CustomDropDown>
                                                        <asp:CustomValidator ID="custValTitle" runat="server" ToolTip="Please select title"
                                                            ValidationGroup="CompensationUpdate" Display="Dynamic" Text="*" ErrorMessage="Please select title"
                                                            OnServerValidate="custValTitle_OnServerValidate">
                                                        </asp:CustomValidator>
                                                        <asp:CustomValidator ID="custLockoutTitle" runat="server" ValidationGroup="CompensationUpdate"
                                                            Display="Dynamic" Text="*" OnServerValidate="custLockoutTitle_OnServerValidate">
                                                        </asp:CustomValidator>
                                                    </EditItemTemplate>
                                                    <FooterTemplate>
                                                        <pmc:CustomDropDown ValidationGroup="CompensationUpdate" ID="ddlTitle" runat="server"
                                                            AutoPostBack="true" OnSelectedIndexChanged="ddlTitle_OnSelectedIndexChanged"
                                                            CssClass="Width80Percent">
                                                        </pmc:CustomDropDown>
                                                        <asp:CustomValidator ID="custValTitle" runat="server" ToolTip="Please select title"
                                                            ValidationGroup="CompensationUpdate" Display="Dynamic" Text="*" ErrorMessage="Please select title"
                                                            OnServerValidate="custValTitle_OnServerValidate">
                                                        </asp:CustomValidator>
                                                        <asp:CustomValidator ID="custLockoutTitle" runat="server" ValidationGroup="CompensationUpdate"
                                                            Display="Dynamic" Text="*" OnServerValidate="custLockoutTitle_OnServerValidate"></asp:CustomValidator>
                                                    </FooterTemplate>
                                                    <HeaderStyle CssClass="Width15Percent" HorizontalAlign="Center" />
                                                </asp:TemplateField>
                                                <asp:TemplateField>
                                                    <HeaderTemplate>
                                                        <div class="ie-bg">
                                                            Basis
                                                        </div>
                                                    </HeaderTemplate>
                                                    <ItemTemplate>
                                                        <asp:Label ID="lblBasis" runat="server" Text='<%# Eval("TimescaleName") %>'></asp:Label>
                                                    </ItemTemplate>
                                                    <EditItemTemplate>
                                                        <asp:DropDownList ID="ddlBasis" runat="server" CssClass="Width90Percent" OnSelectedIndexChanged="ddlBasis_OnSelectedIndexChanged"
                                                            AutoPostBack="true">
                                                            <asp:ListItem Text="W2-Salary" Value="W2-Salary"></asp:ListItem>
                                                            <asp:ListItem Text="W2-Hourly" Value="W2-Hourly"></asp:ListItem>
                                                            <asp:ListItem Text="1099/Hourly" Value="1099/Hourly"></asp:ListItem>
                                                            <asp:ListItem Text="1099/POR" Value="1099/POR"></asp:ListItem>
                                                        </asp:DropDownList>
                                                        <asp:CustomValidator ID="cvSalaryToContractVoilation" runat="server" ErrorMessage="To switch employee status from W2-Hourly or W2-Salary to a status of 1099 Hourly or 1099 POR, the user will have to terminate their employment using the 'Change Employee Status' workflow, select a termination reason, and then re-activate the person's status via the 'Change Employee Status' workflow, changing their pay type to '1099 Hourly' or '1099 POR'"
                                                            ValidationGroup="SalaryToContractVoilation" Text="*" ToolTip="To switch employee status from W2-Hourly or W2-Salary to a status of 1099 Hourly or 1099 POR, the user will have to terminate their employment using the 'Change Employee Status' workflow, select a termination reason, and then re-activate the person's status via the 'Change Employee Status' workflow, changing their pay type to '1099 Hourly' or '1099 POR'"></asp:CustomValidator>
                                                        <asp:CustomValidator ID="custLockoutBasis" runat="server" ValidationGroup="CompensationUpdate"
                                                            Display="Dynamic" Text="*" OnServerValidate="custLockoutBasis_OnServerValidate"></asp:CustomValidator>
                                                        <asp:CustomValidator ID="cvIsDivisionOrPracticeOwner" runat="server" ErrorMessage="This person is currently assigned as a Practice Area Owner or Division Owner.  Please reassign ownership and then make the change."
                                                            ValidationGroup="CompensationUpdate" Display="Dynamic" ToolTip="This person is currently assigned as a Practice Area Owner or Division Owner.  Please reassign ownership and then make the change."
                                                            Text="*" OnServerValidate="custIsDivisionOrPracticeOwner_OnServerValidate" Enabled="false"></asp:CustomValidator>
                                                    </EditItemTemplate>
                                                    <FooterTemplate>
                                                        <asp:DropDownList ID="ddlBasis" runat="server" CssClass="Width90Percent" OnSelectedIndexChanged="ddlBasis_OnSelectedIndexChanged"
                                                            AutoPostBack="true">
                                                            <asp:ListItem Text="W2-Salary" Value="W2-Salary"></asp:ListItem>
                                                            <asp:ListItem Text="W2-Hourly" Value="W2-Hourly"></asp:ListItem>
                                                            <asp:ListItem Text="1099/Hourly" Value="1099/Hourly"></asp:ListItem>
                                                            <asp:ListItem Text="1099/POR" Value="1099/POR"></asp:ListItem>
                                                        </asp:DropDownList>
                                                        <asp:CustomValidator ID="cvSalaryToContractVoilation" runat="server" ErrorMessage="To switch employee status from W2-Hourly or W2-Salary to a status of 1099 Hourly or 1099 POR, the user will have to terminate their employment using the 'Change Employee Status' workflow, select a termination reason, and then re-activate the person's status via the 'Change Employee Status' workflow, changing their pay type to '1099 Hourly' or '1099 POR'"
                                                            ValidationGroup="SalaryToContractVoilation" Text="*" ToolTip="To switch employee status from W2-Hourly or W2-Salary to a status of 1099 Hourly or 1099 POR, the user will have to terminate their employment using the 'Change Employee Status' workflow, select a termination reason, and then re-activate the person's status via the 'Change Employee Status' workflow, changing their pay type to '1099 Hourly' or '1099 POR'"></asp:CustomValidator>
                                                        <asp:CustomValidator ID="custLockoutBasis" runat="server" ValidationGroup="CompensationUpdate"
                                                            Display="Dynamic" Text="*" OnServerValidate="custLockoutBasis_OnServerValidate"></asp:CustomValidator>
                                                        <asp:CustomValidator ID="cvIsDivisionOrPracticeOwner" runat="server" ErrorMessage="This person is currently assigned as a Practice Area Owner or Division Owner.  Please reassign ownership and then make the change."
                                                            ValidationGroup="CompensationUpdate" Display="Dynamic" ToolTip="This person is currently assigned as a Practice Area Owner or Division Owner.  Please reassign ownership and then make the change."
                                                            Text="*" OnServerValidate="custIsDivisionOrPracticeOwner_OnServerValidate" Enabled="false"></asp:CustomValidator>
                                                    </FooterTemplate>
                                                    <HeaderStyle CssClass="Width10Percent" />
                                                </asp:TemplateField>
                                                <asp:TemplateField>
                                                    <HeaderTemplate>
                                                        <div class="ie-bg">
                                                            Vendor Indicator
                                                        </div>
                                                    </HeaderTemplate>
                                                    <ItemTemplate>
                                                        <asp:Label ID="lblVendor" runat="server" Text='<%# Eval("Vendor") != null ? Eval("Vendor.Name") : string.Empty %>'></asp:Label>
                                                    </ItemTemplate>
                                                    <EditItemTemplate>
                                                        <asp:DropDownList ID="ddlVendor" runat="server" CssClass="Width90Percent" AutoPostBack="true" Visible="false"
                                                            ValidationGroup="CompensationUpdate">
                                                        </asp:DropDownList>
                                                        <asp:RequiredFieldValidator ID="reqddlVendor" runat="server" ControlToValidate="ddlVendor"
                                                            Display="Dynamic" EnableClientScript="false" ErrorMessage="The Vendor Indicator is required." ValidationGroup="CompensationUpdate"
                                                            SetFocusOnError="true" Text="*" ToolTip="The Vendor Indicator is required." Enabled="false"></asp:RequiredFieldValidator>
                                                    </EditItemTemplate>
                                                    <FooterTemplate>
                                                        <asp:DropDownList ID="ddlVendor" runat="server" CssClass="Width90Percent" AutoPostBack="true"
                                                            ValidationGroup="CompensationUpdate">
                                                        </asp:DropDownList>
                                                        <asp:RequiredFieldValidator ID="reqddlVendor" runat="server" ControlToValidate="ddlVendor"
                                                            Display="Dynamic" EnableClientScript="false" ErrorMessage="The Vendor is required."
                                                            SetFocusOnError="true" Text="*" ToolTip="The Vendor is required." Enabled="false"></asp:RequiredFieldValidator>
                                                    </FooterTemplate>
                                                    <HeaderStyle CssClass="Width10Percent" />
                                                </asp:TemplateField>
                                                <asp:TemplateField>
                                                    <HeaderTemplate>
                                                        <div class="ie-bg">
                                                            Amount
                                                        </div>
                                                    </HeaderTemplate>
                                                    <ItemTemplate>
                                                        <asp:Label ID="lbAmount" runat="server" Text='<%# Bind("Amount") %>'></asp:Label>
                                                    </ItemTemplate>
                                                    <EditItemTemplate>
                                                        <span class="fl-left Width85Percent">
                                                            <asp:TextBox ID="txtAmount" ValidationGroup="CompensationUpdate" runat="server" CssClass="Width80Percent textRight"
                                                                MaxLength="16"></asp:TextBox>
                                                            <AjaxControlToolkit:FilteredTextBoxExtender ID="ftetxtAmount" runat="server" TargetControlID="txtAmount"
                                                                FilterMode="ValidChars" FilterType="Numbers,Custom" ValidChars=".">
                                                            </AjaxControlToolkit:FilteredTextBoxExtender>
                                                            <asp:HiddenField ID="hdAmount" runat="server" />
                                                            <asp:HiddenField ID="hdSLTApproval" runat="server" />
                                                        </span><span class="Width15Percent vMiddle">
                                                            <asp:RequiredFieldValidator ID="reqAmount" runat="server" ControlToValidate="txtAmount"
                                                                ValidationGroup="CompensationUpdate" ErrorMessage="The Amount is required." ToolTip="The Amount is required."
                                                                Text="*" EnableClientScript="false" Display="Dynamic" SetFocusOnError="true"></asp:RequiredFieldValidator>
                                                            <asp:CompareValidator ID="compAmount" runat="server" ControlToValidate="txtAmount"
                                                                ValidationGroup="CompensationUpdate" ErrorMessage="A number with 2 decimal digits is allowed for the Amount."
                                                                ToolTip="A number with 2 decimal digits is allowed for the Amount." Text="*"
                                                                EnableClientScript="false" SetFocusOnError="true" Operator="DataTypeCheck" Type="Currency"
                                                                Display="Dynamic"></asp:CompareValidator>
                                                            <asp:CustomValidator ID="cvSLTApprovalValidation" runat="server" OnServerValidate="cvSLTApprovalValidation_OnServerValidate"
                                                                ValidationGroup="CompensationUpdate" Text="*" EnableClientScript="false" SetFocusOnError="true"
                                                                Display="Dynamic"></asp:CustomValidator>
                                                            <asp:CompareValidator ID="cmpAmount" runat="server" ControlToValidate="txtAmount"
                                                                Operator="GreaterThan" Display="Dynamic" Type="Double" SetFocusOnError="true"
                                                                ValueToCompare="0" EnableClientScript="false" Text="*" ValidationGroup="CompensationUpdate"
                                                                ErrorMessage="Warning - Incorrect Pay: The wage must be greater than $0." ToolTip="Warning - Incorrect Pay: The wage must be greater than $0.">
                                                            </asp:CompareValidator>
                                                            <asp:CustomValidator ID="custLockoutAmount" runat="server" ValidationGroup="CompensationUpdate"
                                                                Display="Dynamic" Text="*" OnServerValidate="custLockoutAmount_OnServerValidate"></asp:CustomValidator>
                                                        </span>
                                                    </EditItemTemplate>
                                                    <FooterTemplate>
                                                        <span class="fl-left Width85Percent">
                                                            <asp:TextBox ID="txtAmount" ValidationGroup="CompensationUpdate" runat="server" CssClass="Width80Percent textRight"
                                                                MaxLength="16"></asp:TextBox>
                                                            <asp:HiddenField ID="hdAmount" runat="server" />
                                                            <AjaxControlToolkit:FilteredTextBoxExtender ID="ftetxtAmount" runat="server" TargetControlID="txtAmount"
                                                                FilterMode="ValidChars" FilterType="Numbers,Custom" ValidChars=".">
                                                            </AjaxControlToolkit:FilteredTextBoxExtender>
                                                            <asp:HiddenField ID="hdSLTApproval" runat="server" />
                                                        </span><span class="Width15Percent vMiddle">
                                                            <asp:RequiredFieldValidator ID="reqAmount" runat="server" ControlToValidate="txtAmount"
                                                                ValidationGroup="CompensationUpdate" ErrorMessage="The Amount is required." ToolTip="The Amount is required."
                                                                Text="*" EnableClientScript="false" Display="Dynamic" SetFocusOnError="true"></asp:RequiredFieldValidator>
                                                            <asp:CompareValidator ID="compAmount" runat="server" ControlToValidate="txtAmount"
                                                                ValidationGroup="CompensationUpdate" ErrorMessage="A number with 2 decimal digits is allowed for the Amount."
                                                                ToolTip="A number with 2 decimal digits is allowed for the Amount." Text="*"
                                                                EnableClientScript="false" SetFocusOnError="true" Operator="DataTypeCheck" Type="Currency"
                                                                Display="Dynamic"></asp:CompareValidator>
                                                            <asp:CustomValidator ID="cvSLTApprovalValidation" runat="server" OnServerValidate="cvSLTApprovalValidation_OnServerValidate"
                                                                ValidationGroup="CompensationUpdate" Text="*" EnableClientScript="false" SetFocusOnError="true"
                                                                Display="Dynamic"></asp:CustomValidator>
                                                            <asp:CompareValidator ID="cmpAmount" runat="server" ControlToValidate="txtAmount"
                                                                Operator="GreaterThan" Display="Dynamic" Type="Double" SetFocusOnError="true"
                                                                ValueToCompare="0" EnableClientScript="false" Text="*" ValidationGroup="CompensationUpdate"
                                                                ErrorMessage="Warning - Incorrect Pay: The wage must be greater than $0." ToolTip="Warning - Incorrect Pay: The wage must be greater than $0.">
                                                            </asp:CompareValidator>
                                                            <asp:CustomValidator ID="custLockoutAmount" runat="server" ValidationGroup="CompensationUpdate"
                                                                Display="Dynamic" Text="*" OnServerValidate="custLockoutAmount_OnServerValidate"></asp:CustomValidator>
                                                        </span>
                                                    </FooterTemplate>
                                                    <HeaderStyle CssClass="Width8Percent" />
                                                </asp:TemplateField>
                                                <asp:TemplateField>
                                                    <HeaderTemplate>
                                                        <div class="ie-bg">
                                                            PTO Accrual
                                                        </div>
                                                    </HeaderTemplate>
                                                    <ItemTemplate>
                                                        <asp:Label ID="lbVacationDays" runat="server" Text='<%# Bind("VacationDays") %>'></asp:Label>
                                                    </ItemTemplate>
                                                    <EditItemTemplate>
                                                        <span class="fl-left Width85Percent">
                                                            <asp:HiddenField ID="hdVacationDay" runat="server" />
                                                            <asp:HiddenField ID="hdSLTPTOApproval" runat="server" />
                                                            <asp:TextBox ID="txtVacationDays" ValidationGroup="CompensationUpdate" runat="server"
                                                                MaxLength="2" CssClass="Width80Percent" Text="0"></asp:TextBox>
                                                            <AjaxControlToolkit:FilteredTextBoxExtender ID="ftetxtVacationDays" runat="server"
                                                                TargetControlID="txtVacationDays" FilterMode="ValidChars" FilterType="Numbers">
                                                            </AjaxControlToolkit:FilteredTextBoxExtender>
                                                        </span><span class="Width15Percent vMiddle">
                                                            <asp:CompareValidator ID="compVacationDays" runat="server" ControlToValidate="txtVacationDays"
                                                                ValidationGroup="CompensationUpdate" Display="Dynamic" EnableClientScript="False"
                                                                ErrorMessage="The PTO Accrual must be an integer number." Operator="DataTypeCheck"
                                                                ToolTip="The PTO Accrual must be an integer number." Type="Integer">*</asp:CompareValidator>
                                                            <asp:RequiredFieldValidator ID="rfvVacationDays" runat="server" ControlToValidate="txtVacationDays"
                                                                ValidationGroup="CompensationUpdate" Text="*" EnableClientScript="false" Display="Dynamic"
                                                                ErrorMessage="PTO Accrual is required" ToolTip="PTO Accrual is required"></asp:RequiredFieldValidator>
                                                            <asp:CustomValidator ID="cvSLTPTOApprovalValidation" runat="server" OnServerValidate="cvSLTPTOApprovalValidation_OnServerValidate"
                                                                ValidationGroup="CompensationUpdate" Text="*" EnableClientScript="false" SetFocusOnError="true"
                                                                Display="Dynamic"></asp:CustomValidator>
                                                            <asp:CustomValidator ID="custLockoutPTO" runat="server" ValidationGroup="CompensationUpdate"
                                                                Display="Dynamic" Text="*" OnServerValidate="custLockoutPTO_OnServerValidate"></asp:CustomValidator>
                                                        </span>
                                                    </EditItemTemplate>
                                                    <FooterTemplate>
                                                        <span class="fl-left Width85Percent">
                                                            <asp:HiddenField ID="hdVacationDay" runat="server" />
                                                            <asp:HiddenField ID="hdSLTPTOApproval" runat="server" />
                                                            <asp:TextBox ID="txtVacationDays" ValidationGroup="CompensationUpdate" runat="server"
                                                                MaxLength="2" CssClass="Width80Percent" Text="0"></asp:TextBox>
                                                            <AjaxControlToolkit:FilteredTextBoxExtender ID="ftetxtVacationDays" runat="server"
                                                                TargetControlID="txtVacationDays" FilterMode="ValidChars" FilterType="Numbers">
                                                            </AjaxControlToolkit:FilteredTextBoxExtender>
                                                        </span><span class="Width15Percent vMiddle">
                                                            <asp:CompareValidator ID="compVacationDays" runat="server" ControlToValidate="txtVacationDays"
                                                                ValidationGroup="CompensationUpdate" Display="Dynamic" EnableClientScript="False"
                                                                ErrorMessage="The PTO Accrual must be an integer number." Operator="DataTypeCheck"
                                                                ToolTip="The PTO Accrual must be an integer number." Type="Integer">*</asp:CompareValidator>
                                                            <asp:RequiredFieldValidator ID="rfvVacationDays" runat="server" ControlToValidate="txtVacationDays"
                                                                ValidationGroup="CompensationUpdate" Text="*" EnableClientScript="false" Display="Dynamic"
                                                                ErrorMessage="PTO Accrual is required" ToolTip="PTO Accrual is required"></asp:RequiredFieldValidator>
                                                            <asp:CustomValidator ID="cvSLTPTOApprovalValidation" runat="server" OnServerValidate="cvSLTPTOApprovalValidation_OnServerValidate"
                                                                ValidationGroup="CompensationUpdate" Text="*" EnableClientScript="false" SetFocusOnError="true"
                                                                Display="Dynamic"></asp:CustomValidator>
                                                            <asp:CustomValidator ID="custLockoutPTO" runat="server" ValidationGroup="CompensationUpdate"
                                                                Display="Dynamic" Text="*" OnServerValidate="custLockoutPTO_OnServerValidate"></asp:CustomValidator>
                                                        </span>
                                                    </FooterTemplate>
                                                    <HeaderStyle CssClass="Width7Percent" />
                                                </asp:TemplateField>
                                                <asp:TemplateField>
                                                    <HeaderTemplate>
                                                        <div class="ie-bg">
                                                        </div>
                                                    </HeaderTemplate>
                                                    <ItemTemplate>
                                                        <asp:ImageButton ID="imgCompensationDelete" runat="server" AlternateText="Delete"
                                                            EndDate='<%# ((DateTime?)Eval("EndDate")).HasValue ? ((DateTime?)Eval("EndDate")).Value.ToString("MM/dd/yyyy") : string.Empty %>'
                                                            StartDate='<%# ((DateTime)Eval("StartDate")).ToString("MM/dd/yyyy") %>' ImageUrl="~/Images/cross_icon.png"
                                                            OnClick="imgCompensationDelete_OnClick" />
                                                        <AjaxControlToolkit:ConfirmButtonExtender ID="ConfirmButtonExtender1" ConfirmText="Are you sure you want to delete this Compensation?"
                                                            runat="server" TargetControlID="imgCompensationDelete">
                                                        </AjaxControlToolkit:ConfirmButtonExtender>
                                                        <asp:CustomValidator ID="cvDeleteCompensation" runat="server" Text="*" ErrorMessage="The Person is active during this compensation period."
                                                            ToolTip="The Person is active during this compensation period." ValidationGroup="CompensationDelete"></asp:CustomValidator>
                                                    </ItemTemplate>
                                                    <EditItemTemplate>
                                                    </EditItemTemplate>
                                                    <HeaderStyle CssClass="Width4Percent" />
                                                </asp:TemplateField>
                                            </Columns>
                                        </asp:GridView>
                                    </div>
                                    <div id="divCompensation" runat="server">
                                        <uc:PersonnelCompensation ID="personnelCompensation" runat="server" ValidationGroup="Person"
                                            OnDivisionChanged="personnelCompensation_OnDivisionChanged" OnSaveDetails="personnelCompensation_SaveDetails"
                                            StartDateReadOnly="true" OnTitleChanged="personnelCompensation_OnTitleChanged"
                                            OnPracticeChanged="personnelCompensation_OnPracticeChanged" rfvTitleValidationMessage="The Compensation Title is required."
                                            rfvPracticeValidationMessage="The Compensation Practice Area is required." />
                                        <asp:CustomValidator ID="custActiveCompensation" runat="server" ValidationGroup="Person"
                                            Text="*" Display="None" ErrorMessage="This person has a status of Active, but does not have an active compensation record"
                                            OnServerValidate="custActiveCompensation_ServerValidate"> </asp:CustomValidator>
                                    </div>
                                </asp:Panel>
                            </asp:View>
                            <asp:View ID="vwRecruitingMetrics" runat="server">
                                <asp:Panel ID="pnlRecruitingMetrics" runat="server" CssClass="tab-pane">
                                    <table class="Width50Percent">
                                        <tr class="Height30Px">
                                            <td class="padRight10Imp Width20PerImp">Recruiter
                                            </td>
                                            <td>
                                                <asp:DropDownList ID="ddlRecruiter" runat="server" onchange="setDirty();" CssClass="Width256Px" OnSelectedIndexChanged="ddlRecruiter_SelectedIndexChanged"
                                                    AutoPostBack="true">
                                                </asp:DropDownList>
                                            </td>
                                            <td>
                                                <asp:RequiredFieldValidator ID="rfvRecruiter" runat="server" ErrorMessage="The Recruiter is required."
                                                    ControlToValidate="ddlRecruiter" ToolTip="The Recruiter is required." ValidationGroup="Person"
                                                    Text="*" EnableClientScript="false" SetFocusOnError="true" Display="Dynamic"></asp:RequiredFieldValidator>&nbsp;
                                                <asp:CustomValidator ID="custRecruiter" runat="server" ControlToValidate="ddlRecruiter"
                                                    ErrorMessage="Cannot set a other person as recruiter." ToolTip="Cannot set a other person as recruiter."
                                                    ValidationGroup="Person" Text="*" EnableClientScript="false" SetFocusOnError="true"
                                                    Display="Dynamic" OnServerValidate="custRecruiter_ServerValidate"></asp:CustomValidator>&nbsp;
                                            </td>
                                        </tr>
                                        <tr class="Height30Px">
                                            <td>Job Seeker Status
                                            </td>
                                            <td>
                                                <asp:RadioButton ID="rbActiveCandidate" runat="server" Text="Active Candidate" GroupName="JobSeekerStatus" /><asp:RadioButton
                                                    ID="rbPassiveCandidate" runat="server" Text="Passive Candidate" GroupName="JobSeekerStatus" />
                                            </td>
                                        </tr>
                                        <tr class="Height30Px">
                                            <td>Source
                                            </td>
                                            <td>
                                                <asp:DropDownList ID="ddlSource" runat="server">
                                                </asp:DropDownList>
                                            </td>
                                        </tr>
                                        <tr class="Height30Px">
                                            <td>Targeted Company
                                            </td>
                                            <td>
                                                <asp:DropDownList ID="ddlTarget" runat="server">
                                                </asp:DropDownList>
                                            </td>
                                        </tr>
                                        <tr class="Height30Px">
                                            <td>Employee Referral
                                            </td>
                                            <td>
                                                <asp:RadioButton ID="rbEmpReferralNo" Text="No" runat="server" GroupName="EmployeeReferral"
                                                    AutoPostBack="true" OnCheckedChanged="rbActiveCandidate_CheckedChanged" />
                                                <asp:RadioButton ID="rbEmpReferralYes" runat="server" Text="Yes" GroupName="EmployeeReferral"
                                                    AutoPostBack="true" OnCheckedChanged="rbActiveCandidate_CheckedChanged" />
                                                <asp:DropDownList ID="ddlEmpReferral" runat="server" Enabled="false">
                                                </asp:DropDownList>
                                                <asp:CustomValidator ID="CustEmpReferral" runat="server" ErrorMessage=" Please select the name of the employee who provided the referral."
                                                    ToolTip=" Please select the name of the employee who provided the referral."
                                                    ValidationGroup="Person" Text="*" EnableClientScript="false" SetFocusOnError="true"
                                                    Display="Dynamic" OnServerValidate="CustEmpReferral_ServerValidate"></asp:CustomValidator>
                                            </td>
                                        </tr>
                                    </table>
                                </asp:Panel>
                            </asp:View>
                            <asp:View ID="vwEmploymentHistory" runat="server">
                                <asp:Panel ID="pnlEmploymentHistory" runat="server" CssClass="tab-pane">
                                    <asp:GridView ID="gvEmploymentHistory" runat="server" AutoGenerateColumns="false"
                                        CssClass="CompPerfTable CompensationHistory" ShowFooter="false">
                                        <AlternatingRowStyle CssClass="alterrow" />
                                        <Columns>
                                            <asp:TemplateField>
                                                <HeaderTemplate>
                                                    <div class="ie-bg">
                                                        Hire Date
                                                    </div>
                                                </HeaderTemplate>
                                                <ItemTemplate>
                                                    <asp:Label ID="lblHireDate" runat="server" Text='<%# ((DateTime)Eval("HireDate")).ToString("MM/dd/yyyy") %>'></asp:Label>
                                                </ItemTemplate>
                                            </asp:TemplateField>
                                            <asp:TemplateField>
                                                <HeaderTemplate>
                                                    <div class="ie-bg">
                                                        Termination Date
                                                    </div>
                                                </HeaderTemplate>
                                                <ItemTemplate>
                                                    <asp:Label ID="lblHireDate" runat="server" Text='<%# ((DateTime?)Eval("TerminationDate")).HasValue ? ((DateTime)Eval("TerminationDate")).ToString("MM/dd/yyyy") : string.Empty %>'></asp:Label>
                                                </ItemTemplate>
                                            </asp:TemplateField>
                                            <asp:TemplateField>
                                                <HeaderTemplate>
                                                    <div class="ie-bg">
                                                        Termination Reason
                                                    </div>
                                                </HeaderTemplate>
                                                <ItemTemplate>
                                                    <asp:Label ID="lblHireDate" runat="server" Text='<%# GetTerminationReasonById((int?)Eval("TerminationReasonId")) %>'></asp:Label>
                                                </ItemTemplate>
                                            </asp:TemplateField>
                                        </Columns>
                                    </asp:GridView>
                                </asp:Panel>
                            </asp:View>
                            <asp:View ID="vwProjects" runat="server">
                                <asp:Panel ID="pnlProjects" runat="server" CssClass="tab-pane WholeWidth">
                                    <uc:PersonProjects ID="personProjects" runat="server" />
                                </asp:Panel>
                            </asp:View>
                            <%--  <asp:View ID="vwOpportunities" runat="server">
                                <asp:Panel ID="pnOpportunities" runat="server" CssClass="tab-pane WholeWidth">
                                    <uc:OpportunityList ID="personOpportunities" runat="server" AllowAutoRedirectToDetails="false"
                                        FilterMode="ByTargetPerson" />
                                </asp:Panel>
                            </asp:View>--%>
                            <asp:View ID="vwActivityLog" runat="server">
                                <asp:Panel ID="pnlLog" runat="server" CssClass="tab-pane WholeWidth">
                                    <uc:Notes ID="nPerson" runat="server" Target="Person" OnNoteAdded="nPerson_OnNoteAdded" />
                                    <uc:ActivityLog runat="server" ID="activityLog" DisplayDropDownValue="TargetPerson"
                                        ValidationSummaryEnabled="false" DateFilterValue="Year" ShowDisplayDropDown="false"
                                        ShowProjectDropDown="false" ShowPersonDropDown="false" />
                                </asp:Panel>
                            </asp:View>
                            <asp:View ID="vwMSBadge" runat="server">
                                <asp:Panel ID="pnlMSBadge" runat="server" CssClass="tab-pane WholeWidth">
                                    <uc:MSBadge ID="personBadge" runat="server" ValidationGroup="Badge" />
                                </asp:Panel>
                            </asp:View>
                        </asp:MultiView>
                    </td>
                </tr>
                <tr>
                    <td>&nbsp;
                    </td>
                </tr>
                <tr>
                    <td>
                        <table class="WholeWidth">
                            <tr>
                                <td class="Width55Percent" align="right">
                                    <asp:Button ID="btnSave" runat="server" Text="Save" OnClick="btnSave_Click" UseSubmitBehavior="false" />&nbsp;
                                    <asp:CancelAndReturnButton ID="btnCancelAndReturn" runat="server" UseSubmitBehavior="false" />
                                </td>
                                <td align="right" class="padRight15">
                                    <asp:Button ID="btnNext" runat="server" Text="Next" OnClick="btnNext_Click" UseSubmitBehavior="false" />&nbsp;
                                    <asp:Button ID="btnWizardsCancel" runat="server" UseSubmitBehavior="false" Text="Cancel And Return"
                                        OnClick="btnWizardsCancel_Click" />
                                </td>
                            </tr>
                        </table>
                    </td>
                </tr>
            </table>
            <asp:HiddenField ID="hdnField" runat="server" />
            <AjaxControlToolkit:ModalPopupExtender ID="mpeViewTerminationDateErrors" runat="server"
                TargetControlID="hdnField" CancelControlID="btnClose" BackgroundCssClass="modalBackground"
                PopupControlID="pnlTerminationDateErrors" DropShadow="false" />
            <asp:Panel ID="pnlTerminationDateErrors" runat="server" CssClass="popUp PopUpPersonDetailPage"
                Style="display: none;">
                <table class="WholeWidth">
                    <tr class="PopUpHeader">
                        <th>Error
                            <asp:Button ID="btnClose" ToolTip="Close" runat="server" CssClass="mini-report-closeNew"
                                UseSubmitBehavior="false" Text="X"></asp:Button>
                        </th>
                    </tr>
                    <tr>
                        <td class="Padding6">
                            <div id="dvTerminationDateErrors" runat="server" visible="false" class="errorDivPersonDetailPage">
                                <asp:Label ID="lblTerminationDateError" runat="server" CssClass="PaddingTop7">
                                </asp:Label><br />
                                <asp:Label ID="lblTimeEntriesExist" runat="server" CssClass="PaddingTop7">
                                </asp:Label>
                                <div id="dvProjectMilestomesExist" class="PaddingTop5" runat="server">
                                    <asp:Label ID="lblProjectMilestomesExist" runat="server">
                                    </asp:Label><br />
                                    <asp:DataList ID="dtlProjectMilestones" runat="server" CssClass="WS-Normal">
                                        <ItemTemplate>
                                            &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;-&nbsp;&nbsp;<%# HttpUtility.HtmlEncode(((DataTransferObjects.Milestone)Container.DataItem).Project.ProjectNumber) + " - "+HttpUtility.HtmlEncode(((DataTransferObjects.Milestone)Container.DataItem).Project.Name)+ " - " + HttpUtility.HtmlEncode(((DataTransferObjects.Milestone)Container.DataItem).Description)%>
                                        </ItemTemplate>
                                    </asp:DataList>
                                </div>
                                <div id="divOwnerProjectsExist" class="PaddingTop10" runat="server">
                                    <asp:Label ID="lblOwnerProjectsExist" runat="server" Text="Person is designated as the Owner for the following projects:">
                                    </asp:Label><br />
                                    <asp:DataList ID="dtlOwnerProjects" runat="server" CssClass="WS-Normal">
                                        <ItemTemplate>
                                            &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;-&nbsp;&nbsp;<%# HttpUtility.HtmlEncode(((DataTransferObjects.Project)Container.DataItem).ProjectNumber)+ " - "+HttpUtility.HtmlEncode(((DataTransferObjects.Project)Container.DataItem).Name) %>
                                        </ItemTemplate>
                                    </asp:DataList>
                                </div>
                                <div id="divOwnerOpportunitiesExist" class="PaddingTop10" runat="server">
                                    <asp:Label ID="lblOwnerOpportunities" runat="server" Text="Person is designated as the Owner for the following Opportunities:">
                                    </asp:Label><br />
                                    <asp:DataList ID="dtlOwnerOpportunities" runat="server" CssClass="WS-Normal">
                                        <ItemTemplate>
                                            &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;-&nbsp;&nbsp;<%# HttpUtility.HtmlEncode(((DataTransferObjects.Opportunity)Container.DataItem).OpportunityNumber) + " - " + HttpUtility.HtmlEncode(((DataTransferObjects.Opportunity)Container.DataItem).Name) %>
                                        </ItemTemplate>
                                    </asp:DataList>
                                </div>
                            </div>
                        </td>
                    </tr>
                    <tr>
                        <td class="Padding6">Click OK to terminate this person and set an end date for all applicable projects.
                        </td>
                    </tr>
                    <tr>
                        <td align="center" class="Padding6">
                            <table class="Width70Percent">
                                <tr>
                                    <td class="textCenter Width50Percent">
                                        <asp:ImageButton ID="imgPrinter" runat="server" ImageUrl="~/Images/printer.png" ToolTip="Print"
                                            OnClientClick="return printform(1);" />
                                        <asp:ImageButton ID="lnkSaveReport" runat="server" ImageUrl="~/Images/saveToDisk.png"
                                            class="Margin-Left10Px" OnClientClick="saveReport(1);" OnClick="lnkSaveReport_OnClick"
                                            ToolTip="Save Report" /><asp:HiddenField ID="hdnSaveReportText" runat="server" />
                                    </td>
                                    <td class="textCenter Width50Percent">
                                        <asp:Button ID="btnTerminationProcessOK" runat="server" Text="OK" OnClick="btnTerminationProcessOK_OnClick"
                                            UseSubmitBehavior="false" CssClass="Width60Px" />
                                        &nbsp;
                                        <asp:Button ID="btnTerminationProcessCancel" runat="server" Text="Cancel" OnClick="btnTerminationProcessCancel_OnClick"
                                            UseSubmitBehavior="false" CssClass="Width60Px" />
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                </table>
            </asp:Panel>
            <asp:HiddenField ID="hdnOpenChangeStatusPopUp" runat="server" Value="" />
            <AjaxControlToolkit:ModalPopupExtender ID="mpeViewPersonChangeStatus" runat="server"
                TargetControlID="hdnOpenChangeStatusPopUp" BackgroundCssClass="modalBackground"
                PopupControlID="pnlChangeEmployeeStatus" DropShadow="false" />
            <asp:Panel ID="pnlChangeEmployeeStatus" runat="server" CssClass="popUpAttrition"
                Style="display: none;">
                <table>
                    <tr>
                        <th class="TextAlignLeft PaddingBottom10Imp">Change Employee Status:
                        </th>
                    </tr>
                    <tr>
                        <td>
                            <asp:RadioButton ID="rbnCancleTermination" runat="server" Text="Cancel Termination"
                                GroupName="rbtnsChangeStatus" />
                            <asp:RadioButton ID="rbnActive" runat="server" Text="Active" onclick="showDivActive()"
                                GroupName="rbtnsChangeStatus" />
                            <div id="divActive" runat="server">
                                <table>
                                    <tr>
                                        <td>Hire Date:&nbsp;
                                        </td>
                                        <td>
                                            <uc2:DatePicker ID="dtpActiveHireDate" runat="server" />
                                            <asp:RequiredFieldValidator ID="rfvActiveHireDate" runat="server" ControlToValidate="dtpActiveHireDate"
                                                Text="*" ErrorMessage="To Active the person the Hire Date should be specified."
                                                ToolTip="To Active the person the Hire Date should be specified." ValidationGroup="ChangePersonStatusToActive"
                                                Display="Dynamic" EnableClientScript="false" SetFocusOnError="true"></asp:RequiredFieldValidator>
                                            <asp:CompareValidator ID="cvActiveHireDateFormat" runat="server" ControlToValidate="dtpActiveHireDate"
                                                Display="Dynamic" EnableTheming="True" ErrorMessage="Please enter hire date in the correct format: MM/dd/yyyy."
                                                Operator="DataTypeCheck" SetFocusOnError="True" ValidationGroup="ChangePersonStatusToActive"
                                                ToolTip="Please enter hire date in the correct format: MM/dd/yyyy." Type="Date"
                                                EnableClientScript="false">*</asp:CompareValidator>
                                            <asp:CustomValidator ID="cvWithTerminationDate" runat="server" ControlToValidate="dtpActiveHireDate"
                                                ErrorMessage="New Hire Date should be greater than previous Termination date."
                                                ToolTip="New Hire Date should be greater than previous Termination date." ValidationGroup="ChangePersonStatusToActive"
                                                Text="*" Display="Dynamic" OnServerValidate="cvWithTerminationDate_ServerValidate"
                                                ValidateEmptyText="false" SetFocusOnError="true" EnableClientScript="false"></asp:CustomValidator>
                                            <asp:CompareValidator ID="cvWithRtpEndDate" runat="server" ControlToValidate="dtpActiveHireDate"
                                                ControlToCompare="dtpRtpStartDate" Operator="GreaterThan" Type="Date" ErrorMessage="Hire Date should be greater than Right to Present Start Date."
                                                Display="Dynamic" Text="*" ValidationGroup="ChangePersonStatusToActive" ToolTip="Hire Date should be greater than Right to Present Start Date."
                                                SetFocusOnError="true" EnableClientScript="false" Enabled="false"></asp:CompareValidator>
                                        </td>
                                    </tr>
                                </table>
                            </div>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <asp:RadioButton ID="rbnTerminate" runat="server" Text="Terminate" onclick="showDivTerminate()"
                                GroupName="rbtnsChangeStatus" />
                            <div id="divTerminate" runat="server">
                                <table>
                                    <tr>
                                        <td>Termination Date:&nbsp;
                                        </td>
                                        <td>
                                            <uc2:DatePicker ID="dtpPopUpTerminateDate" runat="server" AutoPostBack="true" OnSelectionChanged="dtpPopUpTerminationDate_OnSelectionChanged" />
                                            <asp:RequiredFieldValidator ID="rfvTerminationDate" runat="server" ControlToValidate="dtpPopUpTerminateDate"
                                                Text="*" ErrorMessage="To Terminate the person the Termination Date should be specified."
                                                ToolTip="To Terminate the person the Termination Date should be specified." ValidationGroup="ChangePersonStatusToTerminate"
                                                Display="Dynamic" EnableClientScript="false" SetFocusOnError="true"></asp:RequiredFieldValidator>
                                            <asp:CompareValidator ID="cvTerminationDateFormat" runat="server" ControlToValidate="dtpPopUpTerminateDate"
                                                Display="Dynamic" EnableTheming="True" ErrorMessage="Please enter termination date in the correct format: MM/dd/yyyy."
                                                Operator="DataTypeCheck" SetFocusOnError="True" ValidationGroup="ChangePersonStatusToTerminate"
                                                ToolTip="Please enter termination date in the correct format: MM/dd/yyyy." Type="Date"
                                                EnableClientScript="false">*</asp:CompareValidator>
                                            <asp:CompareValidator ID="cvWithHireDate" runat="server" ControlToValidate="dtpPopUpTerminateDate"
                                                ControlToCompare="dtpHireDate" Operator="GreaterThanEqual" Type="Date" ErrorMessage="Termination date should be greater than or equal to Hire date."
                                                Display="Dynamic" Text="*" ValidationGroup="ChangePersonStatusToTerminate" ToolTip="Termination date should be greater than or equal to Hire date."
                                                SetFocusOnError="true" EnableClientScript="false"></asp:CompareValidator>
                                            <asp:CompareValidator ID="cvWithRtpHireDate" runat="server" ControlToValidate="dtpPopUpTerminateDate"
                                                ControlToCompare="dtpRtpStartDate" Operator="GreaterThanEqual" Type="Date" ErrorMessage="Termination date should be greater than or equal to Right to Present start date."
                                                Display="Dynamic" Text="*" ValidationGroup="ChangePersonStatusToTerminate" ToolTip="Termination date should be greater than or equal to Right to Present start date."
                                                SetFocusOnError="true" EnableClientScript="false" Enabled="false"></asp:CompareValidator>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td class="PaddingTop5Imp">Termination Reason:&nbsp;
                                        </td>
                                        <td class="PaddingTop5Imp">
                                            <asp:DropDownList ID="ddlPopUpTerminationReason" runat="server">
                                            </asp:DropDownList>
                                            <asp:CustomValidator ID="cvTerminationReason" runat="server" ErrorMessage="To Terminate the person the Termination Reason should be specified."
                                                ToolTip="To Terminate the person the Termination Reason should be specified."
                                                ValidationGroup="ChangePersonStatusToTerminate" Text="*" Display="Dynamic" SetFocusOnError="true"
                                                OnServerValidate="cvTerminationReason_ServerValidate" EnableClientScript="false"></asp:CustomValidator>
                                        </td>
                                    </tr>
                                </table>
                            </div>
                            <div>
                            </div>
                            <asp:RadioButton ID="rbnContingent" runat="server" Text="Contingent" onclick="showDivContingent()"
                                GroupName="rbtnsChangeStatus" />
                            <div id="divContingent" runat="server">
                                <table>
                                    <tr>
                                        <td>Hire Date:&nbsp;
                                        </td>
                                        <td>
                                            <uc2:DatePicker ID="dtpContingentHireDate" runat="server" />
                                            <asp:RequiredFieldValidator ID="rfvContingentHireDate" runat="server" ControlToValidate="dtpContingentHireDate"
                                                Text="*" ErrorMessage="To Contingent the person the Hire Date should be specified."
                                                ToolTip="To Contingent the person the Hire Date should be specified." ValidationGroup="ChangePersonStatusToContingent"
                                                Display="Dynamic" EnableClientScript="false" SetFocusOnError="true"></asp:RequiredFieldValidator>
                                            <asp:CompareValidator ID="cvContingentHireDateFormat" runat="server" ControlToValidate="dtpContingentHireDate"
                                                Display="Dynamic" EnableTheming="True" ErrorMessage="Please enter hire date in the correct format: MM/dd/yyyy."
                                                Operator="DataTypeCheck" SetFocusOnError="True" ValidationGroup="ChangePersonStatusToContingent"
                                                Type="Date" EnableClientScript="false">*</asp:CompareValidator>
                                            <asp:CompareValidator ID="cvWithTermiantionDate" runat="server" ControlToValidate="dtpContingentHireDate"
                                                ControlToCompare="dtpTerminationDate" Operator="GreaterThan" Type="Date" ErrorMessage="New Hire date should be greater than previous Termination date."
                                                Display="Dynamic" Text="*" ValidationGroup="ChangePersonStatusToContingent" ToolTip="New Hire date should be greater than previous Termination date."
                                                SetFocusOnError="true" EnableClientScript="false"></asp:CompareValidator>
                                        </td>
                                    </tr>
                                </table>
                            </div>
                            <br />
                            <asp:RadioButton ID="rbnRighttoPresent" runat="server" Text="Right to Present" onclick="showDivRighttoPresent()" GroupName="rbtnsChangeStatus" />
                            <div id="divRighttoPresent" runat="server">
                                <table>
                                    <tr>
                                        <td>Right to Present Start Date:&nbsp;
                                        </td>
                                        <td>
                                            <uc2:DatePicker ID="dtpRighttoPresentStartDate" runat="server" />
                                            <asp:RequiredFieldValidator ID="rfvRighttoPresentStartDate" runat="server" ControlToValidate="dtpRighttoPresentStartDate"
                                                Text="*" ErrorMessage="The person Right to Present start date should be specified."
                                                ToolTip="The person Right to Present start date should be specified." ValidationGroup="ChangePersonStatusToRtP"
                                                Display="Dynamic" EnableClientScript="false" SetFocusOnError="true"></asp:RequiredFieldValidator>
                                            <asp:CompareValidator ID="cvRighttoPresentStartDate" runat="server" ControlToValidate="dtpRighttoPresentStartDate"
                                                Display="Dynamic" EnableTheming="True" ErrorMessage="Please enter Right to Present start date in the correct format: MM/dd/yyyy."
                                                Operator="DataTypeCheck" SetFocusOnError="True" ValidationGroup="ChangePersonStatusToRtP"
                                                Type="Date" EnableClientScript="false">*</asp:CompareValidator>
                                            <asp:CompareValidator ID="cvRtpToTermination" runat="server" ControlToValidate="dtpRighttoPresentStartDate"
                                                ControlToCompare="dtpTerminationDate" Operator="GreaterThan" Type="Date" ErrorMessage="New Right to Present start date should be greater than previous Termination date."
                                                Display="Dynamic" Text="*" ValidationGroup="ChangePersonStatusToRtP" ToolTip="New Right to Present start date should be greater than previous Termination date."
                                                SetFocusOnError="true" EnableClientScript="false"></asp:CompareValidator>
                                            <asp:CompareValidator ID="cvRtPStartToRtPEnd" runat="server" ControlToValidate="dtpRighttoPresentStartDate"
                                                ControlToCompare="dtpRtpEndDate" Operator="GreaterThan" Type="Date" ErrorMessage="New Right to Present start date should be greater than previous Right to Present end date."
                                                Display="Dynamic" Text="*" ValidationGroup="ChangePersonStatusToRtP" ToolTip="New Right to Present start date should be greater than previous Right to Present end date."
                                                SetFocusOnError="true" EnableClientScript="false"></asp:CompareValidator>
                                        </td>
                                    </tr>
                                </table>
                            </div>
                        </td>
                    </tr>
                    <tr>
                        <td colspan="2" class="Padding10">
                            <asp:ValidationSummary ID="valSummaryChangePersonStatusToContingent" runat="server"
                                ValidationGroup="ChangePersonStatusToContingent" EnableClientScript="false" SetFocusOnError="false" />
                            <asp:ValidationSummary ID="valSummaryChangePersonStatusToTerminate" runat="server"
                                ValidationGroup="ChangePersonStatusToTerminate" EnableClientScript="false" SetFocusOnError="false" />
                            <asp:ValidationSummary ID="valSummaryChangePersonStatusToActive" runat="server" ValidationGroup="ChangePersonStatusToActive"
                                EnableClientScript="false" SetFocusOnError="false" />
                            <asp:ValidationSummary ID="valSummaryChangeStatusToRightToPresent" runat="server" ValidationGroup="ChangePersonStatusToRtP"
                                EnableClientScript="false" SetFocusOnError="false" />
                        </td>
                    </tr>
                    <tr>
                        <td colspan="2" class="alignCenter PaddingTop5Imp">
                            <asp:Button ID="btnOkChangePersonStatus" Text="OK" runat="server" OnClick="btnOkChangePersonStatus_Click"
                                UseSubmitBehavior="false" />&nbsp;&nbsp;&nbsp;&nbsp;
                            <asp:Button ID="btnCancelChangePersonStatus" Text="Cancel" runat="server" OnClick="btnCancelChangePersonStatus_Click"
                                UseSubmitBehavior="false" />
                        </td>
                    </tr>
                </table>
            </asp:Panel>
            <asp:HiddenField ID="hndEmployeePayTypeChange" runat="server" Value="change" />
            <AjaxControlToolkit:ModalPopupExtender ID="mpeEmployeePayTypeChange" runat="server"
                TargetControlID="hndEmployeePayTypeChange" PopupControlID="pnlEmployeePayTypeChange"
                BackgroundCssClass="modalBackground" DropShadow="false">
            </AjaxControlToolkit:ModalPopupExtender>
            <asp:Panel ID="pnlEmployeePayTypeChange" runat="server" Style="display: none;" CssClass="popUpAttrition">
                <table>
                    <tr>
                        <td>
                            <asp:CustomValidator ID="cvEmployeePayTypeChangeViolation" runat="server" Text="*"
                                ForeColor="Black" ToolTip="" OnServerValidate="cvEmployeePayTypeChangeViolation_ServerValidate"
                                ValidationGroup="EmployeePayTypeChangeViolation" SetFocusOnError="true" EnableClientScript="false"></asp:CustomValidator>
                        </td>
                    </tr>
                    <tr>
                        <td style="text-align: center; padding: 4px;">
                            <asp:Button ID="btnEmployeePayTypeChangeViolationOk" runat="server" Text="Ok" OnClick="btnEmployeePayTypeChangeViolationOk_Click"
                                UseSubmitBehavior="false" />
                            &nbsp;
                            <asp:Button ID="btnEmployeePayTypeChangeViolationCancel" runat="server" Text="Cancel"
                                OnClick="btnEmployeePayTypeChangeViolationCancel_Click" UseSubmitBehavior="false" />
                        </td>
                    </tr>
                </table>
            </asp:Panel>
            <asp:HiddenField ID="hdnChangeStatusEndCompensation" runat="server" Value="change" />
            <AjaxControlToolkit:ModalPopupExtender ID="mpeChangeStatusEndCompensation" runat="server"
                TargetControlID="hdnChangeStatusEndCompensation" PopupControlID="pnlEndCompensation"
                BackgroundCssClass="modalBackground" DropShadow="false">
            </AjaxControlToolkit:ModalPopupExtender>
            <asp:Panel ID="pnlEndCompensation" runat="server" Style="display: none;" CssClass="popUpAttrition">
                <table>
                    <tr>
                        <td>
                            <asp:CustomValidator ID="cvEndCompensation" runat="server" Text="*" ErrorMessage=""
                                ForeColor="Black" ToolTip="" OnServerValidate="cvEndCompensation_ServerValidate"
                                ValidationGroup="EndCompensation" SetFocusOnError="true" EnableClientScript="false"></asp:CustomValidator>
                        </td>
                    </tr>
                    <tr>
                        <td style="text-align: center; padding: 4px;">
                            <asp:Button ID="bntEndCompensationOk" runat="server" Text="Ok" OnClick="btnEndCompensationOk_Click"
                                UseSubmitBehavior="false" />
                            &nbsp;
                            <asp:Button ID="btnEndCompensationCancel" runat="server" Text="Cancel" OnClick="btnEndCompensationCancel_Click"
                                UseSubmitBehavior="false" />
                        </td>
                    </tr>
                </table>
            </asp:Panel>
            <asp:HiddenField ID="hdnOwnerShip" runat="server" Value="change" />
            <AjaxControlToolkit:ModalPopupExtender ID="mpeOwnerShip" runat="server" PopupControlID="pnlOwnerShip"
                TargetControlID="hdnOwnerShip" BackgroundCssClass="modalBackground" DropShadow="false">
            </AjaxControlToolkit:ModalPopupExtender>
            <asp:Panel ID="pnlOwnerShip" runat="server" Style="display: none;" CssClass="popUpAttrition">
                <table>
                    <tr>
                        <td>
                            <asp:CustomValidator ID="cvIsOwnerForDivisionOrPractice" runat="server" Display="Dynamic"
                                ValidationGroup="OwnerShip" EnableClientScript="false" OnServerValidate="custIsOwnerForDivisionOrPractice_ServerValidate"></asp:CustomValidator>
                            <asp:Label ID="lblDivisionOwnerShip" runat="server"></asp:Label>
                            <br />
                            <asp:Label ID="lblPracticeAreaOwnerShip" runat="server"></asp:Label>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <br />
                        </td>
                    </tr>
                    <tr>
                        <td>Click on "OK" to go back.
                        </td>
                    </tr>
                    <tr>
                        <td style="text-align: center; padding: 4px;">
                            <asp:Button ID="btnOkOwnerShip" runat="server" Text="OK" OnClick="btnOkOwnerShip_Click"
                                UseSubmitBehavior="false" />
                        </td>
                    </tr>
                </table>
            </asp:Panel>
            <asp:HiddenField ID="hdHireDateChange" runat="server" Value="change" />
            <AjaxControlToolkit:ModalPopupExtender ID="mpeHireDateChange" runat="server" TargetControlID="hdHireDateChange"
                PopupControlID="pnlHireDateChange" BackgroundCssClass="modalBackground" DropShadow="false">
            </AjaxControlToolkit:ModalPopupExtender>
            <asp:Panel ID="pnlHireDateChange" runat="server" Style="display: none;" CssClass="popUpAttrition">
                <table>
                    <tr>
                        <td>
                            <asp:CustomValidator ID="cvHireDateChange" runat="server" Text="*" ErrorMessage=""
                                ForeColor="Black" ToolTip="" OnServerValidate="cvHireDateChange_ServerValidate"
                                ValidationGroup="HireDateChange" SetFocusOnError="true" EnableClientScript="false"></asp:CustomValidator>
                        </td>
                    </tr>
                    <tr>
                        <td style="text-align: center; padding: 4px;">
                            <asp:Button ID="btnHireDateChangeOk" runat="server" Text="Ok" OnClick="btnHireDateChangeOk_Click"
                                UseSubmitBehavior="false" />
                            &nbsp;
                            <asp:Button ID="btnHireDateChangeCancel" runat="server" Text="Cancel" OnClick="btnHireDateChangeCancel_Click"
                                UseSubmitBehavior="false" />
                        </td>
                    </tr>
                </table>
            </asp:Panel>
            <asp:HiddenField ID="hdnConsultantToContract" runat="server" Value="change" />
            <AjaxControlToolkit:ModalPopupExtender ID="mpeConsultantToContract" runat="server"
                TargetControlID="hdnConsultantToContract" PopupControlID="pnlConsultantToContract"
                BackgroundCssClass="modalBackground" DropShadow="false">
            </AjaxControlToolkit:ModalPopupExtender>
            <asp:Panel ID="pnlConsultantToContract" runat="server" Style="display: none;" CssClass="popUpAttrition yScrollAuto">
                <table>
                    <tr>
                        <td>
                            <p>
                                <asp:Label ID="lblPerson" runat="server"></asp:Label>
                                has following Attribution record(s). The records will update/delete accordingly.
                                Click "OK" to proceed with this change.
                            </p>
                            <br />
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <asp:DataList ID="dlAttributions" runat="server" CssClass="WS-Normal">
                                <ItemTemplate>
                                    &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;-&nbsp;&nbsp;<%# Eval("ProjectNumber") %>-
                                    <%# Eval("Name") %>
                                </ItemTemplate>
                            </asp:DataList>
                        </td>
                    </tr>
                    <tr>
                        <td style="text-align: center; padding: 4px;">
                            <asp:Button ID="btnOkConsultantToContract" runat="server" Text="Ok" UseSubmitBehavior="false"
                                CssClass="Width60Px" OnClick="btnOkConsultantToContract_Click" />
                            &nbsp;
                            <asp:Button ID="btnCloseConsultantToContract" runat="server" Text="Cancel" UseSubmitBehavior="false"
                                CssClass="Width60Px" OnClick="btnCloseConsultantToContract_Click" />
                        </td>
                    </tr>
                </table>
            </asp:Panel>
            <asp:HiddenField ID="hdnDivisionChange" runat="server" Value="change" />
            <AjaxControlToolkit:ModalPopupExtender ID="mpeDivisionChange" runat="server" TargetControlID="hdnDivisionChange"
                PopupControlID="pnlDivisionChange" BackgroundCssClass="modalBackground" DropShadow="false">
            </AjaxControlToolkit:ModalPopupExtender>
            <asp:Panel ID="pnlDivisionChange" runat="server" Style="display: none;" CssClass="popUpAttrition yScrollAuto">
                <table>
                    <tr>
                        <td>
                            <asp:CustomValidator ID="cvDivisionChange" runat="server" Text="" ErrorMessage=""
                                ForeColor="Black" ToolTip="" OnServerValidate="cvDivisionChange_ServerValidate"
                                ValidationGroup="DivisionChange" SetFocusOnError="true" EnableClientScript="false"></asp:CustomValidator>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <p>
                                <asp:Label runat="server" ID="lblPersonName"></asp:Label>
                                has following Attribution record(s). The records will update/delete accordingly.
                                Click "OK" to proceed with this change.
                                <br />
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <asp:DataList ID="dlCommissionAttribution" runat="server" CssClass="WS-Normal">
                                <ItemTemplate>
                                    &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;-&nbsp;&nbsp;<%# Eval("ProjectNumber") %>-
                                    <%# Eval("Name") %>
                                </ItemTemplate>
                            </asp:DataList>
                        </td>
                    </tr>
                    <tr>
                        <td style="text-align: center; padding: 4px;">
                            <asp:Button ID="btnDivisionChageOk" runat="server" Text="Ok" OnClick="btnDivisionChageOk_Click"
                                CssClass="Width60Px" UseSubmitBehavior="false" />
                            &nbsp;
                            <asp:Button ID="btnDivisionChangeCancel" runat="server" Text="Cancel" OnClick="btnDivisionChangeCancel_Click"
                                CssClass="Width60Px" UseSubmitBehavior="false" />
                        </td>
                    </tr>
                </table>
            </asp:Panel>
            <asp:HiddenField ID="hdRehireConfirmation" runat="server" Value="change" />
            <AjaxControlToolkit:ModalPopupExtender ID="mpeRehireConfirmation" runat="server"
                TargetControlID="hdRehireConfirmation" PopupControlID="pnlRehireConfirmation"
                BackgroundCssClass="modalBackground" DropShadow="false">
            </AjaxControlToolkit:ModalPopupExtender>
            <asp:Panel ID="pnlRehireConfirmation" runat="server" Style="display: none;" CssClass="popUpAttrition">
                <table>
                    <tr>
                        <td>
                            <asp:CustomValidator ID="cvRehireConfirmation" runat="server" Text="*" ErrorMessage=""
                                ForeColor="Black" ToolTip="" OnServerValidate="cvRehireConfirmation_ServerValidate"
                                ValidationGroup="RehireConfirmation" SetFocusOnError="true" EnableClientScript="false"></asp:CustomValidator>
                        </td>
                    </tr>
                    <tr>
                        <td style="text-align: center; padding: 4px;">
                            <asp:Button ID="btnRehireConfirmationOk" runat="server" Text="Ok" OnClick="btnRehireConfirmationOk_Click"
                                UseSubmitBehavior="false" />
                            &nbsp;
                            <asp:Button ID="btnRehireConfirmationCancel" runat="server" Text="Cancel" OnClick="btnRehireConfirmationCancel_Click"
                                UseSubmitBehavior="false" />
                        </td>
                    </tr>
                </table>
            </asp:Panel>
            <asp:HiddenField ID="hdpnlCancelTermination" runat="server" Value="" />
            <AjaxControlToolkit:ModalPopupExtender ID="mpeCancelTermination" runat="server" TargetControlID="hdpnlCancelTermination"
                BackgroundCssClass="modalBackground" PopupControlID="pnlCancelTermination" DropShadow="false" />
            <asp:Panel ID="pnlCancelTermination" runat="server" CssClass="popUp PopUpPersonDetailPage"
                Style="display: none;">
                <table class="WholeWidth">
                    <tr>
                        <td class="Padding6">
                            <div id="dvCancelTerminationDateErrors" runat="server" class="PaddingTop5">
                                <asp:CustomValidator ID="custCancelTermination" runat="server" Text="*" ErrorMessage=""
                                    ForeColor="Black" ToolTip="" OnServerValidate="custCancelTermination_ServerValidate"
                                    ValidationGroup="CancelTermination" SetFocusOnError="true" EnableClientScript="false"></asp:CustomValidator>
                                <br />
                                <br />
                                <asp:DataList ID="dtlCancelProjectMilestones" runat="server" CssClass="WS-Normal">
                                    <ItemTemplate>
                                        &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;-&nbsp;&nbsp;<%# HttpUtility.HtmlEncode(((DataTransferObjects.Milestone)Container.DataItem).Project.ProjectNumber) + " - "+HttpUtility.HtmlEncode(((DataTransferObjects.Milestone)Container.DataItem).Project.Name)+ " - " + HttpUtility.HtmlEncode(((DataTransferObjects.Milestone)Container.DataItem).Description)%>
                                    </ItemTemplate>
                                </asp:DataList>
                            </div>
                        </td>
                    </tr>
                    <tr>
                        <td align="center" class="Padding6">
                            <table class="Width70Percent">
                                <tr>
                                    <td class="textCenter Width50Percent">
                                        <asp:ImageButton ID="imgPrinterCancelTermination" runat="server" ImageUrl="~/Images/printer.png"
                                            ToolTip="Print" OnClientClick="return printform(2);" />
                                        <asp:ImageButton ID="lnkSaveReportCancelTermination" runat="server" ImageUrl="~/Images/saveToDisk.png"
                                            class="Margin-Left10Px" OnClientClick="saveReport(2);" OnClick="lnkSaveReportCancelTermination_OnClick"
                                            ToolTip="Save Report" /><asp:HiddenField ID="hdnSaveReportTextCancelTermination"
                                                runat="server" />
                                    </td>
                                    <td class="textCenter Width50Percent">
                                        <asp:Button ID="btnCancleTerminationOKButton" runat="server" Text="OK" OnClick="btnCancleTerminationOKButton_OnClick"
                                            UseSubmitBehavior="false" CssClass="Width60Px" />
                                        &nbsp;
                                        <asp:Button ID="btnCancelTerminationCancelButton" runat="server" Text="Cancel" OnClick="btnCancelTerminationCancelButton_OnClick"
                                            UseSubmitBehavior="false" CssClass="Width60Px" />
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                </table>
            </asp:Panel>
            <asp:HiddenField ID="hdnExtendingHireDate" runat="server" Value="" />
            <AjaxControlToolkit:ModalPopupExtender ID="mpeExtendingHireDate" runat="server" TargetControlID="hdnExtendingHireDate"
                BackgroundCssClass="modalBackground" PopupControlID="pnlExtendingHireDate" DropShadow="false" />
            <asp:Panel ID="pnlExtendingHireDate" runat="server" CssClass="popUp PopUpPersonDetailPage"
                Style="display: none;">
                <table class="WholeWidth">
                    <tr>
                        <td class="Padding6">
                            <div id="dvExtendingHireDate" runat="server" class="PaddingTop5">
                                <asp:CustomValidator ID="custMilestonesOnPreviousHireDate" runat="server" Text="*"
                                    ErrorMessage="" ForeColor="Black" ToolTip="" OnServerValidate="custMilestonesOnPreviousHireDate_ServerValidate"
                                    ValidationGroup="HireDateExtend" SetFocusOnError="true" EnableClientScript="false"></asp:CustomValidator>
                                <br />
                                <br />
                                <asp:DataList ID="dlMilestonesOnPreviousHireDate" runat="server" CssClass="WS-Normal">
                                    <ItemTemplate>
                                        &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;-&nbsp;&nbsp;<%# HttpUtility.HtmlEncode(((DataTransferObjects.Milestone)Container.DataItem).Project.ProjectNumber) + " - "+HttpUtility.HtmlEncode(((DataTransferObjects.Milestone)Container.DataItem).Project.Name)+ " - " + HttpUtility.HtmlEncode(((DataTransferObjects.Milestone)Container.DataItem).Description)%>
                                    </ItemTemplate>
                                </asp:DataList>
                            </div>
                        </td>
                    </tr>
                    <tr>
                        <td align="center" class="Padding6">
                            <table class="Width70Percent">
                                <tr>
                                    <td class="textCenter Width50Percent">
                                        <asp:ImageButton ID="imgPrinterMilestone" runat="server" ImageUrl="~/Images/printer.png"
                                            ToolTip="Print" OnClientClick="return printform(3);" />
                                        <asp:ImageButton ID="lnkSaveReportMilestone" runat="server" ImageUrl="~/Images/saveToDisk.png"
                                            class="Margin-Left10Px" OnClientClick="saveReport(3);" OnClick="lnkSaveReportMilestone_OnClick"
                                            ToolTip="Save Report" /><asp:HiddenField ID="hdnSaveReportHireDateExtend" runat="server" />
                                    </td>
                                    <td class="textCenter Width50Percent">
                                        <asp:Button ID="btnOkHireDateExtend" runat="server" Text="OK" OnClick="btnOkHireDateExtend_OnClick"
                                            UseSubmitBehavior="false" CssClass="Width60Px" />
                                        &nbsp;
                                        <asp:Button ID="btnCancelHireDateExtend" runat="server" Text="Cancel" OnClick="btnCancelHireDateExtend_OnClick"
                                            UseSubmitBehavior="false" CssClass="Width60Px" />
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                </table>
            </asp:Panel>
            <asp:HiddenField ID="hdnTargetErrorPanel" runat="server" />
            <AjaxControlToolkit:ModalPopupExtender ID="mpeErrorPanel" runat="server" BehaviorID="mpeErrorPanelBehaviourId"
                TargetControlID="hdnTargetErrorPanel" BackgroundCssClass="modalBackground" PopupControlID="pnlErrorPanel"
                CancelControlID="btnCancelErrorPanel" DropShadow="false" OkControlID="btnOKErrorPanel" />
            <asp:Panel ID="pnlErrorPanel" runat="server" Style="display: none;" CssClass="ProjectDetailErrorPanel PanelPerson">
                <table class="Width100Per">
                    <tr>
                        <th class="bgcolorGray TextAlignCenterImp vBottom">
                            <b class="BtnClose">Attention!</b>
                            <asp:Button ID="btnCancelErrorPanel" runat="server" CssClass="mini-report-close floatright"
                                ToolTip="Cancel" Text="X"></asp:Button>
                        </th>
                    </tr>
                    <tr>
                        <td class="Padding10Px">
                            <asp:ValidationSummary ID="valsPerson" runat="server" EnableClientScript="false"
                                CssClass="ApplyStyleForDashBoardLists" ValidationGroup="Person" />
                            <asp:ValidationSummary ID="valsManager" runat="server" EnableClientScript="false"
                                CssClass="ApplyStyleForDashBoardLists" ValidationGroup="ActiveManagers" />
                            <asp:ValidationSummary ID="valSumCompensationDelete" runat="server" EnableClientScript="false"
                                CssClass="ApplyStyleForDashBoardLists" ValidationGroup="CompensationDelete" />
                            <asp:ValidationSummary ID="valSumCompensation" runat="server" EnableClientScript="false"
                                CssClass="ApplyStyleForDashBoardLists" ValidationGroup="CompensationUpdate" />
                            <asp:ValidationSummary ID="valSumBadge" runat="server" EnableClientScript="false"
                                CssClass="ApplyStyleForDashBoardLists" ValidationGroup="Badge" />
                            <asp:ValidationSummary ID="ValSumSalaryToContractVoilation" runat="server" EnableClientScript="false"
                                CssClass="ApplyStyleForDashBoardLists" ValidationGroup="SalaryToContractVoilation" />
                            <uc:MessageLabel ID="mlConfirmation" runat="server" ErrorColor="Red" InfoColor="Green"
                                WarningColor="Orange" />
                            <uc:MessageLabel ID="mlError" runat="server" ErrorColor="Red" InfoColor="DarkGreen"
                                WarningColor="Orange" />
                        </td>
                    </tr>
                    <tr>
                        <td class="Padding10Px TextAlignCenterImp">
                            <asp:Button ID="btnOKErrorPanel" runat="server" Text="OK" Width="100" />
                        </td>
                    </tr>
                </table>
            </asp:Panel>
            <asp:HiddenField ID="hdWizardsCancelPopup" runat="server" Value="" />
            <AjaxControlToolkit:ModalPopupExtender ID="mpeWizardsCancelPopup" runat="server"
                TargetControlID="hdWizardsCancelPopup" BackgroundCssClass="modalBackground" PopupControlID="pnlWizardsCancelPopup"
                DropShadow="false" />
            <asp:Panel ID="pnlWizardsCancelPopup" runat="server" CssClass="popUp PopUpPersonDetailPage"
                Style="display: none;">
                <table class="WholeWidth">
                    <tr>
                        <td class="Padding6">You will lose all information that you have entered for adding this person to Practice
                            Management. Are you sure you wish to continue?
                        </td>
                    </tr>
                    <tr>
                        <td align="center" class="Padding6">
                            <asp:Button ID="btnWizardsCancelPopupOKButton" runat="server" Text="OK" OnClick="btnWizardsCancelPopupOKButton_OnClick"
                                UseSubmitBehavior="false" CssClass="Width60Px" />
                            &nbsp;
                            <asp:Button ID="btnWizardsCancelPopupCancelButton" runat="server" Text="Cancel" OnClick="btnWizardsCancelPopupCancelButton_OnClick"
                                UseSubmitBehavior="false" CssClass="Width60Px" />
                        </td>
                    </tr>
                </table>
            </asp:Panel>
            <asp:HiddenField ID="hdSLTApprovalPopUp" runat="server" Value="" />
            <AjaxControlToolkit:ModalPopupExtender ID="mpeSLTApprovalPopUp" runat="server" TargetControlID="hdSLTApprovalPopUp"
                BackgroundCssClass="modalBackground" PopupControlID="pnlSLTApprovalPopUp" DropShadow="false" />
            <asp:Panel ID="pnlSLTApprovalPopUp" runat="server" CssClass="popUp PopUpPersonDetailPage"
                Style="display: none;">
                <table class="WholeWidth">
                    <tr>
                        <td class="Padding6">
                            <asp:Literal ID="ltrlSLTApprovalPopUp" runat="server"></asp:Literal>
                        </td>
                    </tr>
                    <tr>
                        <td align="center" class="Padding6">
                            <asp:Button ID="btnSLTApproval" runat="server" Text="SLT Approval Received" OnClick="btnSLTApproval_OnClick"
                                UseSubmitBehavior="false" CssClass="Width160px" />
                            &nbsp;
                            <asp:Button ID="btnCancel" runat="server" Text="Cancel" OnClick="btnCancel_OnClick"
                                UseSubmitBehavior="false" CssClass="Width60Px" />
                        </td>
                    </tr>
                </table>
            </asp:Panel>
            <asp:HiddenField ID="hdSLTPTOApprovalPopUp" runat="server" Value="" />
            <AjaxControlToolkit:ModalPopupExtender ID="mpeSLTPTOApprovalPopUp" runat="server"
                TargetControlID="hdSLTPTOApprovalPopUp" BackgroundCssClass="modalBackground"
                PopupControlID="pnlSLTPTOApprovalPopUp" DropShadow="false" />
            <asp:Panel ID="pnlSLTPTOApprovalPopUp" runat="server" CssClass="popUp PopUpPersonDetailPage"
                Style="display: none;">
                <table class="WholeWidth">
                    <tr>
                        <td class="Padding6">
                            <asp:Literal ID="ltrlSLTPTOApprovalPopUp" runat="server"></asp:Literal>
                        </td>
                    </tr>
                    <tr>
                        <td align="center" class="Padding6">
                            <asp:Button ID="btnSLTPTOApproval" runat="server" Text="SLT Approval Received" OnClick="btnSLTPTOApproval_OnClick"
                                UseSubmitBehavior="false" CssClass="Width160px" />
                            &nbsp;
                            <asp:Button ID="btnCancelSLTPTOApproval" runat="server" Text="Cancel" OnClick="btnCancelSLTPTOApproval_OnClick"
                                UseSubmitBehavior="false" CssClass="Width60Px" />
                        </td>
                    </tr>
                </table>
            </asp:Panel>
        </ContentTemplate>
        <Triggers>
            <asp:PostBackTrigger ControlID="lnkSaveReport" />
            <asp:PostBackTrigger ControlID="lnkSaveReportCancelTermination" />
            <asp:PostBackTrigger ControlID="lnkSaveReportMilestone" />
            <asp:PostBackTrigger ControlID="btnOkChangePersonStatus" />
            <asp:PostBackTrigger ControlID="bntEndCompensationOk" />
            <asp:PostBackTrigger ControlID="btnTerminationProcessOK" />
            <asp:PostBackTrigger ControlID="btnSave" />
        </Triggers>
    </asp:UpdatePanel>
</asp:Content>

