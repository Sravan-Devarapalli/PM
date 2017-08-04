$(document).ready(function () {
    $('.FFLock-checkbox').live('click', function (evt) {
        var test = $('[id$=chbLockFFAmount]').attr("checked")
        if (!test) {
            $('[id$=lblMilestoneDiscount]').css("display", "none");
            $('[id$=txtMilestoneDiscount]').css("display", "");
            $('[id$=ddlDiscountType]').css("display", "");
            $('[id$=imgMilestonePersonEntryUpdate] ').each(function () {
                $(this).closest('tr').find('[id$="tblDiscount"]').css("display", "")
                $(this).closest('tr').find('[id$="lblDiscount"]').css("display", "none")
            });
        }
    })

    $('[id$=chbDiscountLock]').live('click', function (evt) {
        if ($(this).attr("checked")) {
            var a = $(this).closest('tr');
            $(this).closest('tr').find('[id$="tblDiscount"]').css("display", "");
            $(this).closest('tr').find('[id$="lblDiscount"]').css("display", "none")
        }
    })
})

function checkDirty(mpId) {
    if (showDialod()) {
        __doPostBack('__Page', mpId);
        return true;
    }
    return false;
}
function dtpStartDate_OnClientChange(dtp) {
    var index = dtp.id.indexOf('dpPerson');
    var row = dtp.id.substring(0, index);
    var chbBadgeRequired = document.getElementById(row + 'chbBadgeRequired');
    var chbOpsApproved = document.getElementById(row + 'chbOpsApproved');
    var dpPersonStart = document.getElementById(row + 'dpPersonStart_txtDate');
    var dpPersonEnd = document.getElementById(row + 'dpPersonEnd_txtDate');
    var dpBadgeStart = document.getElementById(row + 'dpBadgeStart_txtDate');
    var dpBadgeEnd = document.getElementById(row + 'dpBadgeEnd_txtDate');
    var hdnStartDateValue = document.getElementById(row + 'hdnStartDateValue');
    var hdnEndDateValue = document.getElementById(row + 'hdnEndDateValue');
    var hdnApproved = document.getElementById(row + 'hdnApproved');
    var hdnPersonId = document.getElementById(row + 'hdnPersonId');
    var ddlPersonName = document.getElementById(row + 'ddlPersonName');
    if (hdnPersonId.value != ddlPersonName.value)
        return;
    var oldPersonStartDate = new Date(hdnStartDateValue.value);
    var oldPersonEnddate = new Date(hdnEndDateValue.value);
    var newPersonStartdate = new Date(dpPersonStart.value);
    var newPersonEnddate = new Date(dpPersonEnd.value);
    var previousApproved = hdnApproved.value == 'Yes';
    if (chbBadgeRequired.checked) {
        if (oldPersonStartDate != newPersonStartdate || oldPersonEnddate != newPersonEnddate) {
            dpBadgeStart.value = dpPersonStart.value;
            dpBadgeEnd.value = dpPersonEnd.value;
            dtpBadgeStartDate_OnClientChange(dpBadgeStart);
        }
        else {
            chbOpsApproved.checked = previousApproved;
        }
    }
}

function dtpBadgeStartDate_OnClientChange(dtp) {
    var index = dtp.id.indexOf('dpBadge');
    var row = dtp.id.substring(0, index);
    var chbBadgeRequired = document.getElementById(row + 'chbBadgeRequired');
    var chbOpsApproved = document.getElementById(row + 'chbOpsApproved');
    var dpBadgeStart = document.getElementById(row + 'dpBadgeStart_txtDate');
    var dpBadgeEnd = document.getElementById(row + 'dpBadgeEnd_txtDate');
    var hdnStartDateValue = document.getElementById(row + 'hdnBadgeStartDateValue');
    var hdnEndDateValue = document.getElementById(row + 'hdnBadgeEndDateValue');
    var hdnApproved = document.getElementById(row + 'hdnApproved');
    var hdnApprovedChange = document.getElementById(row + 'hdnApprovedChange');
    var hdnPersonId = document.getElementById(row + 'hdnPersonId');
    var ddlPersonName = document.getElementById(row + 'ddlPersonName');
    if (hdnPersonId.value != ddlPersonName.value)
        return;
    var oldPersonStartDate = new Date(hdnStartDateValue.value);
    var oldPersonEnddate = new Date(hdnEndDateValue.value);
    var newPersonStartdate = new Date(dpBadgeStart.value);
    var newPersonEnddate = new Date(dpBadgeEnd.value);
    var previousApproved = hdnApproved.value == 'Yes';
    if (chbBadgeRequired.checked) {
        if (oldPersonStartDate != newPersonStartdate || oldPersonEnddate != newPersonEnddate) {
            if (oldPersonStartDate > newPersonStartdate || oldPersonEnddate < newPersonEnddate) {
                hdnApprovedChange.value = 'false';
                chbOpsApproved.checked = false;
            }
            else {
                hdnApprovedChange.value = 'true';
                chbOpsApproved.checked = previousApproved;
            }
        }
        else {
            chbOpsApproved.checked = previousApproved;
        }
    }
}

