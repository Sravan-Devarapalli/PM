using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using DataTransferObjects;
using PraticeManagement.Controls.ProjectExpenses;
using PraticeManagement.Controls.Generic.Buttons;

namespace PraticeManagement.Controls.Milestones
{
    public partial class MilestoneExpenses : System.Web.UI.UserControl
    {
        #region Constants

        private const string Tbeditname = "tbEditName";
        private const string Tbeditamount = "tbEditAmount";
        private const string Tbeditexpamount = "tbEditExpAmount";
        private const string Tbeditreimbursement = "tbEditReimbursement";

        private const string LblTotalamount = "lblTotalAmount";
        private const string LblTotalExpAmount = "lblTotalExpAmount";
        private const string LblTotalreimbursement = "lblTotalReimbursed";
        private const string LblTotalreimbursementamount = "lblTotalReimbursementAmount";
        private const string tbStartDate = "txtStartDate";
        private const string tbEndDate = "txtEndDate";
        private const string lockdownMessage = "Expenses tab was locked down by System Administrator for the dates on and before '{0}'.";
        private const string LblVariance = "lblVariance";

        #endregion

        public PraticeManagement.MilestoneDetail HostingPage
        {
            get { return ((PraticeManagement.MilestoneDetail)Page); }
        }

        public bool IsLockout
        {
            get;
            set;
        }

        public DateTime? LockoutDate
        {
            get;
            set;
        }

        public List<PeriodicalExpense> MonthlyExpense
        {
            get
            {
                List<PeriodicalExpense> _monthlyExpense;
                if (ViewState["MonthlyExcpense"] == null)
                {
                    _monthlyExpense = new List<PeriodicalExpense>();
                }
                else
                {
                    _monthlyExpense = (List<PeriodicalExpense>)ViewState["MonthlyExcpense"];
                }
                return _monthlyExpense;
            }
            set
            {
                ViewState["MonthlyExcpense"] = value;
            }


        }

        #region Fields

        private decimal _totalAmount;
        private decimal _totalExpectedAmount;
        private decimal _totalReimbursed;
        private decimal _totalReimbursementAmount;
        private int _expensesCount;
        private decimal _totalVariance;

        #endregion

        protected void Page_Load(object sender, EventArgs e)
        {
            lblActualError.Visible =
                lblEstiamatedError.Visible = false;
            LockdownExpenses();
        }

