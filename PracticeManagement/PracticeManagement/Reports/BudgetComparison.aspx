<%@ Page Title="Budget Comparison" Language="C#" MasterPageFile="~/PracticeManagementMain.Master" AutoEventWireup="true" CodeBehind="BudgetComparison.aspx.cs" Inherits="PraticeManagement.Reports.BudgetComparision" %>

<%@ Register TagPrefix="uc" TagName="LoadingProgress" Src="~/Controls/Generic/LoadingProgress.ascx" %>
<%@ Register Src="~/Controls/MessageLabel.ascx" TagName="MessageLabel" TagPrefix="uc" %>
<%@ Register Src="~/Controls/Generic/Filtering/DateInterval.ascx" TagPrefix="uc"
    TagName="DateInterval" %>

<asp:Content ID="Content1" ContentPlaceHolderID="title" runat="server">
    <title>Budget Comparison | Practice Management</title>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="header" runat="server">
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="body" runat="server">

    <script type="text/javascript">
        function txtSearch_onkeypress(e) {
            var keynum;
            if (window.event) // IE8 and earlier
            {
                keynum = e.keyCode;
            }
            else if (e.which) // IE9/Firefox/Chrome/Opera/Safari
            {
                keynum = e.which;
            }
            if (keynum == 13) {
                var btnSearch = document.getElementById('<%= btnProjectSearch.ClientID %>');
                btnSearch.click();
                return false;
            }
            return true;
        }

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
            }
        }



        function txtSearch_onkeyup(e) {

            var txtProjectSearch = document.getElementById('<%= txtProjectSearch.ClientID %>');
            var btnSearch = document.getElementById('<%= btnProjectSearch.ClientID %>');
            if (txtProjectSearch.value != '') {
                btnSearch.removeAttribute('disabled');
            }
            else {
                btnSearch.setAttribute('disabled', 'disabled');
            }
            return true;
        }

        function FindSum() {
            $('.columnSum').each(function () {
                var tdPosition = $(this).index() + 1;
                var sum = 0;
                $(this).closest('table').find('td:nth-child(' + tdPosition + ')').each(function () {
                    val = $(this).text().replace('$', '');
                    val = val.replace(/,/g, '');
                    if (!isNaN(val) && val.length != 0) {
                        sum += parseFloat(val);
                    }
                });
                var _reqText = sum.toFixed(2).toString();
                if (_reqText.indexOf('-') >= 0) {
                    _reqText = '<span class="Bench">' + '(' + _reqText.replace('-', '') + ')' + '</span>';
                }
                $(this).closest('table').find('tr[id$="Footer"] td:nth-child(' + tdPosition + ')').html(_reqText);
            });

            $('.revenueSum').each(function () {
                var tdPosition = $(this).index() + 1;
                var sum = 0;
                var isHidden = false;
                $(this).closest('table').find('td:nth-child(' + tdPosition + ')').each(function () {
                    val = $(this).text().replace('($', '-');
                    val = val.replace(/,/g, '');
                    val = val.replace('$', '');
                    val = val.replace(')', '');
                    if (isNaN(val)) {
                        isHidden = true;
                    }
                    if (!isNaN(val) && val.length != 0) {
                        sum += parseFloat(val);
                    }

                });
                if (!isHidden) {
                    sum = sum.toFixed(0);
                    var _reqText = '$' + sum.toString().replace(/(\d)(?=(\d{3})+\.)/g, "$1,").replace(/\B(?=(?:\d{3})+(?!\d))/g, ",");
                    if (_reqText.indexOf('-') >= 0) {
                        _reqText = '<span class="Bench">' + '(' + _reqText.replace('-', '') + ')' + '</span>';
                    }
                    $(this).closest('table').find('tr[id$="Footer"] td:nth-child(' + tdPosition + ')').html(_reqText);
                }
                else {
                    $(this).closest('table').find('tr[id$="Footer"] td:nth-child(' + tdPosition + ')').html('(Hidden)');

                }
            });

            $('#tblBudgetExpense tr').each(function () {
                var tr = $(this)
                $('#tblBudgetResource').append(tr);
            });

            $('#tblSelectedExpense tr').each(function () {
                var tr = $(this)
                $('#tblResource').append(tr);
            });

            $('#tblDiffExpense tr').each(function () {
                var tr = $(this)
                $('#tblDifference').append(tr);
            });

            $('.expenseSum').each(function () {
                var tdPosition = $(this).index() + 1;
                var sum = 0;
                var isHidden = false;
                $(this).closest('table').find('tr[id$="lvExpenseItem"] td:nth-child(' + tdPosition + ')').each(function () {
                    val = $(this).text().replace('($', '-');
                    val = val.replace(/,/g, '');
                    val = val.replace('$', '');
                    val = val.replace(')', '');

                    if (!isNaN(val) && val.length != 0) {
                        sum += parseFloat(val);
                    }
                });
                $(this).closest('table').find('tr[id$="lvFooter"] td:nth-child(' + tdPosition + ')').each(function () {
                    val = $(this).text().replace('($', '-');
                    val = val.replace(/,/g, '');
                    val = val.replace('$', '');
                    val = val.replace(')', '');
                    if (isNaN(val)) {
                        isHidden = true;
                    }
                    if (!isNaN(val) && val.length != 0) {
                        sum += parseFloat(val);
                    }
                });
                if (!isHidden) {
                    var _reqText = '$' + sum.toString().replace(/(\d)(?=(\d{3})+\.)/g, "$1,").replace(/\B(?=(?:\d{3})+(?!\d))/g, ",");
                    if (_reqText.indexOf('-') >= 0) {
                        _reqText = '<span class="Bench">' + '(' + _reqText.replace('-', '') + ')' + '</span>';
                    }

                    $(this).closest('table').find('tr[id$="lvExpenseFooter"] td:nth-child(' + tdPosition + ')').html(_reqText);
                }
                else {
                    $(this).closest('table').find('tr[id$="lvExpenseFooter"] td:nth-child(' + tdPosition + ')').html('(Hidden)');

                }
            });

            $("[id$=lvExpenseFooter]").each(function () {
                var lastIndex = $(this).find("td:last").index() + 1;
                var margin = $(this).find('td:nth-child(' + (lastIndex - 1) + ')').text().replace('($', '-');
                margin = margin.replace(/,/g, '');
                margin = margin.replace('$', '');
                margin = margin.replace(')', '');
                var revenue = $(this).find('td:nth-child(' + (lastIndex - 3) + ')').text().replace('($', '-');
                revenue = revenue.replace(/,/g, '');
                revenue = revenue.replace('$', '');
                revenue = revenue.replace(')', '');
                if (!isNaN(margin) && margin.length != 0 && !isNaN(revenue) && revenue.length != 0) {
                    var MarPer = (parseFloat(revenue) !== 0 ? parseFloat(margin) * 100 / parseFloat(revenue) : 0).toFixed(2);
                    $(this).find('td:nth-child(' + (lastIndex) + ')').html(MarPer.toString());
                }
                else if (isNaN(margin)) {
                    $(this).find('td:nth-child(' + (lastIndex) + ')').html('(Hidden)');
                }
                else {
                    $(this).find('td:nth-child(' + (lastIndex) + ')').html('-');
                }
            })

        }
    </script>
    <uc:LoadingProgress ID="LoadingProgress1" runat="server" />
    <asp:UpdatePanel ID="upnlBody" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="false">
        <ContentTemplate>
            <asp:UpdatePanel ID="UpdatePanel1" runat="server">
                <ContentTemplate>
                    <table class="WholeWidth Height160Px">
                        <tr>
                            <td colspan="2">&nbsp;</td>
                        </tr>
                        <tr>
                            <td class="ReportTdSecond" id="tdSecond" runat="server">
                                <table class="ReportParametersTable">
                                    <tr>
                                        <td class="FirstTd20">Project Number:&nbsp;
                                        </td>
                                        <td class="SecondTd150">
                                            <table>
                                                <tr>
                                                    <td>
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
                                    </tr>
                                </table>
                            </td>
                            <td id="tdThird" runat="server" class="Width35Percent">&nbsp;
                            </td>
                        </tr>
                        <tr>
                            <td class="ReportTdSecond">
                                <table class="ReportParametersTable">
                                    <tr>
                                        <td class="FirstTd20">Range:&nbsp;
                                        </td>
                                        <td class="SecondTd157">
                                            <pmc:CustomDropDown ID="ddlPeriod" runat="server" AutoPostBack="false" onchange="CheckAndShowCustomDatesPoup(this);">
                                                <asp:ListItem Selected="True" Text="Entire Project" Value="*">
                                                </asp:ListItem>
                                                <asp:ListItem Text="Payroll – Current" Value="15"></asp:ListItem>
                                                <asp:ListItem Text="Payroll – Previous" Value="-15"></asp:ListItem>
                                                <asp:ListItem Text="This Week" Value="7"></asp:ListItem>
                                                <asp:ListItem Text="This Month" Value="30"></asp:ListItem>
                                                <asp:ListItem Text="This Year" Value="365"></asp:ListItem>
                                                <asp:ListItem Text="Last Week" Value="-7"></asp:ListItem>
                                                <asp:ListItem Text="Last Month" Value="-30"></asp:ListItem>
                                                <asp:ListItem Text="Last Year" Value="-365"></asp:ListItem>
                                                <asp:ListItem Text="Q1" Value="1"></asp:ListItem>
                                                <asp:ListItem Text="Q2" Value="2"></asp:ListItem>
                                                <asp:ListItem Text="Q3" Value="3"></asp:ListItem>
                                                <asp:ListItem Text="Q4" Value="4"></asp:ListItem>
                                                <asp:ListItem Text="Custom Dates" Value="0">
                                                </asp:ListItem>
                                            </pmc:CustomDropDown>
                                        </td>
                                    </tr>
                                </table>
                            </td>
                            <td>&nbsp;
                            </td>
                        </tr>
                        <tr>
                            <td class="ReportTdSecond">
                                <table class="ReportParametersTable">
                                    <tr>
                                        <td class="FirstTd20">&nbsp;
                                        </td>
                                        <td class="SecondTd157">
                                            <asp:HiddenField ID="hdnStartDate" runat="server" Value="" />
                                            <asp:HiddenField ID="hdnEndDate" runat="server" Value="" />
                                            <asp:Label ID="lblCustomDateRange" runat="server" Text=""></asp:Label>
                                            <asp:Image ID="imgCalender" runat="server" ImageUrl="~/Images/calendar.gif" />
                                        </td>
                                    </tr>
                                </table>
                            </td>
                            <td>&nbsp;
                            </td>
                        </tr>
                        <tr>
                            <td class="ReportTdSecond">
                                <table class="ReportParametersTable">
                                    <tr>
                                        <td class="FirstTd20">View:&nbsp;
                                        </td>
                                        <td class="SecondTd157">
                                            <asp:DropDownList ID="ddlView" runat="server" AutoPostBack="false">
                                                <asp:ListItem Selected="True" Text="Please Select" Value=""></asp:ListItem>
                                                <asp:ListItem Text="Projected" Value="0"></asp:ListItem>
                                                <asp:ListItem Text="Actuals" Value="1"></asp:ListItem>
                                                <asp:ListItem Text="ETC" Value="2"></asp:ListItem>
                                            </asp:DropDownList>
                                        </td>
                                    </tr>
                                </table>
                            </td>
                            <td>&nbsp;
                            </td>
                        </tr>
                        <tr>
                            <td class="ReportTdSecond">
                                <table class="ReportParametersTable">
                                    <tr>
                                        <td class="FirstTd20">Actuals:&nbsp;
                                        </td>
                                        <td class="SecondTd157">
                                            <asp:DropDownList ID="ddlActualPeriod" runat="server" AutoPostBack="false">
                                                <asp:ListItem Value="0">All</asp:ListItem>
                                                <asp:ListItem Value="15">Last Pay Period</asp:ListItem>
                                                <asp:ListItem Value="30">Prior Month End</asp:ListItem>
                                                <asp:ListItem Selected="True" Value="1">Today</asp:ListItem>
                                            </asp:DropDownList>
                                        </td>
                                    </tr>
                                </table>
                            </td>
                            <td>
                                <asp:Button ID="btnUpdate" Text="Update Report" runat="server" OnClick="btnUpdate_Click" />
                            </td>
                        </tr>
                        <tr>
                            <td class="ReportTdSecond">
                                <table class="ReportParametersTable">
                                    <tr>
                                        <td class="Width300Px" colspan="3">
                                            <uc:MessageLabel ID="msgError" runat="server" ErrorColor="Red" InfoColor="Green"
                                                WarningColor="Orange" EnableViewState="false" />
                                        </td>
                                    </tr>
                                </table>
                            </td>
                            <td>&nbsp;
                            </td>
                        </tr>
                        <tr>
                            <td colspan="2" class="ReportBorderBottom"></td>
                        </tr>
                    </table>
                    <AjaxControlToolkit:ModalPopupExtender ID="mpeCustomDates" runat="server" TargetControlID="imgCalender"
                        BackgroundCssClass="modalBackground" PopupControlID="pnlCustomDates" BehaviorID="bhCustomDates"
                        DropShadow="false" />
                    <asp:Panel ID="pnlCustomDates" runat="server" CssClass="ConfirmBoxClass CustomDatesPopUp" Style="display: none;">
                        <table class="WholeWidth">
                            <tr>
                                <td align="center">
                                    <uc:DateInterval ID="diRange" runat="server" IsFromDateRequired="true" IsToDateRequired="true"
                                        FromToDateFieldCssClass="Width70Px" />
                                </td>
                            </tr>
                            <tr>
                                <td class="custBtns">
                                    <asp:Button ID="btnCustDatesOK" runat="server" OnClick="btnCustDatesOK_Click" Text="OK"
                                        CausesValidation="true" />
                                    &nbsp; &nbsp;
                            <asp:Button ID="btnCustDatesCancel" CausesValidation="false" runat="server" Text="Cancel"
                                OnClick="btnCustDatesCancel_OnClick" />
                                </td>
                            </tr>
                            <tr>
                                <td class="textCenter">
                                    <asp:ValidationSummary ID="valSumDateRange" runat="server" ValidationGroup='<%# ClientID %>' />
                                </td>
                            </tr>
                        </table>
                    </asp:Panel>
                    <AjaxControlToolkit:ModalPopupExtender ID="mpeProjectSearch" runat="server" TargetControlID="imgProjectSearch"
                        BackgroundCssClass="modalBackground" PopupControlID="pnlProjectSearch" BehaviorID="mpeProjectSearch"
                        DropShadow="false" />
                    <asp:Panel ID="pnlProjectSearch" runat="server" CssClass="popUp ProjectSearch" Style="display: none;">
                        <table class="WholeWidth">
                            <tr class="PopUpHeader">
                                <th>Project Search
                            <asp:Button ID="btnclose" runat="server" CssClass="mini-report-closeNew" ToolTip="Close"
                                OnClick="btnclose_OnClick" Text="X"></asp:Button>
                                </th>
                            </tr>
                            <tr>
                                <td class="WholeWidth">
                                    <table class="WholeWidth">
                                        <tr>
                                            <td class="Width100Px textRight">Account:
                                            </td>
                                            <td>
                                                <asp:DropDownList ID="ddlClients" runat="server" CssClass="Width250Px" OnSelectedIndexChanged="ddlClients_OnSelectedIndexChanged"
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
                                                <pmc:CustomDropDown ID="ddlProjects" runat="server" Enabled="false" AutoPostBack="true"
                                                    CssClass="Width250Px" OnSelectedIndexChanged="ddlProjects_OnSelectedIndexChanged">
                                                    <asp:ListItem Text="-- Select a Project --" Value="">
                                                    </asp:ListItem>
                                                </pmc:CustomDropDown>
                                            </td>
                                        </tr>
                                    </table>
                                </td>
                            </tr>
                            <tr>
                                <td class="WholeWidth">
                                    <table class="WholeWidth">
                                        <tr>
                                            <td>
                                                <asp:TextBox ID="txtProjectSearch" onkeypress="return txtSearch_onkeypress(event);"
                                                    onkeyup="return txtSearch_onkeyup(event);" CssClass="Width330Px" runat="server"></asp:TextBox>
                                                <AjaxControlToolkit:TextBoxWatermarkExtender ID="wmeProjectSearch" runat="server"
                                                    TargetControlID="txtProjectSearch" WatermarkCssClass="watermarkedtext Width330Px"
                                                    WatermarkText="To search for a project, click here to begin typing...">
                                                </AjaxControlToolkit:TextBoxWatermarkExtender>
                                            </td>
                                            <td>
                                                <asp:Button ID="btnProjectSearch" UseSubmitBehavior="false" disabled="disabled" runat="server"
                                                    Text="Search" ToolTip="Search" OnClick="btnProjectSearch_Click" />
                                            </td>
                                        </tr>
                                    </table>
                                </td>
                            </tr>
                            <tr>
                                <td class="WholeWidth">
                                    <div class="ProjectSearchResultsDiv">
                                        <asp:Repeater ID="repProjectNamesList" runat="server">
                                            <HeaderTemplate>
                                                <table id="tblProjectSearchResult" class="tablesorter CompPerfTable ProjectSearchResultsTable">
                                                    <thead>
                                                        <tr class="CompPerfHeader">
                                                            <th class="Width20Percent">
                                                                <div class="ie-bg">
                                                                    Project
                                                                </div>
                                                            </th>
                                                            <th class="Width50Percent">
                                                                <div class="ie-bg">
                                                                    Project Name
                                                                </div>
                                                            </th>
                                                            <th>
                                                                <div class="ie-bg">
                                                                    Account
                                                                </div>
                                                            </th>
                                                        </tr>
                                                    </thead>
                                                    <tbody>
                                            </HeaderTemplate>
                                            <ItemTemplate>
                                                <tr>
                                                    <td class="textCenter">
                                                        <asp:LinkButton ID="lnkProjectNumber" ProjectNumber='<%# Eval("ProjectNumber")%>'
                                                            OnClick="lnkProjectNumber_OnClick" runat="server"><%# Eval("ProjectNumber")%></asp:LinkButton>
                                                    </td>
                                                    <td>
                                                        <%# Eval("HtmlEncodedName")%>
                                                    </td>
                                                    <td>
                                                        <%# Eval("Client.HtmlEncodedName")%>
                                                    </td>
                                                </tr>
                                            </ItemTemplate>
                                            <FooterTemplate>
                                                </tbody></table>
                                            </FooterTemplate>
                                        </asp:Repeater>
                                    </div>
                                </td>
                            </tr>
                            <tr>
                                <td class="padLeft8">
                                    <asp:Literal ID="ltrlNoProjectsText" Visible="false" runat="server" Text="No Projects found."></asp:Literal>
                                </td>
                            </tr>
                        </table>
                    </asp:Panel>
                </ContentTemplate>
            </asp:UpdatePanel>

            <asp:UpdatePanel ID="uplReport" runat="server" UpdateMode="Conditional">
                <ContentTemplate>
                    <div id="divWholePage" runat="server" visible="false">
                        <br />
                        <br />
                        <div class="textRight">
                            <table class="WholeWidthWithHeight" id="tblExport" runat="server">
                                <tr>
                                    <td colspan="4" class="FloatLeft font16Px fontBold">
                                        <asp:Label ID="lblProjectName" runat="server"></asp:Label>
                                    </td>
                                    <td class="Width10Percent padRight5">
                                        <table class="WholeWidth">
                                            <tr>
                                                <td class="floatright">Export: &nbsp;
                                                    <asp:Button ID="btnExportToExcel" runat="server" Text="Excel" OnClick="btnExportToExcel_Click"
                                                        Enabled="true" UseSubmitBehavior="false" ToolTip="Export To Excel" />
                                                </td>

                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                            </table>
                        </div>
                        <br />
                        <br />
                        <div style="background-color: #bbcef6;" class="font14Px fontBold">
                            Budget
                        </div>

                        <asp:Repeater ID="repBudgetResource" runat="server" OnItemDataBound="repBudgetResource_ItemDataBound">
                            <HeaderTemplate>
                                <div style="max-height: 400px; overflow: auto;">
                                    <table id="tblBudgetResource" class="tablesorter TimePeriodByproject WholeWidth">
                                        <thead>
                                            <tr runat="server" id="lvHeader" isbudget="true">
                                                <th class="ProjectColoum no-wrap">Resource
                                                </th>
                                            </tr>
                                        </thead>
                                        <tbody>
                            </HeaderTemplate>
                            <ItemTemplate>
                                <tr class="ReportItemTemplate no-wrap" runat="server" id="lvItem" isbudget="true">
                                    <td class="t-left padLeft5 ">
                                        <asp:Label ID="lblPerson" runat="server" Text='<%# HttpUtility.HtmlEncode((string)Eval("Person.LastName") + ", " + Eval("Person.FirstName")+"("+Eval("Person.Title.TitleName")+")") %>'></asp:Label>
                                    </td>
                                </tr>
                            </ItemTemplate>
                            <AlternatingItemTemplate>
                                <tr class="ReportItemTemplate bgGroupByProjectHeader no-wrap" runat="server" id="lvItem" isbudget="true">
                                    <td class="t-left padLeft5 ">
                                        <asp:Label ID="lblPerson" runat="server" Text='<%# HttpUtility.HtmlEncode((string)Eval("Person.LastName") + ", " + Eval("Person.FirstName")+"("+Eval("Person.Title.TitleName")+")") %>'></asp:Label>
                                    </td>
                                </tr>
                            </AlternatingItemTemplate>
                            <FooterTemplate>
                                <tr class="ReportItemTemplate bgGroupByProjectHeader no-wrap fontBold" runat="server" id="lvFooter" isbudget="true">
                                    <td class="t-left padLeft5 ">Services Total
                                    </td>
                                </tr>
                                </tbody></table> </div>
                            </FooterTemplate>
                        </asp:Repeater>
                        <br />
                        <asp:Repeater ID="repBudgetExpense" runat="server" OnItemDataBound="repBudgetExpense_ItemDataBound">
                            <HeaderTemplate>
                                <div style="max-height: 400px; overflow: auto;">
                                    <table id="tblBudgetExpense" class="tablesorter TimePeriodByproject WholeWidth">
                                        <thead>
                                        </thead>
                                        <tbody>
                            </HeaderTemplate>
                            <ItemTemplate>
                                <tr class="ReportItemTemplate no-wrap" runat="server" id="lvExpenseItem" isbudget="true">
                                    <td class="t-left padLeft5 ">
                                        <asp:Label ID="lblType" runat="server" Text='<%# HttpUtility.HtmlEncode((string)Eval("Type.Name")) %>'></asp:Label>
                                    </td>
                                </tr>
                            </ItemTemplate>
                            <AlternatingItemTemplate>
                                <tr class="ReportItemTemplate bgGroupByProjectHeader no-wrap" runat="server" id="lvExpenseItem" isbudget="true">
                                    <td class="t-left padLeft5 ">
                                        <asp:Label ID="lblType" runat="server" Text='<%# HttpUtility.HtmlEncode((string)Eval("Type.Name")) %>'></asp:Label>
                                    </td>
                                </tr>
                            </AlternatingItemTemplate>
                            <FooterTemplate>
                                <tr class="ReportItemTemplate bgGroupByProjectHeader no-wrap fontBold" runat="server" id="lvExpenseFooter" isbudget="true">
                                    <td class="t-left padLeft5 ">Total Expected Billing
                                    </td>
                                </tr>
                                </tbody></table></div>
                            </FooterTemplate>
                        </asp:Repeater>

                        <br />
                        <br />
                        <div style="background-color: #bbcef6;" class="font14Px fontBold">
                            <asp:Label ID="lblDescription" runat="server"></asp:Label>
                        </div>
                        <asp:Repeater ID="repResource" runat="server" OnItemDataBound="repBudgetResource_ItemDataBound">
                            <HeaderTemplate>
                                <div style="max-height: 400px; overflow: auto;">
                                    <table id="tblResource" class="tablesorter TimePeriodByproject WholeWidth">
                                        <thead>
                                            <tr runat="server" id="lvHeader" isbudget="false">
                                                <th class="ProjectColoum no-wrap">Resource
                                                </th>
                                            </tr>
                                        </thead>
                                        <tbody>
                            </HeaderTemplate>
                            <ItemTemplate>
                                <tr class="ReportItemTemplate no-wrap" runat="server" id="lvItem" isbudget="false">
                                    <td class="t-left padLeft5 ">
                                        <asp:Label ID="lblPerson" runat="server" Text='<%# HttpUtility.HtmlEncode((string)Eval("Person.LastName") + ", " + Eval("Person.FirstName")+"("+Eval("Person.Title.TitleName")+")") %>'></asp:Label>
                                    </td>
                                </tr>
                            </ItemTemplate>
                            <AlternatingItemTemplate>
                                <tr class="ReportItemTemplate bgGroupByProjectHeader no-wrap" runat="server" id="lvItem" isbudget="false">
                                    <td class="t-left padLeft5 ">
                                        <asp:Label ID="lblPerson" runat="server" Text='<%# HttpUtility.HtmlEncode((string)Eval("Person.LastName") + ", " + Eval("Person.FirstName")+"("+Eval("Person.Title.TitleName")+")") %>'></asp:Label>
                                    </td>
                                </tr>
                            </AlternatingItemTemplate>
                            <FooterTemplate>
                                <tr class="ReportItemTemplate bgGroupByProjectHeader no-wrap fontBold" runat="server" id="lvFooter" isbudget="false">
                                    <td class="t-left padLeft5 ">Services Total
                                    </td>
                                </tr>
                                </tbody></table></div>
                            </FooterTemplate>
                        </asp:Repeater>
                        <br />
                        <asp:Repeater ID="repSelectedExpense" runat="server" OnItemDataBound="repBudgetExpense_ItemDataBound">
                            <HeaderTemplate>
                                <div style="max-height: 400px; overflow: auto;">
                                    <table id="tblSelectedExpense" class="tablesorter TimePeriodByproject WholeWidth">
                                        <thead>
                                        </thead>
                                        <tbody>
                            </HeaderTemplate>
                            <ItemTemplate>
                                <tr class="ReportItemTemplate no-wrap" runat="server" id="lvExpenseItem" isbudget="false">
                                    <td class="t-left padLeft5 ">
                                        <asp:Label ID="lblType" runat="server" Text='<%# HttpUtility.HtmlEncode((string)Eval("Type.Name")) %>'></asp:Label>
                                    </td>
                                </tr>
                            </ItemTemplate>
                            <AlternatingItemTemplate>
                                <tr class="ReportItemTemplate bgGroupByProjectHeader no-wrap" runat="server" id="lvExpenseItem" isbudget="false">
                                    <td class="t-left padLeft5 ">
                                        <asp:Label ID="lblType" runat="server" Text='<%# HttpUtility.HtmlEncode((string)Eval("Type.Name")) %>'></asp:Label>
                                    </td>
                                </tr>
                            </AlternatingItemTemplate>
                            <FooterTemplate>
                                <tr class="ReportItemTemplate bgGroupByProjectHeader no-wrap fontBold" runat="server" id="lvExpenseFooter" isbudget="false">
                                    <td class="t-left padLeft5 ">Total Expected Billing
                                    </td>
                                </tr>
                                </tbody></table></div>
                            </FooterTemplate>
                        </asp:Repeater>
                        <br />
                        <br />
                        <div style="background-color: #bbcef6;" class="font14Px fontBold">
                            Difference
                        </div>
                        <asp:Repeater ID="repDifference" runat="server" OnItemDataBound="repBudgetResource_ItemDataBound">
                            <HeaderTemplate>
                                <div style="max-height: 400px; overflow: auto;">
                                    <table id="tblDifference" class="tablesorter TimePeriodByproject WholeWidth">
                                        <thead>
                                            <tr runat="server" id="lvHeader" isbudget="difference">
                                                <th class="ProjectColoum no-wrap">Resource
                                                </th>
                                            </tr>
                                        </thead>
                                        <tbody>
                            </HeaderTemplate>
                            <ItemTemplate>
                                <tr class="ReportItemTemplate no-wrap" runat="server" id="lvItem" isbudget="difference">
                                    <td class="t-left padLeft5 ">
                                        <asp:Label ID="lblPerson" runat="server" Text='<%# HttpUtility.HtmlEncode((string)Eval("Person.LastName") + ", " + Eval("Person.FirstName")+"("+Eval("Person.Title.TitleName")+")") %>'></asp:Label>
                                    </td>
                                </tr>
                            </ItemTemplate>
                            <AlternatingItemTemplate>
                                <tr class="ReportItemTemplate bgGroupByProjectHeader no-wrap" runat="server" id="lvItem" isbudget="difference">
                                    <td class="t-left padLeft5 ">
                                        <asp:Label ID="lblPerson" runat="server" Text='<%# HttpUtility.HtmlEncode((string)Eval("Person.LastName") + ", " + Eval("Person.FirstName")+"("+Eval("Person.Title.TitleName")+")") %>'></asp:Label>
                                    </td>
                                </tr>
                            </AlternatingItemTemplate>
                            <FooterTemplate>
                                <tr class="ReportItemTemplate bgGroupByProjectHeader no-wrap fontBold" runat="server" id="lvFooter" isbudget="difference">
                                    <td class="t-left padLeft5 ">Services Total
                                    </td>
                                </tr>
                                </tbody></table></div>
                            </FooterTemplate>
                        </asp:Repeater>
                        <br />
                        <asp:Repeater ID="repExpenseDifference" runat="server" OnItemDataBound="repBudgetExpense_ItemDataBound">
                            <HeaderTemplate>
                                <div style="max-height: 400px; overflow: auto;">
                                    <table id="tblDiffExpense" class="tablesorter TimePeriodByproject WholeWidth">
                                        <thead>
                                        </thead>
                                        <tbody>
                            </HeaderTemplate>
                            <ItemTemplate>
                                <tr class="ReportItemTemplate no-wrap" runat="server" id="lvExpenseItem" isbudget="difference">
                                    <td class="t-left padLeft5 ">
                                        <asp:Label ID="lblType" runat="server" Text='<%# HttpUtility.HtmlEncode((string)Eval("Type.Name")) %>'></asp:Label>
                                    </td>
                                </tr>
                            </ItemTemplate>
                            <AlternatingItemTemplate>
                                <tr class="ReportItemTemplate bgGroupByProjectHeader no-wrap" runat="server" id="lvExpenseItem" isbudget="difference">
                                    <td class="t-left padLeft5 ">
                                        <asp:Label ID="lblType" runat="server" Text='<%# HttpUtility.HtmlEncode((string)Eval("Type.Name")) %>'></asp:Label>
                                    </td>
                                </tr>
                            </AlternatingItemTemplate>
                            <FooterTemplate>
                                <tr class="ReportItemTemplate bgGroupByProjectHeader no-wrap fontBold" runat="server" id="lvExpenseFooter" isbudget="difference">
                                    <td class="t-left padLeft5 ">Total Expected Billing
                                    </td>
                                </tr>
                                </tbody></table></div>
                            </FooterTemplate>
                        </asp:Repeater>
                    </div>
                    <div id="divEmpty" runat="server" visible="false">
                        There are no records for the selected filters
                    </div>
                </ContentTemplate>
            </asp:UpdatePanel>
        </ContentTemplate>
        <Triggers>
            <asp:PostBackTrigger ControlID="btnExportToExcel" />
        </Triggers>
    </asp:UpdatePanel>
</asp:Content>

