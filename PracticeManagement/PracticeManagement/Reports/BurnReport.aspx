<%@ Page Title="Burn Report" Language="C#" MasterPageFile="~/PracticeManagementMain.Master" AutoEventWireup="true" CodeBehind="BurnReport.aspx.cs" Inherits="PraticeManagement.Reports.BurnReport" %>

<%@ Register TagPrefix="uc" TagName="LoadingProgress" Src="~/Controls/Generic/LoadingProgress.ascx" %>
<%@ Register Src="~/Controls/MessageLabel.ascx" TagName="MessageLabel" TagPrefix="uc" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajaxToolkit" %>
<%@ Register Src="~/Controls/Generic/Filtering/DateInterval.ascx" TagPrefix="uc"
    TagName="DateInterval" %>
<asp:Content ID="Content1" ContentPlaceHolderID="title" runat="server">
    <title>Burn Report | Practice Management</title>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="head" runat="server">
    <script type="text/javascript" src="https://cdnjs.cloudflare.com/ajax/libs/bluebird/3.3.5/bluebird.min.js"></script>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="header" runat="server">
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="body" runat="server">
    <script src="../Scripts/jquery-1.4.1.yui.js" type="text/javascript"></script>
    <script src="../Scripts/jquery.tablesorter.min.js" type="text/javascript"></script>
    <script src="../Scripts/jquery.tablesorter.staticrow.min.js" type="text/javascript"></script>
    <script src="../Scripts/html2canvas.min.js" type="text/javascript"></script>
    <script type="text/javascript" src="http://cdn.rawgit.com/niklasvh/html2canvas/master/dist/html2canvas.min.js"></script>
    <script type="text/javascript">

        function CheckAndShowCustomDatesPoup(ddlPeriod) {
            imgCalender = document.getElementById('<%= imgCalender.ClientID %>');
            lblCustomDateRange = document.getElementById('<%= lblCustomDateRange.ClientID %>');
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
                var ddlDetalization = document.getElementById('<%= ddlDetalization.ClientID %>');
                ddlDetalization.options.length = 0;
                var option1 = document.createElement("option");
                option1.text = '1 Day';
                option1.value = '1';
                ddlDetalization.options.add(option1);
                var option2 = document.createElement("option");
                option2.text = '1 Week';
                option2.value = '7';
                option2.selected = 'selected';
                ddlDetalization.options.add(option2);
                var option3 = document.createElement("option");
                option3.text = '1 Month';
                option3.value = '30';
                ddlDetalization.options.add(option3);
            }
        }
        function ReAssignStartDateEndDates() {
            hdnStartDate = document.getElementById('<%= hdnStartDate.ClientID %>');
            hdnEndDate = document.getElementById('<%= hdnEndDate.ClientID %>');
            txtStartDate = document.getElementById(hdnStartDateTxtBoxId.value);
            txtEndDate = document.getElementById(hdnEndDateTxtBoxId.value);
            hdnStartDateCalExtenderBehaviourId = document.getElementById('<%= hdnStartDateCalExtenderBehaviourId.ClientID %>');
            hdnEndDateCalExtenderBehaviourId = document.getElementById('<%= hdnEndDateCalExtenderBehaviourId.ClientID %>');

            var endDateCalExtender = $find(hdnEndDateCalExtenderBehaviourId.value);
            var startDateCalExtender = $find(hdnStartDateCalExtenderBehaviourId.value);
            if (startDateCalExtender != null) {
                startDateCalExtender.set_selectedDate(hdnStartDate.value);
            }
            if (endDateCalExtender != null) {
                endDateCalExtender.set_selectedDate(hdnEndDate.value);
            }
            btnCustDatesOK = document.getElementById('<%= btnCustDatesOK.ClientID %>');
            btnCustDatesOK.click();
        }

        function CheckIfDatesValid() {
            hdnStartDateTxtBoxId = document.getElementById('<%= hdnStartDateTxtBoxId.ClientID %>');
            hdnEndDateTxtBoxId = document.getElementById('<%= hdnEndDateTxtBoxId.ClientID %>');
            txtStartDate = document.getElementById(hdnStartDateTxtBoxId.value);
            txtEndDate = document.getElementById(hdnEndDateTxtBoxId.value);
            var startDate = new Date(txtStartDate.value);
            var endDate = new Date(txtEndDate.value);
            if (txtStartDate.value != '' && txtEndDate.value != ''
            && startDate <= endDate) {
                var btnCustDatesClose = document.getElementById('<%= btnCustDatesClose.ClientID %>');
                hdnStartDate = document.getElementById('<%= hdnStartDate.ClientID %>');
                hdnEndDate = document.getElementById('<%= hdnEndDate.ClientID %>');
                lblCustomDateRange = document.getElementById('<%= lblCustomDateRange.ClientID %>');
                var startDate = new Date(txtStartDate.value);
                var endDate = new Date(txtEndDate.value);
                var startDateStr = startDate.format("MM/dd/yyyy");
                var endDateStr = endDate.format("MM/dd/yyyy");
                hdnStartDate.value = startDateStr;
                hdnEndDate.value = endDateStr;
                lblCustomDateRange.innerHTML = '(' + startDateStr + '&nbsp;-&nbsp;' + endDateStr + ')';
                var oneDay = 24 * 60 * 60 * 1000;
                var diffDays = Math.round(Math.abs((startDate.getTime() - endDate.getTime()) / (oneDay)));

                var ddlDetalization = document.getElementById('<%= ddlDetalization.ClientID %>');
                if (diffDays < 7) {
                    ddlDetalization.options.length = 0;
                    var option = document.createElement("option");
                    option.text = '1 Day';
                    option.value = '1';
                    ddlDetalization.options.add(option);
                }

                else if (diffDays < 30) {
                    ddlDetalization.options.length = 0;
                    var option1 = document.createElement("option");
                    option1.text = '1 Day';
                    option1.value = '1';
                    ddlDetalization.options.add(option1);
                    var option2 = document.createElement("option");
                    option2.text = '1 Week';
                    option2.value = '7';
                    option2.selected = 'selected';
                    ddlDetalization.options.add(option2);
                }
                else {
                    ddlDetalization.options.length = 0;
                    var option1 = document.createElement("option");
                    option1.text = '1 Day';
                    option1.value = '1';
                    ddlDetalization.options.add(option1);
                    var option2 = document.createElement("option");
                    option2.text = '1 Week';
                    option2.value = '7';
                    option2.selected = 'selected';
                    ddlDetalization.options.add(option2);
                    var option3 = document.createElement("option");
                    option3.text = '1 Month';
                    option3.value = '30';
                    ddlDetalization.options.add(option3);
                }

                btnCustDatesClose.click();
            }
            return false;
        }

        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(endRequestHandle);

        function endRequestHandle(sender, Args) {

            $("#tblResources").tablesorter({
                widgets: ['zebra', 'staticRow']
            });

            $("#tblExpenses").tablesorter({
                widgets: ['zebra', 'staticRow']
            });
            $('#tblResources th:first').click();
            $('#tblExpenses th:first').click();

            imgCalender = document.getElementById('<%= imgCalender.ClientID %>');
            lblCustomDateRange = document.getElementById('<%= lblCustomDateRange.ClientID %>');
            ddlPeriod = document.getElementById('<%=  ddlPeriod.ClientID %>');
            if (imgCalender.fireEvent && ddlPeriod.value != '0') {
                imgCalender.style.display = "none";
                lblCustomDateRange.style.display = "none";
            }
        }

        var currentSort = [[1, 0]];

        $(document).ready(function () {
            SetColumnWidth()

            var limit = 2;
            $('.single-checkbox').live('click', function (evt) {
                if ($('.single-checkbox input:checked').length > limit) {
                    return false;
                }
                ShowColumns();

            });

            $("#tblResources").tablesorter({
                widgets: ['zebra', 'staticRow']
            });

            $("#tblExpenses").tablesorter({
                widgets: ['zebra', 'staticRow']
            });

            $('#tblResources th:first').click();
            $('#tblExpenses th:first').click();
        });

        function myPrePostbackFunction() {
            SetColumnWidth();
            html2canvas($("#testPDF")[0]).then(function (canvas) {
                var base64 = canvas.toDataURL();
                $("[id$='hdnPdfData']").val(base64);
            });

        }

        function SetColumnWidth() {
            var expWidth = $(".ExpenseName").width();
            var resWidth = $(".resourceName").width();
            $(".ExpenseName").width(expWidth < resWidth ? expWidth : resWidth);
            $(".resourceName").width(expWidth < resWidth ? expWidth : resWidth);
        }

        function ShowColumns() {
            var chbGridBudgetChecked = $('#ctl00_body_chbGridBudget').attr("checked");
            var chbGridActualsChecked = $('#ctl00_body_chbGridActuals').attr("checked");
            var chbGridProjectedChecked = $('#ctl00_body_chbGridProjected').attr("checked");
            var chbGridEACChecked = $('#ctl00_body_chbGridEAC').attr("checked");
            var rbnHoursChecked = $('#ctl00_body_rbnHours').attr("checked");
            var rbnRevenueChecked = $('#ctl00_body_rbnRevenue').attr("checked");

            if (chbGridBudgetChecked) {
                if (rbnHoursChecked) {
                    $(".BudgetHrs").css("display", "");
                    $(".BudgetRev").css("display", "none");
                    $(".BudgetMargin").css("display", "none");
                }
                else if (rbnRevenueChecked) {
                    $(".BudgetHrs").css("display", "none");
                    $(".BudgetRev").css("display", "");
                    $(".BudgetMargin").css("display", "none");
                }
                else {
                    $(".BudgetHrs").css("display", "none");
                    $(".BudgetRev").css("display", "none");
                    $(".BudgetMargin").css("display", "");
                }
                $(".BudgetExpense").css("display", "");
            }
            else {
                $(".BudgetHrs").css("display", "none");
                $(".BudgetRev").css("display", "none");
                $(".BudgetMargin").css("display", "none");
                $(".BudgetExpense").css("display", "none");
            }

            if (chbGridActualsChecked) {
                if (rbnHoursChecked) {
                    $(".ActualHrs").css("display", "");
                    $(".ActualRev").css("display", "none");
                    $(".ActualMargin").css("display", "none");
                }
                else if (rbnRevenueChecked) {
                    $(".ActualHrs").css("display", "none");
                    $(".ActualRev").css("display", "");
                    $(".ActualMargin").css("display", "none");
                }
                else {
                    $(".ActualHrs").css("display", "none");
                    $(".ActualRev").css("display", "none");
                    $(".ActualMargin").css("display", "");
                }
                $(".ActualsExpense").css("display", "");
            }
            else {
                $(".ActualHrs").css("display", "none");
                $(".ActualRev").css("display", "none");
                $(".ActualMargin").css("display", "none");
                $(".ActualsExpense").css("display", "none");
            }

            if (chbGridProjectedChecked) {
                if (rbnHoursChecked) {
                    $(".ProjectedHrs").css("display", "");
                    $(".ProjectedRev").css("display", "none");
                    $(".ProjectedMargin").css("display", "none");
                }
                else if (rbnRevenueChecked) {
                    $(".ProjectedHrs").css("display", "none");
                    $(".ProjectedRev").css("display", "");
                    $(".ProjectedMargin").css("display", "none");
                }
                else {
                    $(".ProjectedHrs").css("display", "none");
                    $(".ProjectedRev").css("display", "none");
                    $(".ProjectedMargin").css("display", "");
                }
                $(".ProjExpense").css("display", "");
            }
            else {
                $(".ProjectedHrs").css("display", "none");
                $(".ProjectedRev").css("display", "none");
                $(".ProjectedMargin").css("display", "none");
                $(".ProjExpense").css("display", "none");
            }

            if (chbGridEACChecked) {
                if (rbnHoursChecked) {
                    $(".EACHrs").css("display", "");
                    $(".EACRev").css("display", "none");
                    $(".EACMargin").css("display", "none");
                }
                else if (rbnRevenueChecked) {
                    $(".EACHrs").css("display", "none");
                    $(".EACRev").css("display", "");
                    $(".EACMargin").css("display", "none");
                }
                else {
                    $(".EACHrs").css("display", "none");
                    $(".EACRev").css("display", "none");
                    $(".EACMargin").css("display", "");
                }
                $(".EACExpense").css("display", "");
            }
            else {
                $(".EACHrs").css("display", "none");
                $(".EACRev").css("display", "none");
                $(".EACMargin").css("display", "none");
                $(".EACExpense").css("display", "none");
            }
            FindDifference();
        }
        function FindDifference() {
            var chbGridBudgetChecked = $('#ctl00_body_chbGridBudget').attr("checked");
            var chbGridProjectedChecked = $('#ctl00_body_chbGridProjected').attr("checked");
            var chbGridEACChecked = $('#ctl00_body_chbGridEAC').attr("checked");
            var chbGridActualsChecked = $('#ctl00_body_chbGridActuals').attr("checked");

            var rbnHoursChecked = $('#ctl00_body_rbnHours').attr("checked");
            var rbnRevenueChecked = $('#ctl00_body_rbnRevenue').attr("checked");

            var firstTdClass = "";
            var secondTdClass = "";
            var expenseFirstTd = "";
            var expenseSecondTd = "";
            if (chbGridBudgetChecked && chbGridProjectedChecked) {
                if (rbnHoursChecked) {
                    firstTdClass = ".BudgetHrs";
                    secondTdClass = ".ProjectedHrs";
                }
                else if (rbnRevenueChecked) {
                    firstTdClass = ".BudgetRev";
                    secondTdClass = ".ProjectedRev";
                }
                else {
                    firstTdClass = ".BudgetMargin";
                    secondTdClass = ".ProjectedMargin";
                }
                expenseFirstTd = ".BudgetExpense";
                expenseSecondTd = ".ProjExpense";
            }
            if (chbGridBudgetChecked && chbGridEACChecked) {
                if (rbnHoursChecked) {
                    firstTdClass = ".BudgetHrs";
                    secondTdClass = ".EACHrs";
                }
                else if (rbnRevenueChecked) {
                    firstTdClass = ".BudgetRev";
                    secondTdClass = ".EACRev";
                }
                else {
                    firstTdClass = ".BudgetMargin";
                    secondTdClass = ".EACMargin";
                }
                expenseFirstTd = ".BudgetExpense";
                expenseSecondTd = ".EACExpense";
            }
            if (chbGridBudgetChecked && chbGridActualsChecked) {
                if (rbnHoursChecked) {
                    firstTdClass = ".BudgetHrs";
                    secondTdClass = ".ActualHrs";
                }
                else if (rbnRevenueChecked) {
                    firstTdClass = ".BudgetRev";
                    secondTdClass = ".ActualRev";
                }
                else {
                    firstTdClass = ".BudgetMargin";
                    secondTdClass = ".ActualMargin";
                }
                expenseFirstTd = ".BudgetExpense";
                expenseSecondTd = ".ActualsExpense";
            }
            if (chbGridProjectedChecked && chbGridEACChecked) {
                if (rbnHoursChecked) {
                    firstTdClass = ".ProjectedHrs";
                    secondTdClass = ".EACHrs";
                }
                else if (rbnRevenueChecked) {
                    firstTdClass = ".ProjectedRev";
                    secondTdClass = ".EACRev";
                }
                else {
                    firstTdClass = ".ProjectedMargin";
                    secondTdClass = ".EACMargin";
                }
                expenseFirstTd = ".ProjExpense";
                expenseSecondTd = ".EACExpense";
            }
            if (chbGridProjectedChecked && chbGridActualsChecked) {
                if (rbnHoursChecked) {
                    firstTdClass = ".ProjectedHrs";
                    secondTdClass = ".ActualHrs";
                }
                else if (rbnRevenueChecked) {
                    firstTdClass = ".ProjectedRev";
                    secondTdClass = ".ActualRev";
                }
                else {
                    firstTdClass = ".ProjectedMargin";
                    secondTdClass = ".ActualMargin";
                }
                expenseFirstTd = ".ProjExpense";
                expenseSecondTd = ".ActualsExpense";
            }
            if (chbGridEACChecked && chbGridActualsChecked) {
                if (rbnHoursChecked) {
                    firstTdClass = ".EACHrs";
                    secondTdClass = ".ActualHrs";
                }
                else if (rbnRevenueChecked) {
                    firstTdClass = ".EACRev";
                    secondTdClass = ".ActualRev";
                }
                else {
                    firstTdClass = ".EACMargin";
                    secondTdClass = ".ActualMargin";
                }
                expenseFirstTd = ".EACExpense";
                expenseSecondTd = ".ActualsExpense";
            }



            $('.Resource tr').each(function () {
                var firstVal = 0;
                var secondVal = 0;
                var diff = 0;
                var diffPer = 0;
                var isDollar = false;
                firstVal = $(this).find(firstTdClass).text();
                secondVal = $(this).find(secondTdClass).text();
                isDollar = firstVal.indexOf('$') >= 0;
                firstVal = firstVal.replace('$', '');
                secondVal = secondVal.replace('$', '');
                firstVal = firstVal.replace(/,/g, '');
                secondVal = secondVal.replace(/,/g, '');
                firstVal = firstVal.replace(/\n/g, '');
                secondVal = secondVal.replace(/\n/g, '');
                firstVal = firstVal.replace(/ /g, '');
                secondVal = secondVal.replace(/ /g, '');
                if (firstVal != "(Hidden)" || secondVal != "(Hidden)") {

                    if (!isNaN(firstVal) && firstVal.length !== 0 && !isNaN(secondVal) && secondVal.length !== 0) {
                        diff = (parseFloat(firstVal) - parseFloat(secondVal)).toFixed(2);
                        diffPer = (parseFloat(firstVal) !== 0 ? diff * 100 / parseFloat(firstVal) : 0).toFixed(2);
                    }
                    if (isDollar) {
                        diff = parseFloat(diff).toFixed(0);
                        var _reqText = '$' + diff.replace(/(\d)(?=(\d{3})+\.)/g, "$1,").replace(/\B(?=(?:\d{3})+(?!\d))/g, ",");
                        if (_reqText.indexOf('-') >= 0) {
                            _reqText = '<span class="Bench">' + '(' + _reqText.replace('-', '') + ')' + '</span>';
                        }
                        $('.tdVariance', this).html(_reqText);
                    }
                    else {
                        var _reqText = diff.toString();
                        if (_reqText.indexOf('-') >= 0) {
                            _reqText = '<span class="Bench">' + '(' + _reqText.replace('-', '') + ')' + '</span>';
                        }
                        $('.tdVariance', this).html(_reqText);
                    }
                    var _reqPerText = diffPer.toString() + '%';
                    if (_reqPerText.indexOf('-') >= 0) {
                        _reqPerText = '<span class="Bench">' + '(' + _reqPerText.replace('-', '') + ')' + '</span>';
                    }
                    $('.tdVariancePer', this).html(_reqPerText);
                }
                else {
                    $('.tdVariance', this).html("(Hidden)");
                    $('.tdVariancePer', this).html("(Hidden)");
                }
                $('.tdVariance', this).attr('sorttable_customkey', diff);
                $('.tdVariancePer', this).attr('sorttable_customkey', diffPer);

            });

            $('.Expense tr').each(function () {
                var firstVal = 0;
                var secondVal = 0;
                var diff = 0;
                var diffPer = 0;

                firstVal = $(this).find(expenseFirstTd).text();
                secondVal = $(this).find(expenseSecondTd).text();

                firstVal = firstVal.replace('$', '');
                secondVal = secondVal.replace('$', '');
                firstVal = firstVal.replace(/,/g, '');
                secondVal = secondVal.replace(/,/g, '');
                firstVal = firstVal.replace(/\n/g, '');
                secondVal = secondVal.replace(/\n/g, '');
                firstVal = firstVal.replace(/ /g, '');
                secondVal = secondVal.replace(/ /g, '');


                if (!isNaN(firstVal) && firstVal.length !== 0 && !isNaN(secondVal) && secondVal.length !== 0) {
                    diff = (parseFloat(firstVal) - parseFloat(secondVal)).toFixed(2);
                    diffPer = (parseFloat(firstVal) !== 0 ? diff * 100 / parseFloat(firstVal) : 0).toFixed(2);
                }

                diff = parseFloat(diff).toFixed(0);
                var _reqText = '$' + diff.replace(/(\d)(?=(\d{3})+\.)/g, "$1,").replace(/\B(?=(?:\d{3})+(?!\d))/g, ",");
                if (_reqText.indexOf('-') >= 0) {
                    _reqText = '<span class="Bench">' + '(' + _reqText.replace('-', '') + ')' + '</span>';
                }
                $('.tdExpenseVariance', this).html(_reqText);

                var _reqPerText = diffPer.toString() + '%';

                if (_reqPerText.indexOf('-') >= 0) {
                    _reqPerText = '<span class="Bench">' + '(' + _reqPerText.replace('-', '') + ')' + '</span>';
                }
                $('.tdExpenseVariancePer', this).html(_reqPerText);

                $('.tdExpenseVariance', this).attr('sorttable_customkey', diff);
                $('.tdExpenseVariancePer', this).attr('sorttable_customkey', diffPer);
            })
        }

        window.onresize = function () {
            var width = $('#ctl00_body_divReport').width() / 2;
            $('#ctl00_body_activityChart').attr('style', 'width: ' + width + 'px; height:300px; border-width:0px;');
            SetColumnWidth();
        }

        function pageLoad() {
            var width = $('#ctl00_body_divReport').width() / 2;
            $('#ctl00_body_activityChart').attr('style', 'width: ' + width + 'px; height:300px; border-width:0px;');
            myPrePostbackFunction();
        }

    </script>

    <uc:LoadingProgress ID="LoadingProgress1" runat="server" Visible="true" />
    <asp:UpdatePanel ID="flrPanel" runat="server">
        <ContentTemplate>
            <div class="filters">
                <div class="buttons-block">
                    <table class="WholeWidth BorderNone LeftPadding10px no-wrap">
                        <tr>
                            <td>
                                <table>
                                    <tr>
                                        <td>Project Number: &nbsp;</td>
                                        <td class="textLeft">
                                            <asp:TextBox ID="txtProjectNumber" runat="server"></asp:TextBox>
                                            <AjaxControlToolkit:TextBoxWatermarkExtender ID="waterMarkTxtProjectNumber" runat="server"
                                                TargetControlID="txtProjectNumber" BehaviorID="waterMarkTxtProjectNumber" WatermarkCssClass="watermarkedtext"
                                                WatermarkText="Ex: P1234767">
                                            </AjaxControlToolkit:TextBoxWatermarkExtender>
                                        </td>
                                        <td>
                                            <asp:Image ID="imgProjectSearch" runat="server" ToolTip="Project Search" ImageUrl="~/Images/search_24.png" />
                                        </td>
                                    </tr>
                                </table>
                            </td>
                            <td>For &nbsp;
                               <asp:DropDownList ID="ddlPeriod" runat="server" AutoPostBack="false" onchange="CheckAndShowCustomDatesPoup(this);">
                                   <asp:ListItem Text="Entire Project" Value="1" Selected="True"></asp:ListItem>
                                   <asp:ListItem Text="Current Quarter " Value="-3"></asp:ListItem>
                                   <asp:ListItem Text="Next Quarter" Value="3"></asp:ListItem>
                                   <asp:ListItem Text="Current Half" Value="-6"></asp:ListItem>
                                   <asp:ListItem Text="Next Half" Value="6"></asp:ListItem>
                                   <asp:ListItem Text="Custom Dates" Value="0"></asp:ListItem>
                               </asp:DropDownList>
                                <ajaxToolkit:ModalPopupExtender ID="mpeCustomDates" runat="server" TargetControlID="imgCalender"
                                    CancelControlID="btnCustDatesCancel" OkControlID="btnCustDatesClose" BackgroundCssClass="modalBackground"
                                    PopupControlID="pnlCustomDates" BehaviorID="bhCustomDates" DropShadow="false"
                                    OnCancelScript="ReAssignStartDateEndDates();" OnOkScript="return false;" />
                                <asp:HiddenField ID="hdnStartDate" runat="server" Value="" />
                                <asp:HiddenField ID="hdnEndDate" runat="server" Value="" />
                                <asp:HiddenField ID="hdnStartDateTxtBoxId" runat="server" Value="" />
                                <asp:HiddenField ID="hdnEndDateTxtBoxId" runat="server" Value="" />
                                <asp:HiddenField ID="hdnStartDateCalExtenderBehaviourId" runat="server" Value="" />
                                <asp:HiddenField ID="hdnEndDateCalExtenderBehaviourId" runat="server" Value="" />
                                &nbsp;
                      
                            </td>
                            <td>
                                <asp:Label ID="lblCustomDateRange" runat="server" Text=""></asp:Label>
                                <asp:Image ID="imgCalender" runat="server" ImageUrl="~/Images/calendar.gif" />
                            </td>

                            <td>By:
                               <asp:DropDownList ID="ddlDetalization" runat="server" CssClass="Width75PxImp" AutoPostBack="false">
                                   <asp:ListItem Value="1">1 Day</asp:ListItem>
                                   <asp:ListItem Selected="True" Value="7">1 Week</asp:ListItem>
                                   <asp:ListItem Value="30">1 Month</asp:ListItem>
                               </asp:DropDownList>
                            </td>
                            <td>Actuals
                                 <asp:DropDownList ID="ddlActualPeriod" runat="server" AutoPostBack="false">
                                     <asp:ListItem Selected="True" Value="1">Today</asp:ListItem>
                                     <asp:ListItem Value="15">Last Pay Period</asp:ListItem>
                                     <asp:ListItem Value="30">Prior Month End</asp:ListItem>
                                     <asp:ListItem Value="0">All</asp:ListItem>
                                 </asp:DropDownList>
                            </td>
                            <td class="Width35Percent padLeft5"></td>
                            <td>
                                <asp:HiddenField ID="hdnPdfData" runat="server" />
                                <asp:Button ID="btnUpdateReport" OnClientClick="myPrePostbackFunction()" runat="server" Text="Refresh Report" OnClick="btnUpdateReport_Click" />
                            </td>
                            <td></td>
                            <td></td>
                        </tr>
                    </table>
                </div>
                <asp:Panel ID="pnlCustomDates" runat="server" CssClass="ConfirmBoxClass CustomDatesPopUp"
                    Style="display: none;">
                    <table class="WholeWidth">
                        <tr>
                            <td align="center" class="no-wrap">
                                <table>
                                    <tr>
                                        <td>
                                            <uc:DateInterval ID="diRange" runat="server" IsFromDateRequired="true" IsToDateRequired="true"
                                                FromToDateFieldCssClass="Width70Px" />
                                        </td>
                                    </tr>
                                </table>
                            </td>
                        </tr>
                        <tr>
                            <td class="custBtns">
                                <asp:Button ID="btnCustDatesOK" runat="server" OnClientClick="return CheckIfDatesValid();"
                                    ValidationGroup="<%# ClientID %>" Text="OK" CausesValidation="true" />
                                <asp:Button ID="btnCustDatesClose" runat="server" Style="display: none;" CausesValidation="true"
                                    OnClientClick="return false;" />
                                &nbsp; &nbsp;
                        <asp:Button ID="btnCustDatesCancel" runat="server" Text="Cancel" />
                            </td>
                        </tr>
                        <tr>
                            <td class="textCenter">
                                <asp:ValidationSummary ID="valSum" runat="server" />
                            </td>
                        </tr>
                    </table>
                </asp:Panel>
                <AjaxControlToolkit:ModalPopupExtender ID="mpeProjectSearch" runat="server" TargetControlID="imgProjectSearch"
                    BackgroundCssClass="modalBackground" PopupControlID="pnlProjectSearch" BehaviorID="mpeProjectSearch" CancelControlID="btnclose"
                    DropShadow="false" />
                <asp:Panel ID="pnlProjectSearch" runat="server" CssClass="popUp ProjectSearch" Style="display: none;">
                    <table class="WholeWidth">
                        <tr class="PopUpHeader">
                            <th>Project Search
                            <asp:Button ID="btnclose" runat="server" CssClass="mini-report-closeNew" ToolTip="Close"
                                OnClick="btnclose_Click" Text="X"></asp:Button>
                            </th>
                        </tr>
                        <tr>
                            <td class="WholeWidth">
                                <table class="WholeWidth">
                                    <tr>
                                        <td class="Width100Px textRight">Account:
                                        </td>
                                        <td>
                                            <asp:DropDownList ID="ddlClients" runat="server" CssClass="Width250Px" OnSelectedIndexChanged="ddlClients_SelectedIndexChanged"
                                                AutoPostBack="true">
                                            </asp:DropDownList>
                                        </td>
                                    </tr>
                                </table>
                            </td>
                        </tr>
                        <tr>
                            <td class="WholeWidth">
                                <table class="WholeWidth">
                                    <tr>
                                        <td class="Width100Px textRight">Project:
                                        </td>
                                        <td>

                                            <asp:DropDownList ID="ddlProjects" runat="server" Enabled="false" AutoPostBack="true"
                                                CssClass="Width250Px" OnSelectedIndexChanged="ddlProjects_SelectedIndexChanged">
                                                <asp:ListItem Text="-- Select a Project --" Value="">
                                                </asp:ListItem>
                                            </asp:DropDownList>
                                        </td>
                                    </tr>
                                </table>
                            </td>
                        </tr>
                        <tr>
                            <td class="padLeft8">
                                <asp:Literal ID="ltrlNoProjectsText" Visible="false" runat="server" Text="No Projects found."></asp:Literal>
                            </td>
                        </tr>
                    </table>
                </asp:Panel>
            </div>
            <div>
                <uc:MessageLabel ID="msgError" runat="server" ErrorColor="Red" InfoColor="Green"
                    WarningColor="Orange" EnableViewState="false" />
            </div>
        </ContentTemplate>

    </asp:UpdatePanel>
    <hr />
    <asp:UpdatePanel ID="updConsReport" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="false">
        <ContentTemplate>
            <div id="divProjectInfo" runat="server" visible="false">
                <table class="WholeWidth">
                    <tr>
                        <td class="font14Px fontBold">
                            <asp:Label ID="lblProjectName" runat="server"></asp:Label><br />
                            <asp:Label ID="lblDateRange" runat="server"></asp:Label>
                        </td>
                        <td style="float: right; margin-right: 15px;">
                            <asp:UpdatePanel ID="updButtons" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="false">
                                <ContentTemplate>
                                    Export: &nbsp;
                                    <asp:Button ID="btnExport" runat="server" Text="Excel" OnClick="btnExport_Click" />
                                    <asp:Button ID="btnLimitedExport" runat="server" Text="Excel" OnClick="btnLimitedExport_Click" />
                                    <asp:Button ID="btnPdf" runat="server" Text="PDF" OnClick="btnPdf_Click" />
                                    <asp:HiddenField ID="hdnExcel" runat="server" />
                                    <AjaxControlToolkit:ModalPopupExtender ID="mpeExcel" runat="server"
                                        TargetControlID="hdnExcel" CancelControlID="btnCloseExcel" BehaviorID="mpeTEsRelatedToItPopup"
                                        BackgroundCssClass="modalBackground" PopupControlID="pnlSubmitExcel" DropShadow="false" />
                                    <asp:Panel ID="pnlSubmitExcel" runat="server" CssClass="PanelPerson ConfirmBoxClassError"
                                        Style="display: none">
                                        <table class="WholeWidth">
                                            <tr>
                                                <th align="center" class="TextAlignCenter BackGroundColorGray vBottom">
                                                    <b class="BtnClose">Excel Warning</b>
                                                </th>
                                            </tr>
                                            <tr>
                                                <td class="Padding10Px">Burn Report Detail export exceeds 256 columns.  Results may be incomplete.
                                                </td>
                                            </tr>

                                            <tr>
                                                <td class="TextAlignCenter">
                                                    <asp:Button ID="btnExcelSubmit" runat="server" Text="OK" ToolTip="Submit" OnClick="btnExcelSubmit_Click" />
                                                    <asp:Button ID="btnCloseExcel" runat="server" Text="Cancel" ToolTip="Cancel" OnClientClick="return false;" />
                                                </td>
                                            </tr>
                                        </table>
                                    </asp:Panel>
                                </ContentTemplate>
                                <Triggers>
                                    <asp:PostBackTrigger ControlID="btnExport" />
                                    <asp:PostBackTrigger ControlID="btnPdf" />
                                    <asp:PostBackTrigger ControlID="btnExcelSubmit" />
                                </Triggers>
                            </asp:UpdatePanel>


                        </td>
                    </tr>
                </table>

            </div>

            <div id="divReport" runat="server" class="WholeWidth">
                <br />
                <table class="WholeWidth">
                    <tr>
                        <td class="Width50Percent">
                            <div class="tab-pane Height40Px no-wrap">
                                <table>
                                    <tr>
                                        <td>Graph View:</td>
                                        <td>
                                            <asp:CheckBox ID="chbBudget" runat="server" Text="Budget" Checked="true" AutoPostBack="true" OnCheckedChanged="chbBudget_CheckedChanged" /></td>
                                        <td>
                                            <asp:CheckBox ID="chbActuals" runat="server" Text="Actuals" Checked="true" AutoPostBack="true" OnCheckedChanged="chbBudget_CheckedChanged" /></td>
                                        <td>
                                            <asp:CheckBox ID="chbProjected" runat="server" Text="Projected" Checked="true" AutoPostBack="true" OnCheckedChanged="chbBudget_CheckedChanged" /></td>

                                    </tr>
                                </table>
                            </div>
                        </td>
                        <td class="Width50Percent">
                            <div class="tab-pane Height40Px">
                                <table class="WholeWidth no-wrap">
                                    <tr>
                                        <td class="Width10Per">Grid View:</td>
                                        <td class="Width10Per">
                                            <asp:RadioButton ID="rbnHours" runat="server" Checked="true" Text="Hours" GroupName="rbnGrid" OnClick="ShowColumns()" /></td>
                                        <td class="Width10Per">
                                            <asp:RadioButton ID="rbnRevenue" runat="server" Text="Revenue" GroupName="rbnGrid" OnClick="ShowColumns()" /></td>
                                        <td class="Width10Per">
                                            <asp:RadioButton ID="rbnMargin" runat="server" Text="Margin" GroupName="rbnGrid" OnClick="ShowColumns()" /></td>
                                        <td></td>
                                        <td class="floatright">
                                            <asp:HiddenField ID="hdnProjectNumber" runat="server" />
                                            <asp:HyperLink ID="lnkProject" runat="server" Text="Resource Details"
                                                Target="_blank">
                                            </asp:HyperLink>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td></td>
                                        <td>
                                            <asp:CheckBox ID="chbGridBudget" class="single-checkbox" runat="server" Checked="true" Text="Budget" /></td>

                                        <td>
                                            <asp:CheckBox ID="chbGridProjected" class="single-checkbox" runat="server" Text="Projected" /></td>
                                        <td>
                                            <asp:CheckBox ID="chbGridEAC" class="single-checkbox" runat="server" Text="ETC" /></td>
                                        <td>
                                            <asp:CheckBox ID="chbGridActuals" class="single-checkbox" runat="server" Checked="true" Text="Actuals" /></td>
                                        <td class="floatright">
                                            <asp:LinkButton ID="lnkViewAllResources" runat="server" Text="View All" CausesValidation="false" OnClick="lnkViewAllResources_Click"></asp:LinkButton>
                                        </td>
                                    </tr>
                                </table>
                            </div>

                        </td>
                    </tr>
                    <tr>
                        <td class="Width50Percent">
                            <asp:UpdatePanel ID="uplGraph" runat="server" UpdateMode="Conditional">
                                <ContentTemplate>

                                    <div id="divGraphReport">
                                        <br />
                                        <div>
                                            <asp:Chart ID="activityChart" runat="server" Width="900px" Height="300px">
                                                <Legends>
                                                    <asp:Legend Alignment="Center" Docking="Bottom" Name="MainLegend">
                                                    </asp:Legend>

                                                </Legends>
                                                <Series>
                                                    <asp:Series Name="Budget Revenue" BorderWidth="2" ChartType="Spline" BorderDashStyle="Solid"
                                                        Legend="MainLegend" Color="Blue" ChartArea="MainArea">
                                                    </asp:Series>
                                                    <asp:Series Name="Actual Revenue" BorderWidth="2" ChartType="Spline" BorderDashStyle="Solid"
                                                        Legend="MainLegend" Color="Green" ChartArea="MainArea">
                                                    </asp:Series>
                                                    <asp:Series Name="Projected Revenue" BorderWidth="2" ChartType="Spline" BorderDashStyle="Dash"
                                                        Legend="MainLegend" Color="Green" ChartArea="MainArea">
                                                    </asp:Series>
                                                    <asp:Series Name="Budget Amount" BorderWidth="1" ChartType="Line" BorderDashStyle="Solid"
                                                        Legend="MainLegend" Color="Gold" ChartArea="MainArea">
                                                    </asp:Series>
                                                </Series>
                                                <ChartAreas>
                                                    <asp:ChartArea Name="MainArea">
                                                        <AxisY Title="Revenue ($)">
                                                            <MajorGrid LineColor="Gray" LineDashStyle="Solid" />
                                                        </AxisY>
                                                        <AxisX Title="Date">
                                                            <MajorGrid LineColor="DarkGray" LineDashStyle="NotSet" IntervalType="Days" />
                                                            <LabelStyle Angle="60" />
                                                        </AxisX>
                                                    </asp:ChartArea>
                                                </ChartAreas>
                                            </asp:Chart>

                                        </div>
                                        <br />
                                        <div id="testPDF" class="PadLeft5Per" style="display: grid; width: 90%">
                                            <div style="width: 100%; overflow-x: auto;">

                                                <div style="display: inline-table; border-collapse: separate; border-spacing: 5px;">
                                                    <div class="RevenueTile" style="display: table-cell">
                                                        <label class="font16Px no-wrap">Budget Amount</label>
                                                        <br />
                                                        <br />
                                                        <asp:Label CssClass="ft-18px no-wrap" ID="lblBudgteAmount" runat="server"></asp:Label>
                                                        <br />
                                                        <asp:Label CssClass="font14Px no-wrap" ID="lblActualRevenue" runat="server"></asp:Label>
                                                        <br />
                                                        <br />
                                                        <div style="border: 1px solid gray; background-color: white" class="Width90Percent">
                                                            <div id="pgrBudget" runat="server" style="height: 12px; background-color: #7baaf7"></div>
                                                        </div>
                                                        <asp:Label ID="lblBudgetPer" runat="server"></asp:Label>
                                                    </div>
                                                    <div class="RevenueTile" style="display: table-cell">
                                                        <label class="font16Px">ETC($)</label>
                                                        <br />
                                                        <br />
                                                        <asp:Label CssClass="ft-18px no-wrap" ID="lblEACRevenue" runat="server"></asp:Label>
                                                        <br />

                                                        <asp:Label CssClass="font14Px no-wrap" ID="lblEACActualRevenue" runat="server"></asp:Label>
                                                        <br />
                                                        <br />
                                                        <div style="border: 1px solid gray; background-color: white" class="Width90Percent">
                                                            <div id="pgrEACAmount" runat="server" style="height: 12px; background-color: #7baaf7"></div>
                                                        </div>
                                                        <asp:Label ID="lblEACAmountPer" runat="server"></asp:Label>
                                                    </div>
                                                    <div class="RevenueTile" style="display: table-cell">
                                                        <label class="font16Px">ETC(hrs)</label>
                                                        <br />
                                                        <br />
                                                        <asp:Label CssClass="ft-18px no-wrap" ID="lblEACHrs" runat="server"></asp:Label>
                                                        <br />

                                                        <asp:Label CssClass="font14Px no-wrap" ID="lblActualHrs" runat="server"></asp:Label>
                                                        <br />
                                                        <br />
                                                        <div style="border: 1px solid gray; background-color: white" class="Width90Percent">
                                                            <div id="pgrEACHrs" runat="server" style="height: 12px; background-color: #7baaf7"></div>
                                                        </div>
                                                        <asp:Label ID="lblEACHrsPer" runat="server"></asp:Label>
                                                    </div>
                                                    <div id="divMargin" runat="server" class="RevenueTile" style="display: table-cell">
                                                        <label class="font16Px">Margin</label>
                                                        <br />
                                                        <br />
                                                        <asp:Label CssClass="ft-18px" ID="lblMarginPer" runat="server"></asp:Label>
                                                        <br />

                                                        <asp:Label CssClass="font14Px" ID="lblMarginAmount" runat="server"></asp:Label>
                                                        <br />
                                                        <br />
                                                        <asp:Label ID="lblMarginGoal" runat="server"></asp:Label>
                                                    </div>
                                                </div>

                                            </div>
                                            <div style="clear: both;" class="ProjectTimeTile">
                                                <label class="font16Px">Project Elapsed Time</label>
                                                <br />
                                                <br />
                                                <table>
                                                    <tr>
                                                        <td style="width: 80%;">
                                                            <div style="border: 1px solid gray; background-color: white" class="Width95Per">
                                                                <div id="pgrHrs" runat="server" style="height: 15px; background-color: #7baaf7"></div>
                                                            </div>
                                                        </td>
                                                        <td>
                                                            <asp:Label class="font16Px" ID="lblRemainingdays" runat="server"></asp:Label>
                                                        </td>
                                                    </tr>
                                                </table>
                                            </div>
                                        </div>
                                    </div>

                                </ContentTemplate>
                            </asp:UpdatePanel>
                        </td>
                        <td class="Width50Percent vTop">
                            <asp:UpdatePanel ID="uplGrid" runat="server" UpdateMode="Conditional">
                                <ContentTemplate>
                                    <div id="divGridReport" runat="server">
                                        <br />
                                        <div style="max-height: 210px; overflow: auto;">
                                            <asp:Repeater ID="repResources" runat="server" OnItemDataBound="repResources_ItemDataBound">
                                                <HeaderTemplate>
                                                    <div>
                                                        <table id="tblResources" class="gvStrawmen CompPerfTable Resource">
                                                            <thead>
                                                                <tr class="CursorPointer">
                                                                    <th class="ie-bg BorderRightC5C5C5 TextAlignLeftImp LeftPadding10px resourceName Width40P">Resource
                                                                    </th>
                                                                    <th class="ie-bg BorderRightC5C5C5 BudgetHrs Width15Per">Budget
                                                                    </th>
                                                                    <th class="ie-bg BorderRightC5C5C5 BudgetRev Width15Per">Budget
                                                                    </th>
                                                                    <th class="ie-bg BorderRightC5C5C5 BudgetMargin Width15Per">Budget
                                                                    </th>
                                                                    <th class="ie-bg BorderRightC5C5C5 ProjectedHrs Width15Per">Projected
                                                                    </th>
                                                                    <th class="ie-bg BorderRightC5C5C5 ProjectedRev Width15Per">Projected
                                                                    </th>
                                                                    <th class="ie-bg BorderRightC5C5C5 ProjectedMargin Width15Per">Projected
                                                                    </th>
                                                                    <th class="ie-bg BorderRightC5C5C5 EACHrs Width15Per">ETC
                                                                    </th>
                                                                    <th class="ie-bg BorderRightC5C5C5 EACRev Width15Per">ETC
                                                                    </th>
                                                                    <th class="ie-bg BorderRightC5C5C5 EACMargin Width15Per">ETC
                                                                    </th>
                                                                    <th class="ie-bg BorderRightC5C5C5 ActualHrs Width15Per">Actual
                                                                    </th>
                                                                    <th class="ie-bg BorderRightC5C5C5 ActualRev Width15Per">Actual
                                                                    </th>
                                                                    <th class="ie-bg BorderRightC5C5C5 ActualMargin Width15Per">Actual
                                                                    </th>
                                                                    <th class="ie-bg BorderRightC5C5C5 Width15Per">Variance
                                                                    </th>
                                                                    <th class="ie-bg NoBorder Width15Per">Variance %
                                                                    </th>
                                                                </tr>
                                                            </thead>
                                                            <tbody>
                                                </HeaderTemplate>
                                                <ItemTemplate>
                                                    <tr>
                                                        <td class="TextAlignLeftImp LeftPadding10px">
                                                            <%# HttpUtility.HtmlEncode((string)Eval("Person.LastName") + ", " + Eval("Person.FirstName")) %>
                                                        </td>
                                                        <td class="BudgetHrs" sorttable_customkey='<%# Eval("Budget.Hours")%>'>
                                                            <%# ((decimal)Eval("Budget.Hours")).ToString("###,###,##0.##") %>
                                                        </td>
                                                        <td class="BudgetRev" sorttable_customkey='<%# Eval("Budget.Revenue.Value")%>'>
                                                            <%# Eval("Budget.Revenue") %>
                                                        </td>
                                                        <td class="BudgetMargin" sorttable_customkey='<%# Eval("Budget.Margin.Value")%>'>
                                                            <asp:Label ID="lblBudMargin" runat="server"></asp:Label>
                                                        </td>
                                                        <td class="ProjectedHrs" sorttable_customkey='<%# Eval("Projected.Hours")%>'>
                                                            <%# ((decimal)Eval("Projected.Hours")).ToString("###,###,##0.##") %>
                                                        </td>
                                                        <td class="ProjectedRev" sorttable_customkey='<%# Eval("Projected.Revenue.Value")%>'>
                                                            <%# Eval("Projected.Revenue") %>
                                                        </td>
                                                        <td class="ProjectedMargin" sorttable_customkey='<%# Eval("Projected.Margin.Value")%>'>
                                                            <asp:Label ID="lblProjMargin" runat="server"></asp:Label>
                                                        </td>
                                                        <td class="EACHrs" sorttable_customkey='<%# Eval("EAC.Hours")%>'>
                                                            <%# ((decimal)Eval("EAC.Hours")).ToString("###,###,##0.##") %>
                                                        </td>
                                                        <td class="EACRev" sorttable_customkey='<%# Eval("EAC.Revenue.Value")%>'>
                                                            <%# Eval("EAC.Revenue") %>
                                                        </td>
                                                        <td class="EACMargin" sorttable_customkey='<%# Eval("EAC.Margin.Value")%>'>
                                                            <asp:Label ID="lblEACMargin" runat="server"></asp:Label>
                                                        </td>
                                                        <td class="ActualHrs" sorttable_customkey='<%# Eval("Actuals.Hours")%>'>
                                                            <%# ((decimal)Eval("Actuals.Hours")).ToString("###,###,##0.##") %>
                                                        </td>
                                                        <td class="ActualRev" sorttable_customkey='<%# Eval("Actuals.Revenue.Value")%>'>
                                                            <%# Eval("Actuals.Revenue") %>
                                                        </td>
                                                        <td class="ActualMargin" sorttable_customkey='<%# Eval("Actuals.Margin.Value")%>'>
                                                            <asp:Label ID="lblActMargin" runat="server"></asp:Label>
                                                        </td>


                                                        <td class="tdVariance"></td>
                                                        <td class="tdVariancePer"></td>

                                                    </tr>
                                                </ItemTemplate>
                                                <FooterTemplate>
                                                    <tr class="fontBold static ">
                                                        <td class="TextAlignLeftImp LeftPadding10px">Grand Total
                                                        </td>
                                                        <td class="BudgetHrs">
                                                            <asp:Label ID="lblBudgetHrs" runat="server"></asp:Label>
                                                        </td>
                                                        <td class="BudgetRev">
                                                            <asp:Label ID="lblBudgetRev" runat="server"></asp:Label>
                                                        </td>
                                                        <td class="BudgetMargin">
                                                            <asp:Label ID="lblBudgetMargin" runat="server"></asp:Label>
                                                        </td>
                                                        <td class="ProjectedHrs">
                                                            <asp:Label ID="lblProjHrs" runat="server"></asp:Label>
                                                        </td>
                                                        <td class="ProjectedRev">
                                                            <asp:Label ID="lblProjRev" runat="server"></asp:Label>
                                                        </td>
                                                        <td class="ProjectedMargin">
                                                            <asp:Label ID="lblProjMargin" runat="server"></asp:Label>
                                                        </td>
                                                        <td class="EACHrs">
                                                            <asp:Label ID="lblEACHrs" runat="server"></asp:Label>
                                                        </td>
                                                        <td class="EACRev">
                                                            <asp:Label ID="lblEACRev" runat="server"></asp:Label>
                                                        </td>
                                                        <td class="EACMargin">
                                                            <asp:Label ID="lblEACMargin" runat="server"></asp:Label>
                                                        </td>
                                                        <td class="ActualHrs">
                                                            <asp:Label ID="lblActualHrs" runat="server"></asp:Label>
                                                        </td>
                                                        <td class="ActualRev">
                                                            <asp:Label ID="lblActualRev" runat="server"></asp:Label>
                                                        </td>
                                                        <td class="ActualMargin">
                                                            <asp:Label ID="lblActualMargin" runat="server"></asp:Label>
                                                        </td>


                                                        <td class="tdVariance"></td>
                                                        <td class="tdVariancePer"></td>
                                                    </tr>
                                                    </tbody></table></div>
                                                </FooterTemplate>
                                            </asp:Repeater>
                                        </div>
                                        <br />
                                        <div style="max-height: 210px; overflow: auto;">
                                            <asp:Repeater ID="repExpenses" runat="server" OnItemDataBound="repExpenses_ItemDataBound">
                                                <HeaderTemplate>
                                                    <div>
                                                        <table id="tblExpenses" class="gvStrawmen CompPerfTable WholeWidth Expense">
                                                            <thead>
                                                                <tr class="CursorPointer">
                                                                    <th class="ie-bg BorderRightC5C5C5 TextAlignLeftImp LeftPadding10px ExpenseName Width40P">Expense
                                                                    </th>
                                                                    <th class="ie-bg BorderRightC5C5C5 BudgetExpense Width15Per">Budget
                                                                    </th>

                                                                    <th class="ie-bg BorderRightC5C5C5 ProjExpense Width15Per ">Projected
                                                                    </th>
                                                                    <th class="ie-bg BorderRightC5C5C5 EACExpense Width15Per">ETC
                                                                    </th>
                                                                    <th class="ie-bg BorderRightC5C5C5 ActualsExpense Width15Per">Actual
                                                                    </th>
                                                                    <th class="ie-bg BorderRightC5C5C5 Width15Per">Variance
                                                                    </th>
                                                                    <th class="ie-bg NoBorder Width15Per">Variance %
                                                                    </th>
                                                                </tr>
                                                            </thead>
                                                            <tbody>
                                                </HeaderTemplate>
                                                <ItemTemplate>
                                                    <tr>
                                                        <td class="TextAlignLeftImp LeftPadding10px">
                                                            <%#Eval("HtmlEncodedName") %>
                                                        </td>
                                                        <td class="BudgetExpense" sorttable_customkey='<%# Eval("BudgetAmount")%>'>
                                                            <%#((decimal)Eval("BudgetAmount")).ToString("$###,###,##0") %>
                                                        </td>

                                                        <td class="ProjExpense" sorttable_customkey='<%# Eval("ExpectedAmount")%>'>
                                                            <%#((decimal)Eval("ExpectedAmount")).ToString("$###,###,##0") %>
                                                        </td>
                                                        <td class="EACExpense" sorttable_customkey='<%# Eval("EACAmount")%>'>
                                                            <%#((decimal)Eval("EACAmount")).ToString("$###,###,##0") %>
                                                        </td>
                                                        <td class="ActualsExpense" sorttable_customkey='<%# Eval("Amount")%>'>
                                                            <%#((decimal)Eval("Amount")).ToString("$###,###,##0") %>
                                                        </td>
                                                        <td class="tdExpenseVariance"></td>
                                                        <td class="tdExpenseVariancePer"></td>
                                                    </tr>
                                                </ItemTemplate>
                                                <FooterTemplate>
                                                    <tr class="fontBold static TextAlignLeftImp LeftPadding10px">
                                                        <td class="TextAlignLeftImp LeftPadding10px">Grand Total
                                                        </td>
                                                        <td class="BudgetExpense">
                                                            <asp:Label ID="lblTotalBudgetExpense" runat="server"></asp:Label>
                                                        </td>

                                                        <td class="ProjExpense">
                                                            <asp:Label ID="lblTotalProjectedExpense" runat="server"></asp:Label>
                                                        </td>
                                                        <td class="EACExpense">
                                                            <asp:Label ID="lblTotalEACExpense" runat="server"></asp:Label>
                                                        </td>
                                                        <td class="ActualsExpense">
                                                            <asp:Label ID="lblTotalActualExpense" runat="server"></asp:Label>
                                                        </td>
                                                        <td class="tdExpenseVariance"></td>
                                                        <td class="tdExpenseVariancePer"></td>
                                                    </tr>
                                                    </tbody></table></div>
                                                </FooterTemplate>
                                            </asp:Repeater>
                                        </div>

                                    </div>
                                </ContentTemplate>
                                <Triggers>
                                    <asp:AsyncPostBackTrigger ControlID="lnkViewAllResources" />
                                </Triggers>
                            </asp:UpdatePanel>
                        </td>
                    </tr>
                </table>
            </div>
            <br />
            <br />
            <div id="divEmpty" runat="server">
                No data to display for the selected filters
            </div>

            <asp:Chart ID="chartPdf" runat="server" Width="900px" Height="300px" Visible="false">
                <Legends>
                    <asp:Legend Alignment="Center" Docking="Bottom" Name="MainLegend">
                    </asp:Legend>

                </Legends>
                <Series>
                    <asp:Series Name="Budget Revenue" BorderWidth="2" ChartType="Spline" BorderDashStyle="Solid"
                        Legend="MainLegend" Color="Blue" ChartArea="MainArea">
                    </asp:Series>
                    <asp:Series Name="Actual Revenue" BorderWidth="2" ChartType="Spline" BorderDashStyle="Solid"
                        Legend="MainLegend" Color="Green" ChartArea="MainArea">
                    </asp:Series>
                    <asp:Series Name="Projected Revenue" BorderWidth="2" ChartType="Spline" BorderDashStyle="Dash"
                        Legend="MainLegend" Color="Green" ChartArea="MainArea">
                    </asp:Series>
                    <asp:Series Name="Budget Amount" BorderWidth="1" ChartType="Line" BorderDashStyle="Solid"
                        Legend="MainLegend" Color="Gold" ChartArea="MainArea">
                    </asp:Series>
                </Series>
                <ChartAreas>
                    <asp:ChartArea Name="MainArea">
                        <AxisY Title="Revenue ($)">
                            <MajorGrid LineColor="Gray" LineDashStyle="Solid" />
                        </AxisY>
                        <AxisX Title="Date">
                            <MajorGrid LineColor="DarkGray" LineDashStyle="NotSet" IntervalType="Days" />
                            <LabelStyle Angle="60" />
                        </AxisX>
                    </asp:ChartArea>
                </ChartAreas>
            </asp:Chart>
        </ContentTemplate>

    </asp:UpdatePanel>
</asp:Content>