        protected void lnkAdd_OnClick(object sender, EventArgs e)
        {

            var footerRow = gvProjectExpenses.FooterRow;
            var ctrlName = footerRow.FindControl(Tbeditname) as TextBox;
            var ctrlAmount = footerRow.FindControl(Tbeditamount) as TextBox;
            var ctrlExpAmount = footerRow.FindControl(Tbeditexpamount) as TextBox;
            var ctrlReimb = footerRow.FindControl(Tbeditreimbursement) as TextBox;
            var txtStartDate = footerRow.FindControl(tbStartDate) as TextBox;
            var txtEndDate = footerRow.FindControl(tbEndDate) as TextBox;
            var startDate = DateTime.Parse(txtStartDate.Text);
            var endDate = DateTime.Parse(txtEndDate.Text);
            var ddlExpenseType = footerRow.FindControl("ddlExpense") as DropDownList;

            if (startDate > endDate)
            {
                CustomValidator cstEndShouldBeGreater = footerRow.FindControl("cstEndShouldBeGreater") as CustomValidator;
                cstEndShouldBeGreater.IsValid = false;
                return;
            }

            else if (HostingPage.Project.StartDate.HasValue && HostingPage.Project.EndDate.HasValue)
            {
                var isValid = true;
                if (startDate < HostingPage.Milestone.StartDate.Date ||
                    startDate > HostingPage.Milestone.EndDate.Date)
                {
                    CustomValidator cstStartDateShouldbewithinProjectPeriod = footerRow.FindControl("cstStartDateShouldbewithinProjectPeriod") as CustomValidator;
                    cstStartDateShouldbewithinProjectPeriod.IsValid = isValid = false;
                }
                if ((endDate < HostingPage.Milestone.StartDate.Date ||
                    endDate > HostingPage.Milestone.EndDate.Date) && isValid)
                {
                    CustomValidator cstEndDateShouldbewithinProjectPeriod = footerRow.FindControl("cstEndDateShouldbewithinProjectPeriod") as CustomValidator;
                    cstEndDateShouldbewithinProjectPeriod.IsValid = isValid = false;
                }
                if (isValid && IsLockout)
                {
                    CustomValidator custLockdown = footerRow.FindControl("custLockdown") as CustomValidator;
                    if (!ValidateLockdown(custLockdown, true, true, false, false, false))
                    {
                        custLockdown.IsValid = isValid = false;
                        custLockdown.ErrorMessage = custLockdown.ToolTip = string.Format(lockdownMessage, LockoutDate.Value.ToShortDateString());
                    }
                }
                if (!isValid)
                {
                    return;
                }
            }
            int monthsCount = Math.Abs((endDate.Month - startDate.Month) + 12 * (endDate.Year - startDate.Year));
            if (monthsCount > 0)
            {
                lblExpenseName.Text = ctrlName.Text;
                lblPeriod.Text = startDate.Date.ToShortDateString() + "-" + endDate.Date.ToShortDateString();
                decimal monthlyEstimatedExp = Convert.ToDecimal(ctrlExpAmount.Text) / (monthsCount + 1);
                decimal? monthlyActualExp = ctrlAmount.Text.Length > 0 ? (decimal?)Convert.ToDecimal(ctrlAmount.Text) / (monthsCount + 1) : null;
                List<PeriodicalExpense> list = new List<PeriodicalExpense>();

                for (int i = 0; i <= monthsCount; i++)
                {
                    PeriodicalExpense temp = new PeriodicalExpense();
                    temp.StartDate = i == 0 ? startDate : Utils.Calendar.MonthStartDate(startDate.AddMonths(i));
                    temp.EndDate = i == monthsCount ? endDate : Utils.Calendar.MonthEndDate(temp.StartDate);
                    temp.EstimatedExpense = monthlyEstimatedExp;
                    temp.ActualExpense = monthlyActualExp;
                    list.Add(temp);
                }
                btnSave.Attributes.Add("row", "footer");
                btnSave.Enabled = list.Count() > 0 ? true : false;
                repMonthlyExpense.DataSource = list;
                repMonthlyExpense.DataBind();

                mpeMonthlyExpense.Show();
                return;
            }
            else
            {
                SaveExpense(true, false);
            }

        }

