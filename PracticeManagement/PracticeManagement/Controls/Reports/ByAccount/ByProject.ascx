﻿<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ByProject.ascx.cs" Inherits="PraticeManagement.Controls.Reports.ByAccount.ByProject" %>
<%@ Register Src="~/Controls/FilteredCheckBoxList.ascx" TagName="FilteredCheckBoxList"
    TagPrefix="uc" %>
<div class="tab-pane">
    <table class="WholeWidthWithHeight">
        <tr>
            <td colspan="4" class="Width90Percent"></td>
            <td class="textRight Width10Percent padRight5">
                <table class="textRight WholeWidth">
                    <tr>
                        <td>Export:&nbsp;
                            <asp:Button ID="btnExportToExcel" runat="server" Text="Excel" OnClick="btnExportToExcel_OnClick"
                                UseSubmitBehavior="false" ToolTip="Export To Excel" />
                        </td>

                    </tr>
                </table>
            </td>
        </tr>
    </table>

    <asp:Repeater ID="repProject" runat="server" OnItemDataBound="repProject_ItemDataBound">
        <HeaderTemplate>
            <div class="minheight250Px">
                <table id="tblAccountSummaryByProject" class="tablesorter TimePeriodByproject WholeWidth">
                    <thead>
                        <tr>
                            <th class="ProjectColoum">Project
                               
                            </th>
                            <th class="Width140pxImp">Status
                                
                            </th>
                            <th class="Width140pxImp">Billing Type
                            </th>
                            <th class="Width140pxImp">Projected Hours
                            </th>
                            <th class="Width140pxImp">Billable
                            </th>
                            <th class="Width130pxImp" id="thNonBillable" runat="server">Non-Billable
                            </th>
                            <th class="Width140pxImp">Actual Hours
                            </th>
                            <th class="Width140pxImp">Budget Hours
                            </th>
                            <th class="Width140pxImp">ETC Hours
                            </th>
                            <th class="Width160PxImp">Total Estimated Billings
                            </th>
                            <th class="Width170PxImp">Billable Hours Variance
                                  <asp:Image alt="Billable Hours Variance Hint" ImageUrl="~/Images/hint1.png" runat="server"
                                      ID="imgBillableHoursVarianceHint" CssClass="CursorPointer" ToolTip="Billable Hours Variance Calculation" />
                                <AjaxControlToolkit:ModalPopupExtender ID="mpeBillableUtilization" runat="server"
                                    TargetControlID="imgBillableHoursVarianceHint" CancelControlID="btnCancel" BehaviorID="pnlBillableUtilization"
                                    BackgroundCssClass="modalBackground" PopupControlID="pnlBillableUtilization"
                                    DropShadow="false" />
                            </th>
                          
                        </tr>
                    </thead>
                    <tbody>
        </HeaderTemplate>
        <ItemTemplate>
            <tr class="ReportItemTemplate">
                <td class="t-left padLeft5" sorttable_customkey='<%# Eval("Project.TimeEntrySectionId")%><%# Eval("Project.ProjectNumber")%>'>
                    <table class="TdLevelNoBorder PeronSummaryReport">
                        <tr>
                            <td class="FirstTd">
                                <%# Eval("Project.Client.HtmlEncodedName")%>
                                >
                                <%# Eval("Project.Group.HtmlEncodedName")%>
                            </td>
                        </tr>
                        <tr>
                            <td class="SecondTd">
                                <%# Eval("Project.ProjectNumber")%>
                                -
                                <asp:Label ID="lblProjectName" runat="server" Visible="false" Text=' <%# Eval("Project.HtmlEncodedName")%> '></asp:Label>
                                <asp:HyperLink ID="hlProjectName" runat="server" CssClass="HyperlinkByProjectReport"
                                    Text=' <%# Eval("Project.HtmlEncodedName")%> ' Target="_blank" NavigateUrl='<%# GetProjectDetailsLink((int?)(Eval("Project.Id"))) %>'>
                                </asp:HyperLink>
                            </td>
                        </tr>
                    </table>
                </td>
                <td class="textCenter" sorttable_customkey='<%# Eval("Project.Status.Name") %><%#Eval("Project.ProjectNumber")%>'>
                    <%# Eval("Project.Status.Name")%>
                </td>
                <td>
                    <%# Eval("BillingType")%>
                </td>
                <td sorttable_customkey='<%# Eval("ForecastedHours") %>'>
                    <%# GetDoubleFormat((double)Eval("ForecastedHours"))%>
                </td>
                <td>
                    <%# GetDoubleFormat((double)Eval("BillableHours"))%>
                </td>
                <td id="tdNonBillable" runat="server">
                    <%# GetDoubleFormat((double)Eval("NonBillableHours"))%>
                </td>
                <td>
                    <asp:Label ID="lblActualHours" runat="server" Visible="false" Text=' <%# GetDoubleFormat((double)Eval("TotalHours"))%> '></asp:Label>
                    <asp:HyperLink ID="hlActualHours" runat="server" Text=' <%# GetDoubleFormat((double)Eval("TotalHours"))%> '
                        Target="_blank" NavigateUrl='<%# GetReportByProjectLink((string)Eval("Project.ProjectNumber"))%>'>
                    </asp:HyperLink>
                </td>
                <td sorttable_customkey='<%# Eval("BudgetHours") %>'>
                    <%# (double)Eval("BudgetHours")>0?  GetDoubleFormat((double)Eval("BudgetHours")):"-"%>
                </td>
                <td sorttable_customkey='<%# Eval("ETCHours") %>'>
                    <%# GetDoubleFormat((double)Eval("ETCHours"))%>
                </td>
                <td>
                    <asp:Label ID="lblEstimatedBillings" runat="server"></asp:Label>
                </td>
                <td sorttable_customkey='<%# Eval("BillableHoursVariance") %>'>
                    <table class="WholeWidth TdLevelNoBorder">
                        <tr>
                            <td class="Width50Percent textRightImp">
                                <%#((double)Eval("BillableHoursVariance") > 0) ? "+" + GetDoubleFormat((double)Eval("BillableHoursVariance")) : GetDoubleFormat((double)Eval("BillableHoursVariance"))%>
                            </td>
                            <td class="Width50Percent t-left">
                                <asp:Label ID="lblExclamationMark" runat="server" Visible='<%# ((double)Eval("BillableHoursVariance") < 0)%>'
                                    Text="!" CssClass="error-message fontSizeLarge" ToolTip="Project Underrun"></asp:Label>
                            </td>
                        </tr>
                    </table>
                </td>

            </tr>
        </ItemTemplate>
        <FooterTemplate>
            </tbody></table></div>
        </FooterTemplate>
    </asp:Repeater>
    <div id="divEmptyMessage" style="display: none;" class="EmptyMessagediv" runat="server">
        There are no projects with Active or Completed statuses for the report parameters
        selected.
    </div>
</div>
<asp:Panel ID="pnlBillableUtilization" runat="server" CssClass="popUpBillableUtilization"
    Style="display: none;">
    <table>
        <tr>
            <td colspan="2" class="textCenter">
                <label class="LabelProject">
                    Billable Hours Variance
                </label>
            </td>
            <td>
                <asp:Button ID="btnCancel" runat="server" CssClass="mini-report-close floatright"
                    ToolTip="Close" Text="X"></asp:Button>
            </td>
        </tr>
        <tr>
            <td>
                <br />
            </td>
        </tr>
        <tr>
            <td>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; For a time period that includes today's date, the Billable Hours Variance is calculated as the number of Billable Hours <b>up to and including today</b> minus the number of Projected Hours <b>up to and including today</b>.</p>
            </td>
        </tr>
        <tr>
            <td>
                <br />
            </td>
        </tr>
        <tr>
            <td>
                <p>
                    &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;For historical time periods, the system calculates Billable Hours Variance as Projected
                Hours minus Actual Hours.
                </p>
            </td>
        </tr>
    </table>
</asp:Panel>

