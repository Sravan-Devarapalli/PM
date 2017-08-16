function OnResetType() {
    var RB1 = document.getElementById(rbnBudgetResetTypeId);
    var radio = RB1.getElementsByTagName("input");

    for (var i = 0; i < radio.length; i++) {
        if (radio[i].checked) {
            if (radio[i].value == "1") {
                $('[id$=spBudgetToDate]').css("display", "none");
            }
            else {
                $('[id$=spBudgetToDate]').css("display", "");
            }
        }
    }
}

$(document).ready(function () {
    $("a[requestid]").live("click", function (event) {
        event.preventDefault();
        var hdnProjectNumber = document.getElementById(hdnProjectNumberId);
        var win = window.open("Reports/BudgetComparison.aspx?projectnumber=" + hdnProjectNumber.value, '_blank');
        win.focus();
    });

    $("#tblMilestones").tablesorter({
        headers: {
            0: {
                sorter: false
            }
        },
        sortList: currentSort,
        sortForce: [[1, 0]]
    }).bind("sortEnd", function (sorter) {
        currentSort = sorter.target.config.sortList;
        var spanName = $("#tblMilestones #name");
        if (currentSort != '1,0' && currentSort != '1,1') {
            spanName[0].setAttribute('class', 'backGroundNone');
        }
        else {
            spanName[0].setAttribute('class', '');
        }
    });

    $("#tblBudgetManagementResource").tablesorter({
        widgets: ['zebra', 'staticRow']
    });
    $('#tblBudgetManagementResource th:first').click()
    SetBudgetManagementActualDropDown();

});

function EnableOrDisablePreProject(element) {
    var value = $("#ctl00_body_ddlBusinessOptions option:selected").val();
    var prevProjectLink = $("#ctl00_body_txtPrevProjectLink")
    if (value == '2') {
        prevProjectLink.removeAttr('disabled');
        $("#ctl00_body_imgNavigateToProject").show();
    }
    else {
        $("#ctl00_body_txtPrevProjectLink").attr('disabled', 'disabled');
        prevProjectLink.val('');
        $("#ctl00_body_imgNavigateToProject").hide();
    }


}

var fileError = 0;
function pageLoad() {
    if ($('#ctl00_body_budgetManagementByProject_ddlView option:selected').val() == '3') {
        $('#divBudgetManagement').attr('style', 'width: 100%');
    }
    if ($('#ctl00_body_budgetManagementByProject_ddlView option:selected').val() == '1') {
        $('#divBudgetManagement').attr('style', 'width: 50%');
    }
    EnableOrDisablePreProject();
    SetBudgetManagementActualDropDown();
    document.onkeypress = enterPressed;
    $("#" + fuAttachmentsUploadId).fileUpload({
        'uploader': 'Scripts/uploaderRemovedFolder1.swf',
        'cancelImg': 'Images/close_16.png',
        'buttonText': 'Browse File(s)',
        'script': 'Controls/Projects/AttachmentUpload.ashx',
        'fileExt': '*.xls;*.xlsx;*.xlw;*.doc;*.docx;*.pdf;*.ppt;*.pptx;*.mpp;*.vsd;*.msg;*.ZIP;*.RAR;*.sig;*.one*',
        'fileDesc': 'Excel;Word doc;PDF;PowerPoint;MS Project;Visio;Exchange;ZIP;RAR;OneNote',
        'multi': true,
        'auto': false,
        'allowSpecialChars': false,
        'ValidChars': '._',
        'sizeLimit': 4294656, //4194kb - 4294656bytes
        onComplete: function (event, queueID, fileObj, reponse, data) {
            var div = document.getElementById(uploadedFilesId);
            var lblUplodedFilesMsg = document.getElementById(lblUplodedFilesMsgId);
            if (lblUplodedFilesMsg.getAttribute("class") == "displayNone") {
                lblUplodedFilesMsg.setAttribute("class", "fontBold");
                div.appendChild(document.createElement("br"));
            }
            div.appendChild(document.createTextNode("- " + fileObj.name));
            div.appendChild(document.createElement("br"));

            var queueItem = document.getElementById(fuAttachmentsUploadId + queueID);
            queueItem.outerHTML = '';
        },
        onAllComplete: function (event, queueID, fileObj, response, data) {
            var uploadButton = document.getElementById(btnUploadId);
            uploadButton.disabled = "disabled";
            var progressBar = document.getElementById(loadingProgressId + '_upTimeEntries');
            progressBar.setAttribute('style', 'display:none;');
            if (fileError == 0) {
                var btnCancel = document.getElementById(btnCancelId);
                btnCancel.click();
            }
        },
        onError: function (event, queueID, fileObj, errorObj) {
            fileError++;
            var elementId = fuAttachmentsUploadId + queueID;
            var queueItem = document.getElementById(elementId);
            var imgElement = queueItem.firstChild.firstChild;
            imgElement.setAttribute("onclick", "javascript:(document.getElementById('" + elementId + "')).outerHTML= ''; fileError--; EnableUploadButton();");
        },
        onSelectOnce: function () {
            EnableUploadButton(true);
        },
        onCancelComplete: function () {
            EnableUploadButton();
        },
        onErrorComplete: function () {
            if (!IsQueueContainValidFiles()) {
                var progressBar = document.getElementById(loadingProgressId + '_upTimeEntries');
                progressBar.setAttribute('style', 'display:none;');
            }

            EnableUploadButton();
        }
    });
}