function dtpStartDateInsert_OnClientChange(dtp) {
    var index = dtp.id.indexOf('dpPerson');
    var row = dtp.id.substring(0, index);
    var chbBadgeRequired = document.getElementById(row + 'chbBadgeRequiredInsert');
    var dpPersonStart = document.getElementById(row + 'dpPersonStartInsert_txtDate');
    var dpPersonEnd = document.getElementById(row + 'dpPersonEndInsert_txtDate');
    var dpBadgeStart = document.getElementById(row + 'dpBadgeStartInsert_txtDate');
    var dpBadgeEnd = document.getElementById(row + 'dpBadgeEndInsert_txtDate');
    if (chbBadgeRequired.checked) {
        dpBadgeStart.value = dpPersonStart.value;
        dpBadgeEnd.value = dpPersonEnd.value;
    }
}

function ShowConfirmDialogForStartDate(chb) {
    if (confirm("Are you sure you want to Cancel Changes?")) {
        var btnCancelSaving = document.getElementById(btnCancelSavingId);
        btnCancelSaving.click();
    }
    else {
        chb.checked = false;
        document.getElementById(rbtnRemovePersonsStartDateId).checked = true;

    }

}

function ShowConfirmDialogForEndDate(chb) {
    if (confirm("Are you sure you want to Cancel Changes?")) {
        var btnCancelSaving = document.getElementById(btnCancelSavingId);
        btnCancelSaving.click();
    }
    else {
        chb.checked = false;
        document.getElementById(rbtnRemovePersonsEndDateId).checked = true;
    }

}