        protected void gvProjectExpenses_OnRowDataBound(object sender, GridViewRowEventArgs e)
        {
            var row = e.Row;
            switch (row.RowType)
            {
                case DataControlRowType.DataRow:
                    var expense = row.DataItem as ProjectExpense;
                    var lnkEdit = row.FindControl("lnkEdit") as LinkButton;
                    var lnkDelete = row.FindControl("lnkDelete") as LinkButton;

                    if (gvProjectExpenses.EditIndex == row.DataItemIndex)
                    {
                        var ddlExpenseType = row.FindControl("ddlExpenseType") as DropDownList;
                        var tbEditAmount = row.FindControl("tbEditAmount") as TextBox;
                        var tbEditExpAmount = row.FindControl("tbEditExpAmount") as TextBox;
                        DataHelper.FillExpenseTypeList(ddlExpenseType);
                        ddlExpenseType.SelectedValue = expense.Type.Id.ToString();
                        int monthsCount = Math.Abs((expense.EndDate.Month - expense.StartDate.Month) + 12 * (expense.EndDate.Year - expense.StartDate.Year));
                        if (monthsCount > 0)
                        {
                            tbEditAmount.Enabled = tbEditExpAmount.Enabled = false;
                        }
                    }

                    if (expense != null)
                    {
                        _totalAmount += expense.Amount != null ? expense.Amount.Value : 0;
                        _totalReimbursed += expense.Reimbursement;
                        _totalReimbursementAmount += expense.ReimbursementAmount;
                        _totalExpectedAmount += expense.ExpectedAmount;
                        _totalVariance += expense.Difference;
                        _expensesCount++;

                        // Hide rows with null values.
                        // These are special rows that are used not to show
                        //      empty data grid message
                        if (!expense.Id.HasValue)
                            row.Visible = false;
                        if (IsLockout)
                        {
                            if (expense.EndDate <= LockoutDate)
                            {
                                lnkEdit.Enabled = lnkDelete.Enabled = false;
                                lnkDelete.OnClientClick = null;
                            }
                            if (expense.StartDate <= LockoutDate)
                            {
                                lnkDelete.Enabled = false;
                                lnkDelete.OnClientClick = null;
                            }
                        }
                    }
                    if (lnkEdit != null && lnkDelete != null)
                    {
                        lnkEdit.Visible = lnkDelete.Visible = (HostingPage.tierOneExceptionStatus != 1 && HostingPage.tierTwoExceptionStatus != 1);
                    }
                    break;

                case DataControlRowType.Footer:
                    SetRowValue(row, LblTotalamount, _totalAmount);
                    SetRowValue(row, LblTotalExpAmount, _totalExpectedAmount);
                    SetRowValue(row, LblTotalreimbursement, string.Format("{0:0}%", (_totalReimbursed / _expensesCount)));
                    SetRowValue(row, LblTotalreimbursementamount, _totalReimbursementAmount);
                    SetRowValue(row, LblVariance, _totalVariance);
                    var ddlExpense = row.FindControl("ddlExpense") as DropDownList;
                    var btnAddExpense = row.FindControl("btnAddExpense") as ShadowedTextButton;
                    btnAddExpense.Visible = (HostingPage.tierOneExceptionStatus != 1 && HostingPage.tierTwoExceptionStatus != 1);
                    DataHelper.FillExpenseTypeList(ddlExpense);
                    break;
            }
        }

        private static void SetRowValue(Control row, string ctrlName, string text)
        {
            var totalAmountCtrl = row.FindControl(ctrlName) as Label;
            if (totalAmountCtrl != null)
                totalAmountCtrl.Text = text;
        }

        private static void SetRowValue(Control row, string ctrlName, decimal number)
        {
            SetRowValue(row, ctrlName, ((PracticeManagementCurrency)number).ToString());
        }