function ChangeCancelDivInnerHTML() {
    var cancelDiv = $('.fileUploadQueueItem .cancel');
    for (i = 0; i < cancelDiv.length; i++) {
        var anchorTags = cancelDiv[i].firstChild;
        var queueItemId = cancelDiv[i].parentElement.id;

        var imgElement = document.createElement('Img');
        imgElement.setAttribute("src", "Images/close_16.png");
        imgElement.setAttribute("class", "CursorPointer");
        cancelDiv[i].innerHTML = "";
        cancelDiv[i].appendChild(imgElement);

    }
}

function ClearVariables() {
    fileError = 0;
}

function startUpload() {
    var progressBar = document.getElementById(loadingProgressId + '_upTimeEntries');
    progressBar.setAttribute('style', '');
    ChangeCancelDivInnerHTML();
    var ddlAttachmentCategory = document.getElementById(ddlAttachmentCategoryId);
    var selectedValue = ddlAttachmentCategory.value;
    var hdnProjectId = document.getElementById(hdnProjectIdId);

    $("#" + fuAttachmentsUploadId).fileUploadSettings('scriptData', 'ProjectId=' + hdnProjectId.value + '&categoryId=' + selectedValue + '&LoggedInUser=' + UserIdentity);
    $("#" + fuAttachmentsUploadId).fileUploadStart();
}

function EnableUploadButton(selected) {
    var categorySelected = IsAttachmentCategorySelected();
    var fileSelected = (selected == true ? true : false);
    if (categorySelected && !fileSelected) {
        fileSelected = IsQueueContainValidFiles();
    }

    var uploadButton = document.getElementById(btnUploadId);
    uploadButton.disabled = categorySelected && fileSelected ? "" : "disabled";
}

function IsQueueContainValidFiles() {
    var fileUploadQueue = $('.fileUploadQueueItem');
    if (fileUploadQueue.length > 0) {
        return true;
    }
    return false;
}

function DownloadUnsavedFile(linkButton) {
    if (linkButton != null) {
        var lnkbutton = $('#' + linkButton.id)[0];
        var attachmentId = lnkbutton.getAttribute('attachmentid');
        var btn = document.getElementById(btnDownloadButtonId);
        var hdn = document.getElementById(hdnDownloadAttachmentIdId);
        hdn.value = attachmentId;
        btn.click();
    }
}

function enterPressed(evn) {
    if (window.event && window.event.keyCode == 13) {
        if (window.event.srcElement.tagName != "TEXTAREA") {
            return false;
        }
    } else if (evn && evn.keyCode == 13) {
        if (evn.originalTarget.type != "textarea") {
            return false;
        }
    }
}

function checkDirty(target, entityId) {
    var hdnIsMarginException = document.getElementById(hdnIsMarginExceptionId);
    var ddlProjectStatus = document.getElementById(ddlProjectStatusId);
    if (hdnIsMarginException.value == "True" && ddlProjectStatus.options[ddlProjectStatus.selectedIndex].value == "3") {
        __doPostBack('__Page', target + ':' + entityId);
        return false;
    }
    else if (showDialod()) {
        __doPostBack('__Page', target + ':' + entityId);
        return false;
    }
    return true;
}


