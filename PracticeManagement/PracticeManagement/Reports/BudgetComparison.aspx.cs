using DataTransferObjects;
using DataTransferObjects.Filters;
using DataTransferObjects.Reports;
using NPOI.SS.UserModel;
using PraticeManagement.Controls;
using PraticeManagement.Utils;
using PraticeManagement.Utils.Excel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace PraticeManagement.Reports
{
    public partial class BudgetComparision : System.Web.UI.Page
    {

        private const string CurrencyDisplayFormat = "$###,###,###,###,###,##0";
        private int headerRowsCount = 1;
        private int coloumnsCount = 1;
        private const string ProjectNumberKey = "ProjectNumber";

        #region Properties

        private SheetStyles HeaderSheetStyle
        {
            get
            {
                CellStyles cellStyle = new CellStyles();
                cellStyle.IsBold = true;
                cellStyle.BorderStyle = NPOI.SS.UserModel.BorderStyle.None;
                cellStyle.FontHeight = 350;
                CellStyles[] cellStylearray = { cellStyle };
                RowStyles headerrowStyle = new RowStyles(cellStylearray);
                headerrowStyle.Height = 500;

                CellStyles dataCellStyle = new CellStyles();
                dataCellStyle.WrapText = true;
                dataCellStyle.IsBold = true;
                CellStyles[] dataCellStylearray = { dataCellStyle };
                RowStyles datarowStyle = new RowStyles(dataCellStylearray);
                RowStyles datarowStyle2 = new RowStyles(dataCellStylearray);
                datarowStyle2.Height = 600;

                RowStyles[] rowStylearray = { headerrowStyle, datarowStyle2, datarowStyle };

                SheetStyles sheetStyle = new SheetStyles(rowStylearray);
                sheetStyle.MergeRegion.Add(new int[] { 0, 0, 0, coloumnsCount > 10 ? coloumnsCount : 11 - 1 });
                sheetStyle.IsAutoResize = false;

                return sheetStyle;
            }
        }

        public SheetStyles DataStyles
        {
            get;
            set;
        }

        public string ProjectNumberFromQueryString
        {
            get
            {
                return Request.QueryString[ProjectNumberKey];
            }
        }

        public String ProjectNumber
        {
            get
            {
                return txtProjectNumber.Text.ToUpper();
            }
        }

        public string SelectedPeriod
        {
            get;
            set;
        }

        public DateTime? StartDate
        {
            get
            {
                int selectedVal = 0;

                if (int.TryParse(ddlPeriod.SelectedValue, out selectedVal))
                {
                    if (selectedVal == 0)
                    {
                        return diRange.FromDate.Value;
                    }
                    else
                    {
                        var now = Utils.Generic.GetNowWithTimeZone();
                        if (selectedVal > 0)
                        {
                            if (selectedVal == 1)
                            {
                                return Utils.Calendar.QuarterStartDate(now, 1);
                            }
                            else if (selectedVal == 2)
                            {
                                return Utils.Calendar.QuarterStartDate(now, 2);
                            }
                            else if (selectedVal == 3)
                            {
                                return Utils.Calendar.QuarterStartDate(now, 3);
                            }
                            else if (selectedVal == 4)
                            {
                                return Utils.Calendar.QuarterStartDate(now, 4);
                            }
                            else if (selectedVal == 7)
                            {
                                return Utils.Calendar.WeekStartDate(now);
                            }
                            else if (selectedVal == 30)
                            {
                                return Utils.Calendar.MonthStartDate(now);
                            }
                            else if (selectedVal == 15)
                            {
                                return Utils.Calendar.PayrollCurrentStartDate(now);
                            }
                            else
                            {
                                return Utils.Calendar.YearStartDate(now);
                            }

                        }
                        else if (selectedVal < 0)
                        {
                            if (selectedVal == -7)
                            {
                                return Utils.Calendar.LastWeekStartDate(now);
                            }
                            else if (selectedVal == -15)
                            {
                                return Utils.Calendar.PayrollPerviousStartDate(now);
                            }
                            else if (selectedVal == -30)
                            {
                                return Utils.Calendar.LastMonthStartDate(now);
                            }
                            else
                            {
                                return Utils.Calendar.LastYearStartDate(now);
                            }
                        }
                        else
                        {
                            return diRange.FromDate.Value;
                        }
                    }
                }

                return null;
            }
        }

        public DateTime? EndDate
        {
            get
            {

                int selectedVal = 0;

                if (int.TryParse(ddlPeriod.SelectedValue, out selectedVal))
                {
                    if (selectedVal == 0)
                    {
                        return diRange.ToDate.Value;
                    }
                    else
                    {
                        var now = Utils.Generic.GetNowWithTimeZone();
                        if (selectedVal > 0)
                        {
                            if (selectedVal == 1)
                            {
                                return Utils.Calendar.QuarterEndDate(now, 1);
                            }
                            else if (selectedVal == 2)
                            {
                                return Utils.Calendar.QuarterEndDate(now, 2);
                            }
                            else if (selectedVal == 3)
                            {
                                return Utils.Calendar.QuarterEndDate(now, 3);
                            }
                            else if (selectedVal == 4)
                            {
                                return Utils.Calendar.QuarterEndDate(now, 4);
                            }
                            else if (selectedVal == 7)
                            {
                                return Utils.Calendar.WeekEndDate(now);
                            }
                            else if (selectedVal == 15)
                            {
                                return Utils.Calendar.PayrollCurrentEndDate(now);
                            }
                            else if (selectedVal == 30)
                            {
                                return Utils.Calendar.MonthEndDate(now);
                            }
                            else
                            {
                                return Utils.Calendar.YearEndDate(now);
                            }
                        }
                        else if (selectedVal < 0)
                        {
                            if (selectedVal == -7)
                            {
                                return Utils.Calendar.LastWeekEndDate(now);
                            }
                            else if (selectedVal == -15)
                            {
                                return Utils.Calendar.PayrollPerviousEndDate(now);
                            }
                            else if (selectedVal == -30)
                            {
                                return Utils.Calendar.LastMonthEndDate(now);
                            }
                            else
                            {
                                return Utils.Calendar.LastYearEndDate(now);
                            }
                        }
                        else
                        {
                            return diRange.ToDate.Value;
                        }
                    }
                }

                return null;
            }
        }

        private DateTime StartDateLocal
        {
            get;
            set;
        }

        private DateTime EndDateLocal
        {
            get;
            set;
        }

        public int NumberOfMonths
        {
            get
            {
                return Math.Abs((StartDateLocal.Month - EndDateLocal.Month) + 12 * (StartDateLocal.Year - EndDateLocal.Year)) + 1;
            }
        }

        private Project Project
        {
            get;
            set;
        }

        private int NumberOfFixedColumns
        {
            get;
            set;
        }

        private List<PersonBudgetComparison> ReportData
        {
            get;
            set;
        }

        private int NumberOfPeriods
        {
            get;
            set;
        }

        private Dictionary<DateTime, decimal> TotalHours
        {
            get;
            set;
        }

        private List<ExpenseSummary> Expenses
        {
            get;
            set;
        }

        private int ActualPeriod
        {
            get
            {
                return int.Parse(ddlActualPeriod.SelectedValue);
            }
        }

        public DateTime? ActualsEndDate
        {
            get
            {
                var now = Utils.Generic.GetNowWithTimeZone();

                if (ActualPeriod == 30)
                {
                    return Utils.Calendar.MonthEndDate(now.AddMonths(-1));
                }
                else if (ActualPeriod == 15)
                {
                    return Utils.Calendar.PayrollPerviousEndDate(now);
                }
                else if (ActualPeriod == 0)
                {
                    return null;
                }
                else
                {
                    return now;
                }
            }
        }

        #endregion

        #region PageEvents
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                var clients = DataHelper.GetAllClientsSecure(null, true, true);
                DataHelper.FillListDefault(ddlClients, "-- Select an Account -- ", clients as object[], false);

                if (!String.IsNullOrEmpty(ProjectNumberFromQueryString))
                {
                    txtProjectNumber.Text = ProjectNumberFromQueryString.ToUpper();
                    ddlView.SelectedValue = "2";
                    ddlPeriod.SelectedValue = "*";
                }
                else
                {
                    GetFilterValuesForSession();
                }
            }
        }

        protected void Page_PreRender(object sender, EventArgs e)
        {
            var now = Utils.Generic.GetNowWithTimeZone();
            diRange.FromDate = StartDate.HasValue ? StartDate : Utils.Calendar.WeekStartDate(now);
            diRange.ToDate = EndDate.HasValue ? EndDate : Utils.Calendar.WeekEndDate(now);

            lblCustomDateRange.Text = string.Format("({0}&nbsp;-&nbsp;{1})",
                    diRange.FromDate.Value.ToString(Constants.Formatting.EntryDateFormat),
                    diRange.ToDate.Value.ToString(Constants.Formatting.EntryDateFormat)
                    );

            if (ddlPeriod.SelectedValue == "0")
            {
                lblCustomDateRange.Attributes.Add("class", "fontBold");
                imgCalender.Attributes.Add("class", "");
            }
            else
            {
                lblCustomDateRange.Attributes.Add("class", "displayNone");
                imgCalender.Attributes.Add("class", "displayNone");
            }

            hdnStartDate.Value = diRange.FromDate.Value.ToString(Constants.Formatting.EntryDateFormat);
            hdnEndDate.Value = diRange.ToDate.Value.ToString(Constants.Formatting.EntryDateFormat);
            string url = Request.Url.AbsoluteUri;

            if (!IsPostBack)
            {
                LoadData();
            }
        }

        //protected void txtProjectNumber_OnTextChanged(object sender, EventArgs e)
        //{
        //    var value = ddlPeriod.Items.FindByValue(SelectedPeriod);
        //    if (value == null)
        //        ddlPeriod.SelectedValue = "*";
        //    else
        //        ddlPeriod.SelectedValue = value.Value;
        //}

        //protected void ddlPeriod_SelectedIndexChanged(object sender, EventArgs e)
        //{
        //    if (ddlPeriod.SelectedValue == "0")
        //    {
        //        mpeCustomDates.Show();
        //    }
        //}

        protected void btnCustDatesOK_Click(object sender, EventArgs e)
        {
            Page.Validate(valSumDateRange.ValidationGroup);
            if (Page.IsValid)
            {
                hdnStartDate.Value = StartDate.Value.Date.ToShortDateString();
                hdnEndDate.Value = EndDate.Value.Date.ToShortDateString();
            }
            else
            {
                mpeCustomDates.Show();
            }
        }

        protected void btnCustDatesCancel_OnClick(object sender, EventArgs e)
        {
            diRange.FromDate = Convert.ToDateTime(hdnStartDate.Value);
            diRange.ToDate = Convert.ToDateTime(hdnEndDate.Value);
        }

        protected void ddlClients_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            ClearAndAddFirsItemForDdlProjects();

            if (ddlClients.SelectedIndex != 0)
            {
                ddlProjects.Enabled = true;
                int clientId = Convert.ToInt32(ddlClients.SelectedItem.Value);
                var projects = DataHelper.GetProjectsByClientId(clientId);
                projects = projects.OrderBy(p => p.Status.Name).ThenBy(p => p.ProjectNumber).ToArray();
                foreach (var project in projects)
                {
                    var li = new System.Web.UI.WebControls.ListItem(project.ProjectNumber + " - " + project.Name,
                                           project.ProjectNumber.ToString());

                    li.Attributes[Constants.Variables.OptionGroup] = project.Status.Name;
                    ddlProjects.Items.Add(li);
                }
            }
            mpeProjectSearch.Show();
        }

        protected void ddlProjects_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            if (ddlProjects.SelectedValue != string.Empty)
            {
                var projectNumber = ddlProjects.SelectedItem.Value;

                PopulateControls(projectNumber);
            }
            else
            {
                mpeProjectSearch.Show();
            }
        }

        protected void btnProjectSearch_Click(object sender, EventArgs e)
        {
            List<Project> list = ServiceCallers.Custom.Report(r => r.ProjectSearchByName(txtProjectSearch.Text)).ToList();
            btnProjectSearch.Attributes.Remove("disabled");
            if (list.Count > 0)
            {
                ltrlNoProjectsText.Visible = false;
                repProjectNamesList.Visible = true;
                repProjectNamesList.DataSource = list;
                repProjectNamesList.DataBind();
            }
            else
            {
                repProjectNamesList.Visible = false;
                ltrlNoProjectsText.Visible = true;
            }
            mpeProjectSearch.Show();
        }

        protected void lnkProjectNumber_OnClick(object sender, EventArgs e)
        {
            var lnkProjectNumber = sender as LinkButton;
            PopulateControls(lnkProjectNumber.Attributes["ProjectNumber"]);
        }

        protected void btnclose_OnClick(object sender, EventArgs e)
        {
            ClearFilters();
        }
        #endregion

        #region Methods
        private void ClearAndAddFirsItemForDdlProjects()
        {
            System.Web.UI.WebControls.ListItem firstItem = new System.Web.UI.WebControls.ListItem("-- Select a Project --", string.Empty);
            ddlProjects.Items.Clear();
            ddlProjects.Items.Add(firstItem);
            ddlProjects.Enabled = false;
        }

        private void PopulateControls(string projectNumber)
        {
            txtProjectNumber.Text = projectNumber;
            var value = ddlPeriod.Items.FindByValue(SelectedPeriod);
            if (value == null)
                ddlPeriod.SelectedValue = "*";
            else
                ddlPeriod.SelectedValue = value.Value;
            LoadData();
            ClearFilters();
            SaveFilterValuesForSession();
        }

        private void ClearFilters()
        {
            ltrlNoProjectsText.Visible = repProjectNamesList.Visible = false;
            ClearAndAddFirsItemForDdlProjects();
            ddlProjects.SelectedIndex = ddlClients.SelectedIndex = 0;
            txtProjectSearch.Text = string.Empty;
            btnProjectSearch.Attributes["disabled"] = "disabled";
        }

        private void SaveFilterValuesForSession()
        {
            TimeReports filter = new TimeReports();
            filter.ProjectNumber = txtProjectNumber.Text;
            filter.ReportPeriod = ddlPeriod.SelectedValue;
            filter.SelectedView = ddlView.SelectedValue;
            filter.StartDate = diRange.FromDate.Value;
            filter.EndDate = diRange.ToDate.Value;
            filter.ActualsEndDate = ddlActualPeriod.SelectedValue;
            ReportsFilterHelper.SaveFilterValues(ReportName.BudgetComparision, filter);
        }

        private void GetFilterValuesForSession()
        {
            var filters = ReportsFilterHelper.GetFilterValues(ReportName.BudgetComparision) as TimeReports;
            if (filters != null)
            {
                txtProjectNumber.Text = filters.ProjectNumber;
                ddlPeriod.SelectedValue = filters.ReportPeriod;
                ddlView.SelectedValue = filters.SelectedView;
                diRange.FromDate = filters.StartDate;
                diRange.ToDate = filters.EndDate;
                ddlActualPeriod.SelectedValue = filters.ActualsEndDate;
            }
        }

        private void LoadData()
        {
            if (!string.IsNullOrEmpty(ProjectNumber) && ddlView.SelectedValue != string.Empty)
            {
                try
                {
                    Project = ServiceCallers.Custom.Project(p => p.GetProjectShortByProjectNumberForPerson(ProjectNumber, HttpContext.Current.User.Identity.Name));
                    if (Project != null)
                    {
                        try
                        {
                            ScriptManager.RegisterStartupScript(this.Page, Page.GetType(), "", "FindSum();", true);
                            msgError.ClearMessage();
                            ReportData = ServiceCallers.Custom.Report(r => r.GetBudgetComparisonReportForProject(ProjectNumber, StartDate, EndDate, ActualsEndDate)).ToList();

                            StartDateLocal = StartDate.HasValue ? StartDate.Value : Project.StartDate.Value;
                            EndDateLocal = EndDate.HasValue ? EndDate.Value : Project.EndDate.Value;
                            Expenses = ServiceCallers.Custom.Report(p => p.ReadProjectExpensesByTypeandByMonth(Project.Id.Value, null, StartDateLocal, EndDateLocal)).ToList();

                            if (ReportData != null && ReportData.Count() > 0)
                            {
                                divWholePage.Visible = true;
                                NumberOfFixedColumns = 1;
                                repBudgetResource.DataSource = ReportData;
                                repBudgetResource.DataBind();
                                repResource.DataSource = ReportData;
                                repResource.DataBind();
                                repDifference.DataSource = ReportData;
                                repDifference.DataBind();
                                repBudgetExpense.DataSource = Expenses;
                                repBudgetExpense.DataBind();
                                repSelectedExpense.DataSource = Expenses;
                                repSelectedExpense.DataBind();
                                repExpenseDifference.DataSource = Expenses;
                                repExpenseDifference.DataBind();

                                if (ddlView.SelectedValue == "0")
                                {
                                    lblDescription.Text = "Projected";
                                }
                                else if (ddlView.SelectedValue == "1")
                                {
                                    lblDescription.Text = "Actuals";
                                }
                                else if (ddlView.SelectedValue == "2")
                                {
                                    lblDescription.Text = "ETC";
                                }

                                lblProjectName.Text = Project.ProjectNumber + " - " + Project.Name + " (" + StartDateLocal.ToString("MM/dd/yyy") + " - " + EndDateLocal.ToString("MM/dd/yyy") + ")";
                                divEmpty.Visible = false;
                            }
                            else
                            {
                                divWholePage.Visible = false;
                                divEmpty.Visible = true;
                            }
                        }
                        catch (Exception ex)
                        {
                            msgError.ShowErrorMessage(ex.Message);
                            divWholePage.Visible = false;
                        }
                    }
                    else
                    {
                        divWholePage.Visible = false;
                        divEmpty.Visible = true;
                    }
                }
                catch (Exception ex)
                {
                    msgError.ShowErrorMessage(ex.Message);
                    divWholePage.Visible = false;
                }
            }
            else if (IsPostBack)
            {
                if (string.IsNullOrEmpty(ProjectNumber) && ddlView.SelectedValue != string.Empty)
                {
                    msgError.ShowErrorMessage("Please enter a Project Number.");
                }
                if (ddlView.SelectedValue == string.Empty && !string.IsNullOrEmpty(ProjectNumber))
                {
                    msgError.ShowErrorMessage("Please select view for the report.");
                }
                if (string.IsNullOrEmpty(ProjectNumber) && ddlView.SelectedValue == string.Empty)
                {
                    msgError.ShowErrorMessage("Please enter a Project Number and select view for the report.");
                }
                divWholePage.Visible = false;
            }
            uplReport.Update();
        }

        #endregion

        protected void repBudgetResource_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            var rowData = e.Item.DataItem as PersonBudgetComparison;


            if (e.Item.ItemType == ListItemType.Header)
            {
                NumberOfFixedColumns = 1;
                var row = e.Item.FindControl("lvHeader") as HtmlTableRow;
                AddBillRatePeriodHeaderCells(row);
                AddMonthHeaderCells(row);

                if (row.Cells.Count == NumberOfFixedColumns)
                {
                    int i = NumberOfFixedColumns;
                    var newColumn = new HtmlTableCell("TH");
                    row.Cells.Insert(i, newColumn);
                    row.Cells[i].InnerHtml = "Total Hours";
                    row.Cells[i].Attributes["class"] = "CompPerfMonthSummary columnSum";
                    i++;

                    newColumn = new HtmlTableCell("TH");
                    row.Cells.Insert(i, newColumn);
                    row.Cells[i].InnerHtml = "Revenue";
                    row.Cells[i].Attributes["class"] = "CompPerfMonthSummary revenueSum";
                    i++;

                    newColumn = new HtmlTableCell("TH");
                    row.Cells.Insert(i, newColumn);
                    row.Cells[i].InnerHtml = "Cost";
                    row.Cells[i].Attributes["class"] = "CompPerfMonthSummary revenueSum";
                    i++;

                    newColumn = new HtmlTableCell("TH");
                    row.Cells.Insert(i, newColumn);
                    row.Cells[i].InnerHtml = "Margin";
                    row.Cells[i].Attributes["class"] = "CompPerfMonthSummary revenueSum";
                    i++;

                    newColumn = new HtmlTableCell("TH");
                    row.Cells.Insert(i, newColumn);
                    row.Cells[i].InnerHtml = "Margin %";
                    row.Cells[i].Attributes["class"] = "CompPerfMonthSummary";
                    i++;
                }
            }
            else if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                NumberOfFixedColumns = 1;
                var row = e.Item.FindControl("lvItem") as HtmlTableRow;
                if (row.Cells.Count == NumberOfFixedColumns)
                {
                    for (int i = 1; i <= NumberOfPeriods * 3; i++)
                    {
                        var td = new HtmlTableCell() { };
                        td.Attributes["class"] = "CompPerfMonthSummary";
                        row.Cells.Insert(row.Cells.Count, td);
                    }
                }

                FillBillRatePeriodCells(row, rowData);
                NumberOfFixedColumns = NumberOfFixedColumns + (NumberOfPeriods * 3);

                if (row.Cells.Count == NumberOfFixedColumns)
                {
                    var monthsInPeriod = NumberOfMonths;
                    for (int i = 0; i < monthsInPeriod; i++)
                    {
                        var td = new HtmlTableCell() { };
                        td.Attributes["class"] = "CompPerfMonthSummary";
                        row.Cells.Insert(row.Cells.Count, td);
                    }

                }

                FillMonthCells(row, rowData);
                NumberOfFixedColumns = NumberOfFixedColumns + NumberOfMonths;

                if (row.Cells.Count == NumberOfFixedColumns)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        var td = new HtmlTableCell() { };
                        td.Attributes["class"] = "CompPerfMonthSummary";
                        row.Cells.Insert(row.Cells.Count, td);
                    }
                }
                FillPersonTotalCells(row, rowData);
            }
            else if (e.Item.ItemType == ListItemType.Footer)
            {
                NumberOfFixedColumns = 1;
                var row = e.Item.FindControl("lvFooter") as HtmlTableRow;
                if (row.Cells.Count == NumberOfFixedColumns)
                {
                    for (int i = 1; i <= NumberOfPeriods * 3; i++)
                    {
                        var td = new HtmlTableCell() { };
                        td.Attributes["class"] = "CompPerfMonthSummary";
                        row.Cells.Insert(row.Cells.Count, td);
                    }

                    for (int i = 0; i < NumberOfMonths; i++)
                    {
                        var td = new HtmlTableCell() { };
                        td.Attributes["class"] = "CompPerfMonthSummary";
                        row.Cells.Insert(row.Cells.Count, td);
                    }
                    for (int i = 0; i < 5; i++)
                    {
                        var td = new HtmlTableCell() { };
                        string cssClass = "CompPerfMonthSummary";
                        if (i == 1 || i == 2 || i == 3)
                        {
                            cssClass = "CompPerfMonthSummary expenseSum";
                        }
                        td.Attributes["class"] = cssClass;
                        row.Cells.Insert(row.Cells.Count, td);
                    }
                }
            }
        }

        private void FillPersonTotalCells(HtmlTableRow row, PersonBudgetComparison rowData)
        {
            int i = NumberOfFixedColumns;
            if (row.Attributes["isbudget"] == "true")
            {
                Label lbl = new Label();
                lbl.Text = rowData.TotalBudgetHours.ToString();
                row.Cells[i].Controls.Add(lbl);
                i++;
                lbl = new Label();
                lbl.Text = rowData.Financials.BudgetRevenue.ToString();
                row.Cells[i].Controls.Add(lbl);
                i++;
                lbl = new Label();
                rowData.Financials.Cogs = rowData.Financials.BudgetRevenue - rowData.Financials.BudgetGrossMargin;
                lbl.Text = rowData.Financials.Cogs.ToString();
                row.Cells[i].Controls.Add(lbl);
                i++;
                lbl = new Label();
                lbl.Text = rowData.Financials.BudgetGrossMargin.ToString();
                row.Cells[i].Controls.Add(lbl);
                i++;

                lbl = new Label();
                lbl.Text = rowData.Financials.BudgetRevenue != 0 ? (rowData.Financials.BudgetGrossMargin.Value * 100 / rowData.Financials.BudgetRevenue.Value).ToString("##0.0") : "-";
                row.Cells[i].Controls.Add(lbl);
            }
            else if (row.Attributes["isbudget"] == "false")
            {
                if (ddlView.SelectedValue == "0")
                {
                    Label lbl = new Label();
                    lbl.Text = rowData.TotalProjectedHours.ToString();
                    row.Cells[i].Controls.Add(lbl);
                    i++;
                    lbl = new Label();
                    lbl.Text = rowData.Financials.Revenue.ToString();
                    row.Cells[i].Controls.Add(lbl);
                    i++;
                    lbl = new Label();
                    rowData.Financials.Cogs = rowData.Financials.Revenue - rowData.Financials.GrossMargin;
                    lbl.Text = rowData.Financials.Cogs.ToString();
                    row.Cells[i].Controls.Add(lbl);
                    i++;
                    lbl = new Label();
                    lbl.Text = rowData.Financials.GrossMargin.ToString();
                    row.Cells[i].Controls.Add(lbl);
                    i++;
                    lbl = new Label();
                    lbl.Text = rowData.Financials.Revenue != 0 ? (rowData.Financials.GrossMargin.Value * 100 / rowData.Financials.Revenue.Value).ToString("##0.0") : "-";
                    row.Cells[i].Controls.Add(lbl);
                    i++;
                }
                else if (ddlView.SelectedValue == "1")
                {
                    Label lbl = new Label();
                    lbl.Text = rowData.TotalActualHours.ToString();
                    row.Cells[i].Controls.Add(lbl);
                    i++;
                    lbl = new Label();
                    lbl.Text = rowData.Financials.ActualRevenue.ToString();
                    row.Cells[i].Controls.Add(lbl);
                    i++;
                    lbl = new Label();
                    rowData.Financials.Cogs = rowData.Financials.ActualRevenue - rowData.Financials.ActualGrossMargin;
                    lbl.Text = rowData.Financials.Cogs.ToString();
                    row.Cells[i].Controls.Add(lbl);
                    i++;
                    lbl = new Label();
                    lbl.Text = rowData.Financials.ActualGrossMargin.ToString();
                    row.Cells[i].Controls.Add(lbl);
                    i++;
                    lbl = new Label();
                    lbl.Text = rowData.Financials.ActualRevenue != 0 ? (rowData.Financials.ActualGrossMargin.Value * 100 / rowData.Financials.ActualRevenue.Value).ToString("##0.0") : "-";
                    row.Cells[i].Controls.Add(lbl);
                    i++;
                }
                else if (ddlView.SelectedValue == "2")
                {
                    Label lbl = new Label();
                    lbl.Text = rowData.TotalEACHours.ToString();
                    row.Cells[i].Controls.Add(lbl);
                    i++;
                    lbl = new Label();
                    lbl.Text = rowData.Financials.EACRevenue.ToString();
                    row.Cells[i].Controls.Add(lbl);
                    i++;
                    lbl = new Label();
                    rowData.Financials.Cogs = rowData.Financials.EACRevenue - rowData.Financials.EACGrossMargin;
                    lbl.Text = rowData.Financials.Cogs.ToString();
                    row.Cells[i].Controls.Add(lbl);
                    i++;
                    lbl = new Label();
                    lbl.Text = rowData.Financials.EACGrossMargin.ToString();
                    row.Cells[i].Controls.Add(lbl);
                    i++;
                    lbl = new Label();
                    lbl.Text = rowData.Financials.EACRevenue != 0 ? (rowData.Financials.EACGrossMargin.Value * 100 / rowData.Financials.EACRevenue.Value).ToString("##0.0") : "-";
                    row.Cells[i].Controls.Add(lbl);
                    i++;
                }
            }
            else if (row.Attributes["isbudget"] == "difference")
            {
                decimal hoursDiff = 0;
                PracticeManagementCurrency revenueDiff = 0M, costDiff = 0M, marginDiff = 0M;


                if (ddlView.SelectedValue == "0")
                {
                    hoursDiff = rowData.TotalProjectedHours - rowData.TotalBudgetHours;
                    revenueDiff = rowData.Financials.Revenue - rowData.Financials.BudgetRevenue;
                    marginDiff = rowData.Financials.GrossMargin - rowData.Financials.BudgetGrossMargin;
                }
                else if (ddlView.SelectedValue == "1")
                {
                    hoursDiff = rowData.TotalActualHours - rowData.TotalBudgetHours;
                    revenueDiff = rowData.Financials.ActualRevenue - rowData.Financials.BudgetRevenue;
                    marginDiff = rowData.Financials.ActualGrossMargin - rowData.Financials.BudgetGrossMargin;
                }
                else if (ddlView.SelectedValue == "2")
                {
                    hoursDiff = rowData.TotalEACHours - rowData.TotalBudgetHours;
                    revenueDiff = rowData.Financials.EACRevenue - rowData.Financials.BudgetRevenue;
                    marginDiff = rowData.Financials.EACGrossMargin - rowData.Financials.BudgetGrossMargin;
                }

                costDiff = revenueDiff - marginDiff;

                revenueDiff.FormatStyle = NumberFormatStyle.Revenue;
                revenueDiff.DoNotShowDecimals = true;

                costDiff.FormatStyle = NumberFormatStyle.Cogs;
                costDiff.DoNotShowDecimals = true;

                marginDiff.FormatStyle = NumberFormatStyle.Margin;
                marginDiff.DoNotShowDecimals = true;

                Label lbl = new Label();
                lbl.Text = hoursDiff.ToString();
                row.Cells[i].Controls.Add(lbl);
                i++;
                lbl = new Label();
                lbl.Text = revenueDiff.ToString();
                row.Cells[i].Controls.Add(lbl);
                i++;
                lbl = new Label();
                lbl.Text = costDiff.ToString();
                row.Cells[i].Controls.Add(lbl);
                i++;
                lbl = new Label();
                lbl.Text = marginDiff.ToString();
                row.Cells[i].Controls.Add(lbl);
                i++;
                lbl = new Label();
                lbl.Text = revenueDiff.Value != 0 && marginDiff.Value >= 0.5M ? (marginDiff.Value * 100 / revenueDiff.Value).ToString("##0.0") : "-";
                row.Cells[i].Controls.Add(lbl);
            }
        }

        private void AddBillRatePeriodHeaderCells(HtmlTableRow row)
        {
            NumberOfFixedColumns = 1;

            if (row != null)
            {
                int numberOfPeriods = 0;
                foreach (var data in ReportData)
                {
                    if (data.BudgetBillRate != null && data.ProjectedAndActualBillRate == null)
                    {
                        numberOfPeriods = data.BudgetBillRate.Count > numberOfPeriods ? data.BudgetBillRate.Count : numberOfPeriods;
                    }
                    else if (data.BudgetBillRate == null && data.ProjectedAndActualBillRate != null)
                    {
                        numberOfPeriods = data.ProjectedAndActualBillRate.Count > numberOfPeriods ? data.ProjectedAndActualBillRate.Count : numberOfPeriods;
                    }
                    else if (data.BudgetBillRate != null && data.ProjectedAndActualBillRate != null)
                    {
                        var count = data.BudgetBillRate.Count > data.ProjectedAndActualBillRate.Count ? data.BudgetBillRate.Count : data.ProjectedAndActualBillRate.Count;
                        numberOfPeriods = count > numberOfPeriods ? count : numberOfPeriods;
                    }
                }

                NumberOfPeriods = numberOfPeriods;

                for (int i = NumberOfFixedColumns, k = 1; k <= numberOfPeriods; k++)
                {
                    var newColumn = new HtmlTableCell("TH");
                    row.Cells.Insert(i, newColumn);
                    row.Cells[i].InnerHtml = "Period-" + k.ToString() + " StartDate";
                    row.Cells[i].Attributes["class"] = "CompPerfMonthSummary";
                    i++;

                    newColumn = new HtmlTableCell("TH");
                    row.Cells.Insert(i, newColumn);
                    row.Cells[i].InnerHtml = "BillRate/Hr " + "(Period-" + k.ToString() + ")";
                    row.Cells[i].Attributes["class"] = "CompPerfMonthSummary";
                    i++;

                    newColumn = new HtmlTableCell("TH");
                    row.Cells.Insert(i, newColumn);
                    row.Cells[i].InnerHtml = "Cost/Hr " + "(Period-" + k.ToString() + ")";
                    row.Cells[i].Attributes["class"] = "CompPerfMonthSummary";
                    i++;
                }
                NumberOfFixedColumns = NumberOfFixedColumns + (numberOfPeriods * 3);
            }
        }

        private void AddMonthHeaderCells(HtmlTableRow row)
        {
            if (row != null)
            {
                DateTime periodStart = Utils.Calendar.MonthStartDate(StartDateLocal);
                while (row.Cells.Count > NumberOfFixedColumns)
                {
                    row.Cells.RemoveAt(NumberOfFixedColumns);
                }
                for (int i = NumberOfFixedColumns, k = 0; k < NumberOfMonths; i++, k++)
                {
                    var newColumn = new HtmlTableCell("TH");
                    row.Cells.Insert(i, newColumn);

                    row.Cells[i].InnerHtml = periodStart.ToString(Constants.Formatting.CompPerfMonthYearFormat);
                    row.Cells[i].Attributes["class"] = "CompPerfMonthSummary columnSum";

                    periodStart = periodStart.AddMonths(1);
                }
                NumberOfFixedColumns = NumberOfMonths + NumberOfFixedColumns;
            }
        }

        private void FillBillRatePeriodCells(HtmlTableRow row, PersonBudgetComparison rowData)
        {

            if (row != null && rowData != null)
            {
                Dictionary<DateTime, PayRate> periods = null;


                if (row.Attributes["isbudget"] == "true")
                {
                    periods = rowData.BudgetBillRate;
                }
                else if (row.Attributes["isbudget"] == "false")
                {
                    periods = rowData.ProjectedAndActualBillRate;
                }
                else if (row.Attributes["isbudget"] == "difference")
                {
                    periods = GetBillRateDifference(rowData.BudgetBillRate, rowData.ProjectedAndActualBillRate);
                }

                int i = NumberOfFixedColumns;

                if (periods != null)
                {
                    foreach (KeyValuePair<DateTime, PayRate> interestValue in periods)
                    {
                        if (interestValue.Value != null)
                        {
                            Label hrs = new Label();
                            hrs.Text = interestValue.Key.ToString("MM/dd/yyy");
                            row.Cells[i].Controls.Add(hrs);
                            i++;
                            hrs = new Label();
                            hrs.Text = interestValue.Value.BillRate.ToString();
                            row.Cells[i].Controls.Add(hrs);
                            i++;
                            hrs = new Label();
                            hrs.Text = interestValue.Value.PersonCost.ToString();
                            row.Cells[i].Controls.Add(hrs);
                            i++;
                        }
                    }
                }
            }
        }

        private void FillMonthCells(HtmlTableRow row, PersonBudgetComparison rowData)
        {
            DateTime monthBegin = Utils.Calendar.MonthStartDate(StartDateLocal);
            int monthsInPeriod = NumberOfMonths;

            Dictionary<DateTime, decimal> hours = null;
            if (row.Attributes["isbudget"] == "true")
            {
                hours = rowData.BudgetHours;
            }
            else if (row.Attributes["isbudget"] == "false")
            {
                if (ddlView.SelectedValue == "0")
                {
                    hours = rowData.ProjectedHours;
                }
                else if (ddlView.SelectedValue == "1")
                {
                    hours = rowData.ActualHours;
                }
                else if (ddlView.SelectedValue == "2")
                {
                    hours = rowData.EACHours;
                }
            }
            else if (row.Attributes["isbudget"] == "difference")
            {
                var interestValue = new Dictionary<DateTime, decimal>();
                if (ddlView.SelectedValue == "0")
                {
                    interestValue = rowData.ProjectedHours;
                }
                else if (ddlView.SelectedValue == "1")
                {
                    interestValue = rowData.ActualHours;
                }
                else if (ddlView.SelectedValue == "2")
                {
                    interestValue = rowData.EACHours;
                }

                hours = GetDifference(rowData.BudgetHours, interestValue);
            }

            for (int i = NumberOfFixedColumns, k = 0;
               k < monthsInPeriod;
               i++, k++, monthBegin = monthBegin.AddMonths(1))
            {
                DateTime monthEnd = Utils.Calendar.MonthEndDate(monthBegin);


                if (hours != null)
                {
                    foreach (KeyValuePair<DateTime, decimal> interestValue in hours)
                    {
                        if (IsInMonth(interestValue.Key, monthBegin, monthEnd))
                        {

                            Label hrs = new Label();

                            hrs.Text = interestValue.Value.ToString();
                            row.Cells[i].Controls.Add(hrs);

                        }
                    }
                }
                if (row.Cells[i].Controls.Count == 0)
                {
                    row.Cells[i].InnerHtml = "-";
                }
            }
        }

        private static bool IsInMonth(DateTime date, DateTime periodStart, DateTime periodEnd)
        {
            var result =
                (date.Year > periodStart.Year ||
                (date.Year == periodStart.Year && date.Month >= periodStart.Month)) &&
                (date.Year < periodEnd.Year || (date.Year == periodEnd.Year && date.Month <= periodEnd.Month));

            return result;
        }

        private Dictionary<DateTime, decimal> GetDifference(Dictionary<DateTime, decimal> budgetHours, Dictionary<DateTime, decimal> hoursData)
        {
            var result = new Dictionary<DateTime, decimal>();

            if (budgetHours == null && hoursData != null)
            {
                result = hoursData;
                return result;
            }

            else if (hoursData == null && budgetHours != null)
            {
                foreach (KeyValuePair<DateTime, decimal> budgetValue in budgetHours)
                {
                    result.Add(budgetValue.Key, -budgetValue.Value);
                }
                return result;
            }
            else if (hoursData != null && budgetHours != null)
            {
                foreach (KeyValuePair<DateTime, decimal> interestValue in hoursData)
                {
                    foreach (KeyValuePair<DateTime, decimal> budgetValue in budgetHours)
                    {
                        if (budgetValue.Key == interestValue.Key)
                        {
                            result.Add(budgetValue.Key, interestValue.Value - budgetValue.Value);
                        }
                    }
                }
            }

            var budgetMissingKeys = budgetHours.Keys.Where(x => !result.ContainsKey(x));
            foreach (var key in budgetMissingKeys)
            {
                result.Add(key, -budgetHours[key]);
            }
            var selectedMissingKeys = hoursData.Keys.Where(x => !result.ContainsKey(x));
            foreach (var key in selectedMissingKeys)
            {
                result.Add(key, hoursData[key]);
            }


            return result;
        }

        protected void btnExportToExcel_Click(object sender, EventArgs e)
        {
            DataHelper.InsertExportActivityLogMessage("Budget Comparison Report");
            List<SheetStyles> sheetStylesList = new List<SheetStyles>();
            var dataSetList = new List<DataSet>();

            ReportData = ServiceCallers.Custom.Report(r => r.GetBudgetComparisonReportForProject(ProjectNumber, StartDate, EndDate, ActualsEndDate)).ToList();
            Project = ServiceCallers.Custom.Project(p => p.GetProjectShortByProjectNumber(ProjectNumber, null, null, null));
            StartDateLocal = StartDate.HasValue ? StartDate.Value : Project.StartDate.Value;
            EndDateLocal = EndDate.HasValue ? EndDate.Value : Project.EndDate.Value;
            Expenses = ServiceCallers.Custom.Report(p => p.ReadProjectExpensesByTypeandByMonth(Project.Id.Value, null, StartDateLocal, EndDateLocal)).ToList();

            var fileName = string.Format("BudgetComparison.xls");

            if (ReportData.Count > 0)
            {
                DataTable header1 = new DataTable();
                header1.Columns.Add(string.Format("{0}-{1}", ProjectNumber, "BudgetComparison"));

                List<object> row1 = new List<object>();
                string filters = ddlPeriod.SelectedValue != "0" ? ddlPeriod.SelectedItem.Text : string.Format("{0}-{1}", diRange.FromDate.Value.ToString(Constants.Formatting.EntryDateFormat), diRange.ToDate.Value.ToString(Constants.Formatting.EntryDateFormat));
                filters += "\n View:" + ddlView.SelectedItem.Text;
                filters += "\n Actuals:" + ddlActualPeriod.SelectedItem.Text;
                row1.Add(filters);
                header1.Rows.Add(row1.ToArray());
                headerRowsCount = header1.Rows.Count + 3;

                var data = PrepareDataTable(ReportData, Expenses);
                coloumnsCount = data.Columns.Count;
                sheetStylesList.Add(HeaderSheetStyle);

                DataStyles.MergeRegion.Add(new int[] { 3, 3, 1, coloumnsCount - 1 });
                DataStyles.IsAutoResize = true;
                sheetStylesList.Add(DataStyles);
                var dataset = new DataSet();
                dataset.DataSetName = "Budget Comparision - " + ProjectNumber;
                dataset.Tables.Add(header1);
                dataset.Tables.Add(data);
                dataSetList.Add(dataset);
            }
            else
            {
                string dateRangeTitle = "There are no Records the selected parameters.";
                DataTable header = new DataTable();
                header.Columns.Add(dateRangeTitle);
                sheetStylesList.Add(HeaderSheetStyle);
                var dataset = new DataSet();
                dataset.DataSetName = "Budget Comparision - " + ProjectNumber;
                dataset.Tables.Add(header);
                dataSetList.Add(dataset);
            }
            NPOIExcel.Export(fileName, dataSetList, sheetStylesList);
        }

        public DataTable PrepareDataTable(List<PersonBudgetComparison> report, List<ExpenseSummary> expenses)
        {
            var dataStyle = new SheetStyles(new RowStyles[0]);
            var rowStylesList = dataStyle.rowStyles.ToList();

            DataTable data = new DataTable();

            int numberOfPeriods = 0;

            foreach (var repData in report)
            {
                if (repData.BudgetBillRate != null && repData.ProjectedAndActualBillRate == null)
                {
                    numberOfPeriods = repData.BudgetBillRate.Count > numberOfPeriods ? repData.BudgetBillRate.Count : numberOfPeriods;
                }
                else if (repData.BudgetBillRate == null && repData.ProjectedAndActualBillRate != null)
                {
                    numberOfPeriods = repData.ProjectedAndActualBillRate.Count > numberOfPeriods ? repData.ProjectedAndActualBillRate.Count : numberOfPeriods;
                }
                else if (repData.BudgetBillRate != null && repData.ProjectedAndActualBillRate != null)
                {
                    var count = repData.BudgetBillRate.Count > repData.ProjectedAndActualBillRate.Count ? repData.BudgetBillRate.Count : repData.ProjectedAndActualBillRate.Count;
                    numberOfPeriods = count > numberOfPeriods ? count : numberOfPeriods;
                }
            }
            NumberOfPeriods = numberOfPeriods;
            int type = -1;

            switch (ddlView.SelectedValue)
            {
                case "0":
                    type = 1;
                    break;
                case "1":
                    type = 2;
                    break;
                case "2":
                    type = 3;
                    break;
            }

            rowCount = 1;

            //budget Table
            PrepareTable(data, 0, NumberOfMonths, numberOfPeriods, report, rowStylesList, expenses);

            //Selection Table
            PrepareTable(data, type, NumberOfMonths, numberOfPeriods, report, rowStylesList, expenses);

            //Difference
            PrepareTable(data, 4, NumberOfMonths, numberOfPeriods, report, rowStylesList, expenses);


            DataStyles = new SheetStyles(rowStylesList.ToArray());
            DataStyles.TopRowNo = headerRowsCount;
            return data;
        }

        private int rowCount
        {
            get;
            set;
        }

        private void PrepareTable(DataTable data, int type, int monthsInPeriod, int numberOfPeriods, List<PersonBudgetComparison> report, List<RowStyles> rowStylesList, List<ExpenseSummary> expenses)
        {
            var row = new List<object>();
            DateTime periodStart = Utils.Calendar.MonthStartDate(StartDateLocal);

            var dataCellStyle = new CellStyles();
            dataCellStyle.WrapText = true;

            CellStyles dataDateCellStyle = new CellStyles();
            dataDateCellStyle.DataFormat = "mm/dd/yy;@";

            CellStyles dataCurrencyStyle = new CellStyles();
            dataCurrencyStyle.DataFormat = "$#,##0_);($#,##0)";

            CellStyles dataPercentageStyle = new CellStyles();
            dataPercentageStyle.DataFormat = "0%";

            CellStyles headerWrapCellStyle = new CellStyles();
            headerWrapCellStyle.IsBold = true;
            headerWrapCellStyle.HorizontalAlignment = NPOI.SS.UserModel.HorizontalAlignment.Center;
            headerWrapCellStyle.WrapText = true;

            var dataTotalHoursCellStyle = new CellStyles();
            dataTotalHoursCellStyle.WrapText = true;

            List<CellStyles> headerCellStyleList = new List<CellStyles>();
            headerCellStyleList.Add(headerWrapCellStyle);
            var headerrowStyle = new RowStyles(headerCellStyleList.ToArray());
            if (type == 0)
                rowStylesList.Add(headerrowStyle);

            List<string> alpha = new List<string> { "A","B","C","D","E","F","G","H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z",
            "AA","AB","AC","AD","AE","AF","AG","AH", "AI", "AJ", "AK", "AL", "AM", "AN", "AO", "AP", "AQ", "AR", "AS", "AT", "AU", "AV", "AW", "AX", "AY", "AZ",
            "BA","BB","BC","BD","BE","BF","BG","BH", "BI", "BJ", "BK", "BL", "BM", "BN", "BO", "BP", "BQ", "BR", "BS", "BT", "BU", "BV", "BW", "BX", "BY", "BZ",
            "CA","CB","CC","CD","CE","CF","CG","CH", "CI", "CJ", "CK", "CL", "CM", "CN", "CO", "CP", "CQ", "CR", "CS", "CT", "CU", "CV", "CW", "CX", "CY", "CZ"};
            int monthStartCol = 1 + (numberOfPeriods * 3);
            int monthEndcol = (numberOfPeriods * 3) + monthsInPeriod;


            string monthStartColumnAlpha = alpha[monthStartCol];
            string monthEndColumnAlpha = alpha[monthEndcol];
            string revenueColumn = alpha[monthEndcol + 2];
            string costColumn = alpha[monthEndcol + 3];
            string marginColumn = alpha[monthEndcol + 4];

            int numberOfColumns = (numberOfPeriods * 3) + monthsInPeriod + 6;

            var dataEmptyRowStyle = new List<CellStyles>();
            string tableHeader = string.Empty;
            switch (type)
            {
                case 0:
                    tableHeader = "Budget";
                    break;
                case 1:
                    tableHeader = "Projected";
                    break;
                case 2:
                    tableHeader = "Actual";
                    break;
                case 3:
                    tableHeader = "ETC";
                    break;
                case 4:
                    tableHeader = "Difference";
                    break;
            }

            if (type == 0)
            {
                data.Columns.Add(tableHeader);
                for (int i = 0; i < (numberOfColumns - 1); i++)
                {
                    data.Columns.Add(string.Empty);
                }
            }
            else
            {
                row.Add(string.Format(NPOIExcel.CustomColorWithBoldKey, "black", tableHeader));
                for (int i = 0; i < (numberOfColumns - 1); i++)
                {
                    dataEmptyRowStyle.Add(dataCellStyle);
                    string colName = " ";
                    for (int j = 0; j < i; j++)
                    {
                        colName += " ";
                    }
                    row.Add(colName);
                }
                rowStylesList.Add(new RowStyles(dataEmptyRowStyle.ToArray()));
                data.Rows.Add(row.ToArray());
                rowCount++;
            }

            row = new List<object>();

            row.Add(string.Format(NPOIExcel.CustomColorWithBoldKey, "black", "Resource"));

            for (int i = 1; i <= NumberOfPeriods; i++)
            {
                row.Add(string.Format(NPOIExcel.CustomColorWithBoldKey, "black", "Period-" + i.ToString() + " Start Date"));
                row.Add(string.Format(NPOIExcel.CustomColorWithBoldKey, "black", "BillRate/Hr (Period-" + i.ToString() + ")"));
                row.Add(string.Format(NPOIExcel.CustomColorWithBoldKey, "black", "Cost/Hr (Period-" + i.ToString() + ")"));
            }

            for (int k = 0; k < monthsInPeriod; k++)
            {
                row.Add(string.Format(NPOIExcel.CustomColorWithBoldKey, "black", periodStart.ToString(Constants.Formatting.CompPerfMonthYearFormat)));

                periodStart = periodStart.AddMonths(1);
            }

            row.Add(string.Format(NPOIExcel.CustomColorWithBoldKey, "black", "Total Hours"));
            row.Add(string.Format(NPOIExcel.CustomColorWithBoldKey, "black", "Revenue"));
            row.Add(string.Format(NPOIExcel.CustomColorWithBoldKey, "black", "Cost"));
            row.Add(string.Format(NPOIExcel.CustomColorWithBoldKey, "black", "Margin"));
            row.Add(string.Format(NPOIExcel.CustomColorWithBoldKey, "black", "Margin %"));

            var dataHeaderRowStyle = new List<CellStyles>();
            for (int i = 0; i < numberOfColumns; i++)
            {
                dataHeaderRowStyle.Add(dataCellStyle);
            }

            rowStylesList.Add(new RowStyles(dataHeaderRowStyle.ToArray()));
            data.Rows.Add(row.ToArray());
            rowCount++;

            var totalHours = new Dictionary<DateTime, decimal>();
            decimal totalServiceHours = 0, totalRevenue = 0, totalMargin = 0, totalPersonCost = 0;

            foreach (var personData in report)
            {

                var dataCellStylearray = new List<CellStyles>() { dataCellStyle };

                DateTime monthBegin = Utils.Calendar.MonthStartDate(StartDateLocal);

                row = new List<object>();
                row.Add(personData.Person.LastName + ", " + personData.Person.FirstName + (personData.Person.Title != null ? "(" + personData.Person.Title.TitleName + ")" : ""));
                int tempPeriodCount = 0;

                Dictionary<DateTime, PayRate> billrates = null;
                if (type == 0)
                {
                    billrates = personData.BudgetBillRate;
                }
                else if (type == 1 || type == 2 || type == 3)
                {
                    billrates = personData.ProjectedAndActualBillRate;
                }
                else if (type == 4)
                {
                    billrates = GetBillRateDifference(personData.BudgetBillRate, personData.ProjectedAndActualBillRate);
                }
                if (billrates != null)
                {
                    tempPeriodCount = billrates.Count;
                    foreach (var billRates in billrates)
                    {
                        row.Add(billRates.Key.ToString("MM/dd/yyy"));
                        row.Add(billRates.Value.BillRate.Value);
                        row.Add(billRates.Value.PersonCost.Value);

                        dataCellStylearray.Add(dataDateCellStyle);
                        dataCellStylearray.Add(dataCurrencyStyle);
                        dataCellStylearray.Add(dataCurrencyStyle);
                    }
                }

                for (int i = 0; i < numberOfPeriods - tempPeriodCount; i++)
                {
                    row.Add(string.Empty);
                    row.Add(string.Empty);
                    row.Add(string.Empty);
                    dataCellStylearray.Add(dataDateCellStyle);
                    dataCellStylearray.Add(dataCurrencyStyle);
                    dataCellStylearray.Add(dataCurrencyStyle);
                }

                var colValue = -100000M;
                Dictionary<DateTime, decimal> hours = null;
                if (type == 0)
                {
                    hours = personData.BudgetHours;

                }
                else if (type == 1)
                {
                    hours = personData.ProjectedHours;

                }
                else if (type == 2)
                {
                    hours = personData.ActualHours;
                }
                else if (type == 3)
                {
                    hours = personData.EACHours;
                }
                else if (type == 4)
                {
                    var interestValue = new Dictionary<DateTime, decimal>();
                    if (ddlView.SelectedValue == "0")
                    {
                        interestValue = personData.ProjectedHours;
                    }
                    else if (ddlView.SelectedValue == "1")
                    {
                        interestValue = personData.ActualHours;
                    }
                    else if (ddlView.SelectedValue == "2")
                    {
                        interestValue = personData.EACHours;
                    }

                    hours = GetDifference(personData.BudgetHours, interestValue);
                }


                for (int k = 0; k < monthsInPeriod; k++, monthBegin = monthBegin.AddMonths(1))
                {
                    DateTime monthEnd = Utils.Calendar.MonthEndDate(monthBegin);
                    colValue = -100000M;

                    if (hours != null)
                    {
                        foreach (KeyValuePair<DateTime, decimal> interestValue in hours)
                        {
                            if (IsInMonth(interestValue.Key, monthBegin, monthEnd))
                            {
                                colValue = interestValue.Value;
                                decimal temp = colValue;
                                if (totalHours.Any(t => t.Key == interestValue.Key))
                                {
                                    temp += totalHours.FirstOrDefault(t => t.Key == interestValue.Key).Value;
                                    totalHours.Remove(interestValue.Key);
                                    totalHours.Add(interestValue.Key, temp);
                                }
                                else
                                {
                                    totalHours.Add(interestValue.Key, colValue);
                                }
                            }
                        }
                    }
                    row.Add(colValue > -100000 ? colValue.ToString() : (object)"-");
                    dataCellStylearray.Add(dataCellStyle);
                }

                switch (type)
                {
                    case 0:
                        totalServiceHours += personData.TotalBudgetHours;
                        totalRevenue += personData.Financials.BudgetRevenue.Value;
                        totalPersonCost += (personData.Financials.BudgetRevenue - personData.Financials.BudgetGrossMargin);
                        totalMargin += personData.Financials.BudgetGrossMargin.Value;
                        row.Add(personData.TotalBudgetHours);
                        row.Add(personData.Financials.BudgetRevenue.Value);
                        row.Add(personData.Financials.BudgetRevenue.Value - personData.Financials.BudgetGrossMargin.Value);
                        row.Add(personData.Financials.BudgetGrossMargin.Value);
                        row.Add(personData.Financials.BudgetRevenue != 0 ? personData.Financials.BudgetGrossMargin.Value / personData.Financials.BudgetRevenue.Value : 0);
                        break;
                    case 1:
                        totalServiceHours += personData.TotalProjectedHours;
                        totalRevenue += personData.Financials.Revenue.Value;
                        totalPersonCost += (personData.Financials.Revenue.Value - personData.Financials.GrossMargin.Value);
                        totalMargin += personData.Financials.GrossMargin.Value;
                        row.Add(personData.TotalProjectedHours);
                        row.Add(personData.Financials.Revenue.Value);
                        row.Add(personData.Financials.Revenue.Value - personData.Financials.GrossMargin.Value);
                        row.Add(personData.Financials.GrossMargin.Value);
                        row.Add(personData.Financials.Revenue != 0 ? personData.Financials.GrossMargin.Value / personData.Financials.Revenue.Value : 0);
                        break;
                    case 2:
                        totalServiceHours += personData.TotalActualHours;
                        totalRevenue += personData.Financials.ActualRevenue.Value;
                        totalPersonCost += personData.Financials.ActualRevenue.Value - personData.Financials.ActualGrossMargin.Value;
                        totalMargin += personData.Financials.ActualGrossMargin.Value;
                        row.Add(personData.TotalActualHours);
                        row.Add(personData.Financials.ActualRevenue.Value);
                        row.Add(personData.Financials.ActualRevenue.Value - personData.Financials.ActualGrossMargin.Value);
                        row.Add(personData.Financials.ActualGrossMargin.Value);
                        row.Add(personData.Financials.ActualRevenue != 0 ? personData.Financials.ActualGrossMargin.Value / personData.Financials.ActualRevenue.Value : 0);
                        break;
                    case 3:
                        totalServiceHours += personData.TotalEACHours;
                        totalRevenue += personData.Financials.EACRevenue.Value;
                        totalPersonCost += (personData.Financials.EACRevenue.Value - personData.Financials.EACGrossMargin.Value);
                        totalMargin += personData.Financials.EACGrossMargin.Value;
                        row.Add(personData.TotalEACHours);
                        row.Add(personData.Financials.EACRevenue.Value);
                        row.Add(personData.Financials.EACRevenue.Value - personData.Financials.EACGrossMargin.Value);
                        row.Add(personData.Financials.EACGrossMargin.Value);
                        row.Add(personData.Financials.EACRevenue != 0 ? personData.Financials.EACGrossMargin.Value / personData.Financials.EACRevenue.Value : 0);
                        break;
                    case 4:
                        decimal revenue, cost, margin;
                        switch (ddlView.SelectedValue)
                        {
                            case "0":
                                row.Add(personData.TotalProjectedHours - personData.TotalBudgetHours);
                                revenue = personData.Financials.Revenue.Value - personData.Financials.BudgetRevenue.Value;
                                margin = personData.Financials.GrossMargin.Value - personData.Financials.BudgetGrossMargin.Value;
                                cost = revenue - margin;

                                totalServiceHours += personData.TotalProjectedHours - personData.TotalBudgetHours;
                                totalRevenue += revenue;
                                totalPersonCost += cost;
                                totalMargin += margin;
                                row.Add(revenue);
                                row.Add(cost);
                                row.Add(margin);
                                row.Add(revenue != 0 ? margin / revenue : 0);
                                break;
                            case "1":
                                row.Add(personData.TotalActualHours - personData.TotalBudgetHours);
                                revenue = personData.Financials.ActualRevenue.Value - personData.Financials.BudgetRevenue.Value;
                                margin = personData.Financials.ActualGrossMargin.Value - personData.Financials.BudgetGrossMargin.Value;
                                cost = revenue - margin;

                                totalServiceHours += personData.TotalActualHours - personData.TotalBudgetHours;
                                totalRevenue += revenue;
                                totalPersonCost += cost;
                                totalMargin += margin;
                                row.Add(revenue);
                                row.Add(cost);
                                row.Add(margin);
                                row.Add(revenue != 0 ? margin / revenue : 0);
                                break;
                            case "2":
                                row.Add(personData.TotalEACHours - personData.TotalBudgetHours);
                                revenue = personData.Financials.EACRevenue.Value - personData.Financials.BudgetRevenue.Value;
                                margin = personData.Financials.EACGrossMargin.Value - personData.Financials.BudgetGrossMargin.Value;
                                cost = revenue - margin;

                                totalServiceHours += personData.TotalEACHours - personData.TotalBudgetHours;
                                totalRevenue += revenue;
                                totalPersonCost += cost;
                                totalMargin += margin;
                                row.Add(revenue);
                                row.Add(cost);
                                row.Add(margin);
                                row.Add(revenue != 0 ? margin / revenue : 0);
                                break;
                        }
                        break;
                    default:
                        row.Add(string.Empty);
                        row.Add(string.Empty);
                        row.Add(string.Empty);
                        row.Add(string.Empty);
                        row.Add(string.Empty);
                        break;
                }
                dataTotalHoursCellStyle = new CellStyles();
                dataTotalHoursCellStyle.WrapText = true;

                dataTotalHoursCellStyle.CellFormula = string.Format("SUM({0}{1}: {2}{3})", monthStartColumnAlpha, 4 + rowCount, monthEndColumnAlpha, 4 + rowCount);
                dataCellStylearray.Add(dataTotalHoursCellStyle);
                dataCellStylearray.Add(dataCurrencyStyle);
                dataCellStylearray.Add(dataCurrencyStyle);

                var dataCurrencyMarginStyle = new CellStyles();
                dataCurrencyMarginStyle.DataFormat = "$#,##0_);($#,##0)";
                dataCurrencyMarginStyle.CellFormula = string.Format("{0}{1}-{2}{3}", revenueColumn, 4 + rowCount, costColumn, 4 + rowCount);
                dataCellStylearray.Add(dataCurrencyMarginStyle);

                CellStyles dataMarginPercentageStyle = new CellStyles();
                dataMarginPercentageStyle.DataFormat = "0%";
                dataMarginPercentageStyle.CellFormula = string.Format("{0}{1}/{2}{3}", marginColumn, 4 + rowCount, revenueColumn, 4 + rowCount);
                dataCellStylearray.Add(dataMarginPercentageStyle);


                data.Rows.Add(row.ToArray());
                var datarowStyle = new RowStyles(dataCellStylearray.ToArray());
                rowStylesList.Add(datarowStyle);
                rowCount++;
            }

            int serviceRow = 0;
            int expenseEndRow = 0;
            //Services Total
            row = new List<object>();
            var dataTotalStylearray = new List<CellStyles>() { dataCellStyle };
            row.Add(string.Format(NPOIExcel.CustomColorWithBoldKey, "black", "Services Total"));
            for (int i = 0; i < numberOfPeriods; i++)
            {
                row.Add(string.Empty);
                row.Add(string.Empty);
                row.Add(string.Empty);
                dataTotalStylearray.Add(dataDateCellStyle);
                dataTotalStylearray.Add(dataCurrencyStyle);
                dataTotalStylearray.Add(dataCurrencyStyle);
            }

            if (totalHours != null)
            {
                var colValue = -100000M;
                DateTime monthBegin = Utils.Calendar.MonthStartDate(StartDateLocal);
                for (int k = 0; k < monthsInPeriod; k++, monthBegin = monthBegin.AddMonths(1))
                {
                    colValue = -100000M;
                    DateTime monthEnd = Utils.Calendar.MonthEndDate(monthBegin);
                    foreach (KeyValuePair<DateTime, decimal> interestValue in totalHours)
                    {
                        if (IsInMonth(interestValue.Key, monthBegin, monthEnd))
                        {
                            colValue = interestValue.Value;
                        }
                    }
                    row.Add(colValue > -100000 ? colValue.ToString() : (object)"-");
                    dataTotalStylearray.Add(dataCellStyle);
                }
            }

            row.Add(totalServiceHours);
            row.Add(totalRevenue);
            row.Add(totalPersonCost);
            row.Add(totalMargin);
            dataTotalHoursCellStyle = new CellStyles();
            dataTotalHoursCellStyle.WrapText = true;
            dataTotalHoursCellStyle.CellFormula = string.Format("SUM({0}{1}: {2}{3})", monthStartColumnAlpha, 4 + rowCount, monthEndColumnAlpha, 4 + rowCount);
            row.Add(totalRevenue != 0M ? totalMargin / totalRevenue : 0);
            dataTotalStylearray.Add(dataTotalHoursCellStyle);
            dataTotalStylearray.Add(dataCurrencyStyle);
            dataTotalStylearray.Add(dataCurrencyStyle);

            var dataMarginStyle = new CellStyles();
            dataMarginStyle.DataFormat = "$#,##0_);($#,##0)";
            dataMarginStyle.CellFormula = string.Format("{0}{1}-{2}{3}", revenueColumn, 4 + rowCount, costColumn, 4 + rowCount);
            dataTotalStylearray.Add(dataMarginStyle);

            CellStyles dataMarginPerStyle = new CellStyles();
            dataMarginPerStyle.DataFormat = "0%";
            dataMarginPerStyle.CellFormula = string.Format("{0}{1}/{2}{3}", marginColumn, 4 + rowCount, revenueColumn, 4 + rowCount);
            dataTotalStylearray.Add(dataMarginPerStyle);

            rowStylesList.Add(new RowStyles(dataTotalStylearray.ToArray()));
            data.Rows.Add(row.ToArray());
            serviceRow = rowCount + 4;
            rowCount++;

            row = new List<object>();
            dataEmptyRowStyle = new List<CellStyles>();
            for (int i = 0; i < numberOfColumns; i++)
            {
                dataEmptyRowStyle.Add(dataCellStyle);
                row.Add(string.Empty);
            }
            rowStylesList.Add(new RowStyles(dataEmptyRowStyle.ToArray()));
            data.Rows.Add(row.ToArray());
            rowCount++;

            //Add Expenses
            foreach (var expense in expenses)
            {
                row = new List<object>();
                var dataExpenseStylearray = new List<CellStyles>() { dataCellStyle };
                row.Add(expense.Type.Name);

                for (int i = 0; i < numberOfPeriods; i++)
                {
                    row.Add(string.Empty);
                    row.Add(string.Empty);
                    row.Add(string.Empty);
                    dataExpenseStylearray.Add(dataDateCellStyle);
                    dataExpenseStylearray.Add(dataCurrencyStyle);
                    dataExpenseStylearray.Add(dataCurrencyStyle);
                }

                var colValue = -100000M;
                DateTime monthBegin = Utils.Calendar.MonthStartDate(StartDateLocal);

                Dictionary<DateTime, decimal> monthlyExpense = null;
                if (type == 0)
                {
                    monthlyExpense = expense.MonthlyBudgetExpenses;

                }
                else if (type == 1)
                {
                    monthlyExpense = expense.MonthlyExpectedExpenses;

                }
                else if (type == 2)
                {
                    monthlyExpense = expense.MonthlyExpenses;
                }
                else if (type == 3)
                {
                    monthlyExpense = expense.MonthlyEACExpenses;
                }
                else if (type == 4)
                {
                    var interestValue = new Dictionary<DateTime, decimal>();
                    if (ddlView.SelectedValue == "0")
                    {
                        interestValue = expense.MonthlyExpectedExpenses;
                    }
                    else if (ddlView.SelectedValue == "1")
                    {
                        interestValue = expense.MonthlyExpenses;
                    }
                    else if (ddlView.SelectedValue == "2")
                    {
                        interestValue = expense.MonthlyEACExpenses;
                    }

                    monthlyExpense = GetDifference(expense.MonthlyBudgetExpenses, interestValue);
                }

                for (int k = 0; k < monthsInPeriod; k++, monthBegin = monthBegin.AddMonths(1))
                {
                    DateTime monthEnd = Utils.Calendar.MonthEndDate(monthBegin);

                    colValue = -100000M;
                    if (monthlyExpense != null)
                    {
                        foreach (KeyValuePair<DateTime, decimal> interestValue in monthlyExpense)
                        {
                            if (IsInMonth(interestValue.Key, monthBegin, monthEnd))
                            {
                                colValue = interestValue.Value;
                            }
                        }
                    }
                    row.Add(colValue > -100000 ? colValue.ToString() : (object)"-");
                    dataExpenseStylearray.Add(dataCurrencyStyle);
                }
                row.Add(string.Empty);
                dataExpenseStylearray.Add(dataCellStyle);
                if (monthlyExpense != null)
                {
                    var sum = monthlyExpense.Sum(e => e.Value);
                    row.Add(sum);
                    row.Add(sum);
                    row.Add("-");
                    row.Add(0);
                    var dataTotalCurrencyCellStyle = new CellStyles();
                    dataTotalCurrencyCellStyle.DataFormat = "$#,##0_);($#,##0)";
                    dataTotalCurrencyCellStyle.CellFormula = string.Format("SUM({0}{1}: {2}{3})", monthStartColumnAlpha, 4 + rowCount, monthEndColumnAlpha, 4 + rowCount);
                    dataExpenseStylearray.Add(dataTotalCurrencyCellStyle);
                    dataExpenseStylearray.Add(dataTotalCurrencyCellStyle);
                    dataExpenseStylearray.Add(dataCellStyle);
                    dataExpenseStylearray.Add(dataPercentageStyle);
                }
                rowStylesList.Add(new RowStyles(dataExpenseStylearray.ToArray()));
                data.Rows.Add(row.ToArray());
                rowCount++;
            }

            //Empty Row
            row = new List<object>();
            dataEmptyRowStyle = new List<CellStyles>();
            for (int i = 0; i < numberOfColumns; i++)
            {
                dataEmptyRowStyle.Add(dataCellStyle);
                row.Add(string.Empty);
            }
            rowStylesList.Add(new RowStyles(dataEmptyRowStyle.ToArray()));
            data.Rows.Add(row.ToArray());
            expenseEndRow = rowCount + 4;
            rowCount++;

            //Total Billings
            row = new List<object>();
            var dataTotalBillingStylearray = new List<CellStyles>() { dataCellStyle };
            row.Add(string.Format(NPOIExcel.CustomColorWithBoldKey, "black", "Total Expected Billing"));
            for (int i = 0; i < (numberOfPeriods * 3) + NumberOfMonths + 1; i++)
            {
                row.Add(string.Empty);
                dataTotalBillingStylearray.Add(dataCellStyle);
            }

            var dataBillingCurrencyStyle = new CellStyles();
            dataBillingCurrencyStyle.DataFormat = "$#,##0_);($#,##0)";
            dataBillingCurrencyStyle.CellFormula = string.Format("SUM({0}{1}:{2}{3})", revenueColumn, serviceRow, revenueColumn, expenseEndRow);

            dataTotalBillingStylearray.Add(dataBillingCurrencyStyle);

            dataBillingCurrencyStyle = new CellStyles();
            dataBillingCurrencyStyle.DataFormat = "$#,##0_);($#,##0)";
            dataBillingCurrencyStyle.CellFormula = string.Format("SUM({0}{1}:{2}{3})", costColumn, serviceRow, costColumn, expenseEndRow);
            dataTotalBillingStylearray.Add(dataBillingCurrencyStyle);

            dataBillingCurrencyStyle = new CellStyles();
            dataBillingCurrencyStyle.DataFormat = "$#,##0_);($#,##0)";
            dataBillingCurrencyStyle.CellFormula = string.Format("SUM({0}{1}:{2}{3})", marginColumn, serviceRow, marginColumn, expenseEndRow);
            dataTotalBillingStylearray.Add(dataBillingCurrencyStyle);

            var dataBillingMarginPerStyle = new CellStyles();
            dataBillingMarginPerStyle.DataFormat = "0%";
            dataBillingMarginPerStyle.CellFormula = string.Format("{0}{1}/{2}{3}", marginColumn, 4 + rowCount, revenueColumn, 4 + rowCount);
            dataTotalBillingStylearray.Add(dataBillingMarginPerStyle);

            rowStylesList.Add(new RowStyles(dataTotalBillingStylearray.ToArray()));
            data.Rows.Add(row.ToArray());
            rowCount++;

            //Empty Row
            row = new List<object>();
            dataEmptyRowStyle = new List<CellStyles>();
            for (int i = 0; i < numberOfColumns; i++)
            {
                dataEmptyRowStyle.Add(dataCellStyle);
                row.Add(string.Empty);
            }
            rowStylesList.Add(new RowStyles(dataEmptyRowStyle.ToArray()));
            data.Rows.Add(row.ToArray());
            expenseEndRow = rowCount;
            rowCount++;
        }

        private string GetMonthStartDateAlphabet(int noOfPeriods)
        {
            int columnCount = 2 + (noOfPeriods * 3);
            List<string> alpha = new List<string> { "A","B","C","D","E","F","G","H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z",
            "AA","AB","AC","AD","AE","AF","AG","AH", "AI", "AJ", "AK", "AL", "AM", "AN", "AO", "AP", "AQ", "AR", "AS", "AT", "AU", "AV", "AW", "AX", "AY", "AZ",
            "BA","BB","BC","BD","BE","BF","BG","BH", "BI", "BJ", "BK", "BL", "BM", "BN", "BO", "BP", "BQ", "BR", "BS", "BT", "BU", "BV", "BW", "BX", "BY", "BZ"};
            return alpha[coloumnsCount];
        }

        protected void repBudgetExpense_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {

            if (e.Item.ItemType == ListItemType.Header)
            {

            }
            else if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                NumberOfFixedColumns = 1;
                var row = e.Item.FindControl("lvExpenseItem") as HtmlTableRow;

                var rowData = e.Item.DataItem as ExpenseSummary;

                if (row.Cells.Count == NumberOfFixedColumns)
                {
                    for (int i = 1; i <= NumberOfPeriods * 3; i++)
                    {
                        var td = new HtmlTableCell() { };
                        td.Attributes["class"] = "CompPerfMonthSummary";
                        row.Cells.Insert(row.Cells.Count, td);
                    }
                }

                NumberOfFixedColumns = NumberOfFixedColumns + (NumberOfPeriods * 3);

                if (row.Cells.Count == NumberOfFixedColumns)
                {
                    var monthsInPeriod = NumberOfMonths;
                    for (int i = 0; i < monthsInPeriod; i++)
                    {
                        var td = new HtmlTableCell() { };
                        td.Attributes["class"] = "CompPerfMonthSummary";
                        row.Cells.Insert(row.Cells.Count, td);
                    }

                }

                Dictionary<DateTime, decimal> monthlyValues = null;
                if (row.Attributes["isbudget"] == "true")
                {
                    monthlyValues = rowData.MonthlyBudgetExpenses;
                }
                else if (row.Attributes["isbudget"] == "false")
                {
                    if (ddlView.SelectedValue == "0")
                    {
                        monthlyValues = rowData.MonthlyExpectedExpenses;
                    }
                    else if (ddlView.SelectedValue == "1")
                    {
                        monthlyValues = rowData.MonthlyExpenses;
                    }
                    else if (ddlView.SelectedValue == "2")
                    {
                        monthlyValues = rowData.MonthlyEACExpenses;
                    }
                }
                else if (row.Attributes["isbudget"] == "difference")
                {
                    var interestValue = new Dictionary<DateTime, decimal>();
                    if (ddlView.SelectedValue == "0")
                    {
                        interestValue = rowData.MonthlyExpectedExpenses;
                    }
                    else if (ddlView.SelectedValue == "1")
                    {
                        interestValue = rowData.MonthlyExpenses;
                    }
                    else if (ddlView.SelectedValue == "2")
                    {
                        interestValue = rowData.MonthlyEACExpenses;
                    }

                    monthlyValues = GetDifference(rowData.MonthlyBudgetExpenses, interestValue);
                }

                FillMonthlyExpenseCells(row, rowData, monthlyValues);
                NumberOfFixedColumns = NumberOfFixedColumns + NumberOfMonths;

                if (row.Cells.Count == NumberOfFixedColumns)
                {

                    for (int i = 0; i < 5; i++)
                    {
                        var td = new HtmlTableCell() { };
                        string cssClass = "CompPerfMonthSummary";
                        if (i == 1 || i == 2 || i == 3)
                        {
                            cssClass = "CompPerfMonthSummary expenseSum";
                        }
                        td.Attributes["class"] = cssClass;
                        row.Cells.Insert(row.Cells.Count, td);
                    }
                }

                FillExpenseTotalCells(row, rowData, monthlyValues);

            }
            else if (e.Item.ItemType == ListItemType.Footer)
            {
                NumberOfFixedColumns = 1;
                var row = e.Item.FindControl("lvExpenseFooter") as HtmlTableRow;
                if (row.Cells.Count == NumberOfFixedColumns)
                {
                    for (int i = 1; i <= NumberOfPeriods * 3; i++)
                    {
                        var td = new HtmlTableCell() { };
                        td.Attributes["class"] = "CompPerfMonthSummary";
                        row.Cells.Insert(row.Cells.Count, td);
                    }

                    for (int i = 0; i < NumberOfMonths; i++)
                    {
                        var td = new HtmlTableCell() { };
                        td.Attributes["class"] = "CompPerfMonthSummary";
                        row.Cells.Insert(row.Cells.Count, td);
                    }
                    for (int i = 0; i < 5; i++)
                    {
                        var td = new HtmlTableCell() { };
                        td.Attributes["class"] = "CompPerfMonthSummary";
                        row.Cells.Insert(row.Cells.Count, td);
                    }
                }
            }
        }

        private void FillMonthlyExpenseCells(HtmlTableRow row, ExpenseSummary rowData, Dictionary<DateTime, decimal> monthlyValues)
        {
            DateTime monthBegin = Utils.Calendar.MonthStartDate(StartDateLocal);
            int monthsInPeriod = NumberOfMonths;

            for (int i = NumberOfFixedColumns, k = 0;
               k < monthsInPeriod;
               i++, k++, monthBegin = monthBegin.AddMonths(1))
            {
                DateTime monthEnd = Utils.Calendar.MonthEndDate(monthBegin);

                if (monthlyValues != null)
                {
                    foreach (KeyValuePair<DateTime, decimal> interestValue in monthlyValues)
                    {
                        if (IsInMonth(interestValue.Key, monthBegin, monthEnd))
                        {
                            Label hrs = new Label();
                            hrs.Text = interestValue.Value.ToString(CurrencyDisplayFormat);
                            row.Cells[i].Controls.Add(hrs);
                        }
                    }
                }
                if (row.Cells[i].Controls.Count == 0)
                {
                    row.Cells[i].InnerHtml = "-";
                }
            }
        }

        private void FillExpenseTotalCells(HtmlTableRow row, ExpenseSummary rowData, Dictionary<DateTime, decimal> monthlyValues)
        {
            int i = NumberOfFixedColumns;
            var sum = 0M;
            if (monthlyValues != null)
            {
                sum = monthlyValues.Sum(m => m.Value);

            }
            Label lbl = new Label();
            lbl.Text = string.Empty;
            row.Cells[i].Controls.Add(lbl);
            i++;
            lbl = new Label();
            lbl.Text = sum.ToString(CurrencyDisplayFormat);
            row.Cells[i].Controls.Add(lbl);
            i++;
            lbl = new Label();
            lbl.Text = sum.ToString(CurrencyDisplayFormat);
            row.Cells[i].Controls.Add(lbl);
            i++;
            lbl = new Label();
            lbl.Text = "-";
            row.Cells[i].Controls.Add(lbl);
            i++;

            lbl = new Label();
            lbl.Text = "0%";
            row.Cells[i].Controls.Add(lbl);
        }

        private Dictionary<DateTime, PayRate> GetBillRateDifference(Dictionary<DateTime, PayRate> budgetBillRate, Dictionary<DateTime, PayRate> selectedBillRate)
        {
            var result = new Dictionary<DateTime, PayRate>();

            if (budgetBillRate == null && selectedBillRate != null)
            {
                result = selectedBillRate;
                return result;
            }
            else if (selectedBillRate == null && budgetBillRate != null)
            {
                foreach (KeyValuePair<DateTime, PayRate> budgetValue in budgetBillRate)
                {
                    result.Add(budgetValue.Key, new PayRate { BillRate = -budgetValue.Value.BillRate, PersonCost = -budgetValue.Value.PersonCost });
                }
                return result;
            }
            else if (selectedBillRate != null && budgetBillRate != null)
            {
                foreach (KeyValuePair<DateTime, PayRate> interestValue in selectedBillRate)
                {
                    foreach (KeyValuePair<DateTime, PayRate> budgetValue in budgetBillRate)
                    {
                        if (budgetValue.Key == interestValue.Key)
                        {
                            result.Add(budgetValue.Key, new PayRate { BillRate = interestValue.Value.BillRate - budgetValue.Value.BillRate, PersonCost = interestValue.Value.PersonCost - budgetValue.Value.PersonCost });
                        }
                    }
                }
            }
            return result;
        }

        protected void btnUpdate_Click(object sender, EventArgs e)
        {
            LoadData();
            SaveFilterValuesForSession();
        }
    }
}