        protected void gvProjectExpenses_OnRowUpdating(object sender, GridViewUpdateEventArgs e)
        {

            Page.Validate("ProjectExpensesEdit");
            if (!Page.IsValid)
            {
                e.Cancel = true;
            }
            else
            {
                var grid = sender as GridView;
                int rowIndex = grid.EditIndex;
                //GridViewRow row = sender as 
                string newStartDate = e.NewValues["StartDate"].ToString().Trim();
                string newEndDate = e.NewValues["EndDate"].ToString().Trim();
                string oldStartDate = e.OldValues["StartDate"].ToString().Trim();
                string oldEndDate = e.OldValues["EndDate"].ToString().Trim();
                string oldAmount = e.OldValues["Amount"] != null ? e.OldValues["Amount"].ToString().Trim() : string.Empty;
                string newAmount = e.NewValues["Amount"] != null ? e.NewValues["Amount"].ToString().Trim() : string.Empty;
                string oldReimbursement = e.OldValues["Reimbursement"].ToString().Trim();
                string newReimbursement = e.NewValues["Reimbursement"].ToString().Trim();
                string oldName = e.OldValues["Name"].ToString().Trim();
                string newName = e.NewValues["Name"].ToString().Trim();
                string newEstAmount = e.NewValues["ExpectedAmount"].ToString().Trim();

                var startDate = Convert.ToDateTime(newStartDate);
                var endDate = Convert.ToDateTime(newEndDate);

                var ddlExpenseType = gvProjectExpenses.Rows[e.RowIndex].FindControl("ddlExpenseType") as DropDownList;
                if (ddlExpenseType != null)
                {
                    var type = new ExpenseType()
                    {
                        Id = Convert.ToInt32(ddlExpenseType.SelectedValue),
                        Name = ddlExpenseType.SelectedItem.Text
                    };
                    e.NewValues["Type"] = type;
                }

                if (newStartDate != oldStartDate || newEndDate != oldEndDate)
                {

                    if (startDate > endDate)
                    {
                        CustomValidator cstEndShouldBeGreater = gvProjectExpenses.Rows[e.RowIndex].FindControl("cstEndShouldBeGreater") as CustomValidator;
                        cstEndShouldBeGreater.IsValid = false;
                        e.Cancel = true;
                    }
                    else
                    {
                        if (HostingPage.Project.StartDate.HasValue && HostingPage.Project.EndDate.HasValue)
                        {
                            var isValid = true;

                            if ((startDate < HostingPage.Milestone.StartDate.Date ||
                                startDate > HostingPage.Milestone.EndDate.Date))
                            {
                                CustomValidator cstStartDateShouldbewithinProjectPeriod = gvProjectExpenses.Rows[e.RowIndex].FindControl("cstStartDateShouldbewithinProjectPeriod") as CustomValidator;
                                cstStartDateShouldbewithinProjectPeriod.IsValid = isValid = false;
                            }
                            if ((endDate < HostingPage.Milestone.StartDate.Date ||
                               endDate > HostingPage.Milestone.EndDate.Date) && isValid)
                            {
                                CustomValidator cstEndDateShouldbewithinProjectPeriod = gvProjectExpenses.Rows[e.RowIndex].FindControl("cstEndDateShouldbewithinProjectPeriod") as CustomValidator;
                                cstEndDateShouldbewithinProjectPeriod.IsValid = isValid = false;
                            }
                            if (!isValid)
                            {
                                e.Cancel = true;
                                return;
                            }
                        }
                    }
                }
                if (newStartDate != oldStartDate || newEndDate != oldEndDate || oldName != newName || oldAmount != newAmount || oldReimbursement != newReimbursement)
                {
                    var isValid = true;

                    if (isValid && IsLockout)
                    {
                        CustomValidator custLockdown = gvProjectExpenses.Rows[e.RowIndex].FindControl("custLockdown") as CustomValidator;
                        if (!ValidateLockdown(custLockdown, newStartDate != oldStartDate, newEndDate != oldEndDate, oldName != newName, oldAmount != newAmount, oldReimbursement != newReimbursement))
                        {
                            custLockdown.IsValid = isValid = false;
                            custLockdown.ErrorMessage = custLockdown.ToolTip = string.Format(lockdownMessage, LockoutDate.Value.ToShortDateString());
                        }
                    }
                    if (!isValid)
                    {
                        e.Cancel = true;
                        return;
                    }
                }
                int monthsCount = Math.Abs((endDate.Month - startDate.Month) + 12 * (endDate.Year - startDate.Year));
                if (monthsCount > 0)
                {
                    lblExpenseName.Text = newName;
                    lblPeriod.Text = startDate.Date.ToShortDateString() + "-" + endDate.Date.ToShortDateString();
                    decimal monthlyEstimatedExp = Convert.ToDecimal(newEstAmount) / (monthsCount + 1);
                    decimal? monthlyActualExp = newAmount.Length > 0 ? (decimal?)Convert.ToDecimal(newAmount) / (monthsCount + 1) : null;
                    List<PeriodicalExpense> list = new List<PeriodicalExpense>();

                    if (oldStartDate != newStartDate || oldEndDate != newEndDate)
                    {
                        for (int i = 0; i <= monthsCount; i++)
                        {
                            PeriodicalExpense temp = new PeriodicalExpense();
                            temp.StartDate = i == 0 ? startDate : Utils.Calendar.MonthStartDate(startDate.AddMonths(i));
                            temp.EndDate = i == monthsCount ? endDate : Utils.Calendar.MonthEndDate(temp.StartDate);
                            temp.EstimatedExpense = monthlyEstimatedExp;
                            temp.ActualExpense = monthlyActualExp;
                            list.Add(temp);
                        }
                    }
                    else
                    {
                        int expenseId;
                        int.TryParse((gvProjectExpenses.Rows[e.RowIndex].FindControl("tbEditName") as TextBox).Attributes["ExpenseId"], out expenseId);
                        list = ServiceCallers.Custom.Milestone(m => m.GetMonthlyExpensesByExpenseId(expenseId)).ToList();
                    }
                    btnSave.Attributes.Add("row", rowIndex.ToString());
                    btnSave.Enabled = list.Count() > 0 ? true : false;
                    repMonthlyExpense.DataSource = list;
                    repMonthlyExpense.DataBind();

                    mpeMonthlyExpense.Show();
                    e.Cancel = true;
                    return;
                }

            }
        }

