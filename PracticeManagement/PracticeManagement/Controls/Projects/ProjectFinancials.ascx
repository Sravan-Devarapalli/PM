<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ProjectFinancials.ascx.cs"
    Inherits="PraticeManagement.Controls.Projects.ProjectFinancials" %>
<asp:Panel ID="pnlFinancials" runat="server" CssClass="tab-pane">

    <table class="alterrow CompPerfTable">
        <tr class="ie-bg">
            <th>&nbsp;
            </th>
            <th class="padLeft20 borderLeftGrey">Budget
            </th>
            <th>&nbsp;
            </th>
            <th class="padLeft20 borderLeftGrey">Projected<br />
                (Total)
            </th>
            <th>&nbsp;
            </th>
            <th class="borderLeftGrey">Actuals<br />
                (Last Month End)
            </th>
            <th>&nbsp;
            </th>
            <th class="padLeft20 borderLeftGrey">Projected<br />
                (remaining)
            </th>
            <th>&nbsp;
            </th>
            <th class="borderLeftGrey">ETC<br />
                (Actuals + Projected Remaining)
            </th>
            <th>&nbsp;
            </th>
            <th class="borderLeftGrey">Budget to ETC Variance
            </th>

        </tr>
        <tr>
            <td class="Padding2">Services Revenue
            </td>
            <td class="textRightImp borderLeftGrey">
                <asp:Label ID="lblBudgetRevenue" runat="server" CssClass="Revenue">-</asp:Label>
            </td>
            <td>&nbsp;
            </td>
            <td class="textRightImp borderLeftGrey">
                <asp:Label ID="lblEstimatedRevenue" runat="server" CssClass="Revenue">-</asp:Label>
            </td>
            <td>&nbsp;
            </td>
            <td class="textRightImp borderLeftGrey">
                <asp:Label ID="lblActualRevenue" runat="server" CssClass="Revenue">-</asp:Label>
            </td>
            <td>&nbsp;
            </td>
            <td class="textRightImp borderLeftGrey">
                <asp:Label ID="lblProjRemRevenue" runat="server" CssClass="Revenue">-</asp:Label>
            </td>
            <td>&nbsp;
            </td>
            <td class="textRightImp borderLeftGrey">
                <asp:Label ID="lblEACRevenue" runat="server" CssClass="Revenue">-</asp:Label>
            </td>
            <td>&nbsp;
            </td>
            <td class="textRightImp borderLeftGrey">
                <asp:Label ID="lblVarcRevenue" runat="server" CssClass="Revenue">-</asp:Label>
            </td>

        </tr>
        <tr>
            <td class="Padding2">Reimbursed Expenses
            </td>
            <td class="textRightImp borderLeftGrey">
                <asp:Label ID="lblBudgetReimExpenses" runat="server" Font-Bold="true">-</asp:Label>
            </td>
            <td>&nbsp;
            </td>
            <td class="textRightImp borderLeftGrey">
                <asp:Label ID="lblReimbursedExpenses" runat="server" Font-Bold="true">-</asp:Label>
            </td>
            <td>&nbsp;
            </td>
            <td class="textRightImp borderLeftGrey">
                <asp:Label ID="lblActualReimExpenses" runat="server" Font-Bold="true">-</asp:Label>
            </td>
            <td>&nbsp;
            </td>
            <td class="textRightImp borderLeftGrey">
                <asp:Label ID="lblProjRemReimExpenses" runat="server" Font-Bold="true">-</asp:Label>
            </td>
            <td>&nbsp;
            </td>
            <td class="textRightImp borderLeftGrey">
                <asp:Label ID="lblEACReimExpenses" runat="server" Font-Bold="true">-</asp:Label>
            </td>
            <td>&nbsp;
            </td>
            <td class="textRightImp borderLeftGrey">
                <asp:Label ID="lblVarcReimExpenses" runat="server" Font-Bold="true">-</asp:Label>
            </td>

        </tr>
        <tr>
            <td class="Padding2">Account Discount (<asp:Label ID="lblDiscount" runat="server"></asp:Label>%)
            </td>
            <td class="textRightImp borderLeftGrey">
                <asp:Label ID="lblBudgetDiscount" CssClass="Revenue" runat="server">-</asp:Label>
            </td>
            <td>&nbsp;
            </td>
            <td class="textRightImp borderLeftGrey">
                <asp:Label ID="lblDiscountAmount" CssClass="Revenue" runat="server">-</asp:Label>
            </td>
            <td>&nbsp;
            </td>
            <td class="textRightImp borderLeftGrey">
                <asp:Label ID="lblActDiscount" CssClass="Revenue" runat="server">-</asp:Label>
            </td>
            <td>&nbsp;
            </td>
            <td class="textRightImp borderLeftGrey">
                <asp:Label ID="lblProjRemDiscount" CssClass="Revenue" runat="server">-</asp:Label>
            </td>
            <td>&nbsp;
            </td>
            <td class="textRightImp borderLeftGrey">
                <asp:Label ID="lblEACDiscount" CssClass="Revenue" runat="server">-</asp:Label>
            </td>
            <td>&nbsp;
            </td>
            <td class="textRightImp borderLeftGrey">
                <asp:Label ID="lblVarcDiscount" CssClass="Revenue" runat="server">-</asp:Label>
            </td>

        </tr>
        <tr>
            <td class="Padding2">Revenue net of discounts
            </td>
            <td class="textRightImp borderLeftGrey">
                <asp:Label ID="lblBudgetRevenueNet" CssClass="Revenue" runat="server">-</asp:Label>
            </td>
            <td>&nbsp;
            </td>
            <td class="textRightImp borderLeftGrey">
                <asp:Label ID="lblRevenueNet" CssClass="Revenue" runat="server">-</asp:Label>
            </td>
            <td>&nbsp;
            </td>
            <td class="textRightImp borderLeftGrey">
                <asp:Label ID="lblActRevenueNet" CssClass="Revenue" runat="server">-</asp:Label>
            </td>
            <td>&nbsp;
            </td>
            <td class="textRightImp borderLeftGrey">
                <asp:Label ID="lblProjRemRevenueNet" CssClass="Revenue" runat="server">-</asp:Label>
            </td>
            <td>&nbsp;
            </td>
            <td class="textRightImp borderLeftGrey">
                <asp:Label ID="lblEACRevenueNet" CssClass="Revenue" runat="server">-</asp:Label>
            </td>
            <td>&nbsp;
            </td>
            <td class="textRightImp borderLeftGrey">
                <asp:Label ID="lblVarRevenueNet" CssClass="Revenue" runat="server">-</asp:Label>
            </td>


        </tr>
        <tr>
            <td class="Padding2">Resource Expenses
            </td>
            <td class="textRightImp borderLeftGrey">
                <asp:Label ID="lblBudgetCogs" runat="server" CssClass="Cogs">-</asp:Label>
            </td>
            <td>&nbsp;
            </td>
            <td class="textRightImp borderLeftGrey">
                <asp:Label ID="lblEstimatedCogs" runat="server" CssClass="Cogs">-</asp:Label>
            </td>
            <td>&nbsp;
            </td>
            <td class="textRightImp borderLeftGrey">
                <asp:Label ID="lblActCogs" runat="server" CssClass="Cogs">-</asp:Label>
            </td>
            <td>&nbsp;
            </td>
            <td class="textRightImp borderLeftGrey">
                <asp:Label ID="lblProjRemCogs" runat="server" CssClass="Cogs">-</asp:Label>
            </td>
            <td>&nbsp;
            </td>
            <td class="textRightImp borderLeftGrey">
                <asp:Label ID="lblEACCogs" runat="server" CssClass="Cogs">-</asp:Label>
            </td>
            <td>&nbsp;
            </td>
            <td class="textRightImp borderLeftGrey">
                <asp:Label ID="lblVarcCogs" runat="server" CssClass="Cogs">-</asp:Label>
            </td>


        </tr>
        <tr>
            <td class="Padding2">Other Expenses
            </td>
            <td class="textRightImp borderLeftGrey">
                <asp:Label ID="lblBudgetExpense" runat="server" Font-Bold="true">-</asp:Label>
            </td>
            <td>&nbsp;
            </td>
            <td class="textRightImp borderLeftGrey">
                <asp:Label ID="lblExpenses" runat="server" Font-Bold="true">-</asp:Label>
            </td>
            <td>&nbsp;
            </td>
            <td class="textRightImp borderLeftGrey">
                <asp:Label ID="lblActExpenses" runat="server" Font-Bold="true">-</asp:Label>
            </td>
            <td>&nbsp;
            </td>
            <td class="textRightImp borderLeftGrey">
                <asp:Label ID="lblProjRemExpenses" runat="server" Font-Bold="true">-</asp:Label>
            </td>
            <td>&nbsp;
            </td>
            <td class="textRightImp borderLeftGrey">
                <asp:Label ID="lblEACExpenses" runat="server" Font-Bold="true">-</asp:Label>
            </td>
            <td>&nbsp;
            </td>
            <td class="textRightImp borderLeftGrey">
                <asp:Label ID="lblVarcExpenses" runat="server" Font-Bold="true">-</asp:Label>
            </td>

        </tr>
        <tr>
            <td class="Padding2">Total Expenses
            </td>
            <td class="textRightImp borderLeftGrey">
                <asp:Label ID="lblBudgetTotalExpenes" runat="server" Font-Bold="true">-</asp:Label>
            </td>
            <td>&nbsp;
            </td>
            <td class="textRightImp borderLeftGrey">
                <asp:Label ID="lblTotalExpenses" runat="server" Font-Bold="true">-</asp:Label>
            </td>
            <td>&nbsp;
            </td>
            <td class="textRightImp borderLeftGrey">
                <asp:Label ID="lblActTotalExpenses" runat="server" Font-Bold="true">-</asp:Label>
            </td>
            <td>&nbsp;
            </td>
            <td class="textRightImp borderLeftGrey">
                <asp:Label ID="lblProjRemTotalExpenses" runat="server" Font-Bold="true">-</asp:Label>
            </td>
            <td>&nbsp;
            </td>
            <td class="textRightImp borderLeftGrey">
                <asp:Label ID="lblEACTotalExpenses" runat="server" Font-Bold="true">-</asp:Label>
            </td>
            <td>&nbsp;
            </td>
            <td class="textRightImp borderLeftGrey">
                <asp:Label ID="lblVarcTotalExpenses" runat="server" Font-Bold="true">-</asp:Label>
            </td>

        </tr>
        <tr>
            <td class="Padding2">Contribution Margin
            </td>
            <td class="textRightImp borderLeftGrey">
                <asp:Label ID="lblBudgetGrossMargin" CssClass="Margin" runat="server">-</asp:Label>
            </td>
            <td>&nbsp;
            </td>
            <td class="textRightImp borderLeftGrey">
                <asp:Label ID="lblGrossMargin" CssClass="Margin" runat="server">-</asp:Label>
            </td>
            <td>&nbsp;
            </td>
            <td class="textRightImp borderLeftGrey">
                <asp:Label ID="lblActGrossMargin" CssClass="Margin" runat="server">-</asp:Label>
            </td>
            <td>&nbsp;
            </td>
            <td class="textRightImp borderLeftGrey">
                <asp:Label ID="lblProjRemGrossMargin" CssClass="Margin" runat="server">-</asp:Label>
            </td>
            <td>&nbsp;
            </td>
            <td class="textRightImp borderLeftGrey">
                <asp:Label ID="lblEACGrossMargin" CssClass="Margin" runat="server">-</asp:Label>
            </td>
            <td>&nbsp;
            </td>
            <td class="textRightImp borderLeftGrey">
                <asp:Label ID="lblVarcGrossMargin" CssClass="Margin" runat="server">-</asp:Label>
            </td>

        </tr>
        <tr>
            <td class="Padding2">Contribution Margin%
            </td>
            <td class="textRightImp borderLeftGrey">
                <asp:Label ID="lblBudgetMarginPerc" runat="server">-</asp:Label>
            </td>
            <td>&nbsp;
            </td>
            <td class="textRightImp borderLeftGrey">
                <asp:Label ID="lblMarginPerc" runat="server">-</asp:Label>
            </td>
            <td>&nbsp;
            </td>
            <td class="textRightImp borderLeftGrey">
                <asp:Label ID="lblActMarginPerc" runat="server">-</asp:Label>
            </td>
            <td>&nbsp;
            </td>
            <td class="textRightImp borderLeftGrey">
                <asp:Label ID="lblProjRemMarginPer" runat="server">-</asp:Label>
            </td>
            <td>&nbsp;
            </td>
            <td class="textRightImp borderLeftGrey">
                <asp:Label ID="lblEACMarginPerc" runat="server">-</asp:Label>
            </td>
            <td>&nbsp;
            </td>
            <td class="textRightImp borderLeftGrey">
                <asp:Label ID="lblVarcMarginPerc" runat="server">-</asp:Label>
            </td>

        </tr>
    </table>

</asp:Panel>

