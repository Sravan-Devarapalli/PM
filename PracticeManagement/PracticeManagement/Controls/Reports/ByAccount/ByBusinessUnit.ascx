<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ByBusinessUnit.ascx.cs"
    Inherits="PraticeManagement.Controls.Reports.ByAccount.ByBusinessUnit" %>
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

    <asp:Repeater ID="repBusinessUnit" runat="server" OnItemDataBound="repBusinessUnit_ItemDataBound">
        <HeaderTemplate>
            <div class="minheight250Px">
                <table id="tblAccountSummaryByBusinessReport" class="tablesorter PersonSummaryReport WholeWidth zebra">
                    <thead>
                        <tr>
                            <th class="TextAlignLeftImp Width320PxImp">Business Unit
                            </th>
                            <th class="Width170PxImp"># of Active Projects
                            </th>
                            <th class="Width170PxImp"># of Completed Projects
                            </th>
                            <th class="Width170PxImp">Projected Hours
                            </th>
                            <th class="Width150pxImp">Billable
                            </th>
                            <th class="Width150pxImp" id="thNonBillable" runat="server">Non-Billable
                            </th>
                            <th class="Width150pxImp">Actual Hours
                            </th>
                            <th class="Width150pxImp">Budget Hours
                            </th>
                            <th class="Width150pxImp">ETC Hours
                            </th>
                            <th class="Width150pxImp">Total BU Hours
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
                <td class="padLeft5 textLeft">
                    <%# Eval("BusinessUnit.HtmlEncodedName")%>
                    (<%# ((Boolean)Eval("BusinessUnit.IsActive")) ? "Active" : "Inactive"%>)
                </td>
                <td>
                    <%# Eval("ActiveProjectsCount")%>
                </td>
                <td>
                    <%# Eval("CompletedProjectsCount")%>
                </td>
                <td>
                    <%# GetDoubleFormat((double)Eval("ForecastedHours"))%>
                </td>
                <td>
                    <%# GetDoubleFormat((double)Eval("BillableHours"))%>
                </td>
                <td id="tdNonBillable" runat="server">
                    <%# GetDoubleFormat((double)Eval("NonBillableHours"))%>
                </td>
                <td>
                    <%# GetDoubleFormat((double)Eval("ActualHours"))%>
                </td>
                <td sorttable_customkey='<%# Eval("BudgetHours") %>'>
                    <%# GetDoubleFormat((double)Eval("BudgetHours"))%>
                </td>
                <td sorttable_customkey='<%# Eval("ETCHours") %>'>
                    <%# GetDoubleFormat((double)Eval("ETCHours"))%>
                </td>
                <td>
                    <%# GetDoubleFormat((double)Eval("TotalHours"))%>
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
    <div id="divEmptyMessage" class="EmptyMessagediv" style="display: none;" runat="server">
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
                    &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;For historical time periods, the
                    system calculates Billable Hours Variance as Projected Hours minus Actual Hours.
                </p>
            </td>
        </tr>
    </table>
</asp:Panel>