        public bool ValidateLockdown(object sender, bool isStartDateChanged, bool isEndDateChanged, bool isNameChanged, bool isExpenseChanged, bool isReimberseChanged)
        {
            bool isValid = true;
            var source = sender as CustomValidator;
            var gridRow = source.NamingContainer as GridViewRow;
            var txtEndDate = gridRow.FindControl("txtEndDate") as TextBox;
            var txtStartDate = gridRow.FindControl("txtStartDate") as TextBox;
            if (!string.IsNullOrEmpty(txtEndDate.Text) && !string.IsNullOrEmpty(txtStartDate.Text))
            {
                var endDate = DateTime.Parse(txtEndDate.Text);
                var startDate = DateTime.Parse(txtStartDate.Text);
                if ((isEndDateChanged && endDate.Date <= LockoutDate.Value.Date) || (isStartDateChanged && startDate.Date <= LockoutDate.Value.Date) || (isNameChanged && startDate.Date <= LockoutDate.Value.Date) || (isExpenseChanged && startDate.Date <= LockoutDate.Value.Date) || (isReimberseChanged && startDate.Date <= LockoutDate.Value.Date))
                {
                    isValid = false;
                }
            }
            return isValid;
        }

        protected void odsProjectExpenses_OnSelecting(object sender, ObjectDataSourceSelectingEventArgs e)
        {
            if (HostingPage.MilestoneId.HasValue)
            {
                e.InputParameters["milestoneId"] = HostingPage.MilestoneId.Value;
            }
        }

        public void LockdownExpenses()
        {
            DataTransferObjects.Lockout expenseItem = new DataTransferObjects.Lockout();
            using (var service = new ConfigurationService.ConfigurationServiceClient())
            {
                List<DataTransferObjects.Lockout> projectdetailItems = service.GetLockoutDetails((int)LockoutPages.Projectdetail).ToList();
                expenseItem = projectdetailItems.FirstOrDefault(p => p.Name == "Expenses");
                IsLockout = expenseItem.IsLockout;
                LockoutDate = expenseItem.LockoutDate;
            }
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            Page.Validate("MonthlyExpenses");
            if (Page.IsValid)
            {

                var btnSave = sender as Button;
                var isNewEntry = btnSave.Attributes["row"] == "footer";

                SaveExpense(isNewEntry, true);
            }
            else
            {

                mpeMonthlyExpense.Show();
            }
        }