function SetTooltipsForallDropDowns() {
    var optionList = document.getElementsByTagName('option');

    for (var i = 0; i < optionList.length; ++i) {

        optionList[i].title = optionList[i].innerHTML;
    }

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

function ChangeActiveViewIndex(cntrl) {
    var mpEntryId = cntrl.attributes["mpEntryId"].value;
    var hdnEditEntryIdIndex = document.getElementById(hdnEditEntryIdIndexId);
    hdnEditEntryIdIndex.value = mpEntryId;
    return true;
}

function CheckIfDatesValid() {

    txtStartDate = document.getElementById(activityLogId + '_diRange_tbFrom');
    txtEndDate = document.getElementById(activityLogId + '_diRange_tbTo');
    var startDate = new Date(txtStartDate.value);
    var endDate = new Date(txtEndDate.value);
    if (txtStartDate.value != '' && txtEndDate.value != ''
    && startDate <= endDate) {
        var btnCustDatesClose = document.getElementById(activityLogId + '_btnCustDatesClose');
        hdnStartDate = document.getElementById(activityLogId + '_hdnStartDate');
        hdnEndDate = document.getElementById(activityLogId + '_hdnEndDate');
        lblCustomDateRange = document.getElementById(activityLogId + '_lblCustomDateRange');
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
    imgCalender = document.getElementById(activityLogId + '_imgCalender');
    lblCustomDateRange = document.getElementById(activityLogId + '_lblCustomDateRange');
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
    hdnStartDate = document.getElementById(activityLogId + '_hdnStartDate');
    hdnEndDate = document.getElementById(activityLogId + '_hdnEndDate');
    txtStartDate = document.getElementById(activityLogId + '_diRange_tbFrom');
    txtEndDate = document.getElementById(activityLogId + '_diRange_tbTo');
    hdnStartDateCalExtenderBehaviourId = document.getElementById(activityLogId + '_hdnStartDateCalExtenderBehaviourId');
    hdnEndDateCalExtenderBehaviourId = document.getElementById(activityLogId + '_hdnEndDateCalExtenderBehaviourId');

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

Sys.WebForms.PageRequestManager.getInstance().add_endRequest(endRequestHandle);

function endRequestHandle(sender, Args) {
    var activityLog = document.getElementById(activityLogId);
    if (activityLog != null) {
        imgCalender = document.getElementById(activityLogId + '_imgCalender');
        lblCustomDateRange = document.getElementById(activityLogId + '_lblCustomDateRange');
        ddlPeriod = document.getElementById(activityLogId + '_ddlPeriod');
        if (imgCalender.fireEvent && ddlPeriod.value != '0') {
            imgCalender.style.display = "none";
            lblCustomDateRange.style.display = "none";
        }
    }
}

//milestonePersonList.ascx
function imgMilestonePersonDelete_OnClientClick(imgDelete) {
    var hdMilestonePersonEntryId = document.getElementById(MilestonePersonEntryListControlId + '_hdMilestonePersonEntryId');
    var btnDeleteAllPersonEntries = document.getElementById(MilestonePersonEntryListControlId + '_btnDeleteAllPersonEntries');
    var btnDeletePersonEntry = document.getElementById(MilestonePersonEntryListControlId + '_btnDeletePersonEntry');
    var trDeleteOriginalEntry = document.getElementById('trDeleteOriginalEntry');
    var trDeleteExtendedEntry = document.getElementById('trDeleteExtendedEntry');
    var IsOriginalResource = imgDelete.getAttribute("IsOriginalResource");
    hdMilestonePersonEntryId.value = imgDelete.getAttribute("MilestonePersonEntryId");

    if (IsOriginalResource == 'false') {
        btnDeleteAllPersonEntries.style.display = "none";
        btnDeletePersonEntry.value = "   OK   ";
        trDeleteOriginalEntry.style.display = "none";
        trDeleteExtendedEntry.style.display = "block";
    }
    else {
        btnDeleteAllPersonEntries.style.display = "block";
        btnDeletePersonEntry.value = "Delete";
        trDeleteOriginalEntry.style.display = "block";
        trDeleteExtendedEntry.style.display = "none";
    }

    $find('mpeDeleteMileStonePersons').show();
    return false;
}
function btnClose_OnClientClick() {
    $find('mpeDeleteMileStonePersons').hide();
    return false;
}

function ShowPanel(object, displaypnl, lblTimeOffHoursId, lblProjectAffectedHoursId, TimeOffHours, ProjectAffectedHours) {
    var lblTimeOffHours = document.getElementById(lblTimeOffHoursId);
    var lblProjectAffectedHours = document.getElementById(lblProjectAffectedHoursId);
    lblTimeOffHours.innerHTML = TimeOffHours;
    lblProjectAffectedHours.innerHTML = ProjectAffectedHours;
    var obj = $("#" + object);
    var displayPanel = $("#" + displaypnl);

    iptop = obj.offset().top + obj[0].offsetHeight;
    ipleft = obj.offset().left - 70;
    displayPanel.offset({ top: iptop, left: ipleft });
    displayPanel.show();
    displayPanel.offset({ top: iptop, left: ipleft });
}

function HidePanel(hiddenpnl) {
    var displayPanel = $("#" + hiddenpnl);
    displayPanel.hide();
}

function ConvertDiscountUnits() {
    var revenueText = $('[id$=txtFixedRevenue]').first().val().replace(/,/g, '');
    var discountText = $('[id$=txtMilestoneDiscount]').first().val().replace(/,/g, '');
    var discountType = $('[id$=ddlDiscountType]').first().val();
    if (!isNaN(revenueText) && revenueText.length !== 0 && !isNaN(discountText) && discountText.length !== 0) {
        var rev = parseFloat(revenueText);
        var disc = parseFloat(discountText);
        if (discountType == "2") {
            $('[id$=txtMilestoneDiscount]').first().val((disc * 100 / (rev - disc)).toFixed(2));
            $('[id$=lblDoller]').css("display", "none");
            $('[id$=lblPercentage]').css("display", "inline");
        }
        else {
            $('[id$=txtMilestoneDiscount]').first().val((rev * disc / (disc + 100)).toFixed(2));
            $('[id$=lblDoller]').css("display", "inline");
            $('[id$=lblPercentage]').css("display", "none");
        }
    }
}
