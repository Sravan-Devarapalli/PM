<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="BudgetManagementByProject.ascx.cs" Inherits="PraticeManagement.Controls.Projects.BudgetManagementByProject" %>
<%@ Register TagPrefix="ext" Assembly="PraticeManagement" Namespace="PraticeManagement.Controls.Generic.ScrollableDropdown" %>
<script type="text/javascript">
    
</script>
<asp:Panel ID="pnlBudgetByProject" runat="server" CssClass="tab-pane">
    <table class="WholeWidth Height80Px">
        <tr class="tb-header ProjectSummaryAdvancedFiltersHeader">
            <th>View
            </th>
            <td class="Padding5 Width10Px"></td>
            <th>Data Points
            </th>
            <td class="Padding5 Width10Px"></td>

            <th class="tdBudgetManagementActualPicker">Actuals
            </th>
            <td class="Padding5 Width10Px"></td>
            <td rowspan="2" class="Width10Px">
                <asp:Button ID="btnUpdate" runat="server" Text="Update" OnClick="btnUpdate_Click" />
            </td>
            <td class="Padding5 Width10Px"></td>
            <td rowspan="2">
                <asp:Button ID="btnExport" runat="server" Text="Export" OnClick="btnExport_Click" />
            </td>
        </tr>
        <tr>
            <td>
                <asp:DropDownList ID="ddlView" runat="server" AutoPostBack="false" CssClass="height20PImp" Onchange="SetBudgetManagementActualDropDown();">
                    <asp:ListItem Text="-- Select View --" Value="0"></asp:ListItem>
                    <asp:ListItem Text="Budget only" Value="1" Selected="True"></asp:ListItem>
                    <asp:ListItem Text="Budget to ETC" Value="2"></asp:ListItem>
                    <asp:ListItem Text="Budget to Actual + Proj Rem = ETC" Value="3"></asp:ListItem>
                    <asp:ListItem Text="Budget to-date to Actual to-date" Value="4"></asp:ListItem>
                    <asp:ListItem Text="Projected Budget to Projected Remaining" Value="5"></asp:ListItem>
                </asp:DropDownList>
            </td>
            <td></td>
            <td>
                <pmc:ScrollingDropDown ID="ddldataPoints" runat="server" SetDirty="false" CssClass="ProjectSummaryScrollingDropDown TextAlignLeftImp Width200PxImp"
                    onclick="scrollingDropdown_onclick('ddldataPoints','Data point')" DropDownListType="DataPoint" />
                <ext:ScrollableDropdownExtender ID="ScrollableDropdownExtender1" runat="server" TargetControlID="ddldataPoints"
                    UseAdvanceFeature="true" EditImageUrl="~/Images/Dropdown_Arrow.png" Width="200px">
                </ext:ScrollableDropdownExtender>
            </td>
            <td></td>
            <td class="tdBudgetManagementActualPicker">
                <asp:DropDownList ID="ddlActualPeriod" runat="server" AutoPostBack="false" Style="width: 190px;">
                    <asp:ListItem Value="0">All</asp:ListItem>
                    <asp:ListItem Value="15">Last Pay Period</asp:ListItem>
                    <asp:ListItem Value="30">Prior Month End</asp:ListItem>
                    <asp:ListItem Selected="True" Value="1">Today</asp:ListItem>
                </asp:DropDownList>
            </td>
            <td></td>
            <td></td>
        </tr>
    </table>
    <asp:Repeater ID="repResources" runat="server" OnItemDataBound="repResources_ItemDataBound">
        <HeaderTemplate>
            <div id="divBudgetManagement" class="minheight250Px Width90Percent">
                <table id="tblBudgetManagementResource" class="CompPerfTable gvStrawmen" style="overflow: auto">
                    <thead>
                        <tr class="MilestoneHeaderText">
                            <th class="Width20PerImp sorter-false"></th>
                            <th id="thBudget" runat="server" class="ie-bg borderLeftGrey sorter-false" colspan="6">Budget
                            </th>

                            <th id="thActuals" runat="server" class="ie-bg borderLeftGrey sorter-false" colspan="6">Actuals
                            </th>

                            <th id="thProjectedRemaing" runat="server" class="ie-bg borderLeftGrey sorter-false" colspan="6">Projected Remaining 
                            </th>

                            <th id="thEAC" runat="server" class="ie-bg NoBorder borderLeftGrey sorter-false" colspan="6">ETC
                            </th>

                            <th id="thDiff" runat="server" class="ie-bg borderLeftGrey sorter-false" colspan="3">Difference
                            </th>
                        </tr>
                        <tr class="CursorPointer">
                            <th>Title/Role</th>
                            <th id="thBudgetRate" runat="server" class="ie-bg borderLeftGrey Width100Px">Final Rate/Hr.
                            </th>
                            <th id="thBudgetMarginRate" runat="server" class="ie-bg  Width100Px">Margin Rate.
                            </th>
                            <th id="thBudgetHours" runat="server" class="ie-bg NoBorder Width100Px">Hours
                            </th>
                            <th id="thBudgetRevenue" runat="server" class="ie-bg NoBorder Width100Px">Total Revenue
                            </th>
                            <th id="thBudgethMargin" runat="server" class="ie-bg NoBorder Width100Px">Total Margin
                            </th>
                            <th id="thBudgethMarginPer" runat="server" class="ie-bg NoBorder Width100Px">Margin%
                            </th>

                            <th id="thActualRate" runat="server" class="ie-bg borderLeftGrey Width100Px">Rate/Hr.
                            </th>
                            <th id="thActMarginRate" runat="server" class="ie-bg Width100Px">Margin Rate.
                            </th>
                            <th id="thActualHours" runat="server" class="ie-bg NoBorder Width100Px">Hours
                            </th>
                            <th id="thActualTotal" runat="server" class="ie-bg NoBorder Width100Px">Total Revenue
                            </th>
                            <th id="thActualMargin" runat="server" class="ie-bg NoBorder Width100Px">Total Margin
                            </th>
                            <th id="thActMarginPer" runat="server" class="ie-bg NoBorder Width100Px">Margin%
                            </th>

                            <th id="thProjRate" runat="server" class="ie-bg borderLeftGrey Width100Px">Rate/Hr.
                            </th>
                            <th id="thProjMarginRate" runat="server" class="ie-bg Width100Px">Margin Rate.
                            </th>
                            <th id="thProjHours" runat="server" class="ie-bg NoBorder Width100Px">Hours
                            </th>
                            <th id="thProjTotal" runat="server" class="ie-bg NoBorder Width100Px">Total Revenue
                            </th>
                            <th id="thProjMargin" runat="server" class="ie-bg NoBorder Width100Px">Total Margin
                            </th>
                            <th id="thProjMarginPer" runat="server" class="ie-bg NoBorder Width100Px">Margin%
                            </th>

                            <th id="thEACRate" runat="server" class="ie-bg borderLeftGrey Width100Px">Rate/Hr.
                            </th>
                            <th id="thEACMarginRate" runat="server" class="ie-bg Width100Px">Margin Rate.
                            </th>
                            <th id="thEACHours" runat="server" class="ie-bg NoBorder Width100Px">Hours
                            </th>
                            <th id="thEACTotal" runat="server" class="ie-bg NoBorder Width100Px">Total Revenue
                            </th>
                            <th id="thEACMargin" runat="server" class="ie-bg NoBorder Width100Px">Total Margin
                            </th>
                            <th id="thEACMarginPer" runat="server" class="ie-bg NoBorder Width100Px">Margin%
                            </th>


                            <th id="thDiffHours" runat="server" class=" ie-bg borderLeftGrey Width100Px">Hours
                            </th>
                            <th id="thDiffTotals" runat="server" class=" ie-bg NoBorder Width100Px">Total Revenue
                            </th>
                            <th id="thDiffMargin" runat="server" class=" ie-bg NoBorder Width100Px">Total Margin
                            </th>
                        </tr>
                    </thead>
                    <tbody>
        </HeaderTemplate>
        <ItemTemplate>
            <tr>
                <td class="TextAlignLeftImp LeftPadding10px">
                    <asp:Label ID="lblPerson" runat="server" Text='<%# HttpUtility.HtmlEncode((string)Eval("Person.LastName") + ", " + Eval("Person.FirstName")+"("+Eval("Person.Title.TitleName")+")") %>'></asp:Label>
                </td>
                <td id="tdBudgetRate" runat="server" class="borderLeftGrey textRightImp padRight15">
                    <%# Eval("Budget.BillRate") %>
                </td>
                <td id="tdBudgetMarginRate" runat="server" class="textRightImp padRight15">
                    <%# Eval("Budget.MarginRate") %>
                </td>
                <td id="tdBudgetHours" runat="server" class="textRightImp padRight15">
                    <%# Eval("Budget.Hours") %>
                </td>
                <td id="tdBudgetRevenue" runat="server" class="textRightImp padRight15" sorttable_customkey='<%# Eval("Budget.Revenue.Value")%>'>
                    <span class="Revenue"><%# Eval("Budget.Revenue") %></span>
                </td>
                <td id="tdBudgetMargin" runat="server" class="textRightImp padRight15" sorttable_customkey='<%# Eval("Budget.Margin.Value")%>'>
                    <%# Eval("Budget.Margin") %>
                </td>
                <td id="tdBudgetMarginPer" runat="server" class="textRightImp padRight15">
                    <asp:Label ID="lblBudgetMarginPer" runat="server"></asp:Label>
                </td>

                <td id="tdActRate" runat="server" class="borderLeftGrey textRightImp padRight15">
                    <%# Eval("Actuals.BillRate") %>
                </td>
                <td id="tdActMarginRate" runat="server" class="textRightImp padRight15">
                    <%# Eval("Actuals.MarginRate") %>
                </td>
                <td id="tdActHours" runat="server" class="textRightImp padRight15">
                    <%# Eval("Actuals.Hours") %>
                </td>
                <td id="tdActTotal" runat="server" class="textRightImp padRight15" sorttable_customkey='<%# Eval("Actuals.Revenue.Value")%>'>
                    <span class="Revenue"><%# Eval("Actuals.Revenue") %></span>
                </td>
                <td id="tdActMargin" runat="server" class="textRightImp padRight15" sorttable_customkey='<%# Eval("Actuals.Margin.Value")%>'>
                    <%# Eval("Actuals.Margin") %>
                </td>
                <td id="tdActMarginPer" runat="server" class="textRightImp padRight15">
                    <asp:Label ID="lblActMarginPer" runat="server"></asp:Label>
                </td>


                <td id="tdProjRate" runat="server" class="borderLeftGrey textRightImp padRight15">
                    <%# Eval("ProjectedRemaining.BillRate") %>
                </td>
                <td id="tdProjMarginRate" runat="server" class="textRightImp padRight15">
                    <%# Eval("ProjectedRemaining.MarginRate") %>
                </td>
                <td id="tdProjHours" runat="server" class="textRightImp padRight15">
                    <%# Eval("ProjectedRemaining.Hours") %>
                </td>
                <td id="tdProjTotal" runat="server" class="textRightImp padRight15" sorttable_customkey='<%# Eval("ProjectedRemaining.Revenue.Value")%>'>
                    <span class="Revenue"><%# Eval("ProjectedRemaining.Revenue") %></span>
                </td>
                <td id="tdProjMargin" runat="server" class="textRightImp padRight15" sorttable_customkey='<%# Eval("ProjectedRemaining.Margin.Value")%>'>
                    <%# Eval("ProjectedRemaining.Margin") %>
                </td>
                <td id="tdProjMarginPer" runat="server" class="textRightImp padRight15">
                    <asp:Label ID="lblProjMarginPer" runat="server"></asp:Label>
                </td>

                <td id="tdEACRate" runat="server" class="borderLeftGrey textRightImp padRight15">
                    <%# Eval("EAC.BillRate") %>
                </td>
                <td id="tdEACMarginRate" runat="server" class="textRightImp padRight15">
                    <%# Eval("EAC.MarginRate") %>
                </td>
                <td id="tdEACHours" runat="server" class="textRightImp padRight15">
                    <%# Eval("EAC.Hours") %>
                </td>
                <td id="tdEACTotal" runat="server" class="textRightImp padRight15" sorttable_customkey='<%# Eval("EAC.Revenue.Value")%>'>
                    <span class="Revenue"><%# Eval("EAC.Revenue") %></span>
                </td>
                <td id="tdEACMargin" runat="server" class="textRightImp padRight15" sorttable_customkey='<%# Eval("EAC.Margin.Value")%>'>
                    <%# Eval("EAC.Margin") %>
                </td>
                <td id="tdEACMarginPer" runat="server" class="textRightImp padRight15">
                    <asp:Label ID="lblEACMarginPer" runat="server"></asp:Label>
                </td>

                <td id="tdDiffHours" runat="server" class="borderLeftGrey textRightImp padRight15">
                    <asp:Label ID="lblHoursDifference" runat="server"></asp:Label>
                </td>
                <td id="tdDiffTotal" runat="server" class="textRightImp padRight15">

                    <asp:Label ID="lblRevenueDifference" runat="server"></asp:Label>
                </td>
                <td id="tdDiffMargin" runat="server" class="textRightImp padRight15">
                    <asp:Label ID="lblMarginDiff" runat="server"></asp:Label>
                </td>
            </tr>
        </ItemTemplate>
        <FooterTemplate>
            <tr class="static fontBold border-Top">
                <td class="TextAlignLeftImp LeftPadding10px">Services Total </td>
                <td id="tdBudgetSummaryRate" runat="server" class="borderLeftGrey" colspan="2"></td>
                <td id="tdTotalBudgetHours" runat="server" class="textRightImp padRight15">
                    <asp:Label ID="lblTotalBudgetHours" runat="server"></asp:Label>
                </td>
                <td id="tdTotalBudgetRevenue" runat="server" class="textRightImp padRight15">
                    <span class="Revenue">
                        <asp:Label ID="lblTotalRevenue" runat="server"></asp:Label></span>
                </td>
                <td id="tdTotalBudgetMargin" runat="server" class="textRightImp padRight15">
                    <asp:Label ID="lblTotalBudgetMargin" runat="server"></asp:Label>
                </td>
                <td id="tdTotalBudgetMarginPer" runat="server" class="textRightImp padRight15">
                    <asp:Label ID="lblTotalBudgetMarginPer" runat="server"></asp:Label>
                </td>

                <td id="tdActSummaryRate" runat="server" class="borderLeftGrey" colspan="2"></td>
                <td id="tdActTotalHours" runat="server" class="textRightImp padRight15">
                    <asp:Label ID="lblTotalActHours" runat="server"></asp:Label>
                </td>
                <td id="tdActTotalRevenue" runat="server" class="textRightImp padRight15">
                    <span class="Revenue">
                        <asp:Label ID="lblTotalActRevenue" runat="server"></asp:Label></span>
                </td>
                <td id="tdTotalActMargin" runat="server" class="textRightImp padRight15">
                    <asp:Label ID="lblTotalActMargin" runat="server"></asp:Label>
                </td>
                <td id="tdTotalActMarginPer" runat="server" class="textRightImp padRight15">
                    <asp:Label ID="lblTotalActMarginPer" runat="server"></asp:Label>
                </td>

                <td id="tdProjSummaryRate" runat="server" class="borderLeftGrey" colspan="2"></td>
                <td id="tdProjTotalHours" runat="server" class="textRightImp padRight15">
                    <asp:Label ID="lblTotalProjHours" runat="server"></asp:Label>
                </td>
                <td id="tdProjTotalRevenue" runat="server" class="textRightImp padRight15">
                    <span class="Revenue">
                        <asp:Label ID="lblTotalProjRevenue" runat="server"></asp:Label></span>
                </td>
                <td id="tdTotalProjMargin" runat="server" class="textRightImp padRight15">
                    <asp:Label ID="lblTotalProjMargin" runat="server"></asp:Label>
                </td>
                <td id="tdTotalProjMarginPer" runat="server" class="textRightImp padRight15">
                    <asp:Label ID="lblTotalProjMarginPer" runat="server"></asp:Label>
                </td>


                <td id="tdEACSummaryRate" runat="server" class="borderLeftGrey" colspan="2"></td>
                <td id="tdEACTotalHours" runat="server" class="textRightImp padRight15">
                    <asp:Label ID="lblTotalEACHours" runat="server"></asp:Label>
                </td>
                <td id="tdEACTotalRevenue" runat="server" class="textRightImp padRight15">
                    <span class="Revenue">
                        <asp:Label ID="lblTotalEACRevenue" runat="server"></asp:Label></span>
                </td>
                <td id="tdEACTotalMargin" runat="server" class="textRightImp padRight15">
                    <asp:Label ID="lblEACTotalMargin" runat="server"></asp:Label>
                </td>
                <td id="tdEACTotalMarginPer" runat="server" class="textRightImp padRight15">
                    <asp:Label ID="lblEACTotalMarginPer" runat="server"></asp:Label>
                </td>

                <td id="tdDiffSummaryHours" runat="server" class="borderLeftGrey textRightImp padRight15">
                    <asp:Label ID="lblHoursDifferenceSummary" runat="server"></asp:Label>
                </td>
                <td id="tdDiffSummaryTotal" runat="server" class="textRightImp padRight15">
                    <asp:Label ID="lblRevenueDifferenceSummary" runat="server"></asp:Label>
                </td>
                <td id="tdDiffSummaryMargin" runat="server" class="textRightImp padRight15">
                    <asp:Label ID="lblMarginDifferenceSummary" runat="server"></asp:Label>
                </td>
            </tr>
            <tr class="static border-Top">
                <td colspan="28">&nbsp;</td>
            </tr>

            <tr class="static fontBold border-Top">
                <td class="TextAlignLeftImp LeftPadding10px">Project Expenses </td>
                <td id="tdBudgetEmptyExpense" runat="server" class="borderLeftGrey" colspan="3"></td>
                <td id="tdBudgetRevenueExpense" runat="server" class="textRightImp padRight15">
                    <asp:Label ID="lblBudgetRevenueExpense" runat="server"></asp:Label>
                </td>
                <td id="tdBudgetMarginExpense" runat="server" class="textRightImp padRight15">
                    <asp:Label ID="lblBudgetMarginExpense" runat="server"></asp:Label>
                </td>
                <td id="tdBudgetMarinExpense" runat="server"></td>


                <td id="tdActEmpty" runat="server" class="borderLeftGrey" colspan="3"></td>
                <td id="tdActExpense" runat="server" class="textRightImp padRight15">
                    <asp:Label ID="lblActExpense" runat="server"></asp:Label>
                </td>
                <td id="tdActMarginExpense" runat="server" class="textRightImp padRight15">
                    <asp:Label ID="lblActMarginExpense" runat="server"></asp:Label>
                </td>
                <td id="tdActEmptyMargin" runat="server"></td>

                <td id="tdProjEmpty" runat="server" class="borderLeftGrey" colspan="3"></td>
                <td id="tdProjExpense" runat="server" class="textRightImp padRight15">
                    <asp:Label ID="lblProjExpense" runat="server"></asp:Label>
                </td>
                <td id="tdProjMarginExpense" runat="server" class="textRightImp padRight15">
                    <asp:Label ID="lblProjMarginExpense" runat="server"></asp:Label>
                </td>
                <td id="tdProjEmptyMargin" runat="server"></td>

                <td id="tdEACEmpty" runat="server" class="borderLeftGrey" colspan="3"></td>
                <td id="tdEACExpense" runat="server" class="textRightImp padRight15">
                    <asp:Label ID="lblEACExpense" runat="server"></asp:Label>
                </td>
                <td id="tdEACMarginExpense" runat="server" class="textRightImp padRight15">
                    <asp:Label ID="lblEACMarginExpense" runat="server"></asp:Label>
                </td>
                <td id="tdEACEmptyMargin" runat="server"></td>

                <td id="tdDiffEmpty" runat="server" class="borderLeftGrey"></td>
                <td id="tdDiffExpense" runat="server" class="textRightImp padRight15">
                    <asp:Label ID="lblExpenseDiff" runat="server"></asp:Label>
                </td>
                <td id="tdDiffMarginExpense" runat="server" class="textRightImp padRight15">
                    <asp:Label ID="lblMarginExpense" runat="server"></asp:Label>
                </td>
            </tr>
            <tr class="static fontBold border-Top">
                <td class="TextAlignLeftImp LeftPadding10px">Total Expected Billing </td>
                <td id="tdEmptyBudgetTotal" runat="server" class="borderLeftGrey" colspan="3"></td>
                <td id="tdBudgetTotalRevenue" runat="server" class="textRightImp padRight15">
                    <asp:Label ID="lblBudgetTotal" runat="server"></asp:Label>
                </td>
                <td id="tdBudgetTotalMargin" runat="server" class="textRightImp padRight15">
                    <asp:Label ID="lblBudgetMargin" runat="server"></asp:Label>
                </td>
                <td id="tdBudgetMarginPer" runat="server" class="textRightImp padRight15">
                    <asp:Label ID="lblBudgetMarginPer" runat="server"></asp:Label>
                </td>

                <td id="tdActEmp" runat="server" class="borderLeftGrey" colspan="3"></td>
                <td id="tdActTotal" runat="server" class="textRightImp padRight15">
                    <asp:Label ID="lblActTotal" runat="server"></asp:Label>
                </td>
                <td id="tdActMargin" runat="server" class="textRightImp padRight15">
                    <asp:Label ID="lblActMargin" runat="server"></asp:Label>
                </td>
                <td id="tdActMarginPer" runat="server" class="textRightImp padRight15">
                    <asp:Label ID="lblActMarginPer" runat="server"></asp:Label>
                </td>

                <td id="tdProjEmp" runat="server" class="borderLeftGrey" colspan="3"></td>
                <td id="tdProjTotal" runat="server" class="textRightImp padRight15">
                    <asp:Label ID="lblProjTotal" runat="server"></asp:Label>
                </td>
                <td id="tdProjMargin" runat="server" class="textRightImp padRight15">
                    <asp:Label ID="lblProjMargin" runat="server"></asp:Label>
                </td>
                <td id="tdProjMarginPer" runat="server" class="textRightImp padRight15">
                    <asp:Label ID="lblProjMarginPer" runat="server"></asp:Label>
                </td>
                <td id="tdEACEmp" runat="server" class="borderLeftGrey" colspan="3"></td>
                <td id="tdEACTotal" runat="server" class="textRightImp padRight15">
                    <asp:Label ID="lblEACTotal" runat="server"></asp:Label>
                </td>
                <td id="tdEACMargin" runat="server" class="textRightImp padRight15">
                    <asp:Label ID="lblEACMargin" runat="server"></asp:Label>
                </td>
                <td id="tdEACMarginPer" runat="server" class="textRightImp padRight15">
                    <asp:Label ID="lblEACMarginPer" runat="server"></asp:Label>
                </td>
                <td id="tdDiffEmp" runat="server" class="borderLeftGrey"></td>
                <td id="tdDiffTotalRevenue" runat="server" class="textRightImp padRight15">
                    <asp:Label ID="lblDiffTotal" runat="server"></asp:Label>
                </td>
                <td id="tdDiffMargin" runat="server" class="textRightImp padRight15">
                    <asp:Label ID="lblDiffMargin" runat="server"></asp:Label>
                </td>
            </tr>
            </tbody></table></div>
        </FooterTemplate>
    </asp:Repeater>
</asp:Panel>