        private void SaveExpense(bool isNewEntry, bool isMonthlyExpense)
        {

            TextBox ctrlName, ctrlAmount, ctrlExpAmount, ctrlReimb, txtStartDate, txtEndDate;
            DropDownList ddlExpenseType;
            DateTime startDate, endDate;

            List<PeriodicalExpense> monthExpense = null;
            if (isMonthlyExpense)
            {
                monthExpense = new List<PeriodicalExpense>();


                foreach (RepeaterItem item in repMonthlyExpense.Items)
                {
                    if (item.ItemType == ListItemType.Item || item.ItemType == ListItemType.AlternatingItem)
                    {
                        var month = item.FindControl("lblMonth") as Label;
                        var estExp = item.FindControl("txtEstExpense") as TextBox;
                        var actExp = item.FindControl("txtActExpense") as TextBox;

                        PeriodicalExpense temp = new PeriodicalExpense();
                        temp.StartDate = Convert.ToDateTime(month.Attributes["StartDate"]);
                        temp.EndDate = Convert.ToDateTime(month.Attributes["EndDate"]);
                        temp.Id = !string.IsNullOrEmpty(month.Attributes["ExpenseId"]) ? (int?)Convert.ToInt32(month.Attributes["ExpenseId"]) : null;
                        temp.ProjectExpenseId = !string.IsNullOrEmpty(month.Attributes["ProjectExpenseId"]) ? (int?)Convert.ToInt32(month.Attributes["ProjectExpenseId"]) : null;
                        temp.EstimatedExpense = Convert.ToDecimal(estExp.Text);
                        temp.ActualExpense = actExp.Text.Length > 0 ? (decimal?)Convert.ToDecimal(actExp.Text) : null;
                        monthExpense.Add(temp);
                    }
                }

            }


            if (isNewEntry)
            {
                var footerRow = gvProjectExpenses.FooterRow;
                ctrlName = footerRow.FindControl(Tbeditname) as TextBox;
                ctrlAmount = footerRow.FindControl(Tbeditamount) as TextBox;
                ctrlExpAmount = footerRow.FindControl(Tbeditexpamount) as TextBox;
                ctrlReimb = footerRow.FindControl(Tbeditreimbursement) as TextBox;
                txtStartDate = footerRow.FindControl(tbStartDate) as TextBox;
                txtEndDate = footerRow.FindControl(tbEndDate) as TextBox;
                startDate = DateTime.Parse(txtStartDate.Text);
                endDate = DateTime.Parse(txtEndDate.Text);
                ddlExpenseType = footerRow.FindControl("ddlExpense") as DropDownList;

                if (monthExpense != null)
                {
                    var estSum = monthExpense.Sum(m => m.EstimatedExpense);
                    var actSum = monthExpense.Sum(m => m.ActualExpense);
                    var estimatedAmount = Convert.ToDecimal(ctrlExpAmount.Text);
                    ctrlAmount.Text = actSum.ToString();
                    cvEstimatedSum.IsValid = (estSum == estimatedAmount) || (Math.Abs(estSum - estimatedAmount) < 1);
                    if (!cvEstimatedSum.IsValid )
                    {
                        lblEstiamatedError.Visible = !cvEstimatedSum.IsValid;
                        mpeMonthlyExpense.Show();
                        return;
                    }
                }

                if (ctrlName != null && ctrlAmount != null && ctrlReimb != null)
                {
                    ProjectExpenseHelper.AddProjectExpense(
                    new ProjectExpense
                    {
                        Name = ctrlName.Text,
                        Amount = ctrlAmount.Text.Length > 0 ? (decimal?)Convert.ToDecimal(ctrlAmount.Text) : null,
                        ExpectedAmount = Convert.ToDecimal(ctrlExpAmount.Text),
                        Reimbursement = Convert.ToDecimal(ctrlReimb.Text),
                        StartDate = startDate,
                        EndDate = endDate,
                        ProjectId = Convert.ToInt32(HostingPage.Project.Id.Value.ToString()),
                        Milestone = new Milestone()
                        {
                            Id = Convert.ToInt32(HostingPage.MilestoneId.Value.ToString())
                        },
                        Type = new ExpenseType()
                        {
                            Id = Convert.ToInt32(ddlExpenseType.SelectedValue)
                        },
                        MonthlyExpense = monthExpense
                    }
                    );
                }

                gvProjectExpenses.DataBind();
            }

            else
            {
                ProjectExpense expense = new ProjectExpense();
                expense.MonthlyExpense = monthExpense;
                if (gvProjectExpenses.EditIndex > -1)
                {
                    var editRow = gvProjectExpenses.Rows[gvProjectExpenses.EditIndex];

                    ctrlName = editRow.FindControl(Tbeditname) as TextBox;
                    ctrlAmount = editRow.FindControl(Tbeditamount) as TextBox;
                    ctrlExpAmount = editRow.FindControl(Tbeditexpamount) as TextBox;
                    ctrlReimb = editRow.FindControl(Tbeditreimbursement) as TextBox;
                    txtStartDate = editRow.FindControl(tbStartDate) as TextBox;
                    txtEndDate = editRow.FindControl(tbEndDate) as TextBox;
                    startDate = DateTime.Parse(txtStartDate.Text);
                    endDate = DateTime.Parse(txtEndDate.Text);
                    ddlExpenseType = editRow.FindControl("ddlExpenseType") as DropDownList;
                    var expenseId = gvProjectExpenses.DataKeys[gvProjectExpenses.EditIndex].Value;

                    expense.Id = Convert.ToInt32(expenseId);
                    expense.Name = ctrlName.Text;
                    expense.Amount = ctrlAmount.Text.Length > 0 ? (decimal?)Convert.ToDecimal(ctrlAmount.Text) : null;
                    expense.ExpectedAmount = Convert.ToDecimal(ctrlExpAmount.Text);
                    expense.Reimbursement = Convert.ToDecimal(ctrlReimb.Text);
                    expense.StartDate = startDate;
                    expense.EndDate = endDate;
                    expense.Milestone = new Milestone()
                    {
                        Id = Convert.ToInt32(HostingPage.MilestoneId.Value.ToString())
                    };
                    expense.Type = new ExpenseType()
                    {
                        Id = Convert.ToInt32(ddlExpenseType.SelectedValue)
                    };
                    if (monthExpense != null)
                    {
                        var estSum = monthExpense.Sum(m => m.EstimatedExpense);
                        var actSum = monthExpense.Sum(m => m.ActualExpense);
                        expense.Amount = actSum;
                        expense.ExpectedAmount = estSum;
                 
                        if (!cvEstimatedSum.IsValid )
                        {
                            mpeMonthlyExpense.Show();
                            return;
                        }
                    }

                    ProjectExpenseHelper.UpdateProjectExpense(expense);
                    gvProjectExpenses.EditIndex = -1;
                }
                else
                {
                    ServiceCallers.Custom.Milestone(m => m.SaveMonthlyExpenses(monthExpense.First().ProjectExpenseId.Value, monthExpense.ToArray()));
                    gvProjectExpenses.DataBind();
                }
            }
        }

        protected void lnkExpense_OnClick(object sender, EventArgs e)
        {
            btnSave.Attributes["row"] = "";
            var lnkExpense = sender as LinkButton;
            int expenseId;
            int.TryParse(lnkExpense.Attributes["ExpenseId"], out expenseId);
            hdnEstAmount.Value = lnkExpense.Attributes["EstAmount"];
            hdnActAmount.Value = lnkExpense.Attributes["ActAmount"];
            var monthlyExpense = ServiceCallers.Custom.Milestone(m => m.GetMonthlyExpensesByExpenseId(expenseId));
            lblExpenseName.Text = lnkExpense.Text;
            lblPeriod.Text = Convert.ToDateTime(lnkExpense.Attributes["StartDate"]).ToShortDateString() + "-" + Convert.ToDateTime(lnkExpense.Attributes["EndDate"]).ToShortDateString();
            btnSave.Enabled = monthlyExpense.Count() > 0 ? true : false;
            repMonthlyExpense.DataSource = monthlyExpense;
            repMonthlyExpense.DataBind();

            mpeMonthlyExpense.Show();
        }
    }
}