function ConfirmSaveOrExit() {
    var hdnProjectId = document.getElementById(hdnProjectIdId);
    if (getDirty() || hdnProjectId.value == "") {
        return confirm("Some data isn't saved. Click Ok to Save the data or Cancel to exit.");
    }
    return true;
}


function cvProjectAttachment_ClientValidationFunction(obj, args) {
    args.IsValid = IsValidProjectAttachMent();
}

function cvAttachmentCategory_ClientValidationFunction(obj, args) {
    args.IsValid = IsAttachmentCategorySelected();
}

function IsAttachmentCategorySelected() {
    var ddlAttachmentCategory = document.getElementById(ddlAttachmentCategoryId);
    var categoryValue = ddlAttachmentCategory.value;
    if (categoryValue != "0") {
        return true; // Valid
    }
    else {
        return false; // Not valid
    }
}

function CanShowPrompt() {
    return true;
}

function ConfirmToDeleteProject() {
    var hdnProject = document.getElementById(hdnProjectDeleteId);
    var result = confirm("Do you really want to delete the project? The deletion cannot be undone");
    hdnProject.value = result ? 1 : 0;
}

function SetTooltipsForallDropDowns() {
    var optionList = document.getElementsByTagName('option');

    for (var i = 0; i < optionList.length; ++i) {

        optionList[i].title = optionList[i].text;
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
    SetTooltipsForallDropDowns();
    $("#tblProjectAttachments").tablesorter(
    {
        sortList: [[0, 0]]
    });
    $("#tblMilestones").tablesorter({
        headers: {
            0: {
                sorter: false
            }
        },
        sortList: [[1, 0]],
        sortForce: [[1, 0]]
    }).bind("sortEnd", function (sorter) {
        currentSort = sorter.target.config.sortList;
        var spanName = $("#tblMilestones #name");
        if (currentSort != '1,0' && currentSort != '1,1') {
            spanName[0].setAttribute('class', 'backGroundNone');
        }
        else {
            spanName[0].setAttribute('class', '');
        }
    });

    $("#tblBudgetManagementResource").tablesorter({
        widgets: ['zebra', 'staticRow']
    });
    $('#tblBudgetManagementResource th:first').click();
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

//number = 1 to show, 2 to close
function showProjectNumberTextBox(number) {
    var projectNumber = document.getElementById(tblSetProjectNumberId);
    var HiddenField = document.getElementById(hdnVisibiltyId);
    if (number == 1) {
        projectNumber.rows[0].cells[1].children[0].setAttribute('style', 'display:inline;');
        HiddenField.value = 'inline';
    }
    else {
        projectNumber.rows[0].cells[1].children[0].setAttribute('style', 'display:none;');
        HiddenField.value = 'none';
    }
}

//region project time types script

function btnClose_OnClientClick() {
    $find("mpeTimetypeAlertMessage").hide();
    return false;
}

function DeleteWorkType(timetypeid) {

    if (confirm("Are you sure you want to delete this Work Type?")) {
        var btnDeleteWorkType = document.getElementById(btnDeleteWorkTypeId);
        var hdnWorkTypeId = document.getElementById(hdnWorkTypeIdId);
        hdnWorkTypeId.value = timetypeid;
        btnDeleteWorkType.click();
    }

    return false;
}

// End Region projecttimetypes script

$(document).ready(function () {
    SetTooltipsForallDropDowns();
    $('script #tableSorterScript').load(function () {
        $("#tblProjectAttachments").tablesorter(
{
    sortList: [[0, 0]]
}
);
    });
});

function mailTo(url) {

    var mailtoHiddenLink = document.getElementById('mailtoHiddenLink');
    mailtoHiddenLink.href = url;
    mailtoHiddenLink.click();
    return false;
}


function RedirectToProject() {
    var imgRedirectLink = document.getElementById(imgNavigateToProjectId);
    var url = imgRedirectLink.getAttribute("NavigateUrl");

    window.open(url);
}

